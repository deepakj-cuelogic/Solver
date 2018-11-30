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
        internal static int bfp_rowoffset(lprec lp)
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

        internal static int bfp_rowextra(lprec lp)
        {
            if (lp.is_obj_in_basis(lp))
                return (1);
            else
                return (0);
        }

        /* DON'T MODIFY */
        private int bfp_createMDO(lprec lp, bool usedpos, int count, bool doMDO)
        {
            throw new NotImplementedException();
        }

        /* DON'T MODIFY */
        internal static int bfp_pivotcount(lprec lp)
        {
            return (lp.invB.num_pivots);
        }

    }
}
