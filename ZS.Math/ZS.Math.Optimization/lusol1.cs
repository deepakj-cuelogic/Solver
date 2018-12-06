#define FastMXR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class lusol1
    {
        private static void LU1OR2(LUSOLrec LUSOL)
        {
            double ACE;
            double ACEP;
            int L;
            int J;
            int I;
            int JCE;
            int ICE;
            int ICEP;
            int JCEP;
            int JA;
            int JB;

            /*      Set  loc(j)  to point to the beginning of column  j. */
            L = 1;
            for (J = 1; J <= LUSOL.n; J++)
            {
                LUSOL.locc[J] = L;
                L += LUSOL.lenc[J];
            }
            /*      Sort the elements into column order.
                    The algorithm is an in-place sort and is of order  numa. */
            for (I = 1; I <= LUSOL.nelem[0]; I++)
            {
                /*         Establish the current entry. */
                JCE = LUSOL.indr[I];
                if (JCE == 0)
                {
                    continue;
                }
                ACE = LUSOL.a[I];
                ICE = LUSOL.indc[I];
                LUSOL.indr[I] = 0;
                /*         Chain from current entry. */
                for (J = 1; J <= LUSOL.nelem[0]; J++)
                {
                    /*            The current entry is not in the correct position.
                                  Determine where to store it. */
                    L = LUSOL.locc[JCE];
                    LUSOL.locc[JCE]++;
                    /*            Save the contents of that location. */
                    ACEP = LUSOL.a[L];
                    ICEP = LUSOL.indc[L];
                    JCEP = LUSOL.indr[L];
                    /*            Store current entry. */
                    LUSOL.a[L] = ACE;
                    LUSOL.indc[L] = ICE;
                    LUSOL.indr[L] = 0;
                    /*            If next current entry needs to be processed,
                                  copy it into current entry. */
                    if (JCEP == 0)
                    {
                        break;
                    }
                    ACE = ACEP;
                    ICE = ICEP;
                    JCE = JCEP;
                }
            }
            /*      Reset loc(j) to point to the start of column j. */
            JA = 1;
            for (J = 1; J <= LUSOL.n; J++)
            {
                JB = LUSOL.locc[J];
                LUSOL.locc[J] = JA;
                JA = JB;
            }

        }

        /* ==================================================================
   lu1or3  looks for duplicate elements in an  m by n  matrix  A
   defined by the column list  indc, lenc, locc.
   iw  is used as a work vector of length  m.
   ------------------------------------------------------------------
   xx Feb 1985: Original version.
   17 Oct 2000: indc, indr now have size lena to allow nelem = 0.
   ================================================================== */
        private static void LU1OR3(LUSOLrec LUSOL, ref int LERR, ref int INFORM)
        {
            int I;
            int J;
            int L1;
            int L2;
            int L;

#if LUSOLFastClear
  MEMCLEAR((LUSOL.ip + 1), LUSOL.m);
#else
            for (I = 1; I <= LUSOL.m; I++)
            {
                LUSOL.ip[I] = 0;
            }
#endif

            for (J = 1; J <= LUSOL.n; J++)
            {
                if (LUSOL.lenc[J] > 0)
                {
                    L1 = LUSOL.locc[J];
                    L2 = (L1 + LUSOL.lenc[J]) - 1;
                    for (L = L1; L <= L2; L++)
                    {
                        I = LUSOL.indc[L];
                        if (LUSOL.ip[I] == J)
                        {
                            goto x910;
                        }
                        LUSOL.ip[I] = J;
                    }
                }
            }
            INFORM = lusol.LUSOL_INFORM_LUSUCCESS;
            return;
            x910:
            LERR = L;
            INFORM = lusol.LUSOL_INFORM_LUSINGULAR;
        }

        /* ==================================================================
   lu1or4 constructs a row list  indr, locr
   from a corresponding column list  indc, locc,
   given the lengths of both columns and rows in  lenc, lenr.
   ------------------------------------------------------------------
   xx Feb 1985: Original version.
   17 Oct 2000: indc, indr now have size lena to allow nelem = 0.
   ================================================================== */
        private static void LU1OR4(LUSOLrec LUSOL)
        {
            int L;
            int I;
            int L2;
            int J;
            int JDUMMY;
            int L1;
            int LR;

            /*      Initialize  locr(i)  to point just beyond where the
                    last component of row  i  will be stored. */
            L = 1;
            for (I = 1; I <= LUSOL.m; I++)
            {
                L += LUSOL.lenr[I];
                LUSOL.locr[I] = L;
            }
            /*      By processing the columns backwards and decreasing  locr(i)
                    each time it is accessed, it will end up pointing to the
                    beginning of row  i  as required. */
            L2 = LUSOL.nelem[0];
            J = LUSOL.n + 1;
            for (JDUMMY = 1; JDUMMY <= LUSOL.n; JDUMMY++)
            {
                J = J - 1;
                if (LUSOL.lenc[J] > 0)
                {
                    L1 = LUSOL.locc[J];
                    for (L = L1; L <= L2; L++)
                    {
                        I = LUSOL.indc[L];
                        LR = LUSOL.locr[I] - 1;
                        LUSOL.locr[I] = LR;
                        LUSOL.indr[LR] = J;
                    }
                    L2 = L1 - 1;
                }
            }
        }

        /* ==================================================================
   lu1pq1  constructs a permutation  iperm  from the array  len.
   ------------------------------------------------------------------
   On entry:
   len(i)  holds the number of nonzeros in the i-th row (say)
           of an m by n matrix.
   num(*)  can be anything (workspace).

   On exit:
   iperm   contains a list of row numbers in the order
           rows of length 0,  rows of length 1,..., rows of length n.
   loc(nz) points to the first row containing  nz  nonzeros,
           nz = 1, n.
   inv(i)  points to the position of row i within iperm(*).
   ================================================================== */
        private static void LU1PQ1(LUSOLrec LUSOL, int M, int N, int[] LEN, int[] IPERM, int[] LOC, int[] INV, int[] NUM)
        {
            int NZEROS;
            int NZ;
            int I;
            int L;

            /*      Count the number of rows of each length. */
            NZEROS = 0;
            for (NZ = 1; NZ <= N; NZ++)
            {
                NUM[NZ] = 0;
                LOC[NZ] = 0;
            }
            for (I = 1; I <= M; I++)
            {
                NZ = LEN[I];
                if (NZ == 0)
                {
                    NZEROS++;
                }
                else
                {
                    NUM[NZ]++;
                }
            }
            /*      Set starting locations for each length. */
            L = NZEROS + 1;
            for (NZ = 1; NZ <= N; NZ++)
            {
                LOC[NZ] = L;
                L += NUM[NZ];
                NUM[NZ] = 0;
            }
            /*      Form the list. */
            NZEROS = 0;
            for (I = 1; I <= M; I++)
            {
                NZ = LEN[I];
                if (NZ == 0)
                {
                    NZEROS++;
                    IPERM[NZEROS] = I;
                }
                else
                {
                    L = LOC[NZ] + NUM[NZ];
                    IPERM[L] = I;
                    NUM[NZ]++;
                }
            }
            /*      Define the inverse of iperm. */
            for (L = 1; L <= M; L++)
            {
                I = IPERM[L];
                INV[I] = L;
            }
        }

        //C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
        private static void LU1FAD(LUSOLrec LUSOL,
#if ClassicHamaxR
//C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
			int LENA2, int LENH, REAL HA[], int HJ[], int HK[], REAL AMAXR[],
#endif
           ref int INFORM, ref int LENL, ref int LENU, ref int MINLEN, ref int MERSUM, ref int NUTRI, ref int NLTRI, ref int NDENS1, ref int NDENS2, ref int NRANK, ref double LMAX, ref double UMAX, ref double DUMAX, ref double DUMIN, ref double AKMAX)
        {
            bool UTRI;
            bool LTRI;
            bool SPARS1;
            bool SPARS2;
            bool DENSE;
            bool DENSLU;
            bool KEEPLU;
            bool TCP;
            bool TPP;
            bool TRP;
            bool TSP;
            int HLEN=0;
            int HOPS;
            int H=0;
            int LPIV;
            int LPRINT;
            int MAXCOL;
            int MAXROW;
            int ILAST;
            int JLAST;
            int LFILE;
            int LROW;
            int LCOL;
            int MINMN;
            int MAXMN;
            int NZLEFT;
            int NSPARE;
            int LU1;
            int KK;
            int J;
            int LC;
            int MLEFT;
            int NLEFT;
            int NROWU;
            int LQ1;
            int LQ2;
            int JBEST = 0;
            int LQ;
            int I;
            int IBEST = 0;
            int MBEST = 0;
            int LEND=0;
            int NFREE;
            double LD=0;
            int NCOLD;
            int NROWD;
            int MELIM;
            int NELIM;
            int JMAX;
            int IMAX;
            int LL1;
            int LSAVE;
            int LFREE;
            int LIMIT;
            int MINFRE;
            int LPIVR;
            int LPIVR1;
            int LPIVR2;
            int L;
            int LPIVC;
            int LPIVC1;
            int LPIVC2;
            int KBEST;
            int LU;
            int LR;
            int LENJ;
            int LC1;
            int LAST;
            int LL;
            int LS;
            int LENI;
            int LR1;
            int LFIRST;
            int NFILL;
            int NZCHNG=0;
            int K;
            int MRANK=0;
            int NSING=0;
            double LIJ;
            double LTOL;
            double SMALL;
            double USPACE;
            double DENS1;
            double DENS2;
            double AIJMAX=0;
            double AIJTOL=0;
            double AMAX;
            double ABEST;
            double DIAG;
            double V;
            LUSOLrec objLUSOLrec = new LUSOLrec();

#if ClassicHamaxR
  int LDIAGU;
#else
            int LENA2 = LUSOL.lena;
#endif

#if UseTimer
  int eltime;
  int mktime;
  int ntime;
  timer("start", 3);
  ntime = LUSOL.n / 4;
#endif

#if ForceInitialization
  AIJMAX = 0;
  AIJTOL = 0;
  HLEN = 0;
  JBEST = 0;
  IBEST = 0;
  MBEST = 0;
  LEND = 0;
  LD = 0;
#endif

            LPRINT = LUSOL.luparm[lusol.LUSOL_IP_PRINTLEVEL];
            MAXCOL = LUSOL.luparm[lusol.LUSOL_IP_MARKOWITZ_MAXCOL];
            LPIV = LUSOL.luparm[lusol.LUSOL_IP_PIVOTTYPE];
            KEEPLU = (bool)(LUSOL.luparm[lusol.LUSOL_IP_KEEPLU] != 0);
            /*      Threshold Partial   Pivoting (normal). */
            TPP = (bool)(LPIV == lusol.LUSOL_PIVMOD_TPP);
            /*      Threshold Rook      Pivoting */
            TRP = (bool)(LPIV == lusol.LUSOL_PIVMOD_TRP);
            /*      Threshold Complete  Pivoting. */
            TCP = (bool)(LPIV == lusol.LUSOL_PIVMOD_TCP);
            /*      Threshold Symmetric Pivoting. */
            TSP = (bool)(LPIV == lusol.LUSOL_PIVMOD_TSP);
            DENSLU = false;
            MAXROW = MAXCOL - 1;
            /*      Assume row m is last in the row file. */
            ILAST = LUSOL.m;
            /*      Assume col n is last in the col file. */
            JLAST = LUSOL.n;
            LFILE = LUSOL.nelem[0];
            LROW = LUSOL.nelem[0];
            LCOL = LUSOL.nelem[0];
            MINMN = (int)commonlib.MIN(LUSOL.m, LUSOL.n);
            MAXMN = (int)commonlib.MAX(LUSOL.m, LUSOL.n);
            NZLEFT = LUSOL.nelem[0];
            NSPARE = 1;

            if (KEEPLU)
            {
                LU1 = LENA2 + 1;
            }
            else
            {
                /*         Store only the diagonals of U in the top of memory. */
#if ClassicdiagU
	LDIAGU = LENA2 - LUSOL.n;
	LU1 = LDIAGU + 1;
	LUSOL.diagU = LUSOL.a + LDIAGU;
#else
                LU1 = LENA2 + 1;
#endif
            }
            LTOL = LUSOL.parmlu[lusol.LUSOL_RP_FACTORMAX_Lij];
            SMALL = LUSOL.parmlu[lusol.LUSOL_RP_ZEROTOLERANCE];
            USPACE = LUSOL.parmlu[lusol.LUSOL_RP_COMPSPACE_U];
            DENS1 = LUSOL.parmlu[lusol.LUSOL_RP_MARKOWITZ_CONLY];
            DENS2 = LUSOL.parmlu[lusol.LUSOL_RP_MARKOWITZ_DENSE];
            UTRI = true;
            LTRI = false;
            SPARS1 = false;
            SPARS2 = false;
            DENSE = false;
            /*      Check parameters. */
            commonlib.SETMAX(LTOL, 1.0001E+0);
            commonlib.SETMIN(DENS1, DENS2);
            /*      Initialize output parameters.
                    lenL, lenU, minlen, mersum, nUtri, nLtri, ndens1, ndens2, nrank
                    are already initialized by lu1fac. */
            LMAX = 0;
            UMAX = 0;
            DUMAX = 0;
            DUMIN = lusol.LUSOL_BIGNUM;
            if (LUSOL.nelem[0] == 0)
            {
                DUMIN = 0;
            }
            AKMAX = 0;
            HOPS = 0;
            /*      More initialization.
                    Don't worry yet about lu1mxc. */
            if (TPP || TSP)
            {
                AIJMAX = 0;
                AIJTOL = 0;
                HLEN = 1;
                /*      TRP or TCP */
            }
            else
            {
                /*      Move biggest element to top of each column.
                        Set w(*) to mark slack columns (unit vectors). */
                LU1MXC(LUSOL, 1, LUSOL.n, LUSOL.iq);
                LU1SLK(LUSOL);
            }
            if (TRP) 
                /*      Find biggest element in each row. */
#if ClassicHamaxR
	LU1MXR(LUSOL, 1,LUSOL.m,LUSOL.ip,AMAXR);
  }
#else           
                LU1MXR(LUSOL, 1, LUSOL.m, LUSOL.ip, objLUSOLrec.amaxr);
            
#endif

            if (TCP)
            {
                /*      Set Ha(1:Hlen) = biggest element in each column,
                            Hj(1:Hlen) = corresponding column indices. */
                HLEN = 0;
                for (KK = 1; KK <= LUSOL.n; KK++)
                {
                    HLEN++;
                    J = LUSOL.iq[KK];
                    LC = LUSOL.locc[J];
#if ClassicHamaxR
	  HA[HLEN] = Math.Abs(LUSOL.a[LC]);
	  HJ[HLEN] = J;
	  HK[J] = HLEN;
#else
                    LUSOL.Ha[HLEN] = System.Math.Abs(LUSOL.a[LC]);
                    LUSOL.Hj[HLEN] = J;
                    LUSOL.Hk[J] = HLEN;
#endif
                }
                /*      Build the heap, creating new Ha, Hj and setting Hk(1:Hlen). */
#if ClassicHamaxR
	HBUILD(HA, HJ, HK, HLEN, HOPS);
#else
                lusol2.HBUILD(LUSOL.Ha, LUSOL.Hj, LUSOL.Hk, HLEN, ref HOPS);
#endif
            }
            /*      ------------------------------------------------------------------
                    Start of main loop.
                    ------------------------------------------------------------------ */
            MLEFT = LUSOL.m + 1;
            NLEFT = LUSOL.n + 1;
            for (NROWU = 1; NROWU <= MINMN; NROWU++)
            {
#if UseTimer
	mktime = (nrowu / ntime) + 4;
	eltime = (nrowu / ntime) + 9;
#endif
                MLEFT--;
                NLEFT--;
                /*         Bail out if there are no nonzero rows left. */
                if (LUSOL.iploc[1] > LUSOL.m)
                {
                    goto x900;
                }
                /*      For TCP, the largest Aij is at the top of the heap. */
                if (TCP)
                {
                    /*
                                  Marvelously easy */
#if ClassicHamaxR
	  AIJMAX = HA[1];
#else
                    AIJMAX = LUSOL.Ha[1];
#endif
                    commonlib.SETMAX(AKMAX, AIJMAX);
                    AIJTOL = AIJMAX / LTOL;
                }

                /*         ===============================================================
                           Find a suitable pivot element.
                           =============================================================== */
                if (UTRI)
                {
                    /*            ------------------------------------------------------------
                                  So far all columns have had length 1.
                                  We are still looking for the (backward) triangular part of A
                                  that forms the first rows and columns of U.
                                  ------------------------------------------------------------ */
                    LQ1 = LUSOL.iqloc[1];
                    LQ2 = LUSOL.n;
                    if (LUSOL.m > 1)
                    {
                        LQ2 = LUSOL.iqloc[2] - 1;
                    }
                    /*      There are more cols of length 1. */
                    if (LQ1 <= LQ2)
                    {
                        if (TPP || TSP)
                        {
                            /*      Grab the first one. */
                            JBEST = LUSOL.iq[LQ1];
                            /*      Scan all columns of length 1 ... TRP or TCP */
                        }
                        else
                        {
                            JBEST = 0;
                            for (LQ = LQ1; LQ <= LQ2; LQ++)
                            {
                                J = LUSOL.iq[LQ];
                                /*      Accept a slack */
                                if (LUSOL.w[J] > 0)
                                {
                                    JBEST = J;
                                    goto x250;
                                }
                                LC = LUSOL.locc[J];
                                AMAX = System.Math.Abs(LUSOL.a[LC]);
                                if (TRP)
                                {
                                    I = LUSOL.indc[LC];
#if ClassicHamaxR
			  AIJTOL = AMAXR[I] / LTOL;
#else
                                    AIJTOL = LUSOL.amaxr[I] / LTOL;
#endif
                                }
                                if (AMAX >= AIJTOL)
                                {
                                    JBEST = J;
                                    goto x250;
                                }
                            }
                        }
                        x250:
                        if (JBEST > 0)
                        {
                            LC = LUSOL.locc[JBEST];
                            IBEST = LUSOL.indc[LC];
                            MBEST = 0;
                            goto x300;
                        }
                    }
                    /*            This is the end of the U triangle.
                                  We will not return to this part of the code.
                                  TPP and TSP call lu1mxc for the first time
                                  (to move biggest element to top of each column). */
                    if (LPRINT >= lusol.LUSOL_MSG_PIVOT)
                    {
                        lusol.LUSOL_report(LUSOL, 0, "Utri ended.  spars1 = TRUE\n");
                    }
                    UTRI = false;
                    LTRI = true;
                    SPARS1 = true;
                    NUTRI = NROWU - 1;
                    if (TPP || TSP)
                    {
                        LU1MXC(LUSOL, LQ1, LUSOL.n, LUSOL.iq);
                    }
                }
                if (SPARS1)
                {
                    /*            ------------------------------------------------------------
                                  Perform a Markowitz search.
                                  Search cols of length 1, then rows of length 1,
                                  then   cols of length 2, then rows of length 2, etc.
                                  ------------------------------------------------------------ */
#if UseTimer
		timer("start", mktime);
#endif
                    /*      12 Jun 2002: Next line disables lu1mCP below
                                  if (TPP) then */
                    if (TPP || TCP)
                    {
                        LU1MAR(LUSOL, MAXMN, TCP, AIJTOL, LTOL, MAXCOL, MAXROW, ref IBEST, ref JBEST, ref MBEST);
                    }
                    else if (TRP)
                    {
#if ClassicHamaxR
		LU1MRP(LUSOL, MAXMN, LTOL, MAXCOL, MAXROW, IBEST, JBEST, MBEST, AMAXR);
#else
                        LU1MRP(LUSOL, MAXMN, LTOL, MAXCOL, MAXROW, ref IBEST, ref JBEST, ref MBEST, LUSOL.amaxr);
#endif
                        /*      else if (TCP) {
                                lu1mCP( m    , n     , lena  , aijtol,
                                              ibest, jbest , mbest ,
                                              a    , indc  , indr  ,
                                              lenc , lenr  , locc  ,
                                              Hlen , Ha    , Hj    ) */
                    }
                    else if (TSP)
                    {
                        LU1MSP(LUSOL, MAXMN, LTOL, MAXCOL, ref IBEST, ref JBEST, ref MBEST);
                        if (IBEST == 0)
                        {
                            goto x990;
                        }
                    }
#if UseTimer
	  timer("finish", mktime);
#endif
                    if (LTRI)
                    {
                        /*               So far all rows have had length 1.
                                         We are still looking for the (forward) triangle of A
                                         that forms the first rows and columns of L. */
                        if (MBEST > 0)
                        {
                            LTRI = false;
                            NLTRI = NROWU - 1 - NUTRI;
                            if (LPRINT >= lusol.LUSOL_MSG_PIVOT)
                            {
                                lusol.LUSOL_report(LUSOL, 0, "Ltri ended.\n");
                            }
                        }
                    }
                    else
                    {
                        /*               See if what's left is as dense as dens1. */
                        if (NZLEFT >= (DENS1 * MLEFT) * NLEFT)
                        {
                            SPARS1 = false;
                            SPARS2 = true;
                            NDENS1 = NLEFT;
                            MAXROW = 0;
                            if (LPRINT >= lusol.LUSOL_MSG_PIVOT)
                            {
                                lusol.LUSOL_report(LUSOL, 0, "spars1 ended.  spars2 = TRUE\n");
                            }
                        }
                    }
                }
                else if (SPARS2 || DENSE)
                {
                    /*            ------------------------------------------------------------
                                  Perform a restricted Markowitz search,
                                  looking at only the first maxcol columns.  (maxrow = 0.)
                                  ------------------------------------------------------------ */
#if UseTimer
	  timer("start", mktime);
#endif
                    /*      12 Jun 2002: Next line disables lu1mCP below
                                  if (TPP) then */
                    if (TPP || TCP)
                    {
                        LU1MAR(LUSOL, MAXMN, TCP, AIJTOL, LTOL, MAXCOL, MAXROW, ref IBEST, ref JBEST, ref MBEST);
                    }
                    else if (TRP)
                    {
#if ClassicHamaxR
		LU1MRP(LUSOL, MAXMN, LTOL, MAXCOL, MAXROW, IBEST, JBEST, MBEST, AMAXR);
#else
                        LU1MRP(LUSOL, MAXMN, LTOL, MAXCOL, MAXROW, ref IBEST, ref JBEST, ref MBEST, LUSOL.amaxr);
#endif
                        /*      else if (TCP) {
                                lu1mCP( m    , n     , lena  , aijtol,
                                              ibest, jbest , mbest ,
                                              a    , indc  , indr  ,
                                              lenc , lenr  , locc  ,
                                              Hlen , Ha    , Hj    ) */
                    }
                    else if (TSP)
                    {
                        LU1MSP(LUSOL, MAXMN, LTOL, MAXCOL, ref IBEST, ref JBEST, ref MBEST);
                        if (IBEST == 0)
                        {
                            goto x985;
                        }
                    }
#if UseTimer
	  timer("finish", mktime);
#endif
                    /*            See if what's left is as dense as dens2. */
                    if (SPARS2)
                    {
                        if (NZLEFT >= (DENS2 * MLEFT) * NLEFT)
                        {
                            SPARS2 = false;
                            DENSE = true;
                            NDENS2 = NLEFT;
                            MAXCOL = 1;
                            if (LPRINT >= lusol.LUSOL_MSG_PIVOT)
                            {
                                lusol.LUSOL_report(LUSOL, 0, "spars2 ended.  dense = TRUE\n");
                            }
                        }
                    }
                }
                /*         ---------------------------------------------------------------
                           See if we can finish quickly.
                           --------------------------------------------------------------- */
                if (DENSE)
                {
                    LEND = MLEFT * NLEFT;
                    NFREE = LU1 - 1;
                    if (NFREE >= 2 * LEND)
                    {
                        /*               There is room to treat the remaining matrix as
                                         a dense matrix D.
                                         We may have to compress the column file first.
                                         12 Nov 1999: D used to be put at the
                                                      beginning of free storage (lD = lcol + 1).
                                                      Now put it at the end     (lD = lu1 - lenD)
                                                      so the left-shift in lu1ful will not
                                                      involve overlapping storage
                                                      (fatal with parallel dcopy).
                           */
                        DENSLU = true;
                        NDENS2 = NLEFT;
                        LD = LU1 - LEND;
                        if (LCOL >= LD)
                        {
                            LU1REC(LUSOL, LUSOL.n, true, ref LCOL, LUSOL.indc, LUSOL.lenc, LUSOL.locc);
                            LFILE = LCOL;
                            JLAST = LUSOL.indc[LCOL + 1];
                        }
                        goto x900;
                    }
                }
                /*         ===============================================================
       The best  aij  has been found.
       The pivot row  ibest  and the pivot column  jbest
       Define a dense matrix  D  of size  nrowd  by  ncold.
       =============================================================== */
                x300:
                NCOLD = LUSOL.lenr[IBEST];
                NROWD = LUSOL.lenc[JBEST];
                MELIM = NROWD - 1;
                NELIM = NCOLD - 1;
                (MERSUM) += MBEST;
                (LENL) += MELIM;
                (LENU) += NCOLD;
                if (LPRINT >= lusol.LUSOL_MSG_PIVOT)
                {
                    if (NROWU == 1)
                    {
                        lusol.LUSOL_report(LUSOL, 0, "lu1fad debug:\n");
                    }
                    if (TPP || TRP || TSP)
                    {
                        lusol.LUSOL_report(LUSOL, 0, "nrowu:%7d   i,jbest:%7d,%7d   nrowd,ncold:%6d,%6d\n", NROWU, IBEST, JBEST, NROWD, NCOLD);
                        /*      TCP */
                    }
                    else
                    {
#if ClassicHamaxR
		JMAX = HJ[1];
#else
                        JMAX = LUSOL.Hj[1];
#endif
                        IMAX = LUSOL.indc[LUSOL.locc[JMAX]];
                        lusol.LUSOL_report(LUSOL, 0, "nrowu:%7d   i,jbest:%7d,%7d   nrowd,ncold:%6d,%6d   i,jmax:%7d,%7d   aijmax:%g\n", NROWU, IBEST, JBEST, NROWD, NCOLD, IMAX, JMAX, AIJMAX);
                    }
                }
                /*         ===============================================================
                           Allocate storage for the next column of  L  and next row of  U.
                           Initially the top of a, indc, indr are used as follows:
                                      ncold       melim       ncold        melim
                           a      |...........|...........|ujbest..ujn|li1......lim|
                           indc   |...........|  lenr(i)  |  lenc(j)  |  markl(i)  |
                           indr   |...........| iqloc(i)  |  jfill(j) |  ifill(i)  |
                                 ^           ^             ^           ^            ^
                                 lfree   lsave             lu1         ll1          oldlu1
                           Later the correct indices are inserted:
                           indc   |           |           |           |i1........im|
                           indr   |           |           |jbest....jn|ibest..ibest|
                           =============================================================== */
                if (!KEEPLU)
                {
                    /*            Always point to the top spot.
                                  Only the current column of L and row of U will
                                  take up space, overwriting the previous ones. */
#if ClassicHamaxR
	  LU1 = LDIAGU + 1;
#else
                    LU1 = LENA2 + 1;
#endif
                }
                /* Update (left-shift) pointers to make room for the new data */
                LL1 = LU1 - MELIM;
                LU1 = LL1 - NCOLD;
                LSAVE = LU1 - NROWD;
                LFREE = LSAVE - NCOLD;

                /* Check if we need to allocate more memory, and allocate if necessary */
#if false
//    L = NROWD*NCOLD;
//
// /* Try to avoid future expansions by anticipating further updates - KE extension */
//    if(LUSOL->luparm[LUSOL_IP_UPDATELIMIT] > 0)
//#if 1
//      L *= (int) (log(LUSOL->luparm[LUSOL_IP_UPDATELIMIT]-LUSOL->luparm[LUSOL_IP_UPDATECOUNT]+2.0) + 1);
//#else
//      L *= (LUSOL->luparm[LUSOL_IP_UPDATELIMIT]-LUSOL->luparm[LUSOL_IP_UPDATECOUNT]) / 2 + 1;
//#endif
//
#else
                L = (KEEPLU ? (int)commonlib.MAX(LROW, LCOL) + 2 * (LUSOL.m + LUSOL.n) : 0);
                L *= lusol.LUSOL_MULT_nz_a;
                commonlib.SETMAX(L, NROWD * NCOLD);
#endif

                /* Do the memory expansion */
                if ((L > LFREE - LCOL) && lusol.LUSOL_expand_a(LUSOL, ref L, ref LFREE))
                {
                    LL1 += L;
                    LU1 += L;
                    LSAVE += L;
#if ClassicdiagU
	  LUSOL.diagU += L;
#endif
#if ClassicHamaxR
	  HA += L;
	  HJ += L;
	  HK += L;
	  AMAXR += L;
#endif
                }
                LIMIT = (int)(USPACE * LFILE) + LUSOL.m + LUSOL.n + 1000;

                /*         Make sure the column file has room.
                           Also force a compression if its length exceeds a certain limit. */
#if StaticMemAlloc
	MINFRE = NCOLD + MELIM;
#else
                MINFRE = NROWD * NCOLD;
#endif
                NFREE = LFREE - LCOL;
                if (NFREE < MINFRE || LCOL > LIMIT)
                {
                    LU1REC(LUSOL, LUSOL.n, true, ref LCOL, LUSOL.indc, LUSOL.lenc, LUSOL.locc);
                    LFILE = LCOL;
                    JLAST = LUSOL.indc[LCOL + 1];
                    NFREE = LFREE - LCOL;
                    if (NFREE < MINFRE)
                    {
                        goto x970;
                    }
                }
                /*         Make sure the row file has room. */
#if StaticMemAlloc
	MINFRE = NCOLD + MELIM;
#else
                MINFRE = NROWD * NCOLD;
#endif
                NFREE = LFREE - LROW;
                if (NFREE < MINFRE || LROW > LIMIT)
                {
                    LU1REC(LUSOL, LUSOL.m, false, ref LROW, LUSOL.indr, LUSOL.lenr, LUSOL.locr);
                    LFILE = LROW;
                    ILAST = LUSOL.indr[LROW + 1];
                    NFREE = LFREE - LROW;
                    if (NFREE < MINFRE)
                    {
                        goto x970;
                    }
                }
                /*         ===============================================================
                           Move the pivot element to the front of its row
                           and to the top of its column.
                           =============================================================== */
                LPIVR = LUSOL.locr[IBEST];
                LPIVR1 = LPIVR + 1;
                LPIVR2 = LPIVR + NELIM;
                for (L = LPIVR; L <= LPIVR2; L++)
                {
                    if (LUSOL.indr[L] == JBEST)
                    {
                        break;
                    }
                }

                LUSOL.indr[L] = LUSOL.indr[LPIVR];
                LUSOL.indr[LPIVR] = JBEST;
                LPIVC = LUSOL.locc[JBEST];
                LPIVC1 = LPIVC + 1;
                LPIVC2 = LPIVC + MELIM;
                for (L = LPIVC; L <= LPIVC2; L++)
                {
                    if (LUSOL.indc[L] == IBEST)
                    {
                        break;
                    }
                }
                LUSOL.indc[L] = LUSOL.indc[LPIVC];
                LUSOL.indc[LPIVC] = IBEST;
                ABEST = LUSOL.a[L];
                LUSOL.a[L] = LUSOL.a[LPIVC];
                LUSOL.a[LPIVC] = ABEST;
                if (!KEEPLU)
                {
                    /*            Store just the diagonal of U, in natural order.
                       !!         a[ldiagU + nrowu] = abest ! This was in pivot order. */
                    objLUSOLrec.diagU[JBEST] = ABEST;
                }

                /*     ==============================================================
                        Delete pivot col from heap.
                        Hk tells us where it is in the heap.
                       ============================================================== */
                if (TCP)
                {
#if ClassicHamaxR
	  KBEST = HK[JBEST];
	  HDELETE(HA, HJ, HK, HLEN, KBEST, H);
#else
                    KBEST = LUSOL.Hk[JBEST];
                    lusol2.HDELETE(LUSOL.Ha, LUSOL.Hj, LUSOL.Hk, ref HLEN, KBEST, ref H);
#endif
                    HOPS += H;
                }
                /*         ===============================================================
                           Delete the pivot row from the column file
                           and store it as the next row of  U.
                           set  indr(lu) = 0     to initialize jfill ptrs on columns of D,
                                indc(lu) = lenj  to save the original column lengths.
                           =============================================================== */
                LUSOL.a[LU1] = ABEST;
                LUSOL.indr[LU1] = JBEST;
                LUSOL.indc[LU1] = NROWD;
                LU = LU1;
                DIAG = System.Math.Abs(ABEST);
                commonlib.SETMAX(UMAX, DIAG);
                commonlib.SETMAX(DUMAX, DIAG);
                commonlib.SETMIN(DUMIN, DIAG);

                for (LR = LPIVR1; LR <= LPIVR2; LR++)
                {
                    LU++;
                    J = LUSOL.indr[LR];
                    LENJ = LUSOL.lenc[J];
                    LUSOL.lenc[J] = LENJ - 1;
                    LC1 = LUSOL.locc[J];
                    LAST = LC1 + LUSOL.lenc[J];
                    for (L = LC1; L <= LAST; L++)
                    {
                        if (LUSOL.indc[L] == IBEST)
                        {
                            break;
                        }
                    }
                    LUSOL.a[LU] = LUSOL.a[L];
                    LUSOL.indr[LU] = 0;
                    LUSOL.indc[LU] = LENJ;
                    commonlib.SETMAX(UMAX, System.Math.Abs(LUSOL.a[LU]));
                    LUSOL.a[L] = LUSOL.a[LAST];
                    LUSOL.indc[L] = LUSOL.indc[LAST];
                    /*      Free entry */
                    LUSOL.indc[LAST] = 0;
                    /* ???        if (j .eq. jlast) lcol = lcol - 1 */
                }
                /*         ===============================================================
                           Delete the pivot column from the row file
                           and store the nonzeros of the next column of  L.
                           Set  indc(ll) = 0     to initialize markl(*) markers,
                                indr(ll) = 0     to initialize ifill(*) row fill-in cntrs,
                                indc(ls) = leni  to save the original row lengths,
                                indr(ls) = iqloc(i)    to save parts of  iqloc(*),
                                iqloc(i) = lsave - ls  to point to the nonzeros of  L
                                         = -1, -2, -3, ... in mark(*).
                           =============================================================== */
                LUSOL.indc[LSAVE] = NCOLD;
                if (MELIM == 0)
                {
                    goto x700;
                }
                LL = LL1 - 1;
                LS = LSAVE;
                ABEST = 1 / ABEST;
                for (LC = LPIVC1; LC <= LPIVC2; LC++)
                {
                    LL++;
                    LS++;
                    I = LUSOL.indc[LC];
                    LENI = LUSOL.lenr[I];
                    LUSOL.lenr[I] = LENI - 1;
                    LR1 = LUSOL.locr[I];
                    LAST = LR1 + LUSOL.lenr[I];
                    for (L = LR1; L <= LAST; L++)
                    {
                        if (LUSOL.indr[L] == JBEST)
                        {
                            break;
                        }
                    }
                    LUSOL.indr[L] = LUSOL.indr[LAST];
                    /*      Free entry */
                    LUSOL.indr[LAST] = 0;
                    LUSOL.a[LL] = -LUSOL.a[LC] * ABEST;
                    LIJ = System.Math.Abs(LUSOL.a[LL]);
                    commonlib.SETMAX(LMAX, LIJ);
                    LUSOL.indc[LL] = 0;
                    LUSOL.indr[LL] = 0;
                    LUSOL.indc[LS] = LENI;
                    LUSOL.indr[LS] = LUSOL.iqloc[I];
                    LUSOL.iqloc[I] = LSAVE - LS;
                }
                /*         ===============================================================
                           Do the Gaussian elimination.
                           This involves adding a multiple of the pivot column
                           to all other columns in the pivot row.
                           Sometimes more than one call to lu1gau is needed to allow
                           compression of the column file.
                           lfirst  says which column the elimination should start with.
                           minfre  is a bound on the storage needed for any one column.
                           lu      points to off-diagonals of u.
                           nfill   keeps track of pending fill-in in the row file.
                           =============================================================== */
                if (NELIM == 0)
                {
                    goto x700;
                }
                LFIRST = LPIVR1;
                MINFRE = MLEFT + NSPARE;
                LU = 1;
                NFILL = 0;

                x400:
#if UseTimer
	timer("start", eltime);
#endif
                LU1GAU(LUSOL, MELIM, NSPARE, SMALL, LPIVC1, LPIVC2, ref LFIRST, LPIVR2, LFREE, MINFRE, ILAST, ref JLAST, ref LROW, ref LCOL, ref LU, ref NFILL, LUSOL.iqloc[0], LUSOL.a[0] + LL1 - lusol.LUSOL_ARRAYOFFSET, LUSOL.indc[0] + LL1 - lusol.LUSOL_ARRAYOFFSET, LUSOL.a[0] + LU1 - lusol.LUSOL_ARRAYOFFSET, LUSOL.indr[0] + LL1 - lusol.LUSOL_ARRAYOFFSET, LUSOL.indr[0] + LU1 - lusol.LUSOL_ARRAYOFFSET);
#if UseTimer
	timer("finish", eltime);
#endif
                if (LFIRST > 0)
                {
                    /*            The elimination was interrupted.
                                  Compress the column file and try again.
                                  lfirst, lu and nfill have appropriate new values. */
                    LU1REC(LUSOL, LUSOL.n, true, ref LCOL, LUSOL.indc, LUSOL.lenc, LUSOL.locc);
                    LFILE = LCOL;
                    JLAST = LUSOL.indc[LCOL + 1];
                    LPIVC = LUSOL.locc[JBEST];
                    LPIVC1 = LPIVC + 1;
                    LPIVC2 = LPIVC + MELIM;
                    NFREE = LFREE - LCOL;
                    if (NFREE < MINFRE)
                    {
                        goto x970;
                    }
                    goto x400;
                }
                /*         ===============================================================
                           The column file has been fully updated.
                           Deal with any pending fill-in in the row file.
                           =============================================================== */
                if (NFILL > 0)
                {
                    /*            Compress the row file if necessary.
                                  lu1gau has set nfill to be the number of pending fill-ins
                                  plus the current length of any rows that need to be moved. */
                    MINFRE = NFILL;
                    NFREE = LFREE - LROW;
                    if (NFREE < MINFRE)
                    {
                        LU1REC(LUSOL, LUSOL.m, false, ref LROW, LUSOL.indr, LUSOL.lenr, LUSOL.locr);
                        LFILE = LROW;
                        ILAST = LUSOL.indr[LROW + 1];
                        LPIVR = LUSOL.locr[IBEST];
                        LPIVR1 = LPIVR + 1;
                        LPIVR2 = LPIVR + NELIM;
                        NFREE = LFREE - LROW;
                        if (NFREE < MINFRE)
                        {
                            goto x970;
                        }
                    }
                    /*            Move rows that have pending fill-in to end of the row file.
                                  Then insert the fill-in. */
                    LU1PEN(LUSOL, NSPARE, ref ILAST, LPIVC1, LPIVC2, LPIVR1, LPIVR2, ref LROW, LUSOL.indr[0] + LL1 - lusol.LUSOL_ARRAYOFFSET, LUSOL.indr[0] + LU1 - lusol.LUSOL_ARRAYOFFSET);
                }
                /*         ===============================================================
                           Restore the saved values of  iqloc.
                           Insert the correct indices for the col of L and the row of U.
                           =============================================================== */
                x700:
                LUSOL.lenr[IBEST] = 0;
                LUSOL.lenc[JBEST] = 0;
                LL = LL1 - 1;
                LS = LSAVE;
                for (LC = LPIVC1; LC <= LPIVC2; LC++)
                {
                    LL++;
                    LS++;
                    I = LUSOL.indc[LC];
                    LUSOL.iqloc[I] = LUSOL.indr[LS];
                    LUSOL.indc[LL] = I;
                    LUSOL.indr[LL] = IBEST;
                }
                LU = LU1 - 1;
                for (LR = LPIVR; LR <= LPIVR2; LR++)
                {
                    LU++;
                    LUSOL.indr[LU] = LUSOL.indr[LR];
                }
                /*         ===============================================================
                           Free the space occupied by the pivot row
                           and update the column permutation.
                           Then free the space occupied by the pivot column
                           and update the row permutation.
                           nzchng is found in both calls to lu1pq2, but we use it only
                           after the second.
                           =============================================================== */
                LU1PQ2(LUSOL, NCOLD, ref NZCHNG, LUSOL.indr[0] + LPIVR - lusol.LUSOL_ARRAYOFFSET, LUSOL.indc[0] + LU1 - lusol.LUSOL_ARRAYOFFSET, LUSOL.lenc, LUSOL.iqloc, LUSOL.iq, LUSOL.iqinv);
                LU1PQ2(LUSOL, NROWD, ref NZCHNG, LUSOL.indc[0] + LPIVC - lusol.LUSOL_ARRAYOFFSET, LUSOL.indc[0] + LSAVE - lusol.LUSOL_ARRAYOFFSET, LUSOL.lenr, LUSOL.iploc, LUSOL.ip, LUSOL.ipinv);
                NZLEFT += NZCHNG;

                /*         ===============================================================
                           lu1mxr resets Amaxr(i) in each modified row i.
                           lu1mxc moves the largest aij to the top of each modified col j.
                           28 Jun 2002: Note that cols of L have an implicit diag of 1.0,
                                        so lu1mxr is called with ll1, not ll1+1, whereas
                                           lu1mxc is called with          lu1+1.
                           =============================================================== */
                if (UTRI && TPP)
                {
                    /*      Relax -- we're not keeping big elements at the top yet. */
                }
                else
                {
                    if (TRP && MELIM > 0)
                    {
#if ClassicHamaxR
		LU1MXR(LUSOL, LL1,LL,LUSOL.indc,AMAXR);
	  }
#else
                        LU1MXR(LUSOL, LL1, LL, LUSOL.indc, LUSOL.amaxr);
#endif

                        if (NELIM > 0)
                        {
                            LU1MXC(LUSOL, LU1 + 1, LU, LUSOL.indr);
                            /*      Update modified columns in heap */
                            if (TCP)
                            {
                                for (KK = LU1 + 1; KK <= LU; KK++)
                                {
                                    J = LUSOL.indr[KK];
#if ClassicHamaxR
			K = HK[J];
#else
                                    K = LUSOL.Hk[J];
#endif
                                    /*      Biggest aij in column j */
                                    V = System.Math.Abs(LUSOL.a[LUSOL.locc[J]]);
#if ClassicHamaxR
			HCHANGE(HA, HJ, HK, HLEN, K, V, J, H);
#else
                                   lusol2.HCHANGE(LUSOL.Ha, LUSOL.Hj, LUSOL.Hk, HLEN, K, V, J, ref H);
#endif
                                    HOPS += H;
                                }
                            }
                        }
                    }
                    /*         ===============================================================
                               Negate lengths of pivot row and column so they will be
                               eliminated during compressions.
                               =============================================================== */
                    LUSOL.lenr[IBEST] = -NCOLD;
                    LUSOL.lenc[JBEST] = -NROWD;

                    /*         Test for fatal bug: row or column lists overwriting L and U. */
                    if (LROW > LSAVE || LCOL > LSAVE)
                    {
                        goto x980;
                    }

                    /*         Reset the file lengths if pivot row or col was at the end. */
                    if (IBEST == ILAST)
                    {
                        LROW = LUSOL.locr[IBEST];
                    }

                    if (JBEST == JLAST)
                    {
                        LCOL = LUSOL.locc[JBEST];
                    }

                }
                /*      ------------------------------------------------------------------
                        End of main loop.
                        ------------------------------------------------------------------
                        ------------------------------------------------------------------
                        Normal exit.
                        Move empty rows and cols to the end of ip, iq.
                        Then finish with a dense LU if necessary.
                        ------------------------------------------------------------------ */
                x900:
                INFORM = lusol.LUSOL_INFORM_LUSUCCESS;
                LU1PQ3(LUSOL, LUSOL.m, LUSOL.lenr, LUSOL.ip, LUSOL.ipinv, ref MRANK);
                LU1PQ3(LUSOL, LUSOL.n, LUSOL.lenc, LUSOL.iq, LUSOL.iqinv, ref NRANK);
               commonlib.SETMIN(NRANK, MRANK);
                if (DENSLU)
                {
#if UseTimer
	timer("start", 17);
#endif
                    LU1FUL(LUSOL, LEND, LU1, TPP, MLEFT, NLEFT, NRANK, NROWU, ref LENL, ref LENU, ref NSING, KEEPLU, SMALL, LUSOL.a[0] + LD - lusol.LUSOL_ARRAYOFFSET, LUSOL.locr);
                    /* ***     21 Dec 1994: Bug in next line.
                       ***     nrank  = nrank - nsing */
                    NRANK = MINMN - NSING;
#if UseTimer
	timer("finish", 17);
#endif
                }
                MINLEN = (LENL) + (LENU) + 2 * (LUSOL.m + LUSOL.n);
                goto x990;
                /*      Not enough space free after a compress.
                        Set  minlen  to an estimate of the necessary value of  lena. */
                x970:
                INFORM = lusol.LUSOL_INFORM_ANEEDMEM;
                MINLEN = LENA2 + LFILE + 2 * (LUSOL.m + LUSOL.n);
                goto x990;
                /*      Fatal error.  This will never happen!
                       (Famous last words.) */
                x980:
                INFORM = lusol.LUSOL_INFORM_FATALERR;
                goto x990;
                /*      Fatal error with TSP.  Diagonal pivot not found. */
                x985:
                INFORM = lusol.LUSOL_INFORM_NOPIVOT;
                /*      Exit. */
                x990:
#if UseTimer
  timer("finish", 3);
#endif
                ;
            }
        }

        internal static void LU1FAC(LUSOLrec LUSOL, ref int INFORM)
        {
            bool KEEPLU;
            bool TCP;
            bool TPP;
            bool TRP;
            bool TSP;
            int LPIV;
            int[] NELEM0;
            int LPRINT;
            int MINLEN;
            int NUML0;
            int LENL;
            int LENU;
            int LROW;
            int MERSUM;
            int NUTRI;
            int NLTRI;
            int NDENS1;
            int NDENS2;
            int NRANK;
            int NSING;
            int JSING;
            int JUMIN;
            int NUMNZ = 0;
            int LERR;
            int LU;
            int LL;
            int LM;
            int LTOPL;
            int K;
            int I;
            int LENUK;
            int J;
            int LENLK;
            int IDUMMY;
            int LLSAVE;
            int NMOVE;
            int L2;
            int L;
            int NCP;
            int NBUMP;
#if ClassicHamaxR
  int LENH;
  int LENA2;
  int LOCH;
  int LMAXR;
#endif
            double LMAX;
            double LTOL;
            double SMALL;
            double AMAX;
            double UMAX;
            double DUMAX;
            double DUMIN;
            double AKMAX;
            double DM;
            double DN;
            double DELEM;
            double DENSTY;
            double AGRWTH;
            double UGRWTH;
            double GROWTH;
            double CONDU;
            double DINCR;
            double AVGMER;

            /*      Free row-based version of L0 (regenerated by LUSOL_btran). */
            if (LUSOL.L0 != null)
            {
                lusol.LUSOL_matfree((LUSOL.L0));
            }

            /*      Grab relevant input parameters. */
            NELEM0 = LUSOL.nelem;
            LPRINT = LUSOL.luparm[lusol.LUSOL_IP_PRINTLEVEL];
            LPIV = LUSOL.luparm[lusol.LUSOL_IP_PIVOTTYPE];
            KEEPLU = (bool)(LUSOL.luparm[lusol.LUSOL_IP_KEEPLU] != 0);
            /*      Limit on size of Lij */
            LTOL = LUSOL.parmlu[lusol.LUSOL_RP_FACTORMAX_Lij];
            /*      Drop tolerance */
            SMALL = LUSOL.parmlu[lusol.LUSOL_RP_ZEROTOLERANCE];
            TPP = (bool)(LPIV == lusol.LUSOL_PIVMOD_TPP);
            TRP = (bool)(LPIV == lusol.LUSOL_PIVMOD_TRP);
            TCP = (bool)(LPIV == lusol.LUSOL_PIVMOD_TCP);
            TSP = (bool)(LPIV == lusol.LUSOL_PIVMOD_TSP);

            /*      Initialize output parameters. */
            INFORM = lusol.LUSOL_INFORM_LUSUCCESS;
            LERR = 0;
            /// <summary>
            /// PREVIOUS: MINLEN = LUSOL.nelem + 2 * (LUSOL.m + LUSOL.n);
            /// ERROR IN PREVIOUS: Operator '+' cannot be applied to operands of type 'int[]' and 'int'
            /// FIX 1: changed to MINLEN = LUSOL.nelem[0] + 2 * (LUSOL.m + LUSOL.n); need to check at runtime
            /// </summary>
            MINLEN = LUSOL.nelem[0] + 2 * (LUSOL.m + LUSOL.n);
            NUML0 = 0;
            LENL = 0;
            LENU = 0;
            LROW = 0;
            MERSUM = 0;
            NUTRI = LUSOL.m;
            NLTRI = 0;
            NDENS1 = 0;
            NDENS2 = 0;
            NRANK = 0;
            NSING = 0;
            JSING = 0;
            JUMIN = 0;
            AMAX = 0;
            LMAX = 0;
            UMAX = 0;
            DUMAX = 0;
            DUMIN = 0;
            AKMAX = 0;

            /*      Float version of dimensions. */
            DM = LUSOL.m;
            DN = LUSOL.n;
            /// <summary>
            /// PREVIOUS: DELEM = LUSOL.nelem;
            /// ERROR IN PREVIOUS: Cannot implicitly convert type 'int[]' to 'double'
            /// FIX 1: changed to DELEM = LUSOL.nelem[0]; need to check at runtime
            /// </summary>
            DELEM = LUSOL.nelem[0];

            /*      Initialize workspace parameters. */
            LUSOL.luparm[lusol.LUSOL_IP_COMPRESSIONS_LU] = 0;
            if (LUSOL.lena < MINLEN)
            {
                if (!lusol.LUSOL_realloc_a(LUSOL, MINLEN))
                {
                    goto x970;
                }
            }

            /*      ------------------------------------------------------------------
        Organize the  aij's  in  a, indc, indr.
        lu1or1  deletes small entries, tests for illegal  i,j's,
                and counts the nonzeros in each row and column.
        lu1or2  reorders the elements of  A  by columns.
        lu1or3  uses the column list to test for duplicate entries
                (same indices  i,j).
        lu1or4  constructs a row list from the column list.
        ------------------------------------------------------------------ */

            LU1OR1(LUSOL, SMALL, ref AMAX, ref NUMNZ, ref LERR, ref INFORM);
            if (LPRINT >= lusol.LUSOL_MSG_STATISTICS)
            {
                DENSTY = (100 * DELEM) / (DM * DN);
                lusol.LUSOL_report(LUSOL, 0, "m:%6d %c n:%6d  nzcount:%9d  Amax:%g  Density:%g\n", LUSOL.m, lusol.relationChar(LUSOL.m, LUSOL.n), LUSOL.n, LUSOL.nelem, AMAX, DENSTY);
            }

            if (INFORM != lusol.LUSOL_INFORM_LUSUCCESS)
            {
                goto x930;
            }
            /// <summary>
            /// PREVIOUS: LUSOL.nelem = NUMNZ;
            /// ERROR IN PREVIOUS: Cannot implicitly convert type 'int' to 'int[]'
            /// FIX 1: changed to LUSOL.nelem[0] = NUMNZ; need to check at runtime
            /// </summary>
            LUSOL.nelem[0] = NUMNZ;
            LU1OR2(LUSOL);
            LU1OR3(LUSOL, ref LERR, ref INFORM);
            if (INFORM != lusol.LUSOL_INFORM_LUSUCCESS)
            {
                goto x940;
            }
            LU1OR4(LUSOL);

            /*      ------------------------------------------------------------------
        Set up lists of rows and columns with equal numbers of nonzeros,
        using  indc(*)  as workspace.
        ------------------------------------------------------------------ */
            /// <summary>
            /// PREVIOUS: LU1PQ1(LUSOL, LUSOL.m, LUSOL.n, LUSOL.lenr, LUSOL.ip, LUSOL.iploc, LUSOL.ipinv, LUSOL.indc[0] + LUSOL.nelem[0]); // LUSOL_ARRAYOFFSET implied
            /// ERROR IN PREVIOUS: cannot convert from 'int' to 'int[]'
            /// FIX 1: sent only 1 parameter as of now; need to decide at run time
            /// </summary>
            LU1PQ1(LUSOL, LUSOL.m, LUSOL.n, LUSOL.lenr, LUSOL.ip, LUSOL.iploc, LUSOL.ipinv, LUSOL.indc);    // + LUSOL.nelem // LUSOL_ARRAYOFFSET implied
            LU1PQ1(LUSOL, LUSOL.n, LUSOL.m, LUSOL.lenc, LUSOL.iq, LUSOL.iqloc, LUSOL.iqinv, LUSOL.indc); // +LUSOL.nelem // LUSOL_ARRAYOFFSET implied
                                                                                                         /*      ------------------------------------------------------------------
                                                                                                                 For TCP, Ha, Hj, Hk are allocated separately, similarly amaxr
                                                                                                                 for TRP. Then compute the factorization  A = L*U.
                                                                                                                 ------------------------------------------------------------------ */

#if ClassicHamaxR
  if (TPP || TSP)
  {
	LENH = 1;
	LENA2 = LUSOL.lena;
	LOCH = LUSOL.lena;
	LMAXR = 1;
  }
  else if (TRP)
  {
	LENH = 1; // Dummy
	LENA2 = LUSOL.lena - LUSOL.m; // Reduced length of      a
	LOCH = LUSOL.lena; // Dummy
	LMAXR = LENA2 + 1; // Start of Amaxr      in a
  }
  else if (TCP)
  {
	LENH = LUSOL.n; // Length of heap
	LENA2 = LUSOL.lena - LENH; // Reduced length of      a, indc, indr
	LOCH = LENA2 + 1; // Start of Ha, Hj, Hk in a, indc, indr
	LMAXR = 1; // Dummy
  }
  LU1FAD(LUSOL, LENA2, LENH, LUSOL.a + LOCH - LUSOL_ARRAYOFFSET, LUSOL.indc + LOCH - LUSOL_ARRAYOFFSET, LUSOL.indr + LOCH - LUSOL_ARRAYOFFSET, LUSOL.a + LMAXR - LUSOL_ARRAYOFFSET, INFORM, LENL, LENU, MINLEN, MERSUM, NUTRI, NLTRI, NDENS1, NDENS2, NRANK, LMAX, UMAX, DUMAX, DUMIN, AKMAX);
#else
            LU1FAD(LUSOL, ref INFORM, ref LENL, ref LENU, ref MINLEN, ref MERSUM, ref NUTRI, ref NLTRI, ref NDENS1, ref NDENS2, ref NRANK, ref LMAX, ref UMAX, ref DUMAX, ref DUMIN, ref AKMAX);
#endif

            LUSOL.luparm[lusol.LUSOL_IP_RANK_U] = NRANK;
            LUSOL.luparm[lusol.LUSOL_IP_NONZEROS_L] = LENL;
            if (INFORM == lusol.LUSOL_INFORM_ANEEDMEM)
            {
                goto x970;
            }
            if (INFORM == lusol.LUSOL_INFORM_NOPIVOT)
            {
                goto x985;
            }
            if (INFORM > lusol.LUSOL_INFORM_LUSUCCESS)
            {
                goto x980;
            }

            if (KEEPLU)
            {
                /*         ---------------------------------------------------------------
                           The LU factors are at the top of  a, indc, indr,
                           with the columns of  L  and the rows of  U  in the order
                           ( free )   ... ( u3 ) ( l3 ) ( u2 ) ( l2 ) ( u1 ) ( l1 ).
                           Starting with ( l1 ) and ( u1 ), move the rows of  U  to the
                           left and the columns of  L  to the right, giving
                           ( u1 ) ( u2 ) ( u3 ) ...   ( free )   ... ( l3 ) ( l2 ) ( l1 ).
                           Also, set  numl0 = the number of nonempty columns of  U.
                           --------------------------------------------------------------- */
                LU = 0;
                LL = LUSOL.lena + 1;
#if ClassicHamaxR
	LM = LENA2 + 1;
#else
                LM = LL;
#endif
                LTOPL = LL - LENL - LENU;
                LROW = LENU;
                for (K = 1; K <= NRANK; K++)
                {
                    I = LUSOL.ip[K];
                    LENUK = -LUSOL.lenr[I];
                    LUSOL.lenr[I] = LENUK;
                    J = LUSOL.iq[K];
                    LENLK = -LUSOL.lenc[J] - 1;
                    if (LENLK > 0)
                    {
                        NUML0++;
                        LUSOL.iqloc[NUML0] = LENLK;
                    }
                    if (LU + LENUK < LTOPL)
                    {
                        /*               =========================================================
                                         There is room to move ( uk ).  Just right-shift ( lk ).
                                         ========================================================= */
                        for (IDUMMY = 1; IDUMMY <= LENLK; IDUMMY++)
                        {
                            LL--;
                            LM--;
                            LUSOL.a[LL] = LUSOL.a[LM];
                            LUSOL.indc[LL] = LUSOL.indc[LM];
                            LUSOL.indr[LL] = LUSOL.indr[LM];
                        }
                    }
                    else
                    {
                        /*               =========================================================
                                         There is no room for ( uk ) yet.  We have to
                                         right-shift the whole of the remaining LU file.
                                         Note that ( lk ) ends up in the correct place.
                                         ========================================================= */
                        LLSAVE = LL - LENLK;
                        NMOVE = LM - LTOPL;
                        for (IDUMMY = 1; IDUMMY <= NMOVE; IDUMMY++)
                        {
                            LL--;
                            LM--;
                            LUSOL.a[LL] = LUSOL.a[LM];
                            LUSOL.indc[LL] = LUSOL.indc[LM];
                            LUSOL.indr[LL] = LUSOL.indr[LM];
                        }
                        LTOPL = LL;
                        LL = LLSAVE;
                        LM = LL;
                    }
                    /*            ======================================================
                                  Left-shift ( uk ).
                                  ====================================================== */
                    LUSOL.locr[I] = LU + 1;
                    L2 = LM - 1;
                    LM = LM - LENUK;
                    for (L = LM; L <= L2; L++)
                    {
                        LU = LU + 1;
                        LUSOL.a[LU] = LUSOL.a[L];
                        LUSOL.indr[LU] = LUSOL.indr[L];
                    }
                }
                /*         ---------------------------------------------------------------
                           Save the lengths of the nonempty columns of  L,
                           and initialize  locc(j)  for the LU update routines.
                           --------------------------------------------------------------- */
                for (K = 1; K <= NUML0; K++)
                {
                    LUSOL.lenc[K] = LUSOL.iqloc[K];
                }
                for (J = 1; J <= LUSOL.n; J++)
                {
                    LUSOL.locc[J] = 0;
                }
                /*         ---------------------------------------------------------------
                           Test for singularity.
                           lu6chk  sets  nsing, jsing, jumin, Lmax, Umax, DUmax, DUmin
                           (including entries from the dense LU).
                           inform = 1  if there are singularities (nsing gt 0).
                           --------------------------------------------------------------- */
                lusol6a.LU6CHK(LUSOL, 1, LUSOL.lena, ref INFORM);
                NSING = LUSOL.luparm[lusol.LUSOL_IP_SINGULARITIES];
                JSING = LUSOL.luparm[lusol.LUSOL_IP_SINGULARINDEX];
                JUMIN = LUSOL.luparm[lusol.LUSOL_IP_COLINDEX_DUMIN];
                LMAX = LUSOL.parmlu[lusol.LUSOL_RP_MAXMULT_L];
                UMAX = LUSOL.parmlu[lusol.LUSOL_RP_MAXELEM_U];
                DUMAX = LUSOL.parmlu[lusol.LUSOL_RP_MAXELEM_DIAGU];
                DUMIN = LUSOL.parmlu[lusol.LUSOL_RP_MINELEM_DIAGU];
            }

            else
            {
                /*         ---------------------------------------------------------------
                           keepLU = 0.  L and U were not kept, just the diagonals of U.
                           lu1fac will probably be called again soon with keepLU = .true.
                           11 Mar 2001: lu6chk revised.  We can call it with keepLU = 0,
                                        but we want to keep Lmax, Umax from lu1fad.
                           05 May 2002: Allow for TCP with new lu1DCP.  Diag(U) starts
                                        below lena2, not lena.  Need lena2 in next line.
                           --------------------------------------------------------------- */
#if ClassicHamaxR
	LU6CHK(LUSOL, 1,LENA2,INFORM);
#else
                lusol6a.LU6CHK(LUSOL, 1, LUSOL.lena, ref INFORM);
#endif
                NSING = LUSOL.luparm[lusol.LUSOL_IP_SINGULARITIES];
                JSING = LUSOL.luparm[lusol.LUSOL_IP_SINGULARINDEX];
                JUMIN = LUSOL.luparm[lusol.LUSOL_IP_COLINDEX_DUMIN];
                DUMAX = LUSOL.parmlu[lusol.LUSOL_RP_MAXELEM_DIAGU];
                DUMIN = LUSOL.parmlu[lusol.LUSOL_RP_MINELEM_DIAGU];
            }
            goto x990;
            /*      ------------
                    Error exits.
                    ------------ */

            x930:
            INFORM = lusol.LUSOL_INFORM_ADIMERR;
            if (LPRINT >= lusol.LUSOL_MSG_SINGULARITY)
            {
                lusol.LUSOL_report(LUSOL, 0, "lu1fac  error...\nentry  a[%d]  has an illegal row (%d) or column (%d) index\n", LERR, LUSOL.indc[LERR], LUSOL.indr[LERR]);
            }
            goto x990;
            x940:
            INFORM = lusol.LUSOL_INFORM_ADUPLICATE;
            if (LPRINT >= lusol.LUSOL_MSG_SINGULARITY)
            {
                lusol.LUSOL_report(LUSOL, 0, "lu1fac  error...\nentry  a[%d]  is a duplicate with indeces indc=%d, indr=%d\n", LERR, LUSOL.indc[LERR], LUSOL.indr[LERR]);
            }
            goto x990;
            x970:
            INFORM = lusol.LUSOL_INFORM_ANEEDMEM;
            if (LPRINT >= lusol.LUSOL_MSG_SINGULARITY)
            {
                lusol.LUSOL_report(LUSOL, 0, "lu1fac  error...\ninsufficient storage; increase  lena  from %d to at least %d\n", LUSOL.lena, MINLEN);
            }
            goto x990;
            x980:
            INFORM = lusol.LUSOL_INFORM_FATALERR;
            if (LPRINT >= lusol.LUSOL_MSG_SINGULARITY)
            {
                lusol.LUSOL_report(LUSOL, 0, "lu1fac  error...\nfatal bug   (sorry --- this should never happen)\n");
            }
            goto x990;
            x985:
            INFORM = lusol.LUSOL_INFORM_NOPIVOT;
            if (LPRINT >= lusol.LUSOL_MSG_SINGULARITY)
            {
                lusol.LUSOL_report(LUSOL, 0, "lu1fac  error...\nTSP used but diagonal pivot could not be found\n");
            }

            /*      Finalize and store output parameters. */
            x990:
            LUSOL.nelem = NELEM0;
            LUSOL.luparm[lusol.LUSOL_IP_SINGULARITIES] = NSING;
            LUSOL.luparm[lusol.LUSOL_IP_SINGULARINDEX] = JSING;
            LUSOL.luparm[lusol.LUSOL_IP_MINIMUMLENA] = MINLEN;
            LUSOL.luparm[lusol.LUSOL_IP_UPDATECOUNT] = 0;
            LUSOL.luparm[lusol.LUSOL_IP_RANK_U] = NRANK;
            LUSOL.luparm[lusol.LUSOL_IP_COLCOUNT_DENSE1] = NDENS1;
            LUSOL.luparm[lusol.LUSOL_IP_COLCOUNT_DENSE2] = NDENS2;
            LUSOL.luparm[lusol.LUSOL_IP_COLINDEX_DUMIN] = JUMIN;
            LUSOL.luparm[lusol.LUSOL_IP_COLCOUNT_L0] = NUML0;
            LUSOL.luparm[lusol.LUSOL_IP_ROWCOUNT_L0] = 0;
            LUSOL.luparm[lusol.LUSOL_IP_NONZEROS_L0] = LENL;
            LUSOL.luparm[lusol.LUSOL_IP_NONZEROS_U0] = LENU;
            LUSOL.luparm[lusol.LUSOL_IP_NONZEROS_L] = LENL;
            LUSOL.luparm[lusol.LUSOL_IP_NONZEROS_U] = LENU;
            LUSOL.luparm[lusol.LUSOL_IP_NONZEROS_ROW] = LROW;
            LUSOL.luparm[lusol.LUSOL_IP_MARKOWITZ_MERIT] = MERSUM;
            LUSOL.luparm[lusol.LUSOL_IP_TRIANGROWS_U] = NUTRI;
            LUSOL.luparm[lusol.LUSOL_IP_TRIANGROWS_L] = NLTRI;
            LUSOL.parmlu[lusol.LUSOL_RP_MAXELEM_A] = AMAX;
            LUSOL.parmlu[lusol.LUSOL_RP_MAXMULT_L] = LMAX;
            LUSOL.parmlu[lusol.LUSOL_RP_MAXELEM_U] = UMAX;
            LUSOL.parmlu[lusol.LUSOL_RP_MAXELEM_DIAGU] = DUMAX;
            LUSOL.parmlu[lusol.LUSOL_RP_MINELEM_DIAGU] = DUMIN;
            LUSOL.parmlu[lusol.LUSOL_RP_MAXELEM_TCP] = AKMAX;
            AGRWTH = AKMAX / (AMAX + lusol.LUSOL_SMALLNUM);
            UGRWTH = UMAX / (AMAX + lusol.LUSOL_SMALLNUM);
            if (TPP)
            {
                GROWTH = UGRWTH;
            }
            /*      TRP or TCP or TSP */
            else
            {
                GROWTH = AGRWTH;
            }
            LUSOL.parmlu[lusol.LUSOL_RP_GROWTHRATE] = GROWTH;

            LUSOL.luparm[lusol.LUSOL_IP_FTRANCOUNT] = 0;
            LUSOL.luparm[lusol.LUSOL_IP_BTRANCOUNT] = 0;

            /*      ------------------------------------------------------------------
                    Set overall status variable.
                    ------------------------------------------------------------------ */
            LUSOL.luparm[lusol.LUSOL_IP_INFORM] = INFORM;
            if (INFORM == lusol.LUSOL_INFORM_NOMEMLEFT)
            {
                lusol.LUSOL_report(LUSOL, 0, "lu1fac  error...\ninsufficient memory available\n");
            }

            /*      ------------------------------------------------------------------
                    Print statistics for the LU factors.
                    ------------------------------------------------------------------ */
            NCP = LUSOL.luparm[lusol.LUSOL_IP_COMPRESSIONS_LU];
            CONDU = DUMAX / commonlib.MAX(DUMIN, lusol.LUSOL_SMALLNUM);
            /// <summary>
            /// PREVIOUS: DINCR = (LENL + LENU) - LUSOL.nelem;
            /// ERROR IN PREVIOUS: cannot be applied to operands of type 'int' and 'int[]'
            /// FIX 1: changed to DINCR = (LENL + LENU) - LUSOL.nelem; need to check at run time
            /// </summary>
            DINCR = (LENL + LENU) - LUSOL.nelem[0];
            DINCR = (DINCR * 100) / commonlib.MAX(DELEM, 1);
            AVGMER = MERSUM;
            AVGMER = AVGMER / DM;
            NBUMP = LUSOL.m - NUTRI - NLTRI;
            if (LPRINT >= lusol.LUSOL_MSG_STATISTICS)
            {
                if (TPP)
                {
                    lusol.LUSOL_report(LUSOL, 0, "Merit %g %d %d %d %g %d %d %g %g %d %d %d\n", AVGMER, LENL, LENL + LENU, NCP, DINCR, NUTRI, LENU, LTOL, UMAX, UGRWTH, NLTRI, NDENS1, LMAX);
                }
                else
                {
                    lusol.LUSOL_report(LUSOL, 0, "Merit %s %g %d %d %d %g %d %d %g %g %d %d %d %g %g\n", lusol.LUSOL_pivotLabel(LUSOL), AVGMER, LENL, LENL + LENU, NCP, DINCR, NUTRI, LENU, LTOL, UMAX, UGRWTH, NLTRI, NDENS1, LMAX, AKMAX, AGRWTH);
                }
                lusol.LUSOL_report(LUSOL, 0, "bump%9d  dense2%7d  DUmax%g DUmin%g  conDU%g\n", NBUMP, NDENS2, DUMAX, DUMIN, CONDU);
            }
        }

        private static void LU1OR1(LUSOLrec LUSOL, double SMALL, ref double AMAX, ref int NUMNZ, ref int LERR, ref int INFORM)
        {
            int I;
            int J;
            int L;
            int LDUMMY;

#if LUSOLFastClear
  MEMCLEAR((LUSOL.lenr + 1), LUSOL.m);
  MEMCLEAR((LUSOL.lenc + 1), LUSOL.n);
#else
            for (I = 1; I <= LUSOL.m; I++)
            {
                LUSOL.lenr[I] = 0;
            }
            for (I = 1; I <= LUSOL.n; I++)
            {
                LUSOL.lenc[I] = 0;
            }
#endif

            AMAX = 0;
            NUMNZ = LUSOL.nelem[0];
            L = LUSOL.nelem[0] + 1;
            for (LDUMMY = 1; LDUMMY <= LUSOL.nelem[0]; LDUMMY++)
            {
                L--;
                if (System.Math.Abs(LUSOL.a[L]) > SMALL)
                {
                    I = LUSOL.indc[L];
                    J = LUSOL.indr[L];
                    commonlib.SETMAX(AMAX, System.Math.Abs(LUSOL.a[L]));
                    if (I < 1 || I > LUSOL.m)
                    {
                        goto x910;
                    }
                    if (J < 1 || J > LUSOL.n)
                    {
                        goto x910;
                    }
                    LUSOL.lenr[I]++;
                    LUSOL.lenc[J]++;
                }
                else
                {
                    /*            Replace a negligible element by last element.  Since
                                  we are going backwards, we know the last element is ok. */
                    LUSOL.a[L] = LUSOL.a[NUMNZ];
                    LUSOL.indc[L] = LUSOL.indc[NUMNZ];
                    LUSOL.indr[L] = LUSOL.indr[NUMNZ];
                    (NUMNZ)--;
                }
            }
            LERR = 0;
            INFORM = lusol.LUSOL_INFORM_LUSUCCESS;
            return;

            x910:
            LERR = L;
            INFORM = lusol.LUSOL_INFORM_LUSINGULAR;
        }

        /* ==================================================================
   lu1mxc  moves the largest element in each of columns iq(k1:k2)
   to the top of its column.
   If k1 > k2, nothing happens.
   ------------------------------------------------------------------
   06 May 2002: (and earlier)
                All columns k1:k2 must have one or more elements.
   07 May 2002: Allow for empty columns.  The heap routines need to
                find 0.0 as the "largest element".
   29 Nov 2005: Bug fix - avoiding overwriting the next column when
                the current column is empty (i.e. LENJ==0)
                Yin Zhang <yzhang@cs.utexas.edu>
   ================================================================== */
        private static void LU1MXC(LUSOLrec LUSOL, int K1, int K2, int[] IX)
        {
            int I;
            int J;
            int K;
            int L;
            int LC;
            int LENJ;
            double AMAX;

            for (K = K1; K <= K2; K++)
            {
                J = IX[K];
                LC = LUSOL.locc[J];
                LENJ = LUSOL.lenc[J];
                if (LENJ == 0)
                {
                    /*      LUSOL->a[LC] = ZERO;  Removal suggested by Yin Zhang to avoid overwriting next column when current is empty */
                    ;
                }
                else
                {
                    double x = LUSOL.a[0] + Convert.ToDouble(LC) - lusol.LUSOL_ARRAYOFFSET;
                    L = myblas.idamax(LUSOL.lenc[J], ref x, 1) + LC - 1;
                    if (L > LC)
                    {
                        AMAX = LUSOL.a[L];
                        LUSOL.a[L] = LUSOL.a[LC];
                        LUSOL.a[LC] = AMAX;
                        I = LUSOL.indc[L];
                        LUSOL.indc[L] = LUSOL.indc[LC];
                        LUSOL.indc[LC] = I;
                    }
                }
            }
        }

        /* ==================================================================
   lu1slk  sets w(j) > 0 if column j is a unit vector.
   ------------------------------------------------------------------
   21 Nov 2000: First version.  lu1fad needs it for TCP.
                Note that w(*) is nominally an integer array,
                but the only spare space is the double array w(*).
   ================================================================== */
        private static void LU1SLK(LUSOLrec LUSOL)
        {
            int J;
            int LC1;
            int LQ;
            int LQ1;
            int LQ2;

            for (J = 1; J <= LUSOL.n; J++)
            {
                LUSOL.w[J] = 0;
            }
            LQ1 = (LUSOL.iqloc != null ? LUSOL.iqloc[1] : LUSOL.n + 1);
            /*  LQ1 = LUSOL->iqloc[1];   This is the original version; correction above by Yin Zhang */
            LQ2 = LUSOL.n;
            if (LUSOL.m > 1)
            {
                LQ2 = LUSOL.iqloc[2] - 1;
            }
            for (LQ = LQ1; LQ <= LQ2; LQ++)
            {
                J = LUSOL.iq[LQ];
                LC1 = LUSOL.locc[J];
                if (System.Math.Abs(LUSOL.a[LC1]) == 1)
                {
                    LUSOL.w[J] = 1;
                }
            }
        }

        /* ==================================================================
   lu1mxr  finds the largest element in each of row ip(k1:k2)
   and stores it in Amaxr(*).  The nonzeros are stored column-wise
   in (a,indc,lenc,locc) and their structure is row-wise
   in (  indr,lenr,locr).
   If k1 > k2, nothing happens.
   ------------------------------------------------------------------
   11 Jun 2002: First version of lu1mxr.
                Allow for empty columns.
   ================================================================== */
        private static void LU1MXR(LUSOLrec LUSOL, int K1, int K2, int[] IX, double[] AMAXR)
        {
#if FastMXR
            int I;
            int J;
            int IC;
            int K;
            int LC;
            int LC1;
            int LC2;
            int LR;
            int LR1;
            int LR2;
            double AMAX;
#else
   int I;
   int J;
   int K;
   int LC;
   int LC1;
   int LC2;
   int LR;
   int LR1;
   int LR2;
   double AMAX;
#endif
            for (K = K1; K <= K2; K++)
            {
                AMAX = 0;
                I = IX[K];
                /*      Find largest element in row i. */
                LR1 = LUSOL.locr[I];
                LR2 = (LR1 + LUSOL.lenr[I]) - 1;
#if FastMXR
                for (LR = LR1, J = LUSOL.indr[0] + LR1; LR <= LR2; LR++, J++)
                {
                    /*      Find where  aij  is in column  j. */
                    LC1 = LUSOL.locc[J];
                    LC2 = LC1 + LUSOL.lenc[J];
                    for (LC = LC1, IC = LUSOL.indc[0] + LC1; LC < LC2; LC++, IC++)
                    {
                        if (IC == I)
                        {
                            break;
                        }
                    }
                    commonlib.SETMAX(AMAX, System.Math.Abs(LUSOL.a[LC]));
                }
#else
	for (LR = LR1; LR <= LR2; LR++)
	{
	  J = LUSOL.indr[LR];
/*      Find where  aij  is in column  j. */
	  LC1 = LUSOL.locc[J];
	  LC2 = (LC1 + LUSOL.lenc[J]) - 1;
	  for (LC = LC1; LC <= LC2; LC++)
	  {
		if (LUSOL.indc[LC] == I)
		{
		  break;
		}
	  }
	  SETMAX(AMAX,Math.Abs(LUSOL.a[LC]));
	}
#endif
                AMAXR[I] = AMAX;
            }
        }

        /* ==================================================================
   lu1mar  uses a Markowitz criterion to select a pivot element
   for the next stage of a sparse LU factorization,
   subject to a Threshold Partial Pivoting stability criterion (TPP)
   that bounds the elements of L.
   ------------------------------------------------------------------
   gamma  is "gamma" in the tie-breaking rule TB4 in the LUSOL paper.
   ------------------------------------------------------------------
   Search cols of length nz = 1, then rows of length nz = 1,
   then   cols of length nz = 2, then rows of length nz = 2, etc.
   ------------------------------------------------------------------
   00 Jan 1986  Version documented in LUSOL paper:
                Gill, Murray, Saunders and Wright (1987),
                Maintaining LU factors of a general sparse matrix,
                Linear algebra and its applications 88/89, 239-270.
   02 Feb 1989  Following Suhl and Aittoniemi (1987), the largest
                element in each column is now kept at the start of
                the column, i.e. in position locc(j) of a and indc.
                This should speed up the Markowitz searches.
   26 Apr 1989  Both columns and rows searched during spars1 phase.
                Only columns searched during spars2 phase.
                maxtie replaced by maxcol and maxrow.
   05 Nov 1993  Initializing  "mbest = m * n"  wasn't big enough when
                m = 10, n = 3, and last column had 7 nonzeros.
   09 Feb 1994  Realised that "mbest = maxmn * maxmn" might overflow.
                Changed to    "mbest = maxmn * 1000".
   27 Apr 2000  On large example from Todd Munson,
                that allowed  "if (mbest .le. nz1**2) go to 900"
                to exit before any pivot had been found.
                Introduced kbest = mbest / nz1.
                Most pivots can be rejected with no integer multiply.
                TRUE merit is evaluated only if it's as good as the
                best so far (or better).  There should be no danger
                of integer overflow unless A is incredibly
                large and dense.
   10 Sep 2000  TCP, aijtol added for Threshold Complete Pivoting.
   ================================================================== */
        private static void LU1MAR(LUSOLrec LUSOL, int MAXMN, bool TCP, double AIJTOL, double LTOL, int MAXCOL, int MAXROW, ref int IBEST, ref int JBEST, ref int MBEST)
        {
            int KBEST;
            int NCOL;
            int NROW;
            int NZ1;
            int NZ;
            int LQ1;
            int LQ2;
            int LQ;
            int J;
            int LC1;
            int LC2;
            int LC;
            int I;
            int LEN1;
            int MERIT;
            int LP1;
            int LP2;
            int LP;
            int LR1;
            int LR2;
            int LR;
            double ABEST;
            double LBEST;
            double AMAX;
            double AIJ;
            double CMAX;

            ABEST = 0;
            LBEST = 0;
            IBEST = 0;
            MBEST = -1;
            KBEST = MAXMN + 1;
            NCOL = 0;
            NROW = 0;
            NZ1 = 0;

            for (NZ = 1; NZ <= MAXMN; NZ++)
            {
                /*         nz1    = nz - 1
                           if (mbest .le. nz1**2) go to 900 */
                if (KBEST <= NZ1)
                {
                    goto x900;
                }
                if (IBEST > 0)
                {
                    if (NCOL >= MAXCOL)
                    {
                        goto x200;
                    }
                }
                if (NZ > LUSOL.m)
                {
                    goto x200;
                }
                /*         ---------------------------------------------------------------
                           Search the set of columns of length  nz.
                           --------------------------------------------------------------- */
                LQ1 = LUSOL.iqloc[NZ];
                LQ2 = LUSOL.n;
                if (NZ < LUSOL.m)
                {
                    LQ2 = LUSOL.iqloc[NZ + 1] - 1;
                }
                for (LQ = LQ1; LQ <= LQ2; LQ++)
                {
                    NCOL = NCOL + 1;
                    J = LUSOL.iq[LQ];
                    LC1 = LUSOL.locc[J];
                    LC2 = LC1 + NZ1;
                    AMAX = System.Math.Abs(LUSOL.a[LC1]);
                    /*            Test all aijs in this column.
                                  amax is the largest element (the first in the column).
                                  cmax is the largest multiplier if aij becomes pivot. */
                    if (TCP)
                    {
                        /*      Nothing in whole column */
                        if (AMAX < AIJTOL)
                        {
                            continue;
                        }
                    }
                    for (LC = LC1; LC <= LC2; LC++)
                    {
                        I = LUSOL.indc[LC];
                        LEN1 = LUSOL.lenr[I] - 1;
                        /*               merit  = nz1 * len1
                                         if (merit > mbest) continue; */
                        if (LEN1 > KBEST)
                        {
                            continue;
                        }
                        /*               aij  has a promising merit.
                                         Apply the stability test.
                                         We require  aij  to be sufficiently large compared to
                                         all other nonzeros in column  j.  This is equivalent
                                         to requiring cmax to be bounded by Ltol. */
                        if (LC == LC1)
                        {
                            /*                  This is the maximum element, amax.
                                                Find the biggest element in the rest of the column
                                                and hence get cmax.  We know cmax .le. 1, but
                                                we still want it exactly in order to break ties.
                                                27 Apr 2002: Settle for cmax = 1. */
                            AIJ = AMAX;
                            CMAX = 1;
                            /*                  cmax   = zero
                                                for (l = lc1 + 1; l <= lc2; l++)
                                                   cmax  = max( cmax, abs( a(l) ) );
                                                cmax   = cmax / amax; */
                        }
                        else
                        {
                            /*                  aij is not the biggest element, so cmax .ge. 1.
                                                Bail out if cmax will be too big. */
                            AIJ = System.Math.Abs(LUSOL.a[LC]);
                            /*      Absolute test for Complete Pivoting */
                            if (TCP)
                            {
                                if (AIJ < AIJTOL)
                                {
                                    continue;
                                }
                                /*      TPP */
                            }
                            else
                            {
                                if (AIJ * LTOL < AMAX)
                                {
                                    continue;
                                }
                            }
                            CMAX = AMAX / AIJ;
                        }
                        /*               aij  is big enough.  Its maximum multiplier is cmax. */
                        MERIT = NZ1 * LEN1;
                        if (MERIT == MBEST)
                        {
                            /*                  Break ties.
                                                (Initializing mbest < 0 prevents getting here if
                                                nothing has been found yet.)
                                                In this version we minimize cmax
                                                but if it is already small we maximize the pivot. */
                            if (LBEST <= LUSOL.parmlu[lusol.LUSOL_RP_GAMMA] && CMAX <= LUSOL.parmlu[lusol.LUSOL_RP_GAMMA])
                            {
                                if (ABEST >= AIJ)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (LBEST <= CMAX)
                                {
                                    continue;
                                }
                            }
                        }

                        /*               aij  is the best pivot so far. */
                        IBEST = I;
                        JBEST = J;
                        KBEST = LEN1;
                        MBEST = MERIT;
                        ABEST = AIJ;
                        LBEST = CMAX;
                        if (NZ == 1)
                        {
                            goto x900;
                        }
                    }
                    /*            Finished with that column. */
                    if (IBEST > 0)
                    {
                        if (NCOL >= MAXCOL)
                        {
                            goto x200;
                        }
                    }
                }
                /*         ---------------------------------------------------------------
                           Search the set of rows of length  nz.
                           --------------------------------------------------------------- */
                x200:
                /*    if (mbest .le. nz*nz1) go to 900 */
                if (KBEST <= NZ)
                {
                    goto x900;
                }
                if (IBEST > 0)
                {
                    if (NROW >= MAXROW)
                    {
                        goto x290;
                    }
                }
                if (NZ > LUSOL.n)
                {
                    goto x290;
                }
                LP1 = LUSOL.iploc[NZ];
                LP2 = LUSOL.m;
                if (NZ < LUSOL.n)
                {
                    LP2 = LUSOL.iploc[NZ + 1] - 1;
                }
                for (LP = LP1; LP <= LP2; LP++)
                {
                    NROW++;
                    I = LUSOL.ip[LP];
                    LR1 = LUSOL.locr[I];
                    LR2 = LR1 + NZ1;
                    for (LR = LR1; LR <= LR2; LR++)
                    {
                        J = LUSOL.indr[LR];
                        LEN1 = LUSOL.lenc[J] - 1;
                        /*               merit  = nz1 * len1
                                         if (merit .gt. mbest) continue */
                        if (LEN1 > KBEST)
                        {
                            continue;
                        }
                        /*               aij  has a promising merit.
                                         Find where  aij  is in column  j. */
                        LC1 = LUSOL.locc[J];
                        LC2 = LC1 + LEN1;
                        AMAX = System.Math.Abs(LUSOL.a[LC1]);
                        for (LC = LC1; LC <= LC2; LC++)
                        {
                            if (LUSOL.indc[LC] == I)
                            {
                                break;
                            }
                        }
                        /*               Apply the same stability test as above. */
                        AIJ = System.Math.Abs(LUSOL.a[LC]);
                        /*      Absolute test for Complete Pivoting */
                        if (TCP)
                        {
                            if (AIJ < AIJTOL)
                            {
                                continue;
                            }
                        }
                        if (LC == LC1)
                        {
                            /*                  This is the maximum element, amax.
                                                Find the biggest element in the rest of the column
                                                and hence get cmax.  We know cmax .le. 1, but
                                                we still want it exactly in order to break ties.
                                                27 Apr 2002: Settle for cmax = 1. */
                            CMAX = 1;
                            /*                  cmax   = zero
                                                for(l = lc1 + 1; l <= lc2; l++)
                                                   cmax  = max( cmax, fabs( a(l) ) )
                                                cmax   = cmax / amax */
                        }
                        else
                        {
                            /*                  aij is not the biggest element, so cmax .ge. 1.
                                                Bail out if cmax will be too big. */
                            if (TCP)
                            {
                                /*      relax */
                            }
                            else
                            {
                                if (AIJ * LTOL < AMAX)
                                {
                                    continue;
                                }
                            }
                            CMAX = AMAX / AIJ;
                        }
                        /*               aij  is big enough.  Its maximum multiplier is cmax. */
                        MERIT = NZ1 * LEN1;
                        if (MERIT == MBEST)
                        {
                            /*                  Break ties as before.
                                                (Initializing mbest < 0 prevents getting here if
                                                nothing has been found yet.) */
                            if (LBEST <= LUSOL.parmlu[lusol.LUSOL_RP_GAMMA] && CMAX <= LUSOL.parmlu[lusol.LUSOL_RP_GAMMA])
                            {
                                if (ABEST >= AIJ)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (LBEST <= CMAX)
                                {
                                    continue;
                                }
                            }
                        }
                        /*               aij  is the best pivot so far. */
                        IBEST = I;
                        JBEST = J;
                        MBEST = MERIT;
                        KBEST = LEN1;
                        ABEST = AIJ;
                        LBEST = CMAX;
                        if (NZ == 1)
                        {
                            goto x900;
                        }
                    }
                    /*            Finished with that row. */
                    if (IBEST > 0)
                    {
                        if (NROW >= MAXROW)
                        {
                            goto x290;
                        }
                    }
                }
                /*         See if it's time to quit. */
                x290:
                if (IBEST > 0)
                {
                    if (NROW >= MAXROW && NCOL >= MAXCOL)
                    {
                        goto x900;
                    }
                }
                /*         Press on with next nz. */
                NZ1 = NZ;
                if (IBEST > 0)
                {
                    KBEST = MBEST / NZ1;
                }
            }
            x900:
            ;
        }

        /* ==================================================================
   lu1mRP  uses a Markowitz criterion to select a pivot element
   for the next stage of a sparse LU factorization,
   subject to a Threshold Rook Pivoting stability criterion (TRP)
   that bounds the elements of L and U.
   ------------------------------------------------------------------
   11 Jun 2002: First version of lu1mRP derived from lu1mar.
   11 Jun 2002: Current version of lu1mRP.
   ================================================================== */
        private static void LU1MRP(LUSOLrec LUSOL, int MAXMN, double LTOL, int MAXCOL, int MAXROW, ref int IBEST, ref int JBEST, ref int MBEST, double[] AMAXR)
        {
            throw new NotImplementedException();
        }

        private static void LU1MSP(LUSOLrec LUSOL, int MAXMN, double LTOL, int MAXCOL, ref int IBEST, ref int JBEST, ref int MBEST)
        {
            throw new NotImplementedException();
        }

        private static void LU1REC(LUSOLrec LUSOL, int N, bool REALS, ref int LTOP, int[] IND, int[] LEN, int[] LOC)
        {
            throw new NotImplementedException();
        }

        private static void HDELETE(double[] HA, int[] HJ, int[] HK, ref int N, int K, ref int HOPS)
        {
            throw new NotImplementedException();
        }

        private static void LU1GAU(LUSOLrec LUSOL, int MELIM, int NSPARE, double SMALL, int LPIVC1, int LPIVC2, ref int LFIRST, int LPIVR2, int LFREE, int MINFRE, int ILAST, ref int JLAST, ref int LROW, ref int LCOL, ref int LU, ref int NFILL, int MARK, double AL, int MARKL, double AU, int IFILL, int JFILL)
        {
            throw new NotImplementedException();
        }

        private static void LU1PEN(LUSOLrec LUSOL, int NSPARE, ref int ILAST, int LPIVC1, int LPIVC2, int LPIVR1, int LPIVR2, ref int LROW, int IFILL, int JFILL)
        {
            throw new NotImplementedException();
        }

        private static void LU1PQ2(LUSOLrec LUSOL, int NZPIV, ref int NZCHNG, int IND, int LENOLD, int[] LENNEW, int[] IXLOC, int[] IX, int[] IXINV)
        {
            throw new NotImplementedException();
        }

        private static void LU1PQ3(LUSOLrec LUSOL, int MN, int[] LEN, int[] IPERM, int[] IW, ref int NRANK)
        {
            throw new NotImplementedException();
        }

        private static void LU1FUL(LUSOLrec LUSOL, int LEND, int LU1, bool TPP, int MLEFT, int NLEFT, int NRANK, int NROWU, ref int LENL, ref int LENU, ref int NSING, bool KEEPLU, double SMALL, double D, int[] IPVT)
        {
            throw new NotImplementedException();
        }

    }
}
