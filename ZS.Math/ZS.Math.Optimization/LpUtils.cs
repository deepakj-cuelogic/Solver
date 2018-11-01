using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        internal static byte allocINT(lprec lp, int[][] ptr, int size, byte clear)
        {
            throw new NotImplementedException();
        }
        internal static byte allocREAL(lprec lp, double[][] ptr, int size, byte clear)
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
            throw new NotImplementedException();
        }
        internal static string mempool_obtainVector(workarraysrec mempool, int count, int unitsize)
        {
            throw new NotImplementedException();
        }
        internal static byte mempool_releaseVector(workarraysrec mempool, ref string memvector, byte forcefree)
        {
            throw new NotImplementedException();
        }
        internal static byte mempool_free(workarraysrec[] mempool)
        {
            throw new NotImplementedException();
        }

        internal static void roundVector(ref double myvector, int endpos, double roundzero)
        {
            throw new NotImplementedException();
        }
        internal static double normalizeVector(ref double myvector, int endpos)
        {
            throw new NotImplementedException();
        }

        internal static void swapINT(ref int item1, ref int item2)
        {
            throw new NotImplementedException();
        }
        internal static void swapREAL(ref double item1, ref double item2)
        {
            throw new NotImplementedException();
        }
        internal static void swapPTR(object[] item1, object[] item2)
        {
            throw new NotImplementedException();
        }
        internal static double restoreINT(double valREAL, double epsilon)
        {
            throw new NotImplementedException();
        }
        internal static double roundToPrecision(double value, double precision)
        {
            throw new NotImplementedException();
        }

        internal static int searchFor(int target, ref int attributes, int size, int offset, byte absolute)
        {
            throw new NotImplementedException();
        }

        internal static byte isINT(lprec lp, double value)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /* Doubly linked list routines */
        internal static int createLink(int size, LLrec[] linkmap, ref byte usedpos)
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
        internal static byte isActiveLink(LLrec linkmap, int itemnr)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        internal static int prevActiveLink(LLrec linkmap, int forwitemnr)
        {
            throw new NotImplementedException();
        }
        internal static int firstInactiveLink(LLrec linkmap)
        {
            throw new NotImplementedException();
        }
        internal static int lastInactiveLink(LLrec linkmap)
        {
            throw new NotImplementedException();
        }
        internal static int nextInactiveLink(LLrec linkmap, int backitemnr)
        {
            throw new NotImplementedException();
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
