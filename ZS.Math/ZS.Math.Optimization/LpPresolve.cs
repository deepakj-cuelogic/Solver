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
        public const double PRESOLVE_EPSVALUE = (0.1 * lprec.epsprimal);
#else
            public const double PRESOLVE_EPSVALUE = lprec.epsvalue;
#endif

        public const double PRESOLVE_EPSPIVOT = 1.0e-3; // Looses robustness at values smaller than ~1.0e-3
        public const int PRESOLVE_BOUNDSLACK = 10; // Extra error recovery/tolerance margin

        public static object DivisorIntegralityLogicEQ2;          /* Always prefer integer divisors */
        public static object FindImpliedEqualities;       /* Detect equalities (default is enabled) */
        public static object Eq2Reldiff;



        /* Put function headers here */
        public static byte presolve_createUndo(lprec lp)
        {
            throw new NotImplementedException(); throw new NotImplementedException();
        }
        public static byte presolve_rebuildUndo(lprec lp, byte isprimal)
        {
            throw new NotImplementedException();
        }
        public static byte inc_presolve_space(lprec lp, int delta, byte isrows)
        {
            throw new NotImplementedException();
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
                presolve_fillUndo(lp, orig_rows, orig_cols, 0);
            }
            return true;

        }
        public static byte presolve_colfix(presolverec psdata, int colnr, double newvalue, byte remove, ref int tally)
        {
            throw new NotImplementedException();
        }
        public static byte presolve_fillUndo(lprec lp, int orig_rows, int orig_cols, byte setOrig)
        {
            throw new NotImplementedException();
        }
        public static byte presolve_freeUndo(lprec lp)
        {
            throw new NotImplementedException();
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
        public static byte postsolve(lprec lp, int status)
        {
            throw new NotImplementedException();
        }

    }
}
