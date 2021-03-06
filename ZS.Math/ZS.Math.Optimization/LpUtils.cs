﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    /* Temporary data storage arrays */
    public class workarraysrec
    {
        public lprec lp;
        public int size;
        public int count;
        public string[] vectorarray;
        //ORIGINAL LINE: int *vectorsize;
        public int[] vectorsize;
    }

    public class LLrec
    {
        public int size; // The allocated list size
        public int count; // The current entry count
        public int firstitem;
        public int lastitem;
        //ORIGINAL LINE: int *map;
        public int[] map; // The list of forward and backward-mapped entries
    }

    public class PVrec
    {
        public int count; // The allocated list item count
        //ORIGINAL LINE: int *startpos;
        public int[] startpos; // Starting index of the current value
        public double value; // The list of forward and backward-mapped entries
        public PVrec parent; // The parent record in a pushed chain
    }

    public static class lp_utils
    {
        /* Put function headers here */
        internal static byte allocCHAR(lprec lp, string[] ptr, int size, byte clear)
        {
            throw new NotImplementedException();
        }
        internal static byte allocMYBOOL(lprec lp, byte[][] ptr, int size, byte clear)
        {
            throw new NotImplementedException();
        }
        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
        //changed from 'int[][] ptr' to 'int[] ptr'; need to check at run time
        internal static bool allocINT(lprec lp, int[][] ptr, int size, byte clear)
        {
            /*NOT REQUIRED
            if (clear == true)
            {
                //C++ TO C# CONVERTER TODO TASK: The memory management function 'calloc' has no equivalent in C#:
                ptr[0] = new int[];
            }
            else if (clear & AUTOMATIC)
            {
                //C++ TO C# CONVERTER TODO TASK: The memory management function 'realloc' has no equivalent in C#:
                ptr[0] = (int)realloc(ptr[0], size * sizeof(int));
                if (clear & 1)
                {
                    MEMCLEAR(ptr[0], size);
                }
            }
            else
            {
                //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
                ptr[0] = (int)malloc(size * sizeof(int));
            }
            if ((ptr == null) && (size > 0))
            {
                lp.report(lp, CRITICAL, "alloc of %d 'INT' failed\n", size);
                lp.spx_status = NOMEMORY;
                return (0);
            }
            else
            {
                return (1);
            }
            */
            //throw new NotImplementedException();
            return true;
        }
        internal static bool allocREAL(lprec lp, double[][] ptr, int size, byte clear)
        {
            throw new NotImplementedException();
        }
        internal static byte allocLREAL(lprec lp, double[][] ptr, int size, byte clear)
        {
            throw new NotImplementedException();
        }
        internal static byte allocFREE(lprec lp, object[] ptr)
        {
            throw new NotImplementedException();
        }
        //ORIGINAL LINE: double *cloneREAL(lprec *lp, double *origlist, int size)
        public static double cloneREAL(lprec lp, ref double[] origlist, int size)
        {
            throw new NotImplementedException();
        }
        //ORIGINAL LINE: byte *cloneMYBOOL(lprec *lp, byte *origlist, int size)
        public static byte cloneMYBOOL(lprec lp, ref byte[] origlist, int size)
        {
            throw new NotImplementedException();
        }
        //ORIGINAL LINE: int *cloneINT(lprec *lp, int *origlist, int size)
        public static int[] cloneINT(lprec lp, ref int[] origlist, int size)
        {
            throw new NotImplementedException();
        }

        public static int comp_bits(ref byte bitarray1, ref byte bitarray2, int items)
        {
            throw new NotImplementedException();
        }

        internal static workarraysrec mempool_create(lprec lp)
        {
            workarraysrec temp = new workarraysrec();
            /*NOT REQUIRED
            temp = (workarraysrec)calloc(1, sizeof(workarraysrec));
            */
            temp.lp = lp;
            return (temp);

        }
        internal static string mempool_obtainVector(workarraysrec mempool, int count, int unitsize)
        {
            {
                String newmem = null;
                bool? bnewmem = null;
                
                //ORIGINAL LINE: int *inewmem = null, size, i, ib, ie, memMargin = 0;
                int? inewmem = null;
                int size;
                int i;
                int ib;
                int ie;
                int memMargin = 0;
                double? rnewmem = null;
                string msg;

                /* First find the iso-sized window (binary search) */
                size = count * unitsize;
                memMargin += size;
                ib = 0;
                ie = mempool.count - 1;
                while (ie >= ib)
                {
                    i = (ib + ie) / 2;
                    if (System.Math.Abs(mempool.vectorsize[i]) > memMargin)
                    {
                        ie = i - 1;
                    }
                    else if (System.Math.Abs(mempool.vectorsize[i]) < size)
                    {
                        ib = i + 1;
                    }
                    else
                    {
                        /* Find the beginning of the exact-sized array group */
                        do
                        {
                            ib = i;
                            i--;
                        } while ((i >= 0) && (System.Math.Abs(mempool.vectorsize[i]) >= size));
                        break;
                    }
                }

                /* Check if we have a preallocated unused array of sufficient size */
                ie = mempool.count - 1;
                for (i = ib; i <= ie; i++)
                {
                    if (mempool.vectorsize[i] < 0)
                    {
                        break;
                    }
                }

                /* Obtain and activate existing, unused vector if we are permitted */
                if (i <= ie)
                {
                    //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                    ///#if Paranoia
                    if ((mempool.vectorsize[i] > 0) || (System.Math.Abs(mempool.vectorsize[i]) < size))
                    {
                        lprec lp = mempool.lp;
                        msg = "mempool_obtainVector: Invalid %s existing vector selected\n";
                        lp.report(lp, lp_lib.SEVERE, ref msg, (ie < 0 ? "too small" : "occupied"));
                        lp.spx_status = lp_lib.NOMEMORY;
                        lp.bb_break = 1;
                        return (newmem);
                    }
                    ///#endif
                    newmem = mempool.vectorarray[i];
                    mempool.vectorsize[i] *= -1;
                }

                /* Otherwise allocate a new vector */
                
                else if (unitsize == sizeof(bool))
                {
                    //NOT REQUIRED
                    //allocMYBOOL(mempool.lp, bnewmem, count, 1);
                    newmem = Convert.ToString(bnewmem);
                }
                else if (unitsize == (sizeof(int)))
                {
                    //NOT REQUIRED
                    //allocINT(mempool.lp, inewmem, count, 1);
                    newmem = Convert.ToString(inewmem);
                }
                //C++ TO JAVA CONVERTER TODO TASK: There is no Java equivalent to 'sizeof':
                else if (unitsize == sizeof(double))
                {
                    //NOT REQUIRED
                    //allocREAL(mempool.lp, rnewmem, count, 1);
                    newmem = Convert.ToString(rnewmem);
                }

                /* Insert into master array if necessary (maintain sort by ascending size) */
                if ((i > ie) && (newmem != null))
                {
                    mempool.count++;
                    if (mempool.count >= mempool.size)
                    {
                        mempool.size += 10;

                        //NOT REQUIRED
                        /*mempool.vectorarray = (String)realloc(mempool.vectorarray, sizeof(*(mempool.vectorarray)) * mempool.size);

                        mempool.vectorsize = (int)realloc(mempool.vectorsize, sizeof(*(mempool.vectorsize)) * mempool.size);
                        */
                    }
                    ie++;
                    i = ie + 1;
                    if (i < mempool.count)
                    {
                        //NOT REQUIRED
                        //MEMMOVE(mempool.vectorarray + i, mempool.vectorarray + ie, 1);
                        //MEMMOVE(mempool.vectorsize + i, mempool.vectorsize + ie, 1);
                    }
                    mempool.vectorarray[ie] = newmem;
                    mempool.vectorsize[ie] = size;
                }

                return (newmem);
            }

        }

        internal static byte mempool_releaseVector(workarraysrec mempool, ref string memvector, byte forcefree)
        {
            throw new NotImplementedException();
        }
        internal static byte mempool_free(workarraysrec[] mempool)
        {
            throw new NotImplementedException();
        }

        internal static void roundVector(ref double[] myvector, int endpos, double roundzero)
        {
            throw new NotImplementedException();
        }
        internal static double normalizeVector(ref double myvector, int endpos)
        {
            throw new NotImplementedException();
        }

        internal static void swapINT(ref int item1, ref int item2)
        {
            int hold = item1;
            item1 = item2;
            item2 = hold;
        }
        internal static void swapREAL(ref double item1, ref double item2)
        {
            double hold = item1;
            item1 = item2;
            item2 = hold;
        }
        internal static void swapPTR(object[] item1, object[] item2)
        {
            object hold;
  hold = item1[0];
  item1[0] = item2[0];
  item2[0] = hold;

        }
        internal static double restoreINT(double valREAL, double epsilon)
        {
            throw new NotImplementedException();
        }
        internal static double roundToPrecision(double value, double precision)
        {
            //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
            #if ONE
            double vmod = new double();
            int vexp2 = 0;
            int vexp10;
            double sign = new double();

            if (precision == 0)
            {
                return (value);
            }

            sign = lp_types.my_sign(value);
            value = System.Math.Abs(value);

            /* Round to integer if possible */
            if (value < precision)
            {
                return (0);
            }
            else if (value == System.Math.Floor(value))
            {
                return (value * sign);
            }
            else if ((value < (double)commonlib.MAXINT64) && (modf((double)(value + precision), vmod) < precision))
            {
                /* sign *= (LLONG) (value+precision); */
                sign *= (long)(value + 0.5);
                return ((double)sign);
            }

            /* Optionally round with base 2 representation for additional precision */
            //#define roundPrecisionBase2
            //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
            #if roundPrecisionBase2
            //NOTED ISSUE: frexp
            value = frexp(value, vexp2);
            #else
            vexp2 = 0;
            #endif

            /* Convert to desired precision */
            vexp10 = (int)System.Math.Log10(value);
            precision *= System.Math.Pow(10.0, vexp10);
            //NOTED ISSUE
            modf(value / precision + 0.5, value);
            value *= sign * precision;

            /* Restore base 10 representation if base 2 was active */
            if (vexp2 != 0)
            {
                //NOTED ISSUE: ldexp
                value = ldexp(value, vexp2);
            }
            #endif

            return (value);

        }

        internal static double modf(double v, double vmod)
        {
            string input_decimal_number = (v + vmod).ToString();
            string decimal_places = "";
            var regex = new System.Text.RegularExpressions.Regex("(?<=[\\.])[0-9]+");
            if (regex.IsMatch(input_decimal_number))
                decimal_places = "0." + regex.Match(input_decimal_number).Value;
            return Convert.ToDouble(decimal_places);
        }

        internal static int searchFor(int target, ref int attributes, int size, int offset, byte absolute)
        {
            throw new NotImplementedException();
        }

        internal static bool isINT(lprec lp, double value)
        {
#if false
//  return( (bool) (modf(fabs(value)+lp->epsint, &value) < 2*lp->epsint) );
#elif true
  value = System.Math.Abs(value) + lp.epsint;
  return ((bool)(lp_types.my_reldiff(value, System.Math.Floor(value)) < 2 * lp.epsint));
#elif false
//  double hold;
//  value = fabs(value);
//  hold = pow(10, MIN(-2, log10(value+1)+log10(lp->epsint)));
//  return( (bool) (modf(value+lp->epsint, &value) < 2*hold) );
#elif false
//  value -= (double)floor(value);
//  return( (bool) ((value < lp->epsint) || (value > (1 - lp->epsint)) );
#else
            value += lp.epsint;
            return ((bool)(System.Math.Abs(value - System.Math.Floor(value)) < 2 * lp.epsint));
#endif

        }
        internal static byte isOrigFixed(lprec lp, int varno)
        {
            throw new NotImplementedException();
        }
        internal static void chsign_bounds(ref double lobound, ref double upbound)
        {
            throw new NotImplementedException();
        }
        internal static double rand_uniform(lprec lp, double range)
        {
            bool randomized = false; /* static ok here for reentrancy/multithreading */
            Random rnd = new Random();
            if (!randomized)
            {
                randomized = true;
                //NOTED ISSUE
                //srand((unsigned)time(NULL));
                //RESOLVED 
                rnd.Next(-1, 1);
            }
            range *= (double)rnd.Next() / (double)rnd.Next();
            return (range);
        }

        /* Doubly linked list routines */
        internal static int createLink(int size, LLrec linkmap, ref bool? usedpos)
        {
            throw new NotImplementedException();
        }
        internal static byte freeLink(LLrec[] linkmap)
        {
            throw new NotImplementedException();
        }
        internal static int sizeLink(LLrec linkmap)
        {
            throw new NotImplementedException();
        }
        internal static bool isActiveLink(LLrec linkmap, int itemnr)
        {
            if ((linkmap.map[itemnr] != 0) || (linkmap.map[linkmap.size + itemnr] != 0) || (linkmap.map[0] == itemnr))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        internal static int countActiveLink(LLrec linkmap)
        {
            throw new NotImplementedException();
        }
        internal static int countInactiveLink(LLrec linkmap)
        {
            throw new NotImplementedException();
        }
        internal static int firstActiveLink(LLrec linkmap)
        {
            return (linkmap.map[0]);
        }
        internal static int lastActiveLink(LLrec linkmap)
        {
            throw new NotImplementedException();
        }
        internal static byte appendLink(LLrec linkmap, int newitem)
        {
            throw new NotImplementedException();
        }
        internal static byte insertLink(LLrec linkmap, int afteritem, int newitem)
        {
            throw new NotImplementedException();
        }
        internal static byte setLink(LLrec linkmap, int newitem)
        {
            throw new NotImplementedException();
        }
        internal static byte fillLink(LLrec linkmap)
        {
            throw new NotImplementedException();
        }
        internal static int nextActiveLink(LLrec linkmap, int backitemnr)
        {
            if ((backitemnr < 0) || (backitemnr > linkmap.size))
            {
                return (-1);
            }
            else
            {
                if (backitemnr < linkmap.lastitem)
                {
                    while ((backitemnr > linkmap.firstitem) && (linkmap.map[backitemnr] == 0))
                    {
                        backitemnr--;
                    }
                }
                return (linkmap.map[backitemnr]);
            }

        }
        internal static int prevActiveLink(LLrec linkmap, int forwitemnr)
        {
            throw new NotImplementedException();
        }
        internal static int firstInactiveLink(LLrec linkmap)
        {
            int i;
            int n;

            if (countInactiveLink(linkmap) == 0)
            {
                return (0);
            }
            n = 1;
            i = firstActiveLink(linkmap);
            while (i == n)
            {
                n++;
                i = nextActiveLink(linkmap, i);
            }
            return (n);
        }
        internal static int lastInactiveLink(LLrec linkmap)
        {
            throw new NotImplementedException();
        }
        internal static int nextInactiveLink(LLrec linkmap, int backitemnr)
        {
            do
            {
                backitemnr++;
            } while ((backitemnr <= linkmap.size) && isActiveLink(linkmap, backitemnr));
            if (backitemnr <= linkmap.size)
            {
                return (backitemnr);
            }
            else
            {
                return (0);
            }
        }
        internal static int prevInactiveLink(LLrec linkmap, int forwitemnr)
        {
            throw new NotImplementedException();
        }
        internal static int removeLink(LLrec linkmap, int itemnr)
        {
            throw new NotImplementedException();
        }
        internal static LLrec cloneLink(LLrec sourcemap, int newsize, byte freesource)
        {
            throw new NotImplementedException();
        }
        internal static int compareLink(LLrec linkmap1, LLrec linkmap2)
        {
            throw new NotImplementedException();
        }
        internal static byte verifyLink(LLrec linkmap, int itemnr, byte doappend)
        {
            throw new NotImplementedException();
        }

        /* Packed vector routines */
        internal static PVrec createPackedVector(int size, ref double values, ref int workvector)
        {
            throw new NotImplementedException();
        }
        internal static void pushPackedVector(PVrec PV, PVrec parent)
        {
            throw new NotImplementedException();
        }
        internal static byte unpackPackedVector(PVrec PV, double[][] target)
        {
            throw new NotImplementedException();
        }
        internal static double getvaluePackedVector(PVrec PV, int index)
        {
            throw new NotImplementedException();
        }
        internal static PVrec popPackedVector(PVrec PV)
        {
            throw new NotImplementedException();
        }
        internal static byte freePackedVector(PVrec[] PV)
        {
            throw new NotImplementedException();
        }

    }
}
