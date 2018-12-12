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
                    monitor.prevobj = Convert.ToDouble(lp.rhs);
                    monitor.previnfeas = deltaobj;
                }
                monitor.Icount++;
                return (acceptance);
            }

            /* Define progress as primal objective less sum of (primal/dual) infeasibilities */
            monitor.thisobj = Convert.ToDouble(lp.rhs);
            monitor.thisinfeas = deltaobj;

            ///ORIGINAL CODE: if (lp.spx_trace && (lastnr > 0))
            if (!(lp.spx_trace) && (lastnr > 0))
            {
                msg = "%s: Objective at iter %10.0f is " + lp_types.RESULTVALUEMASK + " (%4d: %4d %s- %4d)\n ";
                //NOTED ISSUE:Temporary removed optional parameters, need to check at runtime.
                //ORIGINAL LINE: lp.report(lp, msg, monitor.spxfunc, (double)lp.get_total_iter(lp), monitor.thisobj, rownr, lastnr, lp_types.my_if(minit == Convert.ToBoolean(lprec.ITERATE_MAJORMAJOR), "<", "|"), colnr);
                lp.report(lp, msglevel, ref msg, monitor.spxfunc, (double)lp.get_total_iter(lp), monitor.thisobj, rownr, lastnr, colnr);
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
                        //FIX_36b0c67a-e607-4230-95dd-a228e7622a87 29/11/18
                        //lp_types.AUTOMATIC is byte and copmared with boolean type need to check at runtime.
                        LpPricePSE.restartPricer(lp, Convert.ToBoolean(lp_types.AUTOMATIC));
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
                        //FIX_36b0c67a-e607-4230-95dd-a228e7622a87 29/11/18
                        //lp_types.AUTOMATIC is byte and copmared with boolean type need to check at runtime.
                        LpPricePSE.restartPricer(lp, Convert.ToBoolean(lp_types.AUTOMATIC));
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

        internal static bool add_artificial(lprec lp, int forrownr, ref double[] nzarray, ref int?[] idxarray)
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
                double[] avalue = null;
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
                    rhscoef = Convert.ToDouble(lp.rhs);

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
                    LpCls.set_basisvar(lp, bvar, lp.sum);
                    lp.P1extraDim++;
                }
                else
                {
                    msg = "add_artificial: Could not find replacement basis variable for row %d\n";
                    lp.report(lp, lp_lib.CRITICAL, ref msg, forrownr);
                    lp.basis_valid = false;
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
            //Added first array 0 as first row, need to check at runtime
            if (mat.col_end[0][colnr] - mat.col_end[0][colnr - 1] != 1)
            {
                msg = "get_artificialRow: Invalid column non-zero count\n";
                lp.report(lp, lp_lib.SEVERE, ref msg);
            }
            ///#endif

            /* Return the row index of the singleton */
            //First row added as [0], need to check at runtime
            colnr = mat.col_end[0][colnr - 1];
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

        internal static void eliminate_artificials(lprec lp, ref double?[] prow)
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
                LpCls.set_basisvar(lp, rownr, colnr);
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
                LpCls.set_basisvar(lp, i, j);
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
                LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
                lp.basis_valid = true;
            }
        }

        //Changed By: CS Date:28/11/2018
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
            double?[] prow = null;
            double[] pcol = null;
            double[][] drow = lp.drow;
            string msg;

            LpCls objLpCls = new LpCls();

            //ORIGINAL LINE: int *workINT = null, *nzdrow = lp->nzdrow;
            int? workINT = null;

            //ORIGINAL LINE: int *nzdrow = lp->nzdrow;
            int[][] nzdrow = lp.nzdrow;

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
                    double[] para1 = null;
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
                lp.multivars[0] = (multirec)LpPrice.multi_create(lp, false);
                ok &= (lp.multivars != null) && LpPrice.multi_resize(lp.multivars[0], lp.sum / lp.multiblockdiv, 2, false, true);
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
            while ((lp.spx_status == lp_lib.RUNNING) && !LpCls.userabort(lp, -1))
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
                        colnr = LpPrice.colprim(lp, ref drow[0][0], ref nzdrow[0], (bool)(minit == Convert.ToBoolean(lp_lib.ITERATE_MINORRETRY)), i, ref candidatecount, true, ref xviolated);
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
                    if (LpCls.is_action(lp.spx_action, lp_lib.ACTION_REINVERT))
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
                    lp_matrix.fsolve(lp, colnr,  pcol, null, lp.epsmachine, 1.0, true); // Solve entering column for Pi

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
                    theta = drow[0][0];
                    rownr = LpPrice.rowprim(lp, colnr, ref theta, ref pcol, ref workINT, forceoutEQ, ref cviolated);

                    ///#if AcceptMarginalAccuracy
                    /* Check for marginal accuracy */
                    if ((rownr > 0) && (xviolated + cviolated < lp.epspivot))
                    {
                        if (lp.bb_trace || (lp.bb_totalnodes == 0))
                        {
                            msg = "primloop: Assuming convergence with reduced accuracy %g.\n";
                            lp.report(lp, lp_lib.DETAILED, ref msg, commonlib.MAX(xviolated, cviolated));
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
                        LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_ITERATE);
                        if (!lp.obj_in_basis) // We must manually copy the reduced cost for RHS update
                        {
                            //NOTED ISSUE:
                            pcol[0] = lp_types.my_chsign(!lp.is_lower[colnr], drow[0][0]);
                        }
                        //NOTED ISSUE:
                        lp.bfp_prepareupdate(lp, rownr, colnr, ref pcol[0]);
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
                                LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
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
                    if (!primalfeasible || LpCls.isP1extra(lp))
                    {

                        if (LpCls.feasiblePhase1(lp, epsvalue))
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
                                if ((lp.bb_totalnodes == 0) && (LpCls.MIP_count(lp) == 0))
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
                                            objLpCls.del_constraint(lp, k);
                                        }
                                        else
                                        {
                                            LpCls.set_basisvar(lp, i, k);
                                        }
                                        objLpCls.del_column(lp, j);
                                        lp.P1extraDim--;
                                    }
                                    lp.basis_valid = true;
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
                            LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
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
                        LpCls.set_action(ref lp.piv_strategy, lp_lib.PRICE_FORCEFULL);
                        double Parameter = 0;
                        i = LpPrice.rowdual(lp, lp.rhs, false, false, ref Parameter);
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
                if (LpCls.is_action(lp.spx_action, lp_lib.ACTION_ITERATE))
                {
                    lastnr = lp.var_basic[rownr];

                    if (LpCls.refactRecent(lp) == Convert.ToBoolean(DefineConstants.AUTOMATIC))
                    {
                        minitcount = 0;
                    }
                    else if (minitcount > lp_lib.MAX_MINITUPDATES)
                    {
                        LpCls.recompute_solution(lp, lp_lib.INITSOL_USEZERO);
                        minitcount = 0;
                    }
                    double[] Parameter1 = null;
                    int Parameter2 = 0;
                    minit = LpCls.performiteration(lp, rownr, colnr, theta, primal, (bool)((stallaccept != Convert.ToBoolean(DefineConstants.AUTOMATIC))), ref Parameter1, ref Parameter2, ref pcol, ref Parameter2, ref Parameter2);
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
                        LpPrice.update_reducedcosts(lp, primal, lastnr, colnr, pcol, drow[0][0]);
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
                        objLpCls.del_column(lp, lastnr - lp.rows);
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
                            changedphase = true;
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
                    LpCls.restore_basis(lp);
                }
                i = Convert.ToInt32(lp_matrix.invert(lp, lp_lib.INITSOL_USEZERO, true));
            }

            ///#if Paranoia
            if (!LpCls.verify_basis(lp))
            {
                msg = "primloop: Invalid basis detected due to internal error\n";
                lp.report(lp, lp_lib.SEVERE, ref msg);
            }
            ///#endif

            /* Switch to dual phase 1 simplex for MIP models during
               B&B phases, since this is typically far more efficient */

            ///#if ForceDualSimplexInBB
            if ((lp.bb_totalnodes == 0) && (LpCls.MIP_count(lp) > 0) && ((lp.simplex_strategy & lp_lib.SIMPLEX_Phase1_DUAL) == 0))
            {
                lp.simplex_strategy &= ~lp_lib.SIMPLEX_Phase1_PRIMAL;
                lp.simplex_strategy += lp_lib.SIMPLEX_Phase1_DUAL;
            }
            ///#endif

            Finish:
            stallMonitor_finish(lp);
            //NOT REQUIRED
            //multi_free((lp.multivars));

            //NOT REQUIRED
            //FREE(prow);
            //FREE(pcol);
            //FREE(lp.bsolveVal);

            return (Convert.ToInt32(ok));
        }

        public static int dualloop(lprec lp, bool dualfeasible, int[] dualinfeasibles, double dualoffset)
        {
            bool primal = false;
            bool inP1extra;
            bool dualphase1 = false;
            bool changedphase = true;
            bool pricerCanChange;
            bool minit;
            bool stallaccept;
            bool longsteps;
            bool forceoutEQ = false;
            bool bfpfinal = false;
            int i, colnr = 0, rownr = 0, lastnr = 0, candidatecount = 0, minitcount = 0;
#if FixInaccurateDualMinit
		 minitcolnr = 0,
#endif
            bool ok = true;
            //ORIGINAL LINE: int *boundswaps = null;
            int boundswaps = 0;
            double theta = 0.0;
            double epsvalue;
            double xviolated = 0;
            double cviolated = 0;
            //ORIGINAL LINE: double *prow = null;
            double[] prow = null;
            //ORIGINAL LINE: double *pcol = null;
            double[] pcol = null;
            //ORIGINAL LINE: double *drow = lp->drow;
            double[] drow = lp.drow[0];
            //ORIGINAL LINE: int *nzprow = null, *workINT = null, *nzdrow = lp->nzdrow;
            int[] nzprow = null;
            //ORIGINAL LINE: int *workINT = null;
            int[] workINT = null;
            //ORIGINAL LINE: int *nzdrow = lp->nzdrow;
            int[] nzdrow = lp.nzdrow[0];
            string msg;
            LpCls objLpCls = new LpCls();

            if (lp.spx_trace)
            {
                msg = "Entered dual simplex algorithm with feasibility %s.\n";
                lp.report(lp, lp_lib.DETAILED, ref msg, commonlib.my_boolstr(dualfeasible));
            }
            /* Allocate work arrays */
            //NOT REQUIRED
            // ok = allocREAL(lp, prow, lp.sum + 1, 1) && allocINT(lp, nzprow, lp.sum + 1, 0) && allocREAL(lp, pcol, lp.rows + 1, 1);
            if (!ok)
            {
                goto Finish;
            }

            /* Set non-zero P1extraVal value to force dual feasibility when the dual
               simplex is used as a phase 1 algorithm for the primal simplex.
               The value will be reset when primal feasibility has been achieved, or
               a dual non-feasibility has been encountered (no candidate for a first
               leaving variable) */
            inP1extra = (bool)(dualoffset != 0);
            if (inP1extra)
            {
                LpCls.set_OF_p1extra(lp, dualoffset);
                LpPricePSE.simplexPricer(lp, (bool)!primal);
                lp_matrix.invert(lp, lp_lib.INITSOL_USEZERO, true);
            }
            else
            {
                LpPricePSE.restartPricer(lp, (bool)!primal);
            }

            /* Prepare dual long-step structures */
#if false
//  longsteps = TRUE;
#elif false
//  longsteps = (bool) ((MIP_count(lp) > 0) && (lp->bb_level > 1));
#elif false
//  longsteps = (bool) ((MIP_count(lp) > 0) && (lp->solutioncount >= 1));
#else
            longsteps = false;
#endif
#if UseLongStepDualPhase1
  longsteps = !dualfeasible && (bool)(dualinfeasibles != null);
#endif

            if (longsteps)
            {
                lp.longsteps = LpPrice.multi_create(lp, true);
                ok = (lp.longsteps != null) && LpPrice.multi_resize(lp.longsteps, Convert.ToInt32(commonlib.MIN(lp.boundedvars + 2, 11)), 1, true, true);
                if (!ok)
                {
                    goto Finish;
                }
#if UseLongStepPruning
	lp.longsteps.objcheck = 1;
#endif
                boundswaps = Convert.ToInt32(LpPrice.multi_indexSet(lp.longsteps, false));
            }

            /* Do regular dual simplex variable initializations */
            lp.spx_status = lp_lib.RUNNING;
            minit = lp_lib.ITERATE_MAJORMAJOR;
            epsvalue = lp.epspivot;

            msg = "dualloop";
            ok = stallMonitor_create(lp, true, ref msg);
            if (!ok)
            {
                goto Finish;
            }

            lp.rejectpivot[0] = 0;
            if (dualfeasible)
            {
                lp.simplex_mode = lp_lib.SIMPLEX_Phase2_DUAL;
            }
            else
            {
                lp.simplex_mode = lp_lib.SIMPLEX_Phase1_DUAL;
            }

            /* Check if we have equality slacks in the basis and we should try to
               drive them out in order to reduce chance of degeneracy in Phase 1.
               forceoutEQ = FALSE :    Only eliminate assured "good" violated
                                       equality constraint slacks
                            AUTOMATIC: Seek more elimination of equality constraint
                                       slacks (but not as aggressive as the rule
                                       used in lp_solve v4.0 and earlier)
                            TRUE:      Force remaining equality slacks out of the
                                       basis */
            if (dualphase1 || inP1extra || ((lp.fixedvars > 0) && objLpCls.is_anti_degen(lp, lp_lib.ANTIDEGEN_FIXEDVARS)))
            {
                forceoutEQ = Convert.ToBoolean(lp_types.AUTOMATIC);
            }
#if true
            if (objLpCls.is_anti_degen(lp, lp_lib.ANTIDEGEN_DYNAMIC) && (objLpCls.bin_count(lp, 1) * 2 > lp.columns))
            {
                switch (forceoutEQ)
                {
                    case false:
                        forceoutEQ = Convert.ToBoolean(lp_types.AUTOMATIC);
                        break;
                        /*     case AUTOMATIC: forceoutEQ = TRUE;
                                             break;
                             default:        forceoutEQ = TRUE; */
                }
            }
#endif

            while ((lp.spx_status == lp_lib.RUNNING) && !LpCls.userabort(lp, -1))
            {

                /* Check if we have stalling (from numerics or degenerate cycling) */
                pricerCanChange = !dualphase1 && !inP1extra;
                stallaccept = stallMonitor_check(lp, rownr, colnr, lastnr, minit, pricerCanChange, ref forceoutEQ);
                if (!stallaccept)
                {
                    break;
                }

                /* Store current LP index for reference at next iteration */
                changedphase = false;

                /* Compute (pure) dual phase1 offsets / reduced costs if appropriate */
                dualphase1 &= (bool)(lp.simplex_mode == lp_lib.SIMPLEX_Phase1_DUAL);
                if (longsteps && dualphase1 && !inP1extra)
                {
                    int[] Parameter = null;
                    int Parameter1 = 0;
                    LpCls.obtain_column(lp, dualinfeasibles[1], ref pcol, ref Parameter, ref Parameter1);
                    i = 2;
                    for (i = 2; i <= dualinfeasibles[0]; i++)
                    {
                        lp_matrix.mat_multadd(lp.matA, pcol, dualinfeasibles[i], 1.0);
                    }
                    /* Solve (note that solved pcol will be used instead of lp->rhs) */
                    lp_matrix.ftran(lp, pcol, Parameter1, lp.epsmachine);
                }

                /* Do minor iterations (non-basic variable bound flips) for as
                   long as possible since this is a cheap way of iterating */
#if (dual_Phase1PriceEqualities) || (dual_UseRejectionList)
RetryRow:
#endif
                if (minit != Convert.ToBoolean(lp_lib.ITERATE_MINORRETRY))
                {
                    i = 0;
                    do
                    {
                        double d = 0;
                        i++;
                        //NOTED ISSUE:
                        rownr = LpPrice.rowdual(lp, lp_types.my_if(dualphase1, pcol[0], d), forceoutEQ, true, ref xviolated);
                    } while ((rownr == 0) && (i < LpPrice.partial_countBlocks(lp, (bool)!primal)) && LpPrice.partial_blockStep(lp, (bool)!primal));
                }

                /* Make sure that we do not erroneously conclude that an infeasible model is optimal */
#if dual_UseRejectionList
	if ((rownr == 0) && (lp.rejectpivot[0] > 0))
	{
	  lp.spx_status = INFEASIBLE;
	  if ((lp.spx_trace && (lp.bb_totalnodes == 0)) || (lp.bb_trace && (lp.bb_totalnodes > 0)))
	  {
		report(lp, DETAILED, "The model is primal infeasible.\n");
	  }
	  rownr = lp.rejectpivot[1];
	  colnr = 0;
	  lp.rejectpivot[0] = 0;
	  ok = 0;
	  break;
	}
#endif

                /* If we found a leaving variable, find a matching entering one */
                LpCls.clear_action(ref lp.spx_action, lp_lib.ACTION_ITERATE);
                if (rownr > 0)
                {
                    colnr = LpPrice.coldual(lp, rownr, ref prow[0], ref nzprow, ref drow, ref nzdrow[0], (bool)(dualphase1 && !inP1extra), (bool)(minit == Convert.ToBoolean(lp_lib.ITERATE_MINORRETRY)), ref candidatecount, ref cviolated);
                    if (colnr < 0)
                    {
                        minit = lp_lib.ITERATE_MAJORMAJOR;
                        continue;
                    }
#if AcceptMarginalAccuracy
	  else if (xviolated + cviolated < lp.epspivot)
	  {
		if (lp.bb_trace || (lp.bb_totalnodes == 0))
		{
		  report(lp, DETAILED, "dualloop: Assuming convergence with reduced accuracy %g.\n", MAX(xviolated, cviolated));
		}
		rownr = 0;
		colnr = 0;
	  }
#endif
                    /* Check if the long-dual found reason to prune the B&B tree */
                    if (lp.spx_status == lp_lib.FATHOMED)
                    {
                        break;
                    }
                }
                else
                {
                    colnr = 0;
                }

                /* Process primal-infeasible row */
                if (rownr > 0)
                {

                    if (colnr > 0)
                    {
#if Paranoia
		if ((rownr > lp.rows) || (colnr > lp.sum))
		{
		  report(lp, SEVERE, "dualloop: Invalid row %d(%d) and column %d(%d) pair selected at iteration %.0f\n", rownr, lp.rows, colnr - lp.columns, lp.columns, (double) get_total_iter(lp));
		  lp.spx_status = UNKNOWNERROR;
		  break;
		}
#endif
                        lp_matrix.fsolve(lp, colnr, pcol, workINT, lp.epsmachine, 1.0, true);

#if FixInaccurateDualMinit
	   /* Prevent bound flip-flops during minor iterations; used to detect
	      infeasibility after triggering of minor iteration accuracy management */
		if (colnr != minitcolnr)
		{
		  minitcolnr = 0;
		}
#endif

                        /* Getting division by zero here; catch it and try to recover */
                        if (pcol[rownr] == 0)
                        {
                            if (lp.spx_trace)
                            {
                                msg = "dualloop: Attempt to divide by zero (pcol[%d])\n";
                                lp.report(lp, lp_lib.DETAILED, ref msg, rownr);
                            }
                            if (!LpCls.refactRecent(lp))
                            {
                                msg = "...trying to recover by refactorizing basis.\n";
                                lp.report(lp, lp_lib.DETAILED, ref msg);
                                LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
                                bfpfinal = false;
                            }
                            else
                            {
                                if (lp.bb_totalnodes == 0)
                                {
                                    msg = "...cannot recover by refactorizing basis.\n";
                                    lp.report(lp, lp_lib.DETAILED, ref msg);
                                }
                                lp.spx_status = lp_lib.NUMFAILURE;
                                ok = false;
                            }
                        }
                        else
                        {
                            LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_ITERATE);
                            lp.rejectpivot[0] = 0;
                            if (!lp.obj_in_basis) // We must manually copy the reduced cost for RHS update
                            {
                                pcol[0] = lp_types.my_chsign(!lp.is_lower[colnr], drow[colnr]);
                            }
                            theta = lp.bfp_prepareupdate(lp, rownr, colnr, ref pcol[0]);

                            /* Verify numeric accuracy of the basis factorization and change to
                               the "theoretically" correct version of the theta */
                            if ((lp.improve != 0 & lp_lib.IMPROVE_THETAGAP != 0) && (!LpCls.refactRecent(lp)) && (lp_types.my_reldiff(System.Math.Abs(theta), System.Math.Abs((sbyte)prow[0])) > lp.epspivot * 10.0 * System.Math.Log(2.0 + 50.0 * lp.rows)))
                            { // This is my kludge - KE
                                LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
                                bfpfinal = true;
#if IncreasePivotOnReducedAccuracy
			lp.epspivot = MIN(1.0e-4, lp.epspivot * 2.0);
#endif
                                msg = "dualloop: Refactorizing at iter %.0f due to loss of accuracy.\n";
                                lp.report(lp, lp_lib.DETAILED, ref msg, (double)LpCls.get_total_iter(lp));
                            }
                            theta = prow[0];
                            LpCls.compute_theta(lp, rownr, ref theta, !lp.is_lower[colnr], 0, primal);
                        }
                    }

#if FixInaccurateDualMinit
	  /* Force reinvertion and try another row if we did not find a bound-violated leaving column */
	  else if (!refactRecent(lp) && (minit != ITERATE_MAJORMAJOR) && (colnr != minitcolnr))
	  {
		minitcolnr = colnr;
		i = invert(lp, INITSOL_USEZERO, 1);
		if ((lp.spx_status == USERABORT) || (lp.spx_status == TIMEOUT))
		{
		  break;
		}
		else if (!i)
		{
		  lp.spx_status = SINGULAR_BASIS;
		  break;
		}
		minit = ITERATE_MAJORMAJOR;
		continue;
	  }
#endif

                    /* We may be infeasible, have lost dual feasibility, or simply have no valid entering
                       variable for the selected row.  The strategy is to refactorize if we suspect numerical
                       problems and loss of dual feasibility; this is done if it has been a while since
                       refactorization.  If not, first try to select a different row/leaving variable to
                       see if a valid entering variable can be found.  Otherwise, determine this model
                       as infeasible. */
                    else
                    {

                        /* As a first option, try to recover from any numerical trouble by refactorizing */
                        if (!LpCls.refactRecent(lp))
                        {
                            LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
                            bfpfinal = true;
                        }

#if dual_UseRejectionList
		/* Check for pivot size issues */
		else if (lp.rejectpivot[0] < DEF_MAXPIVOTRETRY)
		{
		  lp.spx_status = RUNNING;
		  lp.rejectpivot[0]++;
		  lp.rejectpivot[lp.rejectpivot[0]] = rownr;
		  if (lp.bb_totalnodes == 0)
		  {
			report(lp, DETAILED, "...trying to find another pivot row!\n");
		  }
		  goto RetryRow;
		}
#endif
                        /* Check if we may have lost dual feasibility if we also did phase 1 here */
                        else if (dualphase1 && (dualoffset != 0))
                        {
                            lp.spx_status = lp_lib.LOSTFEAS;
                            if ((lp.spx_trace && (lp.bb_totalnodes == 0)) || (lp.bb_trace && (lp.bb_totalnodes > 0)))
                            {
                                msg = "dualloop: Model lost dual feasibility.\n";
                                lp.report(lp, lp_lib.DETAILED, ref msg);
                            }
                            ok = false;
                            break;
                        }
                        /* Otherwise just determine that we are infeasible */
                        else
                        {
                            if (lp.spx_status == lp_lib.RUNNING)
                            {
#if TRUE
                                if (xviolated < lp.epspivot)
                                {
                                    if (lp.bb_trace || (lp.bb_totalnodes == 0))
                                    {
                                        msg = "The model is primal optimal, but marginally infeasible.\n";
                                        lp.report(lp, lp_lib.NORMAL, ref msg);
                                    }
                                    lp.spx_status = lp_lib.OPTIMAL;
                                    break;
                                }
#endif
                                lp.spx_status = lp_lib.INFEASIBLE;
                                if ((lp.spx_trace && (lp.bb_totalnodes == 0)) || (lp.bb_trace && (lp.bb_totalnodes > 0)))
                                {
                                    msg = "The model is primal infeasible.\n";
                                    lp.report(lp, lp_lib.DETAILED, ref msg);
                                }
                            }
                            ok = false;
                            break;
                        }
                    }
                }

                /* Make sure that we enter the primal simplex with a high quality solution */
                else if (inP1extra && !LpCls.refactRecent(lp) && LpCls.is_action(lp.improve, lp_lib.IMPROVE_INVERSE))
                {
                    LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
                    bfpfinal = true;
                }

                /* High quality solution with no leaving candidates available ... */
                else
                {

                    bfpfinal = true;

#if dual_RemoveBasicFixedVars
	  /* See if we should try to eliminate basic fixed variables;
	    can be time-consuming for some models */
	  if (inP1extra && (colnr == 0) && (lp.fixedvars > 0) && is_anti_degen(lp, ANTIDEGEN_FIXEDVARS))
	  {
		report(lp, DETAILED, "dualloop: Trying to pivot out %d fixed basic variables at iter %.0f\n", lp.fixedvars, (double) get_total_iter(lp));
		rownr = 0;
		while (lp.fixedvars > 0)
		{
		  rownr = findBasicFixedvar(lp, rownr, 1);
		  if (rownr == 0)
		  {
			colnr = 0;
			break;
		  }
		  colnr = find_rowReplacement(lp, rownr, prow, nzprow);
		  if (colnr > 0)
		  {
			theta = 0;
			performiteration(lp, rownr, colnr, theta, 1, 0, prow, null, null, null, null);
			lp.fixedvars--;
		  }
		}
	  }
#endif

                    /* Check if we are INFEASIBLE for the case that the dual is used
                       as phase 1 before the primal simplex phase 2 */
                    double Parameter = 0;
                    if (inP1extra && (colnr < 0) && !LpCls.isPrimalFeasible(lp, lprec.epsprimal, null, ref Parameter))
                    {
                        if (lp.bb_totalnodes == 0)
                        {
                            if (dualfeasible)
                            {
                                msg = "The model is primal infeasible and dual feasible.\n";
                                lp.report(lp, lp_lib.DETAILED, ref msg);
                            }
                            else
                            {
                                msg = "The model is primal infeasible and dual unbounded.\n";
                                lp.report(lp, lp_lib.DETAILED, ref msg);
                            }
                        }
                        LpCls.set_OF_p1extra(lp, 0);
                        inP1extra = false;
                        LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);
                        lp.spx_status = lp_lib.INFEASIBLE;
                        lp.simplex_mode = lp_lib.SIMPLEX_UNDEFINED;
                        ok = false;
                    }

                    /* Check if we are FEASIBLE (and possibly also optimal) for the case that the
                       dual is used as phase 1 before the primal simplex phase 2 */
                    else if (inP1extra)
                    {

                        /* Set default action; force an update of the rhs vector, adjusted for
                           the new P1extraVal=0 (set here so that usermessage() behaves properly) */
                        if (lp.bb_totalnodes == 0)
                        {
                            msg = "Found feasibility by dual simplex after    %10.0f iter.\n";
                            lp.report(lp, lp_lib.NORMAL, ref msg, (double)LpCls.get_total_iter(lp));
                            if ((lp.usermessage != null) && (lp.msgmask != 0 & lp_lib.MSG_LPFEASIBLE != 0))
                            {
                                lp.usermessage(lp, lp.msghandle, lp_lib.MSG_LPFEASIBLE);
                            }
                        }
                        LpCls.set_OF_p1extra(lp, 0);
                        inP1extra = false;
                        LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REINVERT);

#if true
                        /* Optionally try another dual loop, if so selected by the user */
                        if ((lp.simplex_strategy != 0 & lp_lib.SIMPLEX_DUAL_PRIMAL != 0) && (lp.fixedvars == 0))
                        {
                            lp.spx_status = lp_lib.SWITCH_TO_PRIMAL;
                        }
#endif
                        changedphase = true;

                    }
                    /* We are primal feasible and also optimal if we were in phase 2 */
                    else
                    {

                        lp.simplex_mode = lp_lib.SIMPLEX_Phase2_DUAL;

                        /* Check if we still have equality slacks stuck in the basis; drive them out? */
                        if ((lp.fixedvars > 0) && (lp.bb_totalnodes == 0))
                        {
#if dual_Phase1PriceEqualities
		  if (forceoutEQ != 1)
		  {
			forceoutEQ = 1;
			goto RetryRow;
		  }
#endif
#if Paranoia
		  report(lp, NORMAL,
#else
                            msg = "Found dual solution with %d fixed slack variables left basic.\n";
                            lp.report(lp, lp_lib.DETAILED,
#endif
                    ref msg,
                                      lp.fixedvars);
                        }
                        /* Check if we are still dual feasible; the default assumes that this check
                          is not necessary after the relaxed problem has been solved satisfactorily. */
                        colnr = 0;
                        if ((dualoffset != 0) || (lp.bb_level <= 1) || (lp.improve != 0 & lp_lib.IMPROVE_BBSIMPLEX != 0) || (lp.bb_rule != 0 & lp_lib.NODE_RCOSTFIXING != 0))
                        { // NODE_RCOSTFIXING fix
                            LpCls.set_action(ref lp.piv_strategy, lp_lib.PRICE_FORCEFULL);
                            double d = 0;
                            colnr = LpPrice.colprim(lp, ref drow[0], ref nzdrow, false, 1, ref candidatecount, false, ref d);
                            LpCls.clear_action(ref lp.piv_strategy, lp_lib.PRICE_FORCEFULL);
                            if ((dualoffset == 0) && (colnr > 0))
                            {
                                lp.spx_status = lp_lib.LOSTFEAS;
                                if (lp.total_iter == 0)
                                {
                                    msg = "Recovering lost dual feasibility at iter %10.0f.\n";
                                    lp.report(lp, lp_lib.DETAILED, ref msg, (double)LpCls.get_total_iter(lp));
                                }
                                break;
                            }
                        }

                        if (colnr == 0)
                        {
                            lp.spx_status = lp_lib.OPTIMAL;
                        }
                        else
                        {
                            lp.spx_status = lp_lib.SWITCH_TO_PRIMAL;
                            if (lp.total_iter == 0)
                            {
                                msg = "Use primal simplex for finalization at iter  %10.0f.\n";
                                lp.report(lp, lp_lib.DETAILED, ref msg, (double)LpCls.get_total_iter(lp));
                            }
                        }
                        if ((lp.total_iter == 0) && (lp.spx_status == lp_lib.OPTIMAL))
                        {
                            msg = "Optimal solution with dual simplex at iter   %10.0f.\n";
                            lp.report(lp, lp_lib.DETAILED, ref msg, (double)LpCls.get_total_iter(lp));
                        }
                    }

                    /* Determine if we are ready to break out of the loop */
                    if (!changedphase)
                    {
                        break;
                    }
                }

                /* Check if we are allowed to iterate on the chosen column and row */
                if (LpCls.is_action(lp.spx_action, lp_lib.ACTION_ITERATE))
                {

                    lastnr = lp.var_basic[rownr];
                    if (LpCls.refactRecent(lp) == Convert.ToBoolean(lp_types.AUTOMATIC))
                    {
                        minitcount = 0;
                    }
                    else if (minitcount > lp_lib.MAX_MINITUPDATES)
                    {
                        LpCls.recompute_solution(lp, lp_lib.INITSOL_USEZERO);
                        minitcount = 0;
                    }
                    int Parameter = 0;
                    minit = LpCls.performiteration(lp, rownr, colnr, theta, primal, (bool)((stallaccept != Convert.ToBoolean(lp_types.AUTOMATIC))), ref prow, ref nzprow[0], ref pcol, ref Parameter, ref boundswaps);

                    /* Check if we should abandon iterations on finding that there is no
                      hope that this branch can improve on the incumbent B&B solution */
                    if (lp.is_strongbranch != 0 && (lp.solutioncount >= 1) && !lp.spx_perturbed && !inP1extra && LpCls.bb_better(lp, lp_lib.OF_WORKING, lp_lib.OF_TEST_WE))
                    {
                        lp.spx_status = lp_lib.FATHOMED;
                        ok = true;
                        break;
                    }

                    if (minit != lp_lib.ITERATE_MAJORMAJOR)
                    {
                        minitcount++;
                    }

                    /* Update reduced costs for (pure) dual long-step phase 1 */
                    if (longsteps && dualphase1 && !inP1extra)
                    {
                        int Para1 = 0;
                        double Para2 = 0;
                        dualfeasible = LpCls.isDualFeasible(lp, lprec.epsprimal, ref Para1, dualinfeasibles, Para2);
                        if (dualfeasible)
                        {
                            dualphase1 = true;
                            changedphase = false;
                            lp.simplex_mode = lp_lib.SIMPLEX_Phase2_DUAL;
                        }
                    }
#if UseDualReducedCostUpdate
	  /* Do a fast update of reduced costs in preparation for the next iteration */
	  else if (minit == ITERATE_MAJORMAJOR)
	  {
		update_reducedcosts(lp, primal, lastnr, colnr, prow, drow);
	  }
#endif
                    if ((minit == lp_lib.ITERATE_MAJORMAJOR) && (lastnr <= lp.rows) && LpCls.is_fixedvar(lp, lastnr))
                    {
                        lp.fixedvars--;
                    }
                }
                /* Refactorize if required to */
                if (Convert.ToBoolean(lp.bfp_mustrefactorize(lp)))
                {
                    if (lp_matrix.invert(lp, lp_lib.INITSOL_USEZERO, bfpfinal))
                    {

#if false
// /* Verify dual feasibility in case we are attempting the extra dual loop */
//        if(changedphase && (dualoffset != 0) && !inP1extra && (lp->spx_status != SWITCH_TO_PRIMAL)) {
//#if 1
//          if(!isDualFeasible(lp, lp->epsdual, &colnr, NULL, NULL)) {
//#else
//          set_action(&lp->piv_strategy, PRICE_FORCEFULL);
//            colnr = colprim(lp, drow, nzdrow, FALSE, 1, &candidatecount, FALSE, NULL);
//          clear_action(&lp->piv_strategy, PRICE_FORCEFULL);
//          if(colnr > 0) {
//#endif
//            lp->spx_status = SWITCH_TO_PRIMAL;
//            colnr = 0;
//          }
//        }
#endif

                        bfpfinal = false;
#if ResetMinitOnReinvert
		minit = ITERATE_MAJORMAJOR;
#endif
                    }
                    else
                    {
                        lp.spx_status = lp_lib.SINGULAR_BASIS;
                    }
                }
            }

            Finish:
            stallMonitor_finish(lp);
            //NOT REQUIRED
            //multi_free((lp.longsteps));
            //FREE(prow);
            //FREE(nzprow);
            //FREE(pcol);

            return (Convert.ToInt32(ok));
        }

        public static int spx_run(lprec lp, byte validInvB)
        {
            int i;
            int j;
            int singular_count;
            int lost_feas_count;

            //ORIGINAL LINE: int *infeasibles = null;
            int[] infeasibles = null;

            //ORIGINAL LINE: int *boundflip_count;
            int boundflip_count;
            bool primalfeasible = new bool();
            bool dualfeasible = new bool();
            bool lost_feas_state = new bool();
            bool isbb = new bool();
            double primaloffset = 0;
            double dualoffset = 0;
            LpCls objLpCls = new LpCls();
            string msg;

            lp.current_iter = 0;
            lp.current_bswap = 0;
            lp.spx_status = lp_lib.RUNNING;
            lp.bb_status = lp.spx_status;
            lp.P1extraDim = 0;
            LpCls.set_OF_p1extra(lp, 0);
            singular_count = 0;
            lost_feas_count = 0;
            lost_feas_state = false;
            lp.simplex_mode = lp_lib.SIMPLEX_DYNAMIC;

            /* Compute the number of fixed basic and bounded variables (used in long duals) */
            lp.fixedvars = 0;
            lp.boundedvars = 0;
            for (i = 1; i <= lp.rows; i++)
            {
                j = lp.var_basic[i];
                if ((j <= lp.rows) && LpCls.is_fixedvar(lp, j))
                {
                    lp.fixedvars++;
                }
                if ((lp.upbo[i] < lp.infinite) && (lp.upbo[i] > lprec.epsprimal))
                {
                    lp.boundedvars++;
                }
            }
            for (; i <= lp.sum; i++)
            {
                if ((lp.upbo[i] < lp.infinite) && (lp.upbo[i] > lprec.epsprimal))
                {
                    lp.boundedvars++;
                }
            }
#if UseLongStepDualPhase1
  allocINT(lp, infeasibles, lp.columns + 1, 0);
  infeasibles[0] = 0;
#endif

            /* Reinvert for initialization, if necessary */           
            //ORIGINAL LINE: isbb = (MYBOOL)((MIP_count(lp) > 0) && (lp->bb_level > 1));
            isbb = ((bool)((LpCls.MIP_count(lp) > 0) && (lp.bb_level > 1)));
            if (LpCls.is_action(lp.spx_action, lp_lib.ACTION_REINVERT))
            {
                if (isbb != null && (lp.bb_bounds.nodessolved == 0))
                {
                    /*    if(isbb && (lp->bb_basis->pivots == 0)) */
                    LpCls.recompute_solution(lp, Convert.ToBoolean(lp_lib.INITSOL_SHIFTZERO));
                }
                else
                {
                    i = Convert.ToInt32(lp_types.my_if(LpCls.is_action(lp.spx_action, lp_lib.ACTION_REBASE), lp_lib.INITSOL_SHIFTZERO, Convert.ToDouble(lp_lib.INITSOL_USEZERO)));
                    lp_matrix.invert(lp, Convert.ToBoolean(i), true);
                }
            }
            else if (LpCls.is_action(lp.spx_action, lp_lib.ACTION_REBASE))
            {
                LpCls.recompute_solution(lp, Convert.ToBoolean(lp_lib.INITSOL_SHIFTZERO));
            }

            /* Optionally try to do bound flips to obtain dual feasibility */
            if (LpCls.is_action(lp.improve, lp_lib.IMPROVE_DUALFEAS) || (lp.rows == 0))
            {
                boundflip_count = i;
            }
            else
            {
                boundflip_count = 0;
            }

            /* Loop for as long as is needed */
            while (lp.spx_status == lp_lib.RUNNING)
            {

                /* Check for dual and primal feasibility */
                dualfeasible = isbb != null || LpCls.isDualFeasible(lp, lprec.epsprimal, ref boundflip_count, infeasibles, dualoffset);

                /* Recompute if the dual feasibility check included bound flips */
                if (LpCls.is_action(lp.spx_action, lp_lib.ACTION_RECOMPUTE))
                {
                    LpCls.recompute_solution(lp, lp_lib.INITSOL_USEZERO);
                }
                primalfeasible = LpCls.isPrimalFeasible(lp, lprec.epsprimal, null, ref primaloffset);

                if (LpCls.userabort(lp, -1))
                {
                    break;
                }

                lp_report objReport = new lp_report();
                if (lp.spx_trace)
                {
                    if (primalfeasible != null)
                    {
                        msg = "Start at primal feasible basis\n";
                        objReport.report(lp, lp_lib.NORMAL, ref msg);
                    }
                    else if (dualfeasible)
                    {
                        msg = "Start at dual feasible basis\n";
                        objReport.report(lp, lp_lib.NORMAL, ref msg);
                    }
                    else if (lost_feas_count > 0)
                    {
                        msg = "Continuing at infeasible basis\n";
                        objReport.report(lp, lp_lib.NORMAL, ref msg);
                    }
                    else
                    {
                        msg = "Start at infeasible basis\n";
                        objReport.report(lp, lp_lib.NORMAL, ref msg);
                    }
                }

                /* Now do the simplex magic */
                if (((lp.simplex_strategy & lp_lib.SIMPLEX_Phase1_DUAL) == 0) || ((LpCls.MIP_count(lp) > 0) && (lp.total_iter == 0) && objLpCls.is_presolve(lp, lp_lib.PRESOLVE_REDUCEMIP)))
                {
                    if (lost_feas_state == null && primalfeasible != null && ((lp.simplex_strategy & lp_lib.SIMPLEX_Phase2_DUAL) > 0))
                    {
                        lp.spx_status = lp_lib.SWITCH_TO_DUAL;
                    }
                    else
                    {
                        primloop(lp, primalfeasible, 0.0);
                    }
                    if (lp.spx_status == lp_lib.SWITCH_TO_DUAL)
                    {
                        dualloop(lp, true, null, 0.0);
                    }
                }
                else
                {
                    if (lost_feas_state == null && primalfeasible != null && ((lp.simplex_strategy & lp_lib.SIMPLEX_Phase2_PRIMAL) > 0))
                    {
                        lp.spx_status = lp_lib.SWITCH_TO_PRIMAL;
                    }
                    else
                    {
                        dualloop(lp, dualfeasible, infeasibles, dualoffset);
                    }
                    if (lp.spx_status == lp_lib.SWITCH_TO_PRIMAL)
                    {
                        primloop(lp, true, 0.0);
                    }
                }
                /* Check for simplex outcomes that always involve breaking out of the loop;
   this includes optimality, unboundedness, pure infeasibility (i.e. not
   loss of feasibility), numerical failure and perturbation-based degeneracy
   handling */
                i = lp.spx_status;
                primalfeasible = (bool)(i == lp_lib.OPTIMAL);
                if (primalfeasible || (i == lp_lib.UNBOUNDED))
                {
                    break;
                }
                else if (((i == lp_lib.INFEASIBLE) && objLpCls.is_anti_degen(lp, lp_lib.ANTIDEGEN_INFEASIBLE)) || ((i == lp_lib.LOSTFEAS) && objLpCls.is_anti_degen(lp, lp_lib.ANTIDEGEN_LOSTFEAS)) || ((i == lp_lib.NUMFAILURE) && objLpCls.is_anti_degen(lp, lp_lib.ANTIDEGEN_NUMFAILURE)) || ((i == lp_lib.DEGENERATE) && objLpCls.is_anti_degen(lp, lp_lib.ANTIDEGEN_STALLING)))
                {
                    /* Check if we should not loop here, but do perturbations */
                    if ((lp.bb_level <= 1) || objLpCls.is_anti_degen(lp, lp_lib.ANTIDEGEN_DURINGBB))
                    {
                        break;
                    }

                    /* Assume that accuracy during B&B is high and that infeasibility is "real" */
#if AssumeHighAccuracyInBB
	  if ((lp.bb_level > 1) && (i == INFEASIBLE))
	  {
		break;
	  }
#endif
                }

                /* Check for outcomes that may involve trying another simplex loop */
                if (lp.spx_status == lp_lib.SINGULAR_BASIS)
                {
                    lost_feas_state = false;
                    singular_count++;
                    if (singular_count >= lp_lib.DEF_MAXSINGULARITIES)
                    {
                        msg = "spx_run: Failure due to too many singular bases.\n";
                        objReport.report(lp, lp_lib.IMPORTANT, ref msg);
                        lp.spx_status = lp_lib.NUMFAILURE;
                        break;
                    }
                    if (lp.spx_trace || (lp.verbose > lp_lib.DETAILED))
                    {
                        msg = "spx_run: Singular basis; attempting to recover.\n";
                       objReport.report(lp, lp_lib.NORMAL, ref msg);
                    }
                    lp.spx_status = lp_lib.RUNNING;
                    /* Singular pivots are simply skipped by the inversion, leaving a row's
                       slack variable in the basis instead of the singular user variable. */
                }
                else
                {
                    lost_feas_state = (bool)(lp.spx_status == lp_lib.LOSTFEAS);
#if false
// /* Optionally handle loss of numerical accuracy as loss of feasibility,
//    but only attempt a single loop to try to recover from this. */
//      lost_feas_state |= (MYBOOL) ((lp->spx_status == NUMFAILURE) && (lost_feas_count < 1));
#endif
                    if (lost_feas_state)
                    {
                        lost_feas_count++;
                        if (lost_feas_count < lp_lib.DEF_MAXSINGULARITIES)
                        {
                            msg = "spx_run: Recover lost feasibility at iter  %10.0f.\n";
                            objReport.report(lp, lp_lib.DETAILED, ref msg, (double)LpCls.get_total_iter(lp));
                            lp.spx_status = lp_lib.RUNNING;
                        }
                        else
                        {
                            msg = "spx_run: Lost feasibility %d times - iter %10.0f and %9.0f nodes.\n";
                            objReport.report(lp, lp_lib.IMPORTANT, ref msg, lost_feas_count, (double)LpCls.get_total_iter(lp), (double)lp.bb_totalnodes);
                            lp.spx_status = lp_lib.NUMFAILURE;
                        }
                    }
                }
            }

            /* Update iteration tallies before returning */
            lp.total_iter += lp.current_iter;
            lp.current_iter = 0;
            lp.total_bswap += lp.current_bswap;
            lp.current_bswap = 0;
            //NOT REQUIRED
            //FREE(infeasibles);

            return (lp.spx_status);
        }

        public static lprec make_lag(lprec lpserver)
        {
            int i;
            lprec hlp;
            bool ret;
            double[][] duals=null;
            LpCls objLpCls = new LpCls();

            /* Create a Lagrangean solver instance */
            hlp = objLpCls.make_lp(0, lpserver.columns);

            if (hlp != null)
            {

                /* First create and core variable data */
                objLpCls.set_sense(hlp, LpCls.is_maxim(lpserver));
                hlp.lag_bound = lpserver.bb_limitOF;
                for (i = 1; i <= lpserver.columns; i++)
                {
                    LpCls.set_mat(hlp, 0, i, LpCls.get_mat(lpserver, 0, i));
                    if (objLpCls.is_binary(lpserver, i))
                    {
                        objLpCls.set_binary(hlp, i, true);
                    }
                    else
                    {
                        objLpCls.set_int(hlp, i, LpCls.is_int(lpserver, i));
                        objLpCls.set_bounds(hlp, i, LpCls.get_lowbo(lpserver, i), LpCls.get_upbo(lpserver, i));
                    }
                }
                /* Then fill data for the Lagrangean constraints */
                hlp.matL = lpserver.matA;
                LpCls.inc_lag_space(hlp, lpserver.rows, true);
                ret = objLpCls.get_ptr_sensitivity_rhs(hlp, duals, null, null);
                for (i = 1; i <= lpserver.rows; i++)
                {
                    hlp.lag_con_type[i] = objLpCls.get_constr_type(lpserver, i);
                    hlp.lag_rhs[i] = lpserver.orig_rhs[i];
                    hlp.lambda[i] = (ret) ? duals[i - 1][0] : 0.0;
                }
            }

            return (hlp);
        }

        public static int spx_solve(lprec lp)
        {

            int status;
            bool iprocessed = new bool();
            LpCls objLpCls = new LpCls();
            string msg = "";

            lp.total_iter = 0;
            lp.total_bswap = 0;
            lp.perturb_count = 0;
            lp.bb_maxlevel = 1;
            lp.bb_totalnodes = 0;
            lp.bb_improvements = 0;
            lp.bb_strongbranches = 0;
            lp.is_strongbranch = 0;
            lp.bb_level = 0;
            lp.bb_solutionlevel = 0;
            lp.best_solution[0] = lp_types.my_chsign(LpCls.is_maxim(lp), lp.infinite);
            if (lp.invB != null)
            {
                lp.bfp_restart(lp);
            }

            lp.spx_status = lp_presolve.presolve(lp);
            if (lp.spx_status == lp_lib.PRESOLVED)
            {
                status = lp.spx_status;
                //NOTED ISSUE
                goto Reconstruct;
            }
            else if (lp.spx_status != lp_lib.RUNNING)
            {
                goto Leave;
            }

            iprocessed = !lp.wasPreprocessed;
            if (!objLpCls.preprocess(lp) || LpCls.userabort(lp, -1))
            {
                goto Leave;
            }

            if (lp_matrix.mat_validate(lp.matA))
            {

                /* Do standard initializations */
                lp.solutioncount = 0;
                lp.real_solution = lp.infinite;
                LpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REBASE | lp_lib.ACTION_REINVERT);
                lp.bb_break = 0;

                /* Do the call to the real underlying solver (note that
                   run_BB is replaceable with any compatible MIP solver) */
                status = lp_mipbb.run_BB(lp);

                /* Restore modified problem */
                if (iprocessed != null)
                {
                    objLpCls.postprocess(lp);
                }

            /* Restore data related to presolve (mainly a placeholder as of v5.1) */
            Reconstruct:
                if (!lp_presolve.postsolve(lp, status))
                {
                    msg = "spx_solve: Failure during postsolve.\n";
                    lp.report(lp, lp_lib.SEVERE, ref msg);
                }

                goto Leave;
            }

            /* If we get here, mat_validate(lp) failed. */
            if (lp.bb_trace || lp.spx_trace)
            {
                msg = "spx_solve: The current LP seems to be invalid\n";
                lp.report(lp, lp_lib.CRITICAL, ref msg);
            }
            lp.spx_status = lp_lib.NUMFAILURE;

        Leave:
            lp.timeend = commonlib.timeNow();

            if ((lp.lag_status != lp_lib.RUNNING) && (lp.invB != null))
            {
                int itemp;
                double test = new double();

                itemp = lp.bfp_nonzeros(lp, 1);
                test = 100;
                if (lp.total_iter > 0)
                {
                    test *= (double)lp.total_bswap / lp.total_iter;
                }

                msg = "\n ";
                lp.report(lp, lp_lib.NORMAL, ref msg);
                msg = "MEMO: lp_solve version %d.%d.%d.%d for %d bit OS, with %d bit REAL variables.\n";
                //ORIGINAL LINE:lp.report(lp, lp_lib.NORMAL, ref msg, lp_lib.MAJORVERSION, lp_lib.MINORVERSION, lp_lib.RELEASE, lp_lib.BUILD, 8 * sizeof(object), 8 * sizeof(double));
                //Removed: 8 * sizeof(object), need to check at runtime
                lp.report(lp, lp_lib.NORMAL, ref msg, lp_lib.MAJORVERSION, lp_lib.MINORVERSION, lp_lib.RELEASE, lp_lib.BUILD, 8 * sizeof(double));
                msg = "      In the total iteration count %.0f, %.0f (%.1f%%) were bound flips.\n";
                lp.report(lp, lp_lib.NORMAL, ref msg, (double)lp.total_iter, (double)lp.total_bswap, test);
                msg = "      There were %d refactorizations, %d triggered by time and %d by density.\n";
                lp.report(lp, lp_lib.NORMAL, ref msg, lp.bfp_refactcount(lp, commonlib.BFP_STAT_REFACT_TOTAL), lp.bfp_refactcount(lp, commonlib.BFP_STAT_REFACT_TIMED), lp.bfp_refactcount(lp, commonlib.BFP_STAT_REFACT_DENSE));
                msg = "       ... on average %.1f major pivots per refactorization.\n";
                lp.report(lp, lp_lib.NORMAL, ref msg, objLpCls.get_refactfrequency(lp, 1));
                msg = "      The largest [%s] fact(B) had %d NZ entries, %.1fx largest basis.\n";
                lp.report(lp, lp_lib.NORMAL, ref msg, lp.bfp_name(), itemp, lp.bfp_efficiency(lp));
                if (lp.perturb_count > 0)
                {
                    msg = "      The bounds were relaxed via perturbations %d times.\n";
                    lp.report(lp, lp_lib.NORMAL, ref msg, lp.perturb_count);
                }
                if (LpCls.MIP_count(lp) > 0)
                {
                    if (lp.bb_solutionlevel > 0)
                    {
                        msg = "      The maximum B&B level was %d, %.1fx MIP order, %d at the optimal solution.\n";
                        lp.report(lp, lp_lib.NORMAL, ref msg, lp.bb_maxlevel, (double)lp.bb_maxlevel / (LpCls.MIP_count(lp) + lp.int_vars), lp.bb_solutionlevel);
                    }
                    else
                    {
                        msg = "      The maximum B&B level was %d, %.1fx MIP order, with %.0f nodes explored.\n";
                        lp.report(lp, lp_lib.NORMAL, ref msg, lp.bb_maxlevel, (double)lp.bb_maxlevel / (LpCls.MIP_count(lp) + lp.int_vars), (double)objLpCls.get_total_nodes(lp));
                    }
                    if (LpCls.GUB_count(lp) > 0)
                    {
                        msg = "      %d general upper-bounded (GUB) structures were employed during B&B.\n";
                        lp.report(lp, lp_lib.NORMAL, ref msg, LpCls.GUB_count(lp));
                    }
                }
                msg = "      The constraint matrix inf-norm is %g, with a dynamic range of %g.\n";
                lp.report(lp, lp_lib.NORMAL, ref msg, lp.matA.infnorm, lp.matA.dynrange);
                msg = "      Time to load data was %.3f seconds, presolve used %.3f seconds,\n";
                //ORIGINAL LINE: lp.report(lp, lp_lib.NORMAL, ref msg, lp.timestart - lp.timecreate, lp.timepresolved - lp.timestart);
                //Removed Datetime substraction, need to check at runtime
                lp.report(lp, lp_lib.NORMAL, ref msg);
                msg = "       ... %.3f seconds in simplex solver, in total %.3f seconds.\n";
                lp.report(lp, lp_lib.NORMAL, ref msg, lp.timeend - lp.timepresolved, lp.timeend - lp.timecreate);
            }
            return (lp.spx_status);
        }

        public static int lag_solve(lprec lp, double start_bound, int num_iter)
        {
            int i;
            int j;
            int citer;
            int nochange;
            int oldpresolve;
            bool LagFeas;
            bool AnyFeas;
            bool Converged;
            bool same_basis;
            //ORIGINAL LINE: double *OrigObj, *ModObj, *SubGrad, *BestFeasSol;
            double[] OrigObj=null;
            //ORIGINAL LINE: double *ModObj;
            double[] ModObj = null;
            //ORIGINAL LINE: double *SubGrad;
            double[] SubGrad= null;
            //ORIGINAL LINE: double *BestFeasSol;
            double[] BestFeasSol = null;
            double Zub;
            double Zlb;
            double Znow;
            double Zprev;
            double Zbest;
            double rhsmod;
            double hold;
            double Phi;
            double StepSize = 0.0;
            double SqrsumSubGrad;
            string msg;
            LpCls objLpCls = new LpCls();

            /* Make sure we have something to work with */
            if (lp.spx_status != lp_lib.OPTIMAL)
            {
                lp.lag_status = lp_lib.NOTRUN;
                return (lp.lag_status);
            }

            /* Allocate iteration arrays */
            //Below Code is not required as it is memory allowcation
            
            //NOT RQUIRED
            /*if (!lp_utils.allocREAL(lp, OrigObj, lp.columns + 1, 0) || !lp_utils.allocREAL(lp, ModObj, lp.columns + 1, 1) || !lp_utils.allocREAL(lp, SubGrad, LpCls.get_Lrows(lp) + 1, 1) || !lp_utils.allocREAL(lp, BestFeasSol, lp.sum + 1, 1))
            {
                lp.lag_status = lp_lib.NOMEMORY;
                return (lp.lag_status);
            }*/
            lp.lag_status = lp_lib.RUNNING;

            /* Prepare for Lagrangean iterations using results from relaxed problem */
            oldpresolve = lp.do_presolve;
            lp.do_presolve = lp_lib.PRESOLVE_NONE;
            int Parameter = 0;
            bool Paremeter2 = false;
            LpCls.push_basis(lp, ref Parameter, ref Paremeter2, null);

            /* Initialize variables (assume minimization problem in overall structure) */
            Zlb = (double)lp.best_solution[0];
            Zub = start_bound;
            Zbest = Zub;
            Znow = Zlb;
            Zprev = lp.infinite;
            rhsmod = 0;

            Phi = lp_lib.DEF_LAGCONTRACT; // In the range 0-2.0 to guarantee convergence
                                   /*  Phi      = 0.15; */
            LagFeas = false;
            Converged = false;
            AnyFeas = false;
            citer = 0;
            nochange = 0;

            /* Initialize reference and solution vectors; don't bother about the
               original OF offset since we are maintaining an offset locally. */

            /* #define DirectOverrideOF */

           objLpCls.get_row(lp, 0, ref OrigObj);
#if DirectOverrideOF
  set_OF_override(lp, ModObj);
#endif
            OrigObj[0] = objLpCls.get_rh(lp, 0);
            for (i = 1; i <= LpCls.get_Lrows(lp); i++)
            {
                lp.lambda[i] = 0;
            }

            /* Iterate to convergence, failure or user-specified termination */
            while ((lp.lag_status == lp_lib.RUNNING) && (citer < num_iter))
            {

                citer++;

                /* Compute constraint feasibility gaps and associated sum of squares,
                   and determine feasibility over the Lagrangean constraints;
                   SubGrad is the subgradient, which here is identical to the slack. */
                LagFeas = true;
                Converged = true;
                SqrsumSubGrad = 0;
                for (i = 1; i <= LpCls.get_Lrows(lp); i++)
                {
                    hold = lp.lag_rhs[i];
                    for (j = 1; j <= lp.columns; j++)
                    {
                        hold -= Convert.ToDouble(lp_matrix.mat_getitem(lp.matL, i, j) * lp.best_solution[lp.rows + j]);
                    }
                    if (LagFeas)
                    {
                        if (lp.lag_con_type[i] == lp_lib.EQ)
                        {
                            if (System.Math.Abs(hold) > lprec.epsprimal)
                            {
                                LagFeas = false;
                            }
                        }
                        else if (hold < -lprec.epsprimal)
                        {
                            LagFeas = false;
                        }
                    }
                    /* Test for convergence and update */
                    if (Converged && (System.Math.Abs(lp_types.my_reldiff(hold, SubGrad[i])) > lp.lag_accept))
                    {
                        Converged = false;
                    }
                    SubGrad[i] = hold;
                    SqrsumSubGrad += hold * hold;
                }
                SqrsumSubGrad = System.Math.Sqrt(SqrsumSubGrad);
#if ONE
	Converged &= LagFeas;
#endif
                if (Converged)
                {
                    break;
                }

                /* Modify step parameters and initialize ahead of next iteration */
                Znow = Convert.ToDouble(lp.best_solution[0] - rhsmod);
                if (Znow > Zub)
                {
                    /* Handle exceptional case where we overshoot */
                    Phi *= lp_lib.DEF_LAGCONTRACT;
                    StepSize *= (Zub - Zlb) / (Znow - Zlb);
                }

                else

#if LagBasisContract
                    /*      StepSize = Phi * (Zub - Znow) / SqrsumSubGrad; */
                    StepSize = Phi * (2 - DEF_LAGCONTRACT) * (Zub - Znow) / SqrsumSubGrad;
#else
	  StepSize = Phi * (Zub - Znow) / SqrsumSubGrad;
#endif

                /* Compute the new dual price vector (Lagrangean multipliers, lambda) */
                for (i = 1; i <= LpCls.get_Lrows(lp); i++)
                {
                    lp.lambda[i] += StepSize * SubGrad[i];
                    if ((lp.lag_con_type[i] != lp_lib.EQ) && (lp.lambda[i] > 0))
                    {
                        /* Handle case where we overshoot and need to correct (see above) */
                        if (Znow < Zub)
                        {
                            lp.lambda[i] = 0;
                        }
                    }
                }
                /*    normalizeVector(lp->lambda, get_Lrows(lp)); */

                /* Save the current vector if it is better */
                if (LagFeas && (Znow < Zbest))
                {

                    /* Recompute the objective function value in terms of the original values */
                    //NOT REQUIRED
                    //MEMCOPY(BestFeasSol, lp.best_solution, lp.sum + 1);
                    hold = OrigObj[0];
                    for (i = 1; i <= lp.columns; i++)
                    {
                        hold += Convert.ToDouble(lp.best_solution[lp.rows + i] * OrigObj[i]);
                    }
                    BestFeasSol[0] = hold;
                    if (lp.lag_trace)
                    {
                        msg = "lag_solve: Improved feasible solution at iteration %d of %g\n";
                        lp.report(lp, lp_lib.NORMAL, ref msg, citer, hold);
                    }

                    /* Reset variables */
                    Zbest = Znow;
                    AnyFeas = true;
                    nochange = 0;
                }
                else if (Znow == Zprev)
                {
                    nochange++;
                    if (nochange > lp_lib.LAG_SINGULARLIMIT)
                    {
                        Phi *= 0.5;
                        nochange = 0;
                    }
                }
                Zprev = Znow;

                /* Recompute the objective function values for the next iteration */
                for (j = 1; j <= lp.columns; j++)
                {
                    hold = 0;
                    for (i = 1; i <= LpCls.get_Lrows(lp); i++)
                    {
                        hold += lp.lambda[i] * lp_matrix.mat_getitem(lp.matL, i, j);
                    }
                    ModObj[j] = OrigObj[j] - lp_types.my_chsign(LpCls.is_maxim(lp), hold);
#if !DirectOverrideOF
                    LpCls.set_mat(lp, 0, j, ModObj[j]);
#endif
                }

                /* Recompute the fixed part of the new objective function */
                rhsmod = lp_types.my_chsign(LpCls.is_maxim(lp), objLpCls.get_rh(lp, 0));
                for (i = 1; i <= LpCls.get_Lrows(lp); i++)
                {
                    rhsmod += lp.lambda[i] * lp.lag_rhs[i];
                }

                /* Print trace/debugging information, if specified */
                if (lp.lag_trace)
                {
                    msg = "Zub: %10g Zlb: %10g Stepsize: %10g Phi: %10g Feas %d\n";
                    lp.report(lp, lp_lib.IMPORTANT, ref msg, (double)Zub, (double)Zlb, (double)StepSize, (double)Phi, LagFeas);
                    for (i = 1; i <= LpCls.get_Lrows(lp); i++)
                    {
                        msg = "%3d SubGrad %10g lambda %10g\n";
                        lp.report(lp, lp_lib.IMPORTANT, ref msg, i, (double)SubGrad[i], (double)lp.lambda[i]);
                    }
                    if (lp.sum < 20)
                    {
                       objLpCls.print_lp(lp);
                    }
                }

                /* Solve the Lagrangean relaxation, handle failures and compute
                   the Lagrangean objective value, if successful */
                i = spx_solve(lp);
                if (lp.spx_status == lp_lib.UNBOUNDED)
                {
                    if (lp.lag_trace)
                    {
                        msg = "lag_solve: Unbounded solution encountered with this OF:\n";
                        lp.report(lp, lp_lib.NORMAL, ref msg);
                        for (i = 1; i <= lp.columns; i++)
                        {
                            lp.report(lp, lp_lib.NORMAL, ref msg, (double)ModObj[i]);
                        }
                    }
                    goto Leave;
                }
                else if ((lp.spx_status == lp_lib.NUMFAILURE) || (lp.spx_status == lp_lib.PROCFAIL) || (lp.spx_status == lp_lib.USERABORT) || (lp.spx_status == lp_lib.TIMEOUT) || (lp.spx_status == lp_lib.INFEASIBLE))
                {
                    lp.lag_status = lp.spx_status;
                }

                /* Compare optimal bases and contract if we have basis stationarity */
#if LagBasisContract
                same_basis = compare_basis(lp);
                if (LagFeas && !same_basis)
                {
                    pop_basis(lp, 0);
                    push_basis(lp, null, null, null);
                    Phi *= DEF_LAGCONTRACT;
                }
                if (lp.lag_trace)
                {
                    report(lp, DETAILED, "lag_solve: Simplex status code %d, same basis %s\n", lp.spx_status, my_boolstr(same_basis));
                    print_solution(lp, 1);
                }
#endif
            }

            /* Transfer solution values */
            if (AnyFeas)
            {
                lp.lag_bound = lp_types.my_chsign(LpCls.is_maxim(lp), Zbest);
                for (i = 0; i <= lp.sum; i++)
                {
                    lp.solution[i] = BestFeasSol[i];
                }
                objLpCls.transfer_solution(lp, 1);
                if (!LpCls.is_maxim(lp))
                {
                    for (i = 1; i <= LpCls.get_Lrows(lp); i++)
                    {
                        lp.lambda[i] = lp_types.my_flipsign(lp.lambda[i]);
                    }
                }
            }

        /* Do standard postprocessing */
        Leave:

            /* Set status variables and report */
            if (citer >= num_iter)
            {
                if (AnyFeas)
                {
                    lp.lag_status = lp_lib.FEASFOUND;
                }
                else
                {
                    lp.lag_status = lp_lib.NOFEASFOUND;
                }
            }
            else
            {
                lp.lag_status = lp.spx_status;
            }
            if (lp.lag_status == lp_lib.OPTIMAL)
            {
                msg = "\nLagrangean convergence achieved in %d iterations\n";
                lp.report(lp, lp_lib.NORMAL, ref msg, citer);
                i = LpCls.check_solution(lp, lp.columns, lp.best_solution, lp.orig_upbo, lp.orig_lowbo, lp.epssolution);
            }
            else
            {
                msg = "\nUnsatisfactory convergence achieved over %d Lagrangean iterations.\n";
                lp.report(lp, lp_lib.NORMAL, ref msg, citer);
                if (AnyFeas)
                {
                    msg = "The best feasible Lagrangean objective function value was %g\n";
                    lp.report(lp, lp_lib.NORMAL, ref msg, lp.best_solution[0]);
                }
            }

            /* Restore the original objective function */
#if DirectOverrideOF
  set_OF_override(lp, null);
#else
            for (i = 1; i <= lp.columns; i++)
            {
                LpCls.set_mat(lp, 0, i, OrigObj[i]);
            }
#endif

            /* ... and then free memory */
            //NOT REQUIRED
            //FREE(BestFeasSol);
            //FREE(SubGrad);
            //FREE(OrigObj);
            //FREE(ModObj);
            LpCls.pop_basis(lp, false);

            lp.do_presolve = oldpresolve;

            return (lp.lag_status);

        }

        public static int heuristics(lprec lp, int mode)
        {
            lprec hlp;
            int status = lp_lib.PROCFAIL;
            LpCls objLpCls = new LpCls();

            if (lp.bb_level > 1)
            {
                return (status);
            }

            status = lp_lib.RUNNING;
            lp.bb_limitOF = lp_types.my_chsign(LpCls.is_maxim(lp), -lp.infinite);
            if (false && (lp.int_vars > 0))
            {

                /* 1. Copy the problem into a new relaxed instance, extracting Lagrangean constraints */
                hlp = make_lag(lp);

                /* 2. Run the Lagrangean relaxation */
                status = objLpCls.solve(hlp);

                /* 3. Copy the key results (bound) into the original problem */
                lp.bb_heuristicOF = Convert.ToDouble(hlp.best_solution[0]);

                /* 4. Delete the helper heuristic */
                hlp.matL = null;
                LpCls.delete_lp(hlp);
            }

            lp.timeheuristic = commonlib.timeNow();
            return (status);

        }

        internal static int lin_solve(lprec lp)
        {
            int status = lp_lib.NOTRUN;
            lp_report objlp_report = new lp_report();

            /* Don't do anything in case of an empty model */
            lp.lag_status = lp_lib.NOTRUN;
            /* if(get_nonzeros(lp) == 0) { */
            if (lp.columns == 0)
            {
                LpCls.default_basis(lp);
                lp.spx_status = lp_lib.NOTRUN;
                return (lp.spx_status);
            }

            /* Otherwise reset selected arrays before solving */
            LpCls.unset_OF_p1extra(lp);
            /*NOT REQUIRED
            LpCls.free_duals(lp);
            */
            /*NOT REQUIRED
            FREE(lp.drow);
            FREE(lp.nzdrow);
            
            if (lp.bb_cuttype != null)
            {
                freecuts_BB(lp);
            }
            */

            /* Reset/initialize timers */
            lp.timestart = DateTime.Now;
            lp.timeheuristic = 0;
            lp.timepresolved = 0;
            lp.timeend = 0;

            /* Do heuristics ahead of solving the model */
            if (heuristics(lp, lp_types.AUTOMATIC) != lp_lib.RUNNING)
            {
                return (lp_lib.INFEASIBLE);
            }

            /* Solve the full, prepared model */
            status = spx_solve(lp);
            if ((LpCls.get_Lrows(lp) > 0) && (lp.lag_status == lp_lib.NOTRUN))
            {
                if (status == lp_lib.OPTIMAL)
                {
                    status = lag_solve(lp, lp.bb_heuristicOF, lp_lib.DEF_LAGMAXITERATIONS);
                }
                else
                {
                    string msg = "\nCannot do Lagrangean optimization since root model was not solved.\n";
                    objlp_report.report(lp, lp_lib.IMPORTANT, ref msg);
                }
            }

            /* Reset heuristic in preparation for next run (if any) */
            lp.bb_heuristicOF = lp_types.my_chsign(LpCls.is_maxim(lp), lp.infinite);

            /* Check that correct status code is returned */
            /*
               peno 26.12.07
               status was not set to SUBOPTIMAL, only lp->spx_status
               Bug occured by a change in 5.5.0.10 when  && (lp->bb_totalnodes > 0) was added
               added status =
               See UnitTest3
            */
            /*
               peno 12.01.08
               If an integer solution is found with the same objective value as the relaxed solution then
               searching is stopped. This by setting lp->bb_break. However this resulted in a report of SUBOPTIMAL
               solution. For this,  && !bb_better(lp, OF_DUALLIMIT, OF_TEST_BE) is added in the test.
               See UnitTest20
            */
            if ((lp.spx_status == lp_lib.OPTIMAL) && (lp.bb_totalnodes > 0))
            {
                if (((lp.bb_break!=0) && !LpCls.bb_better(lp, lp_lib.OF_DUALLIMIT, lp_lib.OF_TEST_BE)))
                {
                    status = lp.spx_status = lp_lib.SUBOPTIMAL;
                }
            }
            return (status);
        }
    }
}
