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
            LpCls.set_action(ref lp.piv_strategy, lp_lib.PRICE_FORCEFULL);
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
                if (!lp.is_basic[i] && !lp_LUSOL.is_fixedvar(lp, i) && (System.Math.Abs((sbyte)prow[i]) > bestvalue))
                {
                    bestindex = i;
                    bestvalue = System.Math.Abs((sbyte)prow[i]);
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
            double epsvalue = new lprec().epsvalue; // Any larger value can produce a suboptimal result
            roundmode |= lp_matrix.MAT_ROUNDRC;

            if (isdual != null)
            {
                lp_matrix.bsolve_xA2(lp, ref coltarget[0], row_nr, ref prow[0], epsvalue, ref nzprow[0], 0, ref drow, epsvalue, ref nzdrow, roundmode);
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
                    double[] rhsvector = new double[bVector.Count];
                    for(int idx=0;idx<bVector.Count;idx++)
                    {
                        rhsvector[idx] = bVector[idx];
                    }
                    int? nzidx = (lp.bsolveIdx != null) ? Convert.ToInt32(lp.bsolveIdx) : 0;
                    lp_matrix.bsolve(lp, 0, ref rhsvector, ref nzidx, epsvalue * lp_lib.DOUBLEROUND, 1.0);
                    if (isdual == null && (row_nr == 0) && (lp.improve != 0 && lp_lib.IMPROVE_SOLUTION != 0) && !LpCls.refactRecent(lp) && serious_facterror(lp, ref rhsvector[0], lp.rows, new lprec().epsvalue))
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
                int nzoutput = (nzdrow != null) ? Convert.ToInt32(nzdrow[0]) : 0;
                lp_matrix.prod_xA(lp, ref coltarget[0], ref arrbVector[0], ref lp.bsolveIdx[0], epsvalue, 1.0, ref drow[0], ref nzoutput, roundmode);
            }
        }

        internal static int partial_blockStart(lprec lp, bool isrow)
        {
            partialrec blockdata;

            //NOTED ISSUE
            //RESOLVED ON 26/11/18
            blockdata = (partialrec)commonlib.IF(isrow, lp.rowblocks, lp.colblocks);
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
            blockdata = (partialrec)commonlib.IF(isrow, lp.rowblocks, lp.colblocks);
            if (blockdata == null)
            {
                return (Convert.ToInt32(commonlib.IF(isrow, lp.rows, lp.sum)));
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
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                ib = mat.col_end[j - 1][0];
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                ie = mat.col_end[j][0];
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

        /* Multiple pricing routines */
        internal static multirec multi_create(lprec lp, bool truncinf)
        {
            multirec multi;

            //C++ TO JAVA CONVERTER TODO TASK: The memory management function 'calloc' has no equivalent in Java:
            //C++ TO JAVA CONVERTER TODO TASK: There is no Java equivalent to 'sizeof':
            multi = new multirec();
            if (multi != null)
            {
                multi.active = 1;
                multi.lp = lp;
                multi.epszero = new lprec().epsprimal;
                multi.truncinf = truncinf;
            }

            return (multi);
        }

        internal static bool multi_resize(multirec multi, int blocksize, int blockdiv, bool doVlist, bool doIset)
        {
            bool ok = true;

            if ((blocksize > 1) && (blockdiv > 0))
            {
                int oldsize = multi.size;

                multi.size = blocksize;
                if (blockdiv > 1)
                {
                    multi.limit += (multi.size - oldsize) / blockdiv;
                }

                //ORIGINAL LINE: multi.items = (pricerec)realloc(multi.items, (multi.size + 1) * sizeof(*(multi.items)));
                multi.items = null;// (pricerec)realloc(multi.items, (multi.size + 1) * sizeof(*(multi.items)));
                //ORIGINAL LINE: multi.sortedList = (UNIONTYPE QSORTrec) realloc(multi.sortedList, (multi.size + 1) * sizeof(*(multi.sortedList)));
                multi.sortedList = null;

                //NOT REQUIRED
                //ok = (multi.items != null) && (multi.sortedList != null) && allocINT(multi.lp, (multi.freeList), multi.size + 1, AUTOMATIC);
                if (ok != null)
                {
                    int i;
                    int n;

                    if (oldsize == 0)
                    {
                        i = 0;
                    }
                    else
                    {
                        i = multi.freeList[0];
                    }
                    multi.freeList[0] = i + (multi.size - oldsize);
                    for (n = multi.size - 1, i++; i <= multi.freeList[0]; i++, n--)
                    {
                        multi.freeList[i] = n;
                    }
                }
                if (doVlist)
                {
                    //NOT REQUIRED
                    //ok &= allocREAL(multi.lp, (multi.valueList), multi.size + 1, AUTOMATIC);
                }
                if (doIset)
                {
                    //NOT REQUIRED
                    //ok &= allocINT(multi.lp, (multi.indexSet), multi.size + 1, AUTOMATIC);
                    if (ok != null && (oldsize == 0))
                    {
                        multi.indexSet[0] = 0;
                    }
                }
                if (ok == null)
                {

                    //ORIGINAL LINE: goto Undo;
                    multi.size = 0;
                }

            }
            else
            {
                Undo:
                multi.size = 0;
                //NOT REQUIRED
                //FREE(multi.items);
                //FREE(multi.valueList);
                //FREE(multi.indexSet);
                //FREE(multi.freeList);
                //FREE(multi.sortedList);
            }
            multi.active = 1;

            return (ok);
        }

        /* Find the primal simplex entering non-basic column variable */
        internal static int colprim(lprec lp, ref double[] drow, ref int[] nzdrow, bool skipupdate, int partialloop, ref int candidatecount, bool updateinfeas, ref double xviol)
        {
            int i;
            int ix;
            int iy;
            int iz = 0;
            int ninfeas;
            int nloop = 0;
            double f;
            double sinfeas;
            double xinfeas;
            double epsvalue = new lprec().epsdual;
            pricerec current = new pricerec();
            pricerec candidate = new pricerec();
            bool collectMP = false;

            //ORIGINAL LINE: int *coltarget = null;
            int[] coltarget = null;
            LpCls objLpCls = new LpCls();

            /* Identify pivot column according to pricing strategy; set
               entering variable initial threshold reduced cost value to "0" */
            current.pivot = new lprec().epsprimal; // Minimum acceptable improvement
            current.varno = 0;
            current.lp = lp;
            current.isdual = false;
            candidate.lp = lp;
            candidate.isdual = false;
            candidatecount = 0;

            /* Update local value of pivot setting and determine active multiple pricing set */
            lp._piv_rule_ = LpCls.get_piv_rule(lp);

            doLoop:
            nloop++;
            if ((lp.multivars != null) && ((lp.simplex_mode & lp_lib.SIMPLEX_PRIMAL_PRIMAL) != 0))
            {
                collectMP = multi_mustupdate(lp.multivars[0]);
                if (collectMP)
                {
                    multi_restart(lp.multivars[0]);
                    coltarget = null;
                }
                else
                {
                    coltarget = multi_indexSet(lp.multivars[0], false);
                }
            }

            /* Compute reduced costs c - c*Inv(B), if necessary
               (i.e. the previous iteration was not a "minor" iteration/bound flip) */
            if (!skipupdate)
            {
                ///#if UsePrimalReducedCostUpdate
                /* Recompute from scratch only at the beginning, otherwise update */
                if ((lp.current_iter > 0) && (LpCls.refactRecent(lp) == Convert.ToBoolean(DefineConstants.AUTOMATIC)))
                {

                    double[] Parameter1 = null;
                    int[] Parameter2 = null;
                    ///#endif
                    compute_reducedcosts(lp, false, 0, ref coltarget, (bool)((nloop <= 1) || (partialloop > 1)), ref Parameter1, ref Parameter2, ref drow, ref nzdrow, lp_matrix.MAT_ROUNDDEFAULT);
                }
            }

            /* Loop over active partial column set; we presume that reduced costs
               have only been updated for columns in the active partial range. */
            ix = 1;
            iy = nzdrow[0];
            ninfeas = 0;
            xinfeas = 0;
            sinfeas = 0;
            makePriceLoop(lp, ref ix, ref iy, ref iz);
            iy *= iz;
            for (; ix * iz <= iy; ix += iz)
            {
                i = nzdrow[ix];
                ///#if false
                //    if(i > lp->sum-abs(lp->P1extraDim))
                //      continue;
                ///#endif

                /* Check if the pivot candidate is on the block-list */
                if (lp.rejectpivot[0] > 0)
                {
                    int kk;
                    for (kk = 1; (kk <= lp.rejectpivot[0]) && (i != lp.rejectpivot[kk]); kk++)
                    {
                        ;
                    }
                    if (kk <= lp.rejectpivot[0])
                    {
                        continue;
                    }
                }

                /* Retrieve the applicable reduced cost - threshold should not be smaller than 0 */
                f = lp_types.my_chsign(lp.is_lower[i], drow[i]);
                if (f <= epsvalue)
                {
                    continue;
                }

                /* Find entering variable according to strategy (largest positive f) */
                ninfeas++;
                commonlib.SETMAX(Convert.ToInt32(xinfeas), Convert.ToInt32(f));
                sinfeas += f;
                candidate.pivot = normalizeEdge(lp, i, f, false);
                candidate.varno = i;
                if (findImprovementVar(current, candidate, collectMP, ref candidatecount))
                {
                    break;
                }
            }

            /* Check if we should loop again after a multiple pricing update */
            if (lp.multivars != null)
            {
                if (collectMP)
                {
                    if (!lp.multivars[0].sorted)
                    {
                        //commonlib.findCompare_func findcomp = compareImprovementQS(current,candidate);
                        //NOTED ISSUE
                        int Parameter = 0;
                        //FIX_bed35fa2-3644-4476-93eb-466009d2e532 24/12/18
                        lp.multivars[0].sorted = commonlib.QS_execute(lp.multivars[0].sortedList, lp.multivars[0].used, (commonlib.findCompare_func)compareImprovementQS , ref Parameter);
                        
                        //lp.multivars[0].sorted = commonlib.QS_execute(lp.multivars[0].sortedList, lp.multivars[0].used, findcomp, ref Parameter);
                    }
                    coltarget = multi_indexSet(lp.multivars[0], true);
                }
                else if ((current.varno == 0) && (lp.multivars[0].retries == 0))
                {
                    ix = partial_blockStart(lp, false);
                    iy = partial_blockEnd(lp, false);
                    lp.multivars[0].used = 0;
                    lp.multivars[0].retries++;

                    goto doLoop;
                }
                /* Shrink the candidate list */
                lp.multivars[0].retries = 0;
                if (current.varno != 0)
                {
                    multi_removevar(lp.multivars[0], current.varno);
                }
            }

            /* Check for optimality */
            if (xviol != null)
            {
                xviol = xinfeas;
            }
            if (updateinfeas)
            {
                lp.suminfeas = System.Math.Abs(sinfeas);
            }
            if ((lp.multivars == null) && (current.varno > 0) && !verify_stability(lp, true, xinfeas, sinfeas, ninfeas))
            {
                current.varno = 0;
            }

            /* Produce statistics */
            if (lp.spx_trace)
            {
                string msg = "";
                if (current.varno > 0)
                {
                    msg = "colprim: Column %d reduced cost = " + lp_types.RESULTVALUEMASK + "\n";
                    lp.report(lp, lp_lib.DETAILED, ref msg, current.varno, current.pivot);
                }
                else
                {
                    msg = "colprim: No positive reduced costs found, optimality!\n";
                    lp.report(lp, lp_lib.DETAILED, ref msg);
                }
            }

            return (current.varno);
        } // colprim

        internal static bool multi_mustupdate(multirec multi)
        {
            return ((bool)((multi != null) && (multi.used < multi.limit)));
        }

        internal static int multi_restart(multirec multi)
        {
            int i;
            int n = multi.used;

            multi.used = 0;
            multi.sorted = false;
            multi.dirty = false;
            if (multi.freeList != null)
            {
                for (i = 1; i <= multi.size; i++)
                {
                    multi.freeList[i] = multi.size - i;
                }
                multi.freeList[0] = multi.size;
            }
            ///#if false
            //  if(multi->indexSet != NULL)
            //    multi->indexSet[0] = 0;
            ///#endif
            return (n);
        }

        internal static int[] multi_indexSet(multirec multi, bool regenerate)
        {
            if (regenerate != null)
            {
                int[] parameter = null;
                multi_populateSet(multi, ref parameter, -1);
            }
            return (multi.indexSet);
        }
        internal static int multi_populateSet(multirec multi, ref int[] list, int excludenr)
        {
            int n = 0;
            if (list == null)
            {
                list = (multi.indexSet);
            }
            //ORIGINAL LINE:if ((multi.used > 0) && ((list[0] != null) || allocINT(multi.lp, list, multi.size + 1, 0)))
            if ((multi.used > 0))
            {
                int i;
                int colnr;

                for (i = 0; i < multi.used; i++)
                {
                    colnr = ((pricerec)(multi.sortedList[i].pvoidreal.ptr)).varno;
                    if ((colnr != excludenr) && ((excludenr > 0) && (multi.lp.upbo[colnr] < multi.lp.infinite)))
                    {
                        n++;
                        list[n] = colnr;
                    }
                }
                list[0] = n;
            }
            return (n);
        }

        /* Function to provide for left-right or right-left scanning of entering/leaving
             variables; note that *end must have been initialized by the calling routine! */
        internal static void makePriceLoop(lprec lp, ref int start, ref int end, ref int delta)
        {
            LpCls objLpCls = new LpCls();
            int offset = Convert.ToInt32(objLpCls.is_piv_mode(lp, lp_lib.PRICE_LOOPLEFT));

            if ((offset) != 0 || (((lp.total_iter + offset) % 2 == 0) && objLpCls.is_piv_mode(lp, lp_lib.PRICE_LOOPALTERNATE)))
            {
                delta = -1; // Step backwards - "left"
                lp_utils.swapINT(ref start, ref end);
                lp._piv_left_ = true;
            }
            else
            {
                delta = 1; // Step forwards - "right"
                lp._piv_left_ = false;
            }
        }

        internal static double normalizeEdge(lprec lp, int item, double edge, bool isdual)
        {
            ///#if 1
            /* Don't use the pricer "close to home", since this can possibly
              worsen the final feasibility picture (mainly a Devex issue?) */
            if (System.Math.Abs(edge) > lp.epssolution)
            {
                ///#endif
                edge /= LpPricePSE.getPricer(lp, item, Convert.ToByte(isdual));
            }
            if ((lp.piv_strategy & lp_lib.PRICE_RANDOMIZE) != 0)
            {
                edge *= (1.0 - lp_lib.PRICER_RANDFACT) + lp_lib.PRICER_RANDFACT * lp_utils.rand_uniform(lp, 1.0);
            }
            return (edge);

        }

        internal static bool findImprovementVar(pricerec current, pricerec candidate, bool collectMP, ref int candidatecount)
        {
            /* PRIMAL: Find a variable to enter the basis
           DUAL:   Find a variable to leave the basis
           Allowed variable set: Any pivot PRIMAL:larger or DUAL:smaller than threshold value of 0 */

            bool Action = false,
            ///#if ExtractedValidityTest
		    Accept = true;
            ///#else
            Accept = validImprovementVar(candidate);
            ///#endif
            if (Accept)
            {
                if (candidatecount != null)
                {
                    (candidatecount)++;
                }
                if (collectMP != null)
                {
                    //NOTED ISSUE
                    if (addCandidateVar(candidate, current.lp.multivars[0], (commonlib.findCompare_func)compareImprovementQS, false) < 0)
                    {
                        return (Action);
                    }
                }
                if (current.varno > 0)
                {
                    Accept = (bool)(compareImprovementVar(current, candidate) > 0);
                }
            }

            /* Apply candidate if accepted */
            if (Accept)
            {
                //ORIGINAL LINE: (*current) = *candidate;
                current = candidate;

                /* Force immediate acceptance for Bland's rule using the primal simplex */
                if (!candidate.isdual)
                {
                    Action = (bool)(candidate.lp._piv_rule_ == lp_lib.PRICER_FIRSTINDEX);
                }
            }
            return (Action);
        }

        internal static bool validImprovementVar(pricerec candidate)
        {
            /* Validity operators for entering and leaving columns for both the primal and dual
               simplex.  All candidates must satisfy these tests to qualify to be allowed to be
               a subject for the comparison functions/operators. */

            //ORIGINAL LINE: register REAL candidatepivot = fabs(candidate->pivot);
            double candidatepivot = System.Math.Abs(candidate.pivot);

            ///#if Paranoia
            return ((bool)((candidate.varno > 0) && (candidatepivot > new lprec().epsvalue)));
            ///#else
            return ((bool)(candidatepivot > new lprec().epsvalue));
            ///#endif
        }

        /* Function to add a valid pivot candidate into the specified list */
        internal static int addCandidateVar(pricerec candidate, multirec multi, commonlib.findCompare_func findCompare, bool allowSortedExpand)
        {
            int insertpos = 0;
            int delta = 1;
            pricerec targetrec;
            lprec lp = new lprec();

            /* Find the insertion point (if any) */
            if ((multi.freeList[0] == 0) || (multi.sorted && allowSortedExpand != null) || (candidate.isdual && (multi.used == 1) && ((multi.step_last >= multi.epszero) || multi_truncatingvar(multi, ((pricerec)(multi.sortedList[0].pvoidreal.ptr)).varno))))
            {
                QSORTrec searchTarget = new QSORTrec();

                /* Make sure that the list is sorted before the search for an insertion point */
                if ((multi.freeList[0] == 0) && !multi.sorted)
                {
                    multi.sorted = commonlib.QS_execute(multi.sortedList, multi.used, findCompare, ref insertpos);
                    multi.dirty = (bool)(insertpos > 0);
                }

                /* Perform the search */
                searchTarget.pvoidint2.ptr = (Object)candidate;
                //ORIGINAL CODE: insertpos = sizeof(searchTarget);
                insertpos = 0;
                insertpos = commonlib.findIndexEx(searchTarget, multi.sortedList[delta], multi.used, delta, insertpos, findCompare, 1);
                if (insertpos > 0)
                {
                    return (-1);
                }
                insertpos = -insertpos - delta;

                /* Check if the candidate is worse than the worst of the list */
                if (((insertpos >= multi.size) && (multi.freeList[0] == 0)) || ((insertpos == multi.used) && (allowSortedExpand == null || (multi.step_last >= multi.epszero))))
                {
                    return (-1);
                }

                ///#if Paranoia
                /* Do validation */
                if ((insertpos < 0) || (insertpos > multi.used))
                {
                    return (-1);
                }
                ///#endif

                /* Define the target for storing the candidate;
                   Case 1: List is full and we must discard the previously worst candidate
                   Case 2: List is not full and we simply use the next free position */
                if (multi.freeList[0] == 0)
                {
                    targetrec = (pricerec)multi.sortedList[multi.used - 1].pvoidreal.ptr;
                }
                else
                {
                    delta = multi.freeList[0]--;
                    delta = multi.freeList[delta];
                    targetrec = (multi.items[delta]);
                }
            }
            else
            {
                delta = multi.freeList[0]--;
                delta = multi.freeList[delta];
                targetrec = (multi.items[delta]);
                insertpos = multi.used;
            }

            /* Insert the new candidate record in the data store */
            //NOT REQUIRED
            //commonlib.MEMCOPY(targetrec, candidate, 1);

            /* Store the pointer data and handle tree cases:
               Case 1: The list is unsorted and not full; simply append pointer to list,
               Case 2: The list is sorted and full; insert the pointer by discarding previous last,
               Case 3: The list is sorted and not full; shift the inferior items down, and increment count */
            if ((multi.used < multi.size) && (insertpos >= multi.used))
            {
                commonlib.QS_append(multi.sortedList, insertpos, targetrec);
                multi.used++;
            }
            else
            {
                if (multi.used == multi.size)
                {
                    commonlib.QS_insert(multi.sortedList, insertpos, targetrec, multi.size - 1); // Discard previous last
                }
                else
                {
                    commonlib.QS_insert(multi.sortedList, insertpos, targetrec, multi.used); // Keep previous last
                    multi.used++;
                }
            }
            multi.active = insertpos;

            ///#if Paranoia
            if ((insertpos >= multi.size) || (insertpos >= multi.used))
            {
                string msg = "addCandidateVar: Insertion point beyond limit!\n";
                lp.report(multi.lp, lp_lib.SEVERE, ref msg);
            }
            ///#endif

            return (insertpos);
        }

        internal static bool multi_truncatingvar(multirec multi, int varnr)
        {
            LpCls objLpCls = new LpCls();
            return (multi.truncinf && LpCls.is_infinite(multi.lp, multi.lp.upbo[varnr]));
        }

        /// <summary> FIX_bed35fa2-3644-4476-93eb-466009d2e532 24/12/18
        /// PREVIOUS: internal static int compareImprovementQS(QSORTrec current, QSORTrec candidate)
        /// {
        ///     return (compareImprovementVar((pricerec)current.pvoidint2.ptr, (pricerec)candidate.pvoidint2.ptr));
        /// }
        /// ERROR IN PREVIOUS: Cannot convert type 'method' to 'commonlib.findCompare_func'
        /// FIX 1: changed to current method definition
        /// </summary>
        internal static int compareImprovementQS(params object[] current)
        {
            if (current.Length == 2)
                return (compareImprovementVar((pricerec)(current[0] as QSORTrec).pvoidint2.ptr, (pricerec)(current[1] as QSORTrec).pvoidint2.ptr));
            else
                return 0;
        }
        internal static int compareImprovementVar(pricerec current, pricerec candidate)
        {
            /* Comparison operators for entering and leaving variables for both the primal and
                dual simplexes.  The functions compare a candidate variable with an incumbent. */

            //ORIGINAL LINE: register int result = COMP_PREFERNONE;
            int result = lp_types.COMP_PREFERNONE;
            //ORIGINAL LINE: register lprec *lp = current->lp;
            lprec lp = current.lp;
            //ORIGINAL LINE: register REAL testvalue, margin = PREC_IMPROVEGAP;
            double testvalue = new double();
            double margin = new lp_lib().PREC_IMPROVEGAP;
            int currentcolno;
            int currentvarno = current.varno;
            int candidatecolno;
            int candidatevarno = candidate.varno;
            bool isdual = candidate.isdual;
            LpCls objLpCls = new LpCls();

            if (isdual != null)
            {
                candidatevarno = lp.var_basic[candidatevarno];
                currentvarno = lp.var_basic[currentvarno];
            }
            candidatecolno = candidatevarno - lp.rows;
            currentcolno = currentvarno - lp.rows;

            /* Do pivot-based selection unless Bland's (first index) rule is active */
            if (lp._piv_rule_ != lp_lib.PRICER_FIRSTINDEX)
            {

                bool candbetter = new bool();

                /* Find the largest value - normalize in case of the dual, since
                   constraint violation is expressed as a negative number. */
                /* Use absolute test for "small numbers", relative otherwise */
                testvalue = candidate.pivot;
                if (System.Math.Abs(testvalue) < lp_lib.LIMIT_ABS_REL)
                {
                    testvalue -= current.pivot;
                }
                else
                {
                    testvalue = lp_types.my_reldiff(testvalue, current.pivot);
                }
                if (isdual != null)
                {
                    //ORIGINAL LINE: testvalue = -testvalue;
                    testvalue = (-testvalue);
                }

                //ORIGINAL LINE: candbetter = (MYBOOL)(testvalue > 0);
                candbetter = ((bool)(testvalue > 0));
                if (candbetter != null)
                {
                    if (testvalue > margin)
                    {
                        result = lp_types.COMP_PREFERCANDIDATE;
                    }
                }

                ///#if false
                //    else if (testvalue < -margin)
                ///#else
                else if (testvalue < new lprec().epsvalue)
                    ///#endif
                    result = lp_types.COMP_PREFERINCUMBENT;

                ///#if UseSortOnBound
                /* Extra selection criterion based on the variable's range;
                  variable with - DUAL: small bound out; PRIMAL: large bound in */
                if (result == lp_types.COMP_PREFERNONE)
                {
                    testvalue = lp.upbo[candidatevarno] - lp.upbo[currentvarno];
                    if (testvalue < -margin)
                    {
                        result = lp_types.COMP_PREFERINCUMBENT;
                    }
                    else if (testvalue > margin)
                    {
                        result = lp_types.COMP_PREFERCANDIDATE;
                    }
                    result = Convert.ToInt32(lp_types.my_chsign(isdual, result));
                }
                ///#endif

                ///#if UseSortOnColumnLength
                /* Prevent long columns from entering the basis */
                if (result == lp_types.COMP_PREFERNONE)
                {
                    if (candidatecolno > 0)
                    {
                        testvalue = lp_matrix.mat_collength(lp.matA, candidatecolno) + (objLpCls.is_obj_in_basis(lp) && (lp.obj[candidatecolno] != 0) ? 1 : 0);
                    }
                    else
                    {
                        testvalue = 1;
                    }
                    if (currentcolno > 0)
                    {
                        testvalue -= lp_matrix.mat_collength(lp.matA, currentcolno) + (objLpCls.is_obj_in_basis(lp) && (lp.obj[currentcolno] != 0) ? 1 : 0);
                    }
                    else
                    {
                        testvalue -= 1;
                    }
                    if (testvalue > 0)
                    {
                        result = lp_types.COMP_PREFERINCUMBENT;
                    }
                    else if (testvalue < 0)
                    {
                        result = lp_types.COMP_PREFERCANDIDATE;
                    }
                    result = Convert.ToInt32(lp_types.my_chsign(isdual, result));
                }
                ///#endif

                /* Select absolute best if the non-primary criteria failed to separate */
                if ((result == lp_types.COMP_PREFERNONE) && candbetter != null)
                {
                    result = lp_types.COMP_PREFERCANDIDATE;

                    goto Finish;
                }
            }

            /* Final tie-breakers */
            if (result == lp_types.COMP_PREFERNONE)
            {

                /* Add randomization tie-braker */
                if ((lp.piv_strategy & lp_lib.PRICE_RANDOMIZE) != 0)
                {
                    result = Convert.ToInt32(lp_types.my_sign(lp_lib.PRICER_RANDFACT - lp_utils.rand_uniform(lp, 1.0)));
                    if (candidatevarno < currentvarno)
                    {
                        result = -result;
                    }
                }

                /* Resolve ties via index ordinal */
                if (result == lp_types.COMP_PREFERNONE)
                {
                    if (candidatevarno < currentvarno)
                    {
                        result = lp_types.COMP_PREFERCANDIDATE;
                    }
                    else // if(candidatevarno > currentvarno)
                    {
                        result = lp_types.COMP_PREFERINCUMBENT;
                    }
                    if (lp._piv_left_)
                    {
                        result = -result;
                    }
                }
            }
            Finish:
            return (result);

        }

        internal static bool multi_removevar(multirec multi, int varnr)
        {
            int i = 1;
            int[] coltarget = multi.indexSet;

            if (coltarget == null)
            {
                return (false);
            }

            while ((i <= multi.used) && (coltarget[i] != varnr))
            {
                i++;
            }
            if (i > multi.used)
            {
                return (false);
            }

            for (; i < multi.used; i++)
            {
                coltarget[i] = coltarget[i + 1];
            }
            coltarget[0]--;
            multi.used--;
            multi.dirty = true;
            return (true);
        }

        /* Primal: Prevent acceptance of an entering variable when the magnitude of
                   other candidates is also very small.
           Dual:   Prevent acceptance of a leaving variable when the magnitude of
                   other candidates is also very small.

           Both of these cases are associated with numerical stalling, which we could
           argue should be detected and handled by the stalling monitor routine. */
        internal static bool verify_stability(lprec lp, bool isprimal, double xfeas, double sfeas, int nfeas)
        {
            bool testOK = true;
            return (testOK);

            ///#if 1
            /* Try to make dual feasibility as tight as possible */
            if (isprimal == null)
            /*  if(lp->P1extraVal == 0) */
            {
                xfeas /= (1 + lp.rhsmax);
                sfeas /= (1 + lp.rhsmax);
            }
            ///#endif
            xfeas = System.Math.Abs(xfeas); // Maximum (positive) infeasibility
                                            /*  if(xfeas < lp->epspivot) { */
            if (xfeas < lp.epssolution)
            {
                double f;
                sfeas = System.Math.Abs(sfeas); // Make sum of infeasibilities positive
                xfeas = (sfeas - xfeas) / nfeas; // Average "residual" feasibility
                f = 1 + System.Math.Log10((double)nfeas); // Some numerical complexity scalar
                                                          /* Numerical errors can interact to cause non-convergence, and the
                                                            idea is to relax the tolerance to account for this and only
                                                            marginally weakening the (user-specified) tolerance. */
                if ((sfeas - xfeas) < f * new lprec().epsprimal)
                {
                    testOK = false;
                }
            }
            return (testOK);
        }

        internal static void multi_free(multirec[] multi)
        {
            if ((multi == null) || (multi[0] == null))
            {
                return;
            }
            /*NOT REQUIRED
            FREE(multi.items);
            FREE(multi.valueList);
            FREE(multi.indexSet);
            FREE(multi.freeList);
            FREE(multi.sortedList);
            FREEmulti;
            */
        }


        internal static int partial_countBlocks(lprec lp, bool isrow)
        {
            partialrec blockdata = (partialrec)commonlib.IF(isrow, lp.rowblocks, lp.colblocks);

            if (blockdata == null)
            {
                return (1);
            }
            else
            {
                return (blockdata.blockcount);
            }
        }

        internal static bool partial_blockStep(lprec lp, bool isrow)
        {
            partialrec blockdata;

            blockdata = (partialrec)commonlib.IF(isrow, lp.rowblocks, lp.colblocks);
            if (blockdata == null)
            {
                return (false);
            }
            else if (blockdata.blocknow < blockdata.blockcount)
            {
                blockdata.blocknow++;
                return (true);
            }
            else
            {
                blockdata.blocknow = 1;
                return (true);
            }
        }

        /* Find the primal simplex leaving basic column variable */
        internal static int rowprim(lprec lp, int colnr, ref double theta, ref double[] pcol, ref int? nzpcol, bool forceoutEQ, ref double xviol)
        {
            int i;
            int ii;
            int iy;
            int iz = 0;
            int Hpass;
            int k;
            int[] nzlist = null;
            double f = new double();
            double savef = new double();
            double Heps = new double();
            double Htheta = new double();
            double Hlimit = new double();
            double epsvalue = new double();
            double epspivot = new double();
            double p = 0.0;
            pricerec current = new pricerec();
            pricerec candidate = new pricerec();
            bool isupper = !lp.is_lower[colnr];
            bool HarrisTwoPass = false;
            LpCls objLpCls = new LpCls();
            string msg = "";

            /* Update local value of pivot setting */
            lp._piv_rule_ = LpCls.get_piv_rule(lp);
            if (nzpcol == null)
            {
                int id = 0;
                bool res = int.TryParse(lp_utils.mempool_obtainVector(lp.workarrays, lp.rows + 1, System.Runtime.InteropServices.Marshal.SizeOf(nzlist)), out id);
                /*if(res)
                    nzlist[0] = */
            }
            else
                nzlist[0] = (nzpcol != null) ? Convert.ToInt32(nzpcol) : 0;

            /* Find unconditional non-zeros and optionally compute relative size of epspivot */
            epspivot = lp.epspivot;
            epsvalue = new lprec().epsvalue;
            Hlimit = 0;
            Htheta = 0;
            k = 0;
            for (i = 1; i <= lp.rows; i++)
            {
                p = System.Math.Abs(Convert.ToSByte(pcol));
                if (p > Hlimit)
                {
                    //ORIGINAL LINE: Hlimit = p;
                    Hlimit = p;
                }
                if (p > epsvalue)
                {
                    k++;
                    //ORIGINAL LINE: nzlist[k] = i;
                    nzlist[k] = i;
                    commonlib.SETMAX(Convert.ToInt32(Htheta), Convert.ToInt32(p));
                }

                ///#if Paranoia
                else
                {
                    if (lp.spx_trace)
                    {
                        msg = "rowprim: Row %d with pivot " + lp_types.RESULTVALUEMASK + " rejected as too small\n";
                        lp.report(lp, lp_lib.FULL, ref msg, i, p);
                    }
                }
                ///#endif
            }
            if (xviol != null)
            {
                xviol = Htheta;
            }
            Htheta = 0;

            /* Update non-zero list based on the new pivot threshold */
            ///#if UseRelativePivot_Primal
            /*  epspivot *= sqrt(lp->matA->dynrange) / lp->matA->infnorm; */
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            epspivot /= commonlib.MAX(1, System.Math.Sqrt(lp.matA.colmax[colnr][0]));
            iy = k;
            k = 0;
            p = 0;
            for (ii = 1; ii <= iy; ii++)
            {
                i = nzlist[ii];
                p = System.Math.Abs(Convert.ToSByte(pcol));

                /* Compress the list of valid alternatives, if appropriate */
                if (p > epspivot)
                {
                    k++;
                    nzlist[k] = i;
                }

                ///#if Paranoia
                else
                {
                    if (lp.spx_trace)
                    {
                        msg = "rowprim: Row %d with pivot " + lp_types.RESULTVALUEMASK + " rejected as too small\n";
                        lp.report(lp, lp_lib.FULL, ref msg, i, p);
                    }
                }
                ///#endif
            }
            ///#endif

            /* Initialize counters */
            nzlist[0] = k;
            k = 0;

            Retry:
            k++;
            HarrisTwoPass = objLpCls.is_piv_mode(lp, lp_lib.PRICE_HARRISTWOPASS);
            if (HarrisTwoPass)
            {
                Hpass = 1;
            }
            else
            {
                Hpass = 2;
            }
            current.theta = lp.infinite;
            current.pivot = 0;
            current.varno = 0;
            current.isdual = false;
            current.epspivot = epspivot;
            current.lp = lp;
            candidate.epspivot = epspivot;
            candidate.isdual = false;
            candidate.lp = lp;
            savef = 0;
            for (; Hpass <= 2; Hpass++)
            {
                Htheta = lp.infinite;
                if (Hpass == 1)
                {
                    Hlimit = lp.infinite; // Don't apply any limit in the first pass
                    Heps = epspivot / new lprec().epsprimal; // Scaled to lp->epsprimal used in compute_theta()
                }
                else
                {
                    Hlimit = current.theta; // This is the smallest Theta of the first pass
                    Heps = 0.0;
                }
                current.theta = lp.infinite;
                current.pivot = 0;
                current.varno = 0;
                savef = 0;

                ii = 1;
                iy = nzlist[0];
                makePriceLoop(lp, ref ii, ref iy, ref iz);
                iy *= iz;
                for (; ii * iz <= iy; ii += iz)
                {
                    i = nzlist[ii];
                    f = pcol[0];
                    candidate.theta = f;
                    candidate.pivot = f;
                    candidate.varno = i;

                    /*i =*/
                    LpCls.compute_theta(lp, i, ref candidate.theta, isupper, (double)lp_types.my_if(lp.upbo[lp.var_basic[i]] < new lprec().epsprimal, Heps / 10, Heps), true);

                    if (System.Math.Abs(candidate.theta) >= lp.infinite)
                    {
                        savef = f;
                        candidate.theta = 2 * lp.infinite;
                        continue;
                    }

                    /* Find the candidate leaving variable according to strategy (smallest theta) */
                    if ((Hpass == 2) && (candidate.theta > Hlimit))
                    {
                        continue;
                    }

                    /* Give a slight preference to fixed variables (mainly equality slacks) */
                    if (forceoutEQ)
                    {
                        p = candidate.pivot;
                        if (lp.upbo[lp.var_basic[i]] < new lprec().epsprimal)
                        {
                            /* Give an extra early boost to equality slack elimination, if specified */
                            if (forceoutEQ == Convert.ToBoolean(DefineConstants.AUTOMATIC))
                            {
                                candidate.pivot *= 1.0 + lp.epspivot;
                            }
                            else
                            {
                                candidate.pivot *= 10.0;
                            }

                        }
                    }
                    if (HarrisTwoPass)
                    {
                        f = candidate.theta;
                        if (Hpass == 2)
                        {
                            candidate.theta = 1;
                        }
                        int Parameter = 0;
                        if (findSubstitutionVar(current, candidate, ref Parameter))
                        {
                            break;
                        }
                        if ((Hpass == 2) && (current.varno == candidate.varno))
                        {
                            Htheta = f;
                        }
                    }
                    else
                    {
                        int Parameter = 0;
                        if (findSubstitutionVar(current, candidate, ref Parameter))
                        {
                            break;
                        }
                    }
                    /* Restore temporarily modified pivot */
                    if (forceoutEQ && (current.varno == candidate.varno))
                    {
                        current.pivot = p;
                    }
                }
            }
            if (HarrisTwoPass)
            {
                current.theta = Htheta;
            }

            /* Handle case of no available leaving variable */
            if (current.varno == 0)
            {
                if (lp.upbo[colnr] >= lp.infinite)
                {
                    /* Optionally try again with reduced pivot threshold level */
                    if (k < 2)
                    {
                        epspivot = epspivot / 10;

                        goto Retry;
                    }
                }
                else
                {
                    ///#if 1
                    i = 1;
                    while ((pcol[0] >= 0) && (i <= lp.rows))
                    {
                        i++;
                    }
                    if (i > lp.rows)
                    { // Empty column with upper bound!
                        lp.is_lower[colnr] = !lp.is_lower[colnr];
                        /*        lp->is_lower[colnr] = FALSE; */
                        lp.rhs[0] += lp.upbo[colnr] * pcol[0];
                    }
                    else // if(pcol[i]<0)
                    {
                        current.varno = i;
                    }
                    ///#endif
                }
            }
            else if (current.theta >= lp.infinite)
            {
                msg = "rowprim: Numeric instability pcol[%d] = %g, rhs[%d] = %g, upbo = %g\n";
                lp.report(lp, lp_lib.IMPORTANT, ref msg, current.varno, savef, current.varno, lp.rhs[current.varno], lp.upbo[lp.var_basic[current.varno]]);
            }

            /* Return working array to pool */
            if (nzpcol == null)
            {
                string memvector = nzlist.ToString();
                lp_utils.mempool_releaseVector(lp.workarrays, ref memvector, 0);
            }

            if (lp.spx_trace)
            {
                msg = "row_prim: %d, pivot size = " + lp_types.RESULTVALUEMASK + "\n";
                lp.report(lp, lp_lib.DETAILED, ref msg, current.varno, current.pivot);
            }

            /*  *theta = current.theta; */
            theta = System.Math.Abs(current.theta);

            return (current.varno);

        }

        internal static bool findSubstitutionVar(pricerec current, pricerec candidate, ref int candidatecount)
        {
            /* PRIMAL: Find a variable to leave the basis
           DUAL:   Find a variable to enter the basis
           Allowed variable set: "Equal-valued" smallest thetas! */


            bool Action = false,
            ///#if ExtractedValidityTest
		    Accept = true;
            ///#else
            Accept = validSubstitutionVar(candidate);
            ///#endif
            if (Accept)
            {
                if (candidatecount != null)
                {
                    (candidatecount)++;
                }
                if (current.varno != 0)
                {
                    Accept = (bool)(compareSubstitutionVar(current, candidate) > 0);
                }
            }

            /* Apply candidate if accepted */
            if (Accept)
            {
                //ORIGINAL LINE: (*current) = *candidate;
                current = (candidate);

                /* Force immediate acceptance for Bland's rule using the dual simplex */
                ///#if ForceEarlyBlandRule
                if (candidate.isdual)
                {
                    Action = (bool)(candidate.lp._piv_rule_ == lp_lib.PRICER_FIRSTINDEX);
                }
                ///#endif
            }
            return (Action);
        }

        internal static bool validSubstitutionVar(pricerec candidate)
        {
            //ORIGINAL LINE: register lprec *lp = candidate->lp;
            lprec lp = candidate.lp;

            //ORIGINAL LINE: register REAL theta = (candidate->isdual ? fabs(candidate->theta) : candidate->theta);
            double theta = (candidate.isdual ? System.Math.Abs(candidate.theta) : candidate.theta);

            ///#if Paranoia
            if (candidate.varno <= 0)
            {
                return (false);
            }
            else
            {
                ///#endif
                if (System.Math.Abs(candidate.pivot) >= lp.infinite)
                {
                    return ((bool)(theta < lp.infinite));
                }
                else
                {
                    return ((bool)((theta < lp.infinite) && (System.Math.Abs(candidate.pivot) >= candidate.epspivot)));
                }
            }
        }

        private static int compareSubstitutionVar(pricerec current, pricerec candidate)
        {
            //ORIGINAL LINE: register int result = COMP_PREFERNONE;
            int result = lp_types.COMP_PREFERNONE;

            //ORIGINAL LINE: register lprec *lp = current->lp;
            lprec lp = current.lp;

            //ORIGINAL LINE: register REAL testvalue = candidate->theta, margin = current->theta;
            double testvalue = candidate.theta;
            double margin = current.theta;
            bool isdual = candidate.isdual;
            bool candbetter = new bool();
            int currentcolno;
            int currentvarno = current.varno;
            int candidatecolno;
            int candidatevarno = candidate.varno;
            LpCls objLpCls = new LpCls();

            if (isdual == null)
            {
                candidatevarno = lp.var_basic[candidatevarno];
                currentvarno = lp.var_basic[currentvarno];
            }
            candidatecolno = candidatevarno - lp.rows;
            currentcolno = currentvarno - lp.rows;

            /* Compute the ranking test metric. */
            if (isdual != null)
            {
                testvalue = System.Math.Abs(testvalue);
                margin = System.Math.Abs(margin);
            }

            /* Use absolute test for "small numbers", relative otherwise */
            if (System.Math.Abs(testvalue) < lp_lib.LIMIT_ABS_REL)
            {
                testvalue -= margin;
            }
            else
            {
                testvalue = lp_types.my_reldiff(testvalue, margin);
            }

            /* Find if the new Theta is smaller or near equal (i.e. testvalue <= eps)
               compared to the previous best; ties will be broken by pivot size or index
               NB! The margin below is essential in maintaining primal/dual feasibility
                   during the primal/dual simplex, respectively.  Sometimes a small
                   value prevents the selection of a suitable pivot, thereby weakening
                   the numerical stability of some models */
            margin = new lp_lib().PREC_SUBSTFEASGAP;

            //ORIGINAL LINE: candbetter = (MYBOOL)(testvalue < 0);
            candbetter = ((bool)(testvalue < 0));
            if (candbetter != null)
            {
                if (testvalue < -margin)
                {
                    result = lp_types.COMP_PREFERCANDIDATE;
                }
            }
            else if (testvalue > margin)
            {
                result = lp_types.COMP_PREFERINCUMBENT;
            }

            /* Resolve a tie */
            if (result == lp_types.COMP_PREFERNONE)
            {
                double currentpivot = System.Math.Abs(current.pivot);
                double candidatepivot = System.Math.Abs(candidate.pivot);

                /* Handle first index / Bland's rule specially */
                if (lp._piv_rule_ == lp_lib.PRICER_FIRSTINDEX)
                {
                    ///#if 1
                    /* Special secondary selection by pivot size (limited stability protection) */
                    margin = candidate.epspivot;
                    if ((candidatepivot >= margin) && (currentpivot < margin))
                    {
                        result = lp_types.COMP_PREFERCANDIDATE;
                    }
                    ///#endif
                }

                else
                {

                    /* General secondary selection based on pivot size */
                    ///#if false
                    //      if(candidatepivot > MIN_STABLEPIVOT)
                    //        testvalue = my_reldiff(testvalue, currentpivot);
                    //      else
                    ///#endif
                    //ORIGINAL LINE: testvalue = candidatepivot - currentpivot;
                    testvalue = candidatepivot - currentpivot;
                    if (testvalue > margin)
                    {
                        result = lp_types.COMP_PREFERCANDIDATE;
                    }
                    else if (testvalue < -margin)
                    {
                        result = lp_types.COMP_PREFERINCUMBENT;
                    }

                    ///#if UseSortOnBound
                    /* Extra selection criterion based on the variable's range;
                      variable with - PRIMAL: small bound out; DUAL: large bound in */
                    if (result == lp_types.COMP_PREFERNONE)
                    {
                        testvalue = lp.upbo[candidatevarno] - lp.upbo[currentvarno];
                        if (testvalue < -margin)
                        {
                            result = lp_types.COMP_PREFERCANDIDATE;
                        }
                        else if (testvalue > margin)
                        {
                            result = lp_types.COMP_PREFERINCUMBENT;
                        }
                        result = Convert.ToInt32(lp_types.my_chsign(isdual, result));
                    }
                    ///#endif

                    ///#if UseSortOnColumnLength
                    /* Prevent long columns from entering the basis */
                    if (result == lp_types.COMP_PREFERNONE)
                    {
                        if (candidatecolno > 0)
                        {
                            testvalue = lp_matrix.mat_collength(lp.matA, candidatecolno) + (objLpCls.is_obj_in_basis(lp) && (lp.obj[candidatecolno] != 0) ? 1 : 0);
                        }
                        else
                        {
                            testvalue = 1;
                        }
                        if (currentcolno > 0)
                        {
                            testvalue -= lp_matrix.mat_collength(lp.matA, currentcolno) + (objLpCls.is_obj_in_basis(lp) && (lp.obj[currentcolno] != 0) ? 1 : 0);
                        }
                        else
                        {
                            testvalue -= 1;
                        }
                        if (testvalue > 0)
                        {
                            result = lp_types.COMP_PREFERCANDIDATE;
                        }
                        else if (testvalue < 0)
                        {
                            result = lp_types.COMP_PREFERINCUMBENT;
                        }
                        result = Convert.ToInt32(lp_types.my_chsign(isdual, result));
                    }
                    ///#endif

                }
            }

            /* Select absolute best if the non-primary criteria failed to separate */
            if ((result == lp_types.COMP_PREFERNONE) && candbetter)
            {
                result = lp_types.COMP_PREFERCANDIDATE;

                goto Finish;
            }

            /* Final tie-breakers */
            if (result == lp_types.COMP_PREFERNONE)
            {

                /* Add randomization tie-braker */
                if ((lp.piv_strategy & lp_lib.PRICE_RANDOMIZE) != 0)
                {
                    result = (int)lp_types.my_sign(lp_lib.PRICER_RANDFACT - lp_utils.rand_uniform(lp, 1.0));
                    if (candidatevarno < currentvarno)
                    {
                        result = -result;
                    }
                }

                /* Resolve ties via index ordinal (also prefers slacks over user variables) */
                if (result == lp_types.COMP_PREFERNONE)
                {
                    if (candidatevarno < currentvarno)
                    {
                        result = lp_types.COMP_PREFERCANDIDATE;
                    }
                    else // if(candidatevarno > currentvarno)
                    {
                        result = lp_types.COMP_PREFERINCUMBENT;
                    }
                    if (lp._piv_left_)
                    {
                        result = -result;
                    }
                }
            }
            Finish:
            return (result);
        }

        //Changed By: CS Date:28/11/2018
        /* Find the dual simplex leaving basic variable */
        internal static int rowdual(lprec lp, double[] rhvec, bool forceoutEQ, bool updateinfeas, ref double xviol)
        {
            int k;
            int i;
            int iy;
            int iz = 0;
            int ii;
            int ninfeas;
            //ORIGINAL LINE: register REAL rh;
            double rh = new double();
            double up;
            double lo = 0;
            double epsvalue;
            double sinfeas;
            double xinfeas;
            pricerec current = new pricerec();
            pricerec candidate = new pricerec();
            bool collectMP = false;
            LpCls objLpCls = new LpCls();
            string msg;

            /* Initialize */
            if (rhvec == null)
            {
                rhvec = lp.rhs;
            }
            epsvalue = new lprec().epsdual;
            current.pivot = -epsvalue; // Initialize leaving variable threshold; "less than 0"
            current.theta = 0;
            current.varno = 0;
            current.isdual = true;
            current.lp = lp;
            candidate.isdual = true;
            candidate.lp = lp;

            /* Loop over active partial row set */
            if (LpCls.is_action(lp.piv_strategy, lp_lib.PRICE_FORCEFULL))
            {
                k = 1;
                iy = lp.rows;
            }
            else
            {
                k = partial_blockStart(lp, true);
                iy = partial_blockEnd(lp, true);
            }
            ninfeas = 0;
            xinfeas = 0;
            sinfeas = 0;
            makePriceLoop(lp, ref k, ref iy, ref iz);
            iy *= iz;
            for (; k * iz <= iy; k += iz)
            {

                /* Map loop variable to target */
                i = k;

                /* Check if the pivot candidate is on the block-list */
                if (lp.rejectpivot[0] > 0)
                {
                    int kk;
                    for (kk = 1; (kk <= lp.rejectpivot[0]) && (i != lp.rejectpivot[kk]); kk++)
                    {
                        ;
                    }
                    if (kk <= lp.rejectpivot[0])
                    {
                        continue;
                    }
                }

                /* Set local variables - express violation as a negative number */
                ii = lp.var_basic[i];
                up = lp.upbo[ii];
                lo = 0;
                rh = rhvec[i];
                if (rh > up)
                {
                    rh = up - rh;
                }
                else
                {
                    rh -= lo;
                }
                up -= lo;

                /* Analyze relevant constraints ...
                   KE version skips uninteresting alternatives and gives a noticeable speedup */
                /*    if((rh < -epsvalue*sqrt(lp->matA->rowmax[i])) || */
                if ((rh < -epsvalue) || ((forceoutEQ == true) && (up < epsvalue)))
                { // It causes instability to remove the "TRUE" test

                    /* Accumulate stats */
                    ninfeas++;
                    commonlib.SETMIN(Convert.ToInt32(xinfeas), Convert.ToInt32(rh));
                    sinfeas += rh;

                    /* Give a slight preference to fixed variables (mainly equality slacks) */
                    if (up < epsvalue)
                    {
                        /* Break out immediately if we are directed to force slacks out of the basis */
                        if (forceoutEQ == true)
                        {
                            current.varno = i;
                            current.pivot = -1;
                            break;
                        }
                        /* Give an extra early boost to equality slack elimination, if specified */
                        if (forceoutEQ == Convert.ToBoolean(DefineConstants.AUTOMATIC))
                        {
                            rh *= 10.0;
                        }
                        else // .. or just the normal. marginal boost
                        {
                            rh *= 1.0 + lp.epspivot;
                        }
                    }

                    /* Select leaving variable according to strategy (the most negative/largest violation) */
                    candidate.pivot = normalizeEdge(lp, i, rh, true);
                    candidate.varno = i;
                    int Parameter = 0;
                    if (findImprovementVar(current, candidate, collectMP, ref Parameter))
                    {
                        break;
                    }
                }
            }

            /* Verify infeasibility */
            if (updateinfeas)
            {
                lp.suminfeas = System.Math.Abs(sinfeas);
            }
            if ((ninfeas > 1) && !verify_stability(lp, false, xinfeas, sinfeas, ninfeas))
            {
                msg = "rowdual: Check for reduced accuracy and tolerance settings.\n";
                lp.report(lp, lp_lib.IMPORTANT, ref msg);
                current.varno = 0;
            }

            /* Produce statistics */
            if (lp.spx_trace)
            {
                msg = "rowdual: Infeasibility sum " + lp_types.RESULTVALUEMASK + " in %7d constraints.\n";
                lp.report(lp, lp_lib.NORMAL, ref msg, sinfeas, ninfeas);
                if (current.varno > 0)
                {
                    msg = "rowdual: rhs[%d] = " + lp_types.RESULTVALUEMASK + "\n";
                    lp.report(lp, lp_lib.DETAILED, ref msg, current.varno, lp.rhs[current.varno]);
                }
                else
                {
                    msg = "rowdual: Optimality - No primal infeasibilities found\n";
                    lp.report(lp, lp_lib.FULL, ref msg);
                }
            }
            if (xviol != null)
            {
                xviol = System.Math.Abs(xinfeas);
            }

            return (current.varno);
        } // rowdual

        //Changed By: CS Date:28/11/2018
        internal static double multi_enteringtheta(multirec multi)
        {
            return (multi.step_base);
        }

        //Changed By: CS Date:28/11/2018
        /* Computation of reduced costs */
        internal static void update_reducedcosts(lprec lp, bool isdual, int leave_nr, int enter_nr, double[] prow, double[] drow)
        {
            /* "Fast" update of the dual reduced cost vector; note that it must be called
               after the pivot operation and only applies to a major "true" iteration */
            int i;
            double hold = new double();

            if (isdual)
            {
                hold = -drow[enter_nr] / prow[enter_nr];
                for (i = 1; i <= lp.sum; i++)
                {
                    if (!lp.is_basic[i])
                    {
                        if (i == leave_nr)
                        {
                            drow[i] = hold;
                        }
                        else
                        {
                            drow[i] += hold * prow[i];
                            lp_types.my_roundzero(drow[i], lp.epsmachine);
                        }
                    }
                }
            }
            else
            {
                string msg = "update_reducedcosts: Cannot update primal reduced costs!\n";
                lp.report(lp, lp_lib.SEVERE, ref msg);
            }
        }

        /* Find the dual simplex entering non-basic variable */
        internal static int coldual(lprec lp, int row_nr, ref double prow, ref int[] nzprow, ref double[] drow, ref int nzdrow, bool dualphase1, bool skipupdate, ref int candidatecount, ref double xviol)
        {
            string msg = "";
            LpCls objLpCls = new LpCls();
            int i;
            int iy;
            int iz = 0;
            int ix;
            int k;
            int nbound;
            double w = new double();
            double g = new double();
            double quot = new double();
            double viol = new double();
            double p = new double();
            double epspivot = lp.epspivot;
#if MachinePrecRoundRHS
  REAL epsvalue = lp.epsmachine;
#else
            double epsvalue = new lprec().epsvalue;
#endif
            pricerec current = new pricerec();
            pricerec candidate = new pricerec();
            bool isbatch = false;
            bool dolongsteps = (bool)(lp.longsteps != null);

            /* Initialize */
            if (xviol != null)
            {
                xviol = lp.infinite;
            }
            if (dolongsteps != null && !dualphase1)
            {
                dolongsteps = Convert.ToBoolean(lp_types.AUTOMATIC); // Sets Phase1 = TRUE, Phase2 = AUTOMATIC
            }
            current.theta = lp.infinite;
            current.pivot = 0;
            current.varno = 0;
            current.epspivot = epspivot;
            current.isdual = true;
            current.lp = lp;
            candidate.epspivot = epspivot;
            candidate.isdual = true;
            candidate.lp = lp;
            candidatecount = 0;

            /* Compute reduced costs */
            if (!skipupdate)
            {
#if UseDualReducedCostUpdate
	        /* Recompute from scratch only at the beginning, otherwise update */
	        if ((lp.current_iter > 0) && (refactRecent(lp) < AUTOMATIC))
	        {
	          compute_reducedcosts(lp, 1, row_nr, null, 1, prow, nzprow, null, null, MAT_ROUNDDEFAULT);
	        }
	        else
	        {
                        compute_reducedcosts(lp, 1, row_nr, null, 1, prow, nzprow, drow, nzdrow, MAT_ROUNDDEFAULT);
            }
#endif
            }

#if false
        // /* Override all above to do in-line testing with fixed test set */
        //  if(lp->rows > 1 && lp->columns > 10)
        //    longdual_testset(lp, 10, row_nr, prow, nzprow, drow, nzdrow);
#endif

            /* Compute the current violation of the bounds of the outgoing variable,
               negative for violation of lower bound, positive for upper bound violation.
               (Basic variables are always lower-bounded, by lp_solve convention) */
            g = 1;
            viol = lp.rhs[row_nr];
            if (viol > 0)
            { // Check if the leaving variable is >= its upper bound
                p = lp.upbo[lp.var_basic[row_nr]];
                if (p < lp.infinite)
                {
                    viol -= p;

                    lp_types.my_roundzero(viol, epsvalue);
                    if (viol > 0)
                    {
                        g = -1;
                    }
                }
                /* Do validation of numerics */
                if (g == 1)
                {
                    if (viol >= lp.infinite)
                    {
                        msg = "coldual: Large basic solution value %g at iter %.0f indicates numerical instability\n";
                        lp.report(lp, lp_lib.IMPORTANT, ref msg, lp.rhs[row_nr], (double)LpCls.get_total_iter(lp));
                        lp.spx_status = lp_lib.NUMFAILURE;
                        return (0);

                    }
                    if (skipupdate)
                    {
                        msg = "coldual: Inaccurate bound-flip accuracy at iter %.0f\n";
                        lp.report(lp, lp_lib.DETAILED, ref msg, (double)LpCls.get_total_iter(lp));
                    }
                    else
                    {
                        msg = "coldual: Leaving variable %d does not violate bounds at iter %.0f\n";
                        lp.report(lp, lp_lib.SEVERE, ref msg, row_nr, (double)LpCls.get_total_iter(lp));
                    }
                    return (-1);
                }
            }

            /* Update local value of pivot setting */
            lp._piv_rule_ = LpCls.get_piv_rule(lp);

            /* Condense list of relevant targets */
            p = 0;
            k = 0;
            nbound = 0;
            ix = 1;
            iy = Convert.ToInt32(nzprow[0]);
            for (ix = 1; ix <= iy; ix++)
            {
                i = nzprow[ix];
                w = prow * g; // Change sign if upper bound of the leaving variable is violated
                              /* Change sign if the non-basic variable is currently upper-bounded */
                              /* w *= 2*lp->is_lower[i] - 1; */
                              /* fails on AIX!!! */
                w = lp_types.my_chsign(!lp.is_lower[i], w);

                /* Check if the candidate is worth using for anything */
                if (w < -epsvalue)
                {
                    /* Tally bounded variables */
                    if (lp.upbo[i] < lp.infinite)
                    {
                        nbound++;
                    }

                    /* Update the nz-index */
                    k++;
                    nzprow[k] = i;

                    commonlib.SETMAX(Convert.ToInt32(p), Convert.ToInt32(-w));
                }
#if Paranoia
	        else
	        {
	          if (lp.spx_trace)
	          {
		        report(lp, FULL, "coldual: Candidate variable prow[%d] rejected with %g too small\n", i, w);
	          }
	        }
#endif

            }
            nzprow[0] = k;
            if (xviol != null)
            {

                xviol = p;
            }

#if UseRelativePivot_Dual
        /*  epspivot *= sqrt(lp->matA->dynrange) / lp->matA->infnorm; */
          epspivot /= MAX(1, Math.Sqrt(lp.matA.rowmax[row_nr]));
#endif
            current.epspivot = epspivot;
            candidate.epspivot = epspivot;

            /* Initialize the long-step structures if indicated */
            if (dolongsteps)
            {
                if ((nzprow[0] <= 1) || (nbound == 0))
                { // Don't bother
                    dolongsteps = false;
                    lp.longsteps.indexSet[0] = 0;
                }
                else
                {

                    multi_restart(lp.longsteps);

                    multi_valueInit(lp.longsteps, g * viol, lp.rhs[0]);
                }
            }

            /* Loop over all entering column candidates */
            ix = 1;
            iy = nzprow[0];
            makePriceLoop(lp, ref ix, ref iy, ref iz);
            iy *= iz;
            for (; ix * iz <= iy; ix += iz)
            {
                i = nzprow[ix];

                /* Compute the dual ratio (prow = w and drow = cbar in Chvatal's "nomenclatura") */
                w = prow * g; // Change sign if upper bound of the leaving variable is violated
                quot = -drow[i] / w; // Remember this sign-reversal in multi_recompute!

                /* Apply the selected pivot strategy (smallest theta) */
                candidate.theta = quot; // Note that abs() is applied in findSubstitutionVar
                candidate.pivot = w;
                candidate.varno = i;

                /* Collect candidates for minor iterations/bound flips */
                if (dolongsteps)
                {
                    if (isbatch && (ix == iy))
                    {
                        isbatch = Convert.ToBoolean(lp_types.AUTOMATIC);
                    }
                    if (collectMinorVar(candidate, lp.longsteps, (bool)(dolongsteps == Convert.ToBoolean(lp_types.AUTOMATIC)), isbatch) && lp.spx_trace)
                    {
                        msg = "coldual: Long-dual break point with %d bound-flip variables\n";
                        lp.report(lp, lp_lib.DETAILED, ref msg, lp.longsteps.used);
                    }
                    if (lp.spx_status == lp_lib.FATHOMED)
                    {
                        return (0);
                    }
                }

                /* We have a candidate for entering the basis; check if it is better than the incumbent */
                else if (findSubstitutionVar(current, candidate, ref candidatecount))
                {
                    break;
                }
            }

            /* Set entering variable and long-step bound swap variables */
            if (dolongsteps)
            {

                candidatecount = lp.longsteps.used;
                i = multi_enteringvar(lp.longsteps, null, 3);
            }
            else
            {
                i = current.varno;
            }

            if (lp.spx_trace)
            {
                msg = "coldual: Entering column %d, reduced cost %g, pivot value %g, bound swaps %d\n";
                lp.report(lp, lp_lib.NORMAL, ref msg, i, drow[i], prow, multi_used(lp.longsteps));
            }

            return (i);

        }

        internal static void multi_valueInit(multirec multi, double step_base, double obj_base)
        {
            multi.step_base = multi.step_last = step_base;
            multi.obj_base = multi.obj_last = obj_base;
#if Paranoia
  if (step_base > 0)
  {
	report(multi.lp, SEVERE, "multi_valueInit: Positive constraint violation %g provided at iteration %6.0f\n", step_base, (double) get_total_iter(multi.lp));
  }
#endif
        }

        /* Bound flip variable accumulation routine */
        private static bool collectMinorVar(pricerec candidate, multirec longsteps, bool isphase2, bool isbatch)
        {
            int inspos = 0;

            /* 1. Check for ratio and pivot validity (to have the extra flexibility that all
                  bound-flip candidates are also possible as basis-entering variables */
            if (!validSubstitutionVar(candidate))
            {
                return (false);
            }

            /* 2. If the free-list is empty we need to see if we have a better candidate,
                  and for this the candidate list has to be sorted by merit */
            if (!isbatch && !longsteps.sorted && (longsteps.used > 1) && ((longsteps.freeList[0] == 0) || multi_truncatingvar(longsteps, candidate.varno) || (longsteps.step_last >= longsteps.epszero)))
            {
                //ORIGINAL LINE: longsteps.sorted = commonlib.QS_execute(longsteps.sortedList, longsteps.used, (findCompare_func)compareSubstitutionQS, inspos);
                //NOTED ISSUE:
                longsteps.sorted = commonlib.QS_execute(longsteps.sortedList, longsteps.used, (commonlib.findCompare_func)compareSubstitutionQS, ref inspos);
                longsteps.dirty = (bool)(inspos > 0);
                if (longsteps.dirty)
                {
                    multi_recompute(longsteps, 0, isphase2, true);
                }
            }

            /* 3. Now handle three cases...
                  - Add to the list when the list is not full and there is opportunity for improvement,
                  - Check if we should replace an incumbent when the list is full,
                  - Check if we should replace an incumbent when the list is not full, there is no room
                    for improvement, but the current candidate is better than an incumbent. */
            //NOTED ISSUE:
            inspos = addCandidateVar(candidate, longsteps, (commonlib.findCompare_func)compareSubstitutionQS, true);

            /* 4. Recompute steps and objective, and (if relevant) determine if we
                  may be suboptimal in relation to an incumbent MILP solution. */
            return ((bool)(inspos >= 0) && ((isbatch == true) || multi_recompute(longsteps, inspos, isphase2, true)));
        }

        internal static int compareSubstitutionQS(params QSORTrec[] current)
        {
            if (current.Length == 2)
                return (compareBoundFlipVar((pricerec)current[0].pvoidint2.ptr, (pricerec)current[1].pvoidint2.ptr));
            /*  return( compareSubstitutionVar((pricerec *) current->self, (pricerec *) candidate->self) ); */
            else
                return 0;
        }

        internal static int compareBoundFlipVar(pricerec current, pricerec candidate)
        {
            //ORIGINAL LINE: register double testvalue, margin;
            double testvalue;
            double margin;
            //ORIGINAL LINE: register int result = COMP_PREFERNONE;
            int result = lp_types.COMP_PREFERNONE;
            //ORIGINAL LINE: register lprec *lp = current->lp;
            lprec lp = current.lp;
            bool candbetter = new bool();
            int currentvarno = current.varno;
            int candidatevarno = candidate.varno;

            if (!current.isdual)
            {
                candidatevarno = lp.var_basic[candidatevarno];
                currentvarno = lp.var_basic[currentvarno];
            }

            /* Compute the ranking test metric. */
            testvalue = candidate.theta;
            margin = current.theta;
            if (candidate.isdual)
            {
                testvalue = System.Math.Abs(testvalue);
                margin = System.Math.Abs(margin);
            }
            if (System.Math.Abs(margin) < lp_lib.LIMIT_ABS_REL)
            {
                testvalue -= margin;
            }
            else
            {
                testvalue = lp_types.my_reldiff(testvalue, margin);
            }

            /* Find if the new Theta is smaller or near equal (i.e. testvalue <= eps)
               compared to the previous best; ties will be broken by pivot size or index */
            margin = new lp_lib().PREC_SUBSTFEASGAP;
            //ORIGINAL LINE: candbetter = (MYBOOL)(testvalue < 0);
            candbetter = ((bool)(testvalue < 0));
            if (candbetter != null)
            {
                if (testvalue < -margin)
                {
                    result = lp_types.COMP_PREFERCANDIDATE;
                }
            }
            else if (testvalue > margin)
            {
                result = lp_types.COMP_PREFERINCUMBENT;
            }

            /* Resolve a tie */
            if (result == lp_types.COMP_PREFERNONE)
            {

                /* Tertiary selection based on priority for large pivot sizes */
                if (result == lp_types.COMP_PREFERNONE)
                {
                    double currentpivot = System.Math.Abs(current.pivot);
                    double candidatepivot = System.Math.Abs(candidate.pivot);
                    if (candidatepivot > currentpivot + margin)
                    {
                        result = lp_types.COMP_PREFERCANDIDATE;
                    }
                    else if (candidatepivot < currentpivot - margin)
                    {
                        result = lp_types.COMP_PREFERINCUMBENT;
                    }
                }

                /* Secondary selection based on priority for narrow-bounded variables */
                if (result == lp_types.COMP_PREFERNONE)
                {
                    result = commonlib.compareREAL((lp.upbo[currentvarno]), (lp.upbo[candidatevarno]));
                }

            }

            /* Select absolute best if the non-primary criteria failed to separate */
            if ((result == lp_types.COMP_PREFERNONE) && candbetter != null)
            {
                result = lp_types.COMP_PREFERCANDIDATE;
                goto Finish;
            }

            /* Quaternary selection by index value */
            if (result == lp_types.COMP_PREFERNONE)
            {
                if (candidatevarno < currentvarno)
                {
                    result = lp_types.COMP_PREFERCANDIDATE;
                }
                else
                {
                    result = lp_types.COMP_PREFERINCUMBENT;
                }
                if (lp._piv_left_)
                {
                    result = -result;
                }
            }

            Finish:
            return (result);
        }

        internal static bool multi_recompute(multirec multi, int index, bool isphase2, bool fullupdate)
        {
            int i;
            int n;
            double lB;
            double uB;
            double Alpha;
            double this_theta;
            double prev_theta;
            lprec lp = multi.lp;
            pricerec thisprice;

            /* Define target update window */
            if (multi.dirty)
            {
                index = 0;
                n = multi.used - 1;
            }
            else if (fullupdate)
            {
                n = multi.used - 1;
            }
            else
            {
                n = index;
            }

            /* Initialize accumulators from the specified update index */
            if (index == 0)
            {
                multi.maxpivot = 0;
                multi.maxbound = 0;
                multi.step_last = multi.step_base;
                multi.obj_last = multi.obj_base;
                thisprice = null;
                this_theta = 0;
            }
            else
            {
                multi.obj_last = multi.valueList[index - 1];
                multi.step_last = multi.sortedList[index - 1].pvoidreal.realval;
                thisprice = (pricerec)(multi.sortedList[index - 1].pvoidreal.ptr);
                this_theta = thisprice.theta;
            }

            /* Update step lengths and objective values */
            while ((index <= n) && (multi.step_last < multi.epszero))
            {

                /* Update parameters for this loop */
                prev_theta = this_theta;
                thisprice = (pricerec)(multi.sortedList[index].pvoidreal.ptr);
                this_theta = thisprice.theta;
                Alpha = System.Math.Abs(thisprice.pivot);
                uB = lp.upbo[thisprice.varno];
                lB = 0;
                commonlib.SETMAX(Convert.ToInt32(multi.maxpivot), Convert.ToInt32(Alpha));
                commonlib.SETMAX(Convert.ToInt32(multi.maxbound), Convert.ToInt32(uB));

                /* Do the value updates */
                if (isphase2)
                {
                    multi.obj_last += (this_theta - prev_theta) * multi.step_last; // Sign-readjusted from coldual()/Maros
                    if (uB >= lp.infinite)
                    {
                        multi.step_last = lp.infinite;
                    }
                    else
                    {
                        multi.step_last += Alpha * (uB - lB);
                    }
                }
                else
                {
                    multi.obj_last += (this_theta - prev_theta) * multi.step_last; // Sign-readjusted from coldual()/Maros
                    multi.step_last += Alpha;
                }

                /* Store updated values at the indexed locations */
                multi.sortedList[index].pvoidreal.realval = multi.step_last;
                multi.valueList[index] = multi.obj_last;
#if Paranoia
	if (lp.spx_trace && (multi.step_last > lp.infinite))
	{
	  report(lp, SEVERE, "multi_recompute: A very large step-size %g was generated at iteration %6.0f\n", multi.step_last, (double) get_total_iter(lp));
	}
#endif
                index++;
            }

            /* Discard candidates entered earlier that now make the OF worsen, and
               make sure that the released positions are added to the free list. */
            n = index;
            while (n < multi.used)
            {
                i = ++multi.freeList[0];
                multi.freeList[i] = (int)(Convert.ToInt32((pricerec)multi.sortedList[n].pvoidreal.ptr) - Convert.ToInt32(multi.items));
                n++;
            }
            multi.used = index;
            if (multi.sorted && (index == 1))
            {
                multi.sorted = false;
            }
            multi.dirty = false;

            /* Return TRUE if the step is now positive */
            return ((bool)(multi.step_last >= multi.epszero));
        }

        internal static int multi_enteringvar(multirec multi, pricerec current, int priority)
        {
            lprec lp = multi.lp;
            int i = 0;
            int bestindex;
            int colnr;
            double bound = new double();
            double score = new double();
            double bestscore = -lp.infinite;
            double b1 = new double();
            double b2 = new double();
            double b3 = new double();
            pricerec candidate;
            pricerec bestcand;
            string msg;

            /* Check that we have a candidate */
            multi.active = bestindex = 0;
            if ((multi == null) || (multi.used == 0))
            {
                return (bestindex);
            }

            /* Check for pruning possibility of the B&B tree */
            if (multi.objcheck && (lp.solutioncount > 0) && LpCls.bb_better(lp, lp_lib.OF_WORKING | lp_lib.OF_PROJECTED, lp_lib.OF_TEST_WE))
            {
                lp.spx_status = lp_lib.FATHOMED;
                return (bestindex);
            }

            /* Check the trivial case */
            if (multi.used == 1)
            {
                bestcand = (pricerec)(multi.sortedList[bestindex].pvoidreal.ptr);
                goto Finish;
            }

            /* Set priority weights */
            Redo:
            switch (priority)
            {
                case 0:
                    b1 = 0.0; b2 = 0.0; b3 = 1.0; // Only OF
                    bestindex = multi.used - 2;
                    break;
                case 1:
                    b1 = 0.2; b2 = 0.3; b3 = 0.5;
                    break; // Emphasize OF
                case 2:
                    b1 = 0.3; b2 = 0.5; b3 = 0.2;
                    break; // Emphasize bound
                case 3:
                    b1 = 0.6; b2 = 0.2; b3 = 0.2;
                    break; // Emphasize pivot
                case 4:
                    b1 = 1.0; b2 = 0.0; b3 = 0.0;
                    break; // Only pivot
                default:
                    b1 = 0.4; b2 = 0.2; b3 = 0.4; // Balanced default
                    break;
            }
            bestcand = (pricerec)(multi.sortedList[bestindex].pvoidreal.ptr);

            /* Loop over all candidates to get the best entering candidate;
               start at the end to try to maximize the chain length */
            for (i = multi.used - 1; i >= 0; i--)
            {
                candidate = (pricerec)(multi.sortedList[i].pvoidreal.ptr);
                colnr = candidate.varno;
                bound = lp.upbo[colnr];
                score = System.Math.Abs(candidate.pivot) / multi.maxpivot;
                score = System.Math.Pow(1.0 + score, b1) * System.Math.Pow(1.0 + System.Math.Log(bound / multi.maxbound + 1), b2) * System.Math.Pow(1.0 + (double)i / multi.used, b3);
                if (score > bestscore)
                {
                    //ORIGINAL LINE: bestscore = score;
                    bestscore = (score);
                    bestindex = i;
                    bestcand = candidate;
                }
            }

            /* Do pivot protection */
            if ((priority < 4) && (System.Math.Abs(bestcand.pivot) < lp.epssolution))
            {
                bestindex = 0;
                priority++;
                goto Redo;
            }

            Finish:
            /* Make sure we shrink the list and update */
            multi.active = colnr = bestcand.varno;
            if (bestindex < multi.used - 1)
            {
#if false
// /*    if(lp->upbo[colnr] >= lp->infinite) */
//    QS_swap(multi->sortedList, bestindex, multi->used-1);
//    multi_recompute(multi, bestindex, (bestcand->isdual == AUTOMATIC), TRUE);
#else
                multi.used = i + 1;
#endif
            }
            int[] Parameter = null;
            multi_populateSet(multi, ref Parameter, multi.active);

            /* Compute the entering theta and update parameters */
            score = (multi.used == 1 ? multi.step_base : multi.sortedList[multi.used - 2].pvoidreal.realval);
            score /= bestcand.pivot;
            score = lp_types.my_chsign(!lp.is_lower[multi.active], score);

            if (lp.spx_trace && (System.Math.Abs(score) > 1 / new lprec().epsprimal))
            {
                msg = "multi_enteringvar: A very large Theta %g was generated (pivot %g)\n";
                lp.report(lp, lp_lib.IMPORTANT, ref msg, score, bestcand.pivot);
            }
            multi.step_base = score;
            if (current != null)
            {
                //ORIGINAL LINE: *current = *bestcand;
                current = (bestcand);
            }

            return (multi.active);
        }

        internal static int multi_used(multirec multi)
        {
            if (multi == null)
            {
                return (0);
            }
            else
            {
                return (multi.used);
            }
        }

        /* Support routines for block detection and partial pricing */
        internal static int partial_findBlocks(lprec lp, bool autodefine, bool isrow)
        {
            int i;
            int jj;
            int n;
            int nb;
            int ne;
            int items;
            double hold;
            double biggest;
            double[] sum = null;
            MATrec mat = lp.matA;
            partialrec blockdata;
            LpCls objLpCls = new LpCls();

            if (!lp_matrix.mat_validate(mat))
            {
                return (1);
            }

            blockdata = (partialrec)commonlib.IF(isrow, lp.rowblocks, lp.colblocks);
            items =  (int)commonlib.IF(isrow, lp.rows, lp.columns);

            //NOT REQUIRED
            //lp_utils.allocREAL(lp, sum, items + 1, 0);

            /* Loop over items and compute the average column index for each */
            sum[0] = 0;
            for (i = 1; i <= items; i++)
            {
                n = 0;
                if (isrow)
                {
                    nb = Convert.ToInt32(mat.row_end[i - 1]);
                    ne = Convert.ToInt32(mat.row_end[i]);
                }
                else
                {
                    nb = Convert.ToInt32(mat.col_end[i - 1]);
                    ne = Convert.ToInt32(mat.col_end[i]);
                }
                n = ne - nb;
                sum[i] = 0;
                if (n > 0)
                {
                    if (isrow)
                    {
                        for (jj = nb; jj < ne; jj++)
                        {
                            sum[i] += lp_matrix.ROW_MAT_COLNR(jj);
                        }
                    }
                    else
                    {
                        for (jj = nb; jj < ne; jj++)
                        {
                            sum[i] += lp_matrix.COL_MAT_ROWNR(jj);
                        }
                    }
                    sum[i] /= n;
                }
                else
                {
                    sum[i] = sum[i - 1];
                }
            }

            /* Loop over items again, find largest difference and make monotone */
            hold = 0;
            biggest = 0;
            for (i = 2; i <= items; i++)
            {
                hold = sum[i] - sum[i - 1];
                if (hold > 0)
                {
                    if (hold > biggest)
                    {
                        biggest = hold;
                    }
                }
                else
                {
                    hold = 0;
                }
                sum[i - 1] = hold;
            }

            /* Loop over items again and find differences exceeding threshold;
               the discriminatory power of this routine depends strongly on the
               magnitude of the scaling factor - from empirical evidence > 0.9 */
            biggest = commonlib.MAX(1, 0.9 * biggest);
            n = 0;
            nb = 0;
            ne = 0;
            for (i = 1; i < items; i++)
            {
                if (sum[i] > biggest)
                {
                    ne += i - nb; // Compute sum of index gaps between maxima
                    nb = i;
                    n++; // Increment count
                }
            }

            /* Clean up */
            //NOT REQUIRED
            //FREE(sum);

            /* Require that the maxima are spread "nicely" across the columns,
               otherwise return that there is only one monolithic block.
               (This is probably an area for improvement in the logic!) */
            if (n > 0)
            {
                ne /= n; // Average index gap between maxima
                i = (int)commonlib.IF(isrow, lp.columns, lp.rows);
                nb = i / ne; // Another estimated block count
                if (System.Math.Abs(nb - n) > 2) // Probably Ok to require equality (nb==n)
                {
                    n = 1;
                }
                else if (autodefine) // Generate row/column break-indeces for partial pricing
                {
                    int? Parameter = 0;
                   objLpCls.set_partialprice(lp, nb, ref Parameter, isrow);
                }
            }
            else
            {
                n = 1;
            }

            return (n);
        }

        /* Partial pricing management routines */
        internal static partialrec partial_createBlocks(lprec lp, bool isrow)
        {
            partialrec blockdata;

            blockdata = new partialrec();
            blockdata.lp = lp;
            blockdata.blockcount = 1;
            blockdata.blocknow = 1;
            blockdata.isrow = isrow;

            return (blockdata);
        }

        static int partial_activeBlocks(lprec lp, bool isrow)
        { throw new NotImplementedException(); }

        static void partial_freeBlocks(partialrec[] blockdata)
        { throw new NotImplementedException(); }

        static void longdual_testset(lprec lp, int which, int rownr, ref double prow, ref int nzprow, ref double drow, ref int nzdrow)
        { throw new NotImplementedException(); }

        static int partial_blockNextPos(lprec lp, int block, bool isrow)
        { throw new NotImplementedException(); }

        static bool partial_isVarActive(lprec lp, int varno, bool isrow)
        { throw new NotImplementedException(); }

        static int multi_size(multirec multi)
        { throw new NotImplementedException(); }

        static double multi_valueList(multirec multi)
        { throw new NotImplementedException(); }

        static int multi_getvar(multirec multi, int item)
        { throw new NotImplementedException(); }


    }
}