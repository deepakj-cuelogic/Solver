using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class lusol6l0
    {
        /* Create a row-based version of L0.
           This makes it possible to solve L0'x=h (btran) faster for sparse h,
           since we only run down the columns of L0' (rows of LO) for which
           the corresponding entry in h is non-zero. */
        internal bool LU1L0(LUSOLrec LUSOL, LUSOLmat[] mat, ref int inform)
        {
            bool status = false;
            int K;
            int L;
            int LL;
            int L1;
            int L2;
            int LENL0;
            int NUML0;
            int I;
            //ORIGINAL LINE: int *lsumr;
            int[] lsumr = null;

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
            NUML0 = LUSOL.luparm[commonlib.LUSOL_IP_COLCOUNT_L0];
            LENL0 = LUSOL.luparm[commonlib.LUSOL_IP_NONZEROS_L0];
            if ((NUML0 == 0) || (LENL0 == 0) || (LUSOL.luparm[commonlib.LUSOL_IP_ACCELERATION] == commonlib.LUSOL_BASEORDER) || ((LUSOL.luparm[commonlib.LUSOL_IP_ACCELERATION] & commonlib.LUSOL_ACCELERATE_L0) == 0))
            {
                return (status);
            }

            /* Allocate temporary array */
            //NOT REQUIRED
            //lsumr = (int)LUSOL_CALLOC((LUSOL.m + 1), (Integer.SIZE / Byte.SIZE));
            if (lsumr == null)
            {
                inform = commonlib.LUSOL_INFORM_NOMEMLEFT;
                return (status);
            }

            /* Compute non-zero counts by permuted row index (order is unimportant) */
            K = 0;
            L2 = LUSOL.lena;
            L1 = L2 - LENL0 + 1;
            for (L = L1; L <= L2; L++)
            {
                //ORIGINAL CODE: I = LUSOL.indc[L];
                I = LUSOL.indc[L];
                lsumr[I]++;
                if (lsumr[I] == 1)
                {
                    K++;
                }
            }
            LUSOL.luparm[commonlib.LUSOL_IP_ROWCOUNT_L0] = K;

            /* Check if we should apply "smarts" before proceeding to the row matrix creation */
            //NOTED ISSUE:
            if ((LUSOL.luparm[commonlib.LUSOL_IP_ACCELERATION] & commonlib.LUSOL_AUTOORDER) && ((double)LUSOL.luparm[commonlib.LUSOL_IP_ROWCOUNT_L0] / LUSOL.m > LUSOL.parmlu[commonlib.LUSOL_RP_SMARTRATIO]))
            {          
                  goto Finish;
            }

            /* We are Ok to create the new matrix object */
            //NOT REQUIRED
            //mat[0] = LUSOL_matcreate(LUSOL.m, LENL0);
            if (mat[0] == null)
            {
                inform = commonlib.LUSOL_INFORM_NOMEMLEFT;
                goto Finish;
            }

            /* Cumulate row counts to get vector offsets; first row is leftmost
               (stick with Fortran array offset for consistency) */
               //NOTED ISSUE
            mat[0].lenx = 1;
            for (K = 1; K <= LUSOL.m; K++)
            {
                //NOTED ISSUE
                mat[K].lenx = mat[K - 1].lenx + lsumr[K];
                lsumr[K] = mat[K - 1].lenx;
            }

            /* Map the matrix into row order by permuted index;
               Note: The first permuted row is located leftmost in the array.
                     The column order is irrelevant, since the indeces will
                     refer to constant / resolved values of V[] during solve. */
            L2 = LUSOL.lena;
            L1 = L2 - LENL0 + 1;
            for (L = L1; L <= L2; L++)
            {
                //ORIGINAL CODE: I = LUSOL.indc[L];
                I = LUSOL.indc[L];
                LL = Convert.ToInt32(lsumr[I]++);
                //ORIGINAL CODE: mat.a[LL] = LUSOL.a[L];;
                mat[LL].a = LUSOL.a[L];
                mat[LL].indr = LUSOL.indr[L];
                mat[LL].indc = I;
            }

            /* Pack row starting positions, and set mapper from original index to packed */
            I = 0;
            for (L = 1; L <= LUSOL.m; L++)
            {
                //ORIGINAL CODE: K = LUSOL.ip[L];
                K = LUSOL.ip;
                //NOTED ISSUE
                if (mat[K].lenx > mat[K - 1].lenx)
                {
                    I++;
                    mat[I].indx = K;
                }
            }

            /* Confirm that everything went well */
            status = true;

        /* Clean up */
            Finish:
            //NOT REQUIRED
            //FREE(lsumr);
            return (status);
        }

        /* Solve L0' v = v based on row-based version of L0, constructed by LU1L0 */
        internal void LU6L0T_v(LUSOLrec LUSOL, LUSOLmat[] mat, double[] V, int[] NZidx, ref int INFORM)
        {
            ///#if DoTraceL0
            double TEMP = new double();
            ///#endif
            int LEN;
            int K;
            int KK;
            int L;
            int L1;
            int NUML0;
            double SMALL = new double();
            //ORIGINAL LINE: register double VPIV;
            double VPIV = new double();
            ///#if ( LUSOLFastSolve) && !( DoTraceL0)
            double aptr = new double();
            int jptr;
            ///#else
            int J;
            ///#endif

            NUML0 = LUSOL.luparm[commonlib.LUSOL_IP_ROWCOUNT_L0];
            SMALL = LUSOL.parmlu[commonlib.LUSOL_RP_ZEROTOLERANCE];

            /* Loop over the nz columns of L0' - from the end, going forward. */
            for (K = NUML0; K > 0; K--)
            {
                KK = Convert.ToInt32(mat[K].indx);
                L = mat[KK].lenx;
                L1 = mat[KK - 1].lenx;
                LEN = L - L1;
                if (LEN == 0)
                {
                    continue;
                }
                /* Get value of the corresponding active entry of V[] */
                VPIV = V[KK];
                /* Only process the column of L0' if the value of V[] is non-zero */
                if (System.Math.Abs(VPIV) > SMALL)
                {
                    /*     ***** This loop could be coded specially. */
                    ///#if ( LUSOLFastSolve) && !( DoTraceL0)
                    L--;
                    //NOTED ISSUE: mat.a and mat.indr instead used mat[0].a and mat[0].indr
                    for (aptr = mat[0].a + L, jptr = mat[0].indr + L; LEN > 0; LEN--, aptr--, jptr--)
                    {
                        V[jptr] += VPIV * (aptr);
                    }
                    ///#else
                    for (; LEN > 0; LEN--)
                    {
                        L--;

                        J = mat[L].indr;
                        ///#if ! DoTraceL0
                        V[J] += VPIV * mat[L].a;
                        ///#else
                        TEMP = V[J];
                        V[J] += VPIV * mat[L].a;
                        //ORIGINAL CODE: System.out.printf("V[%3d] = V[%3d] + L[%d,%d]*V[%3d]\n", J, J, KK, J, KK);
                        Console.WriteLine("{0}",J, J, KK, J, KK);
                        //ORIGINAL CODE: System.out.printf("%6g = %6g + %6g*%6g\n", V[J], TEMP, mat.a[L], VPIV);
                        Console.WriteLine("{0}", V[J], TEMP, mat[L].a, VPIV);
                        ///#endif
                    }
                    ///#endif
                }
                ///#if SetSmallToZero
                else
                {
                    V[KK] = 0;
                }
                ///#endif
            }

        }

    }
}
