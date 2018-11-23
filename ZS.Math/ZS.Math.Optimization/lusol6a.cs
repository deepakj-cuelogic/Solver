using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class lusol6a
    {
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

          //NOTED ISSUE
          int ip = LUSOL.ip + 1;
          int iq = LUSOL.iq + 1;
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
