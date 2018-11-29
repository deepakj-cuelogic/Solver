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
        public const int LUSOL_RP_SMARTRATIO = 0;
        public const int LUSOL_IP_ROWCOUNT_L0 = 32;
        public const int LUSOL_AUTOORDER = 2;
        public const int LUSOL_IP_ACCELERATION = 7;
        public const int LUSOL_BASEORDER = 0;
        public const int LUSOL_ACCELERATE_L0 = 4;
        public const int LUSOL_INFORM_NOMEMLEFT = 10;


        //public const int LUSOL_RP_SMARTRATIO = 0;
        public const int ZERO = 0;
        public const int LUSOL_IP_FTRANCOUNT = 30;
        public const int LUSOL_ACCELERATE_U = 8;
        public const int LUSOL_IP_RANK_U = 16;
        public const int LUSOL_IP_NONZEROS_U = 24;
        public const int LUSOL_INFORM_LUSINGULAR = 1;
        public const int LUSOL_RP_RESIDUAL_U = 20;

        public const int LUSOL_INFORM_LUSUCCESS = 0;

        /* luparm OUTPUT parameters: */
        public const int LUSOL_IP_SINGULARITIES = 11;

        internal int LUSOL_getSingularity(LUSOLrec LUSOL, int singitem)
        {
            throw new NotImplementedException();
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

    }

    /* The main LUSOL data record */
    /* ------------------------------------------------------------------------- */
    public class LUSOLrec
    {
        private const int LUSOL_IP_ROWCOUNT_L0 = 32;
        private const int LUSOL_IP_LASTITEM = LUSOL_IP_ROWCOUNT_L0;
        private const int LUSOL_RP_RESIDUAL_U = 20;
        private const int LUSOL_RP_LASTITEM = LUSOL_RP_RESIDUAL_U;

        /* General data */
        public FileStream outstream; // Output stream, initialized to STDOUT
        public LUSOLlogfunc writelog;
        public object loghandle;
        public LUSOLlogfunc debuginfo;

        /* Parameter storage arrays */
        public int[] luparm = new int[LUSOL_IP_LASTITEM + 1];
        public double[] parmlu = new double[LUSOL_RP_LASTITEM + 1];

        /* Arrays of length lena+1 */
        public int lena;
        public int nelem;
        public int indc;
        public int indr;
        public double a;

        /* Arrays of length maxm+1 (row storage) */
        public int maxm;
        public int m;
        public int lenr;
        public int ip;
        public int iqloc;
        public int ipinv;
        public int locr;

        /* Arrays of length maxn+1 (column storage) */
        public int maxn;
        public int n;
        public int lenc;
        public int iq;
        public int iploc;
        public int iqinv;
        public int locc;
        public double w;
        public double vLU6L;

        /* List of singular columns, with dynamic size allocation */
        public int isingular;

        /* Extra arrays of length n for TCP and keepLU == FALSE */
        public double Ha;
        public double diagU;
        public int Hj;
        public int Hk;

        /* Extra arrays of length m for TRP*/
        public double amaxr;

        /* Extra array for L0 and U stored by row/column for faster btran/ftran */
        public LUSOLmat L0;
        public LUSOLmat U;

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

