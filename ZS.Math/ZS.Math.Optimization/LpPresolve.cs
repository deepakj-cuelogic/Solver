using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public class psrec
    {
        public LLrec varmap;
        public int[][] next;
        //ORIGINAL LINE: int *empty;
        public int[] empty;
        //ORIGINAL LINE: int *plucount;
        public int plucount;
        //ORIGINAL LINE: int *negcount;
        public int negcount;
        //ORIGINAL LINE: int *pluneg;
        public int pluneg;
        //ORIGINAL LINE: int *infcount;
        public int infcount;
        //ORIGINAL LINE: double *plulower;
        public double plulower;
        //ORIGINAL LINE: double *neglower;
        public double neglower;
        //ORIGINAL LINE: double *pluupper;
        public double pluupper;
        //ORIGINAL LINE: double *negupper;
        public double negupper;
        public int allocsize;
    }

    public class presolverec
    {
        public psrec rows;
        public psrec cols;
        public LLrec EQmap;
        public LLrec LTmap;
        public LLrec INTmap;
        //ORIGINAL LINE: double *pv_upbo;
        public double pv_upbo;
        //ORIGINAL LINE: double *pv_lobo;
        public double pv_lobo;
        //ORIGINAL LINE: double *dv_upbo;
        public double dv_upbo;
        //ORIGINAL LINE: double *dv_lobo;
        public double dv_lobo;
        public lprec lp;
        public double epsvalue;
        public double epspivot;
        public int innerloops;
        public int middleloops;
        public int outerloops;
        public int nzdeleted;
        public byte forceupdate;
    }

    public static class lp_presolve
    {
        public const int MAX_PSMERGELOOPS = 2; // Max loops to merge compatible constraints
        public const int MAX_PSLINDEPLOOPS = 1; // Max loops to detect linearly dependendent constraints
        public const int MAX_PSBOUNDTIGHTENLOOPS = 5; // Maximumn number of loops to allow bound tightenings
        public const int MIN_SOS1LENGTH = 4; // Minimum length of a constraint for conversion to SOS1

        /// <summary>
        /// Not able to find value assignment
        /// </summary>
#if true
        public static double PRESOLVE_EPSVALUE = (0.1 * lprec.epsprimal);
#else
            public const double PRESOLVE_EPSVALUE = lprec.epsvalue;
#endif

        public const double PRESOLVE_EPSPIVOT = 1.0e-3; // Looses robustness at values smaller than ~1.0e-3
        public const int PRESOLVE_BOUNDSLACK = 10; // Extra error recovery/tolerance margin

        public static object DivisorIntegralityLogicEQ2;          /* Always prefer integer divisors */
        public static object FindImpliedEqualities;       /* Detect equalities (default is enabled) */
        public static object Eq2Reldiff;

        /* Put function headers here */
        public static bool presolve_createUndo(lprec lp)
        {
            if (lp.presolve_undo != null)
            {
                presolve_freeUndo(lp);
            }
            //C++ TO C# CONVERTER TODO TASK: The memory management function 'calloc' has no equivalent in C#:
            lp.presolve_undo = new presolveundorec();    // (presolveundorec)calloc(1, sizeof(presolveundorec));
            lp.presolve_undo.lp = lp;
            if (lp.presolve_undo == null)
            {
                return false;
            }
            return true;
        }

        public static byte presolve_rebuildUndo(lprec lp, byte isprimal)
        {
            throw new NotImplementedException();
        }
        public static bool inc_presolve_space(lprec lp, int delta, bool isrows)
        {
            int i;
            int ii;
            int oldrowcolalloc;
            int rowcolsum;
            int oldrowalloc;
            int oldcolalloc;
            presolveundorec psundo = lp.presolve_undo;

            if (psundo == null)
            {
                presolve_createUndo(lp);
                psundo = lp.presolve_undo;
            }

            /* Set constants */
            oldrowalloc = lp.rows_alloc - delta;
            oldcolalloc = lp.columns_alloc - delta;
            oldrowcolalloc = lp.sum_alloc - delta;
            rowcolsum = lp.sum_alloc + 1;

            /*NOT REQUIRED
            // Reallocate lp memory 
            if (isrows)
            {
                allocREAL(lp, psundo.fixed_rhs, lp.rows_alloc + 1, AUTOMATIC);
            }
            else
            {
                allocREAL(lp, psundo.fixed_obj, lp.columns_alloc + 1, AUTOMATIC);
            }
            allocINT(lp, psundo.var_to_orig, rowcolsum, AUTOMATIC);
            allocINT(lp, psundo.orig_to_var, rowcolsum, AUTOMATIC);
            */

            /* Fill in default values, where appropriate */
            if (isrows)
            {
                ii = oldrowalloc + 1;
            }
            else
            {
                ii = oldcolalloc + 1;
            }
            for (i = oldrowcolalloc + 1; i < rowcolsum; i++, ii++)
            {
                psundo.var_to_orig[i] = 0;
                psundo.orig_to_var[i] = 0;
                if (isrows)
                {
                    psundo.fixed_rhs[ii] = 0;
                }
                else
                {
                    psundo.fixed_obj[ii] = 0;
                }
            }
            return true;
        }

        /// <summary>
        /// changed return type from byte to bool on 13/11/18
        /// </summary>
        public static bool presolve_setOrig(lprec lp, int orig_rows, int orig_cols)
        {
            presolveundorec psundo = lp.presolve_undo;

            if (psundo == null)
            {
                return false;
            }
            psundo.orig_rows = orig_rows;
            psundo.orig_columns = orig_cols;
            psundo.orig_sum = orig_rows + orig_cols;
            if (lp.wasPresolved)
            {
                presolve_fillUndo(lp, orig_rows, orig_cols, false);
            }
            return true;

        }
        public static byte presolve_colfix(presolverec psdata, int colnr, double newvalue, byte remove, ref int tally)
        {
            throw new NotImplementedException();
        }
        public static bool presolve_fillUndo(lprec lp, int orig_rows, int orig_cols, bool setOrig)
        {
            int i;
            presolveundorec psundo = lp.presolve_undo;

            for (i = 0; i <= orig_rows; i++)
            {
                psundo.var_to_orig[i] = i;
                psundo.orig_to_var[i] = i;
                psundo.fixed_rhs[i] = 0;
            }
            for (i = 1; i <= orig_cols; i++)
            {
                psundo.var_to_orig[orig_rows + i] = i;
                psundo.orig_to_var[orig_rows + i] = i;
                psundo.fixed_obj[i] = 0;
            }
            if (setOrig)
            {
                presolve_setOrig(lp, orig_rows, orig_cols);
            }

            return true;

        }
        public static bool presolve_freeUndo(lprec lp)
        {
            //throw new NotImplementedException();
            /* NOT REQUIRED
            presolveundorec psundo = lp.presolve_undo;

            if (psundo == null)
                return false;
            NOT REQUIRED
            FREE(psundo.orig_to_var);
            FREE(psundo.var_to_orig);
            FREE(psundo.fixed_rhs);
            FREE(psundo.fixed_obj);
           
            if (psundo.deletedA != null)
            {
                freeUndoLadder((psundo.deletedA));
            }
            if (psundo.primalundo != null)
            {
                freeUndoLadder((psundo.primalundo));
            }
            if (psundo.dualundo != null)
            {
              lp_matrix.freeUndoLadder((psundo.dualundo));
            }
            FREE(lp.presolve_undo);
            return (1);
             */
            return true;
        }

        public static byte presolve_updatesums(presolverec psdata)
        {
            throw new NotImplementedException();
        }

        public static int presolve_nextrow(presolverec psdata, int colnr, ref int previtem)
        {
            throw new NotImplementedException();
        }
        public static int presolve_nextcol(presolverec psdata, int rownr, ref int previtem)
        {
            throw new NotImplementedException();
        }

        public static presolverec presolve_init(lprec lp)
        {
            throw new NotImplementedException();
        }
        public static void presolve_free(presolverec[] psdata)
        {
            throw new NotImplementedException();
        }
        public static int presolve_shrink(presolverec psdata, ref int nConRemove, ref int nVarRemove)
        {
            throw new NotImplementedException();
        }
        public static void presolve_rowremove(presolverec psdata, int rownr, byte allowcoldelete)
        {
            throw new NotImplementedException();
        }
        public static int presolve_colremove(presolverec psdata, int colnr, byte allowrowdelete)
        {
            throw new NotImplementedException();
        }

        public static byte presolve_colfixdual(presolverec psdata, int colnr, ref double fixValue, ref int status)
        {
            throw new NotImplementedException();
        }

        public static int presolve_rowlength(presolverec psdata, int rownr)
        {
            int[] items = psdata.rows.next[rownr];

            if (items == null)
            {
                return (0);
            }
            else
            {
                return (items[0]);
            }
        }
        public static int presolve_collength(presolverec psdata, int colnr)
        {
            int[] items = psdata.cols.next[colnr];
            if (items == null)
            {
                return (0);
            }
            else
            {
                return (items[0]);
            }
        }

        public static int presolve(lprec lp)
        {
            throw new NotImplementedException();
        }
        public static bool postsolve(lprec lp, int status)
        {
            LpCls objLpCls = new LpCls();
            /* Verify solution */
            if (lp.lag_status != lp_lib.RUNNING)
            {
                int itemp;
                string msg;

                if (status == lp_lib.PRESOLVED)
                {
                    status = lp_lib.OPTIMAL;
                }

                if ((status == lp_lib.OPTIMAL) || (status == lp_lib.SUBOPTIMAL))
                {
                    itemp = LpCls.check_solution(lp, lp.columns, lp.best_solution, lp.orig_upbo, lp.orig_lowbo, lp.epssolution);
                    if ((itemp != lp_lib.OPTIMAL) && (lp.spx_status == lp_lib.OPTIMAL))
                    {
                        lp.spx_status = itemp;
                    }
                    else if ((itemp == lp_lib.OPTIMAL) && ((status == lp_lib.SUBOPTIMAL) || (lp.spx_status == lp_lib.PRESOLVED)))
                    {
                        lp.spx_status = status;
                    }
                }
                else if (status != lp_lib.PRESOLVED)
                {
                    msg = "lp_solve unsuccessful after %.0f iter and a last best value of %g\n";
                    lp.report(lp, lp_lib.NORMAL, ref msg, (double)LpCls.get_total_iter(lp), lp.best_solution[0]);
                    if (lp.bb_totalnodes > 0)
                    {
                        msg = "lp_solve explored %.0f nodes before termination\n";
                        lp.report(lp, lp_lib.NORMAL, ref msg, (double)objLpCls.get_total_nodes(lp));
                    }
                }
                else
                {
                    lp.spx_status = lp_lib.OPTIMAL;
                }

                /* Only rebuild primal solution here, since the dual is only computed on request */
                presolve_rebuildUndo(lp, 1);
            }

            /* Check if we can clear the variable map */
            if (LpCls.varmap_canunlock(lp))
            {
                lp.varmap_locked = false;
            }
#if false
//  REPORT_mat_mmsave(lp, "basis.mtx", NULL, FALSE);  // Write the current basis matrix (no OF) 
#endif

            return (true);

        }

    }
}
