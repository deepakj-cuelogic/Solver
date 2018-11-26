﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class lusol
    {
        internal int LUSOL_btran(LUSOLrec LUSOL, double[] b, int[] NZidx)
        {
            int inform = 0;
            lusol6a objlusol6a = new lusol6a();

            /* Copy RHS vector, but make adjustment for offset since this
               can create a memory error when the calling program uses
               a 0-base vector offset back to comply with LUSOL. */
            //NOT REQUIRED
            //MEMCOPY(LUSOL.w + 1, b + 1, LUSOL.m);
            if (LUSOL.w != null)
            {
                LUSOL.w[0] = 0;
            }

            objlusol6a.LU6SOL(LUSOL, commonlib.LUSOL_SOLVE_Atv_w, b, LUSOL.w, NZidx, ref inform);
            LUSOL.luparm[INVrec.LUSOL_IP_BTRANCOUNT]++;

            return (inform);
        }

       
        internal String LUSOL_informstr(LUSOLrec LUSOL, int inform)
        {

            String[] LUSOL_informstr_informText = { "LUSOL_RANKLOSS: Lost rank", "LUSOL_LUSUCCESS: Success", "LUSOL_LUSINGULAR: Singular A", "LUSOL_LUUNSTABLE: Unstable factorization", "LUSOL_ADIMERR: Row or column count exceeded", "LUSOL_ADUPLICATE: Duplicate A matrix entry found", "(Undefined message)", "(Undefined message)", "LUSOL_ANEEDMEM: Insufficient memory for factorization", "LUSOL_FATALERR: Fatal internal error", "LUSOL_NOPIVOT: Found no suitable pivot", "LUSOL_NOMEMLEFT: Could not obtain more memory" };

            if ((inform < commonlib.LUSOL_INFORM_MIN) || (inform > commonlib.LUSOL_INFORM_MAX))
            {
                inform = LUSOL.luparm[commonlib.LUSOL_IP_INFORM];
            }
            return (LUSOL_informstr_informText[inform - commonlib.LUSOL_INFORM_MIN]);
        }

    }
}
