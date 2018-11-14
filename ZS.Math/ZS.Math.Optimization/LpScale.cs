using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


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
        internal static void unscale_columns(lprec lp)
        {
            int i;
            int j;
            int nz;
            MATrec mat = lp.matA;
            double value;
            LpCls objLpCls = new LpCls();
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *rownr, *colnr;
            int rownr;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *colnr;
            int colnr;

            if (!lp.columns_scaled)
            {
                return;
            }

            /* Unscale OF */
            for (j = 1; j <= lp.columns; j++)
            {
                lp.orig_obj[j] = unscaled_mat(lp, lp.orig_obj[j], 0, j);
            }

            /* Unscale mat */
           lp_matrix.mat_validate(mat);
            nz = objLpCls.get_nonzeros(lp);
            value = (COL_MAT_VALUE(0));
            rownr = (COL_MAT_ROWNR(0));
            colnr = (COL_MAT_COLNR(0));
            for (j = 0; j < nz; j++, value += lp_matrix.matValueStep, rownr += lp_matrix.matRowColStep, colnr += lp_matrix.matRowColStep)
            {
                value = unscaled_mat(lp, value, rownr, colnr);
            }

            /* Unscale bounds as well */
            for (i = lp.rows + 1, j = 1; i <= lp.sum; i++, j++)
            {
                lp.orig_lowbo[i] = unscaled_value(lp, lp.orig_lowbo[i], i);
                lp.orig_upbo[i] = unscaled_value(lp, lp.orig_upbo[i], i);
                lp.sc_lobound[j] = unscaled_value(lp, lp.sc_lobound[j], i);
            }

            for (i = lp.rows + 1; i <= lp.sum; i++)
            {
                lp.scalars[i] = 1;
            }

            lp.columns_scaled = false;
            objLpCls.set_action(ref lp.spx_action, lp_lib.ACTION_REBASE | lp_lib.ACTION_REINVERT | lp_lib.ACTION_RECOMPUTE);

        }

        internal static double scaled_value(lprec lp, double value, int index)
        {
            if (System.Math.Abs(value) < lp.infinite)
            {
                if (lp.scaling_used)
                {
                    if (index > lp.rows)
                    {
                        value /= lp.scalars[index];
                    }
                    else
                    {
                        value *= lp.scalars[index];
                    }
                }
            }
            else
            {
                value = lp_types.my_sign(value) * lp.infinite;
            }
            return (value);
        }

        private static double scale(lprec lp, ref double scaledelta)
        {
            throw new NotImplementedException();
        }
        internal static double scaled_mat(lprec lp, double value, int rownr, int colnr)
        {
            if (lp.scaling_used)
            {
                value *= lp.scalars[rownr] * lp.scalars[lp.rows + colnr];
            }
            return (value);

        }
        private static double unscaled_mat(lprec lp, double value, int rownr, int colnr)
        {
            throw new NotImplementedException();
        }
        internal static double unscaled_value(lprec lp, double value, int index)
        {
            if (System.Math.Abs(value) < lp.infinite)
            {
                if (lp.scaling_used)
                {
                    if (index > lp.rows)
                    {
                        value *= lp.scalars[index];
                    }
                    else
                    {
                        value /= lp.scalars[index];
                    }
                }
            }
            else
            {
                value = lp_types.my_sign(value) * lp.infinite;
            }
            return (value);

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
