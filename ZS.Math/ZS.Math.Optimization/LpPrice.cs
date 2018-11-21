using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public static class LpPrice
    {
        /* Find an entering column for the case that the specified basic variable
        is fixed or zero - typically used for artificial variable elimination */
        internal static int find_rowReplacement(lprec lp, int rownr, ref double prow, ref int nzprow)
        {
            /* The logic in this section generally follows Chvatal: Linear Programming, p. 130
           Basically, the function is a specialized coldual(). */

            int i;
            int bestindex;
            double bestvalue = new double();
            LpCls objLpCls = new LpCls();

            /* Solve for "local reduced cost" */
            objLpCls.set_action(ref lp.piv_strategy, lp_lib.PRICE_FORCEFULL);
            compute_reducedcosts(lp, 1, rownr, null, 1, prow, nzprow, null, null, MAT_ROUNDDEFAULT);
            clear_action(lp.piv_strategy, PRICE_FORCEFULL);

            /* Find a suitably non-singular variable to enter ("most orthogonal") */
            bestindex = 0;
            bestvalue = 0;
            for (i = 1; i <= lp.sum - System.Math.Abs(lp.P1extraDim); i++)
            {
                if (!lp.is_basic[i] && !is_fixedvar(lp, i) && (System.Math.Abs(prow[i]) > bestvalue))
                {
                    bestindex = i;
                    bestvalue = System.Math.Abs(prow[i]);
                }
            }

            /* Prepare to update inverse and pivot/iterate (compute Bw=a) */
            if (i > lp.sum - System.Math.Abs(lp.P1extraDim))
            {
                bestindex = 0;
            }
            else
            {
                fsolve(lp, bestindex, prow, nzprow, lp.epsmachine, 1.0, 1);
            }

            return (bestindex);
        }

        internal static void compute_reducedcosts(lprec lp, bool isdual, int row_nr, ref int coltarget, bool dosolve, ref double prow, ref int nzprow, ref double drow, ref int nzdrow, int roundmode)
        {
            LpCls objLpCls = new LpCls();
            double epsvalue = lprec.epsvalue; // Any larger value can produce a suboptimal result
            roundmode |= lp_matrix.MAT_ROUNDRC;

            if (isdual != null)
            {
                bsolve_xA2(lp, coltarget, row_nr, prow, epsvalue, nzprow, 0, drow, epsvalue, nzdrow, roundmode);
            }
            else
            {
                REAL bVector;

                //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                ///#if 1
                if ((lp.multivars == null) && (lp.P1extraDim == 0))
                {
                    bVector = drow;
                }
                else
                {
                    ///#endif
                    bVector = lp.bsolveVal;
                }
                if (dosolve != null)
                {
                    bsolve(lp, 0, bVector, lp.bsolveIdx, epsvalue * DOUBLEROUND, 1.0);
                    if (isdual == null && (row_nr == 0) && (lp.improve & IMPROVE_SOLUTION) && !refactRecent(lp) && serious_facterror(lp, bVector, lp.rows, lp.epsvalue))
                    {
                        set_action(lp.spx_action, ACTION_REINVERT);
                    }
                }
                prod_xA(lp, coltarget, bVector, lp.bsolveIdx, epsvalue, 1.0, drow, nzdrow, roundmode);
            }
        }

        internal static int partial_blockStart(lprec lp, bool isrow)
        {
            partialrec blockdata;

            //NOTED ISSUE
            blockdata = commonlib.IF(isrow, lp.rowblocks, lp.colblocks);
            if (blockdata == null)
            {
                return (1);
            }
            else
            {
                if ((blockdata.blocknow < 1) || (blockdata.blocknow > blockdata.blockcount))
                {
                    blockdata.blocknow = 1;
                }
                return (blockdata.blockend[blockdata.blocknow - 1]);
            }
        }

        internal static int partial_blockEnd(lprec lp, bool isrow)
        {
            partialrec blockdata;

            //NOTED ISSUE
            blockdata = commonlib.IF(isrow, lp.rowblocks, lp.colblocks);
            if (blockdata == null)
            {
                return (commonlib.IF(isrow, lp.rows, lp.sum));
            }
            else
            {
                if ((blockdata.blocknow < 1) || (blockdata.blocknow > blockdata.blockcount))
                {
                    blockdata.blocknow = 1;
                }
                return (blockdata.blockend[blockdata.blocknow] - 1);
            }
        }
    }
}
