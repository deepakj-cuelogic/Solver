using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public class FILE
    {
        public object _Placeholder;
    }

    public class lp_MPS
    {
        public const string ROWNAMEMASK = "R%d";
        public const string ROWNAMEMASK2 = "r%d";
        public const string COLNAMEMASK = "C%d";
        public const string COLNAMEMASK2 = "c%d";
        /* Read an MPS file */
        public byte MPS_readfile(lprec[] newlp, ref string filename, int typeMPS, int verbose)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO
        /// </summary>
        //public byte __WINAPI MPS_readhandle(lprec[] newlp, FILE filehandle, int typeMPS, int verbose)
        //{
        //    throw new NotImplementedException();
        //}

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
