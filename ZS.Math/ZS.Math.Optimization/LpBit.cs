using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZS.Math.Optimization
{
    public class lp_bit
    {
        public void set_biton(byte[] bitarray, int item)
        {
            bitarray[item / 8] |= (byte)(1 << (item % 8));
        }

        private void set_bitoff(byte[] bitarray, int item)
        {
            bitarray[item / 8] &= (byte)(~(1 << (item % 8)));
        }

        private byte is_biton(byte[] bitarray, int item)
        {
            var a = (int)bitarray[item / 8];
            var b = (1 << (item % 8));
            return (byte)(a & b);
        }

    }
}
