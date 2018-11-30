﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class lusol1
    {
        private static void LU1OR2(LUSOLrec LUSOL)
        {
            throw new NotImplementedException();
        }

        private static void LU1OR3(LUSOLrec LUSOL, ref int LERR, ref int INFORM)
        {
            throw new NotImplementedException();
        }

        private static void LU1OR4(LUSOLrec LUSOL)
        {
            throw new NotImplementedException();
        }

        private static void LU1PQ1(LUSOLrec LUSOL, int M, int N, int[] LEN, int[] IPERM, int[] LOC, int[] INV, int[] NUM)
        {
            throw new NotImplementedException();
        }

        //C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
        private static void LU1FAD(LUSOLrec LUSOL,
#if ClassicHamaxR
//C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
			int LENA2, int LENH, REAL HA[], int HJ[], int HK[], REAL AMAXR[],
#endif
           ref int INFORM, ref int LENL, ref int LENU, ref int MINLEN, ref int MERSUM, ref int NUTRI, ref int NLTRI, ref int NDENS1, ref int NDENS2, ref int NRANK, ref double LMAX, ref double UMAX, ref double DUMAX, ref double DUMIN, ref double AKMAX)
        {
            throw new NotImplementedException();
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
            int NUMNZ=0;
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
                if (! lusol.LUSOL_realloc_a(LUSOL, MINLEN))
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
            LU1FAD(LUSOL,ref INFORM, ref LENL, ref LENU, ref MINLEN, ref MERSUM, ref NUTRI, ref NLTRI, ref NDENS1, ref NDENS2, ref NRANK, ref LMAX, ref UMAX, ref DUMAX, ref DUMIN, ref AKMAX);
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
        }

    }
}
