using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZS.Math.Optimization
{
    public class lp_report
    {
        /* General information functions */
        public string explain(lprec lp, ref string format, params object[] LegacyParamArray)
        {
            throw new NotImplementedException();
        }
        public void report(lprec lp, int level, ref string format, params object[] LegacyParamArray)
        {
            throw new NotImplementedException();
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
        public void blockWriteLREAL(FILE output, ref string label, double vector, int first, int last)
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
        public void REPORT_extended(lprec lp)
        {
            throw new NotImplementedException();
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
