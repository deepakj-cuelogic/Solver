using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZS.Math.Optimization
{
    public static class lp_scale
    {
        private static byte scale_updatecolumns(lprec lp, ref double scalechange, byte updateonly)
        {
            throw new NotImplementedException();
        }
        private static byte scale_updaterows(lprec lp, ref double scalechange, byte updateonly)
        {
            throw new NotImplementedException();
        }
        private static byte scale_rows(lprec lp, ref double scaledelta)
        {
            throw new NotImplementedException();
        }
        private static byte scale_columns(lprec lp, ref double scaledelta)
        {
            throw new NotImplementedException();
        }
        private static void unscale_columns(lprec lp)
        {
            throw new NotImplementedException();
        }
        private static double scale(lprec lp, ref double scaledelta)
        {
            throw new NotImplementedException();
        }
        private static double scaled_mat(lprec lp, double value, int rownr, int colnr)
        {
            throw new NotImplementedException();
        }
        private static double unscaled_mat(lprec lp, double value, int rownr, int colnr)
        {
            throw new NotImplementedException();
        }
        private static double scaled_value(lprec lp, double value, int index)
        {
            throw new NotImplementedException();
        }
        private static double unscaled_value(lprec lp, double value, int index)
        {
            throw new NotImplementedException();
        }
        private static byte scaleCR(lprec lp, ref double scaledelta)
        {
            throw new NotImplementedException();
        }
        private static byte finalize_scaling(lprec lp, ref double scaledelta)
        {
            throw new NotImplementedException();
        }
        private static double auto_scale(lprec lp)
        {
            throw new NotImplementedException();
        }
        private static void undoscale(lprec lp)
        {
            throw new NotImplementedException();
        }

    }
}
