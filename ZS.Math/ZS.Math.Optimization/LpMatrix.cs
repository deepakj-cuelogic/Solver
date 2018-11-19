﻿using System;
using System.Collections.Generic;
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
        public int rows;
        public int columns;

        /* Allocated memory */
        public int rows_alloc;
        public int columns_alloc;
        public int mat_alloc; // The allocated size for matrix sized structures

        /* Sparse problem matrix storage */
#if MatrixColAccess == CAM_Record
        public MATitem col_mat; // mat_alloc : The sparse data storage

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
        public int[] col_end; /* columns_alloc+1 : col_end[i] is the index of the first element after column i; column[i] is stored in elements col_end[i-1] to col_end[i]-1 */
        //ORIGINAL LINE: int *col_tag;
        public int[] col_tag; // user-definable tag associated with each column
        
#if MatrixRowAccess == RAM_Index
        //ORIGINAL LINE: int *row_mat;
        public int[] row_mat; /* mat_alloc : From index 0, row_mat contains the row-ordered index of the elements of col_mat */
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
        public int[] row_end; /* rows_alloc+1 : row_end[i] is the index of the first element in row_mat after row i */
        //ORIGINAL LINE: int *row_tag;
        public int[] row_tag; // user-definable tag associated with each row

        public double colmax; // Array of maximum values of each column
        public double rowmax; // Array of maximum values of each row

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

        //ORIGINAL CODE: #define COL_MAT_ROWNR(item)       (mat->col_mat_rownr[item])
        internal static Func<int, int> COL_MAT_ROWNR = (item) => (mat.col_mat_rownr[item]);

        //ORIGINAL CODE: #define ROW_MAT_COLNR(item)       COL_MAT_COLNR(mat->row_mat[item])
        internal static Func<int, int> ROW_MAT_COLNR = (item) => (mat.row_mat[item]);

        /*ORIGINAL CODE: 
        #define SET_MAT_ijA(item,i,j,A)   mat->col_mat_rownr[item] = i; \
                                          mat->col_mat_colnr[item] = j; \
                                          mat->col_mat_value[item] = A
        */
        /*static Action<int[], int[], int[], int[]> _SET_MAT_ijA = delegate (int[] item, int[] i, int[] j, int[] A)
        {
            /// <summary>
            /// Cannot implicitly convert type 'int[]' to 'int' 
            /// col_mat_rownr is of type int[], item is of type int[], i is of type int[]
            /// </summary>
            mat.col_mat_rownr[item] = i;
            mat.col_mat_colnr[item] = j;
            mat.col_mat_value[item] = A;
        };*/

        //ORIGINAL CODE: #define CAM_Record                0
        public const int CAM_Record = 1;
        //ORIGINAL CODE: #define CAM_Vector                1
        public const int CAM_Vector = 0;

        //ORIGINAL CODE: #define COL_MAT_VALUE(item)       (mat->col_mat_value[item])
        internal static Func<int, double> COL_MAT_VALUE = (item) => (mat.col_mat_value[item]);

        


        //ORIGINAL CODE: #define ROW_MAT_VALUE(item)       COL_MAT_VALUE(mat->row_mat[item])
        static Func<int, double> ROW_MAT_VALUE = (item) => COL_MAT_VALUE(mat.row_mat[item]);

        //ORIGINAL CODE: #define COL_MAT_COLNR(item)       (mat->col_mat_colnr[item])
        internal static Func<int, double> COL_MAT_COLNR = (item) => (mat.col_mat_colnr[item]);

        /*static Action<int[]> ROW_MAT_VALUE = delegate (int[] item)
        {
            mat.row_mat[item];
        };*/

        internal static MATrec mat_create(lprec lp, int rows, int columns, double epsvalue)
        {
            throw new NotImplementedException();
        }
        static byte mat_memopt(MATrec mat, int rowextra, int colextra, int nzextra)
        {
            throw new NotImplementedException();
        }
        static void mat_free(MATrec[] matrix)
        {
            throw new NotImplementedException();
        }
        static byte inc_matrow_space(MATrec mat, int deltarows)
        {
            throw new NotImplementedException();
        }
        static int mat_mapreplace(MATrec mat, LLrec rowmap, LLrec colmap, MATrec insmat) { throw new NotImplementedException(); }
        static int mat_matinsert(MATrec mat, MATrec insmat) { throw new NotImplementedException(); }
        static int mat_zerocompact(MATrec mat) { throw new NotImplementedException(); }
        static int mat_rowcompact(MATrec mat, byte dozeros) { throw new NotImplementedException(); }
        static int mat_colcompact(MATrec mat, int prev_rows, int prev_cols) { throw new NotImplementedException(); }
        static byte inc_matcol_space(MATrec mat, int deltacols) { throw new NotImplementedException(); }
        static bool inc_mat_space(MATrec mat, int mindelta) { throw new NotImplementedException(); }
        static int mat_shiftrows(MATrec mat, int[] bbase, int delta, LLrec varmap) { throw new NotImplementedException(); }
        static int mat_shiftcols(MATrec mat, int[] bbase, int delta, LLrec varmap) { throw new NotImplementedException(); }
        static MATrec mat_extractmat(MATrec mat, LLrec rowmap, LLrec colmap, byte negated) { throw new NotImplementedException(); }
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
                return (mat_appendcol(mat, count, row, colno, mult, false));
            }

            /* Do initialization and validation */
            //ORIGINAL LINE: isA = (MYBOOL)(mat == lp->matA);
            isA = ((mat == lp.matA));
            //ORIGINAL LINE: isNZ = (MYBOOL)(colno != null);
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
                    value = row[0];
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
                    value = saved = row[0];
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
                     * if (!allocMYBOOL(lp, addto, mat.columns + 1, 1))
                    {
                        return (newnr);
                    }*/
                    for (i = mat.columns; i >= 1; i--)
                    {
                        if (System.Math.Abs(row[i]) > mat.epsvalue)
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
                    value = row[newnr];
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
                    value = row[j];
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
                    value = lp_scale.scaled_mat(lp, value, mat.rows, j);
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
        
        private static int mat_appendcol(MATrec mat, int count, double[] column, int rowno, double mult, bool checkrowmode)
        {
            throw new NotImplementedException();
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
               objlpReport.report(mat.lp, lp_lib.IMPORTANT, ref msg , column);
                return (-1);
            }
            if ((row < 0) || (row > mat.rows))
            {
                string msg = "mat_findelm: Row {0} out of range\n";
                objlpReport.report(mat.lp, lp_lib.IMPORTANT, ref msg , row);
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
                //WORKING: mat.col_mat_value[i] *= mult;
                COL_MAT_VALUE(i) *= mult;
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
        static int mat_collength(MATrec mat, int colnr) { throw new NotImplementedException(); }
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
                    ROW_MAT_VALUE(i) *= mult;
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
        static byte mat_transpose(MATrec mat) { throw new NotImplementedException(); }

        /* Refactorization and recomputation routine */
        //byte __WINAPI invert(lprec lp, byte shiftbounds, byte final){throw new NotImplementedException();}

        /* Vector compression and expansion routines */
        static byte vec_compress(double[] densevector, int startpos, int endpos, double epsilon, double[] nzvector, int[] nzindex) { throw new NotImplementedException(); }
        static byte vec_expand(double[] nzvector, int nzindex, double[] densevector, int startpos, int endpos) { throw new NotImplementedException(); }

        /* Sparse matrix products */
        static byte get_colIndexA(lprec lp, int varset, int colindex, byte append) { throw new NotImplementedException(); }
        static int prod_Ax(lprec lp, int[] coltarget, double[] input, int[] nzinput, double roundzero, double ofscalar, double[] output, int[] nzoutput, int roundmode) { throw new NotImplementedException(); }
        static int prod_xA(lprec lp, int[] coltarget, double[] input, int[] nzinput, double roundzero, double ofscalar, double[] output, int[] nzoutput, int roundmode) { throw new NotImplementedException(); }
        static byte prod_xA2(lprec lp, int[] coltarget, double[] prow, double proundzero, int[] pnzprow,
                                                          double[] drow, double droundzero, int[] dnzdrow, double ofscalar, int roundmode)
        { throw new NotImplementedException(); }

        /* Equation solution */
        static byte fimprove(lprec lp, double[] pcol, int nzidx, double roundzero) { throw new NotImplementedException(); }
        static void ftran(lprec lp, double[] rhsvector, int nzidx, double roundzero) { throw new NotImplementedException(); }
        static byte bimprove(lprec lp, double[] rhsvector, int nzidx, double roundzero) { throw new NotImplementedException(); }
        static void btran(lprec lp, double[] rhsvector, int nzidx, double roundzero) { throw new NotImplementedException(); }

        /* Combined equation solution and matrix product for simplex operations */
        static byte fsolve(lprec lp, int varin, double[] pcol, int nzidx, double roundzero, double ofscalar, byte prepareupdate) { throw new NotImplementedException(); }
        static byte bsolve(lprec lp, int row_nr, double[] rhsvector, int nzidx, double roundzero, double ofscalar) { throw new NotImplementedException(); }
        static void bsolve_xA2(lprec lp, int coltarget,
                                          int row_nr1, double[] vector1, double roundzero1, int[] nzvector1,
                                          int row_nr2, double[] vector2, double roundzero2, int[] nzvector2, int roundmode)
        { throw new NotImplementedException(); }

        /* Change-tracking routines (primarily for B&B and presolve) */
        static DeltaVrec createUndoLadder(lprec lp, int levelitems, int maxlevels) { throw new NotImplementedException(); }
        static int incrementUndoLadder(DeltaVrec DV) { throw new NotImplementedException(); }
        static byte modifyUndoLadder(DeltaVrec DV, int itemno, double[] target, double newvalue) { throw new NotImplementedException(); }
        static int countsUndoLadder(DeltaVrec DV) { throw new NotImplementedException(); }
        static int restoreUndoLadder(DeltaVrec DV, double[] target) { throw new NotImplementedException(); }
        static int decrementUndoLadder(DeltaVrec DV) { throw new NotImplementedException(); }
        static byte freeUndoLadder(DeltaVrec[] DV) { throw new NotImplementedException(); }

        /* Specialized presolve undo functions */
        static byte appendUndoPresolve(lprec lp, byte isprimal, double beta, int colnrDep) { throw new NotImplementedException(); }
        static byte addUndoPresolve(lprec lp, byte isprimal, int colnrElim, double alpha, double beta, int colnrDep) { throw new NotImplementedException(); }


    }
}
