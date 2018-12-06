using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class lusol6a
    {
        /* ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
   File  lusol6a
      lu6sol   lu6L     lu6Lt     lu6U     Lu6Ut   lu6LD   lu6chk
   ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
   26 Apr 2002: lu6 routines put into a separate file.
   15 Dec 2002: lu6sol modularized via lu6L, lu6Lt, lu6U, lu6Ut.
                lu6LD implemented to allow solves with LDL' or L|D|L'.
   15 Dec 2002: Current version of lusol6a.f.
   ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ */

        /* ==================================================================
           lu6chk  looks at the LU factorization  A = L*U.
           If mode = 1, lu6chk is being called by lu1fac.
           (Other modes not yet implemented.)
           ------------------------------------------------------------------
           The important input parameters are

                          lprint = luparm(2)
                          keepLU = luparm(8)
                          Utol1  = parmlu(4)
                          Utol2  = parmlu(5)

           and the significant output parameters are

                          inform = luparm(10)
                          nsing  = luparm(11)
                          jsing  = luparm(12)
                          jumin  = luparm(19)
                          Lmax   = parmlu(11)
                          Umax   = parmlu(12)
                          DUmax  = parmlu(13)
                          DUmin  = parmlu(14)
                          and      w(*).

           Lmax  and Umax  return the largest elements in L and U.
           DUmax and DUmin return the largest and smallest diagonals of U
                           (excluding diagonals that are exactly zero).
           In general, w(j) is set to the maximum absolute element in
           the j-th column of U.  However, if the corresponding diagonal
           of U is small in absolute terms or relative to w(j)
           (as judged by the parameters Utol1, Utol2 respectively),
           then w(j) is changed to - w(j).
           Thus, if w(j) is not positive, the j-th column of A
           appears to be dependent on the other columns of A.
           The number of such columns, and the position of the last one,
           are returned as nsing and jsing.
           Note that nrank is assumed to be set already, and is not altered.
           Typically, nsing will satisfy      nrank + nsing = n,  but if
           Utol1 and Utol2 are rather large,  nsing > n - nrank   may occur.
           If keepLU = 0,
           Lmax  and Umax  are already set by lu1fac.
           The diagonals of U are in the top of A.
           Only Utol1 is used in the singularity test to set w(*).
           inform = 0  if  A  appears to have full column rank  (nsing = 0).
           inform = 1  otherwise  (nsing .gt. 0).
           ------------------------------------------------------------------
           00 Jul 1987: Early version.
           09 May 1988: f77 version.
           11 Mar 2001: Allow for keepLU = 0.
           17 Nov 2001: Briefer output for singular factors.
           05 May 2002: Comma needed in format 1100 (via Kenneth Holmstrom).
           06 May 2002: With keepLU = 0, diags of U are in natural order.
                        They were not being extracted correctly.
           23 Apr 2004: TRP can judge singularity better by comparing
                        all diagonals to DUmax.
           27 Jun 2004: (PEG) Allow write only if nout .gt. 0.
           ================================================================== */
        internal static void LU6CHK(LUSOLrec LUSOL, int MODE, int LENA2, ref int INFORM)
        {
            bool KEEPLU;
            bool TRP;
            int I;
            int J;
            int JUMIN;
            int K;
            int L;
            int L1;
            int L2;
            int LENL;
            int LDIAGU;
            int LPRINT;
            int NDEFIC;
            int NRANK;
            double AIJ;
            double DIAG;
            double DUMAX;
            double DUMIN;
            double LMAX;
            double UMAX;
            double UTOL1;
            double UTOL2;

            LPRINT = LUSOL.luparm[lusol.LUSOL_IP_PRINTLEVEL];
            KEEPLU = (bool)(LUSOL.luparm[lusol.LUSOL_IP_KEEPLU] != 0);
            TRP = (bool)(LUSOL.luparm[lusol.LUSOL_IP_PIVOTTYPE] == lusol.LUSOL_PIVMOD_TRP);
            NRANK = LUSOL.luparm[lusol.LUSOL_IP_RANK_U];
            LENL = LUSOL.luparm[lusol.LUSOL_IP_NONZEROS_L];
            UTOL1 = LUSOL.parmlu[lusol.LUSOL_RP_SMALLDIAG_U];
            UTOL2 = LUSOL.parmlu[lusol.LUSOL_RP_EPSDIAG_U];
            INFORM = lusol.LUSOL_INFORM_LUSUCCESS;
            LMAX = 0;
            UMAX = 0;
            LUSOL.luparm[lusol.LUSOL_IP_SINGULARITIES] = 0;
            LUSOL.luparm[lusol.LUSOL_IP_SINGULARINDEX] = 0;
            JUMIN = 0;
            DUMAX = 0;
            DUMIN = lusol.LUSOL_BIGNUM;

#if LUSOLFastClear
  MEMCLEAR(LUSOL.w + 1, LUSOL.n);
#else
            for (I = 1; I <= LUSOL.n; I++)
            {
                LUSOL.w[I] = 0;
            }
#endif
            if (KEEPLU)
            {
                /*     --------------------------------------------------------------
                        Find  Lmax.
                       -------------------------------------------------------------- */
                for (L = (LENA2 + 1) - LENL; L <= LENA2; L++)
                {
                   commonlib.SETMAX(LMAX, System.Math.Abs(LUSOL.a[L]));
                }
                /*     --------------------------------------------------------------
                        Find Umax and set w(j) = maximum element in j-th column of U.
                       -------------------------------------------------------------- */
                for (K = 1; K <= NRANK; K++)
                {
                    I = LUSOL.ip[K];
                    L1 = LUSOL.locr[I];
                    L2 = (L1 + LUSOL.lenr[I]) - 1;
                    for (L = L1; L <= L2; L++)
                    {
                        J = LUSOL.indr[L];
                        AIJ = System.Math.Abs(LUSOL.a[L]);
                        commonlib.SETMAX(LUSOL.w[J], AIJ);
                        commonlib.SETMAX(UMAX, AIJ);
                    }
                }
                LUSOL.parmlu[lusol.LUSOL_RP_MAXMULT_L] = LMAX;
                LUSOL.parmlu[lusol.LUSOL_RP_MAXELEM_U] = UMAX;
                /*     --------------------------------------------------------------
                       Find DUmax and DUmin, the extreme diagonals of U.
                       -------------------------------------------------------------- */
                for (K = 1; K <= NRANK; K++)
                {
                    J = LUSOL.iq[K];
                    I = LUSOL.ip[K];
                    L1 = LUSOL.locr[I];
                    DIAG = System.Math.Abs(LUSOL.a[L1]);
                    commonlib.SETMAX(DUMAX, DIAG);
                    if (DUMIN > DIAG)
                    {
                        DUMIN = DIAG;
                        JUMIN = J;
                    }
                }
            }
            else
            {
                /*     --------------------------------------------------------------
                       keepLU = 0.
                       Only diag(U) is stored.  Set w(*) accordingly.
                       Find DUmax and DUmin, the extreme diagonals of U.
                       -------------------------------------------------------------- */
                LDIAGU = LENA2 - LUSOL.n;
                for (K = 1; K <= NRANK; K++)
                {
                    J = LUSOL.iq[K];
                    DIAG = System.Math.Abs(LUSOL.a[LDIAGU + J]); // are in natural order
                    LUSOL.w[J] = DIAG;
                    commonlib.SETMAX(DUMAX, DIAG);
                    if (DUMIN > DIAG)
                    {
                        DUMIN = DIAG;
                        JUMIN = J;
                    }
                }
            }
            /*     --------------------------------------------------------------
                   Negate w(j) if the corresponding diagonal of U is
                   too small in absolute terms or relative to the other elements
                   in the same column of  U.

                   23 Apr 2004: TRP ensures that diags are NOT small relative to
                                other elements in their own column.
                                Much better, we can compare all diags to DUmax.
                  -------------------------------------------------------------- */
            if ((MODE == 1) && TRP)
            {
               commonlib.SETMAX(UTOL1, UTOL2 * DUMAX);
            }

            if (KEEPLU)
            {
                for (K = 1; K <= LUSOL.n; K++)
                {
                    J = LUSOL.iq[K];
                    if (K > NRANK)
                    {
                        DIAG = 0;
                    }
                    else
                    {
                        I = LUSOL.ip[K];
                        L1 = LUSOL.locr[I];
                        DIAG = System.Math.Abs(LUSOL.a[L1]);
                    }
                    if ((DIAG <= UTOL1) || (DIAG <= UTOL2 * LUSOL.w[J]))
                    {
                        lusol.LUSOL_addSingularity(LUSOL, J, ref INFORM);
                        LUSOL.w[J] = -LUSOL.w[J];
                    }
                }
            }
            else
            { // keepLU = FALSE
                for (K = 1; K <= LUSOL.n; K++)
                {
                    J = LUSOL.iq[K];
                    DIAG = LUSOL.w[J];
                    if (DIAG <= UTOL1)
                    {
                        lusol.LUSOL_addSingularity(LUSOL, J, ref INFORM);
                        LUSOL.w[J] = -LUSOL.w[J];
                    }
                }
            }
            /*     -----------------------------------------------------------------
                    Set output parameters.
                   ----------------------------------------------------------------- */
            if (JUMIN == 0)
            {
                DUMIN = 0;
            }
            LUSOL.luparm[lusol.LUSOL_IP_COLINDEX_DUMIN] = JUMIN;
            LUSOL.parmlu[lusol.LUSOL_RP_MAXELEM_DIAGU] = DUMAX;
            LUSOL.parmlu[lusol.LUSOL_RP_MINELEM_DIAGU] = DUMIN;
            /*      The matrix has been judged singular. */
            if (LUSOL.luparm[lusol.LUSOL_IP_SINGULARITIES] > 0)
            {
                INFORM = lusol.LUSOL_INFORM_LUSINGULAR;
                NDEFIC = LUSOL.n - NRANK;
                if ((LUSOL.outstream != null) && (LPRINT >= lusol.LUSOL_MSG_SINGULARITY))
                {
                    lusol.LUSOL_report(LUSOL, 0, "Singular(m%cn)  rank:%9d  n-rank:%8d  nsing:%9d\n", lusol.relationChar(LUSOL.m, LUSOL.n), NRANK, NDEFIC, LUSOL.luparm[lusol.LUSOL_IP_SINGULARITIES]);
                }
            }
            /*      Exit. */
            LUSOL.luparm[lusol.LUSOL_IP_INFORM] = INFORM;
        }

        internal void LU6SOL(LUSOLrec LUSOL, int MODE, double[] V, double[] W, int[] NZidx, ref int INFORM)
        {
            if (MODE == commonlib.LUSOL_SOLVE_Lv_v)
            { // Solve  L v(new) = v.
                LU6L(LUSOL, ref INFORM, V, NZidx);
            }
            else if (MODE == commonlib.LUSOL_SOLVE_Ltv_v)
            { // Solve  L'v(new) = v.
                LU6LT(LUSOL, ref INFORM, V, NZidx);
            }
            else if (MODE == commonlib.LUSOL_SOLVE_Uw_v)
            { // Solve  U w = v.
                LU6U(LUSOL, ref INFORM, V, W, NZidx);
            }
            else if (MODE == commonlib.LUSOL_SOLVE_Utv_w)
            { // Solve  U'v = w.
                LU6UT(LUSOL, ref INFORM, V, W, NZidx);
            }
            else if (MODE == commonlib.LUSOL_SOLVE_Aw_v)
            { // Solve  A w      = v (i.e. FTRAN)
                LU6L(LUSOL, ref INFORM, V, NZidx); // via     L v(new) = v
                LU6U(LUSOL, ref INFORM, V, W, null); // ... and U w = v(new).
            }
            else if (MODE == commonlib.LUSOL_SOLVE_Atv_w)
            { // Solve  A'v = w (i.e. BTRAN)
                LU6UT(LUSOL, ref INFORM, V, W, NZidx); // via      U'v = w
                LU6LT(LUSOL, ref INFORM, V, null); // ... and  L'v(new) = v.
            }
            else if (MODE == commonlib.LUSOL_SOLVE_Av_v)
            { // Solve  LDv(bar) = v
                LU6LD(LUSOL, ref INFORM, 1, V, NZidx); // and    L'v(new) = v(bar).
                LU6LT(LUSOL, ref INFORM, V, null);
            }
            else if (MODE == commonlib.LUSOL_SOLVE_LDLtv_v)
            { // Solve  L|D|v(bar) = v
                LU6LD(LUSOL, ref INFORM, 2, V, NZidx); // and    L'v(new) = v(bar).
                LU6LT(LUSOL, ref INFORM, V, null);
            }
        }

        internal void LU6L(LUSOLrec LUSOL, ref int INFORM, double[] V, int[] NZidx)
        {
            int JPIV;
            int K;
            int L;
            int L1;
            int LEN;
            int LENL;
            int LENL0;
            int NUML;
            int NUML0;
            double SMALL;

            //ORIGINAL LINE: register REAL VPIV;
            double VPIV = new double();
            
            ///#if LUSOLFastSolve
            double aptr = new double();
            int iptr;
            
            //ORIGINAL LINE: int *jptr;
            int jptr;
            ///#else
            int I;
            int J;
            ///#endif

            NUML0 = LUSOL.luparm[commonlib.LUSOL_IP_COLCOUNT_L0];
            LENL0 = LUSOL.luparm[commonlib.LUSOL_IP_NONZEROS_L0];
            LENL = LUSOL.luparm[commonlib.LUSOL_IP_NONZEROS_L];
            SMALL = LUSOL.parmlu[commonlib.LUSOL_RP_ZEROTOLERANCE];
            INFORM = commonlib.LUSOL_INFORM_LUSUCCESS;
            L1 = LUSOL.lena + 1;
            for (K = 1; K <= NUML0; K++)
            {
                //ORIGINAL LINE: LEN = LUSOL.lenc[K];
                LEN = LUSOL.lenc[K];
                L = L1;
                L1 -= LEN;
                //ORIGINAL LINE: JPIV = LUSOL.indr[L1];
                JPIV = LUSOL.indr[L1];
                VPIV = V[JPIV];
                if (System.Math.Abs(VPIV) > SMALL)
                {
                    /*     ***** This loop could be coded specially. */
                    ///#if LUSOLFastSolve
                    L--;
                    for (aptr = LUSOL.a[L] + L, iptr = LUSOL.indc[L] + L; LEN > 0; LEN--, aptr--, iptr--)
                    {
                        V[iptr] += (aptr) * VPIV;
                    }
                    ///#else
                    for (; LEN > 0; LEN--)
                    {
                        L--;
                        //ORIGINAL LINE: I = Convert.ToInt32(LUSOL.indc[L]);
                        I = Convert.ToInt32(LUSOL.indc);
                        //ORIGINAL LINE:  V[I] += LUSOL.a[L] * VPIV;
                        V[I] += LUSOL.a[L] * VPIV;
                    }
                    ///#endif
                }
                ///#if SetSmallToZero
                else
                {
                    V[JPIV] = 0;
                }
                ///#endif
            }
            L = (LUSOL.lena - LENL0) + 1;
            NUML = LENL - LENL0;
            /*     ***** This loop could be coded specially. */
            //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
            ///#if LUSOLFastSolve
            L--;
            for (aptr = LUSOL.a[L] + L, jptr = LUSOL.indr[L] + L, iptr = LUSOL.indc[L] + L; NUML > 0; NUML--, aptr--, jptr--, iptr--)
            {
                if (System.Math.Abs(V[jptr]) > SMALL)
                {
                    V[iptr] += (aptr) * V[jptr];
                }
                ///#if SetSmallToZero
                else
                {
                    V[jptr] = 0;
                }
                ///#endif
            }
            ///#else
            for (; NUML > 0; NUML--)
            {
                L--;
                //ORIGINAL LINE: J = LUSOL.indr[L];
                J = LUSOL.indr[L];
                if (System.Math.Abs(V[J]) > SMALL)
                {
                    //ORIGINAL LINE: I = LUSOL.indc[L];
                    I = LUSOL.indc[L];
                    //ORIGINAL LINE: V[I] += LUSOL.a[L] * V[J];
                    V[I] += LUSOL.a[L] * V[J];
                }
                ///#if SetSmallToZero
                else
                {
                    V[J] = 0;
                }
                ///#endif
            }
            ///#endif
            /*      Exit. */
            LUSOL.luparm[commonlib.LUSOL_IP_INFORM] = INFORM;
        }

           /* ==================================================================
           lu6Lt  solves   L'v = v(input).
           ------------------------------------------------------------------
           15 Dec 2002: First version derived from lu6sol.
           15 Dec 2002: Current version.
           ================================================================== */
        internal void LU6LT(LUSOLrec LUSOL, ref int INFORM, double[] V, int[] NZidx)
        {
            ///#if DoTraceL0
            double TEMP;
            ///#endif
            int K;
            int L;
            int L1;
            int L2;
            int LEN;
            int LENL;
            int LENL0;
            int NUML0;
            double SMALL;
            lusol6l0 objlusol6l0 = new lusol6l0();
            
            //ORIGINAL LINE: register REALXP SUM;
            double SUM = new double();
            
            //ORIGINAL LINE: register REAL HOLD;
            double HOLD = new double();
            
            ///#if ( LUSOLFastSolve) && !( DoTraceL0)
            
            double aptr;
            int iptr;
            int jptr;
            ///#else
            int I;
            int J;
            ///#endif

            NUML0 = LUSOL.luparm[commonlib.LUSOL_IP_COLCOUNT_L0];
            LENL0 = LUSOL.luparm[commonlib.LUSOL_IP_NONZEROS_L0];
            LENL = LUSOL.luparm[commonlib.LUSOL_IP_NONZEROS_L];
            SMALL = LUSOL.parmlu[commonlib.LUSOL_RP_ZEROTOLERANCE];
            INFORM = commonlib.LUSOL_INFORM_LUSUCCESS;
            L1 = (LUSOL.lena - LENL) + 1;
            L2 = LUSOL.lena - LENL0;

            /*     ***** This loop could be coded specially. */
            ///#if ( LUSOLFastSolve) && !( DoTraceL0)
            for (L = L1, aptr = LUSOL.a[0] + L1, iptr = LUSOL.indr[0] + L1, jptr = LUSOL.indc[0] + L1; L <= L2; L++, aptr++, iptr++, jptr++)
            {
                HOLD = V[jptr];
                if (System.Math.Abs(HOLD) > SMALL)
                {
                    V[iptr] += (aptr) * HOLD;
                }
                ///#if SetSmallToZero
                else
                {
                    V[jptr] = 0;
                }
                ///#endif
            }
            ///#else
            for (L = L1; L <= L2; L++)
            {
                //ORIGINAL CODE: J = LUSOL.indc[L];
                J = LUSOL.indc[0];
                HOLD = V[J];
                if (System.Math.Abs(HOLD) > SMALL)
                {
                    //ORIGINAL CODE: I = LUSOL.indr[L];
                    I = LUSOL.indr[0];
                    //ORIGINAL CODE: V[I] += LUSOL.a[L] * HOLD;
                    V[I] += LUSOL.a[0] * HOLD;
                }
                ///#if SetSmallToZero
                else
                {
                    V[J] = 0;
                }
                ///#endif
            }
            ///#endif

            /* Do row-based L0 version, if available */
            if ((LUSOL.L0 != null) || ((LUSOL.luparm[commonlib.LUSOL_IP_BTRANCOUNT] == 0) && objlusol6l0.LU1L0(LUSOL,LUSOL.L0 , ref INFORM)))
            {
                objlusol6l0.LU6L0T_v(LUSOL, LUSOL.L0, V, NZidx, ref INFORM);
            }

            /* Alternatively, do the standard column-based L0 version */
            else
            {
                /* Perform loop over columns */
                for (K = NUML0; K >= 1; K--)
                {
                    SUM = commonlib.ZERO;
                    LEN = LUSOL.lenc[K];
                    L1 = L2 + 1;
                    L2 += LEN;
                    /*     ***** This loop could be coded specially. */
                    ///#if ( LUSOLFastSolve) && !( DoTraceL0)
                    for (L = L1, aptr = LUSOL.a[0] + L1, jptr = LUSOL.indc[0] + L1; L <= L2; L++, aptr++, jptr++)
                    {
                        SUM += Convert.ToDouble((aptr) * V[jptr]);
                    }
                    ///#else
                    for (L = L1; L <= L2; L++)
                    {
                        J = LUSOL.indc[L];
                        ///#if ! DoTraceL0
                        SUM += LUSOL.a[L] * V[J];
                        ///#else
                        TEMP = V[LUSOL.indr[L1]] + SUM;
                        SUM += LUSOL.a[L] * V[J];
                        //ORIGINAL CODE: System.out.printf("V[%3d] = V[%3d] + L[%d,%d]*V[%3d]\n", LUSOL.indr[L1], LUSOL.indr[L1], J, LUSOL.indr[L1], J);
                        Console.WriteLine("{0}", LUSOL.indr[L1], LUSOL.indr[L1], J, LUSOL.indr[L1], J);
                        //ORIGINAL CODE:System.out.printf("%6g = %6g + %6g*%6g\n", V[LUSOL.indr[L1]] + SUM, TEMP, LUSOL.a[L], V[J]);
                        Console.WriteLine("{0}", V[LUSOL.indr[L1]] + SUM, TEMP, LUSOL.a[L], V[J]);
                        
                        ///#endif
                    }
                    ///#endif
                    V[LUSOL.indr[L1]] += (double)SUM;
                }
            }

            /*      Exit. */
            LUSOL.luparm[commonlib.LUSOL_IP_INFORM] = INFORM;
        }

        /* ==================================================================
   lu6U   solves   U w = v.          v  is not altered.
   ------------------------------------------------------------------
   15 Dec 2002: First version derived from lu6sol.
   15 Dec 2002: Current version.
   ================================================================== */
        private void LU6U(LUSOLrec LUSOL, ref int INFORM, double[] V, double[] W, int[] NZidx)
        {
            lusol6l0 objlusol6l0 = new lusol6l0();
            lusol6u objlusol6u = new lusol6u();
            /* Do column-based U version, if available */
            if ((LUSOL.U != null) || ((LUSOL.luparm[commonlib.LUSOL_IP_FTRANCOUNT] == 0) && objlusol6u.LU1U0(LUSOL, (LUSOL.U), ref INFORM)))
            {
                objlusol6u.LU6U0_v(LUSOL, LUSOL.U[0], V, W, NZidx, ref INFORM);
            }
            /* Alternatively, do the standard column-based L0 version */
            else
            {
                int I;
                int J;
                int K;
                int KLAST;
                int L;
                int L1;
                int L2;
                int L3;
                int NRANK;
                int NRANK1;
                double SMALL = new double();
                
                //ORIGINAL LINE: register REALXP T;
                double T = new double();
                
                ///#if LUSOLFastSolve
                double aptr;
                
                //ORIGINAL LINE: int *jptr;
                int jptr;
                ///#endif
                NRANK = LUSOL.luparm[commonlib.LUSOL_IP_RANK_U];
                SMALL = LUSOL.parmlu[commonlib.LUSOL_RP_ZEROTOLERANCE];
                INFORM = commonlib.LUSOL_INFORM_LUSUCCESS;
                NRANK1 = NRANK + 1;
                /*      Find the first nonzero in v(1:nrank), counting backwards. */
                for (KLAST = NRANK; KLAST >= 1; KLAST--)
                {
                    I = LUSOL.ip[KLAST];
                    if (System.Math.Abs(V[I]) > SMALL)
                    {
                        break;
                    }
                }
                L = LUSOL.n;
                //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                ///#if LUSOLFastSolve
                for (K = KLAST + 1, jptr = LUSOL.iq[0] + K; K <= L; K++, jptr++)
                {
                    W[jptr] = commonlib.ZERO;
                }
                ///#else
                for (K = KLAST + 1; K <= L; K++)
                {
                    J = LUSOL.iq[K];
                    W[J] = commonlib.ZERO;
                }
                ///#endif
                /*      Do the back-substitution, using rows 1:klast of U. */
                for (K = KLAST; K >= 1; K--)
                {
                    I = LUSOL.ip[K];
                    T = V[I];
                    L1 = LUSOL.locr[I];
                    L2 = L1 + 1;
                    L3 = (L1 + LUSOL.lenr[I]) - 1;
                    /*     ***** This loop could be coded specially. */
                    
                    ///#if LUSOLFastSolve
                    for (L = L2, aptr = LUSOL.a[0] + L2, jptr = LUSOL.indr[0] + L2; L <= L3; L++, aptr++, jptr++)
                    {
                        T -= aptr * W[jptr];
                    }
                    ///#else
                    for (L = L2; L <= L3; L++)
                    {
                        J = LUSOL.indr[L];
                        T -= LUSOL.a[L] * W[J];
                    }
                    ///#endif
                    J = LUSOL.iq[K];
                    if (System.Math.Abs((double)T) <= SMALL)
                    {
                        T = commonlib.ZERO;
                    }
                    else
                    {
                        T /= LUSOL.a[L1];
                    }
                    W[J] = (double)T;
                }
                /*      Compute residual for overdetermined systems. */
                T = commonlib.ZERO;
                for (K = NRANK1; K <= LUSOL.m; K++)
                {
                    I = LUSOL.ip[K];
                    T += System.Math.Abs(V[I]);
                }
                /*      Exit. */
                if (T > commonlib.ZERO)
                {
                    INFORM = commonlib.LUSOL_INFORM_LUSINGULAR;
                }
                LUSOL.luparm[commonlib.LUSOL_IP_INFORM] = INFORM;
                LUSOL.parmlu[commonlib.LUSOL_RP_RESIDUAL_U] = (double)T;
            }
        }

        private void LU6UT(LUSOLrec LUSOL, ref int INFORM, double[] V, double[] W, int[] NZidx)
        {
          int I;
          int J;
          int K;
          int L;
          int L1;
          int L2;
          int NRANK;
          int NRANK1;
            
          int ip = LUSOL.ip[0] + 1;
          int iq = LUSOL.iq[0] + 1;
          double SMALL = new double();

        //ORIGINAL LINE: register REAL T;
          double T = new double();

        ///#if LUSOLFastSolve
          double aptr;

          int jptr;
        ///#endif

          NRANK = LUSOL.luparm[commonlib.LUSOL_IP_RANK_U];
          SMALL = LUSOL.parmlu[commonlib.LUSOL_RP_ZEROTOLERANCE];
          INFORM = commonlib.LUSOL_INFORM_LUSUCCESS;
          NRANK1 = NRANK + 1;
          L = LUSOL.m;

        ///#if LUSOLFastSolve
          for (K = NRANK1, jptr = LUSOL.ip[0] + K; K <= L; K++, jptr++)
          {
	        V[jptr] = commonlib.ZERO;
          }
        ///#else
          for (K = NRANK1; K <= L; K++)
          {
	        I = LUSOL.ip[K];
	        V[I] = commonlib.ZERO;
          }
        ///#endif
        /*      Do the forward-substitution, skipping columns of U(transpose)
                when the associated element of w(*) is negligible. */

        ///#if false
        //  for(K = 1; K <= NRANK; K++) {
        //    I = LUSOL->ip[K];
        //    J = LUSOL->iq[K];
        ///#else
          for (K = 1; K <= NRANK; K++, ip++, iq++)
          {
	        I = ip;
	        J = iq;
        ///#endif
	        T = W[J];
	        if (System.Math.Abs(T) <= SMALL)
	        {
	          V[I] = commonlib.ZERO;
	          continue;
	        }
	        L1 = LUSOL.locr[I];
	        T /= LUSOL.a[L1];
	        V[I] = T;
	        L2 = (L1 + LUSOL.lenr[I]) - 1;
	        L1++;
        /*     ***** This loop could be coded specially. */

        ///#if LUSOLFastSolve
	        for (L = L1, aptr = LUSOL.a[0] + L1, jptr = LUSOL.indr[0] + L1; L <= L2; L++, aptr++, jptr++)
	        {
	          W[jptr] -= T * aptr;
	        }
        ///#else
	        for (L = L1; L <= L2; L++)
	        {
	          J = LUSOL.indr[L];
	          W[J] -= T * LUSOL.a[L];
	        }
        ///#endif
          }
        /*      Compute residual for overdetermined systems. */
          T = commonlib.ZERO;
          for (K = NRANK1; K <= LUSOL.n; K++)
          {
	        J = LUSOL.iq[K];
	        T += System.Math.Abs(W[J]);
          }
        /*      Exit. */
          if (T > commonlib.ZERO)
          {
	        INFORM = commonlib.LUSOL_INFORM_LUSINGULAR;
          }
          LUSOL.luparm[commonlib.LUSOL_IP_INFORM] = INFORM;
          LUSOL.parmlu[commonlib.LUSOL_RP_RESIDUAL_U] = T;
        }

        private void LU6LD(LUSOLrec LUSOL, ref int INFORM, int MODE, double[] V, int[] NZidx)
        {
            int IPIV;
            int K;
            int L;
            int L1;
            int LEN;
            int NUML0;
            double DIAG = new double();
            double SMALL = new double();
            
            //ORIGINAL LINE: register double VPIV;
            double VPIV = new double();
            
            ///#if LUSOLFastSolve
            
            double aptr = new double();
            
            int jptr;
            ///#else
            int J;
            ///#endif

            /*      Solve L D v(new) = v  or  L|D|v(new) = v, depending on mode.
                    The code for L is the same as in lu6L,
                    but when a nonzero entry of v arises, we divide by
                    the corresponding entry of D or |D|. */
            NUML0 = LUSOL.luparm[commonlib.LUSOL_IP_COLCOUNT_L0];
            SMALL = LUSOL.parmlu[commonlib.LUSOL_RP_ZEROTOLERANCE];
            INFORM = commonlib.LUSOL_INFORM_LUSUCCESS;
            L1 = LUSOL.lena + 1;
            for (K = 1; K <= NUML0; K++)
            {
                LEN = LUSOL.lenc[K];
                L = L1;
                L1 -= LEN;
                IPIV = LUSOL.indr[L1];
                VPIV = V[IPIV];
                if (System.Math.Abs(VPIV) > SMALL)
                {
                    /*     ***** This loop could be coded specially. */
                    
                    ///#if LUSOLFastSolve
                    L--;
                    for (aptr = LUSOL.a[0] + L, jptr = LUSOL.indc[0] + L; LEN > 0; LEN--, aptr--, jptr--)
                    {
                        V[jptr] += (aptr) * VPIV;
                    }
                    ///#else
                    for (; LEN > 0; LEN--)
                    {
                        L--;
                        J = LUSOL.indc[L];
                        V[J] += LUSOL.a[L] * VPIV;
                    }
                    ///#endif
                    /*      Find diag = U(ipiv,ipiv) and divide by diag or |diag|. */
                    L = LUSOL.locr[IPIV];
                    DIAG = LUSOL.a[L];
                    if (MODE == 2)
                    {
                        DIAG = System.Math.Abs(DIAG);
                    }
                    V[IPIV] = VPIV / DIAG;
                }
                
                ///#if SetSmallToZero
                else
                {
                    V[IPIV] = 0;
                }
                ///#endif
            }
        }

    }
}
