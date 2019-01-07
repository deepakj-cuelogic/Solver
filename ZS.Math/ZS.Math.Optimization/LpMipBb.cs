using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    /* Bounds storage for B&B routines */
    public class BBrec
    {
        public BBrec parent;
        public BBrec child;
        public lprec lp;
        public int varno;
        public int vartype;
        public int lastvarcus; // Count of non-int variables of the previous branch
        public int lastrcf;
        public int nodesleft;
        public int nodessolved;
        public int nodestatus;
        public double noderesult;
        public double lastsolution; // Optimal solution of the previous branch
        public double sc_bound;
        public double[] upbo;
        public double[] lowbo;
        public double UPbound;
        public double LObound;
        public int UBtrack; // Signals that incoming bounds were changed
        public int LBtrack;
        public bool contentmode; // Flag indicating if we "own" the bound vectors
        public bool sc_canset;
        public bool isSOS;
        public bool isGUB;
        public int[] varmanaged; // Extended list of variables managed by this B&B level
        public bool isfloor; // State variable indicating the active B&B bound
        public bool UBzerobased; // State variable indicating if bounds have been rebased
    }

    public static class lp_mipbb
    {
        internal static BBrec create_BB(lprec lp, BBrec parentBB, byte dofullcopy)
        {
            throw new NotImplementedException();
        }
        internal static BBrec push_BB(lprec lp, BBrec parentBB, int varno, int vartype, int varcus)
        {
            BBrec newBB;

            /* Do initialization and updates */
            if (parentBB == null)
            {
                parentBB = lp.bb_bounds;
            }
            newBB = create_BB(lp, parentBB, 0);
            if (newBB != null)
            {

                newBB.varno = varno;
                newBB.vartype = vartype;
                newBB.lastvarcus = varcus;
                lp_matrix.incrementUndoLadder(lp.bb_lowerchange);
                newBB.LBtrack++;
                lp_matrix.incrementUndoLadder(lp.bb_upperchange);
                newBB.UBtrack++;

                /* Adjust variable fixing/bound tightening based on the last reduced cost */
                if ((parentBB != null) && (parentBB.lastrcf > 0))
                {
                    bool isINT = new bool();
                    int k;
                    int ii;
                    int nfixed = 0;
                    int ntighten = 0;
                    double deltaUL = new double();

                    for (k = 1; k <= lp.nzdrow[0][0]; k++)
                    {
                        //Added second array as [0], need to check at runtime.
                        ii = lp.nzdrow[k][0];
#if UseMilpSlacksRCF
		isINT = 0;
#else
                        if (ii <= lp.rows)
                        {
                            continue;
                        }
                        isINT = LpCls.is_int(lp, ii - lp.rows);
#endif
#if !UseMilpExpandedRCF
                        if (isINT == null)
                        {
                            continue;
                        }
#endif
                        bool Parameter = false;
                        switch (System.Math.Abs(rcfbound_BB(newBB, ii, isINT, deltaUL, ref Parameter)))
                        {
                            case lp_lib.LE:
                                commonlib.SETMIN(deltaUL, newBB.upbo[ii]);
                                commonlib.SETMAX(deltaUL, newBB.lowbo[ii]);
                                lp_matrix.modifyUndoLadder(lp.bb_upperchange, ii, newBB.upbo, deltaUL);
                                break;
                            case lp_lib.GE:
                                commonlib.SETMAX(deltaUL, newBB.lowbo[ii]);
                                commonlib.SETMIN(deltaUL, newBB.upbo[ii]);
                                lp_matrix.modifyUndoLadder(lp.bb_lowerchange, ii, newBB.lowbo, deltaUL);
                                break;
                            default:
                                continue;
                        }
                        if (newBB.upbo[ii] == newBB.lowbo[ii])
                        {
                            nfixed++;
                        }
                        else
                        {
                            ntighten++;
                        }
                    }
                    if (lp.bb_trace)
                    {
                        string msg = "push_BB: Used reduced cost to fix %d variables and tighten %d bounds\n";
                        lp.report(lp, lp_lib.DETAILED, ref msg, nfixed, ntighten);
                    }
                }

                /* Handle case where we are pushing at the end */
                if (parentBB == lp.bb_bounds)
                {
                    lp.bb_bounds = newBB;
                }
                /* Handle case where we are pushing in the middle */
                else
                {
                    newBB.child = parentBB.child;
                }
                if (parentBB != null)
                {
                    parentBB.child = newBB;
                }

                lp.bb_level++;
                if (lp.bb_level > lp.bb_maxlevel)
                {
                    lp.bb_maxlevel = lp.bb_level;
                }

                if (!Convert.ToBoolean(initbranches_BB(newBB)))
                {
                    newBB = pop_BB(newBB);
                }
                else if (LpCls.MIP_count(lp) > 0)
                {
                    //ORIGINAL LINE: if ((lp.bb_level <= 1) && (lp.bb_varactive == null) && (!lp_utils.allocINT(lp, lp.bb_varactive, lp.columns + 1, 1) || !Convert.ToBoolean(initcuts_BB(lp))))
                    //Removed allocINT temo, Need to check at runtime
                    if ((lp.bb_level <= 1) && (lp.bb_varactive == null))
                    {
                        newBB = pop_BB(newBB);
                    }
                    if (varno > 0)
                    {
                        lp.bb_varactive[varno - lp.rows]++;
                    }
                }
            }
            return (newBB);
        }
        internal static byte initbranches_BB(BBrec BB)
        {
            double new_bound;
            double temp=0;
            int k;
            lprec lp = BB.lp;
            int Parameter = 0;
            bool Parameter2 = false;
            LpCls objLpCls = new LpCls();

            /* Create and initialize local bounds and basis */
            BB.nodestatus = lp_lib.NOTRUN;
            BB.noderesult = lp.infinite;
            LpCls.push_basis(lp, ref Parameter, ref Parameter2, null);

            /* Set default number of branches at the current B&B branch */
            if (BB.vartype == lp_lib.BB_REAL)
            {
                BB.nodesleft = 1;
            }

            else
            {
                /* The default is a binary up-low branching */
                BB.nodesleft = 2;

                /* Initialize the MIP status code pair and set reference values */
                k = BB.varno - lp.rows;
                BB.lastsolution = lp.solution[BB.varno];

                /* Determine if we must process in the B&B SOS mode */
                BB.isSOS = (bool)((BB.vartype == lp_lib.BB_SOS) || lp_SOS.SOS_is_member(lp.SOS, 0, k));
#if Paranoia
	if ((BB.vartype == BB_SOS) && !SOS_is_member(lp.SOS, 0, k))
	{
	  report(lp, SEVERE, "initbranches_BB: Inconsistent identification of SOS variable %s (%d)\n", get_col_name(lp, k), k);
	}
#endif

                /* Check if we have a GUB-member variable that needs a triple-branch */
                BB.isGUB = (bool)((BB.vartype == lp_lib.BB_INT) && Convert.ToBoolean(lp_SOS.SOS_can_activate(lp.GUB, 0, k)));
                if (BB.isGUB)
                {
                    /* Obtain variable index list from applicable GUB - now the first GUB is used,
                      but we could also consider selecting the longest */
                    BB.varmanaged = lp_SOS.SOS_get_candidates(lp.GUB, -1, k, true, ref BB.upbo, ref BB.lowbo);
                    BB.nodesleft++;
                }


                /* Set local pruning info, automatic, or user-defined strategy */
                if (BB.vartype == lp_lib.BB_SOS)
                {
                    if (! Convert.ToBoolean(lp_SOS.SOS_can_activate(lp.SOS, 0, k)))
                    {
                        BB.nodesleft--;
                        BB.isfloor = true;
                    }
                    else
                    {
                        BB.isfloor = (bool)(BB.lastsolution == 0);
                    }
                }

                /* First check if the user wishes to select the branching direction */
                else if (lp.bb_usebranch != null)
                {
                    BB.isfloor = Convert.ToBoolean(lp.bb_usebranch(lp, lp.bb_branchhandle, k));
                }

                /* Otherwise check if we should do automatic branching */
                else if (objLpCls.get_var_branch(lp, k) == lp_lib.BRANCH_AUTOMATIC)
                {
                    new_bound = lp_utils.modf(BB.lastsolution / LpCls.get_pseudorange(lp.bb_PseudoCost, k, BB.vartype), temp);
                    //NOTED ISSUE
                    if (new_bound == double.NaN)
                    {
                        new_bound = 0;
                    }
                    else if (new_bound < 0)
                    {
                        new_bound += 1.0;
                    }
                    BB.isfloor = (bool)(new_bound <= 0.5);

                    /* Set direction by OF value; note that a zero-value in
                       the OF gives priority to floor_first = TRUE */
                    if (LpCls.is_bb_mode(lp, lp_lib.NODE_GREEDYMODE))
                    {
                        if (LpCls.is_bb_mode(lp, lp_lib.NODE_PSEUDOCOSTMODE))
                        {
                            BB.sc_bound = LpCls.get_pseudonodecost(lp.bb_PseudoCost, k, BB.vartype, BB.lastsolution);
                        }
                        else
                        {
                            BB.sc_bound = lp_matrix.mat_getitem(lp.matA, 0, k);
                        }
                        new_bound -= 0.5;
                        BB.sc_bound *= new_bound;
                        BB.isfloor = (bool)(BB.sc_bound > 0);
                    }
                    /* Set direction by pseudocost (normally used in tandem with NODE_PSEUDOxxxSELECT) */
                    else if (LpCls.is_bb_mode(lp, lp_lib.NODE_PSEUDOCOSTMODE))
                    {
                        BB.isfloor = (bool)(objLpCls.get_pseudobranchcost(lp.bb_PseudoCost, k, 1) > objLpCls.get_pseudobranchcost(lp.bb_PseudoCost, k, 0));
                        if (LpCls.is_maxim(lp))
                        {
                            BB.isfloor = !BB.isfloor;
                        }
                    }

                    /* Check for reversal */
                    if (LpCls.is_bb_mode(lp, lp_lib.NODE_BRANCHREVERSEMODE))
                    {
                        BB.isfloor = !BB.isfloor;
                    }
                }
                else
                {
                    BB.isfloor = (bool)(objLpCls.get_var_branch(lp, k) == lp_lib.BRANCH_FLOOR);
                }

                /* SC logic: If the current SC variable value is in the [0..NZLOBOUND> range, then

                  UP: Set lower bound to NZLOBOUND, upper bound is the original
                  LO: Fix the variable by setting upper/lower bound to zero

                  ... indicate that the variable is B&B-active by reversing sign of sc_lobound[]. */
                new_bound = System.Math.Abs(lp.sc_lobound[k]);
                BB.sc_bound = new_bound;
                BB.sc_canset = (bool)(new_bound != 0);

                /* Must make sure that we handle fractional lower bounds properly;
                   also to ensure that we do a full binary tree search */
                new_bound =lp_scale.unscaled_value(lp, new_bound, BB.varno);
                if (LpCls.is_int(lp, k) && ((new_bound > 0) && (BB.lastsolution > System.Math.Floor(new_bound))))
                {
                    if (BB.lastsolution < System.Math.Ceiling(new_bound))
                    {
                        BB.lastsolution += 1;
                    }
                    lp_matrix.modifyUndoLadder(lp.bb_lowerchange, BB.varno, BB.lowbo, LpCls.scaled_floor(lp, BB.varno, BB.lastsolution, 1));
                }
            }
            /* Now initialize the brances and set to first */
            return (fillbranches_BB(BB));
        }
        internal static byte fillbranches_BB(BBrec BB)
        {
            throw new NotImplementedException();
        }
        internal static bool nextbranch_BB(BBrec BB)
        {
            throw new NotImplementedException();
        }
        internal static byte strongbranch_BB(lprec lp, BBrec BB, int varno, int vartype, int varcus)
        {
            throw new NotImplementedException();
        }
        internal static byte initcuts_BB(lprec lp)
        {
            throw new NotImplementedException();
        }
        internal static int updatecuts_BB(lprec lp)
        {
            throw new NotImplementedException();
        }
        internal static byte freecuts_BB(lprec lp)
        {
            if (lp.bb_cuttype != null)
            {
                //NOT REQUIRED
                //FREE(lp.bb_cuttype);
            }
            return (1);
        }
        internal static BBrec findself_BB(BBrec BB)
        {
            throw new NotImplementedException();
        }
        internal static int solve_LP(lprec lp, BBrec BB)
        {
            throw new NotImplementedException();
        }
        internal static int rcfbound_BB(BBrec BB, int varno, bool isINT, double newbound, ref bool isfeasible)
        {
            int i = lp_lib.FR;
            lprec lp = BB.lp;
            double deltaRC;
            double rangeLU;
            double deltaOF;
            double lowbo;
            double upbo;
            LpCls objLpCls = new LpCls();

            /* Make sure we only accept non-basic variables */
            if (lp.is_basic[varno])
            {
                return (i);
            }

            /* Make sure we only accept non-fixed variables */
            lowbo = BB.lowbo[varno];
            upbo = BB.upbo[varno];
            rangeLU = upbo - lowbo;

            if (rangeLU > new lprec().epsprimal)
            {
#if ONE
	deltaOF = lp.rhs[0] - lp.bb_workOF;
#elif false
//    deltaOF = my_chsign(is_maxim(lp), lp->real_solution) - lp->bb_workOF;
#else
                deltaOF = lp_types.my_chsign(LpCls.is_maxim(lp), lp.real_solution) - lp.rhs[0];
#endif

                deltaRC = lp_types.my_chsign(!lp.is_lower[varno], Convert.ToDouble(lp.drow[varno]));
                /* Protect against divisions with tiny numbers and stray sign
                   reversals of the reduced cost */
                if (deltaRC < lp.epspivot)
                {
                    return (i);
                }
                deltaRC = deltaOF / deltaRC; // Should always be a positive number!
#if Paranoia
	if (deltaRC <= 0)
	{
	  report(lp, SEVERE, "rcfbound_BB: A negative bound fixing level was encountered after node %.0f\n", (double) lp.bb_totalnodes);
	}
#endif

                /* Check if bound implied by the reduced cost is less than existing range */
                if (deltaRC < rangeLU + lp.epsint)
                {
                    if (lp.is_lower[varno])
                    {
                        if (isINT)
                        {
                            deltaRC = LpCls.scaled_floor(lp, varno, lp_scale.unscaled_value(lp, deltaRC, varno) + new lprec().epsprimal, 1);
                        }
                        upbo = lowbo + deltaRC;
                        deltaRC = upbo;
                        i = lp_lib.LE; // Sets the upper bound
                    }
                    else
                    {
                        if (isINT)
                        {
                            deltaRC = LpCls.scaled_ceil(lp, varno, lp_scale.unscaled_value(lp, deltaRC, varno) + new lprec().epsprimal, 1);
                        }
                        lowbo = upbo - deltaRC;
                        deltaRC = lowbo;
                        i = lp_lib.GE; // Sets the lower bound
                    }

                    /* Check and set feasibility status */
                    if ((isfeasible != null) && (upbo - lowbo < -new lprec().epsprimal))
                    {
                        isfeasible = false;
                    }

                    /* Flag that we can fix the variable by returning the relation code negated */
                    else if (System.Math.Abs(upbo - lowbo) < new lprec().epsprimal)
                    {
                        i = -i;
                    }
                    if (newbound != null)
                    {
                        lp_types.my_roundzero(deltaRC, new lprec().epsprimal);
                        newbound = deltaRC;
                    }
                }

            }
            return (i);

        }
        internal static bool findnode_BB(BBrec BB, ref int varno, ref int vartype, ref int varcus)
        {
            throw new NotImplementedException();
        }
        internal static int solve_BB(BBrec BB)
        {
            int K;
            int status;
            lprec lp = BB.lp;

            /* Protect against infinite recursions do to integer rounding effects */
            status = lp_lib.PROCFAIL;

            /* Shortcut variables, set default bounds */
            K = BB.varno;

            /* Load simple MIP bounds */
            if (K > 0)
            {

                /* Update cuts, if specified */
                updatecuts_BB(lp);

                /* BRANCH_FLOOR: Force the variable to be smaller than the B&B upper bound */
                if (BB.isfloor)
                {
                    lp_matrix.modifyUndoLadder(lp.bb_upperchange, K, BB.upbo, BB.UPbound);
                }

                /* BRANCH_CEILING: Force the variable to be greater than the B&B lower bound */
                else
                {
                    lp_matrix.modifyUndoLadder(lp.bb_lowerchange, K, BB.lowbo, BB.LObound);
                }

                /* Update MIP node count */
                BB.nodessolved++;

            }

            /* Solve! */
            status = solve_LP(lp, BB);

            /* Do special feasibility assessment of high order SOS'es */
#if ONE
  if ((status == OPTIMAL) && (BB.vartype == BB_SOS) && !SOS_is_feasible(lp.SOS, 0, lp.solution))
  {
	status = INFEASIBLE;
  }
#endif

            return (status);
        }
        internal static bool free_BB(BBrec BB)
        {
            bool parentreturned = false;

            if ((BB != null) && (BB != null))
            {
                BBrec parent = (BBrec)BB;

                if ((parent == null) || parent.contentmode)
                {
                    //FREE(BB.upbo);
                    //FREE(BB.lowbo);
                }
                //FREE(BB.varmanaged);
                //FREE(BB);

                parentreturned = (bool)(parent != null);
                if (parentreturned)
                {
                    BB = parent;
                }

            }
            return (parentreturned);
        }
        internal static BBrec pop_BB(BBrec BB)
        {
            int k;
            BBrec parentBB;
            lprec lp = BB.lp;

            if (BB == null)
            {
                return (BB);
            }

            /* Handle case where we are popping the end of the chain */
            parentBB = BB.parent;
            if (BB == lp.bb_bounds)
            {
                lp.bb_bounds = parentBB;
                if (parentBB != null)
                {
                    parentBB.child = null;
                }
            }
            /* Handle case where we are popping inside or at the beginning of the chain */
            else
            {
                if (parentBB != null)
                {
                    parentBB.child = BB.child;
                }
                if (BB.child != null)
                {
                    BB.child.parent = parentBB;
                }
            }

            /* Unwind other variables */
            if (lp.bb_upperchange != null)
            {
                lp_matrix.restoreUndoLadder(lp.bb_upperchange, BB.upbo);
                for (; BB.UBtrack > 0; BB.UBtrack--)
                {
                    lp_matrix.decrementUndoLadder(lp.bb_upperchange);
                    lp_matrix.restoreUndoLadder(lp.bb_upperchange, BB.upbo);
                }
            }
            if (lp.bb_lowerchange != null)
            {
                lp_matrix.restoreUndoLadder(lp.bb_lowerchange, BB.lowbo);
                for (; BB.LBtrack > 0; BB.LBtrack--)
                {
                    lp_matrix.decrementUndoLadder(lp.bb_lowerchange);
                    lp_matrix.restoreUndoLadder(lp.bb_lowerchange, BB.lowbo);
                }
            }
            lp.bb_level--;
            k = BB.varno - lp.rows;
            if (lp.bb_level == 0)
            {
                if (lp.bb_varactive != null)
                {
                    //NOT REQUIRED
                    //FREE(lp.bb_varactive);
                    freecuts_BB(lp);
                }
                if (lp.int_vars + lp.sc_vars > 0)
                {
                    LpCls.free_pseudocost(lp);
                }
                LpCls.pop_basis(lp, false);
                lp.rootbounds = null;
            }
            else
            {
                lp.bb_varactive[k]--;
            }

            /* Undo SOS/GUB markers */
            if (BB.isSOS && (BB.vartype != lp_lib.BB_INT))
            {
                lp_SOS.SOS_unmark(lp.SOS, 0, k);
            }
            else if (BB.isGUB)
            {
                lp_SOS.SOS_unmark(lp.GUB, 0, k);
            }

            /* Undo the SC marker */
            if (BB.sc_canset)
            {
                lp.sc_lobound[k] *= -1;
            }

            /* Pop the associated basis */
#if one
  /* Original version that does not restore previous basis */
  pop_basis(lp, 0);
#else
            /* Experimental version that restores previous basis */
            LpCls.pop_basis(lp, BB.isSOS);
#endif

            /* Finally free the B&B object */
            free_BB(BB);

            /* Return the parent BB */
            return (parentBB);
        }
        internal static int run_BB(lprec lp)
        {
            BBrec currentBB;
            int varno;
            int vartype=0;
            int varcus=0;
            int prevsolutions;
            int status = lp_lib.NOTRUN;
            LpCls objLpCls = new LpCls();

            /* Initialize */
            pre_BB(lp);
            prevsolutions = lp.solutioncount;
#if UseMilpSlacksRCF
  varno = lp.sum;
#else
            varno = lp.columns;
#endif
            lp.bb_upperchange = lp_matrix.createUndoLadder(lp, varno, 2 * LpCls.MIP_count(lp));
            lp.bb_lowerchange = lp_matrix.createUndoLadder(lp, varno, 2 * LpCls.MIP_count(lp));
            lp.rootbounds = currentBB = push_BB(lp, null, 0, lp_lib.BB_REAL, 0);

            /* Perform the branch & bound loop */
            while (lp.bb_level > 0)
            {
                status = solve_BB(currentBB);

#if false
//    if((lp->bb_level == 1) && (MIP_count(lp) > 0)) {
//      if(status == RUNNING)
//        ;
//
// /* Check if there was an integer solution of an aborted model */
//      else if((status == SUBOPTIMAL) && (lp->solutioncount == 1) &&
//              findnode_BB(currentBB, &varno, &vartype, &varcus))
//        status = USERABORT;
//    }
#endif

                if ((status == lp_lib.OPTIMAL) && findnode_BB(currentBB, ref varno, ref vartype, ref varcus))
                {
                    currentBB = push_BB(lp, currentBB, varno, vartype, varcus);
                }

                else
                {
                    while ((lp.bb_level > 0) && !nextbranch_BB(currentBB))
                    {
                        currentBB = pop_BB(currentBB);
                    }
                }

            }

            /* Finalize */
            //NOT REQUIRED
            //lp_matrix.freeUndoLadder((lp.bb_upperchange));
            //lp_matrix.freeUndoLadder((lp.bb_lowerchange));

            /* Check if we should adjust status */
            if (lp.solutioncount > prevsolutions)
            {
                if ((status == lp_lib.PROCBREAK) || (status == lp_lib.USERABORT) || (status == lp_lib.TIMEOUT) || LpCls.userabort(lp, -1))
                {
                    status = lp_lib.SUBOPTIMAL;
                }
                else
                {
                    status = lp_lib.OPTIMAL;
                }
                if (lp.bb_totalnodes > 0)
                {
                    lp.spx_status = lp_lib.OPTIMAL;
                }
            }
            post_BB(lp);
            return (status);

        }

        static bool post_BB(lprec lp)
        {
            return (true);
        }

        /* Future functions */
        static bool pre_BB(lprec lp)
        {
            return (true);
        }
    }

}
