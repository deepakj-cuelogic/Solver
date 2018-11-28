using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public static class LpPricePSE
    {
        /* Price norm management routines */
        public static bool initPricer(lprec lp)
        {
            if (!applyPricer(lp))
            {
                return false;
            }

            /* Free any pre-existing pricer */
            freePricer(lp);

            /* Allocate vector to fit current problem size */
            return (resizePricer(lp));

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
        public static bool resizePricer(lprec lp)
        {
            if (!applyPricer(lp))
            {
                return true;
            }

            /* NOT REQUIRED
            // Reallocate vector for new size 
            if (!allocREAL(lp, (lp.edgeVector), lp.sum_alloc + 1, AUTOMATIC))
            {
                return (0);
            }

            // Signal that we have not yet initialized the price vector 
            MEMCLEAR(lp.edgeVector, lp.sum_alloc + 1);
            */

            lp.edgeVector[0] = -1;
            return true;

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
        //Changed By: CS Date:28/11/2018
        public static bool updatePricer(lprec lp, int rownr, int colnr, double pcol, ref double prow, ref int nzprow)
        {
            double[] vEdge = null;
            double cEdge;
            double hold;
            double newEdge;
            double[] w = null;
            int i;
            int m;
            int n;
            int exitcol;
            int errlevel = lp_lib.DETAILED;
            bool forceRefresh = false;
            bool isDual = new bool();
            bool isDEVEX = new bool();
            bool ok = false;
            string msg = "";

            LpCls objLpCls = new LpCls();

            if (!applyPricer(lp))
            {
                return (ok);
            }

            /* Make sure we have something to update */
            hold = lp.edgeVector[0];
            if (hold < 0)
            {
                return (ok);
            }
            
            //ORIGINAL LINE: isDual = (bool)(hold > 0);
            isDual = ((bool)(hold > 0));

            /* Do common initializations and computations */
            m = lp.rows;
            n = lp.sum;
            isDEVEX = objLpCls.is_piv_rule(lp, lp_lib.PRICER_DEVEX);
            exitcol = lp.var_basic[rownr];

            /* Solve/copy Bw = a */
#if false
//  ok = formWeights(lp, colnr, NULL, &w);  // Compute from scratch - Experimental 
#else
            ok = formWeights(lp, colnr, ref pcol, w); // Use previously computed values
#endif
            if (ok == null)
            {
                return (ok);
            }

            /* Price norms for the dual simplex - the basic columns */
            if (isDual != null)
            {
                double rw = new double();
                int targetcol;

                /* Don't need to compute cross-products with DEVEX */
                if (isDEVEX == null)
                {
                    //ok = allocREAL(lp, vEdge, m + 1, 0);
                    if (ok == null)
                    {
                        return (ok);
                    }

                    /* Extract the row of the inverse containing the leaving variable
                       and then form the dot products against the other variables, i.e. "Tau" */
#if false
//      bsolve(lp, rownr, vEdge, 0, 0.0);
#else
                    //NOT REQUIRED
                    //MEMCOPY(vEdge, prow, m + 1);
                    vEdge[0] = 0;
#endif
                    int Parameter = 0;
                    lp.bfp_ftran_normal(lp, ref vEdge[0], Parameter);
                }

                /* Update the squared steepest edge norms; first store some constants */
                cEdge = lp.edgeVector[exitcol];
                rw = w[rownr];
                if (System.Math.Abs(rw) < lp.epspivot)
                {
                    forceRefresh = true;
                    goto Finish2;
                }

                /* Deal with the variable entering the basis to become a new leaving candidate */
                hold = 1 / rw;
                lp.edgeVector[colnr] = (hold * hold) * cEdge;

#if Paranoia
	if (lp.edgeVector[colnr] <= lp.epsmachine)
	{
	  report(lp, errlevel, "updatePricer: Invalid dual norm %g at entering index %d - iteration %.0f\n", lp.edgeVector[colnr], rownr, (double)(lp.total_iter + lp.current_iter));
	}
#endif

                /* Then loop over all basic variables, but skip the leaving row */
                for (i = 1; i <= m; i++)
                {
                    if (i == rownr)
                    {
                        continue;
                    }
                    targetcol = lp.var_basic[i];
                    hold = w[i];
                    if (hold == 0)
                    {
                        continue;
                    }
                    hold /= rw;
                    if (System.Math.Abs(hold) < lp.epsmachine)
                    {
                        continue;
                    }

                    newEdge = (lp.edgeVector[targetcol]);
                    newEdge += (hold * hold) * cEdge;
                    if (isDEVEX != null)
                    {
                        if ((newEdge) > lp_lib.DEVEX_RESTARTLIMIT)
                        {
                            forceRefresh = true;
                            break;
                        }
                    }
                    else
                    {
                        newEdge -= 2 * hold * vEdge[i];
#if xxApplySteepestEdgeMinimum
		SETMAX(*newEdge, hold * hold + 1); // Kludge; use the primal lower bound
#else

                        if (newEdge <= 0)
                        {
                            msg = "updatePricer: Invalid dual norm %g at index %d - iteration %.0f\n";
                            lp.report(lp, errlevel, ref msg, newEdge, i, (double)(lp.total_iter + lp.current_iter));
                            forceRefresh = true;
                            break;
                        }
#endif
                    }
                }


            }
            /* Price norms for the primal simplex - the non-basic columns */
            else
            {
                double[] vTemp = null;
                double[] vAlpha = null;
                double cAlpha = new double();
                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                //ORIGINAL LINE: int *coltarget;
                int coltarget = 0;

                //NOT REQUIRED
                //ok = allocREAL(lp, vTemp, m + 1, 1) && allocREAL(lp, vAlpha, n + 1, 1);
                if (!ok)
                {
                    return (ok);
                }

                /* Check if we have strategy fallback for the primal */
                if (!isDEVEX)
                {
                    isDEVEX = objLpCls.is_piv_mode(lp, lp_lib.PRICE_PRIMALFALLBACK);
                }

                /* Initialize column target array */
                //NOTED ISSUE
                coltarget = (int)lp_utils.mempool_obtainVector(lp.workarrays, lp.sum + 1, sizeof(int));
                ok = lp_matrix.get_colIndexA(lp, lp_lib.SCAN_SLACKVARS + lp_lib.SCAN_USERVARS + lp_lib.USE_NONBASICVARS, coltarget, false);
                if (!ok)
                {
                    lp_utils.mempool_releaseVector(lp.workarrays, ref Convert.ToString(coltarget), 0);
                    return (ok);
                }

                /* Don't need to compute cross-products with DEVEX */
                if (!isDEVEX)
                {
                    //NOT REQUIRED
                    //ok = allocREAL(lp, vEdge, n + 1, 1);
                    if (!ok)
                    {
                        return (ok);
                    }

                    /* Compute v and then N'v */
                    //NOT REQUIRED
                    //MEMCOPY(vTemp, w, m + 1);
                    int? Para = null;
                    //NOTED ISSUE
                   lp_matrix.bsolve(lp, -1, ref vTemp, ref Para, lp.epsmachine * lp_lib.DOUBLEROUND, 0.0);
                    vTemp[0] = 0;
                    lp_matrix.prod_xA(lp, coltarget, vTemp, null, lp.epsmachine, 0.0, vEdge, null, lp_matrix.MAT_ROUNDDEFAULT);
                }

                /* Compute Sigma and then Alpha */
                int? Parameter = null;
                lp_matrix.bsolve(lp, rownr, ref vTemp, ref Parameter, 0 * lp_lib.DOUBLEROUND, 0.0);
                vTemp[0] = 0;
                lp_matrix.prod_xA(lp, coltarget, vTemp, null, lp.epsmachine, 0.0, vAlpha, null, lp_matrix.MAT_ROUNDDEFAULT);
                lp_utils.mempool_releaseVector(lp.workarrays, (string)coltarget, 0);

                /* Update the squared steepest edge norms; first store some constants */
                cEdge = lp.edgeVector[colnr];
                cAlpha = vAlpha[colnr];
                if (System.Math.Abs(cAlpha) < lp.epspivot)
                {
                    forceRefresh = true;
                    //goto Finish1;
                }

                /* Deal with the variable leaving the basis to become a new entry candidate */
                hold = 1 / cAlpha;
                lp.edgeVector[exitcol] = (hold * hold) * cEdge;

#if Paranoia
	if (lp.edgeVector[exitcol] <= lp.epsmachine)
	{
	  report(lp, errlevel, "updatePricer: Invalid primal norm %g at leaving index %d - iteration %.0f\n", lp.edgeVector[exitcol], exitcol, (double)(lp.total_iter + lp.current_iter));
	}
#endif

                /* Then loop over all non-basic variables, but skip the entering column */
                for (i = 1; i <= lp.sum; i++)
                {
                    if (lp.is_basic[i] || (i == colnr))
                    {
                        continue;
                    }
                    hold = vAlpha[i];
                    if (hold == 0)
                    {
                        continue;
                    }
                    hold /= cAlpha;
                    if (System.Math.Abs(hold) < lp.epsmachine)
                    {
                        continue;
                    }

                    newEdge = (lp.edgeVector[i]);
                    newEdge += (hold * hold) * cEdge;
                    if (isDEVEX)
                    {
                        if ((newEdge) > lp_lib.DEVEX_RESTARTLIMIT)
                        {
                            forceRefresh = true;
                            break;
                        }
                    }
                    else
                    {
                        newEdge -= 2 * hold * vEdge[i];
#if ApplySteepestEdgeMinimum
		SETMAX(*newEdge, hold * hold + 1);
#else
                        if (newEdge < 0)
                        {
                            msg = "updatePricer: Invalid primal norm %g at index %d - iteration %.0f\n";
                            lp.report(lp, errlevel, ref msg, newEdge, i, (double)(lp.total_iter + lp.current_iter));
                            if (lp.spx_trace)
                            {
                                msg = "Error detail: (RelAlpha=%g, vEdge=%g, cEdge=%g)\n";
                                lp.report(lp, errlevel, ref msg, hold, vEdge[i], cEdge);
                            }
                            forceRefresh = true;
                            break;
                        }
#endif
                    }
                }

           // Finish1:
                //NOT REQUIRED
                //FREE(vAlpha);
                //FREE(vTemp);

            }

        Finish2:
            //NOT REQUIRED
            //FREE(vEdge);
            //freeWeights(w);

            if (forceRefresh)
            {
                ok = restartPricer(lp, Convert.ToBoolean(DefineConstants.AUTOMATIC));
            }
            else
            {
                ok = true;
            }

            return (ok);

        }
        public static byte verifyPricer(lprec lp)
        {
            throw new NotImplementedException();
        }

        //Changed By: CS Date:28/11/2018
        private static bool formWeights(lprec lp, int colnr, ref double pcol, double[] w)
        {
            /* This computes Bw = a, where B is the basis and a is a column of A */

            bool ok = true; //allocREAL(lp, w, lp.rows + 1, 0);

            if (ok)
            {
                if (pcol == null)
                {
                    lp_matrix.fsolve(lp, colnr, ref w[0], null, 0.0, 0.0, false);
                }
                else
                {
                    //NOT REQUIRED
                    //MEMCOPY(w[0], pcol, lp.rows + 1);
                    /*    *w[0] = 0; */
                    /* Test */
                }
            }
            /*
              if(pcol != NULL) {
                double cEdge, hold;
                int  i;

                cEdge = 0;
                for(i = 1; i <= m; i++) {
                  hold = *w[i]-pcol[i];
                  cEdge += hold*hold;
                }
                cEdge /= m;
                cEdge = sqrt(cEdge);
                if(cEdge > lp->epspivot)
                  report(lp, SEVERE, "updatePricer: MRS error is %g\n", cEdge);
              }
            */
            return (ok);
        }


    }
}