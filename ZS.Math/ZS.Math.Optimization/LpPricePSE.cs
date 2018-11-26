﻿using System;
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
                lp.edgeVector[0] = (double)isdual;
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
            throw new NotImplementedException();
        }
        public static bool restartPricer(lprec lp, bool isdual)
        {
            double sEdge = 0;
            double seNorm;
            double hold;
            int i;
            int j;
            int m;
            bool isDEVEX = new bool();
            bool ok = applyPricer(lp);

            if (ok == null)
            {
                return (ok);
            }

            /* Store the active/current pricing type */
            if (isdual == AUTOMATIC)
            {
                isdual = (MYBOOL)lp.edgeVector[0];
            }
            else
            {
                lp.edgeVector[0] = isdual;
            }

            m = lp.rows;

            /* Determine strategy and check if we have strategy fallback for the primal */
            isDEVEX = is_piv_rule(lp, PRICER_DEVEX);
            if (isDEVEX == null && !isdual)
            {
                isDEVEX = is_piv_mode(lp, PRICE_PRIMALFALLBACK);
            }

            /* Check if we only need to do the simple DEVEX initialization */
            if (!is_piv_mode(lp, PRICE_TRUENORMINIT))
            {
                if (isdual)
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
            ok = allocREAL(lp, sEdge, m + 1, 0);
            if (ok == null)
            {
                return (ok);
            }

            if (isdual)
            {

                /* Extract the rows of the basis inverse and compute their squared norms */

                for (i = 1; i <= m; i++)
                {

                    bsolve(lp, i, sEdge, null, 0, 0.0);

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

                    fsolve(lp, i, sEdge, null, 0, 0.0, 0);

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

            FREE(sEdge);

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
