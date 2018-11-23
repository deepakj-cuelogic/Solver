using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class lusol6u
    {
        internal bool LU1U0(LUSOLrec LUSOL, LUSOLmat[] mat, ref int inform)
        {
            bool status = false;
            int K;
            int L;
            int LL;
            int LENU;
            int NUMU;
            int J;
            
            //ORIGINAL LINE: int *lsumc;
            int[] lsumc;

            /* Assume success */
            inform = commonlib.LUSOL_INFORM_LUSUCCESS;

            /* Check if there is anything worth doing */
            if (mat == null)
            {
                return (status);
            }
            if (mat[0] != null)
            {
                //NOT REQUIRED
                //LUSOL_matfree(mat);
            }
            NUMU = LUSOL.luparm[commonlib.LUSOL_IP_RANK_U];
            LENU = LUSOL.luparm[commonlib.LUSOL_IP_NONZEROS_U];
            if ((NUMU == 0) || (LENU == 0) || (LUSOL.luparm[commonlib.LUSOL_IP_ACCELERATION] == commonlib.LUSOL_BASEORDER) || ((LUSOL.luparm[commonlib.LUSOL_IP_ACCELERATION] & commonlib.LUSOL_ACCELERATE_U) == 0))
            {
                return (status);
            }

            /* Allocate temporary array */
            lsumc = null;//  (int)LUSOL_CALLOC((LUSOL.n + 1), (int.SIZE / Byte.SIZE));
            if (lsumc == null)
            {
                inform = commonlib.LUSOL_INFORM_NOMEMLEFT;
                return (status);
            }

            /* Compute non-zero counts by permuted column index (order is unimportant) */
            for (L = 1; L <= LENU; L++)
            {
                J = LUSOL.indr[L];
                lsumc[J]++;
            }

            /* Check if we should apply "smarts" before proceeding to the column matrix creation */
            //NOTED ISSUE
            if ((LUSOL.luparm[commonlib.LUSOL_IP_ACCELERATION] & commonlib.LUSOL_AUTOORDER) && ((double)System.Math.Sqrt((double)NUMU / LENU) > LUSOL.parmlu[Convert.ToInt32(commonlib.LUSOL_RP_SMARTRATIO)]))
            {
                goto Finish;
            }

            /* We are Ok to create the new matrix object */
            mat[0] = new LUSOLmat();// LUSOL_matcreate(LUSOL.n, LENU);
            if (mat[0] == null)
            {
                inform = commonlib.LUSOL_INFORM_NOMEMLEFT;
                goto Finish;
            }

            /* Cumulate row counts to get vector offsets; first column is leftmost
               (stick with Fortran array offset for consistency) */
            mat[0].lenx = 1;
            for (K = 1; K <= LUSOL.n; K++)
            {
                mat[K].lenx = mat[K - 1].lenx + lsumc[K];
                lsumc[K] = mat[K - 1].lenx;
            }

            /* Map the matrix into column order by permuted index;
               Note: The first permuted column is located leftmost in the array.
                     The row order is irrelevant, since the indeces will
                     refer to constant / resolved values of V[] during solve. */
            for (L = 1; L <= LENU; L++)
            {
                J = LUSOL.indr[L];
                LL = lsumc[J]++;
                mat[LL].a = LUSOL.a[L];
                mat[LL].indr = J;
                mat[LL].indc = LUSOL.indc[L];
            }

            /* Pack column starting positions, and set mapper from original index to packed */
            J = 0;
            for (L = 1; L <= LUSOL.n; L++)
            {
                K = LUSOL.iq[L];
                ///#if 1
                if (mat[K].lenx > mat[K - 1].lenx)
                ///#endif
                {
                    J++;
                    mat[J].indx = K;
                }
            }

            /* Confirm that everything went well */
            status = true;

        /* Clean up */
        Finish:
            //NOT REQUIRED
            //FREE(lsumc);
            return (status);
        }

        /* Solve U w = v based on column-based version of U, constructed by LU1U0 */
        internal void LU6U0_v(LUSOLrec LUSOL, LUSOLmat mat, double[] V, double[] W, int[] NZidx, ref int INFORM)
        {
            ///#if DoTraceU0
            double TEMP = new double();
            ///#endif
            int LEN;
            int I;
            int K;
            int L;
            int L1;
            int NRANK;
            int NRANK1;
            int KLAST;
            double SMALL = new double();
            //ORIGINAL LINE: register REAL T;
            double T = new double();
            
            ///#if ( xxLUSOLFastSolve) && !( DoTraceU0)
            double aptr;
            
            //ORIGINAL LINE: int *jptr;
            int jptr;
            ///#else
            int J;
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
            
            ///#if xxLUSOLFastSolve
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
            /* Loop over the nz columns of U - from the right, going left. */
            for (K = NRANK; K > 0; K--)
            {
                I = mat.indx[K];
                L = mat.lenx[I];
                L1 = mat.lenx[I - 1];
                LEN = L - L1;
                T = V[I];
                if (System.Math.Abs(T) <= SMALL)
                {
                    W[K] = commonlib.ZERO;
                    continue;
                }
                T /= mat.a[L1]; // Should it be L or L1 ?
                W[K] = T;
                LEN--;
                /*     ***** This loop could be coded specially. */
                
                ///#if xxLUSOLFastSolve
                L--;
                for (aptr = mat.a[0] + L, jptr = mat.indc[0] + L; LEN > 0; LEN--, aptr--, jptr--)
                {
                    V[jptr] -= T * aptr;
                }
                ///#else
                for (; LEN > 0; LEN--)
                {
                    L--;
                    J = mat.indc[L];

                    ///#if ! DoTraceL0
                    V[J] -= T * mat.a[L];
                    ///#else
                    TEMP = V[J];
                    V[J] += T * mat.a[L];
                    //ORIGINAL CODE: System.out.printf("V[%3d] = V[%3d] + L[%d,%d]*V[%3d]\n", J, J, I, J, I);
                    Console.WriteLine("{0}", J, J, I, J, I);
                    //ORIGINAL CODE: System.out.printf("%6g = %6g + %6g*%6g\n", V[J], TEMP, mat.a[L], T);
                    Console.WriteLine("{0}", V[J], TEMP, mat.a[L], T);
                    
                    ///#endif
                }
                ///#endif
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
}
