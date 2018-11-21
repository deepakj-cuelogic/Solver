using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public static class lp_simplex
    {
        public const int OBJ_STEPS = 5;
        public const int PRICE_ADAPTIVE = 32;
        public const int MAX_STALLCOUNT = 12;
        public const int PRICER_DEVEX = 2;
        public const int MAX_RULESWITCH = 5;
        public const double epsprimal = 0;


        internal static void stallMonitor_update(lprec lp, double newOF)
        {
            int newpos;
            OBJmonrec monitor = lp.monitor;

            if (monitor.countstep < OBJ_STEPS)
            {
                monitor.countstep++;
            }
            else
            {
                monitor.startstep = commonlib.mod(monitor.startstep + 1, OBJ_STEPS);
            }
            newpos = commonlib.mod(monitor.startstep + monitor.countstep - 1, OBJ_STEPS);
            monitor.objstep[newpos] = newOF;
            monitor.idxstep[newpos] = monitor.Icount;
            monitor.currentstep = newpos;
        }

        internal static bool stallMonitor_creepingObj(lprec lp)
        {
            OBJmonrec monitor = lp.monitor;

            if (monitor.countstep > 1)
            {
                double deltaOF = (monitor.objstep[monitor.currentstep] - monitor.objstep[monitor.startstep]) / monitor.countstep;
                deltaOF /= commonlib.MAX(1, (monitor.idxstep[monitor.currentstep] - monitor.idxstep[monitor.startstep]));
                deltaOF = lp_types.my_chsign(monitor.isdual, deltaOF);
                return ((bool)(deltaOF < monitor.epsvalue));
            }
            else
            {
                return (false);
            }
        }

        internal static bool stallMonitor_shortSteps(lprec lp)
        {
            OBJmonrec monitor = lp.monitor;

            if (monitor.countstep == OBJ_STEPS)
            {
                double deltaOF = commonlib.MAX(1, (monitor.idxstep[monitor.currentstep] - monitor.idxstep[monitor.startstep])) / monitor.countstep;
                deltaOF = System.Math.Pow(deltaOF * OBJ_STEPS, 0.66);
                return ((bool)(deltaOF > monitor.limitstall[1]));
            }
            else
            {
                return (false);
            }
        }

        internal static void stallMonitor_reset(lprec lp)
        {
            OBJmonrec monitor = lp.monitor;

            monitor.ruleswitches = 0;
            monitor.Ncycle = 0;
            monitor.Mcycle = 0;
            monitor.Icount = 0;
            monitor.startstep = 0;
            monitor.objstep[monitor.startstep] = lp.infinite;
            monitor.idxstep[monitor.startstep] = monitor.Icount;
            monitor.prevobj = 0;
            monitor.countstep = 1;
        }

        internal static bool stallMonitor_create(lprec lp, bool isdual, ref string funcname)
        {
            OBJmonrec monitor = null;
            double suminfeas;
            if (lp.monitor != null)
            {
                return (false);
            }

            ///PREVIOUS CODE: monitor = (OBJmonrec)calloc(sizeof(monitor), 1);
            monitor = new OBJmonrec();
            if (monitor == null)
            {
                return (false);
            }

            monitor.lp = lp;
            monitor.spxfunc = funcname;
            monitor.isdual = isdual;
            monitor.pivdynamic = lp.is_piv_mode(lp, PRICE_ADAPTIVE);
            monitor.oldpivstrategy = lp.piv_strategy;
            monitor.oldpivrule = lp.get_piv_rule(lp);
            if (MAX_STALLCOUNT <= 1)
            {
                monitor.limitstall[0] = 0;
            }
            else
            {
                monitor.limitstall[0] = Convert.ToInt32(commonlib.MAX(MAX_STALLCOUNT, (int)System.Math.Pow((double)(lp.rows + lp.columns) / 2, 0.667)));
            }

            ///#if 1
            monitor.limitstall[0] *= 2 + 2; // Expand degeneracy/stalling tolerance range
                                            ///#endif
            monitor.limitstall[1] = monitor.limitstall[0];
            if (monitor.oldpivrule == PRICER_DEVEX) // Increase tolerance since primal Steepest Edge is expensive
            {
                monitor.limitstall[1] *= 2;
            }
            if (MAX_RULESWITCH <= 0)
            {
                ///NOT Required
                ///monitor.limitruleswitches = MAXINT32;
            }
            else
            {
                monitor.limitruleswitches = Convert.ToInt32(commonlib.MAX(MAX_RULESWITCH, lp.rows / MAX_RULESWITCH));
            }
            monitor.epsvalue = epsprimal; // lp->epsvalue;
            lp.monitor = monitor;
            stallMonitor_reset(lp);
            suminfeas = lp.infinite;
            return (true);
        }

        internal static bool stallMonitor_check(lprec lp, int rownr, int colnr, int lastnr, bool minit, bool approved, ref bool forceoutEQ)
        {
            OBJmonrec monitor = lp.monitor;
            //LpPricePSE LpPrice = new LpPricePSE();
            LpCls objLpCls = new LpCls();
            bool isStalled = new bool();
            bool isCreeping = new bool();
            bool acceptance = true;
            string msg = "";
            int altrule,

            ///#if Paranoia
            msglevel = lprec.NORMAL;
            ///#else
            msglevel = lprec.DETAILED;
            ///#endif
            double deltaobj = lp.suminfeas;

            /* Accept unconditionally if this is the first or second call */
            monitor.active = 0;
            if (monitor.Icount <= 1)
            {
                if (monitor.Icount == 1)
                {
                    monitor.prevobj = lp.rhs;
                    monitor.previnfeas = deltaobj;
                }
                monitor.Icount++;
                return (acceptance);
            }

            /* Define progress as primal objective less sum of (primal/dual) infeasibilities */
            monitor.thisobj = lp.rhs;
            monitor.thisinfeas = deltaobj;

            ///ORIGINAL CODE: if (lp.spx_trace && (lastnr > 0))
            if ((lp.spx_trace != 0) && (lastnr > 0))
            {
                msg = "%s: Objective at iter %10.0f is " + lp_types.RESULTVALUEMASK + " (%4d: %4d %s- %4d)\n ";
                //NOTED ISSUE:
                lp.report(lp, msg, monitor.spxfunc, (double)lp.get_total_iter(lp), monitor.thisobj, rownr, lastnr, lp_types.my_if(minit == Convert.ToBoolean(lprec.ITERATE_MAJORMAJOR), "<", "|"), colnr);
            }
            monitor.pivrule = lp.get_piv_rule(lp);

            /* Check if we have a stationary solution at selected tolerance level;
               allow some difference in case we just refactorized the basis. */
            deltaobj = lp_types.my_reldiff(monitor.thisobj, monitor.prevobj);
            deltaobj = System.Math.Abs(deltaobj); // Pre v5.2 version
            isStalled = (bool)(deltaobj < monitor.epsvalue);
            double testvalue = new double();

            /* Also require that we have a measure of infeasibility-stalling */
            if (isStalled)
            {

                double refvalue = monitor.epsvalue;

                ///#if 1
                if (monitor.isdual)
                {
                    refvalue *= 1000 * System.Math.Log10(9.0 + lp.rows);
                }
                else
                {
                    refvalue *= 1000 * System.Math.Log10(9.0 + lp.columns);
                }
                ///#else
                refvalue *= 1000 * System.Math.Log10(9.0 + lp.sum);
                ///#endif
                testvalue = lp_types.my_reldiff(monitor.thisinfeas, monitor.previnfeas);
                isStalled &= (System.Math.Abs(testvalue) < refvalue);

                /* Check if we should force "major" pivoting, i.e. no bound flips;
      this is activated when we see the feasibility deteriorate */
                /*    if(!isStalled && (testvalue > 0) && (TRUE || is_action(lp->anti_degen, ANTIDEGEN_BOUNDFLIP))) */
                ///#if ! _PRICE_NOBOUNDFLIP
                if (!isStalled && (testvalue > 0) && lp.is_action(lp.anti_degen, lp_lib.ANTIDEGEN_BOUNDFLIP))
                {
                    ///TODO: Cannot convert int value to bool.
                    acceptance = DefineConstants.AUTOMATIC;
                }
            }
            ///#else
            #region Need to check
            ///Below Code is commented in C version till #endif need to check while implementing
            if (!isStalled && (testvalue > 0) && !ISMASKSET(lp.piv_strategy, PRICE_NOBOUNDFLIP))
            {
                SETMASK(lp.piv_strategy, PRICE_NOBOUNDFLIP);
                acceptance = DefineConstants.AUTOMATIC;
            }
            else
            {
                CLEARMASK(lp.piv_strategy, PRICE_NOBOUNDFLIP);
                ///#endif
                ///#if 1
                isCreeping = false;
                ///#else
                isCreeping |= stallMonitor_creepingObj(lp);
                /*  isCreeping |= stallMonitor_shortSteps(lp); */
                ///#endif
            }
            #endregion

            if (isStalled || isCreeping)
            {

                /* Update counters along with specific tolerance for bound flips */
                ///#if 1
                if (minit != lp_lib.ITERATE_MAJORMAJOR)
                {
                    if (++monitor.Mcycle > 2)
                    {
                        monitor.Mcycle = 0;
                        monitor.Ncycle++;
                    }
                }
                else
                {
                    ///#endif
                    monitor.Ncycle++;
                }

                /* Start to monitor for variable cycling if this is the initial stationarity */
                if (monitor.Ncycle <= 1)
                {
                    monitor.Ccycle = colnr;
                    monitor.Rcycle = rownr;
                }
            }
            /* Check if we should change pivoting strategy */
            ///ORIGINAL CODE: else if (isCreeping || (monitor.Ncycle > monitor.limitstall[monitor.isdual]) || ((monitor.Ccycle == rownr) && (monitor.Rcycle == colnr)))
            else if (isCreeping || (monitor.Ncycle > monitor.limitstall[Convert.ToInt32(monitor.isdual)]) || ((monitor.Ccycle == rownr) && (monitor.Rcycle == colnr)))
            { // Obvious cycling

                monitor.active = 1;

                /* Try to force out equality slacks to combat degeneracy */
                if ((lp.fixedvars > 0) && (forceoutEQ != true))
                {
                    forceoutEQ = true;
                    goto Proceed;
                }

                /* Our options are now to select an alternative rule or to do bound perturbation;
                   check if these options are available to us or if we must signal failure and break out. */
                approved &= monitor.pivdynamic && (monitor.ruleswitches < monitor.limitruleswitches);
                if (!approved && !lp.is_anti_degen(lp, lprec.ANTIDEGEN_STALLING))
                {
                    lp.spx_status = lp_lib.DEGENERATE;
                    msg = "%s: Stalling at iter %10.0f; no alternative strategy left.\n";
                    lp.report(lp, msglevel, ref msg, monitor.spxfunc, (double)objLpCls.get_total_iter(lp));
                    acceptance = false;
                    return (acceptance);
                }

                /* See if we can do the appropriate alternative rule. */
                switch (monitor.oldpivrule)
                {
                    case lp_lib.PRICER_FIRSTINDEX:
                        altrule = PRICER_DEVEX;
                        break;
                    case lp_lib.PRICER_DANTZIG:
                        altrule = PRICER_DEVEX;
                        break;
                    case PRICER_DEVEX:
                        altrule = lp_lib.PRICER_STEEPESTEDGE;
                        break;
                    case lp_lib.PRICER_STEEPESTEDGE:
                        altrule = PRICER_DEVEX;
                        break;
                    default:
                        altrule = lp_lib.PRICER_FIRSTINDEX;
                        break;
                }
                if (approved && (monitor.pivrule != altrule) && (monitor.pivrule == monitor.oldpivrule))
                {

                    /* Switch rule to combat degeneracy. */
                    monitor.ruleswitches++;
                    lp.piv_strategy = altrule;
                    monitor.Ccycle = 0;
                    monitor.Rcycle = 0;
                    monitor.Ncycle = 0;
                    monitor.Mcycle = 0;

                    msg = "%s: Stalling at iter %10.0f; changed to '%s' rule.\n";
                    lp.report(lp, msglevel, ref msg, monitor.spxfunc, (double)lp.get_total_iter(lp), lp.get_str_piv_rule(lp.get_piv_rule(lp)));
                    if ((altrule == PRICER_DEVEX) || (altrule == lp_lib.PRICER_STEEPESTEDGE))
                    {
                        //NOTED ISSUE:
                        LpPricePSE.restartPricer(lp, DefineConstants.AUTOMATIC);
                    }
                }

                /* If not, code for bound relaxation/perturbation */
                else
                {
                    msg = "%s: Stalling at iter %10.0f; proceed to bound relaxation.\n";
                    lp.report(lp, msglevel, ref msg, monitor.spxfunc, (double)objLpCls.get_total_iter(lp));
                    acceptance = false;
                    lp.spx_status = lp_lib.DEGENERATE;
                    return (acceptance);
                }
            }

            /* Otherwise change back to original selection strategy as soon as possible */
            else
            {
                if (monitor.pivrule != monitor.oldpivrule)
                {
                    lp.piv_strategy = monitor.oldpivstrategy;
                    altrule = monitor.oldpivrule;
                    if ((altrule == PRICER_DEVEX) || (altrule == lp_lib.PRICER_STEEPESTEDGE))
                    {
                        //NOTED ISSUE:
                        LpPricePSE.restartPricer(lp, DefineConstants.AUTOMATIC);
                    }
                    msg = "...returned to original pivot selection rule at iter %.0f.\n";
                    lp.report(lp, msglevel, ref msg, (double)objLpCls.get_total_iter(lp));
                }

                stallMonitor_update(lp, monitor.thisobj);
                monitor.Ccycle = 0;
                monitor.Rcycle = 0;
                monitor.Ncycle = 0;
                monitor.Mcycle = 0;
            }

            /* Update objective progress tracker */
            Proceed:
            monitor.Icount++;
            if (deltaobj >= monitor.epsvalue)
            {
                monitor.prevobj = monitor.thisobj;
            }
            monitor.previnfeas = monitor.thisinfeas;

            return (acceptance);

        }

        internal static void stallMonitor_finish(lprec lp)
        {
            OBJmonrec monitor = lp.monitor;
            if (monitor == null)
            {
                return;
            }
            if (lp.piv_strategy != monitor.oldpivstrategy)
            {
                lp.piv_strategy = monitor.oldpivstrategy;
            }

            ///NOT REQUIRED
            ///FREE(monitor);
            lp.monitor = null;
        }

        internal static bool add_artificial(lprec lp, int forrownr, ref double?[] nzarray, ref int?[] idxarray)
        {
            /* This routine is called for each constraint at the start of
           primloop and the primal problem is infeasible. Its
           purpose is to add artificial variables and associated
           objective function values to populate primal phase 1. */
            bool add;
            LpCls objLpCls = new LpCls();

            /* Make sure we don't add unnecessary artificials, i.e. avoid
               cases where the slack variable is enough */
            add = !LpCls.isBasisVarFeasible(lp, lp.epspivot, forrownr);

            if (add)
            {
                //ORIGINAL LINE: int *rownr = null, i, bvar, ii;
                int?[] rownr = null;
                int i;
                int bvar;
                int ii;

                //ORIGINAL LINE: double *avalue = null, rhscoef, acoef;
                double?[] avalue = null;
                double rhscoef;
                double acoef;
                MATrec mat = lp.matA;
                string msg;

                /* Check the simple case where a slack is basic */
                for (i = 1; i <= lp.rows; i++)
                {
                    if (lp.var_basic[i] == forrownr)
                    {
                        break;
                    }
                }
                acoef = 1;

                /* If not, look for any basic user variable that has a
                   non-zero coefficient in the current constraint row */
                if (i > lp.rows)
                {
                    for (i = 1; i <= lp.rows; i++)
                    {
                        ii = lp.var_basic[i] - lp.rows;
                        if ((ii <= 0) || (ii > (lp.columns - lp.P1extraDim)))
                        {
                            continue;
                        }
                        ii = lp_matrix.mat_findelm(mat, forrownr, ii);
                        if (ii >= 0)
                        {
                            acoef = lp_matrix.COL_MAT_VALUE(ii);
                            break;
                        }
                    }
                }

                /* If no candidate was found above, gamble on using the densest column available */
                ///#if false
                //    if(i > lp->rows) {
                //      int len = 0;
                //      bvar = 0;
                //      for(i = 1; i <= lp->rows; i++) {
                //        ii = lp->var_basic[i] - lp->rows;
                //        if((ii <= 0) || (ii > (lp->columns-lp->P1extraDim)))
                //          continue;
                //        if(mat_collength(mat, ii) > len) {
                //          len = mat_collength(mat, ii);
                //          bvar = i;
                //        }
                //      }
                //      i = bvar;
                //      acoef = 1;
                //    }
                ///#endif

                bvar = i;

                add = (bool)(bvar <= lp.rows);
                if (add)
                {
                    rhscoef = lp.rhs[forrownr];

                    /* Create temporary sparse array storage */
                    if (nzarray == null)
                    {
                        //NOT REQUIRED
                        //allocREAL(lp, avalue, 2, 0);
                    }
                    else
                    {
                        avalue = nzarray;
                    }
                    if (idxarray == null)
                    {
                        //NOT REQUIRED
                        //allocINT(lp, rownr, 2, 0);
                    }
                    else
                    {
                        rownr = idxarray;
                    }

                    /* Set the objective coefficient */
                    rownr[0] = 0;
                    avalue[0] = lp_types.my_chsign(objLpCls.is_chsign(lp, 0), 1);

                    /* Set the constraint row coefficient */
                    rownr[1] = forrownr;
                    avalue[1] = lp_types.my_chsign(objLpCls.is_chsign(lp, forrownr), lp_types.my_sign(rhscoef / acoef));

                    /* Add the column of artificial variable data to the user data matrix */
                    objLpCls.add_columnex(lp, 2, ref avalue, ref rownr);

                    /* Free the temporary sparse array storage */
                    if (idxarray == null)
                    {
                        //NOT REQUIRED
                        //FREE(rownr);
                    }
                    if (nzarray == null)
                    {
                        //NOT REQUIRED
                        //FREE(avalue);
                    }

                    /* Now set the artificial variable to be basic */
                    objLpCls.set_basisvar(lp, bvar, lp.sum);
                    lp.P1extraDim++;
                }
                else
                {
                    msg = "add_artificial: Could not find replacement basis variable for row %d\n";
                    lp.report(lp, lp_lib.CRITICAL, ref msg, forrownr);
                    lp.basis_valid = 0;
                }

            }

            return (add);

        }

        internal static int get_artificialRow(lprec lp, int colnr)
        {
            MATrec mat = lp.matA;
            string msg;

            ///#if Paranoia
            if ((colnr <= lp.columns - System.Math.Abs(lp.P1extraDim)) || (colnr > lp.columns))
            {
                msg = "get_artificialRow: Invalid column index %d\n";
                lp.report(lp, lp_lib.SEVERE, ref msg, colnr);
            }
            if (mat.col_end[colnr] - mat.col_end[colnr - 1] != 1)
            {
                msg = "get_artificialRow: Invalid column non-zero count\n";
                lp.report(lp, lp_lib.SEVERE, ref msg);
            }
            ///#endif

            /* Return the row index of the singleton */
            colnr = mat.col_end[colnr - 1];
            colnr = lp_matrix.COL_MAT_ROWNR(colnr);
            return (colnr);
        }

        internal static int findAnti_artificial(lprec lp, int colnr)
        {
            /* Primal simplex: Find a basic artificial variable to swap
           against the non-basic slack variable, if possible */
            int i;
            int k;
            int rownr = 0;
            int P1extraDim = System.Math.Abs(lp.P1extraDim);

            //NOTED ISSUE
            if ((P1extraDim == 0) || (colnr > lp.rows) || !lp.is_basic[colnr])
            {
                return (rownr);
            }

            for (i = 1; i <= lp.rows; i++)
            {
                k = lp.var_basic[i];
                if ((k > lp.sum - P1extraDim) && (lp.rhs[i] == 0))
                {
                    rownr = get_artificialRow(lp, k - lp.rows);

                    /* Should we find the artificial's slack direct "antibody"? */
                    if (rownr == colnr)
                    {
                        break;
                    }
                    rownr = 0;
                }
            }
            return (rownr);
        }

        internal static int findBasicArtificial(lprec lp, int before)
        {
            int i = 0;
            int P1extraDim = System.Math.Abs(lp.P1extraDim);

            if (P1extraDim > 0)
            {
                if (before > lp.rows || before <= 1)
                {
                    i = lp.rows;
                }
                else
                {
                    i = before;
                }

                while ((i > 0) && (lp.var_basic[i] <= lp.sum - P1extraDim))
                {
                    i--;
                }
            }

            return (i);
        }

        internal static void eliminate_artificials(lprec lp, ref double prow)
        {
            int i;
            int j;
            int colnr;
            int rownr;
            int P1extraDim = System.Math.Abs(lp.P1extraDim);
            LpCls objLpCls = new LpCls();

            for (i = 1; (i <= lp.rows) && (P1extraDim > 0); i++)
            {
                j = lp.var_basic[i];
                if (j <= lp.sum - P1extraDim)
                {
                    continue;
                }
                j -= lp.rows;
                rownr = get_artificialRow(lp, j);
                colnr = find_rowReplacement(lp, rownr, prow, null);

                ///#if false
                //    performiteration(lp, rownr, colnr, 0.0, TRUE, FALSE, prow, NULL,
                //                                                          NULL, NULL, NULL);
                ///#else
                objLpCls.set_basisvar(lp, rownr, colnr);
                ///#endif
                del_column(lp, j);
                P1extraDim--;
            }
            lp.P1extraDim = 0;
        }

        public static int primloop(lprec lp, byte primalfeasible, double primaloffset)
        {
            throw new NotImplementedException();
        }
        public static int dualloop(lprec lp, byte dualfeasible, int[] dualinfeasibles, double dualoffset)
        {
            throw new NotImplementedException();
        }
        public static int spx_run(lprec lp, byte validInvB)
        {
            throw new NotImplementedException();
        }
        public static int spx_solve(lprec lp)
        {
            throw new NotImplementedException();
        }
        public static int lag_solve(lprec lp, double start_bound, int num_iter)
        {
            throw new NotImplementedException();
        }
        public static int heuristics(lprec lp, int mode)
        {
            throw new NotImplementedException();
        }
        public static int lin_solve(lprec lp)
        {
            throw new NotImplementedException();
        }
    }
}
