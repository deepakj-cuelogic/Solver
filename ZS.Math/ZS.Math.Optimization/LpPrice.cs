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
        internal static int find_rowReplacement(lprec lp, int rownr, ref double[] prow, ref int[] nzprow)
        {
            /* The logic in this section generally follows Chvatal: Linear Programming, p. 130
           Basically, the function is a specialized coldual(). */

            int i;
            int bestindex;
            double bestvalue = new double();
            LpCls objLpCls = new LpCls();

            /* Solve for "local reduced cost" */
            objLpCls.set_action(ref lp.piv_strategy, lp_lib.PRICE_FORCEFULL);
            int[] nullpara = null;
            double[] nullpara2 = null;
            //ORIGINAL LINE: compute_reducedcosts(lp, true, rownr, null, 1, prow, nzprow, null, null, lp_matrix.MAT_ROUNDDEFAULT);
            compute_reducedcosts(lp, true, rownr, ref nullpara, true, ref prow, ref nzprow, ref nullpara2, ref nullpara, lp_matrix.MAT_ROUNDDEFAULT);
            LpCls.clear_action(ref lp.piv_strategy, lp_lib.PRICE_FORCEFULL);

            /* Find a suitably non-singular variable to enter ("most orthogonal") */
            bestindex = 0;
            bestvalue = 0;
            for (i = 1; i <= lp.sum - System.Math.Abs(lp.P1extraDim); i++)
            {
                if (!lp.is_basic[i] && !lp_LUSOL.is_fixedvar(lp, i) && (System.Math.Abs(prow[i]) > bestvalue))
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
                lp_matrix.fsolve(lp, bestindex, prow, nzprow, lp.epsmachine, 1.0, true);
            }

            return (bestindex);
        }

        internal static void compute_reducedcosts(lprec lp, bool? isdual, int row_nr, ref int[] coltarget, bool? dosolve, ref double[] prow, ref int[] nzprow, ref double[] drow, ref int[] nzdrow, int roundmode)
        {
            LpCls objLpCls = new LpCls();
            double epsvalue = lprec.epsvalue; // Any larger value can produce a suboptimal result
            roundmode |= lp_matrix.MAT_ROUNDRC;

            if (isdual != null)
            {
                lp_matrix.bsolve_xA2(lp, coltarget, row_nr, ref prow, epsvalue, nzprow, 0, drow, epsvalue, nzdrow, roundmode);
            }
            else
            {
                /// <summary> FIX_20521988-5de6-4a36-b964-ff9504331085 26/11/18
                /// PREVIOUS: double[] bVector = 0;
                /// ERROR IN PREVIOUS: Cannot implicitly convert type 'int' to 'double[]'
                /// FIX 1: List<double> bVector = new List<double>();
                /// </summary>
                List<double> bVector = new List<double>();

                ///#if 1
                if ((lp.multivars == null) && (lp.P1extraDim == 0))
                {
                    bVector[0] = drow[0];
                }
                else
                {
                    ///#endif
                    /// FIX_20521988-5de6-4a36-b964-ff9504331085 26/11/18
                    /// PREVIOUS bVector = lp.bsolveVal;
                    bVector.Add(lp.bsolveVal);
                }
                if (dosolve != null)
                {
                    ///FIX_20521988-5de6-4a36-b964-ff9504331085 26/11/18
                    /// PREVIOUS: ref bVector[0]
                    double rhsvector = bVector[0];
                    int? nzidx = lp.bsolveIdx;
                    lp_matrix.bsolve(lp, 0, ref rhsvector, ref nzidx, epsvalue * lp_lib.DOUBLEROUND, 1.0);
                    if (isdual == null && (row_nr == 0) && (lp.improve!=0 && lp_lib.IMPROVE_SOLUTION!=0) && !LpCls.refactRecent(lp) && serious_facterror(lp, ref rhsvector, lp.rows, lprec.epsvalue))
                    {
                        lp.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
                    }
                }
                ///FIX_20521988-5de6-4a36-b964-ff9504331085 26/11/18
                /// need to convert List<double> to double[] for passing as a parameter 
                double[] arrbVector = new double[bVector.Count];
                for (int idx = 0; idx < bVector.Count; idx++)
                {
                    arrbVector[idx] = bVector[idx];
                }
                lp_matrix.prod_xA(lp, coltarget, arrbVector, lp.bsolveIdx, epsvalue, 1.0, drow, nzdrow, roundmode);
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

        /* Routine to verify accuracy of the current basis factorization */
        internal static bool serious_facterror(lprec lp, ref double bvector, int maxcols, double tolerance)
        {
            int i;
            int j;
            int ib;
            int ie;
            int nz;
            int nc;
            double sum = new double();
            double tsum = 0;
            double err = 0;
            MATrec mat = lp.matA;
            LpCls objLpCls = new LpCls();

            if (bvector == 0)
            {
                bvector = lp.bsolveVal;
            }
            nc = 0;
            nz = 0;
            for (i = 1; (i <= lp.rows) && (nc <= maxcols); i++)
            {

                /* Do we have a non-slack variable? (we choose to skip slacks,
                  since they have "natural" good accuracy properties) */
                j = lp.var_basic[i] - lp.rows;
                if (j <= 0)
                {
                    continue;
                }
                nc++;

                /* Compute cross product for basic, non-slack column */
                ib = mat.col_end[j - 1];
                ie = mat.col_end[j];
                nz += ie - ib;
                sum = LpCls.get_OF_active(lp, j + lp.rows, bvector);
                for (; ib < ie; ib++)
                {
                    //ORIGINAL CODE: sum += lp_matrix.COL_MAT_VALUE(ib) * bvector[lp_matrix.COL_MAT_ROWNR(ib)];
                    sum += lp_matrix.COL_MAT_VALUE(ib) * bvector;
                }

                /* Catch high precision early, so we don't to uneccessary work */
                tsum += sum;
                commonlib.SETMAX(Convert.ToInt32(err), System.Math.Abs(Convert.ToInt32(sum)));
                if ((tsum / nc > tolerance / 100) && (err < tolerance / 100))
                {
                    break;
                }
            }
            err /= mat.infnorm;
            return ((bool)(err >= tolerance));
        }

    }
}
