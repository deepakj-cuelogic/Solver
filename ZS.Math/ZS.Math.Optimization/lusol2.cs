using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class lusol2
    {
        /* ==================================================================
   Hbuild initializes the heap by inserting each element of Ha.
   Input:  Ha, Hj.
   Output: Ha, Hj, Hk, hops.
   ------------------------------------------------------------------
   01 May 2002: Use k for new length of heap, not k-1 for old length.
   05 May 2002: Use kk in call to stop loop variable k being altered.
                (Actually Hinsert no longer alters that parameter.)
   07 May 2002: ftnchek wants us to protect Nk, Ha(k), Hj(k) too.
   07 May 2002: Current version of Hbuild.
   ================================================================== */
        internal static void HBUILD(double[] HA, int[] HJ, int[] HK, int N, ref int HOPS)
        {
            int H = 0;
            int JV;
            int K;
            int KK;
            double V;

            HOPS = 0;
            for (K = 1; K <= N; K++)
            {
                KK = K;
                V = HA[K];
                JV = HJ[K];
                HINSERT(HA, HJ, HK, KK, V, JV, ref H);
                (HOPS) += H;
            }

        }

        /* ==================================================================
   Hdelete deletes Ha(k) from heap of length N.
   ------------------------------------------------------------------
   03 Apr 2002: Current version of Hdelete.
   01 May 2002: Need Nk for length of Hk.
   07 May 2002: Protect input parameters N, Nk, k.
   07 May 2002: Current version of Hdelete.
   ================================================================== */
        internal static void HDELETE(double[] HA, int[] HJ, int[] HK, ref int N, int K, ref int HOPS)
        {
            int JV;
            int NX;
            double V;

            NX = N;
            V = HA[NX];
            JV = HJ[NX];
            (N)--;
           HOPS = 0;
            if (K < NX)
            {
                HCHANGE(HA, HJ, HK, NX, K, V, JV, ref HOPS);
            }
        }

        /* ==================================================================
   Hchange changes Ha(k) to v in heap of length N.
   ------------------------------------------------------------------
   01 May 2002: Need Nk for length of Hk.
   07 May 2002: Protect input parameters N, Nk, k.
   07 May 2002: Current version of Hchange.
   ================================================================== */
        internal static void HCHANGE(double[] HA, int[] HJ, int[] HK, int N, int K, double V, int JV, ref int HOPS)
        {
            double V1;

            V1 = HA[K];
            HA[K] = V;
            HJ[K] = JV;
            HK[JV] = K;
            if (V1 < V)
            {
                HUP(HA, HJ, HK, K, ref HOPS);
            }
            else
            {
                HDOWN(HA, HJ, HK, N, K, ref HOPS);
            }
        }

        /* ==================================================================
   Hdown  updates heap by moving down tree from node k.
   ------------------------------------------------------------------
   01 May 2002: Need Nk for length of Hk.
   05 May 2002: Change input paramter k to kk to stop k being output.
   05 May 2002: Current version of Hdown.
   ================================================================== */
        internal static void HDOWN(double[] HA, int[] HJ, int[] HK, int N, int K, ref int HOPS)
        {
            int J;
            int JJ;
            int JV;
            int N2;
            double V;

            HOPS = 0;
            V = HA[K];
            JV = HJ[K];
            N2 = N / 2;
            /*      while 1
                    break */
            x100:
            if (K > N2)
            {
                goto x200;
            }
            HOPS++;
            J = K + K;
            if (J < N)
            {
                if (HA[J] < HA[J + 1])
                {
                    J++;
                }
            }
            /*      break */
            if (V >= HA[J])
            {
                goto x200;
            }
            HA[K] = HA[J];
            JJ = HJ[J];
            HJ[K] = JJ;
            HK[JJ] = K;
            K = J;
            goto x100;
            /*      end while */
            x200:
            HA[K] = V;
            HJ[K] = JV;
            HK[JV] = K;
        }

        /* ==================================================================
   Hinsert inserts (v,jv) into heap of length N-1
   to make heap of length N.
   ------------------------------------------------------------------
   03 Apr 2002: First version of Hinsert.
   01 May 2002: Require N to be final length, not old length.
                Need Nk for length of Hk.
   07 May 2002: Protect input parameters N, Nk.
   07 May 2002: Current version of Hinsert.
   ================================================================== */
        internal static void HINSERT(double[] HA, int[] HJ, int[] HK, int N, double V, int JV, ref int HOPS)
        {
            HA[N] = V;
            HJ[N] = JV;
            HK[JV] = N;
            HUP(HA, HJ, HK, N, ref HOPS);
        }

        /* ==================================================================
   Hup updates heap by moving up tree from node k.
   ------------------------------------------------------------------
   01 May 2002: Need Nk for length of Hk.
   05 May 2002: Change input paramter k to kk to stop k being output.
   05 May 2002: Current version of Hup.
   ================================================================== */
        internal static void HUP(double[] HA, int[] HJ, int[] HK, int K, ref int HOPS)
        {
            int J;
            int JV;
            int K2;
            double V;

            HOPS = 0;
            V = HA[K];
            JV = HJ[K];
            /*      while 1
                    break */
            x100:
            if (K < 2)
            {
                goto x200;
            }
            K2 = K / 2;
            /*      break */
            if (V < HA[K2])
            {
                goto x200;
            }
            HOPS++;
            HA[K] = HA[K2];
            J = HJ[K2];
            HJ[K] = J;
            HK[J] = K;
            K = K2;
            goto x100;
            /*      end while */
            x200:
            HA[K] = V;
            HJ[K] = JV;
            HK[JV] = K;
        }


    }
}
