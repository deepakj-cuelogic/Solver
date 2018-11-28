using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class MyBlasLib
    {
        public delegate void BLAS_dscal_func(ref int n, ref double da, ref double dx, ref int incx);
        public delegate void BLAS_dcopy_func(ref int n, ref double dx, ref int incx, ref double dy, ref int incy);
        public delegate void BLAS_daxpy_func(ref int n, ref double da, ref double dx, ref int incx, ref double dy, ref int incy);
        public delegate void BLAS_dswap_func(ref int n, ref double dx, ref int incx, ref double dy, ref int incy);
        public delegate double BLAS_ddot_func(ref int n, ref double dx, ref int incx, ref double dy, ref int incy);
        public delegate int BLAS_idamax_func(ref int n, ref double x, ref int @is);
        public delegate void BLAS_dload_func(ref int n, ref double da, ref double dx, ref int incx);
        public delegate double BLAS_dnormi_func(ref int n, ref double x);
    }
}
