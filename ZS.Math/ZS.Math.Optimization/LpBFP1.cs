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
        private int bfp_createMDO(lprec lp, bool[] usedpos, int count, bool doMDO)
        {
            int[] mdo;
            int i;
            int j;
            int kk;

            mdo = new int[(count + 1)];
            /*  allocINT(lp, &mdo, count + 1, FALSE); */

            /* Fill the mdo[] array with remaining full-pivot basic user variables */
            kk = 0;
            for (j = 1; j <= lp.columns; j++)
            {
                i = lp.rows + j;
                if (usedpos[i] == true)
                {
                    kk++;
                    mdo[kk] = i;
                }
            }
            mdo[0] = kk;
            if (kk == 0)
            {
                goto Process;
            }

            /* Calculate the approximate minimum degree column ordering */
            if (doMDO)
            {
                int size = 0;
                i = lp_MDO.getMDO(lp, ref usedpos[0], ref mdo, ref size, 0);
                if (i != 0)
                {
                    string msg = "bfp_createMDO: Internal error {0} in minimum degree ordering routine";
                    lp.report(lp, lp_lib.CRITICAL, ref msg, i);
                    /*NOT REQUIRED
                    FREE(mdo);
                    */
                }
            }
            Process:
            return (mdo[0]);

        }

        /* DON'T MODIFY */
        internal static int bfp_pivotcount(lprec lp)
        {
            return (lp.invB.num_pivots);
        }

    }
}
