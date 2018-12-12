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
        public const long MAXINT64 = 0;
        // public const double REALXP = 0;
        public const int LUSOL_SOLVE_Atv_w = 6;
        public const int BFP_STATUS_ERROR = 7;
        public const int matRowColStep = 1;
        public const int matValueStep = 1;
        public const int BFP_STAT_REFACT_TOTAL = 0;
        public const int BFP_STAT_REFACT_TIMED = 1;
        public const int BFP_STAT_REFACT_DENSE = 2;
        public const int QS_IS_switch = 4;
        

        public const int LUSOL_INFORM_RANKLOSS = -1;

        public const int LUSOL_INFORM_MIN = LUSOL_INFORM_RANKLOSS;
        public const int LUSOL_INFORM_MAX = LUSOL_INFORM_NOMEMLEFT;

        //START:ADDED BELOW CONSTANT FOR METHOD LU6SOL which is in lusol6a.cs file
        public const int LUSOL_SOLVE_Lv_v = 1;  /* v  solves   L v = v(input). w  is not touched. */
        public const int LUSOL_SOLVE_Ltv_v = 2;  /* v  solves   L'v = v(input). w  is not touched. */
        public const int LUSOL_SOLVE_Uw_v = 3;  /* w  solves   U w = v.        v  is not altered. */
        public const int LUSOL_SOLVE_Utv_w = 4;  /* v  solves   U'v = w.        w  is destroyed. */
        public const int LUSOL_SOLVE_Aw_v = 5; /* w  solves   A w = v.        v  is altered as in 1. */
                                               //NOTED ISSUE: Temporary set to 0
                                               //public const int LUSOL_FTRAN   LUSOL_SOLVE_Aw_v = 0;
                                               //NOTED ISSUE: Temporary set to 0
                                               //public const int LUSOL_BTRAN  LUSOL_SOLVE_Atv_w = 0;

        public const int LUSOL_SOLVE_Av_v = 7;  /* v  solves   A v = L D L'v = v(input). w  is not touched. */
        public const int LUSOL_SOLVE_LDLtv_v = 8;  /* v  solves       L |D| L'v = v(input). w  is not touched. */
        //END:ADDED BELOW CONSTANT FOR METHOD LU6SOL which is in lusol6a.cs file

        //START:ADDED BELOW CONSTANT FOR METHOD LU6L which is in lusol6a.cs file
        public const int LUSOL_IP_COLCOUNT_L0 = 20;
        public const int LUSOL_IP_NONZEROS_L0 = 21;
        public const int LUSOL_IP_NONZEROS_L = 23;
        public const int LUSOL_RP_ZEROTOLERANCE = 3;
        public const int LUSOL_INFORM_LUSUCCESS = 0;
        public const int LUSOL_IP_INFORM = 10;
        public const int LUSOL_IP_BTRANCOUNT = 31;
        public const int LUSOL_RP_SMARTRATIO = 0;
        public const int LUSOL_IP_ROWCOUNT_L0 = 32
        //END:ADDED BELOW CONSTANT FOR METHOD LU6L which is in lusol6a.cs file

        //START:ADDED BELOW CONSTANT FOR METHOD LU1L0 which is in lusol6l0.cs file
        public const int LUSOL_IP_ACCELERATION = 7;
        public const int LUSOL_AUTOORDER = 2;
        public const int LUSOL_BASEORDER = 0;
        public const int LUSOL_ACCELERATE_L0 = 4;
        public const int LUSOL_INFORM_NOMEMLEFT = 10;


        //public const int LUSOL_RP_SMARTRATIO = 0;
        public const int ZERO = 0;
        public const int LUSOL_IP_FTRANCOUNT = 30;
        public const int LUSOL_ACCELERATE_U = 8;
        public const int LUSOL_IP_RANK_U = 16;
        public const int LUSOL_IP_NONZEROS_U = 24;
        public const int LUSOL_INFORM_LUSINGULAR = 1;
        public const int LUSOL_RP_RESIDUAL_U = 20;
        //END:ADDED BELOW CONSTANT FOR METHOD LU1L0 which is in lusol6l0.cs file

        // changed from 'Func<double, double, double>' to 'Func<object, object, object>' on 27/11/18
        internal static Func<object, object, object> MIN = (x, y) => (((double)x) < ((double)y) ? (x) : (y));
        internal static Func<double, double, double> MAX = (x, y) => ((x) > (y) ? (x) : (y));

        internal static Func<bool, bool> my_boolstr = (x) => (!(x) ? false : true);
        // PREVIOUS: internal static Action<int, int> SETMIN = delegate (int x, int y) { if (x > y) x = y; };
        internal static Action<double, double> SETMIN = delegate (double x, double y) { if (x > y) x = y; };
        // PREVIOUS: public static Action<int, int> SETMAX = delegate (int x, int y) { if (x < y) x = y; };
        public static Action<double, double> SETMAX = delegate (double x, double y) { if (x < y) x = y; };

        public static Action<double, double> doubleSETMAX = delegate (double x, double y) { if (x < y) x = y; };

        static Func<int, int, int, int> LIMIT = (lo, x, hi) => ((x < (lo) ? lo : ((x) > hi ? hi : x)));
        static Func<int, int, int, bool> BETWEEN = (x, a, b) => ((x - a) * (x - b) <= 0);
        internal static Func<bool, object, object, object> IF = (t, x, y) => ((t) ? (x) : (y));
        static Func<object, object> SIGN = (x) => ((int)(x) < 0 ? -1 : 1);
        // changed from 'Func<double, double, double>' to 'Func<object, object, object>' on 27/11/18
        internal static Func<object, object, object> DELTA_SIZE = (newSize, oldSize) =>
            (double)(((double)newSize) * (double)MIN(1.33, System.Math.Pow(1.5, System.Math.Abs((sbyte)newSize) / (((double)oldSize + (double)newSize) + 1))));

        // ORIGINAL LINE: typedef int (CMP_CALLMODEL findCompare_func)(const void* current, const void* candidate);
        public delegate int findCompare_func(object current, object candidate);

        static Func<double, double, object> CMP_COMPARE = (current, candidate) => (current < candidate ? -1 : (current > candidate ? 1 : 0));
        /// <summary> FIX_f2848dbd-7f97-4103-bea5-ba91f8eb29ce 28/11/18
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
        internal static int mod(int n, int d)
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
        internal static int gcd(int a, int b, ref int c, ref int d)
        {
            /* Return the greatest common divisor of a and b, or -1 if it is
               not defined. Return through the pointer arguments the integers
               such that gcd(a,b) = c*a + b*d. */

            long q = new long();
            long r = new long();
            long t = new long();
            int cret =0;
            int dret =0;
            int C = 0;
            int D = 0;
            int rval;
            int sgn_a = 1;
            int sgn_b = 1;
            int swap = 0;

            if ((a == 0) || (b == 0))
            {
                return (-1);
            }

            /* Use local multiplier instances, if necessary */
            if (c == null)
            {
                c = cret;
            }
            if (d == null)
            {
                d = dret;
            }

            /* Normalize so that 0 < a <= b */
            if (a < 0)
            {
                //ORIGINAL LINE: a = -a;
                a = (-a);
                sgn_a = -1;
            }
            if (b < 0)
            {
                //ORIGINAL LINE: b = -b;
                b = (-b);
                sgn_b = -1;
            }
            if (b < a)
            {
                //ORIGINAL LINE: t = b;
                t=(b);
                //ORIGINAL LINE: b = a;
                b= (a);
                //ORIGINAL LINE: a = t;
                a=((int)t);
                swap = 1;
            }

            /* Now a <= b and both >= 1. */
            //ORIGINAL LINE: q = b/a;
            q = (b / a);
            
            //ORIGINAL LINE: r = b - a *q;
            r = (b - a * q);
            if (r == 0)
            {
                if (swap != 0)
                {
                    d = 1;
                    c = 0;
                }
                else
                {
                    c = 1;
                    d = 0;
                }
                c = sgn_a * c;
                d = sgn_b * d;
                return ((int)a);
            }

            rval = gcd(a, (int)r, ref C, ref D);
            if (swap != 0)
            {
                d = (int)(C - D * q);
                c = D;
            }
            else
            {
                d = D;
                c = (int)(C - D * q);
            }
            c = sgn_a * c;
            d = sgn_b * d;
            return (rval);

        }

        private static int findIndex(int target, ref int attributes, int count, int offset)
        {
            throw new NotImplementedException();
        }
        internal static int findIndexEx(object target, object attributes, int count, int offset, int recsize, findCompare_func findCompare, byte ascending)
        {
            int focusPos;
            int beginPos;
            int endPos;
            int compare;
            int order;
            Object focusAttrib;
            Object beginAttrib;
            Object endAttrib;

            /* Set starting and ending index offsets */
            beginPos = offset;
            endPos = beginPos + count - 1;
            if (endPos < beginPos)
            {
                return (-1);
            }
            order = (ascending != null ? -1 : 1);

            /* Do binary search logic based on a sorted attribute vector */
            focusPos = (beginPos + endPos) / 2;
            //NOTED ISSUE
            //FIX_f2848dbd-7f97-4103-bea5-ba91f8eb29ce 28/11/18
            beginAttrib = (((string)attributes) + ((double)beginPos) * (double)recsize);    // CMP_ATTRIBUTES(beginPos);
            //FIX_f2848dbd-7f97-4103-bea5-ba91f8eb29ce 28/11/18
            focusAttrib = (((string)attributes) + ((double)focusPos) * (double)recsize);    //CMP_ATTRIBUTES(focusPos);
            //FIX_f2848dbd-7f97-4103-bea5-ba91f8eb29ce 28/11/18
            endAttrib = (((string)attributes) + ((double)endPos) * (double)recsize);  // CMP_ATTRIBUTES(endPos);

            compare = 0;
            while (endPos - beginPos > LINEARSEARCH)
            {
                if (findCompare(target, beginAttrib) == 0)
                {
                    focusAttrib = beginAttrib;
                    endPos = beginPos;
                }
                else if (findCompare(target, endAttrib) == 0)
                {
                    focusAttrib = endAttrib;
                    beginPos = endPos;
                }
                else
                {
                    compare = findCompare(target, focusAttrib) * order;
                    if (compare < 0)
                    {
                        beginPos = focusPos + 1;
                        //FIX_f2848dbd-7f97-4103-bea5-ba91f8eb29ce 28/11/18
                        beginAttrib = (((string)attributes) + ((double)beginPos) * (double)recsize);    //CMP_ATTRIBUTES(beginPos);
                        focusPos = (beginPos + endPos) / 2;
                        //FIX_f2848dbd-7f97-4103-bea5-ba91f8eb29ce 28/11/18
                        focusAttrib = (((string)attributes) + ((double)focusPos) * (double)recsize);    // CMP_ATTRIBUTES(focusPos);
                    }
                    else if (compare > 0)
                    {
                        endPos = focusPos - 1;
                        //FIX_f2848dbd-7f97-4103-bea5-ba91f8eb29ce 28/11/18
                        endAttrib = (((string)attributes) + ((double)endPos) * (double)recsize);  // CMP_ATTRIBUTES(endPos);
                        //FIX_f2848dbd-7f97-4103-bea5-ba91f8eb29ce 28/11/18
                        focusPos = (beginPos + endPos) / 2;
                        focusAttrib = (((string)attributes) + ((double)focusPos) * (double)recsize);    // CMP_ATTRIBUTES(focusPos);
                    }
                    else
                    {
                        beginPos = focusPos;
                        endPos = focusPos;
                    }
                }
            }

            /* Do linear (unsorted) search logic */
            if (endPos - beginPos <= LINEARSEARCH)
            {

                /* Do traditional indexed access */
                //FIX_f2848dbd-7f97-4103-bea5-ba91f8eb29ce 28/11/18
                focusAttrib = (((string)attributes) + ((double)beginPos) * (double)recsize);    // CMP_ATTRIBUTES(beginPos);
                if (beginPos == endPos)
                {
                    compare = findCompare(target, focusAttrib) * order;
                }
                else
                {
                    while ((beginPos < endPos) && ((compare = findCompare(target, focusAttrib) * order) < 0))
                    {
                        beginPos++;
                        //FIX_f2848dbd-7f97-4103-bea5-ba91f8eb29ce 28/11/18
                        focusAttrib = (((string)attributes) + ((double)beginPos) * (double)recsize);    // CMP_ATTRIBUTES(beginPos);
                    }
                }
            }

            /* Return the index if a match was found, or signal failure with a -1        */
            if (compare == 0) // Found; return retrieval index
            {
                return (beginPos);
            }
            else if (compare > 0) // Not found; last item
            {
                return (-beginPos);
            }
            else if (beginPos > offset + count - 1)
            {
                return (-(endPos + 1)); // Not found; end of list
            }
            else
            {
                return (-(beginPos + 1)); // Not found; intermediate point
            }
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
        internal static int compareINT(object current, object candidate)
        {
            return Convert.ToInt32((CMP_COMPARE((int)current, (int)candidate)));
        }
        /// <summary>
        /// Please check usage and do changes appropriately 
        /// </summary>
        // ORIGINAL LINE: int CMP_CALLMODEL compareREAL(const void* current, const void* candidate);
        internal static int compareREAL(object current, object candidate)
        {
            return (int)(CMP_COMPARE((double)current, (double)candidate));
        }

        /* Heap sort function (procedurally based on the Numerical Recipes version,
   but expanded and generalized to hande any object with the use of
   qsort-style comparison operator).  An expanded version is also implemented,
   where interchanges are reflected in a caller-initialized integer "tags" list. */
        private static void hpsort(object attributes, int count, int offset, int recsize, bool descending, findCompare_func findCompare)
        {
            //C++ TO C# CONVERTER NOTE: 'register' variable declarations are not supported in C#:
            //ORIGINAL LINE: register int i, j, k, ir, order;
            int i;
            int j;
            int k;
            int ir;
            int order;
            //C++ TO C# CONVERTER NOTE: 'register' variable declarations are not supported in C#:
            //ORIGINAL LINE: register char *hold, *base;
            //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
            string hold;
            string @base;
            string save="";

            if (count < 2)
            {
                return;
            }
            offset -= 1;
            attributes = CMP_ATTRIBUTES(attributes, offset, recsize);
            @base = CMP_ATTRIBUTES(attributes, 1, recsize).ToString();
            /*NOT REQUIRED
            save = (string)malloc(recsize);
            */

            if (descending)
            {
                order = -1;
            }
            else
            {
                order = 1;
            }

            k = (count >> 1) + 1;
            ir = count;

            for (;;)
            {
                /*NOT REQUIRED
                if (k > 1)
                {
                    MEMCOPY(save, CMP_ATTRIBUTES(--k), recsize);
                }
                else
                {
                */
                    hold = CMP_ATTRIBUTES(attributes, ir, recsize).ToString();
                /*NOT REQUIRED
                    MEMCOPY(save, hold, recsize);
                    MEMCOPY(hold, @base, recsize);
                    if (--ir == 1)
                    {
                        MEMCOPY(@base, save, recsize);
                        break;
                    }
                }
                */

                i = k;
                j = k << 1;
                while (j <= ir)
                {
                    hold = CMP_ATTRIBUTES(attributes, j, recsize).ToString();
                    if ((j < ir) && (findCompare(hold, Convert.ToInt32(CMP_ATTRIBUTES(attributes, (j + 1), recsize)) * order) < 0))
                    {
                        hold += recsize;
                        j++;
                    }
                    if (findCompare(save, hold) * order < 0)
                    {
                        /*NOT REQUIRED
                        MEMCOPY(CMP_ATTRIBUTES(i), hold, recsize);
                        */
                        i = j;
                        j <<= 1;
                    }
                    else
                    {
                        break;
                    }
                }
                /*NOT REQUIRED
                MEMCOPY(CMP_ATTRIBUTES(i), save, recsize);
                */
            }
            /*NOT REQUIRED
            FREE(save);
            */
        }
        internal static void hpsortex(object attributes, int count, int offset, int recsize, bool descending, findCompare_func findCompare, ref int[] tags)
        {
            if (count < 2)
            {
                return;
            }
            if (tags == null)
            {
                hpsort(attributes, count, offset, recsize, descending, findCompare);
                return;
            }
            else
            {
                int i;
                int j;
                int k;
                int ir;
                int order;
                string hold;
                string @base;
                string save ="";
                int savetag;

                offset -= 1;
                attributes = (((string)attributes) + ((double)offset) * (double)recsize);
                tags[0] += offset;
                @base = (((string)attributes) + ((double)1) * (double)recsize);
                /*NOT REQUIRED
                save = (string)malloc(recsize);
                */
            if (descending)
                {
                    order = -1;
                }
                else
                {
                    order = 1;
                }

                k = (count >> 1) + 1;
                ir = count;

                for (;;)
                {
                    if (k > 1)
                    {
                        /*NOT REQUIRED
                        MEMCOPY(save, CMP_ATTRIBUTES(--k), recsize);
                        */
                        savetag = tags[k];
                    }
                    else
                    {
                        hold = (((string)attributes) + ((double)ir) * (double)recsize);
                        /*NOT REQUIRED
                        MEMCOPY(save, hold, recsize);
                        MEMCOPY(hold, @base, recsize);
                        */
                        savetag = tags[ir];
                        tags[ir] = tags[1];
                        if (--ir == 1)
                        {
                            /*NOT REQUIRED
                            MEMCOPY(@base, save, recsize);
                            */
                            tags[1] = savetag;
                            break;
                        }
                    }

                    i = k;
                    j = k << 1;
                    while (j <= ir)
                    {
                        hold = CMP_ATTRIBUTES(attributes,j, recsize).ToString();
                        if ((j < ir) && (findCompare(hold, Convert.ToInt32(CMP_ATTRIBUTES(attributes,(j + 1),recsize)) * order) < 0))
                        {
                            hold += recsize;
                            j++;
                        }
                        if (findCompare(save, hold) * order < 0)
                        {
                            /*NOT REQUIRED
                            MEMCOPY(CMP_ATTRIBUTES(i), hold, recsize);
                            */
                            tags[i] = tags[j];
                            i = j;
                            j <<= 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    /*NOT REQUIRED
                    MEMCOPY(CMP_ATTRIBUTES(i), save, recsize);
                    */
                    tags[i] = savetag;
                }
                /*NOT REQUIRED
                FREE(save);
                */
            }

        }

        private static void QS_swap(QSORTrec[] a, int i, int j)
        {
            throw new NotImplementedException();
        }
        private static int QS_addfirst(QSORTrec[] a, object mydata)
        {
            a[0].pvoid2.ptr = mydata;
            return (0);
        }
        internal static int QS_append(QSORTrec[] a, int ipos, object mydata)
        {
            if (ipos <= 0)
            {
                ipos = QS_addfirst(a, mydata);
            }
            else
            {
                a[ipos].pvoid2.ptr = mydata;
            }
            return (ipos);
        }
        private static void QS_replace(QSORTrec[] a, int ipos, object mydata)
        {
            throw new NotImplementedException();
        }
        internal static void QS_insert(QSORTrec[] a, int ipos, object mydata, int epos)
        {
            for (; epos > ipos; epos--)
            {
                a[epos] = a[epos - 1];
            }
            a[ipos].pvoid2.ptr = mydata;
        }
        private static void QS_delete(QSORTrec[] a, int ipos, int epos)
        {
            throw new NotImplementedException();
        }
        internal static bool QS_execute(QSORTrec[] a, int count, findCompare_func findCompare, ref int nswaps)
        {
            int iswaps = 0;

            /* Check and initialize */
                if (count <= 1)
            {
                goto Finish;
            }
            count--;

            /* Perform sort */
            iswaps = QS_sort(a, 0, count, findCompare);

            ///#if QS_IS_switch > 0
            iswaps += QS_finish(a, 0, count, findCompare);
            ///#endif

            Finish:
            if (nswaps != null)
            {
                nswaps = iswaps;
            }
            return true;
        }

        internal static int QS_sort(QSORTrec[] a, int l, int r, findCompare_func findCompare)
        {
            //ORIGINAL LINE: register int i, j, nmove = 0;
            int i;
            int j;
            int nmove = 0;
            QSORTrec v;

            /* Perform the a fast QuickSort */
            if ((r - l) > QS_IS_switch)
            {
                i = (r + l) / 2;

                /* Tri-Median Method */
                if (findCompare(a[l], a[i]) > 0)
                {
                    nmove++;
                    QS_swap(a, l, i);
                }
                if (findCompare(a[l], a[r]) > 0)
                {
                    nmove++;
                    QS_swap(a, l, r);
                }
                if (findCompare(a[i], a[r]) > 0)
                {
                    nmove++;
                    QS_swap(a, i, r);
                }

                j = r - 1;
                QS_swap(a, i, j);
                i = l;
                v = a[j];
                for (;;)
                {
                    while (findCompare(a[++i], v) < 0)
                    {
                        ;
                    }
                    while (findCompare(a[--j], v) > 0)
                    {
                        ;
                    }
                    if (j < i)
                    {
                        break;
                    }
                    nmove++;
                    QS_swap(a, i, j);
                }
                nmove++;
                QS_swap(a, i, r - 1);
                nmove += QS_sort(a, l, j, new findCompare_func(findCompare));
                nmove += QS_sort(a, i + 1, r, new findCompare_func(findCompare));
            }
            return (nmove);
        }

        internal static int QS_finish(QSORTrec[] a, int lo0, int hi0, findCompare_func findCompare)
        {
            int i;
            int j;
            int nmove = 0;
            QSORTrec v;

            /* This is actually InsertionSort, which is faster for local sorts */
            for (i = lo0 + 1; i <= hi0; i++)
            {

                /* Save bottom-most item */
                v = a[i];

                /* Shift down! */
                j = i;
                while ((j > lo0) && (findCompare(a[j - 1], v) > 0))
                {
                    a[j] = a[j - 1];
                    j--;
                    nmove++;
                }

                /* Store bottom-most item at the top */
                a[j] = v;
            }
            return (nmove);
        }


        private static int sortByREAL(ref int item, ref double weight, int size, int offset, byte unique)
        {
            throw new NotImplementedException();
        }
        internal static int sortByINT(int[] item, int[] weight, int size, int offset, bool unique)
        {
            int i;
            int ii;
            int saveI;
            int saveW;

            for (i = 1; i < size; i++)
            {
                ii = i + offset - 1;
                while ((ii >= offset) && (weight[ii] >= weight[ii + 1]))
                {
                    if (weight[ii] == weight[ii + 1])
                    {
                        if (unique)
                        {
                            return (item[ii]);
                        }
                    }
                    else
                    {
                        saveI = item[ii];
                        saveW = weight[ii];
                        item[ii] = item[ii + 1];
                        weight[ii] = weight[ii + 1];
                        item[ii + 1] = saveI;
                        weight[ii + 1] = saveW;
                    }
                    ii--;
                }
            }
            return (0);
        }

        /// <summary> FIX_56e7e228-14a6-4c52-9a3c-9dc092b89cb1  16/11/18
        /// PREVIOUS: public static double sortREALByINT(ref double item, ref int? weight, int size, int offset, bool unique)
        /// ERROR 1 IN PREVIOUS: Cannot apply indexing with[] to an expression of type 'int?'	ZS.Math.Optimization
        /// FIX 1_1: changed 'ref int? weight' to 'ref int?[] weight'
        /// ERROR 2 IN PREVIOUS: Cannot apply indexing with[] to an expression of type 'double'	ZS.Math.Optimization
        /// FIX 2_1: changed 'ref double item' to 'ref double[] item'
        /// </summary>
        public static double sortREALByINT(ref double?[] item, ref int?[] weight, int size, int offset, bool unique)
        {
            int i;
            int ii;
            int saveW;
            double saveI;

            for (i = 1; i < size; i++)
            {
                ii = i + offset - 1;
                while ((ii >= offset) && (weight[ii] >= weight[ii + 1]))
                {
                    if (weight[ii] == weight[ii + 1])
                    {
                        if (unique)
                        {
                            return ((item != null) ? Convert.ToDouble(item[ii]) : 0);
                        }
                    }
                    else
                    {
                        saveI = (item != null) ? Convert.ToDouble(item[ii]) : 0;
                        /// <summary> FIX_2be20793-24c7-43c7-b004-301fe9846a02  16/11/18
                        /// PREVIOUS: saveW = weight[ii];
                        /// ERROR IN PREVIOUS: Cannot implicitly convert type 'int?' to 'int'.An explicit conversion exists (are you missing a cast?)
                        /// FIX 1: saveW = (weight[ii] != null) ? Convert.ToInt32(weight[ii]) : 0;
                        /// </summary>
                        saveW = (weight[ii] != null) ? Convert.ToInt32(weight[ii]) : 0;
                        item[ii] = item[ii + 1];
                        weight[ii] = weight[ii + 1];
                        item[ii + 1] = saveI;
                        weight[ii + 1] = saveW;
                    }
                    ii--;
                }
            }
            return (0);

        }

        internal static double timeNow()
        {
#if INTEGERTIME
//C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
  return ((double)time(null));
#elif CLOCKTIME
//C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
  return ((double)clock() / CLOCKS_PER_SEC);
#elif PosixTime
  private timespec t = new timespec();
#if false
//  clock_gettime(CLOCK_REALTIME, &t);
//  return( (double) t.tv_sec + (double) t.tv_nsec/1.0e9 );
#else
  private static double timeBase;

//C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
  clock_gettime(CLOCK_MONOTONIC, &t);
//C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
  if (timeBase == 0)
//C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
	timeBase = clockNow() - ((double) t.tv_sec + (double) t.tv_nsec / 1.0e9);
//C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
  return (timeBase + (double) t.tv_sec + (double) t.tv_nsec / 1.0e9);
#endif
#elif EnhTime
  private static LARGE_INTEGER freq = new LARGE_INTEGER();
  private static double timeBase;
  private LARGE_INTEGER now = new LARGE_INTEGER();

//C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
  QueryPerformanceCounter(&now);
//C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
  if (timeBase == 0)
  {
	QueryPerformanceFrequency(freq);
	timeBase = clockNow() - (double) now.QuadPart / (double) freq.QuadPart;
  }
//C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
  return (timeBase + (double) now.QuadPart / (double) freq.QuadPart);
#else
            /* can use Time.Now() instead??
            timeb buf = new timeb();
            */

            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            /* can use Time.Now() instead??
              ftime(&buf);
              */
            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            /* can use Time.Now() instead??
              return ((double)buf.time + ((double) buf.millitm) / 1000.0);
            #endif
            */
            return Convert.ToDouble(DateTime.Now);
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
#endif
}
