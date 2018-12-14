using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class MATrec
    {
        public const int CAM_Record = 0;
        public const int CAM_Vector = 1;
        public const int CAM = 0;



#if CAM == CAM_Record
        public const int MatrixColAccess = CAM_Record;
#else
            public const int MatrixColAccess = CAM_Vector;
#endif

        /* Owner reference */
        public lprec lp;

        /* Active dimensions */
        public int? rows;
        public int columns;

        /* Allocated memory */
        public int rows_alloc;
        public int columns_alloc;
        public int mat_alloc; // The allocated size for matrix sized structures

        /* Sparse problem matrix storage */
#if MatrixColAccess == CAM_Record
        public MATitem[] col_mat; // mat_alloc : The sparse data storage

        public int[] col_mat_rownr;
        public int[] col_mat_colnr;
        public double[] col_mat_value;
#else
            //ORIGINAL LINE: int *col_mat_colnr;
            public int[] col_mat_colnr;
            //ORIGINAL LINE: int *col_mat_rownr;
            public int[] col_mat_rownr;
            public double col_mat_value;
#endif
        //ORIGINAL LINE: int *col_end;
        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
        public int[][] col_end; /* columns_alloc+1 : col_end[i] is the index of the first element after column i; column[i] is stored in elements col_end[i-1] to col_end[i]-1 */
        //ORIGINAL LINE: int *col_tag;
        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
        // changed from 'int[][] col_tag' to 'int[] col_tag'
        public int[][] col_tag; // user-definable tag associated with each column

#if MatrixRowAccess == RAM_Index
        //ORIGINAL LINE: int *row_mat;
        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
        // changed from 'int[] row_mat' to 'int[][] row_mat'
        public int[][] row_mat; /* mat_alloc : From index 0, row_mat contains the row-ordered index of the elements of col_mat */
        /// <summary> FIX_b077d4ca-c675-4a2d-a4a9-9d70eb69cf70 19/11/18
        /// taken out from #else
        /// changed datatype from 'double' to 'double[]'
        /// </summary>
        public double[] row_mat_value;
#elif MatrixColAccess == CAM_Record
            public MATitem row_mat; /* mat_alloc : From index 0, row_mat contains the row-ordered copy of the elements in col_mat */
#else
            //ORIGINAL LINE: int *row_mat_colnr;
            public int[] row_mat_colnr;
            //ORIGINAL LINE: int *row_mat_rownr;
            public int[] row_mat_rownr;
            public double row_mat_value;
#endif


        //ORIGINAL LINE: int *row_end;
        public int[][] row_end; /* rows_alloc+1 : row_end[i] is the index of the first element in row_mat after row i */
        //ORIGINAL LINE: int *row_tag;
        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
        // changed from 'int[] row_tag' to 'int[][] row_tag'
        public int[][] row_tag; // user-definable tag associated with each row

        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
        // changed from 'double[] colmax' to 'double[][] colmax'
        public double[][] colmax; // Array of maximum values of each column
        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
        // changed from 'double[] rowmax' to 'double[][] rowmax'
        public double[][] rowmax; // Array of maximum values of each row

        public double epsvalue; // Zero element rejection threshold
        public double infnorm; // The largest absolute value in the matrix
        public double dynrange;
        /// <summary>
        /// changed from byte to bool on 13/11/18
        /// </summary>
        public bool row_end_valid; // TRUE if row_end & row_mat are valid
        public bool is_roworder; // TRUE if the current (temporary) matrix order is row-wise
    }

    public class DeltaVrec
    {
        public lprec lp;
        public int activelevel;
        public MATrec tracker;
    }

    public class MATitem
    {
        public int rownr;
        public int colnr;
        public double value;
    }

    public static class lp_matrix
    {
        public const int matRowColStep = 1;
        public const int matValueStep = 1;
        static MATrec mat;
        static MATrec mat2;
        public const int MAT_ROUNDRC = 4;
        public const int MAT_ROUNDREL = 2;
        public const int MAT_ROUNDDEFAULT = MAT_ROUNDREL;

        /* Constants for matrix product rounding options */
        internal const int MAT_ROUNDNONE = 0;
        internal const int MAT_ROUNDABS = 1;
        //internal const int MAT_ROUNDREL = 2;
        /*internal const int MAT_ROUNDABSREL        = (MAT_ROUNDABS + MAT_ROUNDREL);
        internal const int MAT_ROUNDRC            = 4;
        internal const double MAT_ROUNDRCMIN         = 1.0; /* lp->epspivot */
        //#if 1
        //internal const  MAT_ROUNDDEFAULT      int  = MAT_ROUNDREL;  /* Typically increases performance */
        //#else
        //internal const MAT_ROUNDDEFAULT       int MAT_ROUNDABS;  /* Probably gives more precision */
        //#endif*/

        //ORIGINAL CODE: #define COL_MAT_ROWNR(item)       (mat->col_mat_rownr[item])
        internal static Func<int, int> COL_MAT_ROWNR = (item) => (mat.col_mat_rownr[item]);

        //ORIGINAL CODE: #define ROW_MAT_COLNR(item)       COL_MAT_COLNR(mat->row_mat[item])
        internal static Func<int, int> ROW_MAT_COLNR = (item) => (Convert.ToInt32(mat.row_mat[item]));

        //ORIGINAL CODE: COL_MAT2_ROWNR(item)      (mat2->col_mat_rownr[item])
        internal static Func<int, int> COL_MAT2_ROWNR = (item) => (mat2.col_mat_rownr[item]);

        //ORIGINAL LINE: COL_MAT2_VALUE(item)      (mat2->col_mat_value[item])
        internal static Func<int, double> COL_MAT2_VALUE = (item) => (mat2.col_mat_value[item]);

        /*ORIGINAL CODE: 
        #define SET_MAT_ijA(item,i,j,A)   mat->col_mat_rownr[item] = i; \
                                          mat->col_mat_colnr[item] = j; \
                                          mat->col_mat_value[item] = A
        */
        static Action<int, int, int, int> _SET_MAT_ijA = delegate (int item, int i, int j, int A)
        {
            /// <summary>
            /// Cannot implicitly convert type 'int[]' to 'int' 
            /// col_mat_rownr is of type int[], item is of type int[], i is of type int[]
            /// </summary>
            mat.col_mat_rownr[item] = i;
            mat.col_mat_colnr[item] = j;
            mat.col_mat_value[item] = A;
        };

        /*ORIGINAL CODE
         * #define COL_MAT_COPY(left,right)  COL_MAT_COLNR(left) = COL_MAT_COLNR(right); \
                                  COL_MAT_ROWNR(left) = COL_MAT_ROWNR(right); \
                                  COL_MAT_VALUE(left) = COL_MAT_VALUE(right)
                                  */
        static Action<int, int> COL_MAT_COPY = delegate (int left, int right)
        {
            int colnrLeft = COL_MAT_COLNR(left);
            colnrLeft = COL_MAT_COLNR(right);
            int rownrLeft = COL_MAT_ROWNR(left);
            rownrLeft = COL_MAT_ROWNR(right);
            double valueLeft = COL_MAT_VALUE(left);
            valueLeft = COL_MAT_VALUE(right);
        };

        //ORIGINAL CODE: #define CAM_Record                0
        public const int CAM_Record = 1;
        //ORIGINAL CODE: #define CAM_Vector                1
        public const int CAM_Vector = 0;

        //ORIGINAL CODE: #define COL_MAT_VALUE(item)       (mat->col_mat_value[item])
        internal static Func<int, double> COL_MAT_VALUE = (item) => (mat.col_mat_value[item]);


        /* //ORIGINAL CODE: /*#define COL_MAT_MOVE(to,from,rec) MEMMOVE(&COL_MAT_COLNR(to),&COL_MAT_COLNR(from),rec); \
                                   MEMMOVE(&COL_MAT_ROWNR(to),&COL_MAT_ROWNR(from),rec); \
                                   MEMMOVE(&COL_MAT_VALUE(to),&COL_MAT_VALUE(from),rec) */

        internal static Action<int, int, int> COL_MAT_MOVE = delegate (int to, int from, int rec)
        {
            //NOTED ISSUE: NEED TO CHECK ON 05/12/2018
            /*NOT REQUIRED
            MEMMOVE(COL_MAT_COLNR(to), COL_MAT_COLNR(from), rec);
            MEMMOVE(COL_MAT_ROWNR(to), COL_MAT_ROWNR(from), rec);
            MEMMOVE(COL_MAT_VALUE(to), COL_MAT_VALUE(from), rec);
            */
        };
        //ORIGINAL CODE: #define ROW_MAT_VALUE(item)       COL_MAT_VALUE(mat->row_mat[item])
        static Func<int, double> ROW_MAT_VALUE = (item) => COL_MAT_VALUE(Convert.ToInt32(mat.row_mat[item]));

        /// <summary> 
        /// FIX_0693941b-e4fc-458f-bb39-e6c3540a30fc 19/11/18
        /// </summary>
        //ORIGINAL CODE: #define COL_MAT_COLNR(item)       (mat->col_mat_colnr[item])
        internal static Func<int, int> COL_MAT_COLNR = (item) => (mat.col_mat_colnr[item]);

        /*static Action<int[]> ROW_MAT_VALUE = delegate (int[] item)
        {
            mat.row_mat[item];
        };*/

        internal static MATrec mat_create(lprec lp, int rows, int columns, double epsvalue)
        {
            MATrec newmat;

            //C++ TO C# CONVERTER TODO TASK: The memory management function 'calloc' has no equivalent in C#:
            newmat = new MATrec();  // (MATrec)calloc(1, sizeof(MATrec));
            newmat.lp = lp;

            newmat.rows_alloc = 0;
            newmat.columns_alloc = 0;
            newmat.mat_alloc = 0;

            inc_matrow_space(newmat, rows);
            newmat.rows = rows;
            inc_matcol_space(newmat, columns);
            newmat.columns = columns;
            inc_mat_space(newmat, 0);

            newmat.epsvalue = epsvalue;

            return (newmat);
        }

        internal static bool mat_memopt(MATrec mat, int rowextra, int colextra, int nzextra)
        {
            bool status = true;
            int matalloc;
            int colalloc;
            int rowalloc;

            if ((mat == null) || (rowextra < 0) || (colextra < 0) || (nzextra < 0))
                return (false);

            mat.rows_alloc = (int)commonlib.MIN(mat.rows_alloc, mat.rows + rowextra);
            mat.columns_alloc = (int)commonlib.MIN(mat.columns_alloc, mat.columns + colextra);
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            mat.mat_alloc = (int)commonlib.MIN(mat.mat_alloc, mat.col_end[mat.columns][0] + nzextra);
#if false
//  rowalloc = mat->rows_alloc;
//  colalloc = mat->columns_alloc;
//  matalloc = mat->mat_alloc;
#else
            rowalloc = mat.rows_alloc + 1;
            colalloc = mat.columns_alloc + 1;
            matalloc = mat.mat_alloc + 1;
#endif

#if MatrixColAccess == CAM_Record
            //NOT REQUIRED
            //mat.col_mat = (MATitem)realloc(mat.col_mat, matalloc * sizeof(*(mat.col_mat)));
            status &= (mat.col_mat != null);
#else
  status &= allocINT(mat.lp, (mat.col_mat_colnr), matalloc, AUTOMATIC) && allocINT(mat.lp, (mat.col_mat_rownr), matalloc, AUTOMATIC) && allocREAL(mat.lp, (mat.col_mat_value), matalloc, AUTOMATIC);
#endif
            status &= lp_utils.allocINT(mat.lp, mat.col_end, colalloc, DefineConstants.AUTOMATIC);
            if (mat.col_tag != null)
            {
                status &= lp_utils.allocINT(mat.lp, mat.col_tag, colalloc, lp_types.AUTOMATIC);
            }

#if MatrixRowAccess == RAM_Index
            status &= lp_utils.allocINT(mat.lp, (mat.row_mat), matalloc, DefineConstants.AUTOMATIC);

#elif MatrixColAccess == CAM_Record
//C++ TO C# CONVERTER TODO TASK: The memory management function 'realloc' has no equivalent in C#:
  mat.row_mat = (MATitem) realloc(mat.row_mat, matalloc * sizeof(*(mat.row_mat)));
  status &= (mat.row_mat != null);
#else
  status &= allocINT(mat.lp, (mat.row_mat_colnr), matalloc, AUTOMATIC) && allocINT(mat.lp, (mat.row_mat_rownr), matalloc, AUTOMATIC) && allocREAL(mat.lp, (mat.row_mat_value), matalloc, AUTOMATIC);
#endif
            status &= lp_utils.allocINT(mat.lp, mat.row_end, rowalloc, DefineConstants.AUTOMATIC);
            if (mat.row_tag != null)
            {
                status &= lp_utils.allocINT(mat.lp, mat.row_tag, rowalloc, DefineConstants.AUTOMATIC);
            }

            if (mat.colmax != null)
            {
                status &= lp_utils.allocREAL(mat.lp, (mat.colmax), colalloc, DefineConstants.AUTOMATIC);
            }
            if (mat.rowmax != null)
            {
                status &= lp_utils.allocREAL(mat.lp, (mat.rowmax), rowalloc, DefineConstants.AUTOMATIC);
            }

            return (status);
        }
        internal static void mat_free(MATrec[] matrix)
        {
            if ((matrix == null) || (matrix[0] == null))
            {
                return;
            }


#if MatrixColAccess == CAM_Record
            //FREE(matrix.col_mat);
#else
  FREE(matrix.col_mat_colnr);
  FREE(matrix.col_mat_rownr);
  FREE(matrix.col_mat_value);
#endif
            //FREE(matrix.col_end);
            //FREE(matrix.col_tag);


#if MatrixRowAccess == RAM_Index
            //FREE(matrix.row_mat);
#elif MatrixColAccess == CAM_Record
  FREE(matrix.row_mat);
#else
  FREE(matrix.row_mat_colnr);
  FREE(matrix.row_mat_rownr);
  FREE(matrix.row_mat_value);
#endif
            // FREE(matrix.row_end);
            // FREE(matrix.row_tag);

            //FREE(matrix.colmax);
            //FREE(matrix.rowmax);

            //FREEmatrix;
        }
        internal static bool inc_matrow_space(MATrec mat, int deltarows)
        {
            int rowsum;
            int oldrowsalloc;
            bool status = true;

            /* Adjust lp row structures */
            if (mat.rows + deltarows >= mat.rows_alloc)
            {

                /* Update memory allocation and sizes */
                oldrowsalloc = mat.rows_alloc;
                deltarows = (int)commonlib.DELTA_SIZE((double)deltarows, (mat.rows != null) ? Convert.ToDouble(mat.rows) : 0);
                commonlib.SETMAX(deltarows, lp_lib.DELTAROWALLOC);
                mat.rows_alloc += deltarows;
                rowsum = mat.rows_alloc + 1;

                /* Update row pointers */
                status = lp_utils.allocINT(mat.lp, mat.row_end, rowsum, lp_types.AUTOMATIC);
                mat.row_end_valid = false;
            }
            return (status);
        }

        /* Map-based compacting+insertion of matrix elements without changing row and column indeces.
            When mat2 is NULL, a simple compacting of non-deleted rows and columns is done. */
        internal static int mat_mapreplace(MATrec mat, LLrec rowmap, LLrec colmap, MATrec mat2)
        {
            lprec lp = mat.lp;
            int i;
            int ib;
            int ie;
            int ii;
            int j;
            int jj;
            int jb;
            int je;
            int nz;

            //ORIGINAL LINE: int *colend;
            int colend;

            //ORIGINAL LINE: int *rownr;
            int rownr;

            //ORIGINAL LINE: int *rownr2;
            int rownr2;
            int[] indirect = null;

            //ORIGINAL LINE: double *value, *value2;
            double value;

            //ORIGINAL LINE: double *value2;
            double value2;
            LpCls objLpCls = new LpCls();

            /* Check if there is something to insert */
            if ((mat2 != null) && ((mat2.col_tag == null) || (Convert.ToInt32(mat2.col_tag[0]) <= 0) || (mat_nonzeros(mat2) == 0)))
            {
                return (0);
            }

            /* Create map and sort by increasing index in "mat" */
            if (mat2 != null)
            {
                jj = Convert.ToInt32(mat2.col_tag[0]);
                //NOT REQUIRED
                //lp_utils.allocINT(lp, indirect, jj + 1, 0);
                indirect[0] = jj;
                for (i = 1; i <= jj; i++)
                {
                    indirect[i] = i;
                }

                commonlib.hpsortex(mat2.col_tag, jj, 1, indirect.Length, false, commonlib.compareINT, ref indirect);
            }

            /* Do the compacting */
            mat.row_end_valid = false;
            nz = Convert.ToInt32(mat.col_end[mat.columns]);
            ie = 0;
            ii = 0;
            if ((mat2 == null) || (indirect[0] == 0))
            {
                je = mat.columns + 1;
                jj = 1;
                jb = 0;
            }
            else
            {
                je = indirect[0];
                jj = 0;
                do
                {
                    jj++;
                    jb = Convert.ToInt32(mat2.col_tag[jj]);
                } while (jb <= 0);

            }
            //Added default row and column array[0][0] to mat.col_end, Need to check at runtime
            for (j = 1, colend = mat.col_end[0][0] + 1; j <= mat.columns; j++, colend++)
            {
                ib = ie;
                ie = colend;

                /* Always skip (condense) replacement columns */
                if (j == jb)
                {
                    jj++;
                    if (jj <= je)
                    {
                        jb = Convert.ToInt32(mat2.col_tag[jj]);
                    }
                    else
                    {
                        jb = mat.columns + 1;
                    }
                }

                /* Only include active columns */
                else if (lp_utils.isActiveLink(colmap, j))
                {
                    rownr = COL_MAT_ROWNR(ib);
                    for (; ib < ie; ib++, rownr += matRowColStep)
                    {

                        /* Also make sure the row is active */
                        if (lp_utils.isActiveLink(rowmap, rownr))
                        {
                            if (ii != ib)
                            {
                                COL_MAT_COPY(ii, ib);
                            }
                            ii++;
                        }
                    }
                }
                colend = ii;
            }
            if (mat2 == null)
            {
                goto Finish;
            }

            /* Tally non-zero insertions */
            i = 0;
            for (j = 1; j <= Convert.ToInt32(mat2.col_tag[0]); j++)
            {
                jj = Convert.ToInt32(mat2.col_tag[j]);
                if ((jj > 0) && lp_utils.isActiveLink(colmap, jj))
                {
                    jj = indirect[j];
                    je = Convert.ToInt32(mat2.col_end[jj]);
                    jb = Convert.ToInt32(mat2.col_end[jj - 1]);
                    rownr2 = COL_MAT2_ROWNR(jb);
                    for (; jb < je; jb++, rownr2 += matRowColStep)
                    {
                        if ((rownr2 > 0) && lp_utils.isActiveLink(rowmap, rownr2))
                        {
                            i++;
                        }
                    }
                }
            }

            /* Make sure we have enough matrix space */
            //Added row array as default [0], need to check at runtime.
            ii = mat.col_end[0][mat.columns] + i;
            if (mat.mat_alloc <= ii)
            {
                inc_mat_space(mat, i);
            }

            /* Do shifting and insertion - loop from the end going forward */
            jj = indirect[0];
            jj = Convert.ToInt32(mat2.col_tag[jj]);
            //Added row and column array as [0] to mat.col_end, need to check at runtime
            for (j = mat.columns, colend = mat.col_end[0][0] + mat.columns, ib = colend; j > 0; j--)
            {

                /* Update indeces for this loop */
                ie = ib;
                colend = ii;
                colend--;
                ib = colend;

                /* Insert new values */
                if (j == jj)
                {
                    /* Only include an active column */
                    if (lp_utils.isActiveLink(colmap, j))
                    {
                        jj = indirect[0];
                        jj = indirect[jj];
                        rownr = COL_MAT_ROWNR(ii - 1);
                        value = COL_MAT_VALUE(ii - 1);
                        //Added column as [0] to mat2.col_end[jj - 1], Need to check at runtime 
                        jb = mat2.col_end[jj - 1][0];
                        //Added column as [0] to mat2.col_end[jj], Need to check at runtime 
                        je = mat2.col_end[jj][0] - 1;
                        rownr2 = COL_MAT2_ROWNR(je);
                        value2 = COL_MAT2_VALUE(je);

                        /* Process constraint coefficients */
                        for (; je >= jb; je--, rownr2 -= matRowColStep, value2 -= matValueStep)
                        {
                            i = rownr2;
                            if (i == 0)
                            {
                                i = -1;
                                break;
                            }
                            else if (lp_utils.isActiveLink(rowmap, i))
                            {
                                ii--;
                                rownr = i;
                                rownr -= matRowColStep;
                                value = lp_types.my_chsign(objLpCls.is_chsign(lp, i), value2);
                                value -= matValueStep;
                            }
                        }

                        /* Then handle the objective */
                        if (i == -1)
                        {
                            lp.orig_obj[j] = lp_types.my_chsign(LpCls.is_maxim(lp), value2);
                            rownr2 -= matRowColStep;
                            value2 -= matValueStep;
                        }
                        else
                        {
                            lp.orig_obj[j] = 0;
                        }

                    }
                    /* Update replacement column index or break if no more candidates */
                    jj = --indirect[0];
                    if (jj == 0)
                    {
                        break;
                    }
                    jj = Convert.ToInt32(mat2.col_tag[jj]);
                    if (jj <= 0)
                    {
                        break;
                    }
                }
                /* Shift existing values down */
                else
                {
                    if (lp_utils.isActiveLink(colmap, j))
                    {
                        while (ie > ib)
                        {
                            ii--;
                            ie--;
                            if (ie != ii)
                            {
                                COL_MAT_COPY(ii, ie);
                            }
                        }
                    }
                }
            }

            /* Return the delta number of non-zero elements */
            Finish:
            //Added row array as default [0] to  mat.col_end[mat.columns], Need to check at runtime.
            nz -= mat.col_end[0][mat.columns];

            //NOT REQUIRED
            //FREE(indirect);

            return (nz);

        }
        static int mat_matinsert(MATrec mat, MATrec insmat) { throw new NotImplementedException(); }
        internal static int mat_zerocompact(MATrec mat)
        {
            //ORIGINAL LINE: return( mat_rowcompact(mat, TRUE))
            //SOLUTION: Method return type is int hence changed TRUE to 1.
            return (mat_rowcompact(mat, true));
        }
        internal static int mat_rowcompact(MATrec mat, bool dozeros)
        {
            int i;
            int ie;
            int ii;
            int j;
            int nn;
            int colend;
            int rownr;
            double value;

            nn = 0;
            ie = 0;
            ii = 0;
            //Added default array of row and column[0] to mat.col_end. Need to check at runtime
            for (j = 1, colend = mat.col_end[0][0] + 1; j <= mat.columns; j++, colend++)
            {
                i = ie;
                ie = colend;
                rownr = COL_MAT_ROWNR(i);
                value = COL_MAT_VALUE(i);
                for (; i < ie; i++, rownr += matRowColStep, value += matValueStep)
                {
                    if ((rownr < 0) || (dozeros && (System.Math.Abs(value) < mat.epsvalue)))
                    {
                        nn++;
                        continue;
                    }
                    if (ii != i)
                    {
                        COL_MAT_COPY(ii, i);
                    }
                    ii++;
                }
                colend = ii;
            }
            return (nn);

        }

        /* Routines to compact columns and their indeces based on precoded entries */
        internal static int mat_colcompact(MATrec mat, int prev_rows, int prev_cols)
        {
            int i;
            int ii;
            int j;
            int k;
            int n_del;
            int n_sum;
            int colend;
            int newcolend;
            int colnr;
            int newcolnr;
            bool deleted;
            lprec lp = mat.lp;
            presolveundorec lpundo = lp.presolve_undo;


            n_sum = 0;
            k = 0;
            ii = 0;
            newcolnr = 1;
            //Added default row and column[0] to  mat.col_end, need to check at runtime.
            for (j = 1, colend = newcolend = mat.col_end[0][0] + 1; j <= prev_cols; j++, colend++)
            {
                n_del = 0;
                i = k;
                k = colend;
                for (colnr = COL_MAT_COLNR(i); i < k; i++, colnr += matRowColStep)
                {
                    if (colnr < 0)
                    {
                        n_del++;
                        n_sum++;
                        continue;
                    }
                    if (ii < i)
                    {
                        COL_MAT_COPY(ii, i);
                    }
                    if (newcolnr < j)
                    {
                        //NOTED ISSUE
                        //ORIGINAL CODE: COL_MAT_COLNR(ii) = newcolnr;
                        //ERROR: The left-hand side of an assignment must be a variable, property or indexer
                        //SOLUTION: newcolnr = COL_MAT_COLNR(ii);
                        newcolnr = COL_MAT_COLNR(ii);
                    }
                    ii++;
                }
                newcolend = ii;

                deleted = (bool)(n_del > 0);
#if ONE
	/* Do hoops in case there was an empty column */
	deleted |= (MYBOOL)(!lp.wasPresolved && (lpundo.var_to_orig[prev_rows + j] < 0));

#endif
                /* Increment column variables if current column was not deleted */
                if (!deleted)
                {
                    newcolend++;
                    newcolnr++;
                }
            }
            return (n_sum);

        }
        internal static bool inc_matcol_space(MATrec mat, int deltacols)
        {
            int i;
            int colsum;
            int oldcolsalloc;
            bool status = true;

            /* Adjust lp column structures */
            if (mat.columns + deltacols >= mat.columns_alloc)
            {

                /* Update memory allocation and sizes */
                oldcolsalloc = mat.columns_alloc;
                deltacols = (int)commonlib.DELTA_SIZE(deltacols, mat.columns);
                commonlib.SETMAX(deltacols, lp_lib.DELTACOLALLOC);
                mat.columns_alloc += deltacols;
                colsum = mat.columns_alloc + 1;
                /// <summary> FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                /// PREVIOUS: status = lp_utils.allocINT(mat.lp, mat.col_end, colsum, lp_types.AUTOMATIC);
                /// ERROR IN PREVIOUS: cannot convert from 'int[]' to 'int[][]'
                /// FIX 1: changed col_end datatype from int[] to int[][]
                /// </summary>
                status = lp_utils.allocINT(mat.lp, mat.col_end, colsum, lp_types.AUTOMATIC);

                /* Update column pointers */
                if (oldcolsalloc == 0)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    mat.col_end[0][0] = 0;
                }
                for (i = (int)commonlib.MIN(oldcolsalloc, mat.columns) + 1; i < colsum; i++)
                {
                    mat.col_end[i] = mat.col_end[i - 1];
                }
                mat.row_end_valid = false;
            }
            return (status);

        }
        static bool inc_mat_space(MATrec mat, int mindelta)
        {
            int spaceneeded;
            int nz = mat_nonzeros(mat);

            if (mindelta <= 0)
            {
                mindelta = (int)commonlib.MAX((mat.rows != null) ? Convert.ToDouble(mat.rows) : 0, Convert.ToDouble(mat.columns)) + 1;
            }
            spaceneeded = (int)commonlib.DELTA_SIZE(mindelta, nz);
            commonlib.SETMAX(mindelta, spaceneeded);

            if (mat.mat_alloc == 0)
            {
                spaceneeded = mindelta;
            }
            else
            {
                spaceneeded = nz + mindelta;
            }

            if (spaceneeded >= mat.mat_alloc)
            {
                /* Let's allocate at least MAT_START_SIZE entries */
                if (mat.mat_alloc < lp_lib.MAT_START_SIZE)
                {
                    mat.mat_alloc = lp_lib.MAT_START_SIZE;
                }

                /* Increase the size by RESIZEFACTOR each time it becomes too small */
                while (spaceneeded >= mat.mat_alloc)
                {
                    mat.mat_alloc += mat.mat_alloc / lp_lib.RESIZEFACTOR;
                }

                //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if MatrixColAccess == CAM_Record
                //C++ TO C# CONVERTER TODO TASK: The memory management function 'realloc' has no equivalent in C#:
                /*NOT REQUIRED
                mat.col_mat = (MATitem)realloc(mat.col_mat, (mat.mat_alloc) * sizeof(*(mat.col_mat)));
                */
#else
	allocINT(mat.lp, (mat.col_mat_colnr), mat.mat_alloc, AUTOMATIC);
	allocINT(mat.lp, (mat.col_mat_rownr), mat.mat_alloc, AUTOMATIC);
	allocREAL(mat.lp, (mat.col_mat_value), mat.mat_alloc, AUTOMATIC);
#endif

                //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if MatrixRowAccess == RAM_Index
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                // commented on 7/12/18, check at runtime
                // lp_utils.allocINT(mat.lp, mat.row_mat, mat.mat_alloc, lp_types.AUTOMATIC);
                //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#elif MatrixColAccess == CAM_Record
//C++ TO C# CONVERTER TODO TASK: The memory management function 'realloc' has no equivalent in C#:
	mat.row_mat = (MATitem) realloc(mat.row_mat, (mat.mat_alloc) * sizeof(*(mat.row_mat)));
#else
	allocINT(mat.lp, (mat.row_mat_colnr), mat.mat_alloc, AUTOMATIC);
	allocINT(mat.lp, (mat.row_mat_rownr), mat.mat_alloc, AUTOMATIC);
	allocREAL(mat.lp, (mat.row_mat_value), mat.mat_alloc, AUTOMATIC);
#endif
            }
            return true;
        }

        internal static int mat_shiftrows(MATrec mat, ref int bbase, int delta, LLrec varmap)
        {
            int j;
            int k;
            int i;
            int ii;
            int thisrow;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *colend;
            int colend;
            int @base;
            bool preparecompact = false;
            //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
            int rownr;

            if (delta == 0)
            {
                return (0);
            }
            @base = System.Math.Abs(bbase);

            if (delta > 0)
            {

                /* Insert row by simply incrementing existing row indeces */
                if (@base <= mat.rows)
                {
                    k = mat_nonzeros(mat);
                    rownr = COL_MAT_ROWNR(0);
                    for (ii = 0; ii < k; ii++, rownr += matRowColStep)
                    {
                        if (rownr >= @base)
                        {
                            rownr += delta;
                        }
                    }
                }
                /* Set defaults (actual basis set in separate procedure) */
                for (i = 0; i < delta; i++)
                {
                    ii = @base + i;
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set first [] as 0 for now; need to check at run time
                    mat.row_end[0][ii] = 0;
                }
            }
            else if (@base <= mat.rows)
            {

                /* Check for preparation of mass-deletion of rows */
                preparecompact = (bool)(varmap != null);
                if (preparecompact)
                {
                    /* Create the offset array */
                    int[][] newrowidx = null;
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    lp_utils.allocINT(mat.lp, newrowidx, (mat.rows != null) ? Convert.ToInt32(mat.rows) + 1 : 1, 0);
                    newrowidx[0][0] = 0;
                    delta = 0;
                    for (j = 1; j <= mat.rows; j++)
                    {
                        if (lp_utils.isActiveLink(varmap, j))
                        {
                            delta++;
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                            //set second [] as 0 for now; need to check at run time
                            newrowidx[j][0] = delta;
                        }
                        else
                        {
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                            //set second [] as 0 for now; need to check at run time
                            newrowidx[j][0] = -1;
                        }
                    }
                    k = 0;
                    delta = 0;
                    @base = mat_nonzeros(mat);
                    rownr = COL_MAT_ROWNR(0);
                    for (i = 0; i < @base; i++, rownr += matRowColStep)
                    {
                        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                        //set second [] as 0 for now; need to check at run time
                        thisrow = newrowidx[rownr][0];
                        if (thisrow < 0)
                        {
                            rownr = -1;
                            delta++;
                        }
                        else
                        {
                            rownr = thisrow;
                        }
                    }
                    /*NOT REQUIRED
                    FREE(newrowidx);
                    */
                    return (delta);
                }

                /* Check if we should prepare for compacting later
                   (this is in order to speed up multiple row deletions) */
                preparecompact = (bool)(bbase < 0);
                if (preparecompact)
                {
                    bbase = (int)lp_types.my_flipsign((bbase));
                }

                /* First make sure we don't cross the row count border */
                if (@base - delta - 1 > mat.rows)
                {
                    delta = @base - ((mat.rows != null) ? Convert.ToInt32(mat.rows) - 1 : -1);
                }

                /* Then scan over all entries shifting and updating rows indeces */
                if (preparecompact)
                {
                    k = 0;
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now
                    for (j = 1, colend = mat.col_end[0][0] + 1; j <= mat.columns; j++, colend++)
                    {
                        i = k;
                        k = colend;
                        rownr = COL_MAT_ROWNR(i);
                        for (; i < k; i++, rownr += matRowColStep)
                        {
                            thisrow = rownr;
                            if (thisrow < @base)
                            {
                                continue;
                            }
                            else if (thisrow >= @base - delta)
                            {
                                rownr += delta;
                            }
                            else
                            {
                                rownr = -1;
                            }
                        }
                    }
                }
                else
                {
                    k = 0;
                    ii = 0;
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now
                    for (j = 1, colend = mat.col_end[0][0] + 1; j <= mat.columns; j++, colend++)
                    {
                        i = k;
                        k = colend;
                        rownr = COL_MAT_ROWNR(i);
                        for (; i < k; i++, rownr += matRowColStep)
                        {
                            thisrow = rownr;
                            if (thisrow >= @base)
                            {
                                if (thisrow >= @base - delta)
                                {
                                    rownr += delta;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            if (ii != i)
                            {
                                COL_MAT_COPY(ii, i);
                            }
                            ii++;
                        }
                        colend = ii;
                    }
                }
            }
            return (0);
        }

        internal static int mat_shiftcols(MATrec mat, ref int bbase, int delta, LLrec varmap)
        {
            int i;
            int ii;
            int k;
            int n;
            int @base;


            k = 0;
            if (delta == 0)
            {
                return (k);
            }
            @base = System.Math.Abs(bbase);

            if (delta > 0)
            {
                /* Shift pointers right */
                for (ii = mat.columns; ii > @base; ii--)
                {
                    i = ii + delta;
                    mat.col_end[i] = mat.col_end[ii];
                }
                /* Set defaults */
                for (i = 0; i < delta; i++)
                {
                    ii = @base + i;
                    mat.col_end[ii] = mat.col_end[ii - 1];
                }
            }
            else
            {

                /* Check for preparation of mass-deletion of columns */
                bool? preparecompact = (bool)(varmap != null);
                if (preparecompact != null)
                {
                    /* Create the offset array */
                    int j;
                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
                    int colnr;
                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
                    int colend;
                    n = 0;
                    k = 0;
                    @base = 0;
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now
                    for (j = 1, colend = mat.col_end[0][0] + 1; j <= mat.columns; j++, colend++)
                    {
                        i = k;
                        k = colend;
                        if (lp_utils.isActiveLink(varmap, j))
                        {
                            @base++;
                            ii = @base;
                        }
                        else
                        {
                            ii = -1;
                        }
                        if (ii < 0)
                        {
                            n += k - i;
                        }
                        colnr = COL_MAT_COLNR(i);
                        for (; i < k; i++, colnr += matRowColStep)
                        {
                            colnr = ii;
                        }
                    }
                    return (n);
                }

                /* Check if we should prepare for compacting later
                   (this is in order to speed up multiple column deletions) */
                //C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
                //ORIGINAL LINE: preparecompact = (MYBOOL)(*bbase < 0);
                preparecompact = ((bool)(bbase < 0));
                if (preparecompact != null)
                {
                    bbase = (int)lp_types.my_flipsign(bbase);
                }

                /* First make sure we don't cross the column count border */
                if (@base - delta - 1 > mat.columns)
                {
                    delta = @base - mat.columns - 1;
                }

                /* Then scan over all entries shifting and updating column indeces */
                if (preparecompact != null)
                {
                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
                    int colnr;
                    n = 0;
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    i = mat.col_end[@base - 1][0];
                    //set second [] as 0 for now
                    k = mat.col_end[@base - delta - 1][0];
                    for (colnr = COL_MAT_COLNR(i); i < k; i++, colnr += matRowColStep)
                    {
                        n++;
                        colnr = -1;
                    }
                    k = n;
                }
                else
                {
                    /* Delete sparse matrix data, if required */
                    if (@base <= mat.columns)
                    {
                        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                        //set second [] as 0 for now; need to check at run time
                        i = mat.col_end[@base - 1][0]; // Beginning of data to be deleted
                        //set second [] as 0 for now
                        ii = mat.col_end[@base - delta - 1][0]; // Beginning of data to be shifted left
                        n = mat_nonzeros(mat); // Total number of non-zeros
                        k = ii - i; // Number of entries to be deleted
                        if ((k > 0) && (n > i))
                        {
                            n -= ii;
                            /* contains MEMMOVE operations, hence NOT REQUIRED
                            COL_MAT_MOVE(i, ii, n);
                            */
                        }

                        /* Update indexes */
                        for (i = @base; i <= mat.columns + delta; i++)
                        {
                            ii = i - delta;
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                            //set second [] as 0 for now for both (left & right); need to check at run time
                            mat.col_end[i][0] = mat.col_end[ii][0] - k;
                        }
                    }
                }
            }
            return (k);
        }

        static MATrec mat_extractmat(MATrec mat, LLrec rowmap, LLrec colmap, byte negated)
        {
            throw new NotImplementedException();
        }
        static internal int mat_appendrow(MATrec mat, int count, double[] row, int?[] colno, double mult, bool checkrowmode)
        {
            int i;
            int j;
            int jj = 0;
            int stcol = 0;
            int elmnr;
            int orignr;
            int newnr;
            int firstcol;
            bool? addto = null;
            bool? isA;
            bool? isNZ;
            double value;
            double saved = 0;
            lprec lp = mat.lp;
            lp_lib objlp_lib = new lp_lib();

            /* Check if we are in row order mode and should add as column instead;
               the matrix will be transposed at a later stage */
            if (checkrowmode && mat.is_roworder)
            {
                ///<summary> FIX_7092efd5-8395-4e93-b63c-1412df391d55 on 19/11/18
                /// PREVIOUS: return (mat_appendcol(mat, count, row, colno, mult, false));
                /// ERROR IN PREVIOUS: cannot convert from 'int?[]' to 'int'
                /// FIX 1: changed mat_appendcol parameter type from int to int?[]
                /// ERROR 2: cannot convert from 'double[]' to 'double?[]' 
                /// (changed parameter in method definition from 'double[] column' 'double?[] column' 22/11/18
                /// </summary>
                return (mat_appendcol(mat, count, row, colno, mult, false));
            }

            /* Do initialization and validation */
            //ORIGINAL LINE: isA = (bool)(mat == lp->matA);
            isA = ((mat == lp.matA));
            //ORIGINAL LINE: isNZ = (bool)(colno != null);
            isNZ = ((colno != null));
            if (isNZ != null && (count > 0))
            {
                if (count > 1)
                {
                    commonlib.sortREALByINT(ref row, ref colno, count, 0, true);
                }
                if ((colno[0] < 1) || (colno[count - 1] > mat.columns))
                {
                    return (0);
                }
            }
            /* else if((row != NULL) && !mat->is_roworder) */
            else if (isNZ == null && (row != null) && !mat.is_roworder)
            {
                row[0] = 0;
            }

            /* Capture OF definition in row mode */
            if (isA != null && mat.is_roworder)
            {
                if (isNZ != null && (colno[0] == 0))
                {
                    value = (row != null) ? Convert.ToDouble(row[0]) : 0;
#if DoMatrixRounding
	  value = roundToPrecision(value, mat.epsvalue);
#endif
                    value = lp_scale.scaled_mat(lp, value, 0, lp.columns);
                    value = lp_types.my_chsign(objlp_lib.is_maxim(lp), value);
                    lp.orig_obj[lp.columns] = value;
                    count--;
                    row[0]++;
                    colno[0]++;
                }
                else if (isNZ == null && (row != null) && (row[0] != 0))
                {
                    value = saved = (row != null) ? Convert.ToDouble(row[0]) : 0;
#if DoMatrixRounding
	  value = roundToPrecision(value, mat.epsvalue);
#endif
                    value = lp_scale.scaled_mat(lp, value, 0, lp.columns);
                    value = lp_types.my_chsign(objlp_lib.is_maxim(lp), value);
                    lp.orig_obj[lp.columns] = value;
                    row[0] = 0;
                }
                else
                {
                    lp.orig_obj[lp.columns] = 0;
                }
            }

            /* Optionally tally and map the new non-zero values */
            firstcol = mat.columns + 1;
            if (isNZ != null)
            {
                newnr = count;
                if (newnr != 0)
                {
                    firstcol = (int)colno[0];
                    jj = (int)colno[newnr - 1];
                }
            }
            else
            {
                newnr = 0;
                if (row != null)
                {
                    /*NOT REQUIRED
                     * if (!allocbool(lp, addto, mat.columns + 1, 1))
                    {
                        return (newnr);
                    }*/
                    for (i = mat.columns; i >= 1; i--)
                    {
                        if (System.Math.Abs((sbyte)row[i]) > mat.epsvalue)
                        {
                            addto = true;
                            firstcol = i;
                            newnr++;
                        }
                    }
                }
            }

            /*NOT REQUIRED
            // Make sure we have sufficient space 
            if (!inc_mat_space(mat, newnr))
            {
                newnr = 0;
                goto Done;
            }
            */

            /* Insert the non-zero constraint values */
            orignr = mat_nonzeros(mat) - 1;
            elmnr = orignr + newnr;

            for (j = mat.columns; j >= firstcol; j--)
            {
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                stcol = mat.col_end[j][0] - 1;
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                mat.col_end[j][0] = elmnr + 1;
            }
            /* Add a new non-zero entry */
            if ((((bool)isNZ) && (j == jj)) || ((addto != null) && ((bool)addto)))
            {
                newnr--;
                if ((bool)isNZ)
                {
                    value = (row != null) ? Convert.ToDouble(row[newnr]) : 0;
                    if (newnr > 0)
                    {
                        jj = (int)colno[newnr - 1];
                    }
                    else
                    {
                        jj = 0;
                    }
                }
                else
                {
                    value = (row != null) ? Convert.ToDouble(row[j]) : 0;
                }
#if DoMatrixRounding
	  value = roundToPrecision(value, mat.epsvalue);
#endif
                value *= mult;
                if ((bool)isA)
                {
                    if (mat.is_roworder)
                    {
                        value = lp_types.my_chsign(objlp_lib.is_chsign(lp, j), value);
                    }
                    value = lp_scale.scaled_mat(lp, value, (mat.rows != null) ? Convert.ToInt32(mat.rows) : 0, j);
                }
                /*TODO: 13/11/18
                SET_MAT_ijA(mat, elmnr, mat.rows, j, value);*/
                elmnr--;
            }

            /* Shift previous column entries down */
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            i = stcol - mat.col_end[j - 1][0] + 1;
            if (i > 0)
            {
                orignr -= i;
                elmnr -= i;
                /*TODO: 13/11/18
                COL_MAT_MOVE(elmnr + 1, orignr + 1, i);*/
            }
            if (saved != 0)
                row[0] = saved;
            return (newnr);
        }

        /// <summary> FIX_7092efd5-8395-4e93-b63c-1412df391d55 on 19/11/18
        /// </summary>
        internal static int mat_appendcol(MATrec mat, int count, double[] column, int?[] rowno, double mult, bool? checkrowmode)
        {

            int i;
            int row;
            int elmnr;
            int lastnr;
            double value;
            bool isA = new bool();
            bool? isNZ = new bool();
            lprec lp = mat.lp;
            LpCls objLpCls = new LpCls();

            /* Check if we are in row order mode and should add as row instead;
               the matrix will be transposed at a later stage */
            if (checkrowmode != null && mat.is_roworder)
            {
                return (mat_appendrow(mat, count, column, rowno, mult, false));
            }

            /* Make sure we have enough space */
            /*
              if(!inc_mat_space(mat, mat->rows+1))
                return( 0 );
            */
            if (column == null)
            {
                i = 0;
            }
            else if (rowno != null)
            {
                i = count;
            }
            else
            {
                int nrows = (mat.rows != null) ? Convert.ToInt32(mat.rows) : 0;

                elmnr = 0;
                for (i = 1; i <= nrows; i++)
                {
                    if (column[i] != 0)
                    {
                        elmnr++;
                    }
                }
                i = elmnr;
            }
            if ((mat_nz_unused(mat) <= i) && !inc_mat_space(mat, i))
            {
                return (0);
            }

            /* Do initialization and validation */
            //ORIGINAL LINE: isA = (bool)(mat == lp->matA);
            isA = (bool)(mat == lp.matA);

            //ORIGINAL LINE: isNZ = (bool)(column == null || rowno != null);
            isNZ = (bool)(column == null || rowno != null);
            if (isNZ != null && (count > 0))
            {
                if (count > 1)
                {
                    commonlib.sortREALByINT(ref column, ref rowno, count, 0, true);
                }
                if ((rowno[0] < 0))
                {
                    return (0);
                }
            }
            if (rowno != null)
            {
                count--;
            }

            /* Append sparse regular constraint values */
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            elmnr = mat.col_end[mat.columns - 1][0];
            if (column != null)
            {
                row = -1;
                for (i = ((isNZ != null || !mat.is_roworder) ? 0 : 1); i <= count; i++)
                {
                    value = (column[i] != null) ? Convert.ToDouble(column[i]) : 0;
                    if (System.Math.Abs(value) > mat.epsvalue)
                    {
                        if (isNZ != null)
                        {
                            lastnr = row;
                            row = Convert.ToInt32(rowno[i]);
                            /* Check if we have come to the Lagrangean constraints */
                            if (row > mat.rows)
                            {
                                break;
                            }
                            if (row <= lastnr)
                            {
                                return (-1);
                            }
                        }
                        else
                        {
                            row = i;
                        }
                        ///#if DoMatrixRounding
                        value = lp_utils.roundToPrecision(value, mat.epsvalue);
                        ///#endif
                        if (mat.is_roworder)
                        {
                            value *= mult;
                        }
                        else if (isA)
                        {
                            value = lp_types.my_chsign(objLpCls.is_chsign(lp, row), value);
                            value = lp_scale.scaled_mat(lp, value, row, mat.columns);
                            if (!mat.is_roworder && (row == 0))
                            {
                                lp.orig_obj[mat.columns] = value;
                                continue;
                            }
                        }

                        /* Store the item and update counters */
                        //TO DO
                        _SET_MAT_ijA(elmnr, row, mat.columns, (int)value);
                        elmnr++;
                    }
                }

                /* Fill dense Lagrangean constraints */
                if (LpCls.get_Lrows(lp) > 0)
                {
                    ///NOTED ISSUE
                    //double? (column != null) ? Convert.ToDouble(column[0]) : 0 + Convert.ToDouble(mat.rows)
                    double[] ccn = new double[1];
                    ccn[0] = ((mat.rows != null) ? Convert.ToDouble(mat.rows) : 0) + ((column != null) ? Convert.ToDouble(column[0]) : 0);
                    mat_appendcol(lp.matL, LpCls.get_Lrows(lp), ccn, null, mult, checkrowmode);
                }
            }

            /* Set end of data */
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            mat.col_end[mat.columns][0] = elmnr;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now for both; need to check at run time
            return (mat.col_end[mat.columns][0] - mat.col_end[mat.columns - 1][0]);
        }
        internal static byte mat_get_data(lprec lp, int matindex, bool isrow, int[] rownr, int[] colnr, double[][] value)
        {
            MATrec mat = lp.matA;

#if MatrixRowAccess == RAM_Index
            if (isrow)
            {
                matindex = Convert.ToInt32(mat.row_mat[matindex]);
            }
            if (rownr != null)
            {
                rownr[0] = COL_MAT_ROWNR(matindex);
            }
            if (colnr != null)
            {
                colnr[0] = COL_MAT_COLNR(matindex);
            }
            if (value != null)
            {
                //first added as [0], need to check at runtime
                value[0][0] = COL_MAT_VALUE(matindex);
            }

#else
  if (isrow)
  {
	if (rownr != null)
	{
	  rownr[0] = ROW_MAT_ROWNR(matindex);
	}
	if (colnr != null)
	{
	  colnr[0] = ROW_MAT_COLNR(matindex);
	}
	if (value != null)
	{
	  value[0] = ROW_MAT_VALUE(matindex);
	}
  }
  else
  {
	if (rownr != null)
	{
	  rownr[0] = COL_MAT_ROWNR(matindex);
	}
	if (colnr != null)
	{
	  colnr[0] = COL_MAT_COLNR(matindex);
	}
	if (value != null)
	{
	  value[0] = COL_MAT_VALUE(matindex);
	}
  }

#endif

            return (1);
        }
        static bool mat_set_rowmap(MATrec mat, int row_mat_index, int rownr, int colnr, int col_mat_index)
        {
            
#if MatrixRowAccess == RAM_Index
            //Added second array for column as [0] , Need to check at runtime.
            mat.row_mat[row_mat_index][0] =  col_mat_index;

            
#elif MatrixColAccess == CAM_Record
  mat.row_mat[row_mat_index].rownr = rownr;
  mat.row_mat[row_mat_index].colnr = colnr;
  mat.row_mat[row_mat_index].value = COL_MAT_VALUE(col_mat_index);

#else
  mat.row_mat_rownr[row_mat_index] = rownr;
  mat.row_mat_colnr[row_mat_index] = colnr;
  mat.row_mat_value[row_mat_index] = COL_MAT_VALUE(col_mat_index);

#endif
            return true;
        }

        internal static bool mat_indexrange(MATrec mat, int index, bool isrow, int startpos, int endpos)
        {
#if Paranoia
  if (isrow && ((index < 0) || (index > mat.rows)))
  {
	return (0);
  }
  else if (!isrow && ((index < 1) || (index > mat.columns)))
  {
	return (0);
  }
#endif

            if (isrow && mat_validate(mat))
            {
                if (index == 0)
                {
                    startpos = 0;
                }
                else
                {
                    startpos = Convert.ToInt32(mat.row_end[index - 1]);
                }
                endpos = Convert.ToInt32(mat.row_end[index]);
            }
            else
            {
                startpos = Convert.ToInt32(mat.col_end[index - 1]);
                endpos = Convert.ToInt32(mat.col_end[index]);
            }
            return (true);

        }
        internal static bool mat_validate(MATrec mat)
        /* Routine to make sure that row mapping arrays are valid */
        {
            int i;
            int j;
            int je;
            int[] rownum = null;
            //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
            int rownr;
            //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
            int colnr;

            if (!mat.row_end_valid)
            {

                // NOT REQUIRED: MEMCLEAR(mat.row_end, mat.rows + 1);
                // NOT REQUIRED: allocINT(mat.lp, rownum, mat.rows + 1, 1);

                /* First tally row counts and then cumulate them */
                j = mat_nonzeros(mat);
                rownr = COL_MAT_ROWNR(0);
                for (i = 0; i < j; i++, rownr += matRowColStep)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    mat.row_end[rownr][0]++;
                }
                for (i = 1; i <= mat.rows; i++)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now for both; need to check at run time
                    mat.row_end[i][0] += mat.row_end[i - 1][0];
                }

                /* Calculate the column index for every non-zero */
                for (i = 1; i <= mat.columns; i++)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    j = mat.col_end[i - 1][0];
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    je = mat.col_end[i][0];
                    rownr = COL_MAT_ROWNR(j);
                    /// <summary> FIX_0693941b-e4fc-458f-bb39-e6c3540a30fc 19/11/18
                    /// PREVIOUS: colnr = COL_MAT_COLNR(j);
                    /// ERROR IN PREVIOUS: Cannot implicitly convert type 'double' to 'int'.An explicit conversion exists (are you missing a cast?)
                    /// FIX 1: changed Func return type from double to int
                    /// </summary>
                    colnr = COL_MAT_COLNR(j);
                    for (; j < je; j++, rownr += matRowColStep, colnr += matRowColStep)
                    {
                        /*#if Paranoia
                                if ((*rownr < 0) || (*rownr > mat.rows))
                                {
                                  report(mat.lp, SEVERE, "mat_validate: Matrix value storage error row %d [0..%d], column %d [1..%d]\n", *rownr, mat.rows, *colnr, mat.columns);
                                  mat.lp.spx_status = UNKNOWNERROR;
                                  return (0);
                                }
                        #endif*/
                        colnr = i;
                        if (rownr == 0)
                        {
                            mat_set_rowmap(mat, rownum[rownr], rownr, i, j);
                        }
                        else
                        {
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                            //set second [] as 0 for now; need to check at run time
                            mat_set_rowmap(mat, mat.row_end[rownr - 1][0] + rownum[rownr], rownr, i, j);
                        }
                        rownum[rownr]++;
                    }
                }

                //FREE(rownum);
                mat.row_end_valid = true;
            }

            if (mat == mat.lp.matA)
            {
                mat.lp.model_is_valid = 1;
            }
            return true;
        }

        static byte mat_equalRows(MATrec mat, int baserow, int comprow) { throw new NotImplementedException(); }

        /* Implement combined binary/linear sub-search for matrix look-up */
        internal static int mat_findelm(MATrec mat, int row, int column)
        {
            int low;
            int high;
            int mid;
            int item;

            ///#if false
            //  if(mat->row_end_valid && (row > 0) &&
            //     (ROW_MAT_COLNR(mat->row_mat[(low = mat->row_end[row-1])]) == column))
            //    return(low);
            ///#endif

            lp_report objlpReport = new lp_report();

            if ((column < 1) || (column > mat.columns))
            {
                string msg = "mat_findelm: Column {0} out of range\n";
                objlpReport.report(mat.lp, lp_lib.IMPORTANT, ref msg, column);
                return (-1);
            }
            if ((row < 0) || (row > mat.rows))
            {
                string msg = "mat_findelm: Row {0} out of range\n";
                objlpReport.report(mat.lp, lp_lib.IMPORTANT, ref msg, row);
                return (-1);
            }
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            low = mat.col_end[column - 1][0];
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            high = mat.col_end[column][0] - 1;
            if (low > high)
            {
                return (-2);
            }

            /* Do binary search logic */
            mid = (low + high) / 2;
            item = COL_MAT_ROWNR(mid);
            while (high - low > commonlib.LINEARSEARCH)
            {
                if (item < row)
                {
                    low = mid + 1;
                    mid = (low + high) / 2;
                    item = COL_MAT_ROWNR(mid);
                }
                else if (item > row)
                {
                    high = mid - 1;
                    mid = (low + high) / 2;
                    item = COL_MAT_ROWNR(mid);
                }
                else
                {
                    low = mid;
                    high = mid;
                }
            }

            /* Do linear scan search logic */
            if ((high > low) && (high - low <= commonlib.LINEARSEARCH))
            {
                item = COL_MAT_ROWNR(low);
                while ((low < high) && (item < row))
                {
                    low++;
                    item = COL_MAT_ROWNR(low);
                }
                if (item == row)
                {
                    high = low;
                }
            }

            if ((low == high) && (row == item))
            {
                return (low);
            }
            else
            {
                return (-2);
            }

        }
        static int mat_findins(MATrec mat, int row, int column, int insertpos, byte validate) { throw new NotImplementedException(); }
        static internal void mat_multcol(MATrec mat, int col_nr, double mult, bool DoObj)
        {
            int i;
            int ie;
            bool isA;
            LpCls objLpCls = new LpCls();

#if Paranoia
              if ((col_nr < 1) || (col_nr > mat.columns))
              {
	            report(mat.lp, IMPORTANT, "mult_column: Column %d out of range\n", col_nr);
	            return;
              }
#endif
            if (mult == 1.0)
                return;

            isA = (mat == mat.lp.matA);

            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            ie = mat.col_end[col_nr][0];
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            for (i = mat.col_end[col_nr - 1][0]; i < ie; i++)
            {
                ///<summary>
                /// PREVIOUS: COL_MAT_VALUE(i) *= mult;
                /// ERROR IN PREVIOUS: The left-hand side of an assignment must be a variable, property or indexer
                /// FIX 1: changed to mat.col_mat_value[i] *= mult;
                /// need to check while implementing
                /// </summary>
                mat.col_mat_value[i] *= mult;
            }
            if (isA)
            {
                if (DoObj)
                {
                    mat.lp.orig_obj[col_nr] *= mult;
                }
                if (LpCls.get_Lrows(mat.lp) > 0)
                {
                    mat_multcol(mat.lp.matL, col_nr, mult, DoObj);
                }
            }
        }

        internal static double mat_getitem(MATrec mat, int row, int column)
        {
            int elmnr;

#if DirectOverrideOF
  if ((row == 0) && (mat == mat.lp.matA) && (mat.lp.OF_override != null))
  {
	return (mat.lp.OF_override[column]);
  }
  else
#endif
            {
                elmnr = mat_findelm(mat, row, column);
                if (elmnr >= 0)
                {
                    return (COL_MAT_VALUE(elmnr));
                }
                else
                {
                    return (0);
                }
            }
        }
        static byte mat_setitem(MATrec mat, int row, int column, double value) { throw new NotImplementedException(); }
        static byte mat_additem(MATrec mat, int row, int column, double delta) { throw new NotImplementedException(); }
        internal static bool mat_setvalue(MATrec mat, int Row, int Column, double Value, bool doscale)
        {
            int elmnr = 0;
            int lastelm;
            int i;
            int RowA = Row;
            int ColumnA = Column;
            bool isA;
            LpCls objLpCls = new LpCls();

            /* This function is inefficient if used to add new matrix entries in
               other places than at the end of the matrix. OK for replacing existing
               a non-zero value with another non-zero value */
            isA = (bool)(mat == mat.lp.matA);
            if (mat.is_roworder)
            {
                lp_utils.swapINT(ref Row, ref Column);
            }

            /* Set small numbers to zero */
            if (System.Math.Abs(Value) < mat.epsvalue)
            {
                Value = 0;
            }
#if DoMatrixRounding
  else
  {
	Value = roundToPrecision(Value, mat.epsvalue);
  }
#endif

            /* Check if we need to update column space */
            if (Column > mat.columns)
            {
                if (isA)
                {
                    LpCls.inc_col_space(mat.lp, ColumnA - mat.columns);
                }
                else
                {
                    inc_matcol_space(mat, Column - mat.columns);
                }
            }

            /* Find out if we already have such an entry, or return insertion point */
            i = mat_findins(mat, Row, Column, elmnr, 0);
            if (i == -1)
            {
                return false;
            }

            if (isA)
            {
                LpCls.set_action(ref mat.lp.spx_action, lp_lib.ACTION_REBASE | lp_lib.ACTION_RECOMPUTE | lp_lib.ACTION_REINVERT);
            }

            if (i >= 0)
            {
                /* there is an existing entry */
                if (System.Math.Abs(Value) > mat.epsvalue)
                { // we replace it by something non-zero
                    if (isA)
                    {
                        Value = lp_types.my_chsign(objLpCls.is_chsign(mat.lp, RowA), Value);
                        if (doscale && mat.lp.scaling_used)
                        {
                            Value = lp_scale.scaled_mat(mat.lp, Value, RowA, ColumnA);
                        }
                    }
                    //NEED TO CHECK AT RUNTIME.
                    //ERROR: Left hand side of an assingment must be variable property or index, Whereas COL_MAT_VALUE is Func
                    //ORIGINAL LINE: COL_MAT_VALUE(elmnr) = Value;
                    //BELOW IS THE TENTATIVE SOLUTION
                    Value = COL_MAT_VALUE(elmnr);
                }
                else
                { // setting existing non-zero entry to zero. Remove the entry
                  /* This might remove an entire column, or leave just a bound. No
                      nice solution for that yet */

                    /* Shift up tail end of the matrix */
                    lastelm = mat_nonzeros(mat);
#if false
//      for(i = elmnr; i < lastelm ; i++) {
//        COL_MAT_COPY(i, i + 1);
//      }
#else
                    lastelm -= elmnr;
                    COL_MAT_MOVE(elmnr, elmnr + 1, lastelm);
#endif
                    for (i = Column; i <= mat.columns; i++)
                    {
                        //Added first array as [0] default, need to check at runtime.
                        mat.col_end[0][i]--;
                    }

                    mat.row_end_valid = false;
                }
            }
            else if (System.Math.Abs(Value) > mat.epsvalue)
            {
                /* no existing entry. make new one only if not nearly zero */
                /* check if more space is needed for matrix */
                if (!inc_mat_space(mat, 1))
                {
                    return false;
                }

                if (Column > mat.columns)
                {
                    i = mat.columns + 1;
                    if (isA)
                    {
                        LpCls.shift_coldata(mat.lp, i, ColumnA - mat.columns, null);
                    }
                    else
                    {
                        mat_shiftcols(mat, ref i, Column - mat.columns, null);
                    }
                }

                /* Shift down tail end of the matrix by one */
                lastelm = mat_nonzeros(mat);
#if ONE
	for (i = lastelm; i > elmnr ; i--)
	{
	  COL_MAT_COPY(i, i - 1);
	}
#else
                lastelm -= elmnr - 1;
                COL_MAT_MOVE(elmnr + 1, elmnr, lastelm);
#endif

                /* Set new element */
                if (isA)
                {
                    Value = lp_types.my_chsign(objLpCls.is_chsign(mat.lp, RowA), Value);
                    if (doscale)
                    {
                        Value = lp_scale.scaled_mat(mat.lp, Value, RowA, ColumnA);
                    }
                }
                _SET_MAT_ijA(elmnr, Row, Convert.ToInt32(Column), Convert.ToInt32(Value));

                /* Update column indexes */
                for (i = Column; i <= mat.columns; i++)
                {
                    //Added first array [0] default, need to check at runtime.
                    mat.col_end[0][i]++;
                }

                mat.row_end_valid = false;
            }

            if (isA && (mat.lp.var_is_free != null) && (mat.lp.var_is_free[0][ColumnA] > 0))
            {
                return (mat_setvalue(mat, RowA, Convert.ToInt32(mat.lp.var_is_free[ColumnA]), -Value, doscale));
            }
            return (true);
        }
        internal static int mat_nonzeros(MATrec mat)
        {
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            return (mat.col_end[mat.columns][0]);
        }
        internal static int mat_collength(MATrec mat, int colnr)
        {
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now for both; need to check at run time
            return (mat.col_end[colnr][0] - mat.col_end[colnr - 1][0]);
        }
        static int mat_rowlength(MATrec mat, int rownr)
        {
            if (mat_validate(mat))
            {
                if (rownr <= 0)
                {
                    return (Convert.ToInt32(mat.row_end[0]));
                }
                else
                {
                    //Added second array as default [0], need to check at runtime
                    return (mat.row_end[rownr][0] - mat.row_end[rownr - 1][0]);
                }
            }
            else
            {
                return (0);
            }
        }
        static internal void mat_multrow(MATrec mat, int row_nr, double mult)
        {
            int i;
            int k1;
            int k2;

            /*#if false
            //  if(row_nr == 0) {
            //    k2 = mat->col_end[0];
            //    for(i = 1; i <= mat->columns; i++) {
            //      k1 = k2;
            //      k2 = mat->col_end[i];
            //      if((k1 < k2) && (COL_MAT_ROWNR(k1) == row_nr))
            //        COL_MAT_VALUE(k1) *= mult;
            //    }
            //  }
            //  else if(mat_validate(mat)) {
            //    if(row_nr == 0)
            //      k1 = 0;
            //    else
            #else*/
            if (mat_validate(mat))
            {
                if (row_nr == 0)
                {
                    k1 = 0;
                }
                else
                {
                    //#endif
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    k1 = mat.row_end[row_nr - 1][0];
                }
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                k2 = mat.row_end[row_nr][0];
                for (i = k1; i < k2; i++)
                {
                    ///<summary> FIX_b077d4ca-c675-4a2d-a4a9-9d70eb69cf70 19/11/18
                    /// PREVIOUS: ROW_MAT_VALUE(i) *= mult;
                    /// ERROR IN PREVIOUS: The left-hand side of an assignment must be a variable, property or indexer
                    /// FIX 1: changed to mat.col_mat_value[i] *= mult;
                    /// need to check while implementing
                    /// </summary>
                    mat.row_mat_value[i] *= mult;
                }
            }

        }
        //Changed By: CS Date:28/11/2018
        internal static void mat_multadd(MATrec mat, double[] lhsvector, int varnr, double mult)
        {
            int colnr;
            //ORIGINAL LINE: register int ib, ie, *matRownr;
            int ib;
            int ie;
            //ORIGINAL LINE: int *matRownr;
            int matRownr;
            //ORIGINAL LINE: register double *matValue;
            double matValue;

            LpCls objLpCls = new LpCls();

            /* Handle case of a slack variable */
            if (varnr <= mat.lp.rows)
            {
                lhsvector[varnr] += mult;
                return;
            }

            /* Do operation on the objective */
            if (mat.lp.matA == mat)
            {
                lhsvector[0] += LpCls.get_OF_active(mat.lp, varnr, mult);
            }

            /* Scan the constraint matrix target columns */
            colnr = varnr - mat.lp.rows;
            ib = Convert.ToInt32(mat.col_end[colnr - 1]);
            ie = Convert.ToInt32(mat.col_end[colnr]);
            if (ib < ie)
            {

                /* Initialize pointers */
                matRownr = COL_MAT_ROWNR(ib);
                matValue = COL_MAT_VALUE(ib);

                /* Then loop over all regular rows */
                for (; ib < ie; ib++, matValue += matValueStep, matRownr += matRowColStep)
                {
                    lhsvector[matRownr] += mult * matValue;
                }
            }

        }
        static byte mat_setrow(MATrec mat, int rowno, int count, double[] row, int colno, byte doscale, byte checkrowmode) { throw new NotImplementedException(); }
        static byte mat_setcol(MATrec mat, int colno, int count, double[] column, int rowno, byte doscale, byte checkrowmode) { throw new NotImplementedException(); }
        static byte mat_mergemat(MATrec target, MATrec source, byte usecolmap) { throw new NotImplementedException(); }
        static int mat_checkcounts(MATrec mat, int rownum, int colnum, byte freeonexit) { throw new NotImplementedException(); }
        static int mat_expandcolumn(MATrec mat, int colnr, double[] column, int[] nzlist, byte signedA) { throw new NotImplementedException(); }
        static byte mat_computemax(MATrec mat) { throw new NotImplementedException(); }
        internal static bool mat_transpose(MATrec mat)
        {
            int i;
            int j;
            int nz;
            int k;
            bool status;

            status = mat_validate(mat);
            if (status)
            {
                /* Create a column-ordered sparse element list; "column" index must be shifted */
                nz = mat_nonzeros(mat);
                if (nz > 0)
                {
                    //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if MatrixColAccess == CAM_Record
                    /* PREVIOUS CODE:
                    MATitem[] newmat;
                    newmat = Arrays.InitializeWithDefaultInstances<MATitem>((mat.mat_alloc));
                    as we don't know the required size of the array, we can't use newmat = new MATitem[]();
                    need to check while implementing
                    */
                    //ArrayList newmat = new ArrayList();
                    List<MATitem> newmat = new List<MATitem>();
                    //newmat.Add(new MATitem());
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    j = mat.row_end[0][0];
                    for (i = nz - 1; i >= j; i--)
                    {
                        k = i - j;
                        newmat[k] = mat.col_mat[mat.row_mat[i][0]];
                        newmat[k].rownr = newmat[k].colnr;
                    }
                    for (i = j - 1; i >= 0; i--)
                    {
                        k = nz - j + i;
                        newmat[k] = mat.col_mat[mat.row_mat[i][0]];
                        newmat[k].rownr = newmat[k].colnr;
                    }
                    //pointers are replaced by object
                    lp_utils.swapPTR(mat.col_mat, newmat.Cast<object>().ToArray());
                    /*NOT REQUIRED
                    FREE(newmat);*/
#else
	  double[] newValue = 0;
	  int[] newRownr = 0;
	  allocREAL(mat.lp, newValue, mat.mat_alloc, 0);
	  allocINT(mat.lp, newRownr, mat.mat_alloc, 0);

	  j = mat.row_end[0];
	  for (i = nz - 1; i >= j ; i--)
	  {
		k = i - j;
		newValue[k] = ROW_MAT_VALUE(i);
		newRownr[k] = ROW_MAT_COLNR(i);
	  }
	  for (i = j - 1; i >= 0 ; i--)
	  {
		k = nz - j + i;
		newValue[k] = ROW_MAT_VALUE(i);
		newRownr[k] = ROW_MAT_COLNR(i);
	  }

	  swapPTR((object) mat.col_mat_rownr, (object) newRownr);
	  swapPTR((object) mat.col_mat_value, (object) newValue);
	  FREE(newValue);
	  FREE(newRownr);
#endif
                }

                /* Transfer row start to column start position; must adjust for different offsets */
                if (mat.rows == mat.rows_alloc)
                {
                    inc_matcol_space(mat, 1);
                }
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                j = mat.row_end[0][0];
                for (i = (mat.rows != null) ? Convert.ToInt32(mat.rows) : 0; i >= 1; i--)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    mat.row_end[i][0] -= j;
                }
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                mat.row_end[(mat.rows != null) ? Convert.ToInt32(mat.rows) : 0][0] = nz;
                lp_utils.swapPTR(mat.row_end.Cast<Object>().ToArray(), mat.col_end.Cast<Object>().ToArray());

                /* Swap arrays of maximum values */
                lp_utils.swapPTR(mat.rowmax.Cast<object>().ToArray(), mat.colmax.Cast<object>().ToArray());

                /* Swap array sizes */
                int item1 = (mat.rows != null) ? Convert.ToInt32(mat.rows) : 0;
                lp_utils.swapINT(ref item1, ref mat.columns);
                lp_utils.swapINT(ref mat.rows_alloc, ref mat.columns_alloc);

                /* Finally set current storage mode */
                mat.is_roworder = (bool)!mat.is_roworder;
                mat.row_end_valid = false;
            }
            return (status);
        }

        /* Refactorization and recomputation routine */
        //byte __WINAPI invert(lprec lp, byte shiftbounds, byte final){throw new NotImplementedException();}

        /* Vector compression and expansion routines */
        static bool vec_compress(ref double densevector, int startpos, int endpos, double epsilon, ref double nzvector, ref int nzindex)
        { throw new NotImplementedException(); }
        static byte vec_expand(double[] nzvector, int nzindex, double[] densevector, int startpos, int endpos) { throw new NotImplementedException(); }

        //Changed By: CS Date:28/11/2018
        /* Sparse matrix products */
        internal static bool get_colIndexA(lprec lp, int varset, int colindex, bool append)
        {
            int i;
            int varnr;
            int P1extraDim;
            int vb;
            int ve;
            int n;
            int nrows = lp.rows;
            int nsum = lp.sum;
            bool omitfixed = new bool();
            bool omitnonfixed = new bool();
            double v = new double();

            /* Find what variable range to scan - default is {SCAN_USERVARS} */
            /* First determine the starting position; add from the top, going down */
            P1extraDim = System.Math.Abs(lp.P1extraDim);
            vb = nrows + 1;
            if ((varset & lp_lib.SCAN_ARTIFICIALVARS) != 0)
            {
                vb = nsum - P1extraDim + 1;
            }
            if ((varset & lp_lib.SCAN_USERVARS) != 0)
            {
                vb = nrows + 1;
            }
            if ((varset & lp_lib.SCAN_SLACKVARS) != 0)
            {
                vb = 1;
            }

            /* Then determine the ending position, add from the bottom, going up */
            ve = nsum;
            if ((varset & lp_lib.SCAN_SLACKVARS) != 0)
            {
                ve = nrows;
            }
            if ((varset & lp_lib.SCAN_USERVARS) != 0)
            {
                ve = nsum - P1extraDim;
            }
            if ((varset & lp_lib.SCAN_ARTIFICIALVARS) != 0)
            {
                ve = nsum;
            }

            /* Adjust for partial pricing */
            if ((varset & lp_lib.SCAN_PARTIALBLOCK) != 0)
            {
                commonlib.SETMAX(vb, LpPrice.partial_blockStart(lp, false));
                commonlib.SETMIN(ve, LpPrice.partial_blockEnd(lp, false));
            }

            /* Determine exclusion columns */
            //ORIGINAL LINE: omitfixed = (bool)((varset & OMIT_FIXED) != 0);
            omitfixed = ((bool)((varset & lp_lib.OMIT_FIXED) != 0));

            //ORIGINAL LINE: omitnonfixed = (bool)((varset & OMIT_NONFIXED) != 0);
            omitnonfixed = ((bool)((varset & lp_lib.OMIT_NONFIXED) != 0));
            if (omitfixed != null && omitnonfixed != null)
            {
                return false;
            }

            /* Scan the target colums */
            if (append)
            {
                n = colindex;
            }
            else
            {
                n = 0;
            }
            for (varnr = vb; varnr <= ve; varnr++)
            {

                /* Skip gap in the specified column scan range (possibly user variables) */
                if (varnr > nrows)
                {
                    //ORIGINAL CODE : if ((varnr <= nsum - P1extraDim) && !(varset & lp_lib.SCAN_USERVARS))
                    if ((varnr <= nsum - P1extraDim) && (varset != 0 && lp_lib.SCAN_USERVARS != 0))
                    {
                        continue;
                    }

                    ///#if 1
                    /* Skip empty columns */
                    if ((mat_collength(lp.matA, varnr - nrows) == 0))
                    {
                        continue;
                    }
                    ///#endif
                }

                /* Find if the variable is in the scope - default is {Ø} */
                //NOTED ISSUE
                i = Convert.ToInt32(lp.is_basic[varnr]);
                if ((varset & lp_lib.USE_BASICVARS) > 0 && (i) != 0)
                {
                    ;
                }
                else if ((varset & lp_lib.USE_NONBASICVARS) > 0 && (i == 0))
                {
                    ;
                }
                else
                {
                    continue;
                }

                v = lp.upbo[varnr];
                if ((omitfixed != null && (v == 0)) || (omitnonfixed != null && (v != 0)))
                {
                    continue;
                }

                /* Append to list */
                n++;
                colindex = varnr;
            }
            colindex = n;

            return (true);

        }
        static int prod_Ax(lprec lp, int[] coltarget, double[] input, int[] nzinput, double roundzero, double ofscalar, double[] output, int[] nzoutput, int roundmode) { throw new NotImplementedException(); }
        internal static int prod_xA(lprec lp, ref int coltarget, ref double input, ref int nzinput, double roundzero, double ofscalar, ref double output, ref int nzoutput, int roundmode)
        {
            LpCls objLpCls = new LpCls();
            int colnr;
            int rownr;
            int varnr;
            int ib;
            int ie;
            int vb;
            int ve;
            int nrows = lp.rows;
            bool localset = new bool();
            bool? localnz = false;
            bool includeOF = new bool();
            bool isRC = new bool();
            //ORIGINAL CODE: REALXP vmax = new REALXP(); /* REALXP in C is long double */
            double vmax = new double();

            //ORIGINAL LINE: REALXP v = new REALXP();
            double v = 0;
            int inz;

            //ORIGINAL LINE: int *rowin;
            int rowin;
            int countNZ = 0;
            MATrec mat = lp.matA;

            //ORIGINAL LINE: register REAL *matValue;
            double matValue;

            //ORIGINAL LINE: register int *matRownr;

            int matRownr;

            /* Clean output area (only necessary if we are returning the full vector) */

            //ORIGINAL LINE: isRC = (bool)((roundmode & MAT_ROUNDRC) != 0);
            isRC = ((bool)((roundmode & MAT_ROUNDRC) != 0));
            /*NOT REQUIRED
            if (nzoutput == null)
            {
                if (input == output)
                {
                    //NOT REQUIRED
                    //MEMCLEAR(output + nrows + 1, lp.columns);
                }
                else
                {
                    //NOT REQUIRED
                    //MEMCLEAR(output, lp.sum + 1);
                }
            }
            */

            /* Find what variable range to scan - default is {SCAN_USERVARS} */
            /* Define default column target if none was provided */

            //ORIGINAL LINE: localset = (bool)(coltarget == null);
            localset = ((bool)(coltarget == null));
            if (localset != null)
            {
                int varset = lp_lib.SCAN_SLACKVARS | lp_lib.SCAN_USERVARS | lp_lib.USE_NONBASICVARS | lp_lib.OMIT_FIXED;
                if (isRC != null && objLpCls.is_piv_mode(lp, lp_lib.PRICE_PARTIAL) && !objLpCls.is_piv_mode(lp, lp_lib.PRICE_FORCEFULL))
                {
                    varset |= lp_lib.SCAN_PARTIALBLOCK;
                }

                coltarget = Convert.ToInt32(lp_utils.mempool_obtainVector(lp.workarrays, lp.sum + 1, coltarget));
                if (!get_colIndexA(lp, varset, coltarget, false))
                {
                    string memvector = coltarget.ToString();
                    lp_utils.mempool_releaseVector(lp.workarrays, ref memvector, 0);
                    return (0);
                }
            }
            /*#define UseLocalNZ*/
            //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
            ///#if UseLocalNZ
            //C++ TO JAVA CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'copyFrom' method should be created:
            //ORIGINAL LINE: localnz = (bool)(nzinput == null);
            localnz = (bool)(nzinput == null);
            if (localnz != null)
            {
                //C++ TO JAVA CONVERTER TODO TASK: There is no Java equivalent to 'sizeof':
                nzinput = Convert.ToInt32(lp_utils.mempool_obtainVector(lp.workarrays, nrows + 1, nzinput));
                double nzvector = 0;
                vec_compress(ref input, 0, nrows, lp.matA.epsvalue, ref nzvector, ref nzinput);
            }
            ///#endif
            //C++ TO JAVA CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'copyFrom' method should be created:
            //ORIGINAL LINE: includeOF = (bool)(((nzinput == null) || (nzinput[1] == 0)) && (input[0] != 0) && lp->obj_in_basis);
            //includeOF.copyFrom((bool)(((nzinput == null) || (nzinput[1] == 0)) && (input[0] != 0) && lp.obj_in_basis));
            includeOF = (bool)(((nzinput > 0) || (nzinput == 0)) && (input != 0) && lp.obj_in_basis);   //[1]

            /* Scan the target colums */
            vmax = 0;
            ve = coltarget;
            for (vb = 1; vb <= ve; vb++)
            {

                varnr = coltarget;  //[vb]

                if (varnr <= nrows)
                {
                    v = input;  //[varnr]
                }
                else
                {
                    colnr = varnr - nrows;
                    v = 0;
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    ib = mat.col_end[colnr - 1][0];
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    ie = mat.col_end[colnr][0];
                    if (ib < ie)
                    {

                        /* Do dense input vector version */
                        //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                        ///#if UseLocalNZ
                        if (localnz != null || (nzinput == null))
                        {
                            ///#else
                            if (nzinput == null)
                            {
                                ///#endif
                                /* Do the OF */
                                if (includeOF != null)
                                {
                                    //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                                    ///#if DirectArrayOF
                                    v += input * lp.obj[colnr] * ofscalar;
                                }
                                ///#else
                                v += input * LpCls.get_OF_active(lp, varnr, ofscalar);
                                ///#endif

                                /* Initialize pointers */
                                matRownr = COL_MAT_ROWNR(ib);
                                matValue = COL_MAT_VALUE(ib);

                                /* Do extra loop optimization based on target window overlaps */
                                //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                                ///#if UseLocalNZ
                                if ((ib < ie) && (colnr <= nzinput) && (COL_MAT_ROWNR(ie - 1) >= nzinput) && (matRownr <= nzinput))  //[colnr]
                                {
                                    ///#endif
                                    //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                                    ///#if NoLoopUnroll
                                    /* Then loop over all regular rows */
                                    for (; ib < ie; ib++)
                                    {
                                        v += input * matValue;  //[matRownr]
                                        matValue += matValueStep;
                                        matRownr += matRowColStep;
                                    }
                                }
                                ///#else
                                /* Prepare for simple loop unrolling */
                                if (((ie - ib) % 2) == 1)
                                {
                                    v += input * matValue;  //[matRownr]
                                    ib++;
                                    matValue += matValueStep;
                                    matRownr += matRowColStep;
                                }

                                /* Then loop over remaining pairs of regular rows */
                                while (ib < ie)
                                {
                                    v += input * matValue;  //[matRownr]
                                    v += input * ((matValue + matValueStep));   //[(matRownr + matRowColStep)]
                                    ib += 2;
                                    matValue += 2 * matValueStep;
                                    matRownr += 2 * matRowColStep;
                                }
                                ///#endif
                            }
                            /* Do sparse input vector version */
                            else
                            {

                                /* Do the OF */
                                if (includeOF != null)
                                {
                                    //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                                    ///#if DirectArrayOF
                                    v += input * lp.obj[colnr] * ofscalar;
                                }
                                ///#else
                                v += input * LpCls.get_OF_active(lp, varnr, ofscalar);
                                ///#endif

                                /* Initialize pointers */
                                inz = 1;
                                rowin = nzinput + inz;
                                matRownr = COL_MAT_ROWNR(ib);
                                matValue = COL_MAT_VALUE(ib);
                                ie--;

                                /* Then loop over all non-OF rows */
                                while ((inz <= nzinput) && (ib <= ie))
                                {

                                    /* Try to synchronize at right */
                                    while ((rowin > matRownr) && (ib < ie))
                                    {
                                        ib++;
                                        matValue += matValueStep;
                                        matRownr += matRowColStep;
                                    }
                                    /* Try to synchronize at left */
                                    while ((rowin < matRownr) && (inz < nzinput))
                                    {
                                        inz++;
                                        rowin++;
                                    }
                                    /* Perform dot product operation if there was a match */
                                    if (rowin == matRownr)
                                    {
                                        v += input * matValue;  //[rowin]
                                        /* Step forward at left */
                                        inz++;
                                        rowin++;
                                    }
                                }
                            }
                        }
                        if ((roundmode & lp_matrix.MAT_ROUNDABS) != 0)
                        {
                            lp_types.my_roundzero(v, roundzero);
                        }
                    }

                    /* Special handling of small reduced cost values */
                    if (isRC == null || (lp_types.my_chsign(lp.is_lower[varnr], v) < 0))
                    {
                        commonlib.SETMAX(Convert.ToInt32(vmax), Convert.ToInt32(System.Math.Abs(v)));
                    }
                    vmax *= roundzero;
                    for (ib = 1; ib <= countNZ; ib++)
                    {
                        rownr = nzoutput;   //[ib]
                        if (System.Math.Abs(output) < vmax) //[rownr]
                        {
                            output = 0; //[rownr]
                        }
                        else
                        {
                            ie++;
                            nzoutput = rownr;   //[ie]
                        }
                    }
                    countNZ = ie;
                }
            }

            /* Clean up and return */
            if (localset)
            {
                string memvector = coltarget.ToString();
                lp_utils.mempool_releaseVector(lp.workarrays, ref memvector, 0);
            }
            if (localnz != null)
            {
                string memvector = nzinput.ToString();
                lp_utils.mempool_releaseVector(lp.workarrays, ref memvector, 0);
            }

            if (nzoutput != null)
            {
                nzoutput = countNZ;
            }
            return (countNZ);
        }
        internal static bool prod_xA2(lprec lp, ref int coltarget, ref double prow, double proundzero, ref int nzprow, ref double drow, double droundzero, ref int nzdrow, double ofscalar, int roundmode)

        { throw new NotImplementedException(); }

        /* Equation solution */
        static byte fimprove(lprec lp, double[] pcol, int nzidx, double roundzero) { throw new NotImplementedException(); }

        //Changed By: CS Date:28/11/2018
        internal static void ftran(lprec lp, double[] rhsvector, int nzidx, double roundzero)
        {
#if false
        //  if(is_action(lp->improve, IMPROVE_SOLUTION) && lp->bfp_pivotcount(lp))
        //    fimprove(lp, rhsvector, nzidx, roundzero);
        //  else
#endif
            lp.bfp_ftran_normal(lp, ref rhsvector[0], nzidx);
        }
        static byte bimprove(lprec lp, double[] rhsvector, int nzidx, double roundzero) { throw new NotImplementedException(); }
        static void btran(lprec lp, double rhsvector, int nzidx, double roundzero)
        {
            lp.bfp_btran_normal(lp, ref rhsvector, nzidx);
        }

        /* Combined equation solution and matrix product for simplex operations */
        internal static bool fsolve(lprec lp, int varin, double[] pcol, int[] nzidx, double roundzero, double ofscalar, bool prepareupdate)
        {
            bool ok = true;
            LpCls objLpCls = new LpCls();

            if (varin > 0)
            {
                int Para = 0;
                /// <summary>
                /// ERROR: cannot convert from 'ref double?[]' to 'ref double[]'
                /// pcol is of type double?[]
                /// type required for method is double[]
                /// </summary>  
                LpCls.obtain_column(lp, varin, ref pcol, ref nzidx, ref Para);
            }

            /* Solve, adjusted for objective function scalar */
            pcol[0] *= ofscalar;
            if (prepareupdate)
            {
                lp.bfp_ftran_prepare(lp, ref pcol[0], Convert.ToInt32(nzidx));
            }
            else
            {
                ftran(lp, pcol, Convert.ToInt32(nzidx), roundzero);
            }

            return (ok);
        }
        static byte bsolve(lprec lp, int row_nr, double[] rhsvector, int nzidx, double roundzero, double ofscalar) { throw new NotImplementedException(); }
        internal static void bsolve_xA2(lprec lp, ref int coltarget, int row_nr1, ref double vector1, double roundzero1, ref int nzvector1, int row_nr2, ref double[] vector2, double roundzero2, ref int[] nzvector2, int roundmode)
        {
            double ofscalar = 1.0;
            lp_LUSOL objLpLusol = new lp_LUSOL();
            LpCls objLpCls = new LpCls();

            /* Clear and initialize first vector */
            if (nzvector1 == null)
            {
                //NOT REQUIRED
                //MEMCLEAR(vector1, lp.sum + 1);
            }
            else
            {
                //NOT REQUIRED
                //MEMCLEAR(vector1, lp.rows + 1);
            }
            vector1 = 1;    //[row_nr1]
            /*  workINT[0] = 1;
              workINT[1] = row_nr1; */

            if (vector2 == null)
            {
                //NOTED ISSUE
                int[] lastParameter = null;
                objLpLusol.bfp_btran_normal(lp, ref vector1, ref lastParameter);
                int nzinput = 0;
                prod_xA(lp, ref coltarget, ref vector1, ref nzinput, roundzero1, ofscalar * 0, ref vector1, ref nzvector1, roundmode);
            }
            else
            {

                /* Clear and initialize second vector */
                if (nzvector2 == null)
                {
                    //NOT REQUIRED
                    //MEMCLEAR(vector2, lp.sum + 1);
                }
                else
                {
                    //NOT REQUIRED
                    //MEMCLEAR(vector2, lp.rows + 1);
                }
                if (lp.obj_in_basis || (row_nr2 > 0))
                {
                    vector2[row_nr2] = 1;
                    /*      workINT[2] = 1;
                          workINT[3] = row_nr2; */
                }
                else
                {
                    LpCls.get_basisOF(lp, null, vector2, nzvector2);
                }

                /* A double BTRAN equation solver process is implemented "in-line" below in
                   order to save time and to implement different rounding for the two */
                int bfp_btran_doubleParaNo3 = 0;
                //NOTED ISSUE
                lp.bfp_btran_double(lp, ref vector1, bfp_btran_doubleParaNo3, vector2[0], 0);

                /* Multiply solution vectors with matrix values */
                prod_xA2(lp, ref coltarget, ref vector1, roundzero1, ref nzvector1, ref vector2[0], roundzero2, ref nzvector2[0], ofscalar, roundmode);
            }

        }

        /* Change-tracking routines (primarily for B&B and presolve) */
        internal static DeltaVrec createUndoLadder(lprec lp, int levelitems, int maxlevels)
        {

            DeltaVrec hold;

            hold = new DeltaVrec();
            hold.lp = lp;
            hold.activelevel = 0;
            hold.tracker = mat_create(lp, levelitems, 0, 0.0);
            inc_matcol_space(hold.tracker, maxlevels);
            return (hold);
        }
        internal static int incrementUndoLadder(DeltaVrec DV)
        {
            DV.activelevel++;
            inc_matcol_space(DV.tracker, 1);
            mat_shiftcols(DV.tracker, ref (DV.activelevel), 1, null);
            DV.tracker.columns++;
            return (DV.activelevel);
        }
        internal static bool modifyUndoLadder(DeltaVrec DV, int itemno, double[] target, double newvalue)
        {
            bool status;
            int varindex = itemno;
            double oldvalue = target[itemno];

#if !UseMilpSlacksRCF
            varindex -= DV.lp.rows;
#endif
            status = mat_appendvalue(DV.tracker, varindex, oldvalue);
            target[itemno] = newvalue;
            return (status);
        }

        internal static bool mat_appendvalue(MATrec mat, int Row, double Value)
        {
            //ORIGINAL LINE: int *elmnr, Column = mat->columns;
            int elmnr;
            int Column = mat.columns;

            /* Set small numbers to zero */
            if (System.Math.Abs(Value) < mat.epsvalue)
            {
                Value = 0;
            }
#if DoMatrixRounding
  else
  {
	Value = roundToPrecision(Value, mat.epsvalue);
  }
#endif

            /* Check if more space is needed for matrix */
            if (!inc_mat_space(mat, 1))
            {
                return (false);
            }

#if Paranoia
  /* Check valid indeces */
  if ((Row < 0) || (Row > mat.rows))
  {
	report(mat.lp, SEVERE, "mat_appendvalue: Invalid row index %d specified\n", Row);
	return (0);
  }
#endif

            /* Get insertion point and set value */
            elmnr = mat.col_end[0][0] + Column;
            _SET_MAT_ijA(elmnr, Row, Column, Convert.ToInt32(Value));

            /* Update column count */
            elmnr++;
            mat.row_end_valid = false;

            return (true);
        }

        static int countsUndoLadder(DeltaVrec DV) { throw new NotImplementedException(); }
        internal static int restoreUndoLadder(DeltaVrec DV, double[] target)
        {
            int iD = 0;

            if (DV.activelevel > 0)
            {
                MATrec mat = DV.tracker;
                int iB = Convert.ToInt32(mat.col_end[DV.activelevel - 1]);
                int iE = Convert.ToInt32(mat.col_end[DV.activelevel]);

                int matRownr = COL_MAT_ROWNR(iB);

                double matValue = COL_MAT_VALUE(iB);
                double oldvalue = new double();

                /* Restore the values */
                iD = iE - iB;
                for (; iB < iE; iB++, matValue += matValueStep, matRownr += matRowColStep)
                {
                    //ORIGINAL LINE: oldvalue = *matValue;
                    oldvalue = (matValue);
#if UseMilpSlacksRCF
	  target[(*matRownr)] = oldvalue;
#else
                    target[DV.lp.rows + (matRownr)] = oldvalue;
#endif
                }

                /* Get rid of the changes */
                mat_shiftcols(DV.tracker, ref (DV.activelevel), -1, null);
            }

            return (iD);

        }
        internal static int decrementUndoLadder(DeltaVrec DV)
        {
            int deleted = 0;

            if (DV.activelevel > 0)
            {
                deleted = mat_shiftcols(DV.tracker, ref (DV.activelevel), -1, null);
                DV.activelevel--;
                DV.tracker.columns--;
            }
            return (deleted);
        }
        internal static bool freeUndoLadder(DeltaVrec[] DV)
        {
            throw new NotImplementedException();
            /*NOT REQUIRED
            if ((DV == null) || (DV == null))
                return false;

            mat_free((DV.tracker));
            FREEDV;
            return (TRUE);
            */
        }

        /* Specialized presolve undo functions */
        static byte appendUndoPresolve(lprec lp, byte isprimal, double beta, int colnrDep) { throw new NotImplementedException(); }
        static byte addUndoPresolve(lprec lp, byte isprimal, int colnrElim, double alpha, double beta, int colnrDep) { throw new NotImplementedException(); }

        private static int mat_nz_unused(MATrec mat)
        {
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            return (mat.mat_alloc - mat.col_end[mat.columns][0]);
        }

        //Changed By: CS Date:28/11/2018
        internal static bool bsolve(lprec lp, int row_nr, ref double[] rhsvector, ref int? nzidx, double roundzero, double ofscalar)
        {
            bool ok = true;

            if (row_nr >= 0) // Note that row_nr == 0 returns the [1, 0...0 ] vector
            {
                int LastPara = 0;
                int nzlist = (nzidx != null) ? Convert.ToInt32(nzidx) : 0;
                row_nr = lp.obtain_column(lp, row_nr, ref rhsvector[0], ref nzlist, ref LastPara);
            }

            /* Solve, adjusted for objective function scalar */
            rhsvector[0] = ofscalar;
            btran(lp, rhsvector[0], (nzidx != null) ? Convert.ToInt32(nzidx) : 0, roundzero);

            return (ok);

        } // bsolve

        internal static bool invert(lprec lp, bool shiftbounds, bool final_Renamed)
        {
            bool usedpos;
            bool resetbasis = new bool();
            double test = new double();
            int k;
            int i;
            int j;
            int singularities;
            int usercolB;
            string msg;
            LpCls objLpCls = new LpCls();
            FileStream stdout = null;
            lp_report objlp_report = new lp_report();

            /* Make sure the tags are correct */
            if (!lp_matrix.mat_validate(lp.matA))
            {
                lp.spx_status = lp_lib.INFEASIBLE;
                return false;
            }

            /* Create the inverse management object at the first call to invert() */
            if (lp.invB == null)
            {
                msg = null;
                lp.bfp_init(lp, lp.rows, 0, ref msg);
            }
            else
            {
                lp.bfp_preparefactorization(lp);
            }
            singularities = 0;

            /* Must save spx_status since it is used to carry information about
               the presence and handling of singular columns in the matrix */
            if (LpCls.userabort(lp, lp_lib.MSG_INVERT))
            {
                return (false);
            }

            ///#if Paranoia
            if (lp.spx_trace)
            {
                msg = "invert: Iter %10g, fact-length %7d, OF " + lp_types.RESULTVALUEMASK + ".\n";
                lp.report(lp, lp_lib.DETAILED, ref msg, (double)lp.get_total_iter(lp), lp.bfp_colcount(lp), (double)-lp.rhs[0]);
            }
            ///#endif

            /* Store state of pre-existing basis, and at the same time check if
               the basis is I; in this case take the easy way out */
            //NOT REQUIRED
            /*if (!allocMYBOOL(lp, usedpos, lp.sum + 1, 1))
            {
                lp.bb_break = 1;
                return (0);
            }*/
            usedpos = true;
            usercolB = 0;
            for (i = 1; i <= lp.rows; i++)
            {
                k = lp.var_basic[i];
                if (k > lp.rows)
                {
                    usercolB++;
                }
                usedpos = true;
            }

            ///#if Paranoia
            if (!LpCls.verify_basis(lp))
            {
                msg = "invert: Invalid basis detected (iter %g).\n";
                lp.report(lp, lp_lib.SEVERE, ref msg, (double)lp.get_total_iter(lp));
            }
            ///#endif

            /* Tally matrix nz-counts and check if we should reset basis
               indicators to all slacks */

            //ORIGINAL LINE: resetbasis = (MYBOOL)((usercolB > 0) && lp->bfp_canresetbasis(lp));
            //NOTED ISSUE
            resetbasis = ((bool)((usercolB > 0) && LpBFP2.bfp_canresetbasis(lp)));
            k = 0;
            for (i = 1; i <= lp.rows; i++)
            {
                if (lp.var_basic[i] > lp.rows)
                {
                    k += mat_collength(lp.matA, lp.var_basic[i] - lp.rows) + (LpCls.is_OF_nz(lp, lp.var_basic[i] - lp.rows) ? 1 : 0);
                }
                if (resetbasis != null)
                {
                    j = lp.var_basic[i];
                    if (j > lp.rows)
                    {
                        lp.is_basic[j] = false;
                    }
                    lp.var_basic[i] = i;
                    lp.is_basic[i] = true;
                }
            }

            /* Now do the refactorization */
            //NOTED ISSUE
            singularities = lp_LUSOL.bfp_factorize(lp, usercolB, k, usedpos, final_Renamed);

            /* Do user reporting */
            if (LpCls.userabort(lp, lp_lib.MSG_INVERT))
            {
                goto Cleanup;
            }

            /* Finalize factorization/inversion */
            lp.bfp_finishfactorization(lp);

            /* Recompute the RHS ( Ref. lp_solve inverse logic and Chvatal p. 121 ) */
            ///#if DebugInv
            // blockWriteLREAL(stdout, "RHS-values pre invert", lp.rhs, 0, lp.rows);
            ///#endif
            LpCls.recompute_solution(lp, shiftbounds);
            LpPricePSE.restartPricer(lp, Convert.ToBoolean(DefineConstants.AUTOMATIC));

            ///#if DebugInv
            msg = "RHS-values post invert";
            objlp_report.blockWriteLREAL(stdout, ref msg, lp.rhs[0], 0, lp.rows);
            ///#endif

            Cleanup:
            /* Check for numerical instability indicated by frequent refactorizations */
            test = objLpCls.get_refactfrequency(lp, 0);
            if (test < lp_lib.MIN_REFACTFREQUENCY)
            {
                test = objLpCls.get_refactfrequency(lp, 1);
                msg = "invert: Refactorization frequency %.1g indicates numeric instability.\n";
                objlp_report.report(lp, lp_lib.NORMAL, ref msg, test);
                lp.spx_status = lp_lib.NUMFAILURE;
            }
            /*NOT REQUIRED
            FREE(usedpos);
            */
            return ((bool)(singularities <= 0));
        } // invert

        static int mat_findcolumn(MATrec mat, int matindex) { throw new NotImplementedException(); }
    }
}