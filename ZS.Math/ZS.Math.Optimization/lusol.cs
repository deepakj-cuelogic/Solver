using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    /* Prototypes for call-back functions                                        */
    /* ------------------------------------------------------------------------- */
    public delegate void LUSOLlogfunc(object lp, object userhandle, string buf);




    public class lusol
    {
        //ORIGINAL CODE: #define LUSOL_FREE(ptr)               {free(ptr); ptr=NULL;}
        internal static Action<object> LUSOL_FREE = delegate (object obj) { obj = null; };

        public const int LUSOL_RP_SMARTRATIO = 0;
        public const int LUSOL_AUTOORDER = 2;
        public const int LUSOL_IP_ACCELERATION = 7;
        public const int LUSOL_BASEORDER = 0;
        public const int LUSOL_ACCELERATE_L0 = 4;
        public const int LUSOL_INFORM_NOMEMLEFT = 10;


        //public const int LUSOL_RP_SMARTRATIO = 0;
        public const int ZERO = 0;
        public const int LUSOL_ACCELERATE_U = 8;
        public const int LUSOL_INFORM_LUSINGULAR = 1;
        public const int LUSOL_RP_RESIDUAL_U = 20;

        public const int LUSOL_INFORM_LUSUCCESS = 0;
        public const int LUSOL_INFORM_ADIMERR = 3;
        public const int LUSOL_INFORM_ADUPLICATE = 4;
        public const int LUSOL_INFORM_ANEEDMEM = 7;  /* Set lena >= luparm[LUSOL_IP_MINIMUMLENA] */
        public const int LUSOL_INFORM_FATALERR = 8;
        public const int LUSOL_INFORM_NOPIVOT = 9;  /* No diagonal pivot found with TSP or TDP. */

        /* luparm INPUT parameters: */
        public const int LUSOL_IP_PRINTLEVEL = 2;
        public const int LUSOL_IP_MARKOWITZ_MAXCOL = 3;
        public const int LUSOL_IP_SCALAR_NZA = 4;
        public const int LUSOL_IP_PIVOTTYPE = 6;
        public const int LUSOL_IP_KEEPLU = 8;
        public const int LUSOL_IP_SINGULARLISTSIZE = 9;

        /* luparm OUTPUT parameters: */
        public const int LUSOL_IP_INFORM = 10;
public const int LUSOL_IP_SINGULARITIES = 11;
public const int LUSOL_IP_SINGULARINDEX = 12;
public const int LUSOL_IP_MINIMUMLENA = 13;
public const int LUSOL_IP_MAXLEN = 14;
public const int LUSOL_IP_UPDATECOUNT = 15;
public const int LUSOL_IP_RANK_U = 16;
public const int LUSOL_IP_COLCOUNT_DENSE1 = 17;
public const int LUSOL_IP_COLCOUNT_DENSE2 = 18;
public const int LUSOL_IP_COLINDEX_DUMIN = 19;
public const int LUSOL_IP_COLCOUNT_L0 = 20;
public const int LUSOL_IP_NONZEROS_L0 = 21;
public const int LUSOL_IP_NONZEROS_U0 = 22;
public const int LUSOL_IP_NONZEROS_L = 23;
public const int LUSOL_IP_NONZEROS_U = 24;
public const int LUSOL_IP_NONZEROS_ROW = 25;
public const int LUSOL_IP_COMPRESSIONS_LU = 26;
public const int LUSOL_IP_MARKOWITZ_MERIT = 27;
public const int LUSOL_IP_TRIANGROWS_U = 28;
public const int LUSOL_IP_TRIANGROWS_L = 29;
public const int LUSOL_IP_FTRANCOUNT = 30;
public const int LUSOL_IP_BTRANCOUNT = 31;
public const int LUSOL_IP_ROWCOUNT_L0 = 32;
        public const int LUSOL_IP_LASTITEM = LUSOL_IP_ROWCOUNT_L0;

        /* parmlu INPUT parameters: */
        public const int LUSOL_RP_FACTORMAX_Lij = 1;
        public const int LUSOL_RP_UPDATEMAX_Lij = 2;
        public const int LUSOL_RP_ZEROTOLERANCE = 3;
        public const int LUSOL_RP_SMALLDIAG_U = 4;
        public const int LUSOL_RP_EPSDIAG_U = 5;
        public const int LUSOL_RP_COMPSPACE_U = 6;
        public const int LUSOL_RP_MARKOWITZ_CONLY = 7;
        public const int LUSOL_RP_MARKOWITZ_DENSE = 8;
        public const int LUSOL_RP_GAMMA = 9;

        /* parmlu OUPUT parameters: */
        public const int LUSOL_RP_MAXELEM_A = 10;
        public const int LUSOL_RP_MAXMULT_L = 11;
        public const int LUSOL_RP_MAXELEM_U = 12;
        public const int LUSOL_RP_MAXELEM_DIAGU = 13;
        public const int LUSOL_RP_MINELEM_DIAGU = 14;
        public const int LUSOL_RP_MAXELEM_TCP = 15;
        public const int LUSOL_RP_GROWTHRATE = 16;

        /* Parameter/option defines                                                  */
        /* ------------------------------------------------------------------------- */

        public const int LUSOL_MSG_SINGULARITY = 0;
        public const int LUSOL_MSG_STATISTICS = 10;
        public const int LUSOL_MSG_PIVOT = 50;

        public const int LUSOL_PIVMOD_NOCHANGE = -2;  /* Don't change active pivoting model */
        public const int LUSOL_PIVMOD_DEFAULT = -1;  /* Set pivoting model to default */
        public const int LUSOL_PIVMOD_TPP = 0;  /* Threshold Partial   pivoting (normal) */
        public const int LUSOL_PIVMOD_TRP = 1;  /* Threshold Rook      pivoting */
        public const int LUSOL_PIVMOD_TCP = 2;  /* Threshold Complete  pivoting */
        public const int LUSOL_PIVMOD_TSP = 3;  /* Threshold Symmetric pivoting */
        public const int LUSOL_PIVMOD_MAX = LUSOL_PIVMOD_TSP;

        public const int LUSOL_PIVTOL_NOCHANGE = 0;
        public const int LUSOL_PIVTOL_BAGGY = 1;
        public const int LUSOL_PIVTOL_LOOSE = 2;
        public const int LUSOL_PIVTOL_NORMAL = 3;
        public const int LUSOL_PIVTOL_SLIM = 4;
        public const int LUSOL_PIVTOL_TIGHT = 5;
        public const int LUSOL_PIVTOL_SUPER = 6;
        public const int LUSOL_PIVTOL_CORSET = 7;
        public const int LUSOL_PIVTOL_MAX = LUSOL_PIVTOL_CORSET;


        public const int LUSOL_PIVTOL_DEFAULT = LUSOL_PIVTOL_SLIM;

        /* General constants and data type definitions                               */
        /* ------------------------------------------------------------------------- */
        public const int LUSOL_ARRAYOFFSET = 1;

        /* User-settable default parameter values                                    */
        /* ------------------------------------------------------------------------- */
        public const double LUSOL_SMALLNUM = 1.0e-20;  /* IAEE doubles have precision 2.22e-16 */
        public const double LUSOL_BIGNUM = 1.0e+20;
        public const double LUSOL_MINDELTA_a = 10000;

        public const int LUSOL_MULT_nz_a = 2;  /* Suggested by Yin Zhang */

        internal static bool LUSOL_addSingularity(LUSOLrec LUSOL, int singcol, ref int inform)
        {
            int NSING = LUSOL.luparm[LUSOL_IP_SINGULARITIES];
            int ASING = LUSOL.luparm[LUSOL_IP_SINGULARLISTSIZE];

            /* Check if we need to allocated list memory to store multiple singularities */
            if ((NSING > 0) && (NSING >= ASING))
            {

                /* Increase list in "reasonable" steps */
                ASING += (int)(10.0 * (System.Math.Log10((double)LUSOL.m) + 1.0));
                /*NOT REQUIRED
                LUSOL.isingular = (int)lusol.LUSOL_REALLOC(LUSOL.isingular, sizeof(LUSOL.isingular) * (ASING + 1));
                */
                if (LUSOL.isingular == null)
                {
                    LUSOL.luparm[lusol.LUSOL_IP_SINGULARLISTSIZE] = 0;
                    inform = lusol.LUSOL_INFORM_NOMEMLEFT;
                    return false;
                }
                LUSOL.luparm[lusol.LUSOL_IP_SINGULARLISTSIZE] = ASING;

                /* Transfer the first singularity if the list was just created */
                if (NSING == 1)
                {
                    LUSOL.isingular[NSING] = LUSOL.luparm[LUSOL_IP_SINGULARINDEX];
                }
            }

            /* Update singularity count and store its index */
            NSING++;
            if (NSING > 1)
            {
                LUSOL.isingular[0] = NSING;
                LUSOL.isingular[NSING] = singcol;
            }
            LUSOL.luparm[LUSOL_IP_SINGULARITIES] = NSING;

            /* Mimic old logic by keeping the last singularity stored */
            LUSOL.luparm[LUSOL_IP_SINGULARINDEX] = singcol;

            return true;
        }

        internal static char relationChar(double left, double right)
        {
            if (left > right)
            {
                return ('>');
            }
            else if (left == right)
            {
                return ('=');
            }
            else
            {
                return ('<');
            }

        }

        internal static int LUSOL_getSingularity(LUSOLrec LUSOL, int singitem)
        {
            if ((singitem > LUSOL.luparm[LUSOL_IP_SINGULARITIES]) || (singitem < 0))
            {
                singitem = -1;
            }
            else if (singitem == 0)
            {
                singitem = LUSOL.luparm[LUSOL_IP_SINGULARITIES];
            }
            else if (singitem > 1)
            {
                singitem = LUSOL.isingular[singitem];
            }
            else
            {
                singitem = LUSOL.luparm[LUSOL_IP_SINGULARINDEX];
            }
            return (singitem);
        }

        private void LUSOL_free(LUSOLrec LUSOL)
        {
            /*NOT REQUIRED
            LUSOL_realloc_a(LUSOL, 0);
            LUSOL_realloc_r(LUSOL, 0);
            LUSOL_realloc_c(LUSOL, 0);
            */
            if (LUSOL.L0 != null)
            {
                LUSOL_matfree((LUSOL.L0));
            }
            if (LUSOL.U != null)
            {
                LUSOL_matfree((LUSOL.U));
            }
            if (!myblas.is_nativeBLAS())
            {
                myblas.unload_BLAS();
            }
            LUSOL_FREE(LUSOL);
        }

        internal static void LUSOL_matfree(LUSOLmat[] mat)
        {
            if ((mat == null) || (mat[0] == null))
            {
                return;
            }
            // can send whole object and set it to null at once
            /*NOT REQUIRED
            LUSOL_FREE(mat.a);
            LUSOL_FREE(mat.indc);
            LUSOL_FREE(mat.indr);
            LUSOL_FREE(mat.lenx);
            LUSOL_FREE(mat.indx);
            */
            LUSOL_FREE(mat);
        }

        internal int LUSOL_btran(LUSOLrec LUSOL, double[] b, int[] NZidx)
        {
            int inform = 0;
            lusol6a objlusol6a = new lusol6a();

            /* Copy RHS vector, but make adjustment for offset since this
               can create a memory error when the calling program uses
               a 0-base vector offset back to comply with LUSOL. */
            //NOT REQUIRED
            //MEMCOPY(LUSOL.w + 1, b + 1, LUSOL.m);
            if (LUSOL.w != null)
            {
                LUSOL.w[0] = 0;
            }

            objlusol6a.LU6SOL(LUSOL, commonlib.LUSOL_SOLVE_Atv_w, b, LUSOL.w, NZidx, ref inform);
            LUSOL.luparm[INVrec.LUSOL_IP_BTRANCOUNT]++;

            return (inform);
        }


        internal String LUSOL_informstr(LUSOLrec LUSOL, int inform)
        {

            String[] LUSOL_informstr_informText = { "LUSOL_RANKLOSS: Lost rank", "LUSOL_LUSUCCESS: Success", "LUSOL_LUSINGULAR: Singular A", "LUSOL_LUUNSTABLE: Unstable factorization", "LUSOL_ADIMERR: Row or column count exceeded", "LUSOL_ADUPLICATE: Duplicate A matrix entry found", "(Undefined message)", "(Undefined message)", "LUSOL_ANEEDMEM: Insufficient memory for factorization", "LUSOL_FATALERR: Fatal internal error", "LUSOL_NOPIVOT: Found no suitable pivot", "LUSOL_NOMEMLEFT: Could not obtain more memory" };

            if ((inform < commonlib.LUSOL_INFORM_MIN) || (inform > commonlib.LUSOL_INFORM_MAX))
            {
                inform = LUSOL.luparm[commonlib.LUSOL_IP_INFORM];
            }
            return (LUSOL_informstr_informText[inform - commonlib.LUSOL_INFORM_MIN]);
        }


        /*NOT REQUIRED
        internal void LUSOL_free(LUSOLrec LUSOL)
        {
            LUSOL_realloc_a(LUSOL, 0);
            LUSOL_realloc_r(LUSOL, 0);
            LUSOL_realloc_c(LUSOL, 0);
            if (LUSOL.L0 != null)
            {
                LUSOL_matfree((LUSOL.L0));
            }
            if (LUSOL.U != null)
            {
                LUSOL_matfree((LUSOL.U));
            }
            if (!is_nativeBLAS())
            {
                unload_BLAS();
            }
            LUSOL_FREE(LUSOL);
        }
        */

        internal static int LUSOL_tightenpivot(LUSOLrec LUSOL)
        {
#if false
//  REAL newvalue;
#endif

            /* Give up tightening if we are already less than limit and we cannot change strategy */
            if ((double)commonlib.MIN(LUSOL.parmlu[LUSOL_RP_FACTORMAX_Lij], LUSOL.parmlu[LUSOL_RP_UPDATEMAX_Lij]) < 1.1)
            {
                if (LUSOL.luparm[LUSOL_IP_PIVOTTYPE] >= LUSOL_PIVMOD_TRP)
                {
                    return 0;
                }
                LUSOL_setpivotmodel(LUSOL, LUSOL.luparm[LUSOL_IP_PIVOTTYPE] + 1, LUSOL_PIVTOL_DEFAULT + 1);
                return (2);
            }

            /* Otherwise tighten according to defined schedule */
#if false
//  newvalue = sqrt(LUSOL->parmlu[LUSOL_RP_FACTORMAX_Lij]);
//  LUSOL->parmlu[LUSOL_RP_FACTORMAX_Lij] = newvalue;
//  SETMIN(LUSOL->parmlu[LUSOL_RP_UPDATEMAX_Lij], newvalue);
#elif false
//  newvalue = sqrt(LUSOL->parmlu[LUSOL_RP_FACTORMAX_Lij]);
//  LUSOL->parmlu[LUSOL_RP_FACTORMAX_Lij] = newvalue;
//  LUSOL->parmlu[LUSOL_RP_UPDATEMAX_Lij] = 1.0 + (newvalue - 1.0) / 2;
#else
            LUSOL.parmlu[LUSOL_RP_FACTORMAX_Lij] = 1.0 + LUSOL.parmlu[LUSOL_RP_FACTORMAX_Lij] / 3.0;
            LUSOL.parmlu[LUSOL_RP_UPDATEMAX_Lij] = 1.0 + LUSOL.parmlu[LUSOL_RP_UPDATEMAX_Lij] / 3.0;
#endif
            return (1);

        }

        private static void LUSOL_setpivotmodel(LUSOLrec LUSOL, int pivotmodel, int initlevel)
        {
            double newFM;
            double newUM;

            /* Set pivotmodel if specified */
            if (pivotmodel > LUSOL_PIVMOD_NOCHANGE)
            {
                if ((pivotmodel <= LUSOL_PIVMOD_DEFAULT) || (pivotmodel > LUSOL_PIVMOD_MAX))
                {
                    pivotmodel = LUSOL_PIVMOD_TPP;
                }
                LUSOL.luparm[LUSOL_IP_PIVOTTYPE] = pivotmodel;
            }

            /* Check if we need bother about changing tolerances */
            if ((initlevel <= LUSOL_PIVTOL_NOCHANGE) || (initlevel > LUSOL_PIVTOL_MAX))
            {
                return;
            }

            /* Set default pivot tolerances
               (note that UPDATEMAX should always be <= FACTORMAX) */
            if (initlevel == LUSOL_PIVTOL_BAGGY)
            { // Extra-loose pivot thresholds
                newFM = 500.0;
                newUM = newFM / 20;
            }
            else if (initlevel == LUSOL_PIVTOL_LOOSE)
            { // Moderately tight pivot tolerances
                newFM = 100.0;
                newUM = newFM / 10;
            }
            else if (initlevel == LUSOL_PIVTOL_NORMAL)
            { // Standard pivot tolerances
                newFM = 28.0;
                newUM = newFM / 4;
            }
            else if (initlevel == LUSOL_PIVTOL_SLIM)
            { // Better accuracy pivot tolerances
                newFM = 10.0;
                newUM = newFM / 2;
            }
            else if (initlevel == LUSOL_PIVTOL_TIGHT)
            { // Enhanced accuracy pivot tolerances
                newFM = 5.0;
                newUM = newFM / 2;
            }
            else if (initlevel == LUSOL_PIVTOL_SUPER)
            { // Very tight pivot tolerances for extra accuracy
                newFM = 2.5;
                newUM = 1.99;
            }
            else
            { // Extremely tight pivot tolerances for extra accuracy
                newFM = 1.99;
                newUM = newFM / 1.49;
            }

            /* Set the tolerances */
            LUSOL.parmlu[LUSOL_RP_FACTORMAX_Lij] = newFM;
            LUSOL.parmlu[LUSOL_RP_UPDATEMAX_Lij] = newUM;

        }

        //C++ TO C# CONVERTER NOTE: This was formerly a static local variable declaration (not allowed in C#):
        internal static string[] LUSOL_pivotLabel_pivotText = { "TPP", "TRP", "TCP", "TSP" };
        internal static string LUSOL_pivotLabel(LUSOLrec LUSOL)
        {
            //C++ TO C# CONVERTER NOTE: This static local variable declaration (not allowed in C#) has been moved just prior to the method:
            //  static char *pivotText[LUSOL_PIVMOD_MAX+1] = {"TPP", "TRP", "TCP", "TSP"};
            return (LUSOL_pivotLabel_pivotText[LUSOL.luparm[LUSOL_IP_PIVOTTYPE]]);
        }

        internal static void LUSOL_clear(LUSOLrec LUSOL, bool nzonly)
        {
            int len;

            LUSOL.nelem[0] = 0;
            if (!nzonly)
            {

                /* lena arrays */
                len = LUSOL.lena + LUSOL_ARRAYOFFSET;
                /*NOT REQUIRED
                MEMCLEAR(LUSOL.a, len);
                MEMCLEAR(LUSOL.indc, len);
                MEMCLEAR(LUSOL.indr, len);
                */

                /* maxm arrays */
                len = LUSOL.maxm + LUSOL_ARRAYOFFSET;
            
                /*NOT REQUIRED
                MEMCLEAR(LUSOL.lenr, len);
                MEMCLEAR(LUSOL.ip, len);
                MEMCLEAR(LUSOL.iqloc, len);
                MEMCLEAR(LUSOL.ipinv, len);
                MEMCLEAR(LUSOL.locr, len);
                */
#if !ClassicHamaxR
                //C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
                //if ((LUSOL.amaxr != null)) { }
            
    
#if AlwaysSeparateHamaxR
//C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
	   && (LUSOL.luparm[LUSOL_IP_PIVOTTYPE] == LUSOL_PIVMOD_TRP)
#endif
	 //)
/*NOT REQUIRED
  MEMCLEAR(LUSOL.amaxr, len);
  */
                
#endif

                /* maxn arrays */
                len = LUSOL.maxn + LUSOL_ARRAYOFFSET;
                /*NOT REQUIRED
                MEMCLEAR(LUSOL.lenc, len);
                MEMCLEAR(LUSOL.iq, len);
                MEMCLEAR(LUSOL.iploc, len);
                MEMCLEAR(LUSOL.iqinv, len);
                MEMCLEAR(LUSOL.locc, len);
                MEMCLEAR(LUSOL.w, len);
                

                if (LUSOL.luparm[LUSOL_IP_PIVOTTYPE] == LUSOL_PIVMOD_TCP)
                {
                    MEMCLEAR(LUSOL.Ha, len);
                    MEMCLEAR(LUSOL.Hj, len);
                    MEMCLEAR(LUSOL.Hk, len);
                }
#if !ClassicdiagU
                if (LUSOL.luparm[LUSOL_IP_KEEPLU] == 0)
                {
                    MEMCLEAR(LUSOL.diagU, len);
                }
#endif
*/
            }

        }

        internal static int LUSOL_loadColumn(LUSOLrec LUSOL, int[][] iA, int jA, double[] Aij, int nzcount, int offset1)
        {
            int i;
            int ii;
            int nz;
            int k;

            nz = LUSOL.nelem[0];
            i = nz + nzcount;
            if (i > (LUSOL.lena / LUSOL.luparm[LUSOL_IP_SCALAR_NZA]) && !LUSOL_realloc_a(LUSOL, i * LUSOL.luparm[LUSOL_IP_SCALAR_NZA]))
            {
                return (-1);
            }

            k = 0;
            for (ii = 1; ii <= nzcount; ii++)
            {
                i = ii + offset1;
                if (Aij[i] == 0)
                {
                    continue;
                }
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                if (iA[i][0] <= 0 || iA[i][0] > LUSOL.m || jA <= 0 || jA > LUSOL.n)
                {
                    LUSOL_report(LUSOL, 0, "Variable index outside of set bounds (r:%d/%d, c:%d/%d)\n", iA[i], LUSOL.m, jA, LUSOL.n);
                    continue;
                }
                k++;
                nz++;
                LUSOL.a[nz] = Aij[i];
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                LUSOL.indc[nz] = iA[i][0];
                LUSOL.indr[nz] = jA;
            }
            LUSOL.nelem[0] = nz;
            return (k);
        }

        internal static void LUSOL_report(LUSOLrec LUSOL, int msglevel, string format, params object[] LegacyParamArray)
        {
            /*
            va_list ap;
            if (LUSOL == null)
            {
                
                va_start(ap, format);
                vfprintf(stderr, format, ap);
                va_end(ap);
                
             }
        
            else if (msglevel >= 0)
            {
                if (LUSOL.writelog != null)
                {
                    string buff = new string(new char[255]);

                    va_start(ap, format);
                    vsprintf(buff, format, ap);
                    va_end(ap);
                    LUSOL.writelog(LUSOL, LUSOL.loghandle, buff);
                }
                if (LUSOL.outstream != null)
                {
                    va_start(ap, format);
                    vfprintf(LUSOL.outstream, format, ap);
                    va_end(ap);
                    fflush(LUSOL.outstream);
                }
            }
            */
            throw new NotImplementedException();
        }

        internal static bool LUSOL_realloc_a(LUSOLrec LUSOL, int newsize)
        {
            int oldsize;

            if (newsize < 0)
                newsize = (int)(LUSOL.lena + commonlib.MAX(System.Math.Abs(newsize), LUSOL_MINDELTA_a));

            oldsize = LUSOL.lena;
            LUSOL.lena = newsize;
            if (newsize > 0)
                newsize++;
            if (oldsize > 0)
                oldsize++;

            /*NOT REQUIRED
            LUSOL.a = (REAL)clean_realloc(LUSOL.a, sizeof(*(LUSOL.a)), newsize, oldsize);
            LUSOL.indc = (int)clean_realloc(LUSOL.indc, sizeof(*(LUSOL.indc)), newsize, oldsize);
            LUSOL.indr = (int)clean_realloc(LUSOL.indr, sizeof(*(LUSOL.indr)), newsize, oldsize);
            */
            if ((newsize == 0) || ((LUSOL.a != null) && (LUSOL.indc != null) && (LUSOL.indr != null)))
                return true;
            else
                return false;
            
        }

        internal static int LUSOL_factorize(LUSOLrec LUSOL)
        {
            int inform = 0;

            lusol1.LU1FAC(LUSOL, ref inform);
            return (inform);
        }

        internal static bool LUSOL_expand_a(LUSOLrec LUSOL, ref int delta_lena, ref int right_shift)
        {
#if StaticMemAlloc
  return (0);
#else
            int LENA;
            int NFREE;
            int LFREE;

            /* Add expansion factor to avoid having to resize too often/too much;
               (exponential formula suggested by Michael A. Saunders) */
            LENA = LUSOL.lena;
            delta_lena = (int)commonlib.DELTA_SIZE(delta_lena, LENA);

            /* Expand it! */
            if ((delta_lena <= 0) || !LUSOL_realloc_a(LUSOL, LENA + (delta_lena)))
            {
                return false;
            }

            /* Make sure we return the actual memory increase of a */
            delta_lena = LUSOL.lena - LENA;

            /* Shift the used memory area to the right */
            LFREE = right_shift;
            NFREE = LFREE + delta_lena;
            LENA -= LFREE - 1;
            /*NOT REQUIRED
            MEMMOVE(LUSOL.a + NFREE, LUSOL.a + LFREE, LENA);
            MEMMOVE(LUSOL.indr + NFREE, LUSOL.indr + LFREE, LENA);
            MEMMOVE(LUSOL.indc + NFREE, LUSOL.indc + LFREE, LENA);
            */

            /* Also return the new starting position for the used memory area of a */
            right_shift = NFREE;

            LUSOL.expanded_a++;
            return true;
#endif
        }

    }

    /* The main LUSOL data record */
    /* ------------------------------------------------------------------------- */
    public class LUSOLrec
    {
        private const int LUSOL_RP_RESIDUAL_U = 20;
        private const int LUSOL_RP_LASTITEM = LUSOL_RP_RESIDUAL_U;

        /* General data */
        public FileStream outstream; // Output stream, initialized to STDOUT
        public LUSOLlogfunc writelog;
        public object loghandle;
        public LUSOLlogfunc debuginfo;

        /* Parameter storage arrays */
        public int[] luparm = new int[lusol.LUSOL_IP_LASTITEM + 1];
        public double[] parmlu = new double[LUSOL_RP_LASTITEM + 1];

        /* Arrays of length lena+1 */
        public int lena;
        public int[] nelem;
        public int[] indc;
        public int[] indr;
        public double[] a;

        /* Arrays of length maxm+1 (row storage) */
        public int maxm;
        public int m;
        public int[] lenr;
        public int[] ip;
        public int[] iqloc;
        public int[] ipinv;
        public int[] locr;

        /* Arrays of length maxn+1 (column storage) */
        public int maxn;
        public int n;
        public int[] lenc;
        public int[] iq;
        public int[] iploc;
        public int[] iqinv;
        public int[] locc;
        public double[] w;
        public double vLU6L;

        /* List of singular columns, with dynamic size allocation */
        //changed to int[] on 30/11/18
        public int[] isingular;

        /* Extra arrays of length n for TCP and keepLU == FALSE */
        public double[] Ha;
        public double[] diagU;
        public int[] Hj;
        public int[] Hk;

        /* Extra arrays of length m for TRP*/
        public double[] amaxr;
        public LUSOLmat[] L0;
        public LUSOLmat[] U;

        /* Miscellaneous data */
        public int expanded_a;
        public int replaced_c;
        public int replaced_r;
    }

    /* Sparse matrix data */
    public class LUSOLmat
    {
        double a;
        int lenx, indr, indc, indx;
    }

    
}

