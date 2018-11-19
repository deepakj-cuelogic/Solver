using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public static class lp_simplex
    {
        public const int OBJ_STEPS = 5;
        public const int PRICE_ADAPTIVE = 32;
        public const int MAX_STALLCOUNT = 12;
        public const int PRICER_DEVEX = 2;
        public const int MAX_RULESWITCH = 5;
        public const double epsprimal = 0;

        internal static void stallMonitor_update(lprec lp, double newOF)
        {
            int newpos;
            OBJmonrec monitor = lp.monitor;

            if (monitor.countstep < OBJ_STEPS)
            {
                monitor.countstep++;
            }
            else
            {
                monitor.startstep = commonlib.mod(monitor.startstep + 1, OBJ_STEPS);
            }
            newpos = commonlib.mod(monitor.startstep + monitor.countstep - 1, OBJ_STEPS);
            monitor.objstep[newpos] = newOF;
            monitor.idxstep[newpos] = monitor.Icount;
            monitor.currentstep = newpos;
        }

        internal static bool stallMonitor_creepingObj(lprec lp)
        {
            OBJmonrec monitor = lp.monitor;

            if (monitor.countstep > 1)
            {
                double deltaOF = (monitor.objstep[monitor.currentstep] - monitor.objstep[monitor.startstep]) / monitor.countstep;
                deltaOF /= commonlib.MAX(1, (monitor.idxstep[monitor.currentstep] - monitor.idxstep[monitor.startstep]));
                deltaOF = lp_types.my_chsign(monitor.isdual, deltaOF);
                return ((bool)(deltaOF < monitor.epsvalue));
            }
            else
            {
                return (false);
            }
        }

        internal static bool stallMonitor_shortSteps(lprec lp)
        {
            OBJmonrec monitor = lp.monitor;

            if (monitor.countstep == OBJ_STEPS)
            {
                double deltaOF = commonlib.MAX(1, (monitor.idxstep[monitor.currentstep] - monitor.idxstep[monitor.startstep])) / monitor.countstep;
                deltaOF = System.Math.Pow(deltaOF * OBJ_STEPS, 0.66);
                return ((bool)(deltaOF > monitor.limitstall[1]));
            }
            else
            {
                return (false);
            }
        }

        internal static void stallMonitor_reset(lprec lp)
        {
            OBJmonrec monitor = lp.monitor;

            monitor.ruleswitches = 0;
            monitor.Ncycle = 0;
            monitor.Mcycle = 0;
            monitor.Icount = 0;
            monitor.startstep = 0;
            monitor.objstep[monitor.startstep] = lp.infinite;
            monitor.idxstep[monitor.startstep] = monitor.Icount;
            monitor.prevobj = 0;
            monitor.countstep = 1;
        }

        internal static bool stallMonitor_create(lprec lp, bool isdual, ref string funcname)
        {
            OBJmonrec monitor = null;
            double suminfeas;
            if (lp.monitor != null)
            {
                return (false);
            }

            ///PREVIOUS CODE: monitor = (OBJmonrec)calloc(sizeof(monitor), 1);
            monitor = new OBJmonrec();
            if (monitor == null)
            {
                return (false);
            }

            monitor.lp = lp;
            monitor.spxfunc = funcname;
            monitor.isdual = isdual;
            monitor.pivdynamic = lp.is_piv_mode(lp, PRICE_ADAPTIVE);
            monitor.oldpivstrategy = lp.piv_strategy;
            monitor.oldpivrule = lp.get_piv_rule(lp);
            if (MAX_STALLCOUNT <= 1)
            {
                monitor.limitstall[0] = 0;
            }
            else
            {
                monitor.limitstall[0] = Convert.ToInt32(commonlib.MAX(MAX_STALLCOUNT, (int)System.Math.Pow((double)(lp.rows + lp.columns) / 2, 0.667)));
            }
            
            ///#if 1
            monitor.limitstall[0] *= 2 + 2; // Expand degeneracy/stalling tolerance range
                                            ///#endif
            monitor.limitstall[1] = monitor.limitstall[0];
            if (monitor.oldpivrule == PRICER_DEVEX) // Increase tolerance since primal Steepest Edge is expensive
            {
                monitor.limitstall[1] *= 2;
            }
            if (MAX_RULESWITCH <= 0)
            {
                ///NOT Required
                ///monitor.limitruleswitches = MAXINT32;
            }
            else
            {
                monitor.limitruleswitches = Convert.ToInt32(commonlib.MAX(MAX_RULESWITCH, lp.rows / MAX_RULESWITCH));
            }
            monitor.epsvalue = epsprimal; // lp->epsvalue;
            lp.monitor = monitor;
            stallMonitor_reset(lp);
            suminfeas = lp.infinite;
            return (true);
        }


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