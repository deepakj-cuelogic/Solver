﻿using System;

namespace ZS.Math.Optimization
{
    public static class lp_types
    {
        //ORIGINAL LINE: #define my_chsign(t, x)       ( ((t) && ((x) != 0)) ? -(x) : (x))
        public static Func<bool, double, double> my_chsign = (t, x) => ((t) && (x != 0)) ? -(x) : (x);

        //ORIGINAL LINE: #define my_flipsign(x)          ( fabs((REAL) (x)) == 0 ? 0 : -(x) )
        public static Func<double, double> my_flipsign = (x) => System.Math.Abs((x == 0) ? 0 : -(x));

        //ORIGINAL LINE: #define my_sign(x)              ((x) < 0 ? -1 : 1)
        public static Func<double, double> my_sign = (x) => ((x) < 0 ? -1 : 1);

        //ORIGINAL LINE: #define my_if(t, x, y)          ((t) ? (x) : (y))
        public static Func<bool, double, double, double> my_if = (t, x, y) => ((t) ? (x) : (y));

        //ORIGINAL LINE: #define my_reldiff(x, y)       (((x) - (y)) / (1.0 + fabs((REAL) (y))))
        public static Func<double, double,double> my_reldiff = (x, y) => (((x) - (y)) / (1.0 + System.Math.Abs((double)(y))));

        //ORIGINAL LINE: #define my_roundzero(val, eps)  if (fabs((REAL) (val)) < eps) val = 0
        public static Action<double, double> my_roundzero = delegate (double val, double eps) { if (System.Math.Abs(val) < eps) val = 0; };

        internal static string RESULTVALUEMASK = "%18.12g";
        
        /* Library load status values */
        internal const int LIB_LOADED = 0;
        internal const int LIB_NOTFOUND = 1;
        internal const int LIB_NOINFO = 2;
        internal const int LIB_NOFUNCTION = 3;
        internal const int LIB_VERINVALID = 4;
        internal const string LIB_STR_LOADED = "Successfully loaded";
        internal const string LIB_STR_NOTFOUND = "File not found";
        internal const string LIB_STR_NOINFO = "No version data";
        internal const string LIB_STR_NOFUNCTION = "Missing function header";
        internal const string LIB_STR_VERINVALID = "Incompatible version";
        internal const int LIB_STR_MAXLEN = 23;

        /* Byte-sized Booleans and extended options */
        internal const byte FALSE = 0;
        internal const byte TRUE = 1;
        internal const byte AUTOMATIC = 2;
        internal const byte DYNAMIC = 4;
    }
    internal static class DefineConstants
    {
        /* Byte-sized Booleans and extended options */
        public const int FALSE = 0;
        public const int TRUE = 1;
        public const int AUTOMATIC = 2;
        public const int DYNAMIC = 4;
    }
    /* B4 factorization optimization data */
    public class B4rec
    {
        //ORIGINAL LINE: int *B4_var;
        public int B4_var; // Position of basic columns in the B4 basis
        //ORIGINAL LINE: int *var_B4;
        public int var_B4; // Variable in the B4 basis
        //ORIGINAL LINE: int *B4_row;
        public int B4_row; // B4 position of the i'th row
        //ORIGINAL LINE: int *row_B4;
        public int row_B4; // Original position of the i'th row
        //ORIGINAL LINE: double *wcol;
        public double wcol;
        //ORIGINAL LINE: int *nzwcol;
        public int nzwcol;
    }

    public class OBJmonrec
    {
        public const int OBJ_STEPS = 5;
        public lprec lp;
        public int oldpivstrategy;
        public int oldpivrule;
        public int pivrule;
        public int ruleswitches;
        public int[] limitstall = new int[2];
        public int limitruleswitches;
        public int[] idxstep = new int[OBJ_STEPS];
        public int countstep;
        public int startstep;
        public int currentstep;
        public int Rcycle;
        public int Ccycle;
        public int Ncycle;
        public int Mcycle;
        public int Icount;
        public double thisobj;
        public double prevobj;
        public double[] objstep = new double[OBJ_STEPS];
        public double thisinfeas;
        public double previnfeas;
        public double epsvalue;
        public string spxfunc = new string(new char[10]);
        public bool pivdynamic;
        public bool isdual;
        public byte active;
    }

    public class edgerec
    {
        //ORIGINAL LINE: double *edgeVector;
        public double[] edgeVector;
    }

    public class pricerec
    {
        double theta;
        double pivot;
        double epspivot;
        int varno;
        lprec[] lp;
        byte isdual;
    }

    public class multirec
    {
        lprec lp;
        int size;                   /* The maximum number of multiply priced rows/columns */
        int used;                   /* The current / active number of multiply priced rows/columns */
        int limit;                  /* The active/used count at which a full update is triggered */
        pricerec[] items;           /* Array of best multiply priced rows/columns */
        int[] freeList;             /* The indeces of available positions in "items" */
        QSORTrec[] sortedList;      /* List of pointers to "pricerec" items in sorted order */
        double[] stepList;          /* Working array (values in sortedList order) */
        double[] valueList;         /* Working array (values in sortedList order) */
        int[] indexSet;             /* The final exported index list of pivot variables */
        int active;                 /* Index of currently active multiply priced row/column */
        int retries;
        double step_base;
        double step_last;
        double obj_base;
        double obj_last;
        double epszero;
        double maxpivot;
        double maxbound;
        byte sorted;
        byte truncinf;
        byte objcheck;
        byte dirty;
    }
}
