using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public static class lp_simplex
    {
        public static int primloop(lprec lp, byte primalfeasible, double primaloffset)
        {
            throw new NotImplementedException();
        }
        public static int dualloop(lprec lp, byte dualfeasible, int[] dualinfeasibles, double dualoffset)
        {
            throw new NotImplementedException();
        }
        public static int spx_run(lprec lp, byte validInvB)
        {
            throw new NotImplementedException();
        }
        public static int spx_solve(lprec lp)
        {
            throw new NotImplementedException();
        }
        public static int lag_solve(lprec lp, double start_bound, int num_iter)
        {
            throw new NotImplementedException();
        }
        public static int heuristics(lprec lp, int mode)
        {
            throw new NotImplementedException();
        }
        public static int lin_solve(lprec lp)
        {
            throw new NotImplementedException();
        }
    }
}