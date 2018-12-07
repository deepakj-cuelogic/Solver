using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public class lp_report
    {
        /* General information functions */
        public string explain(lprec lp, ref string format, params object[] LegacyParamArray)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// changed ref string format to string format on 12/11/18
        /// </summary>
        public void report(lprec lp, int level, ref string format, params object[] LegacyParamArray)
        {
            string buff = "";        //NOT REQUIRED: = new string(new char[DEF_STRBUFSIZE + 1]);
            string ap;
            int ParamCount = -1;
            if (lp == null)
            {
                //	va_start(ap, format);
                //ORIGINAL CODE: vfprintf(stderr, format, ap);
                throw new Exception(string.Format(format, LegacyParamArray));
                //	va_end(ap);
            }
            else if (level <= lp.verbose)
            {
                if (lp.writelog != null)
                {
                    ParamCount = -1;
                    //	  va_start(ap, format);
                    //ORIGINAL CODE: vsnprintf(buff, DEF_STRBUFSIZE, format, ap);
                    string.Format(format, LegacyParamArray);
                    //	  va_end(ap);
                    lp.writelog(lp, lp.loghandle, ref buff);
                }
                if (lp.outstream != null)
                {
                    ParamCount = -1;

                    // va_start(ap, format);
                    ///<summary>
                    /// PREVIOUS: vfprintf(lp.outstream, format, ap);
                    /// ERROR IN PREVIOUS: The name 'vfprintf' does not exist in the current context
                    /// FIX 1: changed to string.Format(format, lp.outstream); 
                    /// unsure; need to check while implementing
                    /// </summary>
                    string.Format(format, lp.outstream);
                    //	  va_end(ap);
                    if (lp.outstream != corecrt_wstdio.stdout)
                    {
                        //changed from 'fflush(lp.outstream)' to 'lp.outstream.Close()' 19/11/18
                        lp.outstream.Close();
                    }
                }
            }
            /*#if xParanoia
              if (level == CRITICAL)
              {
	            raise(SIGSEGV);
              }
            #endif*/

        }

        /* Prototypes for debugging and general data dumps */
        public void debug_print(lprec lp, ref string format, params object[] LegacyParamArray)
        {
            throw new NotImplementedException();
        }
        public void debug_print_solution(lprec lp)
        {
            throw new NotImplementedException();
        }
        public void debug_print_bounds(lprec lp, ref double upbo, ref double lowbo)
        {
            throw new NotImplementedException();
        }
        public void blockWriteLREAL(FileStream output, ref string label, double vector, int first, int last)
        {
            throw new NotImplementedException();
        }
        public void blockWriteAMAT(FILE output, string label, lprec lp, int first, int last)
        {
            throw new NotImplementedException();
        }
        public void blockWriteBMAT(FILE output, string label, lprec lp, int first, int last)
        {
            throw new NotImplementedException();
        }


        /* Model reporting headers */
        public void REPORT_objective(lprec lp)
        {
            throw new NotImplementedException();
        }
        public void REPORT_solution(lprec lp, int columns)
        {
            throw new NotImplementedException();
        }
        public void REPORT_constraints(lprec lp, int columns)
        {
            throw new NotImplementedException();
        }
        public void REPORT_duals(lprec lp)
        {
            throw new NotImplementedException();
        }
        internal void REPORT_extended(lprec lp)
        {
            int i;
            int j;
            double hold = new double();
            LpCls objLpCls = new LpCls();
            string msg;
            
            //ORIGINAL LINE: double *duals, *dualsfrom, *dualstill, *objfrom, *objtill;
            double[][] duals = null;
            
            //ORIGINAL LINE: double *dualsfrom;
            double[][] dualsfrom=null;
            
            //ORIGINAL LINE: double *dualstill;
            double[][] dualstill=null;
            double[][] objfrom =null;
            double[][] objtill=null;
            bool ret;

            ret = objLpCls.get_ptr_sensitivity_obj(lp, objfrom, objtill);
            msg = " \n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = "Primal objective:\n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = " \n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = "  Column name                      Value   Objective         Min         Max\n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = "  --------------------------------------------------------------------------\n";
            report(lp, lp_lib.NORMAL, ref msg);
            for (j = 1; j <= lp.columns; j++)
            {
                hold = objLpCls.get_mat(lp, 0, j);
                msg = "  %-25s "  + lp_lib.MPSVALUEMASK + "\n";
                //NEED TO CHECK, temporary avoid 
                //ORIGINAL LINE: report(lp, lp_lib.NORMAL, ref msg, objLpCls.get_col_name(lp, j), lp_types.my_precision(hold, lprec.epsprimal), lp_types.my_precision(hold * lp.best_solution[lp.rows + j], lprec.epsprimal), lp_types.my_precision((ret) ? objfrom[j - 1] : 0.0, lprec.epsprimal), lp_types.my_precision((ret) ? objtill[j - 1] : 0.0, lprec.epsprimal));
                report(lp, lp_lib.NORMAL, ref msg, objLpCls.get_col_name(lp, j));
            }
            msg = " \n";
            report(lp, lp_lib.NORMAL, ref msg);

            ret = objLpCls.get_ptr_sensitivity_rhs(lp, duals, dualsfrom, dualstill);
            msg = "Primal variables:\n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = " \n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = "  Column name                      Value       Slack         Min         Max\n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = "  --------------------------------------------------------------------------\n";
            report(lp, lp_lib.NORMAL, ref msg);
            for (j = 1; j <= lp.columns; j++)
            {
                msg = "  %-25s " + lp_lib.MPSVALUEMASK+""+ lp_lib.MPSVALUEMASK+""+ lp_lib.MPSVALUEMASK+""+ lp_lib.MPSVALUEMASK+ "\n";
                //ORIGINAL LINE: report(lp, lp_lib.NORMAL, ref msg, objLpCls.get_col_name(lp, j), my_precision(lp.best_solution[lp.rows + j], lp.epsprimal), my_precision(my_inflimit(lp, (ret) ? duals[lp.rows + j - 1] : 0.0), lp.epsprimal), my_precision((ret) ? dualsfrom[lp.rows + j - 1] : 0.0, lp.epsprimal), my_precision((ret) ? dualstill[lp.rows + j - 1] : 0.0, lp.epsprimal));
                //Temporary some lines not considered
                report(lp, lp_lib.NORMAL, ref msg, objLpCls.get_col_name(lp, j));
            }
            msg = " \n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = "Dual variables:\n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = " \n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = "  Row name                         Value       Slack         Min         Max\n";
            report(lp, lp_lib.NORMAL, ref msg);
            msg = "  --------------------------------------------------------------------------\n";
            report(lp, lp_lib.NORMAL, ref msg);
            for (i = 1; i <= lp.rows; i++)
            {
                msg = "  %-25s " + lp_lib.MPSVALUEMASK + "" + lp_lib.MPSVALUEMASK + "" + lp_lib.MPSVALUEMASK + "" + lp_lib.MPSVALUEMASK + "\n";
                //ORIGINAL LINE: report(lp, lp_lib.NORMAL, ref msg, objLpCls.get_col_name(lp, j), my_precision(lp.best_solution[lp.rows + j], lp.epsprimal), my_precision(my_inflimit(lp, (ret) ? duals[lp.rows + j - 1] : 0.0), lp.epsprimal), my_precision((ret) ? dualsfrom[lp.rows + j - 1] : 0.0, lp.epsprimal), my_precision((ret) ? dualstill[lp.rows + j - 1] : 0.0, lp.epsprimal));
                //Temporary some lines not considered
                report(lp, lp_lib.NORMAL, ref msg, objLpCls.get_row_name(lp, i));
            }
            msg = " \n";
            report(lp, lp_lib.NORMAL, ref msg);

        }

        /* Other rarely used, but sometimes extremely useful reports */
        public void REPORT_constraintinfo(lprec lp, ref string datainfo)
        {
            throw new NotImplementedException();
        }
        public void REPORT_modelinfo(lprec lp, byte doName, ref string datainfo)
        {
            throw new NotImplementedException();
        }
        public void REPORT_lp(lprec lp)
        {
            throw new NotImplementedException();
        }
        public byte REPORT_tableau(lprec lp)
        {
            throw new NotImplementedException();
        }
        public void REPORT_scales(lprec lp)
        {
            throw new NotImplementedException();
        }
        public byte REPORT_debugdump(lprec lp, ref string filename, byte livedata)
        {
            throw new NotImplementedException();
        }
        public byte REPORT_mat_mmsave(lprec lp, ref string filename, ref int colndx, byte includeOF, ref string infotext)
        {
            throw new NotImplementedException();
        }

    }
}
