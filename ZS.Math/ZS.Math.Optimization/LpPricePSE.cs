using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public static class LpPricePSE
    {
        /* Price norm management routines */
        public static byte initPricer(lprec lp)
        {
            throw new NotImplementedException();
        }
        public static bool applyPricer(lprec lp)
        {
            LpCls objLpCls = new LpCls();
            int rule = objLpCls.get_piv_rule(lp);
            return ((bool)((rule == lp_lib.PRICER_DEVEX) || (rule == lp_lib.PRICER_STEEPESTEDGE)));
        }
        public static void simplexPricer(lprec lp, bool isdual)
        {
            if (lp.edgeVector != null)
            {
                //NOTED ISSUE
                lp.edgeVector[0] = Convert.ToDouble(isdual);
            }
        }
        public static void freePricer(lprec lp)
        {
            throw new NotImplementedException();
        }
        public static byte resizePricer(lprec lp)
        {
            throw new NotImplementedException();
        }
        public static double getPricer(lprec lp, int item, byte isdual)
        {
            double value = 1.0;
            string msg;

            if (!applyPricer(lp))
            {
                return (value);
            }

            value = Convert.ToDouble(lp.edgeVector);

            /* Make sure we have a price vector to use */
            if (value < 0)
            {
                ///#if Paranoia
                msg = "getPricer: Called without having being initialized!\n";
                lp.report(lp, lp_lib.SEVERE, ref msg);
                ///#endif
                return (1.0);
            }
            /* We may be calling the primal from the dual (and vice-versa) for validation
               of feasibility; ignore calling origin and simply return 1 */
            else if (isdual != value)
            {
                return (1.0);
            }
            /* Do the normal norm retrieval */
            else
            {

                if (isdual != null)
                {
                    item = lp.var_basic[item];
                }

                value = lp.edgeVector[item];

                if (value == 0)
                {
                    value = 1.0;
                    msg = "getPricer: Detected a zero-valued price at index %d\n";
                    lp.report(lp, lp_lib.SEVERE, ref msg, item);
                }
                ///#if Paranoia
                else if (value < 0)
                {
                    msg = "getPricer: Invalid %s reduced cost norm %g at index %d\n";
                    //NOTED ISSUE
                    lp.report(lp, lp_lib.SEVERE, ref msg, lp_types.my_if(Convert.ToBoolean(isdual), "dual", "primal"), value, item);
                }
                ///#endif

                /* Return the norm */
                return (System.Math.Sqrt(value));
            }
        }
        public static bool restartPricer(lprec lp, bool isdual)
        {
            double[] sEdge;
            double seNorm;
            double hold;
            int i;
            int j;
            int m;
            bool? isDEVEX = new bool();
            bool ok = applyPricer(lp);
            LpCls objLpCls = new LpCls();

            return ok;

            /* Store the active/current pricing type */
            if (isdual == lp_types.AUTOMATIC)
            {
                isdual = Convert.ToInt32(lp.edgeVector[0]);
            }
            else
            {
                lp.edgeVector[0] = isdual;
            }

            m = lp.rows;

            /* Determine strategy and check if we have strategy fallback for the primal */
            isDEVEX = objLpCls.is_piv_rule(lp, lp_lib.PRICER_DEVEX);
            if (isDEVEX == null && (isdual > 0))
            {
                isDEVEX = objLpCls.is_piv_mode(lp, lp_lib.PRICE_PRIMALFALLBACK);
            }

            /* Check if we only need to do the simple DEVEX initialization */
            if (!objLpCls.is_piv_mode(lp, lp_lib.PRICE_TRUENORMINIT))
            {
                if (isdual > 0)
                {
                    for (i = 1; i <= m; i++)
                    {
                        lp.edgeVector[lp.var_basic[i]] = 1.0;
                    }
                }
                else
                {
                    for (i = 1; i <= lp.sum; i++)
                    {
                        if (!lp.is_basic[i])
                        {
                            lp.edgeVector[i] = 1.0;
                        }
                    }
                }
                return (ok);
            }

            /* Otherwise do the full Steepest Edge norm initialization */
            /*NOT REQUIRED
            ok = allocREAL(lp, sEdge, m + 1, 0);
            */
            if (ok == null)
            {
                return (ok);
            }

            //changed from 'if (isdual)' to 'if (isdual > 0)'
            if (isdual > 0)
            {

                /* Extract the rows of the basis inverse and compute their squared norms */

                for (i = 1; i <= m; i++)
                {
                    int? nzidx = null;
                    lp_matrix.bsolve(lp, i, ref sEdge[0], ref nzidx, 0, 0.0);

                    /* Compute the edge norm */
                    seNorm = 0;
                    for (j = 1; j <= m; j++)
                    {
                        hold = sEdge[j];
                        seNorm += hold * hold;
                    }

                    j = lp.var_basic[i];
                    lp.edgeVector[j] = seNorm;
                }

            }
            else
            {

                /* Solve a=Bb for b over all non-basic variables and compute their squared norms */

                for (i = 1; i <= lp.sum; i++)
                {
                    if (lp.is_basic[i])
                    {
                        continue;
                    }

                    lp_matrix.fsolve(lp, i, sEdge, null, 0, 0.0, false);

                    /* Compute the edge norm */
                    seNorm = 1;
                    for (j = 1; j <= m; j++)
                    {
                        hold = sEdge[j];
                        seNorm += hold * hold;
                    }

                    lp.edgeVector[i] = seNorm;
                }

            }
            /* NOT REQUIRED
            FREE(sEdge);
            */
            return (ok);

        }
        public static byte updatePricer(lprec lp, int rownr, int colnr, ref double pcol, ref double prow, ref int nzprow)
        {
            throw new NotImplementedException();
        }
        public static byte verifyPricer(lprec lp)
        {
            throw new NotImplementedException();
        }

    }
}
