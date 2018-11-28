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
        /* ************************************************************************ */
        /* Function pointers for external BLAS library (C base 0)                   */
        /* ************************************************************************ */
        internal static MyBlasLib.BLAS_dscal_func BLAS_dscal;
        internal static MyBlasLib.BLAS_dcopy_func BLAS_dcopy;
        internal static MyBlasLib.BLAS_daxpy_func BLAS_daxpy;
        internal static MyBlasLib.BLAS_dswap_func BLAS_dswap;
        internal static MyBlasLib.BLAS_ddot_func BLAS_ddot;
        internal static MyBlasLib.BLAS_idamax_func BLAS_idamax;
        internal static MyBlasLib.BLAS_dload_func BLAS_dload;
        internal static MyBlasLib.BLAS_dnormi_func BLAS_dnormi;

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
        {
#if LoadableBlasLib
  return ((bool)(hBLAS == null));
#else
            return true;
#endif
        }
        /// <summary>
        /// changed from byte to bool on 8/11/18 as expected return type is bool in LpMps method
        /// </summary>
        public static bool load_BLAS(ref string libname)
        {
            bool result = true;

#if LoadableBlasLib
  if (hBLAS != null)
  {
#if WIN32
//C++ TO C# CONVERTER NOTE: There is no C# equivalent to 'FreeLibrary':
//	FreeLibrary(hBLAS);
#else
	dlclose(hBLAS);
#endif
	hBLAS = null;
  }
#endif

            if (libname == null)
            {
                if (!mustinitBLAS && is_nativeBLAS())
                {
                    return false;
                }
                BLAS_dscal = my_dscal;
                BLAS_dcopy = my_dcopy;
                BLAS_daxpy = my_daxpy;
                BLAS_dswap = my_dswap;
                BLAS_ddot = my_ddot;
                BLAS_idamax = my_idamax;
                BLAS_dload = my_dload;
                BLAS_dnormi = my_dnormi;
                if (mustinitBLAS)
                {
                    mustinitBLAS = false;
                }
            }
            else
            {
#if LoadableBlasLib
#if WIN32
   /* Get a handle to the Windows DLL module. */
	hBLAS = LoadLibrary(libname);

   /* If the handle is valid, try to get the function addresses. */
	result = (bool)(hBLAS != null);
	if (result)
	{
	  BLAS_dscal = (BLAS_dscal_func) GetProcAddress(hBLAS, BLAS_prec "scal");
	  BLAS_dcopy = (BLAS_dcopy_func) GetProcAddress(hBLAS, BLAS_prec "copy");
	  BLAS_daxpy = (BLAS_daxpy_func) GetProcAddress(hBLAS, BLAS_prec "axpy");
	  BLAS_dswap = (BLAS_dswap_func) GetProcAddress(hBLAS, BLAS_prec "swap");
	  BLAS_ddot = (BLAS_ddot_func) GetProcAddress(hBLAS, BLAS_prec "dot");
	  BLAS_idamax = (BLAS_idamax_func) GetProcAddress(hBLAS, "i" BLAS_prec "amax");
#if false
//      BLAS_dload  = (BLAS_dload_func *)  GetProcAddress(hBLAS, BLAS_prec "load");
//      BLAS_dnormi = (BLAS_dnormi_func *) GetProcAddress(hBLAS, BLAS_prec "normi");
#endif
	}
#else
   /* First standardize UNIX .SO library name format. */
	string blasname = new string(new char[260]);
//C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
	char * ptr;

	blasname = libname;
	if ((ptr = StringFunctions.StrRChr(libname, '/')) == null)
	{
	  ptr = libname;
	}
	else
	{
	  ptr++;
	}
	blasname = blasname.Substring(0, (int)(ptr - libname));
	if (string.Compare(ptr, 0, "lib", 0, 3))
	{
	  blasname += "lib";
	}
	blasname += ptr;
	if (string.Compare(blasname.Substring(blasname.Length) - 3, ".so"))
	{
	  blasname += ".so";
	}

   /* Get a handle to the module. */
	hBLAS = dlopen(blasname, RTLD_LAZY);

   /* If the handle is valid, try to get the function addresses. */
	result = (bool)(hBLAS != null);
	if (result)
	{
	  BLAS_dscal = (BLAS_dscal_func) dlsym(hBLAS, BLAS_prec "scal");
	  BLAS_dcopy = (BLAS_dcopy_func) dlsym(hBLAS, BLAS_prec "copy");
	  BLAS_daxpy = (BLAS_daxpy_func) dlsym(hBLAS, BLAS_prec "axpy");
	  BLAS_dswap = (BLAS_dswap_func) dlsym(hBLAS, BLAS_prec "swap");
	  BLAS_ddot = (BLAS_ddot_func) dlsym(hBLAS, BLAS_prec "dot");
	  BLAS_idamax = (BLAS_idamax_func) dlsym(hBLAS, "i" BLAS_prec "amax");
#if false
//      BLAS_dload  = (BLAS_dload_func *)  dlsym(hBLAS, BLAS_prec "load");
//      BLAS_dnormi = (BLAS_dnormi_func *) dlsym(hBLAS, BLAS_prec "normi");
#endif
	}
#endif
#endif
                /* Do validation */
                if (!result || ((BLAS_dscal == null) || (BLAS_dcopy == null) || (BLAS_daxpy == null) || (BLAS_dswap == null) || (BLAS_ddot == null) || (BLAS_idamax == null) || (BLAS_dload == null) || (BLAS_dnormi == null)))
                {
                    libname = null;
                    load_BLAS(ref libname);
                    result = false;
                }
            }
            return result;
        }

        public static bool unload_BLAS()
        {
            string libname = null;
            return (load_BLAS(ref libname));
        }

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
