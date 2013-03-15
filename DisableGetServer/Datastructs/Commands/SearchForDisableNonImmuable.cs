using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisableGetServer.Datastructs.Commands
{
    public class SearchForDisableNonImmuable : SearchForDisable
    {
        public override bool IsImmuable
        {
            get
            {
                return false;
            }
        }

        public override bool IfNeedExecution
        {
            get
            {
                return true;
            }
        }

        public SearchForDisableNonImmuable(DisableGetObjects.Setting_Type_Switch sts) :base(sts)
        {
        }
    }
}
