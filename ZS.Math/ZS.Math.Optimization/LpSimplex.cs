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
            if (!(lp.spx_trace) && (lastnr > 0))
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
                    acceptance = Convert.ToBoolean(DefineConstants.AUTOMATIC);
                }
            }
            ///#else
            #region Need to check
            ///Below Code is commented in C version till #endif need to check while implementing
            if (!isStalled && (testvalue > 0) && !objLpCls.ISMASKSET(lp.piv_strategy, lp_lib.PRICE_NOBOUNDFLIP))
            {
                objLpCls.SETMASK(lp.piv_strategy, lp_lib.PRICE_NOBOUNDFLIP);
                acceptance = Convert.ToBoolean(DefineConstants.AUTOMATIC);
            }
            else
            {
                objLpCls.CLEARMASK(lp.piv_strategy, lp_lib.PRICE_NOBOUNDFLIP);
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
                    lp.report(lp, msglevel, ref msg, monitor.spxfunc, (double)LpCls.get_total_iter(lp));
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
                        LpPricePSE.restartPricer(lp, Convert.ToBoolean(DefineConstants.AUTOMATIC));
                    }
                }

                /* If not, code for bound relaxation/perturbation */
                else
                {
                    msg = "%s: Stalling at iter %10.0f; proceed to bound relaxation.\n";
                    lp.report(lp, msglevel, ref msg, monitor.spxfunc, (double)LpCls.get_total_iter(lp));
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
                        LpPricePSE.restartPricer(lp, Convert.ToBoolean(DefineConstants.AUTOMATIC));
                    }
                    msg = "...returned to original pivot selection rule at iter %.0f.\n";
                    lp.report(lp, msglevel, ref msg, (double)LpCls.get_total_iter(lp));
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
                    rhscoef = lp.rhs;

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
                if ((k > lp.sum - P1extraDim) && (lp.rhs == 0))
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

        internal static void eliminate_artificials(lprec lp, ref double[] prow)
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
                int[] Para = null;
                colnr = LpPrice.find_rowReplacement(lp, rownr, ref prow, ref Para);

                ///#if false
                //    performiteration(lp, rownr, colnr, 0.0, TRUE, FALSE, prow, NULL,
                //                                                          NULL, NULL, NULL);
                ///#else
                objLpCls.set_basisvar(lp, rownr, colnr);
                ///#endif
                objLpCls.del_column(lp, j);
                P1extraDim--;
            }
            lp.P1extraDim = 0;
        }

        internal static void clear_artificials(lprec lp)
        {
            int i;
            int j;
            int n;
            int P1extraDim;
            string msg;
            LpCls objLpCls = new LpCls();

            /* Substitute any basic artificial variable for its slack counterpart */
            n = 0;
            P1extraDim = System.Math.Abs(lp.P1extraDim);
            for (i = 1; (i <= lp.rows) && (n < P1extraDim); i++)
            {
                j = lp.var_basic[i];
                if (j <= lp.sum - P1extraDim)
                {
                    continue;
                }
                j = get_artificialRow(lp, j - lp.rows);
                objLpCls.set_basisvar(lp, i, j);
                n++;
            }

            ///#if Paranoia
            if (n != lp.P1extraDim)
            {
                msg = "clear_artificials: Unable to clear all basic artificial variables\n";
                lp.report(lp, lp_lib.SEVERE, ref msg);
            }
            ///#endif

            /* Delete any remaining non-basic artificial variables */
            while (P1extraDim > 0)
            {
                i = lp.sum - lp.rows;
                objLpCls.del_column(lp, i);
                P1extraDim--;
            }
            lp.P1extraDim = 0;
            if (n > 0)
            {
                objLpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
                lp.basis_valid = 1;
            }
        }

        public static int primloop(lprec lp, bool primalfeasible, double primaloffset)
        {
            bool primal = true;
            bool bfpfinal = false;
            bool changedphase = false;
            bool forceoutEQ = Convert.ToBoolean(DefineConstants.AUTOMATIC);
            bool primalphase1 = new bool();
            bool pricerCanChange = new bool();
            bool minit = new bool();
            bool stallaccept = new bool();
            bool pendingunbounded = new bool();
            int i;
            int j;
            int k;
            int colnr = 0;
            int rownr = 0;
            int lastnr = 0;
            int candidatecount = 0;
            int minitcount = 0;
            bool ok = true;
            double theta = 0.0;
            double epsvalue = new double();
            double xviolated = 0.0;
            double cviolated = 0.0;
            double? prow = null;
            double?[] pcol = null;
            double[] drow = lp.drow;
            string msg;

            LpCls objLpCls = new LpCls();

            //ORIGINAL LINE: int *workINT = null, *nzdrow = lp->nzdrow;
            int? workINT = null;

            //ORIGINAL LINE: int *nzdrow = lp->nzdrow;
            int[] nzdrow = lp.nzdrow;

            if (lp.spx_trace)
            {
                msg = "Entered primal simplex algorithm with feasibility %s\n";
                //ORIGINAL CODE: lp.report(lp, lp_lib.DETAILED,ref msg, commonlib.my_boolstr(primalfeasible));
                lp.report(lp, lp_lib.DETAILED, ref msg, commonlib.my_boolstr(primalfeasible));
            }
            /* Add sufficent number of artificial variables to make the problem feasible
            through the first phase; delete when primal feasibility has been achieved */
            lp.P1extraDim = 0;
            if (!primalfeasible)
            {
                lp.simplex_mode = lp_lib.SIMPLEX_Phase1_PRIMAL;

                ///#if Paranoia
                if (!LpCls.verify_basis(lp))
                {
                    msg = "primloop: No valid basis for artificial variables\n";
                    lp.report(lp, lp_lib.SEVERE, ref msg);
                }
                ///#endif
                ///#if false
                // /* First check if we can get away with a single artificial variable */
                //    if(lp->equalities == 0) {
                //      i = (int) feasibilityOffset(lp, !primal);
                //      add_artificial(lp, i, prow, (int *) pcol);
                //    }
                //    else
                ///#endif
                /* Otherwise add as many artificial variables as is necessary
                   to force primal feasibility. */
                for (i = 1; i <= lp.rows; i++)
                {
                    //ORIGINAL LINE: add_artificial(lp, i, null, null);
                    double?[] para1 = null;
                    int?[] para2 = null;
                    add_artificial(lp, i, ref para1, ref para2);
                }

                /* Make sure we update the working objective */
                if (lp.P1extraDim > 0)
                {
                    ///#if 1
                    /*  variables(stored at the end of the column list, they are initially
                        basic and are never allowed to enter the basis, once they exit) */

                    //NOT REQUIRED
                    //ok = allocREAL(lp, (lp.drow), lp.sum + 1, AUTOMATIC) && allocINT(lp, (lp.nzdrow), lp.sum + 1, AUTOMATIC);
                    if (!ok)
                    {
                        goto Finish;
                    }
                    lp.nzdrow = null;
                    drow = lp.drow;
                    nzdrow = lp.nzdrow;
                    ///#endif
                    lp_matrix.mat_validate(lp.matA);
                    LpCls.set_OF_p1extra(lp, 0.0);
                }
                if (lp.spx_trace)
                {
                    msg = "P1extraDim count = %d\n";
                    lp.report(lp, lp_lib.DETAILED, ref msg, lp.P1extraDim);
                }

                LpPricePSE.simplexPricer(lp, (bool)!primal);
                lp_matrix.invert(lp, lp_lib.INITSOL_USEZERO, true);
            }
            else
            {
                lp.simplex_mode = lp_lib.SIMPLEX_Phase2_PRIMAL;
                LpPricePSE.restartPricer(lp, (bool)!primal);
            }
            /* Create work arrays and optionally the multiple pricing structure */
            //NOT REQUIRED
            //ok = allocREAL(lp, (lp.bsolveVal), lp.rows + 1, 0) && allocREAL(lp, prow, lp.sum + 1, 1) && allocREAL(lp, pcol, lp.rows + 1, 1);
            if (objLpCls.is_piv_mode(lp, lp_lib.PRICE_MULTIPLE) && (lp.multiblockdiv > 1))
            {
                lp.multivars = LpPrice.multi_create(lp, false);
                ok &= (lp.multivars != null) && LpPrice.multi_resize(lp.multivars, lp.sum / lp.multiblockdiv, 2, false, true);
            }
            if (!ok)
            {
                goto Finish;
            }

            /* Initialize regular primal simplex algorithm variables */
            lp.spx_status = lp_lib.RUNNING;
            minit = lp_lib.ITERATE_MAJORMAJOR;
            epsvalue = lp.epspivot;
            pendingunbounded = false;
            msg = "primloop";
            ok = stallMonitor_create(lp, false, ref msg);
            if (!ok)
            {
                goto Finish;
            }

            lp.rejectpivot[0] = 0;

            /* Iterate while we are successful; exit when the model is infeasible/unbounded,
               or we must terminate due to numeric instability or user-determined reasons */
            while ((lp.spx_status == lp_lib.RUNNING) && !objLpCls.userabort(lp, -1))
            {

                primalphase1 = (bool)(lp.P1extraDim > 0);
                LpCls.clear_action(ref lp.spx_action, lp_lib.ACTION_REINVERT | lp_lib.ACTION_ITERATE);

                /* Check if we have stalling (from numerics or degenerate cycling) */
                pricerCanChange = !primalphase1;
                stallaccept = stallMonitor_check(lp, rownr, colnr, lastnr, minit, pricerCanChange, ref forceoutEQ);
                if (!stallaccept)
                {
                    break;
                }

                /* Find best column to enter the basis */
                RetryCol:
                ///#if false
                //    if(verify_solution(lp, FALSE, "spx_loop") > 0)
                //      i = 1; // This is just a debug trap 
                ///#endif
                if (!changedphase)
                {
                    i = 0;
                    do
                    {
                        i++;
                        colnr = LpPrice.colprim(lp, ref drow, ref nzdrow, (bool)(minit == Convert.ToBoolean(lp_lib.ITERATE_MINORRETRY)), i, ref candidatecount, true, ref xviolated);
                    } while ((colnr == 0) && (i < LpPrice.partial_countBlocks(lp, (bool)!primal)) && LpPrice.partial_blockStep(lp, (bool)!primal));

                    /* Handle direct outcomes */
                    if (colnr == 0)
                    {
                        lp.spx_status = lp_lib.OPTIMAL;
                    }
                    if (lp.rejectpivot[0] > 0)
                    {
                        minit = lp_lib.ITERATE_MAJORMAJOR;
                    }

                    /* See if accuracy check during compute_reducedcosts flagged refactorization */
                    if (objLpCls.is_action(lp.spx_action, lp_lib.ACTION_REINVERT))
                    {
                        bfpfinal = true;
                    }

                }
                /* Make sure that we do not erroneously conclude that an unbounded model is optimal */
                ///#if primal_UseRejectionList
                if ((colnr == 0) && (lp.rejectpivot[0] > 0))
                {
                    lp.spx_status = lp_lib.UNBOUNDED;
                    if ((lp.spx_trace && (lp.bb_totalnodes == 0)) || (lp.bb_trace && (lp.bb_totalnodes > 0)))
                    {
                        msg = "The model is primal unbounded.\n";
                        lp.report(lp, lp_lib.DETAILED, ref msg);
                    }
                    colnr = lp.rejectpivot[1];
                    rownr = 0;
                    lp.rejectpivot[0] = 0;
                    ok = false;
                    break;
                }
                ///#endif

                /* Check if we found an entering variable (indicating that we are still dual infeasible) */
                if (colnr > 0)
                {
                    changedphase = false;
                    lp_matrix.fsolve(lp, colnr, pcol, null, lp.epsmachine, 1.0, true); // Solve entering column for Pi

                    /* Do special anti-degeneracy column selection, if specified */
                    int parameter = 0;
                    if (objLpCls.is_anti_degen(lp, lp_lib.ANTIDEGEN_COLUMNCHECK) && !LpCls.check_degeneracy(lp, ref pcol, ref parameter))
                    {
                        if (lp.rejectpivot[0] < lp_lib.DEF_MAXPIVOTRETRY / 3)
                        {
                            i = ++lp.rejectpivot[0];
                            lp.rejectpivot[i] = colnr;
                            msg = "Entering column %d found to be non-improving due to degeneracy.\n";
                            lp.report(lp, lp_lib.DETAILED, ref msg, colnr);
                            minit = Convert.ToBoolean(lp_lib.ITERATE_MINORRETRY);

                            goto RetryCol;
                        }
                        else
                        {
                            lp.rejectpivot[0] = 0;
                            msg = "Gave up trying to find a strictly improving entering column.\n";
                            lp.report(lp, lp_lib.DETAILED, ref msg);
                        }
                    }

                    /* Find the leaving variable that gives the most stringent bound on the entering variable */
                    theta = drow[0];
                    rownr = LpPrice.rowprim(lp, colnr, ref theta, ref pcol, ref workINT, forceoutEQ, ref cviolated);

                    ///#if AcceptMarginalAccuracy
                    /* Check for marginal accuracy */
                    if ((rownr > 0) && (xviolated + cviolated < lp.epspivot))
                    {
                        if (lp.bb_trace || (lp.bb_totalnodes == 0))
                        {
                            msg = "primloop: Assuming convergence with reduced accuracy %g.\n";
                            lp.report(lp, lp_lib.DETAILED, ref msg, MAX(xviolated, cviolated));
                        }
                        rownr = 0;
                        colnr = 0;
                        //NOTED ISSUE
                        goto Optimality;
                    }
                    else
                    {
                        ///#endif

                        /* See if we can do a straight artificial<->slack replacement (when "colnr" is a slack) */
                        if ((lp.P1extraDim != 0) && (rownr == 0) && (colnr <= lp.rows))
                        {
                            rownr = findAnti_artificial(lp, colnr);
                        }
                    }

                    if (rownr > 0)
                    {
                        pendingunbounded = false;
                        lp.rejectpivot[0] = 0;
                        objLpCls.set_action(ref lp.spx_action, lp_lib.ACTION_ITERATE);
                        if (!lp.obj_in_basis) // We must manually copy the reduced cost for RHS update
                        {
                            pcol[0] = lp_types.my_chsign(!lp.is_lower[colnr], drow);
                        }
                        //NOTED ISSUE:
                        lp.bfp_prepareupdate(lp, rownr, colnr, ref pcol);
                    }
                    /* We may be unbounded... */
                    else
                    {
                        /* First make sure that we are not suffering from precision loss */

                        ///#if primal_UseRejectionList
                        if (lp.rejectpivot[0] < lp_lib.DEF_MAXPIVOTRETRY)
                        {
                            lp.spx_status = lp_lib.RUNNING;
                            lp.rejectpivot[0]++;
                            lp.rejectpivot[lp.rejectpivot[0]] = colnr;
                            msg = "...trying to recover via another pivot column.\n";
                            lp.report(lp, lp_lib.DETAILED, ref msg);
                            minit = Convert.ToBoolean(lp_lib.ITERATE_MINORRETRY);
                            goto RetryCol;
                        }
                        else
                        {
                            ///#endif
                            /* Check that we are not having numerical problems */
                            if (!LpCls.refactRecent(lp) && !pendingunbounded)
                            {
                                bfpfinal = true;
                                pendingunbounded = true;
                                objLpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
                            }

                            /* Conclude that the model is unbounded */
                            else
                            {
                                lp.spx_status = lp_lib.UNBOUNDED;
                                msg = "The model is primal unbounded.\n";
                                lp.report(lp, lp_lib.DETAILED, ref msg);
                                break;
                            }
                        }
                    }
                }

                /* We handle optimality and phase 1 infeasibility ... */
                else
                {
                    Optimality:
                    /* Handle possible transition from phase 1 to phase 2 */
                    if (!primalfeasible || isP1extra(lp))
                    {

                        if (feasiblePhase1(lp, epsvalue))
                        {
                            lp.spx_status = lp_lib.RUNNING;
                            if (lp.bb_totalnodes == 0)
                            {
                                msg = "Found feasibility by primal simplex after  %10.0f iter.\n";
                                lp.report(lp, lp_lib.NORMAL, ref msg, (double)LpCls.get_total_iter(lp));
                                if ((lp.usermessage != null) && (lp.msgmask != 0 & lp_lib.MSG_LPFEASIBLE != 0))
                                {
                                    lp.usermessage(lp, lp.msghandle, lp_lib.MSG_LPFEASIBLE);
                                }
                            }
                            changedphase = false;
                            primalfeasible = true;
                            lp.simplex_mode = lp_lib.SIMPLEX_Phase2_PRIMAL;
                            LpCls.set_OF_p1extra(lp, 0.0);

                            /* We can do two things now;
                               1) delete the rows belonging to those variables, since they are redundant, OR
                               2) drive out the existing artificial variables via pivoting. */
                            if (lp.P1extraDim > 0)
                            {

                                //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                                ///#if Phase1EliminateRedundant
                                /* If it is not a MIP model we can try to delete redundant rows */
                                if ((lp.bb_totalnodes == 0) && (MIP_count(lp) == 0))
                                {
                                    while (lp.P1extraDim > 0)
                                    {
                                        i = lp.rows;
                                        while ((i > 0) && (lp.var_basic[i] <= lp.sum - lp.P1extraDim))
                                        {
                                            i--;
                                        }

                                        ///#if Paranoia
                                        if (i <= 0)
                                        {
                                            msg = "primloop: Could not find redundant artificial.\n";
                                            lp.report(lp, lp_lib.SEVERE, ref msg);
                                            break;
                                        }
                                        ///#endif
                                        /* Obtain column and row indeces */
                                        j = lp.var_basic[i] - lp.rows;
                                        k = get_artificialRow(lp, j);

                                        /* Delete row before column due to basis "compensation logic" */
                                        if (lp.is_basic[k])
                                        {
                                            lp.is_basic[lp.rows + j] = false;
                                            del_constraint(lp, k);
                                        }
                                        else
                                        {
                                            objLpCls.set_basisvar(lp, i, k);
                                        }
                                        objLpCls.del_column(lp, j);
                                        lp.P1extraDim--;
                                    }
                                    lp.basis_valid = 1;
                                }
                                /* Otherwise we drive out the artificials by elimination pivoting */
                                else
                                {
                                    eliminate_artificials(lp, ref prow);
                                }

                                ///#else
                                /* Indicate phase 2 with artificial variables by negating P1extraDim */
                                lp.P1extraDim = Convert.ToInt32(lp_types.my_flipsign(lp.P1extraDim));
                                ///#endif
                            }
                            /* We must refactorize since the OF changes from phase 1 to phase 2 */
                            objLpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
                            bfpfinal = true;
                        }

                        /* We are infeasible in phase 1 */
                        else
                        {
                            lp.spx_status = lp_lib.INFEASIBLE;
                            minit = lp_lib.ITERATE_MAJORMAJOR;
                            if (lp.spx_trace)
                            {
                                msg = "Model infeasible by primal simplex at iter   %10.0f.\n";
                                lp.report(lp, lp_lib.NORMAL, ref msg, (double)LpCls.get_total_iter(lp));
                            }
                        }
                    }

                    /* Handle phase 1 optimality */
                    else
                    {
                        /* (Do nothing special) */
                    }

                    /* Check if we are still primal feasible; the default assumes that this check
                       is not necessary after the relaxed problem has been solved satisfactorily. */
                    if ((lp.bb_level <= 1) || (lp.improve != 0 & lp_lib.IMPROVE_BBSIMPLEX != 0))
                    { // NODE_RCOSTFIXING fix
                        objLpCls.set_action(ref lp.piv_strategy, lp_lib.PRICE_FORCEFULL);
                        i = rowdual(lp, lp.rhs, 0, 0, null);
                        LpCls.clear_action(ref lp.piv_strategy, lp_lib.PRICE_FORCEFULL);
                        if (i > 0)
                        {
                            lp.spx_status = lp_lib.LOSTFEAS;
                            if (lp.total_iter == 0)
                            {
                                msg = "primloop: Lost primal feasibility at iter  %10.0f: will try to recover.\n";
                                lp.report(lp, lp_lib.DETAILED, ref msg, (double)LpCls.get_total_iter(lp));
                            }
                        }
                    }
                }

                /* Pivot row/col and update the inverse */
                if (objLpCls.is_action(lp.spx_action, lp_lib.ACTION_ITERATE))
                {
                    lastnr = lp.var_basic[rownr];

                    if (LpCls.refactRecent(lp) == Convert.ToBoolean(DefineConstants.AUTOMATIC))
                    {
                        minitcount = 0;
                    }
                    else if (minitcount > lp_lib.MAX_MINITUPDATES)
                    {
                        recompute_solution(lp, lp_lib.INITSOL_USEZERO);
                        minitcount = 0;
                    }
                    minit = performiteration(lp, rownr, colnr, theta, primal, (bool)((stallaccept != Convert.ToBoolean(DefineConstants.AUTOMATIC))), null, null, pcol, null, null);
                    if (minit != lp_lib.ITERATE_MAJORMAJOR)
                    {
                        minitcount++;
                    }

                    if ((lp.spx_status == lp_lib.USERABORT) || (lp.spx_status == lp_lib.TIMEOUT))
                    {
                        break;
                    }
                    else if (minit == Convert.ToBoolean(lp_lib.ITERATE_MINORMAJOR))
                    {
                        continue;
                    }

                    ///#if UsePrimalReducedCostUpdate
                    /* Do a fast update of the reduced costs in preparation for the next iteration */
                    if (minit == lp_lib.ITERATE_MAJORMAJOR)
                    {
                        update_reducedcosts(lp, primal, lastnr, colnr, pcol, drow);
                    }
                    ///#endif

                    /* Detect if an auxiliary variable has left the basis and delete it; if
                       the non-basic variable only changed bound (a "minor iteration"), the
                       basic artificial variable did not leave and there is nothing to do */
                    if ((minit == lp_lib.ITERATE_MAJORMAJOR) && (lastnr > lp.sum - System.Math.Abs(lp.P1extraDim)))
                    {
                        ///#if Paranoia
                        if (lp.is_basic[lastnr] || !lp.is_basic[colnr])
                        {
                            msg = "primloop: Invalid basis indicator for variable %d at iter %10.0f.\n";
                            lp.report(lp, lp_lib.SEVERE, ref msg, lastnr, (double)LpCls.get_total_iter(lp));
                        }
                        ///#endif
                        del_column(lp, lastnr - lp.rows);
                        if (lp.P1extraDim > 0)
                        {
                            lp.P1extraDim--;
                        }
                        else
                        {
                            lp.P1extraDim++;
                        }
                        if (lp.P1extraDim == 0)
                        {
                            colnr = 0;
                            changedphase = 1;
                            stallMonitor_reset(lp);
                        }
                    }
                }
                if (lp.spx_status == lp_lib.SWITCH_TO_DUAL)
                {
                    ;
                }
                else if (!changedphase && Convert.ToBoolean(lp.bfp_mustrefactorize(lp)))
                {
                    ///#if ResetMinitOnReinvert
                    minit = lp_lib.ITERATE_MAJORMAJOR;
                    ///#endif
                    if (!lp_matrix.invert(lp, lp_lib.INITSOL_USEZERO, bfpfinal))
                    {
                        lp.spx_status = lp_lib.SINGULAR_BASIS;
                    }
                    bfpfinal = false;
                }
            }

            /* Remove any remaining artificial variables (feasible or infeasible model) */
            lp.P1extraDim = System.Math.Abs(lp.P1extraDim);
            /*  if((lp->P1extraDim > 0) && (lp->spx_status != DEGENERATE)) { */
            if (lp.P1extraDim > 0)
            {
                clear_artificials(lp);
                if (lp.spx_status != lp_lib.OPTIMAL)
                {
                    restore_basis(lp);
                }
                i = Convert.ToInt32(lp_matrix.invert(lp, lp_lib.INITSOL_USEZERO, true));
            }

            ///#if Paranoia
            if (!verify_basis(lp))
            {
                msg = "primloop: Invalid basis detected due to internal error\n";
                lp.report(lp, lp_lib.SEVERE, ref msg);
            }
            ///#endif

            /* Switch to dual phase 1 simplex for MIP models during
               B&B phases, since this is typically far more efficient */

            ///#if ForceDualSimplexInBB
            if ((lp.bb_totalnodes == 0) && (MIP_count(lp) > 0) && ((lp.simplex_strategy & lp_lib.SIMPLEX_Phase1_DUAL) == 0))
            {
                lp.simplex_strategy &= ~lp_lib.SIMPLEX_Phase1_PRIMAL;
                lp.simplex_strategy += lp_lib.SIMPLEX_Phase1_DUAL;
            }
            ///#endif

            Finish:
            stallMonitor_finish(lp);
            multi_free((lp.multivars));

            //NOT REQUIRED
            //FREE(prow);
            //FREE(pcol);
            //FREE(lp.bsolveVal);

            return (Convert.ToInt32(ok));
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