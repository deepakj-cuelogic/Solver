using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public class myblas
    {
        public const int BLAS_BASE = 1;
        public const int UseMacroVector = 0;


        public void BLAS_dscal_func(ref int n, ref double da, ref double dx, ref int incx)
        { throw new NotImplementedException(); }
        public void BLAS_dcopy_func(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public void BLAS_daxpy_func(ref int n, ref double da, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public void BLAS_dswap_func(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public double BLAS_ddot_func(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public int BLAS_idamax_func(ref int n, ref double x, ref int @is)
        { throw new NotImplementedException(); }
        public int BLAS_idamin_func(ref int n, ref double x, ref int @is)
        { throw new NotImplementedException(); }
        public void BLAS_dload_func(ref int n, ref double da, ref double dx, ref int incx)
        { throw new NotImplementedException(); }
        public double BLAS_dnormi_func(ref int n, ref double x)
        { throw new NotImplementedException(); }



        public void init_BLAS()
        { throw new NotImplementedException(); }
        public byte is_nativeBLAS()
        { throw new NotImplementedException(); }
        public byte load_BLAS(ref string libname)
        { throw new NotImplementedException(); }
        public byte unload_BLAS()
        { throw new NotImplementedException(); }

        /* ************************************************************************ */
        /* User-callable BLAS definitions (C base 1)                                */
        /* ************************************************************************ */
        public void dscal(int n, double da, ref double dx, int incx)
        { throw new NotImplementedException(); }
        public void dcopy(int n, ref double dx, int incx, ref double dy, int incy)
        { throw new NotImplementedException(); }
        public void daxpy(int n, double da, ref double dx, int incx, ref double dy, int incy)
        { throw new NotImplementedException(); }
        public void dswap(int n, ref double dx, int incx, ref double dy, int incy)
        { throw new NotImplementedException(); }
        public double ddot(int n, ref double dx, int incx, ref double dy, int incy)
        { throw new NotImplementedException(); }
        public int idamax(int n, ref double x, int @is)
        { throw new NotImplementedException(); }
        public int idamin(int n, ref double x, int @is)
        { throw new NotImplementedException(); }
        public void dload(int n, double da, ref double dx, int incx)
        { throw new NotImplementedException(); }
        public double dnormi(int n, ref double x)
        { throw new NotImplementedException(); }


        /* ************************************************************************ */
        /* Locally implemented BLAS functions (C base 0)                            */
        /* ************************************************************************ */
        public void my_dscal(ref int n, ref double da, ref double dx, ref int incx)
        { throw new NotImplementedException(); }
        public void my_dcopy(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public void my_daxpy(ref int n, ref double da, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public void my_dswap(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public double my_ddot(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public int my_idamax(ref int n, ref double x, ref int @is)
        { throw new NotImplementedException(); }
        public int my_idamin(ref int n, ref double x, ref int @is)
        { throw new NotImplementedException(); }
        public void my_dload(ref int n, ref double da, ref double dx, ref int incx)
        { throw new NotImplementedException(); }
        public double my_dnormi(ref int n, ref double x)
        { throw new NotImplementedException(); }


        public int submat(int nrowb, int row, int col)
        { throw new NotImplementedException(); }
        public int posmat(int nrowb, int row, int col)
        { throw new NotImplementedException(); }

        /* ************************************************************************ */
        /* Subvector and submatrix access routines (Fortran compatibility)          */
        /* ************************************************************************ */
#if UseMacroVector
    /// Not able to convert
    ///ORIGINAL LINE: #define subvec(item) (item - 1)
#else
        public int subvec(int item)
        { throw new NotImplementedException(); }
#endif

        /* ************************************************************************ */
        /* Randomization functions                                                  */
        /* ************************************************************************ */
        public void randomseed(ref int seeds)
        { throw new NotImplementedException(); }
        public void randomdens(int n, ref double x, double r1, double r2, double densty, ref int seeds)
        { throw new NotImplementedException(); }
        public void ddrand(int n, ref double x, int incx, ref int seeds)
        { throw new NotImplementedException(); }


    }
}
