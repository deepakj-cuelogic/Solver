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
        public int[] col_tag; // user-definable tag associated with each column

#if MatrixRowAccess == RAM_Index
        //ORIGINAL LINE: int *row_mat;
        public int[] row_mat; /* mat_alloc : From index 0, row_mat contains the row-ordered index of the elements of col_mat */
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
        public int[] row_tag; // user-definable tag associated with each row

        public double[] colmax; // Array of maximum values of each column
        public double[] rowmax; // Array of maximum values of each row

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
        internal static Func<int, int> ROW_MAT_COLNR = (item) => (mat.row_mat[item]);

        /*ORIGINAL CODE: 
        #define SET_MAT_ijA(item,i,j,A)   mat->col_mat_rownr[item] = i; \
                                          mat->col_mat_colnr[item] = j; \
                                          mat->col_mat_value[item] = A
        */
        static Action<int[], int[], int[], int[]> _SET_MAT_ijA = delegate (int[] item, int[] i, int[] j, int[] A)
        {
            /// <summary>
            /// Cannot implicitly convert type 'int[]' to 'int' 
            /// col_mat_rownr is of type int[], item is of type int[], i is of type int[]
            /// </summary>
            mat.col_mat_rownr[item] = i;
            mat.col_mat_colnr[item] = j;
            mat.col_mat_value[item] = A;
        };

        //ORIGINAL CODE: #define CAM_Record                0
        public const int CAM_Record = 1;
        //ORIGINAL CODE: #define CAM_Vector                1
        public const int CAM_Vector = 0;

        //ORIGINAL CODE: #define COL_MAT_VALUE(item)       (mat->col_mat_value[item])
        internal static Func<int, double> COL_MAT_VALUE = (item) => (mat.col_mat_value[item]);




        //ORIGINAL CODE: #define ROW_MAT_VALUE(item)       COL_MAT_VALUE(mat->row_mat[item])
        static Func<int, double> ROW_MAT_VALUE = (item) => COL_MAT_VALUE(mat.row_mat[item]);

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

        static byte mat_memopt(MATrec mat, int rowextra, int colextra, int nzextra)
        {
            throw new NotImplementedException();
        }
        internal static void mat_free(MATrec[] matrix)
        {
            throw new NotImplementedException();
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
        static int mat_mapreplace(MATrec mat, LLrec rowmap, LLrec colmap, MATrec insmat) { throw new NotImplementedException(); }
        static int mat_matinsert(MATrec mat, MATrec insmat) { throw new NotImplementedException(); }
        static int mat_zerocompact(MATrec mat) { throw new NotImplementedException(); }
        static int mat_rowcompact(MATrec mat, byte dozeros) { throw new NotImplementedException(); }
        static int mat_colcompact(MATrec mat, int prev_rows, int prev_cols) { throw new NotImplementedException(); }
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
        static bool inc_mat_space(MATrec mat, int mindelta) { throw new NotImplementedException(); }
        internal static int mat_shiftrows(MATrec mat, ref int bbase, int delta, LLrec varmap) { throw new NotImplementedException(); }
        internal static int mat_shiftcols(MATrec mat, ref int bbase, int delta, LLrec varmap) { throw new NotImplementedException(); }
        static MATrec mat_extractmat(MATrec mat, LLrec rowmap, LLrec colmap, byte negated) { throw new NotImplementedException(); }
        static internal int mat_appendrow(MATrec mat, int count, double?[] row, int?[] colno, double mult, bool checkrowmode)
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
                stcol = mat.col_end[j] - 1;
                mat.col_end[j] = elmnr + 1;
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
            i = stcol - mat.col_end[j - 1] + 1;
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
        internal static int mat_appendcol(MATrec mat, int count, double?[] column, int?[] rowno, double mult, bool? checkrowmode)
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
            elmnr = mat.col_end[mat.columns - 1];
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
                        SET_MAT_ijA(elmnr, row, mat.columns, value);
                        elmnr++;
                    }
                }

                /* Fill dense Lagrangean constraints */
                if (objLpCls.get_Lrows(lp) > 0)
                {
                    ///NOTED ISSUE
                    //double? (column != null) ? Convert.ToDouble(column[0]) : 0 + Convert.ToDouble(mat.rows)
                    double c = (mat.rows != null) ? Convert.ToDouble(mat.rows) : 0;
                    mat_appendcol(lp.matL, objLpCls.get_Lrows(lp), column + c, null, mult, checkrowmode);
                }

            }

            /* Set end of data */
            mat.col_end[mat.columns] = elmnr;

            return (mat.col_end[mat.columns] - mat.col_end[mat.columns - 1]);
        }
        static byte mat_get_data(lprec lp, int matindex, byte isrow, int[] rownr, int[] colnr, double[][] value) { throw new NotImplementedException(); }
        static byte mat_set_rowmap(MATrec mat, int row_mat_index, int rownr, int colnr, int col_mat_index) { throw new NotImplementedException(); }
        static byte mat_indexrange(MATrec mat, int index, byte isrow, int startpos, int endpos) { throw new NotImplementedException(); }
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
                    mat.row_end[rownr]++;
                }
                for (i = 1; i <= mat.rows; i++)
                {
                    mat.row_end[i] += mat.row_end[i - 1];
                }

                /* Calculate the column index for every non-zero */
                for (i = 1; i <= mat.columns; i++)
                {
                    j = mat.col_end[i - 1];
                    je = mat.col_end[i];
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
                            mat_set_rowmap(mat, mat.row_end[rownr - 1] + rownum[rownr], rownr, i, j);
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

            low = mat.col_end[column - 1];
            high = mat.col_end[column] - 1;
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

            ie = mat.col_end[col_nr];
            for (i = mat.col_end[col_nr - 1]; i < ie; i++)
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
                if (objLpCls.get_Lrows(mat.lp) > 0)
                {
                    mat_multcol(mat.lp.matL, col_nr, mult, DoObj);
                }
            }
        }

        static double mat_getitem(MATrec mat, int row, int column) { throw new NotImplementedException(); }
        static byte mat_setitem(MATrec mat, int row, int column, double value) { throw new NotImplementedException(); }
        static byte mat_additem(MATrec mat, int row, int column, double delta) { throw new NotImplementedException(); }
        static byte mat_setvalue(MATrec mat, int Row, int Column, double Value, byte doscale) { throw new NotImplementedException(); }
        internal static int mat_nonzeros(MATrec mat)
        {
            return (mat.col_end[mat.columns]);
        }
        internal static int mat_collength(MATrec mat, int colnr)
        {
            return (mat.col_end[colnr] - mat.col_end[colnr - 1]);
        }
        static int mat_rowlength(MATrec mat, int rownr) { throw new NotImplementedException(); }
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
                    k1 = mat.row_end[row_nr - 1];
                }
                k2 = mat.row_end[row_nr];
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
        static void mat_multadd(MATrec mat, double[] lhsvector, int varnr, double mult) { throw new NotImplementedException(); }
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
                    j = mat.row_end[0];
                    for (i = nz - 1; i >= j; i--)
                    {
                        k = i - j;
                        newmat[k] = mat.col_mat[mat.row_mat[i]];
                        newmat[k].rownr = newmat[k].colnr;
                    }
                    for (i = j - 1; i >= 0; i--)
                    {
                        k = nz - j + i;
                        newmat[k] = mat.col_mat[mat.row_mat[i]];
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
                j = mat.row_end[0];
                for (i = (mat.rows != null) ? Convert.ToInt32(mat.rows) : 0; i >= 1; i--)
                {
                    mat.row_end[i] -= j;
                }
                mat.row_end[(mat.rows != null) ? Convert.ToInt32(mat.rows) : 0] = nz;
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
        static byte vec_compress(double[] densevector, int startpos, int endpos, double epsilon, double[] nzvector, int[] nzindex) { throw new NotImplementedException(); }
        static byte vec_expand(double[] nzvector, int nzindex, double[] densevector, int startpos, int endpos) { throw new NotImplementedException(); }

        /* Sparse matrix products */
        static bool get_colIndexA(lprec lp, int varset, int colindex, bool append)
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
        internal static int prod_xA(lprec lp, int[] coltarget, double[] input, int[] nzinput, double roundzero, double ofscalar, double[] output, int[] nzoutput, int roundmode)
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

                coltarget[0] = Convert.ToInt32(lp_utils.mempool_obtainVector(lp.workarrays, lp.sum + 1, coltarget[0]));
                if (!get_colIndexA(lp, varset, coltarget[0], false))
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
                nzinput[0] = Convert.ToInt32(lp_utils.mempool_obtainVector(lp.workarrays, nrows + 1, nzinput[0]));
                vec_compress(input, 0, nrows, lp.matA.epsvalue, null, nzinput);
            }
            ///#endif
            //C++ TO JAVA CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'copyFrom' method should be created:
            //ORIGINAL LINE: includeOF = (bool)(((nzinput == null) || (nzinput[1] == 0)) && (input[0] != 0) && lp->obj_in_basis);
            //includeOF.copyFrom((bool)(((nzinput == null) || (nzinput[1] == 0)) && (input[0] != 0) && lp.obj_in_basis));
            includeOF = (bool)(((nzinput == null) || (nzinput[1] == 0)) && (input[0] != 0) && lp.obj_in_basis);

            /* Scan the target colums */
            vmax = 0;
            ve = coltarget[0];
            for (vb = 1; vb <= ve; vb++)
            {

                varnr = coltarget[vb];

                if (varnr <= nrows)
                {
                    v = input[varnr];
                }
                else
                {
                    colnr = varnr - nrows;
                    v = 0;
                    ib = mat.col_end[colnr - 1];
                    ie = mat.col_end[colnr];
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
                                    v += input[0] * lp.obj[colnr] * ofscalar;
                                }
                                ///#else
                                v += input[0] * LpCls.get_OF_active(lp, varnr, ofscalar);
                                ///#endif

                                /* Initialize pointers */
                                matRownr = COL_MAT_ROWNR(ib);
                                matValue = COL_MAT_VALUE(ib);

                                /* Do extra loop optimization based on target window overlaps */
                                //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                                ///#if UseLocalNZ
                                if ((ib < ie) && (colnr <= nzinput[0]) && (COL_MAT_ROWNR(ie - 1) >= nzinput[colnr]) && (matRownr <= nzinput[0]))
                                {
                                    ///#endif
                                    //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                                    ///#if NoLoopUnroll
                                    /* Then loop over all regular rows */
                                    for (; ib < ie; ib++)
                                    {
                                        v += input[matRownr] * matValue;
                                        matValue += matValueStep;
                                        matRownr += matRowColStep;
                                    }
                                }
                                ///#else
                                /* Prepare for simple loop unrolling */
                                if (((ie - ib) % 2) == 1)
                                {
                                    v += input[matRownr] * matValue;
                                    ib++;
                                    matValue += matValueStep;
                                    matRownr += matRowColStep;
                                }

                                /* Then loop over remaining pairs of regular rows */
                                while (ib < ie)
                                {
                                    v += input[matRownr] * matValue;
                                    v += input[(matRownr + matRowColStep)] * ((matValue + matValueStep));
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
                                    v += input[0] * lp.obj[colnr] * ofscalar;
                                }
                                ///#else
                                v += input[0] * LpCls.get_OF_active(lp, varnr, ofscalar);
                                ///#endif

                                /* Initialize pointers */
                                inz = 1;
                                rowin = nzinput[0] + inz;
                                matRownr = COL_MAT_ROWNR(ib);
                                matValue = COL_MAT_VALUE(ib);
                                ie--;

                                /* Then loop over all non-OF rows */
                                while ((inz <= nzinput[0]) && (ib <= ie))
                                {

                                    /* Try to synchronize at right */
                                    while ((rowin > matRownr) && (ib < ie))
                                    {
                                        ib++;
                                        matValue += matValueStep;
                                        matRownr += matRowColStep;
                                    }
                                    /* Try to synchronize at left */
                                    while ((rowin < matRownr) && (inz < nzinput[0]))
                                    {
                                        inz++;
                                        rowin++;
                                    }
                                    /* Perform dot product operation if there was a match */
                                    if (rowin == matRownr)
                                    {
                                        v += input[rowin] * matValue;
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
                        rownr = nzoutput[ib];
                        if (System.Math.Abs(output[rownr]) < vmax)
                        {
                            output[rownr] = 0;
                        }
                        else
                        {
                            ie++;
                            nzoutput[ie] = rownr;
                        }
                    }
                    countNZ = ie;
                }
            }

            /* Clean up and return */
            if (localset)
            {
                string memvector = coltarget[0].ToString();
                lp_utils.mempool_releaseVector(lp.workarrays, ref memvector, 0);
            }
            if (localnz != null)
            {
                string memvector = nzinput[0].ToString();
                lp_utils.mempool_releaseVector(lp.workarrays, ref memvector, 0);
            }

            if (nzoutput != null)
            {
                nzoutput[0] = countNZ;
            }
            return (countNZ);
        }
        static byte prod_xA2(lprec lp, int[] coltarget, double[] prow, double proundzero, int[] pnzprow,
                                                          double[] drow, double droundzero, int[] dnzdrow, double ofscalar, int roundmode)
        { throw new NotImplementedException(); }

        /* Equation solution */
        static byte fimprove(lprec lp, double[] pcol, int nzidx, double roundzero) { throw new NotImplementedException(); }
        static void ftran(lprec lp, double[] rhsvector, int nzidx, double roundzero) { throw new NotImplementedException(); }
        static byte bimprove(lprec lp, double[] rhsvector, int nzidx, double roundzero) { throw new NotImplementedException(); }
        static void btran(lprec lp, double rhsvector, int nzidx, double roundzero)
        {
            lp.bfp_btran_normal(lp, ref rhsvector, nzidx);
        }

        /* Combined equation solution and matrix product for simplex operations */
        internal static bool fsolve(lprec lp, int varin, double?[] pcol, int[] nzidx, double roundzero, double ofscalar, bool prepareupdate)
        {
            bool ok = true;
            LpCls objLpCls = new LpCls();

            if (varin > 0)
            {
                int Para = 0;
                objLpCls.obtain_column(lp, varin, ref pcol, ref nzidx, ref Para);
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
        internal static void bsolve_xA2(lprec lp, int[] coltarget,
                                          int row_nr1, ref double[] vector1, double roundzero1, int[] nzvector1,
                                          int row_nr2, double[] vector2, double roundzero2, int[] nzvector2, int roundmode)
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
            vector1[row_nr1] = 1;
            /*  workINT[0] = 1;
              workINT[1] = row_nr1; */

            if (vector2 == null)
            {
                //NOTED ISSUE
                int[] lastParameter = null;
                objLpLusol.bfp_btran_normal(lp, ref vector1, ref lastParameter);
                prod_xA(lp, coltarget, vector1, null, roundzero1, ofscalar * 0, vector1, nzvector1, roundmode);
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
                    objLpCls.get_basisOF(lp, null, vector2, nzvector2);
                }

                /* A double BTRAN equation solver process is implemented "in-line" below in
                   order to save time and to implement different rounding for the two */
                int bfp_btran_doubleParaNo3 = 0;
                //NOTED ISSUE
                lp.bfp_btran_double(lp, ref vector1[0], bfp_btran_doubleParaNo3, vector2[0], 0);

                /* Multiply solution vectors with matrix values */
                prod_xA2(lp, coltarget, vector1, roundzero1, nzvector1, vector2, roundzero2, nzvector2, ofscalar, roundmode);
            }

        }

        /* Change-tracking routines (primarily for B&B and presolve) */
        static DeltaVrec createUndoLadder(lprec lp, int levelitems, int maxlevels) { throw new NotImplementedException(); }
        static int incrementUndoLadder(DeltaVrec DV) { throw new NotImplementedException(); }
        static byte modifyUndoLadder(DeltaVrec DV, int itemno, double[] target, double newvalue) { throw new NotImplementedException(); }
        static int countsUndoLadder(DeltaVrec DV) { throw new NotImplementedException(); }
        static int restoreUndoLadder(DeltaVrec DV, double[] target) { throw new NotImplementedException(); }
        static int decrementUndoLadder(DeltaVrec DV) { throw new NotImplementedException(); }
        static bool freeUndoLadder(DeltaVrec[] DV)
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
            return (mat.mat_alloc - mat.col_end[mat.columns]);
        }

        internal static bool bsolve(lprec lp, int row_nr, ref double rhsvector, ref int? nzidx, double roundzero, double ofscalar)
        {
            bool ok = true;

            if (row_nr >= 0) // Note that row_nr == 0 returns the [1, 0...0 ] vector
            {
                int LastPara = 0;
                int nzlist = (nzidx != null) ? Convert.ToInt32(nzidx) : 0;
                row_nr = lp.obtain_column(lp, row_nr, ref rhsvector, ref nzlist, ref LastPara);
            }

            /* Solve, adjusted for objective function scalar */
            rhsvector = ofscalar;
            btran(lp, rhsvector, (nzidx != null) ? Convert.ToInt32(nzidx) : 0, roundzero);

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
            if (objLpCls.userabort(lp, lp_lib.MSG_INVERT))
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
            resetbasis = ((bool)((usercolB > 0) && lp.bfp_canresetbasis(lp)));
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
            singularities = lp.bfp_factorize(lp, usercolB, k, ref usedpos, final_Renamed);

            /* Do user reporting */
            if (objLpCls.userabort(lp, lp_lib.MSG_INVERT))
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
            LpPricePSE.restartPricer(lp, DefineConstants.AUTOMATIC);

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

    }
}
