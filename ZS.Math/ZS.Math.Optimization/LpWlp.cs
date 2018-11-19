using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public class lp_wlp
    {
        /* ------------------------------------------------------------------------- */
        /* Input and output of lp format model files for lp_solve                    */
        /* ------------------------------------------------------------------------- */

        static int LP_MAXLINELEN = 100;

        internal static int write_data(Object userhandle, LpCls.write_modeldata_func write_modeldata, ref string format, params object[] paraArr)
        {
            // NOT REQUIRED
            String buff = new String(new char[512 + 1]);
            /*TO DO
            va_list ap;
            */
            int n;

            /*TO DO
            va_start(ap, format);          
            vsnprintf(buff, DEF_STRBUFSIZE, format, ap);
            va_end(ap);
            */
            n = write_modeldata(userhandle, buff);
            return (n);
        }

        internal static void write_lpcomment(Object userhandle, LpCls.write_modeldata_func write_modeldata, ref string str, bool? newlinebefore)
        {
            string msg = "%s/* %s */\n";
            write_data(userhandle, write_modeldata, ref msg, (newlinebefore) != null ? "\n" : "", str);
        }

        internal static int write_lprow(lprec lp, int rowno, Object userhandle, LpCls.write_modeldata_func write_modeldata, int maxlen, ref int[] idx, ref double[] val)
        {
            int i;
            int j;
            int nchars;
            int elements;
            double a;
            bool first = true;
            string msg = "";

            //NOT REQUIRED
            String buf = new String(new char[50]);

            ///<summary>
            /// PREVIOUS: elements = lp.get_rowex(lp, rowno, val, idx);
            /// ERROR 1 IN PREVIOUS: Argument 3 must be passed with the 'ref' keyword
            /// FIX1: Change to 'ref val' and 'ref idx'
            /// ERROR 2 IN PREVIOUS: Argument 3: cannot convert from 'ref double[]' to 'ref double'
            /// FIX 2: change to 'ref val[0]' and 'ref idx[0]'
            ///</summary>
            elements = lp.get_rowex(lp, rowno, ref val[0], ref idx[0]);
            if (write_modeldata != null)
            {
                nchars = 0;
                for (i = 0; i < elements; i++)
                {
                    j = idx[i];
                    if (lp.is_splitvar(lp, j))
                    {
                        continue;
                    }
                    a = val[i];
                    if (!first)
                    {
                        nchars += write_data(userhandle, write_modeldata, ref msg);
                    }
                    else
                    {
                        first = false;
                    }
                    buf = String.Format("%+.12g", (double)a);
                    if (string.Compare(buf, "-1") == 0)
                    {
                        msg = "-";
                        nchars += write_data(userhandle, write_modeldata, ref msg);
                    }
                    else if (string.Compare(buf, "+1") == 0)
                    {
                        msg = "+";
                        nchars += write_data(userhandle, write_modeldata, ref msg);
                    }
                    else
                    {
                        msg = "%s";
                        nchars += write_data(userhandle, write_modeldata, ref msg, buf);
                    }
                    msg = "%s";
                    nchars += write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, j));
                    /* Check if we should add a linefeed */
                    if ((maxlen > 0) && (nchars >= maxlen) && (i < elements - 1))
                    {
                        string msg1 = "\n";
                        write_data(userhandle, write_modeldata, ref msg, msg1);
                        nchars = 0;
                    }
                }
            }

            return (elements);
        }

        internal bool write_lpex(lprec lp, Object userhandle, LpCls.write_modeldata_func write_modeldata)
        {
            int i;
            int j;
            int b;
            int nrows = lp.rows;
            int ncols = lp.columns;
            int nchars;
            int maxlen = LP_MAXLINELEN;
            //ORIGINAL LINE: int *idx;
            int[] idx = null;
            bool ok;
            double a;
            //ORIGINAL LINE: double *val;
            double[] val = null;
            String ptr;
            string msg;

            lp_report objlpReport = new lp_report();

            if (!lp_matrix.mat_validate(lp.matA))
            {
                msg = "LP_writefile: Could not validate the data matrix.\n";
                objlpReport.report(lp, lp_lib.IMPORTANT, ref msg);
                return false;
            }

            /* Write name of model */
            ptr = lp.get_lp_name(lp);
            if (ptr != null)
            {
                if (ptr!=null)
                {
                    write_lpcomment(userhandle, write_modeldata, ref ptr, false);
                }
                else
                {
                    ptr = null;
                }
            }

            /* Write the objective function */
            msg = "Objective function";
            write_lpcomment(userhandle, write_modeldata, ref msg, (bool)(ptr != null));
            if (lp.is_maxim(lp))
            {
                msg = "msx: ";
                write_data(userhandle, write_modeldata, ref msg);
            }
            else
            {
                msg = "min: ";
                write_data(userhandle, write_modeldata, ref msg);
            }

            /* NOT REQUIRED
            allocREAL(lp, val, 1 + lp.columns, 1);
            allocINT(lp, idx, 1 + lp.columns, 1);
            */
            write_lprow(lp, 0, userhandle, write_modeldata, maxlen, ref idx, ref val);
            a = lp.get_rh(lp, 0);
            if (a != 0)
            {
                msg = " %+.12g";
                write_data(userhandle, write_modeldata, ref msg, a);
            }
            msg = ";\n";
            write_data(userhandle, write_modeldata, ref msg);

            /* Write constraints */
            if (nrows > 0)
            {
                msg = "Constraints";
                write_lpcomment(userhandle, write_modeldata, ref msg, true);
            }
            for (j = 1; j <= nrows; j++)
            {
                if (((lp.names_used) && (lp.row_name[j] != null)) || (write_lprow(lp, j, userhandle, null, maxlen, ref idx, ref val) == 1))
                {
                    ptr = lp.get_row_name(lp, j);
                }
                else
                {
                    ptr = null;
                }
                if ((ptr != null) && (ptr!=""))
                {
                    msg = "%s: ";
                    write_data(userhandle, write_modeldata, ref msg, ptr);
                }
                ///#if ! SingleBoundedRowInLP
                /* Write the ranged part of the constraint, if specified */
                ///ORIGINAL CODE: if ((lp.orig_upbo[j]) && (lp.orig_upbo[j] < lp.infinite))
                if ((lp.orig_upbo[j]) != 0 && (lp.orig_upbo[j] < lp.infinite))
                {
                    if (lp_types.my_chsign(lp.is_chsign(lp, j), lp.orig_rhs[j]) == -lp.infinite)
                    {
                        msg = "-Inf %s ";
                        write_data(userhandle, write_modeldata, ref msg, (lp.is_chsign(lp, j)) ? ">=" : "<=");
                    }
                    else if (lp_types.my_chsign(lp.is_chsign(lp, j), lp.orig_rhs[j]) == lp.infinite)
                    {
                        msg = "+Inf %s ";
                        write_data(userhandle, write_modeldata, ref msg, (lp.is_chsign(lp, j)) ? ">=" : "<=");
                    }
                    else
                    {
                        msg = "%+.12g %s ";
                        write_data(userhandle, write_modeldata, ref msg, (lp.orig_upbo[j] - lp.orig_rhs[j]) * (lp.is_chsign(lp, j) ? 1.0 : -1.0) / (lp.scaling_used ? lp.scalars[j] : 1.0), (lp.is_chsign(lp, j)) ? ">=" : "<=");
                    }
                }
                ///#endif

                ///ORIGINAL CODE: if ((!write_lprow(lp, j, userhandle, write_modeldata, maxlen, ref idx, ref val)) && (ncols >= 1))
                if ((write_lprow(lp, j, userhandle, write_modeldata, maxlen, ref idx, ref val)) != 0 && (ncols >= 1))
                {
                    msg = "0 %s";
                    write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, 1));
                }

                if (lp.orig_upbo[j] == 0)
                {
                    msg = " =";
                    write_data(userhandle, write_modeldata, ref msg);
                }
                else if (lp.is_chsign(lp, j))
                {
                    msg = " >=";
                    write_data(userhandle, write_modeldata, ref msg);
                }
                else
                {
                    msg = " <=";
                    write_data(userhandle, write_modeldata, ref msg);
                }
                if (System.Math.Abs(lp.get_rh(lp, j) + lp.infinite) < 1)
                {
                    msg = " -Inf;\n";
                    write_data(userhandle, write_modeldata, ref msg);
                }
                else if (System.Math.Abs(lp.get_rh(lp, j) - lp.infinite) < 1)
                {
                    msg = " +Inf;\n";
                    write_data(userhandle, write_modeldata, ref msg);
                }
                else
                {
                    msg = " %.12g;\n";
                    write_data(userhandle, write_modeldata, ref msg, lp.get_rh(lp, j));
                }

                ///#if SingleBoundedRowInLP
                /* Write the ranged part of the constraint, if specified */
                ///ORIGINAL CODE: if ((lp.orig_upbo[j]) && (lp.orig_upbo[j] < lp.infinite))
                if ((lp.orig_upbo[j]) != 0 && (lp.orig_upbo[j] < lp.infinite))
                {
                    if (((lp.names_used) && (lp.row_name[j] != null)) || (write_lprow(lp, j, userhandle, null, maxlen, ref idx, ref val) == 1))
                    {
                        ptr = lp.get_row_name(lp, j);
                    }
                    else
                    {
                        ptr = null;
                    }
                    if ((ptr != null) && (ptr!=""))
                    {
                        msg = "%s: ";
                        write_data(userhandle, write_modeldata, ref msg, ptr);
                    }

                    ///ORIGINAL CODE: if ((!write_lprow(lp, j, userhandle, write_modeldata, maxlen, ref idx, ref val)) && (lp.get_Ncolumns(lp) >= 1))
                    if ((write_lprow(lp, j, userhandle, write_modeldata, maxlen, ref idx, ref val)) != 0 && (lp.get_Ncolumns(lp) >= 1))
                    {
                        msg = "0 %s";
                        write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, 1));
                    }
                    msg = " %s %g;\n";
                    write_data(userhandle, write_modeldata, ref msg, (lp.is_chsign(lp, j)) ? "<=" : ">=", (lp.orig_upbo[j] - lp.orig_rhs[j]) * (lp.is_chsign(lp, j) ? 1.0 : -1.0) / (lp.scaling_used ? lp.scalars[j] : 1.0));
                }
                ///#endif
            }
            /* Write bounds on variables */
            ok = false;
            for (i = nrows + 1; i <= lp.sum; i++)
            {
                if (!lp.is_splitvar(lp, i - nrows))
                {
                    if (lp.orig_lowbo[i] == lp.orig_upbo[i])
                    {
                        if (!ok)
                        {
                            msg = "Variable bounds";
                            write_lpcomment(userhandle, write_modeldata, ref msg, true);
                            ok = true;
                        }
                        msg = "%s = %.12g;\n";
                        write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, i - nrows), lp.get_upbo(lp, i - nrows));
                    }
                    else
                    {
                        ///#if ! SingleBoundedRowInLP
                        if ((lp.orig_lowbo[i] != 0) && (lp.orig_upbo[i] < lp.infinite))
                        {
                            if (!ok)
                            {
                                msg = "Variable bounds";
                                write_lpcomment(userhandle, write_modeldata, ref msg, true);
                                ok = true;
                            }
                            if (lp.orig_lowbo[i] == -lp.infinite)
                            {
                                msg = "-Inf";
                                write_data(userhandle, write_modeldata, ref msg);
                            }
                            else
                            {
                                msg = "%.12g";
                                write_data(userhandle, write_modeldata, ref msg, lp.get_lowbo(lp, i - nrows));
                            }
                            msg = " <= %s <= ";
                            write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, i - nrows));
                            if (lp.orig_lowbo[i] == lp.infinite)
                            {
                                msg = "+Inf";
                                write_data(userhandle, write_modeldata, ref msg);
                            }
                            else
                            {
                                msg = "%.12g";
                                write_data(userhandle, write_modeldata, ref msg, lp.get_upbo(lp, i - nrows));
                            }
                            msg = ";\n";
                            write_data(userhandle, write_modeldata, ref msg);
                        }
                        else
                        ///#endif
                        {
                            if (lp.orig_lowbo[i] != 0)
                            {
                                if (!ok)
                                {
                                    msg = "Variable bounds";
                                    write_lpcomment(userhandle, write_modeldata, ref msg, true);
                                    ok = true;
                                }
                                if (lp.orig_lowbo[i] == -lp.infinite)
                                {
                                    msg = "%s >= -Inf;\n";
                                    write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, i - nrows));
                                }
                                else if (lp.orig_lowbo[i] == lp.infinite)
                                {
                                    msg = "%s >= +Inf;\n";
                                    write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, i - nrows));
                                }
                                else
                                {
                                    msg = "%s >= %.12g;\n";
                                    write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, i - nrows), lp.get_lowbo(lp, i - nrows));
                                }
                            }
                            if (lp.orig_upbo[i] != lp.infinite)
                            {
                                if (!ok)
                                {
                                    msg = "Variable bounds";
                                    write_lpcomment(userhandle, write_modeldata, ref msg, true);
                                    ok = true;
                                }
                                msg = "%s <= %.12g;\n";
                                write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, i - nrows), lp.get_upbo(lp, i - nrows));
                            }
                        }
                    }
                }
            }

            /* Write optional integer section */
            if (lp.int_vars > 0)
            {
                msg = "Integer definitions";
                write_lpcomment(userhandle, write_modeldata, ref msg, true);
                i = 1;
                while ((i <= ncols) && !lp.is_int(lp, i))
                {
                    i++;
                }
                if (i <= ncols)
                {
                    msg = "int %s";
                    nchars = write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, i));
                    i++;
                    for (; i <= ncols; i++)
                    {
                        if ((!lp.is_splitvar(lp, i)) && (lp.is_int(lp, i)))
                        {
                            if ((maxlen != 0) && (nchars > maxlen))
                            {
                                msg = "%s";
                                write_data(userhandle, write_modeldata, ref msg, "\n");
                                nchars = 0;
                            }
                            msg = ",%s";
                            write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, i));
                        }
                    }
                    msg = ";\n";
                    write_data(userhandle, write_modeldata, ref msg);
                }
            }

            /* Write optional SEC section */
            if (lp.sc_vars > 0)
            {
                msg = "Semi-continuous variables";
                write_lpcomment(userhandle, write_modeldata, ref msg, true);
                i = 1;
                while ((i <= ncols) && !lp.is_semicont(lp, i))
                {
                    i++;
                }
                if (i <= ncols)
                {
                    msg = "sec %s";
                    nchars = write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, i));
                    i++;
                    for (; i <= ncols; i++)
                    {
                        if ((!lp.is_splitvar(lp, i)) && (lp.is_semicont(lp, i)))
                        {
                            if ((maxlen != 0) && (nchars > maxlen))
                            {
                                msg = "%s";
                                write_data(userhandle, write_modeldata, ref msg, "\n");
                                nchars = 0;
                            }
                            msg = ",%s";
                            nchars += write_data(userhandle, write_modeldata, ref msg, lp.get_col_name(lp, i));
                        }
                    }
                    msg = ";\n";
                    write_data(userhandle, write_modeldata, ref msg);
                }
            }

            /* Write optional SOS section */
            if (lp.SOS_count(lp) > 0)
            {
                SOSgroup SOS = lp.SOS;
                msg = "SOS definitions";
                write_lpcomment(userhandle, write_modeldata, ref msg, true);
                msg = "SOS\n";
                write_data(userhandle, write_modeldata, ref msg);
                for (b = 0, i = 0; i < SOS.sos_count; b = SOS.sos_list[i].priority, i++)
                {
                    msg = "%s: ";
                    nchars = write_data(userhandle, write_modeldata, ref msg, (SOS.sos_list[i].name == null) || (SOS.sos_list[i].name == "") ? "SOS" : SOS.sos_list[i].name); // formatnumber12((double) lp->sos_list[i]->priority)

                    for (a = 0.0, j = 1; j <= SOS.sos_list[i].size; a = SOS.sos_list[i].weights[j], j++)
                    {
                        if ((maxlen != 0) && (nchars > maxlen))
                        {
                            msg = "%s";
                            write_data(userhandle, write_modeldata, ref msg, "\n");
                            nchars = 0;
                        }
                        if (SOS.sos_list[i].weights[j] == ++a)
                        {
                            msg = "%s%s";
                            nchars += write_data(userhandle, write_modeldata, ref msg, (j > 1) ? "," : "", lp.get_col_name(lp, SOS.sos_list[i].members[j]));
                        }
                        else
                        {
                            msg = "%s%s:%.12g";
                            nchars += write_data(userhandle, write_modeldata, ref msg, (j > 1) ? "," : "", lp.get_col_name(lp, SOS.sos_list[i].members[j]), SOS.sos_list[i].weights[j]);
                        }
                    }
                    if (SOS.sos_list[i].priority == ++b)
                    {
                        msg = " <= %d;\n";
                        nchars += write_data(userhandle, write_modeldata, ref msg, SOS.sos_list[i].type);
                    }
                    else
                    {
                        msg = " <= %d:%d;\n";
                        nchars += write_data(userhandle, write_modeldata, ref msg, SOS.sos_list[i].type, SOS.sos_list[i].priority);
                    }
                }
            }

           // FREE(val);
           // FREE(idx);

            ok = true;

            return (ok);
        }

        internal static int write_lpdata(Object userhandle, ref string buf)
        {
            return (fprintf((FILE)userhandle, "%s", buf));
        }

        internal bool LP_writefile(lprec lp, ref string filename)
        {
            FILE output = stdout;
            bool ok = new bool();

            if (filename != null)
            {
                //ORIGINAL LINE: ok = (MYBOOL)((output = fopen(filename, "w")) != null);
                ok = ((output = fopen(filename, "w")) != null);
                if (ok == null)
                {
                    return (ok);
                }
            }
            else
            {
                output = lp.outstream;
            }

            ok = write_lpex(lp, (Object)output, write_lpdata);

            if (filename != null)
            {
                fclose(output);
            }

            return (ok);

        }

        internal bool LP_writehandle(lprec lp, FILE output)
        {
            bool ok = new bool();

            if (output != null)
            {
               lp.set_outputstream(lp, output);
            }

            output = lp.outstream;

            ok = write_lpex(lp, (Object)output, write_lpdata);

            return (ok);
        }

    }
}
