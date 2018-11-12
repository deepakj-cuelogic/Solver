using System;

namespace ZS.Math.Optimization
{
    public static class commonlib
    {
        public const int LINEARSEARCH = 5;
        public const double BIGNUMBER = 1.0e+30;
        public const double TINYNUMBER = 1.0e-04;
        public const double MACHINEPREC = 2.22e-16;
        public const double MATHPREC = 1.0e-16;
        public const double ERRLIMIT = 1.0e-06;

        static Func<double, double, double> MIN = (x, y) => ((x) < (y) ? (x) : (y));
        static Func<double, double, double> MAX = (x, y) => ((x) > (y) ? (x) : (y));

        static Action<int, int> SETMIN = delegate (int x, int y) { if (x > y) x = y; };
        static Action<int, int> SETMAX = delegate (int x, int y) { if (x < y) x = y; };

        static Func<int, int, int, int> LIMIT = (lo, x, hi) => ((x < (lo) ? lo : ((x) > hi ? hi : x)));
        static Func<int, int, int, bool> BETWEEN = (x, a, b) => ((x - a) * (x - b) <= 0);
        static Func<bool, int, int, int> IF = (t, x, y) => ((t) ? (x) : (y));
        static Func<object, object> SIGN = (x) => ((int)(x) < 0 ? -1 : 1);

        static Func<double, double, double> DELTA_SIZE = (newSize, oldSize) =>
            (double)((newSize) * MIN(1.33, System.Math.Pow(1.5, System.Math.Abs(newSize) / ((oldSize + newSize) + 1))));

        // ORIGINAL LINE: typedef int (CMP_CALLMODEL findCompare_func)(const void* current, const void* candidate);
        public delegate int findCompare_func(object current, object candidate);

        static Func<int, int, object> CMP_COMPARE = (current, candidate) => (current < candidate ? -1 : (current > candidate ? 1 : 0));
        /// <summary>
        /// Not able to find variables in file i.e. attributes, recsize
        /// Please check usage and do changes appropriately 
        /// </summary>
        // ORIGINAL LINE: #define CMP_ATTRIBUTES(item)            (((char *) attributes)+(item)*recsize)
        static Func<object, object, object, object> CMP_ATTRIBUTES = (attributes, item, recsize) => (((string)attributes) + ((double)item) * (double)recsize);
        /// <summary>
        /// Not able to find variables in file i.e. tags, tagsize
        /// Please check usage and do changes appropriately 
        /// </summary>
        // ORIGINAL LINE: #define CMP_TAGS(item)                  (((char *) tags)+(item)*tagsize)
        static Func<object, object, object, object> CMP_TAGS = (tags, item, tagsize) => (((string)tags) + ((double)item) * (double)tagsize);


        private static int intpow(int @base, int exponent)
        {
            throw new NotImplementedException();
        }
        private static int mod(int n, int d)
        {
            throw new NotImplementedException();
        }

        private static void strtoup(ref string s)
        {
            throw new NotImplementedException();
        }
        private static void strtolo(ref string s)
        {
            throw new NotImplementedException();
        }
        private static void strcpyup(ref string t, ref string s)
        {
            throw new NotImplementedException();
        }
        private static void strcpylo(ref string t, ref string s)
        {
            throw new NotImplementedException();
        }

        private static byte so_stdname(ref string stdname, ref string descname, int buflen)
        {
            throw new NotImplementedException();
        }
        private static int gcd(int a, int b, ref int c, ref int d)
        {
            throw new NotImplementedException();
        }

        private static int findIndex(int target, ref int attributes, int count, int offset)
        {
            throw new NotImplementedException();
        }
        private static int findIndexEx(object target, object attributes, int count, int offset, int recsize, findCompare_func findCompare, byte ascending)
        {
            throw new NotImplementedException();
        }

        private static void qsortex_swap(object attributes, int l, int r, int recsize, object tags, int tagsize, ref string save, ref string savetag)
        {
            throw new NotImplementedException();
        }

        private static int qsortex(object attributes, int count, int offset, int recsize, byte descending, findCompare_func findCompare, object tags, int tagsize)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Please check usage and do changes appropriately 
        /// </summary>
        // ORIGINAL LINE: int CMP_CALLMODEL compareCHAR(const void *current, const void *candidate);
        private static int compareCHAR(object current, object candidate)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Please check usage and do changes appropriately 
        /// </summary>
        // ORIGINAL LINE: int CMP_CALLMODEL compareINT(const void *current, const void *candidate);
        private static int compareINT(object current, object candidate)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Please check usage and do changes appropriately 
        /// </summary>
        // ORIGINAL LINE: int CMP_CALLMODEL compareREAL(const void* current, const void* candidate);
        private static int compareREAL(object current, object candidate)
        {
            throw new NotImplementedException();
        }
        private static void hpsort(object attributes, int count, int offset, int recsize, byte descending, findCompare_func findCompare)
        {
            throw new NotImplementedException();
        }
        private static void hpsortex(object attributes, int count, int offset, int recsize, byte descending, findCompare_func findCompare, ref int tags)
        {
            throw new NotImplementedException();
        }

        private static void QS_swap(QSORTrec[] a, int i, int j)
        {
            throw new NotImplementedException();
        }
        private static int QS_addfirst(QSORTrec[] a, object mydata)
        {
            throw new NotImplementedException();
        }
        private static int QS_append(QSORTrec[] a, int ipos, object mydata)
        {
            throw new NotImplementedException();
        }
        private static void QS_replace(QSORTrec[] a, int ipos, object mydata)
        {
            throw new NotImplementedException();
        }
        private static void QS_insert(QSORTrec[] a, int ipos, object mydata, int epos)
        {
            throw new NotImplementedException();
        }
        private static void QS_delete(QSORTrec[] a, int ipos, int epos)
        {
            throw new NotImplementedException();
        }
        private static byte QS_execute(QSORTrec[] a, int count, findCompare_func findCompare, ref int nswaps)
        {
            throw new NotImplementedException();
        }

        private static int sortByREAL(ref int item, ref double weight, int size, int offset, byte unique)
        {
            throw new NotImplementedException();
        }
        private static int sortByINT(ref int item, ref int weight, int size, int offset, byte unique)
        {
            throw new NotImplementedException();
        }
        private static double sortREALByINT(ref double item, ref int weight, int size, int offset, byte unique)
        {
            throw new NotImplementedException();
        }

        private static double timeNow()
        {
            throw new NotImplementedException();
        }

        private static void blockWriteBOOL(FILE output, ref string label, ref byte myvector, int first, int last, byte asRaw)
        {
            throw new NotImplementedException();
        }
        private static void blockWriteINT(FILE output, ref string label, ref int myvector, int first, int last)
        {
            throw new NotImplementedException();
        }
        private static void blockWriteREAL(FILE output, ref string label, ref double myvector, int first, int last)
        {
            throw new NotImplementedException();
        }

        private static void printvec(int n, ref double x, int modulo)
        {
            throw new NotImplementedException();
        }
        private static void printmatSQ(int size, int n, ref double X, int modulo)
        {
            throw new NotImplementedException();
        }
        private static void printmatUT(int size, int n, ref double U, int modulo)
        {
            throw new NotImplementedException();
        }

        private static uint catchFPU(uint mask)
        {
            throw new NotImplementedException();
        }

#if _MSC_VER
private static  int fileCount(ref string filemask)
{
throw new NotImplementedException(); 
        }
private static  byte fileSearchPath(ref string envvar, ref string searchfile, ref string foundpath)
{
throw new NotImplementedException(); 
        }
#endif

    }


    /* This defines a 16 byte sort record (in both 32 and 64 bit OS-es) */
    public class QSORTrec1
    {
        public object ptr;
        public object ptr2;
    }
    public class QSORTrec2
    {
        public object ptr;
        public double realval;
    }
    public class QSORTrec3
    {
        public object ptr;
        public int intval;
        public int intpar1;
    }
    public class QSORTrec4
    {
        public double realval;
        public int intval;
        public int intpar1;
    }
    public class QSORTrec5
    {
        public double realval;
        public int longval;
    }
    public class QSORTrec6
    {
        public double realval;
        public double realpar1;
    }
    public class QSORTrec7
    {
        public int intval;
        public int intpar1;
        public int intpar2;
        public int intpar3;
    }
    public class QSORTrec
    {
        public QSORTrec1 pvoid2 = new QSORTrec1();
        public QSORTrec2 pvoidreal = new QSORTrec2();
        public QSORTrec3 pvoidint2 = new QSORTrec3();
        public QSORTrec4 realint2 = new QSORTrec4();
        public QSORTrec5 reallong = new QSORTrec5();
        public QSORTrec6 real2 = new QSORTrec6();
        public QSORTrec7 int4 = new QSORTrec7();
    }
}
