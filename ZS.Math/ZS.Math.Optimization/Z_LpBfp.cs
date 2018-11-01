//#define BFP_STATUS_RANKLOSS     -1
//#define BFP_STATUS_SUCCESS       0
//#define BFP_STATUS_SINGULAR      1
//#define BFP_STATUS_UNSTABLE      2
//#define BFP_STATUS_NOPIVOT       3
//#define BFP_STATUS_DIMERROR      4
//#define BFP_STATUS_DUPLICATE     5
//#define BFP_STATUS_NOMEMORY      6
//#define BFP_STATUS_ERROR         7             /* Unspecified, command-related error */
//#define BFP_STATUS_FATAL         8

//#define BFP_STAT_ERROR          -1
//#define BFP_STAT_REFACT_TOTAL    0
//#define BFP_STAT_REFACT_TIMED    1
//#define BFP_STAT_REFACT_DENSE    2

//# ifndef BFP_CALLMODEL
//# ifdef WIN32
//#define BFP_CALLMODEL __stdcall   /* "Standard" call model */
//#else
//#define BFP_CALLMODEL
//#endif
//#endif
///* Routines with UNIQUE implementations for each inversion engine                     */
//char __BFP_EXPORT_TYPE *(BFP_CALLMODEL bfp_name)(void);
//void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_free)(lprec* lp);
//unsigned char __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_resize)(lprec* lp, int newsize);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_nonzeros)(lprec* lp, unsigned char maximum);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_memallocated)(lprec* lp);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_preparefactorization)(lprec* lp);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_factorize)(lprec* lp, int uservars, int Bsize, unsigned char* usedpos, unsigned char final);
//unsigned char __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_finishupdate)(lprec* lp, unsigned char changesign);
//void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_ftran_normal)(lprec* lp, double* pcol, int* nzidx);
//void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_ftran_prepare)(lprec* lp, double* pcol, int* nzidx);
//void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_btran_normal)(lprec* lp, double* prow, int* nzidx);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_status)(lprec* lp);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_findredundant)(lprec* lp, int items, getcolumnex_func cb, int* maprow, int* mapcol);


///* Routines SHARED for all inverse implementations; located in lp_BFP1.c              */
//unsigned char __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_compatible)(lprec* lp, int bfpversion, int lpversion, int sizeofvar);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_indexbase)(lprec* lp);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_rowoffset)(lprec* lp);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_pivotmax)(lprec* lp);
//double __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_efficiency)(lprec* lp);
//double __BFP_EXPORT_TYPE *(BFP_CALLMODEL bfp_pivotvector)(lprec* lp);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_pivotcount)(lprec* lp);
//unsigned char __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_mustrefactorize)(lprec* lp);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_refactcount)(lprec* lp, int kind);
//unsigned char __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_isSetI)(lprec* lp);
//int* bfp_createMDO(lprec* lp, unsigned char* usedpos, int count, unsigned char doMDO);
//void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_updaterefactstats)(lprec* lp);
//int BFP_CALLMODEL bfp_rowextra(lprec* lp);

///* Routines with OPTIONAL SHARED code; template routines suitable for canned          */
///* inverse engines are located in lp_BFP2.c                                           */
//unsigned char __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_init)(lprec* lp, int size, int deltasize, char* options);
//unsigned char __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_restart)(lprec* lp);
//unsigned char __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_implicitslack)(lprec* lp);
//unsigned char __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_pivotalloc)(lprec* lp, int newsize);
//int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_colcount)(lprec* lp);
//unsigned char __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_canresetbasis)(lprec* lp);
//void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_finishfactorization)(lprec* lp);
//double __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_prepareupdate)(lprec* lp, int row_nr, int col_nr, double* pcol);
//double __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_pivotRHS)(lprec* lp, double theta, double* pcol);
//void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_btran_double)(lprec* lp, double* prow, int* pnzidx, double* drow, int* dnzidx);

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ZS.Math.Optimization
//{
//    class LpBfp
//    {

//        public const int BFP_STATUS_RANKLOSS = -1;
//        public const int BFP_STATUS_SUCCESS = 0;
//        public const int BFP_STATUS_SINGULAR = 1;
//        public const int BFP_STATUS_UNSTABLE = 2;
//        public const int BFP_STATUS_NOPIVOT = 3;
//        public const int BFP_STATUS_DIMERROR = 4;
//        public const int BFP_STATUS_DUPLICATE = 5;
//        public const int BFP_STATUS_NOMEMORY = 6;
//        public const int BFP_STATUS_ERROR = 7; // Unspecified, command-related error
//        public const int BFP_STATUS_FATAL = 8;
//        public const int BFP_STAT_ERROR = -1;
//        public const int BFP_STAT_REFACT_TOTAL = 0;
//        public const int BFP_STAT_REFACT_TIMED = 1;
//        public const int BFP_STAT_REFACT_DENSE = 2;

//#if !BFP_CALLMODEL
//#if WIN32
////C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
////C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
////ORIGINAL LINE: #define BFP_CALLMODEL __stdcall
//#define BFP_CALLMODEL
//#else
//#define BFP_CALLMODEL
//#endif
//#endif
//        /* Routines with UNIQUE implementations for each inversion engine                     */
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        char __BFP_EXPORT_TYPE * (BFP_CALLMODEL bfp_name)();
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_free)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        byte __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_resize)(lprec* lp, int newsize);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_nonzeros)(lprec* lp, byte maximum);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_memallocated)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_preparefactorization)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_factorize)(lprec* lp, int uservars, int Bsize, byte* usedpos, byte final);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        byte __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_finishupdate)(lprec* lp, byte changesign);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_ftran_normal)(lprec* lp, double* pcol, int* nzidx);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_ftran_prepare)(lprec* lp, double* pcol, int* nzidx);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_btran_normal)(lprec* lp, double* prow, int* nzidx);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_status)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_findredundant)(lprec* lp, int items, getcolumnex_func cb, int* maprow, int* mapcol);


//        /* Routines SHARED for all inverse implementations; located in lp_BFP1.c              */
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        byte __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_compatible)(lprec* lp, int bfpversion, int lpversion, int sizeofvar);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_indexbase)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_rowoffset)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_pivotmax)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        double __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_efficiency)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        double __BFP_EXPORT_TYPE * (BFP_CALLMODEL bfp_pivotvector)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_pivotcount)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        byte __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_mustrefactorize)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_refactcount)(lprec* lp, int kind);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        byte __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_isSetI)(lprec* lp);
//        //C++ TO C# CONVERTER WARNING: C# has no equivalent to methods returning pointers to value types:
//        //ORIGINAL LINE: int *bfp_createMDO(lprec *lp, byte *usedpos, int count, byte doMDO);
//        //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
//        //int bfp_createMDO(lprec lp, ref byte usedpos, int count, byte doMDO);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_updaterefactstats)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
//        //int BFP_CALLMODEL bfp_rowextra(lprec lp);

//        /* Routines with OPTIONAL SHARED code; template routines suitable for canned          */
//        /* inverse engines are located in lp_BFP2.c                                           */
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        byte __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_init)(lprec* lp, int size, int deltasize, char* options);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        byte __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_restart)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        byte __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_implicitslack)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        byte __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_pivotalloc)(lprec* lp, int newsize);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        int __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_colcount)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        byte __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_canresetbasis)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_finishfactorization)(lprec* lp);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        double __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_prepareupdate)(lprec* lp, int row_nr, int col_nr, double* pcol);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        double __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_pivotRHS)(lprec* lp, double theta, double* pcol);
//        //C++ TO C# CONVERTER TODO TASK: The #define macro 'BFP_CALLMODEL' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//        void __BFP_EXPORT_TYPE(BFP_CALLMODEL bfp_btran_double)(lprec* lp, double* prow, int* pnzidx, double* drow, int* dnzidx);


//    }
//}
