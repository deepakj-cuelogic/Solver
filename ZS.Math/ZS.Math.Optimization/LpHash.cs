using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public static class lp_Hash
    {
        internal const int HASH_START_SIZE = 5000;  /* Hash table size for row and column name storage */
        internal const int NUMHASHPRIMES = 45;

        private static readonly int HASH_1; //chanegd from uint to int as left/right shift does not work on long and uint
        private static readonly int HASH_2; //chanegd from long to int as left/right shift does not work on long and uint
        private static readonly int HASH_3; //chanegd from long to int as left/right shift does not work on long and uint
        internal static hashtable create_hash_table(int size, int b)
        {
            int i;
            int[] HashPrimes = { 29, 229, 883, 1671, 2791, 4801, 8629, 10007, 15289, 25303, 34843, 65269, 99709, 129403, 147673, 166669, 201403, 222163, 242729, 261431, 303491, 320237, 402761, 501131, 602309, 701507, 800999, 900551, 1000619, 1100837, 1200359, 1300021, 1400017, 1500007, 1750009, 2000003, 2500009, 3000017, 4000037, 5000011, 6000011, 7000003, 8000009, 9000011, 9999991 };
            hashtable ht;

            /* Find a good size for the hash table */
            if (size < HASH_START_SIZE)
            {
                size = HASH_START_SIZE;
            }
            for (i = 0; i < NUMHASHPRIMES - 1; i++)
            {
                if (HashPrimes[i] > size)
                {
                    break;
                }
            }
            size = HashPrimes[i];

            /* Then allocate and initialize memory */
            //C++ TO C# CONVERTER TODO TASK: The memory management function 'calloc' has no equivalent in C#:
            ht = new hashtable();
            //C++ TO C# CONVERTER TODO TASK: The memory management function 'calloc' has no equivalent in C#:
            ht.table = new hashelem[System.Runtime.InteropServices.Marshal.SizeOf(ht.table)];
            ht.size = size;
            ht.@base = b;
            ht.count = b - 1;

            return ht;

        }
        static internal void free_hash_table(hashtable ht)
        {
            throw new NotImplementedException();
        }
        static void drophash(string name, hashelem list, hashtable ht)
        {
            throw new NotImplementedException();
        }
        static void free_hash_item(hashelem hp)
        {
            throw new NotImplementedException();
        }
        static internal hashtable copy_hash_table(hashtable ht, hashelem list, int newsize)
        {
            throw new NotImplementedException();
        }
        internal static int find_var(lprec lp, string name, bool verbose)
        {
            hashelem hp;
            lp_report objlp_report = new lp_report();

            if (lp.colname_hashtab != null)
            {
                hp = findhash(name, lp.colname_hashtab);
            }
            else
            {
                hp = null;
            }

            if (hp == null)
            {
                if (verbose)
                {
                    string msg = "find_var: Unknown variable name '{0}'\n";
                    objlp_report.report(lp, lp_lib.SEVERE, ref msg, name);
                }
                return (-1);
            }
            return (hp.index);
        }
        internal static int find_row(lprec lp, string name, bool Unconstrained_rows_found)
        {
            hashelem hp;

            if (lp.rowname_hashtab != null)
            {
                hp = findhash(name, lp.rowname_hashtab);
            }
            else
            {
                hp = null;
            }

            if (hp == null)
            {
                if (Unconstrained_rows_found)
                { // just ignore them in this case
                    return (-1);
                }
                else
                {
                    return (-1);
                }
            }
            return (hp.index);

        }

        internal static hashelem puthash(string name, int index, hashelem[] list, hashtable ht)
        {
            hashelem hp = null;
            int hashindex;

            if (list != null)
            {
                hp = list[index];
                if (hp != null)
                {
                    list[index] = null;
                }
            }

            if ((hp = findhash(name, ht)) == null)
            {

                hashindex = hashval(name, ht.size);
                //C++ TO C# CONVERTER TODO TASK: The memory management function 'calloc' has no equivalent in C#:
                //NOT REQUIRED: (hashelem)calloc(1, sizeof(hashelem));
                hp = new hashelem();

                //NOT REQUIRED: allocCHAR(null, hp.name, (int)(name.Length + 1), 0);
                hp.name = name;
                hp.index = index;
                ht.count++;
                if (list != null)
                {
                    list[index] = hp;
                }
                ///<summary>
                /// PREVIOUS: hp.next = ht.table[hashindex];
                /// ERROR IN PREVIOUS: Cannot implicitly convert type 'ZS.Math.Optimization.hashelem' to 'ZS.Math.Optimization.hashelem[]' 
                /// hashelem hashelem[] = hashtable.hashelem[][hashindex]
                /// FIX 1: hp.next[0] = ht.table[hashindex];
                /// FIX 2: hp.next = ht.table;
                /// use any of the fixes as per requirement
                ///</summary>
                hp.next[0] = ht.table[hashindex];
                ht.table[hashindex] = hp;
                if (ht.first == null)
                {
                    ht.first = hp;
                }
                if (ht.last != null)
                {
                    ht.last.nextelem = hp;
                }
                ht.last = hp;

            }
            return (hp);
        }

        internal static hashelem findhash(string name, hashtable ht)
        {
            hashelem h_tab_p;
            for (h_tab_p = ht.table[hashval(name, ht.size)]; 
                h_tab_p != null;
            ///<summary>
            /// PREVIOUS: h_tab_p = h_tab_p.next)
            /// ERROR IN PREVIOUS: Cannot implicitly convert type 'ZS.Math.Optimization.hashelem[]' to 'ZS.Math.Optimization.hashelem'
            /// FIX 1: h_tab_p.next[h_tab_p.next.Length-1] 
            ///</summary>
                h_tab_p = h_tab_p.next[h_tab_p.next.Length-1])
            {
                if (string.Compare(name, h_tab_p.name) == 0) // got it!
                {
                    break;
                }
            }
            return (h_tab_p);
        } /* findhash */

        private static int hashval(string str, int size)
        {
            uint result = 0;
            uint tmp;

            for (int idx=0; idx< str.Length; idx++)
            {
                result = Convert.ToUInt32((result << HASH_1) + str.Length);
                if ((tmp = Convert.ToUInt32(result & HASH_3)) != 0)
                {
                    /* if any of the most significant bits is on */
                    result ^= tmp >> HASH_2; // xor them in in a less significant part
                    result ^= tmp; // and reset the most significant bits to 0
                }
            }
            return ((int)(result % size));
        } // hashval

    }
}
