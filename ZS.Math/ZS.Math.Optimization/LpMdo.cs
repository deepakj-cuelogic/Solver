using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public class lp_MDO
    {
        /* size of the knobs [ ] array.  Only knobs [0..1] are currently used. */
        private const int COLAMD_KNOBS = 20;

        /* number of output statistics.  Only stats [0..6] are currently used. */
        private const int COLAMD_STATS = 20;

        /* stats [3]: colamd status:  zero OK, > 0 warning or notice, < 0 error */
        private const int COLAMD_STATUS = 3;

        /// <summary>
        /// Check and do changes as per logic file
        /// ORIGINAL LINE: int __WINAPI getMDO(lprec *lp, MYBOOL *usedpos, int *colorder, int *size, MYBOOL symmetric);
        /// </summary>
        internal static int getMDO(lprec lp, ref bool usedpos, ref int[] colorder, ref int size, byte symmetric)
        {
            int error = 0;
            int nrows = lp.rows + 1;
            int ncols = colorder[0];
            int i;
            int j;
            int kk;
            int n;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'int[] col_end' to 'int[][] col_end'
            int[][] col_end = null;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'int[] row_map' to 'int[][] row_map'
            int[][] row_map = null;
            int Bnz;
            int Blen;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'int? Brows' to 'int[][] Brows'
            int[][] Brows = null;
            int[] stats = new int[COLAMD_STATS];
            double[] knobs = new double[COLAMD_KNOBS];

            /* Tally the non-zero counts of the unused columns/rows of the 
               basis matrix and store corresponding "net" starting positions */
           lp_utils.allocINT(lp, col_end, ncols + 1, 0);
            int rowmap = 0;
            n = prepareMDO(lp, ref usedpos, ref colorder, ref col_end, ref rowmap);
            
            //set second [] as 0 for now; need to check at run time"
            Bnz = col_end[ncols][0];

            /* Check that we have unused basic columns, otherwise skip analysis */
            if (ncols == 0 || Bnz == 0)
            {
                goto Transfer;
            }
            /* Get net number of rows and fill mapper */
            lp_utils.allocINT(lp, row_map, nrows, 0);
            nrows = 0;
            for (i = 0; i <= lp.rows; i++)
            {
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time"
                row_map[i][0] = i - nrows;
                /* Increment eliminated row counter if necessary */
                if (!includeMDO(ref usedpos, i))
                    nrows++;
            }
            nrows = lp.rows + 1 - nrows;

            /* Store row indeces of non-zero values in the basic columns */
            Blen = colamd.colamd_recommended(Bnz, nrows, ncols);
            lp_utils.allocINT(lp, Brows, Blen, 0);
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'row_map' to 'row_map[0][0]'
            prepareMDO(lp, ref usedpos, ref colorder, ref Brows, ref row_map[0][0]);
#if Paranoia
  verifyMDO(lp, col_end, Brows, nrows, ncols);
#endif

            /* Compute the MDO */
/*#if 
  colamd_set_defaults(knobs);
  knobs [COLAMD_DENSE_ROW] = 0.2 + 0.2; // default changed for UMFPACK
  knobs [COLAMD_DENSE_COL] = knobs [COLAMD_DENSE_ROW];
  if (symmetric && (nrows == ncols))
  {
	MEMCOPY(colorder, Brows, ncols + 1);
	error = !symamd(nrows, colorder, col_end, Brows, knobs, stats, mdo_calloc, mdo_free);
  }
  else
  {
	error = !colamd(nrows, ncols, Blen, Brows, col_end, knobs, stats);
  }
#else
            if (symmetric && (nrows == ncols))
            {
                MEMCOPY(colorder, Brows, ncols + 1);
                error = !symamd(nrows, colorder, col_end, Brows, knobs, stats, mdo_calloc, mdo_free);
            }
            else
            {
                error = !colamd(nrows, ncols, Blen, Brows, col_end, (double)null, stats);
            }
#endif*/

            /* Transfer the estimated optimal ordering, adjusting for index offsets */
            Transfer:
            if (error != 0)
            {
                error = stats[COLAMD_STATUS];
            }
            else
            {
                /*NOT REQUIRED
                MEMCOPY(Brows, colorder, ncols + 1);
                */
                for (j = 0; j < ncols; j++)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time"
                    kk = col_end[j][0];
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time"
                    n = Brows[kk + 1][0];
                    colorder[j + 1] = n;
                }
            }

            /* Free temporary vectors */
            /*NOT REQUIRED
            FREE(col_end);
            if (row_map != null)
            {
                FREE(row_map);
            }
            if (Brows != null)
            {
                FREE(Brows);
            }
            */

            if (size != null)
            {
                size = ncols;
            }
            return (error);

        }

        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
        //changed from 'ref int data' to 'ref int[][] data'
        private static int prepareMDO(lprec lp, ref bool usedpos, ref int[] colorder, ref int[][] data, ref int rowmap)
        /* This routine prepares data structures for colamd().  It is called twice, the first
           time to count applicable non-zero elements by column, and the second time to fill in 
           the row indexes of the non-zero values from the first call.  Note that the colamd() 
           row index base is 0 (which suits lp_solve fine). */
        {
            int i;
            int ii;
            int j;
            int k;
            int kk;
            int nrows = lp.rows + 1;
            int ncols = colorder[0];
            int offset = 0;
            int Bnz = 0;
            int Tnz;
            bool dotally = (bool)(rowmap == null);
            MATrec mat = lp.matA;
            double hold;
            double value;
            int rownr;

            if (dotally != null)
            {
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time"
                data[0][0] = 0;
            }

            Tnz = nrows - ncols;
            for (j = 1; j <= ncols; j++)
            {
                kk = colorder[j];

                /* Process slacks */
                if (kk <= lp.rows)
                {
                    if (includeMDO(ref usedpos, kk))
                    {
                        if (dotally == null)
                        {
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                            //set second [] as 0 for now; need to check at run time"
                            data[Bnz][0] = rowmap + offset;
                        }
                        Bnz++;
                    }
                    Tnz++;
                }
                /* Process user columns */
                else
                {
                    k = kk - lp.rows;
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time"
                    i = mat.col_end[k - 1][0];
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time"
                    ii = mat.col_end[k][0];
                    Tnz += ii - i;
#if Paranoia
	  if (i >= ii)
	  {
		lp.report(lp, SEVERE, "prepareMDO: Encountered empty basic column %d\n", k);
	  }
#endif

                    /* Detect if we need to do phase 1 adjustments of zero-valued OF variable */
                    rownr = lp_matrix.COL_MAT_ROWNR(i);
                    value = lp_matrix.COL_MAT_VALUE(i);
                    hold = 0;
                    if ((rownr > 0) && includeMDO(ref usedpos, 0) && LpCls.modifyOF1(lp, kk, ref hold, 1.0))
                    {
                        if (dotally == null)
                        {
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                            //set second [] as 0 for now; need to check at run time"
                            data[Bnz][0] = offset;
                        }
                        Bnz++;
                    }
                    /* Loop over all NZ-variables */
                    for (; i < ii; i++, value += lp_matrix.matValueStep, rownr += lp_matrix.matRowColStep)
                    {
                        if (!includeMDO(ref usedpos, rownr))
                        {
                            continue;
                        }
                        /* See if we need to change phase 1 OF value */
                        if (rownr == 0)
                        {
                            //C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
                            //ORIGINAL LINE: hold = *value;
                            hold = value;
                            if (!LpCls.modifyOF1(lp, kk, ref hold, 1.0))
                            {
                                continue;
                            }
                        }
                        /* Tally uneliminated constraint row values */
                        if (dotally == null)
                        {
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                            //set second [] as 0 for now; need to check at run time"
                            data[Bnz][0] = rowmap + offset;
                        }
                        Bnz++;
                    }
                }
                if (dotally != null)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time"
                    data[j][0] = Bnz;
                }
            }
            return (Tnz);
        }

        private static bool includeMDO(ref bool usedpos, int item)
        {
            /*  Legend:   TRUE            => A basic slack variable already in the basis
              FALSE           => A column free for being pivoted in
              AUTOMATIC+TRUE  => A row-singleton user column pivoted into the basis
              AUTOMATIC+FALSE => A column-singleton user column pivoted into the basis */

            /* Handle case where we are processing all columns */
            if (usedpos == null)
            {
                return true;
            }

            else
            {
                /* Otherwise do the selective case */
                bool test = usedpos;
                /*
#if 1
	return (test != 1);
#else
                //C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
                //ORIGINAL LINE: test = test & 1;
                test=test & true;
                return (test == 0);
#endif*/
            }
            return true;
        }

        static bool verifyMDO(lprec lp, ref int col_end, ref int row_nr, int rowmax, int colmax)
        { throw new NotImplementedException(); }

    }
}
