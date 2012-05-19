// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED!
//
// IF YOU FIND ERRORS OR POSSIBLE IMPROVEMENTS, PLEASE LET ME KNOW.
// MAYBE TOGETHER WE CAN SOLVE THIS.
//
// YOU MAY USE THIS CODE: HOWEVER THIS GRANTS NO FUTURE RIGHTS.

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Telnet
{
	/// <summary>
	/// Implements a simple "screen". This is used by telnet.
	/// <p>
	/// The x (rows) and y (columns) values may have an offset. If the offset
	/// is 0/0 the left upper corner is [0,0], or 0-based. With an offset of 1/1
	/// the left upper corner is [1,1], or 1-based.
	/// </p>
	/// </summary>
	/// <remarks>
	/// The class is not thread safe (e.g. search in buffer and modification
	/// of buffer must not happen. It is duty of the calling class to guarantee this.
	/// </remarks>
	public class VirtualScreen : IDisposable
	{
		/// <summary>
		/// ASCII code for Space
		/// </summary>
		public const byte SPACE = 32;

		/// <summary>
		/// window size 
		/// </summary>
		public int Width 
		{
			get
			{
				if (this.vs==null)
					return 0;
				else
					return this.vs.GetLength(0);
			}
		} // Width

		/// <summary>
		/// Window height
		/// </summary>
		public int Height 
		{
			get
			{
				if (this.vs==null)
					return 0;
				else
					return this.vs.GetLength(1);
			}
		} // Width

		// External cursor values allowing an offset and thus
		// 0-based or 1-based coordinates
		private int offsetx = 0;
		private int offsety = 0;

		/// <summary>
		/// Cursor position with offset considered
		/// </summary>
		public int CursorX 
		{
			get
			{
				// 2004-09-01 fixed to plus based on mail of Steve
				return this.CursorX0 + this.offsetx;
			}
			set
			{
				this.CursorX0 = value - this.offsetx;
			}
		} // X

		/// <summary>
		/// Cursor position with offset considered
		/// </summary>
		public int CursorY 
		{
			get
			{
				// 2004-09-01 fixed to plus based on mail of Steve
				return this.CursorY0 + this.offsety;
			}
			set
			{
				this.CursorY0 = value - this.offsety;
			}
		} // Y
		
		/// <summary>
		/// X Offset 
		/// </summary>
		public int CursorXLeft
		{
			get
			{
				return this.offsetx;
			}
		}

		/// <summary>
		/// X max value 
		/// </summary>
		public int CursorXRight
		{
			get
			{
				return this.Width - 1 + this.offsetx;
			}
		}
		/// <summary>
		/// Y max value 
		/// </summary>
		public int CursorYMax
		{
			get
			{
				return this.Height - 1 + this.offsety;
			}
		}
		/// <summary>
		/// Y max value 
		/// </summary>
		public int CursorYMin
		{
			get
			{
				return this.offsety;
			}
		}
		/// <summary>
		/// Reset the cursor to upper left corner
		/// </summary>
		public void CursorReset() 
		{
			this.CursorY0 = 0;
			this.CursorX0 = 0;
		}

		/// <summary>
		/// Move the cursor to the beginning of the next line
		/// </summary>
		public void CursorNextLine() 
		{
			this.Write("\n\r");
		}

		/// <summary>
		/// Set the cursor (offset coordinates)
		/// </summary>
		/// <param name="x">X Position (lines) with offset considered</param>
		/// <param name="y">Y Position (columns) with offset considered</param>
		/// <remarks>
		/// Use the method MoveCursorTo(x,y) when upscrolling should
		/// be supported
		/// </remarks>
		public void CursorPosition(int x, int y) 
		{
			this.CursorX = x;
			this.CursorY = y;
		}

		// zero based cursor positions
		// for internal usage only
		private int cursorx0 = 0; 
		private int cursory0 = 0;
		/// <summary>
		/// 0-based coordinates for cursor, internally used.
		/// </summary>
		private int CursorX0
		{
			get
			{
				return this.cursorx0;
			}
			set 
			{
				if (value<=0)
					this.cursorx0 = 0;
				else if (value>=this.Width)
					this.cursorx0 = this.Width - 1;
				else
					this.cursorx0 = value;
			}
		}
		/// <summary>
		/// 0-based coordinates for cursor, internally used
		/// </summary>
		private int CursorY0
		{
			get
			{
				return this.cursory0;
			}
			set 
			{
				if (value<=0)
					this.cursory0 = 0;
				else
					this.cursory0 = value; // due to the scrolling behaviour we do not cut big values
			}
		}

		// screen array [x,y]
		private byte[,] vs = null;
		// cache for screen as string
		string screenString = null;
		string screenStringLower = null;

		// changed output since last hardcopy of the screen
		private bool changedScreen = false;

		// visible area (=> scrolled updwards)
		// - cleared by clean screen
		// - used by the MoveCursorTo and MoveCursor methods
		// - used by WriteByte
		// - NOT used by ScrollUp
		private int visibleAreaY0top = 0;
		private int visibleAreaY0bottom = 0;

		/// <summary>
		/// Changed screen buffer ?
		/// </summary>
		public bool ChangedScreen 
		{
			get 
			{
				return this.changedScreen;
			}
		}

		/// <summary>
		/// Constructor (offset 0/0)
		/// </summary>
		/// <param name="width">Screen's width</param>
		/// <param name="height">Screen's height</param>
		public VirtualScreen(int width, int height) : this(width, height, 0, 0)
		{
			// nothing here
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="width">Screen's width</param>
		/// <param name="height">Screen's height</param>
		/// <param name="xOffset">Screen coordinates are 0,1 .. based</param>
		/// <param name="yOffset">Screen coordinates are 0,1 .. based</param>
		public VirtualScreen(int width, int height, int xOffset, int yOffset) 
		{
			this.offsetx = xOffset;
			this.offsety = yOffset;
			this.vs = new byte[width,height];
			this.CleanScreen();
			this.changedScreen = false; // reset becuase constructor
			this.visibleAreaY0top = 0;
			this.visibleAreaY0bottom = height -1;
			this.CursorReset();
		}

		/// <summary>
		/// Clean the screen and reset cursor.
		/// </summary>
		/// <remarks>
		/// Changes the output-flag and scrolledUp attribute!
		/// </remarks>
		public void CleanScreen() 
		{
			int lx = this.vs.GetLength(0);
			int ly = this.vs.GetLength(1);
			for (int y=0; y < ly; y++) 
			{
				for (int x=0; x < lx; x++) 
				{
					this.vs[x,y] = SPACE;
				}
			}
			this.CursorReset(); // cursor back to beginning
			this.changedScreen = true;
			this.visibleAreaY0top = 0;
			this.visibleAreaY0bottom = this.Height-1;
		} // cleanScreen

		/// <summary>
		/// Cleans a screen area, all values are
		/// considering any offset
		/// </summary>
		/// <remarks>
		/// - Changes the output-flag!
		/// - Visible area is considered
		/// </remarks>
		/// <param name="xstart">upper left corner (included)</param>
		/// <param name="ystart">upper left corner (included)</param>
		/// <param name="xend">lower right corner (included)</param>
		/// <param name="yend">lower right corner (included)</param>
		public void CleanScreen(int xstart, int ystart, int xend, int yend) 
		{
			if (this.vs == null || xend <= xstart || yend <= ystart || xstart < this.offsetx || xend < this.offsetx || ystart < this.offsety || yend < this.offsety)
				return; // nothing to do

			int x0start = xstart - this.offsetx;
			int y0start = ystart - this.offsety - this.visibleAreaY0top;
			if (y0start<0)
				y0start=0; // only visible area
			int x0end = xend - this.offsetx;
			int y0end = yend - this.offsety - this.visibleAreaY0top;
			if (y0end<0)
				return; // nothing to do

			int lx = this.vs.GetLength(0);
			int ly = this.vs.GetLength(1);

			if (x0end >= lx)
				x0end = lx -1;
			if (y0end >= ly)
				y0end = ly -1;
			
			for (int y=y0start; y <=y0end; y++) 
			{
				for (int x=x0start; x <=x0end; x++) 
				{
					this.vs[x,y] = SPACE;
				}
			}
			this.changedScreen = true;
		}

		/// <summary>
		/// Clean the current line
		/// </summary>
		/// <remarks>
		/// - Changes the output-flag! <br/>
		/// - Visible area is considered
		/// </remarks>
		/// <param name="xStart">X with offset considered</param>
		/// <param name="xEnd">X with offset considered</param>
		public void CleanLine(int xStart, int xEnd) 
		{
			int x0s = xStart - this.offsetx;
			int x0e = xEnd - this.offsetx;
			
			if (xStart < xEnd)
				return;
			if (x0s<0)
				x0s=0;
			if (x0e>=this.Width)
				x0e = this.Width -1;
			
			int y = this.cursory0 - this.visibleAreaY0top;
			if (this.vs == null || y<0 || y > this.vs.GetLength(1))
				return;

			for (int x=x0s; x <=x0e; x++) 
			{
				this.vs[x,y] = SPACE;
			}
			this.changedScreen = true;
		}

		/// <summary>
		/// Clean screen including the cursor position
		/// </summary>
		/// - Changes the output-flag! <br/>
		/// - Visible area is considered
		public void CleanToCursor() 
		{
			int y = this.CursorY -1; // line before
			if (y>=this.offsety)
				this.CleanScreen(this.CursorXLeft, this.offsety, this.CursorXRight, y);
			this.CleanLine(this.CursorXLeft, this.CursorX);
			this.changedScreen = true;
		}

		/// <summary>
		/// Clean screen including the cursor position
		/// </summary>
		/// - Changes the output-flag! <br/>
		/// - Visible area is considered
		public void CleanFromCursor() 
		{
			int y = this.CursorY + 1; // line before
			if (y <= this.visibleAreaY0bottom + this.offsety)
				this.CleanScreen(this.CursorXLeft, y, this.CursorXRight, this.visibleAreaY0bottom + this.offsety);
			this.CleanLine(this.CursorX, this.CursorXRight);
			this.changedScreen = true;
		}

		/// <summary>
		/// Scrolls up about n lines.
		/// </summary>
		/// <param name="lines"></param>
		/// <remarks>
		/// Changes the output-flag! <br/>
		/// TODO: Do we have to change the coordinates offset?
		/// Is line 5 after 2 lines scrolling now line 3 or still 5?
		/// </remarks>
		/// <returns>number of lines scrolled</returns>
		/// 
		public int ScrollUp(int lines) 
		{
			// scrolls up about n lines
			if (lines < 1)
				return 0;
			
			int lx = this.vs.GetLength(0);
			int ly = this.vs.GetLength(1);

			if (lines >= ly) 
			{
				// we need to save the visible are info
				int vat = this.visibleAreaY0top;
				int vab = this.visibleAreaY0bottom;
				this.CleanScreen();
				this.visibleAreaY0top = vat;
				this.visibleAreaY0bottom = vab;
			} 
			else 
			{
				for (int y=lines; y < ly; y++) 
				{
					int yTo = y - lines;
					for (int x = 0; x < lx; x++) 
					{
						this.vs[x,yTo] = this.vs[x,y];
					}
				} // for copy over
				// delete the rest
				this.CleanScreen(this.offsetx, ly - lines, lx + this.offsetx, ly - 1 + this.offsety);
			}
			this.changedScreen = true;
			return lines;
		}

		/// <summary>
		/// Write a byte to the screen, and set new cursor position.
		/// </summary>
		/// <remarks>
		/// Changes the output-flag!
		/// </remarks>
		/// <param name="writeByte">Output byte</param>
		/// <returns>True if byte has been written</returns>
		public bool WriteByte(byte writeByte) 
		{
			return this.WriteByte(writeByte, true);
		}

		/// <summary>
		/// Write a byte to the screen, and set new cursor position.
		/// </summary>
		/// <remarks>
		/// Changes the output-flag!
		/// </remarks>
		/// <param name="writeBytes">Output bytes</param>
		/// <returns>True if byte has been written</returns>
		public bool WriteByte(byte[] writeBytes) 
		{
			if (writeBytes==null || writeBytes.Length < 1)
				return false;
			else 
			{
				for (int i=0; i < writeBytes.Length; i++) 
				{
					if (!this.WriteByte(writeBytes[i], true))
						return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Write a byte to the screen.
		/// </summary>
		/// <remarks>
		/// Changes the output-flag!
		/// </remarks>
		/// <param name="writeByte">Output byte</param>
		/// <param name="moveCursor">Move the cursor or not</param>
		/// <returns>True if byte has been written</returns>
		public bool WriteByte(byte writeByte, bool moveCursor) 
		{
			if (vs==null)
				return false;
			else 
			{
				switch(writeByte) 
				{
					case 10:
						// NL
						this.CursorY0++;
						break;
					case 13:
						// CR
						this.CursorX0=0;
						break;
					default:
						int y = CursorY0;
						if (this.visibleAreaY0top>0)
							y -= this.visibleAreaY0top; 
						if (y>=0) 
						{
							try 
							{
								this.vs[CursorX0, y] = writeByte;
							} 
							catch
							{
								// boundary problems should never occur, however
							}
						}
						if (moveCursor)
							this.MoveCursor(1);
						break;
				}
				this.changedScreen = true;
			}
			return true;
		}

		/// <summary>
		/// Write a string to the screen, and set new cursor position.
		/// </summary>
		/// <remarks>
		/// Changes the output-flag!
		/// </remarks>
		/// <param name="s">Output string</param>
		/// <returns>True if byte has been written</returns>
		public bool WriteLine(String s) 
		{
			if (s==null)
				return false;
			else
				return this.Write(s + "\n\r");
		}

		/// <summary>
		/// Write a string to the screen, and set new cursor position.
		/// </summary>
		/// <remarks>
		/// Changes the output-flag!
		/// </remarks>
		/// <param name="s">Output string</param>
		/// <returns>True if string has been written</returns>
		public bool Write(string s) 
		{
			if (s==null)
				return false;
			else
				return this.WriteByte(Encoding.ASCII.GetBytes(s));
		}

		/// <summary>
		/// Write a char to the screen, and set new cursor position.
		/// </summary>
		/// <remarks>
		/// Changes the output-flag!
		/// </remarks>
		/// <param name="c">Output char</param>
		/// <returns>True if char has been written</returns>
		public bool Write(char c) 
		{
			return this.Write(new string(c, 1));
		}

		/// <summary>
		/// Move cursor +/- positions forward.
		/// Scrolls up if necessary.
		/// </summary>
		/// <param name="positions">Positions to move (+ forward / - backwards)</param>
		/// <remarks>
		/// Changes the scrolledUp attribute!
		/// </remarks>
		public void MoveCursor(int positions) 
		{
			if (positions==0)
				return;
			int dy = positions / this.Width;
			int dx = positions - (dy * this.Width); // remaining x

			// fix dx / dy if necessary
			if (dx>=0) 
			{
				// move forward
				if ((this.CursorX0 + dx) >= this.Width) 
				{
					dy++;
					dx = dx - this.Width;
				}
			} 
			else 
			{
				// move backward (dx is NEGATIVE)
				if(this.CursorX0 + dx <0) 
				{
					dy --; // one line up
					dx = dx - this.Width;
				}
			}
			
			// new values:
			// do we have to scroll, line wraping for x is guaranteed
			int ny = this.CursorY0 + dy;
			int nx = this.CursorX0 + dx;
			if (ny > this.visibleAreaY0bottom) 
			{
				int sUp = ny - this.visibleAreaY0bottom;
				this.ScrollUp(sUp);
				this.visibleAreaY0bottom += sUp;
				this.visibleAreaY0top = this.visibleAreaY0bottom - this.Height -1;
			}
			this.CursorY0 = ny;
			this.CursorX0 = nx; // since we use the PROPERTY exceeding values are cut
		}

		/// <summary>
		/// Move the cursor n rows down (+) or up(-)
		/// </summary>
		/// <param name="lines">Number of rows up(-) or down(+)</param>
		/// <remarks>
		/// Changes the scrolledUp attribute!
		/// </remarks>
		public void MoveCursorVertical(int lines) 
		{
			this.MoveCursor(lines * this.Width);
		}

		/// <summary>
		/// Move cursor to a position considering 
		/// scrolling up / lines breaks
		/// </summary>
		/// <param name="xPos">X Position considering offset</param>
		/// <param name="yPos">Y Position considering offset</param>
		/// <returns>true if cursor could be moved</returns>
		/// <remarks>
		/// Just to set a cursor position the attributes CursorX / CursorY
		/// could be used. This here features scrolling.
		/// </remarks>
		/// <remarks>
		/// Changes the scrolledUp attribute!
		/// </remarks>
		public bool MoveCursorTo(int xPos, int yPos) 
		{
			int x0 = xPos - this.offsetx;
			int y0 = yPos - this.offsety;

			// check
			if (x0<0 || y0<0)
				return false;

			// determine extra lines because of 
			// X-Pos too high
			int dy = x0 / this.Width;
			if (dy>0) 
			{
				y0 += dy;
				x0 = x0 - (dy*this.Width);
			}
			
			// do we have to scroll?
			if (y0 > this.visibleAreaY0bottom) 
			{
				int sUp = y0 - this.visibleAreaY0bottom;
				this.ScrollUp(sUp);
				this.visibleAreaY0bottom = y0+sUp;
				this.visibleAreaY0top = this.visibleAreaY0bottom - this.Height -1;
			}

			// set values
			this.CursorX0 = x0;
			this.CursorY0 = y0;
			return true;
		}

		/// <summary>
		/// Clean everything up
		/// </summary>
		public void Dispose() 
		{
			this.vs = null; // break link to array
			this.screenString = null;
			this.screenStringLower = null;
		}

		/// <summary>
		/// Get a line as string
		/// </summary>
		/// <param name="yPosition"></param>
		/// <returns></returns>
		public string GetLine(int yPosition) 
		{
			int y0 = yPosition - this.offsety;
			if (vs==null || y0 >= this.Height || this.Width < 1)
				return null;
			else 
			{
				byte[] la = new byte[this.Width];
				for (int x=0; x < this.Width; x++) 
				{
					la[x] = this.vs[x, y0];
				}
				return Encoding.ASCII.GetString(la, 0, la.Length);
			}
		}

		/// <summary>
		/// Class info
		/// </summary>
		/// <returns></returns>
		override public string ToString() 
		{
			return this.GetType().FullName + " " + this.Width + " | " + this.Height + " | changed " + this.changedScreen;
		}

		/// <summary>
		/// Return the values as string
		/// </summary>
		/// <returns>Screen buffer as string including NLs (newlines)</returns>
		public string Hardcopy() 
		{
			return this.Hardcopy(false);
		}

		/// <summary>
		/// Return the values as string
		/// </summary>
		/// <param name="lowercase">true return as lower case</param>
		/// <returns>Screen buffer as string including NLs (newlines)</returns>
		public string Hardcopy(bool lowercase) 
		{
			if (this.vs==null)
				return null;
			else 
			{
				if (this.changedScreen || this.screenString==null) 
				{
					int cap = this.Width * this.Height;
					StringBuilder sb = new StringBuilder(cap);
					for (int y = 0; y < this.Height; y++) 
					{
						if (y>0)
							sb.Append('\n');
						sb.Append(this.GetLine(y + this.offsety));
					} // for
					this.screenString = sb.ToString();
					this.changedScreen = false; // reset the flag
					if (!lowercase) 
					{
						return this.screenString;
					}
					else 
					{
						this.screenStringLower = this.screenString.ToLower();
						return this.screenStringLower;
					}
				} 
				else 
				{
					// return from cache
					if (lowercase) 
					{
						if (this.screenStringLower==null)
							this.screenStringLower = this.screenString.ToLower();
						return this.screenStringLower;
					} 
					else
						return this.screenString; // from cache
				}
			}
		} // method Hardcopy

		/// <summary>
		/// Find a string on the screen
		/// </summary>
		/// <param name="findString">String to find</param>
		/// <param name="caseSensitive">true for case sensitive search</param>
		/// <returns>string found</returns>
		public string FindOnScreen(string findString, bool caseSensitive) 
		{
			if (this.vs == null || findString==null || findString.Length<1)
				return null;
			try {
				string screen = (caseSensitive) ? this.Hardcopy() : this.Hardcopy(true);
				int index = (caseSensitive) ? screen.IndexOf(findString) : screen.IndexOf(findString.ToLower());
				if (index <0) 
					return null;
				else
				{
					if (caseSensitive) 
						return findString;
					else 
					{	
						// return the orignal string
						return this.Hardcopy().Substring(index, findString.Length);
					}
				}
			} 
			catch 
			{
				// Null pointer etc.
				return null;
			}
		} // FindOnScreen

		/// <summary>
		/// Find a regular expression on the screen
		/// </summary>
		/// <param name="regExp">Regular expression to find</param>
		/// <returns>string found</returns>
		public string FindRegExOnScreen(string regExp) 
		{
			return this.FindRegExOnScreen(regExp, false);

		} // find regular expression

		/// <summary>
		/// Find a regular expression on the screen
		/// </summary>
		/// <param name="regExp">Regular expression to find</param>
		/// <param name="caseSensitive">true for case sensitive search</param>
		/// <returns>string found</returns>
		public string FindRegExOnScreen(string regExp, bool caseSensitive) 
		{
            if (this.vs ==null || regExp == null || regExp.Length < 1)
				return null;
			Regex r = caseSensitive ? new Regex(regExp) : new Regex(regExp, RegexOptions.IgnoreCase);
			Match m = r.Match(this.Hardcopy()); // Remark: hardcopy uses a cache !
			if (m!=null && m.Success) 
				return m.Value;
			else 
				return null;
		} // find regular expression

		/// <summary>
		/// Find a regular expression on the screen
		/// </summary>
		/// <param name="regExp">Regular expression to find</param>
		/// <returns>Mathc object or null</returns>
		public Match FindRegExOnScreen(Regex regExp) 
		{
			if (this.vs ==null || regExp == null)
				return null;
			Match m = regExp.Match(this.Hardcopy()); // Remark: hardcopy uses a cache !
			if (m!=null && m.Success) 
				return m;
			else 
				return null;
		} // find regular expression
	} // class
} // namespace