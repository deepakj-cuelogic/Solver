using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public delegate int scan_lineDelegate(lprec lp, int section, ref string line, ref string field1, ref string field2, ref string field3, ref double field4, ref string field5, ref double field6);
    
    public class FILE
    {
        public object _Placeholder;
    }

    public class lp_MPS
    {
        scan_lineDelegate scan_line;

        public const string ROWNAMEMASK = "R%d";
        public const string ROWNAMEMASK2 = "r%d";
        public const string COLNAMEMASK = "C%d";
        public const string COLNAMEMASK2 = "c%d";
        /* Read an MPS file */
        public bool MPS_readfile(lprec[] newlp, ref string filename, int typeMPS, int verbose)
        {
            bool status = false;

            FileStream fpin = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read);
            if (fpin != null)
            {
                status = MPS_readhandle(newlp, fpin, typeMPS, verbose);
                fclose(fpin);
            }
            return (status);
        }

        private bool MPS_readhandle(lprec[] newlp, FileStream filehandle, int typeMPS, int verbose)
        {
            return (MPS_readex(newlp, (object)filehandle, MPS_input, typeMPS, verbose));
        }
        private bool MPS_readex(lprec[] newlp, object userhandle, lp_lib.read_modeldata_func read_modeldata, int typeMPS, int verbose)
        {
            string field1 = new string(new char[lp_lib.BUFSIZ]);
            string field2 = new string(new char[lp_lib.BUFSIZ]);
            string field3 = new string(new char[lp_lib.BUFSIZ]);
            string field5 = new string(new char[lp_lib.BUFSIZ]);
            string line = new string(new char[lp_lib.BUFSIZ]);
            string tmp = new string(new char[lp_lib.BUFSIZ]);
            string Last_col_name = new string(new char[lp_lib.BUFSIZ]);
            string probname = new string(new char[lp_lib.BUFSIZ]);
            string OBJNAME = new string(new char[lp_lib.BUFSIZ]);
            string ptr;
            int items;
            int row;
            int Lineno;
            int @var;
            int section = lp_lib.MPSUNDEF;
            int variant = 0;
            int NZ = 0;
            int SOS = 0;
            byte Int_section;
            byte Column_ready;
            byte Column_ready1;
            byte Unconstrained_rows_found = 0;
            byte OF_found = 0;
            bool CompleteStatus = false;
            double field4;
            double field6;
            //ORIGINAL LINE: double *Last_column = null;
            double? Last_column = null;
            int count = 0;
            int? Last_columnno = 0;
            int OBJSENSE = lp_lib.ROWTYPE_EMPTY;
            lprec lp;
            lp_lib lpLib = new lp_lib();


            if (newlp == null)
                return (CompleteStatus);
            else if (newlp[0] == null)
                lp = lpLib.make_lp(0, 0);
            else
                lp = newlp[0];
        }
        
        private static int MPS_input(object fpin, ref string buf, int max_size)
        {
            //ORIGINAL LINE: return (fgets(buf, max_size, (FILE*)fpin) != NULL);
            int count = 0;
            using (StreamReader reader = File.OpenText("foo.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    count++;
                }
            }
            return count;
        }

        /* Write a MPS file to output */
        public byte MPS_writefile(lprec lp, int typeMPS, ref string filename)
        {
            throw new NotImplementedException();
        }
        public byte MPS_writehandle(lprec lp, int typeMPS, FILE output)
        {
            throw new NotImplementedException();
        }

        /* Read and write BAS files */
        public byte MPS_readBAS(lprec lp, int typeMPS, ref string filename, ref string info)
        {
            throw new NotImplementedException();
        }
        public byte MPS_writeBAS(lprec lp, int typeMPS, ref string filename)
        {
            throw new NotImplementedException();
        }

    }
}
