using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    /// <summary>
    /// changed to static class on 8/11/18 to keep a single instance for LpMsp.cs
    /// </summary>
    public static class myblas
    {
        public const int BLAS_BASE = 1;
        public const int UseMacroVector = 0;

        public static bool mustinitBLAS = true;

        public static void BLAS_dscal_func(ref int n, ref double da, ref double dx, ref int incx)
        { throw new NotImplementedException(); }
        public static void BLAS_dcopy_func(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public static void BLAS_daxpy_func(ref int n, ref double da, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public static void BLAS_dswap_func(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public static double BLAS_ddot_func(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public static int BLAS_idamax_func(ref int n, ref double x, ref int @is)
        { throw new NotImplementedException(); }
        public static int BLAS_idamin_func(ref int n, ref double x, ref int @is)
        { throw new NotImplementedException(); }
        public static void BLAS_dload_func(ref int n, ref double da, ref double dx, ref int incx)
        { throw new NotImplementedException(); }
        public static double BLAS_dnormi_func(ref int n, ref double x)
        { throw new NotImplementedException(); }



        public static void init_BLAS()
        {
            if (myblas.mustinitBLAS)
            {
                string libname = "";
                myblas.load_BLAS(ref libname);
                myblas.mustinitBLAS = false;
            }
        }
        /// <summary>
        /// changed from byte to bool on 8/11/18 as expected return type is bool in LpMps method
        /// </summary>
        public static bool is_nativeBLAS()
        { throw new NotImplementedException(); }
        /// <summary>
        /// changed from byte to bool on 8/11/18 as expected return type is bool in LpMps method
        /// </summary>
        public static bool load_BLAS(ref string libname)
        { throw new NotImplementedException(); }
        public static byte unload_BLAS()
        { throw new NotImplementedException(); }

        /* ************************************************************************ */
        /* User-callable BLAS definitions (C base 1)                                */
        /* ************************************************************************ */
        public static void dscal(int n, double da, ref double dx, int incx)
        { throw new NotImplementedException(); }
        public static void dcopy(int n, ref double dx, int incx, ref double dy, int incy)
        { throw new NotImplementedException(); }
        public static void daxpy(int n, double da, ref double dx, int incx, ref double dy, int incy)
        { throw new NotImplementedException(); }
        public static void dswap(int n, ref double dx, int incx, ref double dy, int incy)
        { throw new NotImplementedException(); }
        public static double ddot(int n, ref double dx, int incx, ref double dy, int incy)
        { throw new NotImplementedException(); }
        public static int idamax(int n, ref double[] x, int @is)
        { throw new NotImplementedException(); }
        public static int idamin(int n, ref double x, int @is)
        { throw new NotImplementedException(); }
        public static void dload(int n, double da, ref double dx, int incx)
        { throw new NotImplementedException(); }
        public static double dnormi(int n, ref double x)
        { throw new NotImplementedException(); }


        /* ************************************************************************ */
        /* Locally implemented BLAS functions (C base 0)                            */
        /* ************************************************************************ */
        public static void my_dscal(ref int n, ref double da, ref double dx, ref int incx)
        { throw new NotImplementedException(); }
        public static void my_dcopy(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public static void my_daxpy(ref int n, ref double da, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public static void my_dswap(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public static double my_ddot(ref int n, ref double dx, ref int incx, ref double dy, ref int incy)
        { throw new NotImplementedException(); }
        public static int my_idamax(ref int n, ref double x, ref int @is)
        { throw new NotImplementedException(); }
        public static int my_idamin(ref int n, ref double x, ref int @is)
        { throw new NotImplementedException(); }
        public static void my_dload(ref int n, ref double da, ref double dx, ref int incx)
        { throw new NotImplementedException(); }
        public static double my_dnormi(ref int n, ref double x)
        { throw new NotImplementedException(); }


        public static int submat(int nrowb, int row, int col)
        { throw new NotImplementedException(); }
        public static int posmat(int nrowb, int row, int col)
        { throw new NotImplementedException(); }

        /* ************************************************************************ */
        /* Subvector and submatrix access routines (Fortran compatibility)          */
        /* ************************************************************************ */
#if UseMacroVector
    /// Not able to convert
    ///ORIGINAL LINE: #define subvec(item) (item - 1)
#else
        public static int subvec(int item)
        { throw new NotImplementedException(); }
#endif

        /* ************************************************************************ */
        /* Randomization functions                                                  */
        /* ************************************************************************ */
        public static void randomseed(ref int seeds)
        { throw new NotImplementedException(); }
        public static void randomdens(int n, ref double x, double r1, double r2, double densty, ref int seeds)
        { throw new NotImplementedException(); }
        public static void ddrand(int n, ref double x, int incx, ref int seeds)
        { throw new NotImplementedException(); }

        
    }
}
