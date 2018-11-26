using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class LpBFP1
    {
        /* DON'T MODIFY */
        //NOTED ISSUE: Need to check BFP_CALLMODEL
        //ORIGINAL LINE: internal int BFP_CALLMODEL bfp_rowoffset(lprec lp)
        //Above line needs to replace with current method signature
        internal int bfp_rowoffset(lprec lp)
        {
            if (lp.obj_in_basis)
            {
                return (1);
            }
            else
            {
                return (0);
            }
        }
    }
}
