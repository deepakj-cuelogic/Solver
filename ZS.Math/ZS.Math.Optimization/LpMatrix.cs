using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZS.Math.Optimization
{
    public class MATrec
    {
        public const int CAM_Record = 0;
        public const int CAM_Vector = 1;
        public const int CAM = 0;

        #if CAM == CAM_Record
            public const int  MatrixColAccess = CAM_Record;
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
        public byte row_end_valid; // TRUE if row_end & row_mat are valid
        public byte is_roworder; // TRUE if the current (temporary) matrix order is row-wise
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
        static MATrec mat_create(lprec lp, int rows, int columns, double epsvalue)
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
        static byte inc_matrow_space(MATrec mat, int deltarows) {
            throw new NotImplementedException();
        }
        static int mat_mapreplace(MATrec mat, LLrec rowmap, LLrec colmap, MATrec insmat){throw new NotImplementedException();}
        static int mat_matinsert(MATrec mat, MATrec insmat){throw new NotImplementedException();}
        static int mat_zerocompact(MATrec mat){throw new NotImplementedException();}
        static int mat_rowcompact(MATrec mat, byte dozeros){throw new NotImplementedException();}
        static int mat_colcompact(MATrec mat, int prev_rows, int prev_cols){throw new NotImplementedException();}
        static byte inc_matcol_space(MATrec mat, int deltacols){throw new NotImplementedException();}
        static byte inc_mat_space(MATrec mat, int mindelta){throw new NotImplementedException();}
        static int mat_shiftrows(MATrec mat, int[] bbase, int delta, LLrec varmap){throw new NotImplementedException();}
        static int mat_shiftcols(MATrec mat, int[] bbase, int delta, LLrec varmap){throw new NotImplementedException();}
        static MATrec mat_extractmat(MATrec mat, LLrec rowmap, LLrec colmap, byte negated){throw new NotImplementedException();}
        static int mat_appendrow(MATrec mat, int count, double[] row, int colno, double mult, byte checkrowmode){throw new NotImplementedException();}
        static int mat_appendcol(MATrec mat, int count, double[] column, int rowno, double mult, byte checkrowmode){throw new NotImplementedException();}
        static byte mat_get_data(lprec lp, int matindex, byte isrow, int[] rownr, int[] colnr, double[][] value){throw new NotImplementedException();}
        static byte mat_set_rowmap(MATrec mat, int row_mat_index, int rownr, int colnr, int col_mat_index){throw new NotImplementedException();}
        static byte mat_indexrange(MATrec mat, int index, byte isrow, int startpos, int endpos){throw new NotImplementedException();}
        static byte mat_validate(MATrec mat){throw new NotImplementedException();}
        static byte mat_equalRows(MATrec mat, int baserow, int comprow){throw new NotImplementedException();}
        static int mat_findelm(MATrec mat, int row, int column){throw new NotImplementedException();}
        static int mat_findins(MATrec mat, int row, int column, int insertpos, byte validate){throw new NotImplementedException();}
        static void mat_multcol(MATrec mat, int col_nr, double mult, byte DoObj){throw new NotImplementedException();}
        static double mat_getitem(MATrec mat, int row, int column){throw new NotImplementedException();}
        static byte mat_setitem(MATrec mat, int row, int column, double value){throw new NotImplementedException();}
        static byte mat_additem(MATrec mat, int row, int column, double delta){throw new NotImplementedException();}
        static byte mat_setvalue(MATrec mat, int Row, int Column, double Value, byte doscale){throw new NotImplementedException();}
        static int mat_nonzeros(MATrec mat){throw new NotImplementedException();}
        static int mat_collength(MATrec mat, int colnr){throw new NotImplementedException();}
        static int mat_rowlength(MATrec mat, int rownr){throw new NotImplementedException();}
        static void mat_multrow(MATrec mat, int row_nr, double mult){throw new NotImplementedException();}
        static void mat_multadd(MATrec mat, double[] lhsvector, int varnr, double mult){throw new NotImplementedException();}
        static byte mat_setrow(MATrec mat, int rowno, int count, double[] row, int colno, byte doscale, byte checkrowmode){throw new NotImplementedException();}
        static byte mat_setcol(MATrec mat, int colno, int count, double[] column, int rowno, byte doscale, byte checkrowmode){throw new NotImplementedException();}
        static byte mat_mergemat(MATrec target, MATrec source, byte usecolmap){throw new NotImplementedException();}
        static int mat_checkcounts(MATrec mat, int rownum, int colnum, byte freeonexit){throw new NotImplementedException();}
        static int mat_expandcolumn(MATrec mat, int colnr, double[] column, int[] nzlist, byte signedA){throw new NotImplementedException();}
        static byte mat_computemax(MATrec mat){throw new NotImplementedException();}
        static byte mat_transpose(MATrec mat){throw new NotImplementedException();}

        /* Refactorization and recomputation routine */
        //byte __WINAPI invert(lprec lp, byte shiftbounds, byte final){throw new NotImplementedException();}

        /* Vector compression and expansion routines */
        static byte vec_compress(double[] densevector, int startpos, int endpos, double epsilon, double[] nzvector, int[] nzindex){throw new NotImplementedException();}
        static byte vec_expand(double[] nzvector, int nzindex, double[] densevector, int startpos, int endpos){throw new NotImplementedException();}

        /* Sparse matrix products */
        static byte get_colIndexA(lprec lp, int varset, int colindex, byte append){throw new NotImplementedException();}
        static int prod_Ax(lprec lp, int[] coltarget, double[] input, int[] nzinput, double roundzero, double ofscalar, double[] output, int[] nzoutput, int roundmode){throw new NotImplementedException();}
        static int prod_xA(lprec lp, int[] coltarget, double[] input, int[] nzinput, double roundzero, double ofscalar, double[] output, int[] nzoutput, int roundmode){throw new NotImplementedException();}
        static byte prod_xA2(lprec lp, int[] coltarget, double[] prow, double proundzero, int[] pnzprow,
                                                          double[] drow, double droundzero, int[] dnzdrow, double ofscalar, int roundmode){throw new NotImplementedException();}

        /* Equation solution */
        static byte fimprove(lprec lp, double[] pcol, int nzidx, double roundzero){throw new NotImplementedException();}
        static void ftran(lprec lp, double[] rhsvector, int nzidx, double roundzero){throw new NotImplementedException();}
        static byte bimprove(lprec lp, double[] rhsvector, int nzidx, double roundzero){throw new NotImplementedException();}
        static void btran(lprec lp, double[] rhsvector, int nzidx, double roundzero){throw new NotImplementedException();}

        /* Combined equation solution and matrix product for simplex operations */
        static byte fsolve(lprec lp, int varin, double[] pcol, int nzidx, double roundzero, double ofscalar, byte prepareupdate){throw new NotImplementedException();}
        static byte bsolve(lprec lp, int row_nr, double[] rhsvector, int nzidx, double roundzero, double ofscalar){throw new NotImplementedException();}
        static void bsolve_xA2(lprec lp, int coltarget,
                                          int row_nr1, double[] vector1, double roundzero1, int[] nzvector1,
                                          int row_nr2, double[] vector2, double roundzero2, int[] nzvector2, int roundmode){throw new NotImplementedException();}

        /* Change-tracking routines (primarily for B&B and presolve) */
        static DeltaVrec createUndoLadder(lprec lp, int levelitems, int maxlevels){throw new NotImplementedException();}
        static int incrementUndoLadder(DeltaVrec DV){throw new NotImplementedException();}
        static byte modifyUndoLadder(DeltaVrec DV, int itemno, double[] target, double newvalue){throw new NotImplementedException();}
        static int countsUndoLadder(DeltaVrec DV){throw new NotImplementedException();}
        static int restoreUndoLadder(DeltaVrec DV, double[] target){throw new NotImplementedException();}
        static int decrementUndoLadder(DeltaVrec DV){throw new NotImplementedException();}
        static byte freeUndoLadder(DeltaVrec[] DV){throw new NotImplementedException();}

        /* Specialized presolve undo functions */
        static byte appendUndoPresolve(lprec lp, byte isprimal, double beta, int colnrDep){throw new NotImplementedException();}
        static byte addUndoPresolve(lprec lp, byte isprimal, int colnrElim, double alpha, double beta, int colnrDep){throw new NotImplementedException();}


    }

}
