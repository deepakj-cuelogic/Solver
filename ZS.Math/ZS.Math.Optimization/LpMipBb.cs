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
        public byte contentmode; // Flag indicating if we "own" the bound vectors
        public byte sc_canset;
        public byte isSOS;
        public byte isGUB;
        public int[] varmanaged; // Extended list of variables managed by this B&B level
        public byte isfloor; // State variable indicating the active B&B bound
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

                if (!initbranches_BB(newBB))
                {
                    newBB = pop_BB(newBB);
                }
                else if (LpCls.MIP_count(lp) > 0)
                {
                    if ((lp.bb_level <= 1) && (lp.bb_varactive == null) && (!lp_utils.allocINT(lp, lp.bb_varactive, lp.columns + 1, 1) || !initcuts_BB(lp)))
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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

            if (rangeLU > lprec.epsprimal)
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
                            deltaRC = objLpCls.scaled_floor(lp, varno, lp_scale.unscaled_value(lp, deltaRC, varno) + lprec.epsprimal, 1);
                        }
                        upbo = lowbo + deltaRC;
                        deltaRC = upbo;
                        i = lp_lib.LE; // Sets the upper bound
                    }
                    else
                    {
                        if (isINT)
                        {
                            deltaRC = LpCls.scaled_ceil(lp, varno, lp_scale.unscaled_value(lp, deltaRC, varno) + lprec.epsprimal, 1);
                        }
                        lowbo = upbo - deltaRC;
                        deltaRC = lowbo;
                        i = lp_lib.GE; // Sets the lower bound
                    }

                    /* Check and set feasibility status */
                    if ((isfeasible != null) && (upbo - lowbo < -lprec.epsprimal))
                    {
                        isfeasible = false;
                    }

                    /* Flag that we can fix the variable by returning the relation code negated */
                    else if (System.Math.Abs(upbo - lowbo) < lprec.epsprimal)
                    {
                        i = -i;
                    }
                    if (newbound != null)
                    {
                        lp_types.my_roundzero(deltaRC, lprec.epsprimal);
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
            throw new NotImplementedException();
        }
        internal static byte free_BB(BBrec[] BB)
        {
            throw new NotImplementedException();
        }
        internal static BBrec pop_BB(BBrec BB)
        {
            throw new NotImplementedException();
        }
        internal static int run_BB(lprec lp)
        {
            BBrec currentBB;
            int varno;
            int vartype=0;
            int varcus=0;
            int prevsolutions;
            int status = lp_lib.NOTRUN;

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
            freeUndoLadder((lp.bb_upperchange));
            freeUndoLadder((lp.bb_lowerchange));

            /* Check if we should adjust status */
            if (lp.solutioncount > prevsolutions)
            {
                if ((status == PROCBREAK) || (status == USERABORT) || (status == TIMEOUT) || userabort(lp, -1))
                {
                    status = SUBOPTIMAL;
                }
                else
                {
                    status = OPTIMAL;
                }
                if (lp.bb_totalnodes > 0)
                {
                    lp.spx_status = OPTIMAL;
                }
            }
            post_BB(lp);
            return (status);

        }

        /* Future functions */
        static bool pre_BB(lprec lp)
        {
            return (true);
        }
    }

}
