using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public class lp_LUSOL
    {


        public const int LU_START_SIZE = 10000; // Start size of LU; realloc'ed if needed
        public const int DEF_MAXPIVOT = 250; // Maximum number of pivots before refactorization
        public const double MAX_DELTAFILLIN = 2.0; // Do refactorizations based on sparsity considerations
        public const int TIGHTENAFTER = 10; // Tighten LU pivot criteria only after this number of singularities

        /* MUST MODIFY */
        //NOTED ISSUE: BFP_CALLMODEL
        //ORIGINAL LINE: internal BFP_CALLMODEL void bfp_btran_normal(lprec lp, ref double[] pcol, ref int[] nzidx)
        //Temporary BFP_CALLMODEL excluded
        internal void bfp_btran_normal(lprec lp, ref double pcol, ref int[] nzidx)
        {
            int i;
            INVrec lu;
            string msg;
            lu = lp.invB;
            lusol objlusol = new lusol();
            LpBFP1 objLpBFP1 = new LpBFP1();

            /* Do the LUSOL btran */
            //NOTED ISSUE
            double[] b = new double[1];
            b[0] = pcol[0] - Convert.ToDouble(LpBFP1.bfp_rowoffset(lp));
            i = objlusol.LUSOL_btran(lu.LUSOL, b, nzidx);
            if (i != commonlib.LUSOL_INFORM_LUSUCCESS)
            {
                msg = "bfp_btran_normal: Failed at iter %.0f, pivot %d;\n%s\n";
                lu.status = commonlib.BFP_STATUS_ERROR;
                lp.report(lp, lp_lib.NORMAL, ref msg, (double)(lp.total_iter + lp.current_iter), lu.num_pivots, objlusol.LUSOL_informstr(lu.LUSOL, i));
            }

            /* Check performance data */
            //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
            ///#if false
            //  if(lu->num_pivots == 1) {
            //    if(lu->LUSOL->luparm[LUSOL_IP_ACCELERATION] > 0)
            //      lp->report(lp, NORMAL, "RowL0 R:%10.7f  C:%10.7f  NZ:%10.7f\n",
            //                             (double) lu->LUSOL->luparm[LUSOL_IP_ROWCOUNT_L0] / lu->LUSOL->m,
            //                             (double) lu->LUSOL->luparm[LUSOL_IP_COLCOUNT_L0] / lu->LUSOL->m,
            //                             (double) lu->LUSOL->luparm[LUSOL_IP_NONZEROS_L0] / pow((double) lu->LUSOL->m, 2));
            //    else
            //      lp->report(lp, NORMAL, "ColL0 C:%10.7f  NZ:%10.7f\n",
            //                             (double) lu->LUSOL->luparm[LUSOL_IP_COLCOUNT_L0] / lu->LUSOL->m,
            //                             (double) lu->LUSOL->luparm[LUSOL_IP_NONZEROS_L0] / pow((double) lu->LUSOL->m, 2));
            //  }
            ///#endif

        }

        internal static bool is_fixedvar(lprec lp, int variable)
        {
            if ((lp.bb_bounds != null && lp.bb_bounds.UBzerobased) || (variable <= lp.rows))
            {
                return ((bool)(lp.upbo[variable] < lprec.epsprimal));
            }
            else
            {
                return ((bool)(lp.upbo[variable] - lp.lowbo[variable] < lprec.epsprimal));
            }
            if (!myblas.is_nativeBLAS())
            {
                myblas.unload_BLAS();
            }
            //NOT REQUIRED
            //LUSOL_FREE(LUSOL);
        }



        // MUST MODIFY 
        internal void bfp_free(lprec lp)    //BFP_CALLMODEL
        {
            INVrec lu;

            lu = lp.invB;
            if (lu == null)
            {
                return;
            }

            /*NOT REQUIRED
            // General arrays 
            FREE(lu.opts);
            FREE(lu.value);

            // Data specific to the factorization engine 
            LUSOL_free(lu.LUSOL);

            FREE(lu);
            */
            lp.invB = null;
        }

        internal static int bfp_factorize(lprec lp, int uservars, int Bsize, bool? usedpos, bool? final)
        {
            int kcol;
            int inform;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *rownum = null;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'int rownum' to 'int[][] rownum'; need to check at run time
            int[][] rownum = null;
            int singularities = 0;
            int dimsize = lp.invB.dimcount;
            LUSOLrec LUSOL = lp.invB.LUSOL;

            /* Set dimensions and create work array */
            commonlib.SETMAX(lp.invB.max_Bsize, Bsize + (1 + lp.rows - uservars));
            kcol = lp.invB.dimcount;
            LUSOL.m = kcol;
            LUSOL.n = kcol;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18; need to check at run time
            lp_utils.allocINT(lp, rownum, kcol + 1, 0);

            /* Check if the refactorization frequency is low;
               tighten pivot thresholds if appropriate */
            inform = LpBFP1.bfp_pivotcount(lp);
            if (final == null && !lp.invB.force_refact && !lp.is_action(lp.spx_action, lp_lib.ACTION_TIMEDREINVERT) && (inform > 5) && (inform < 0.25 * lp.bfp_pivotmax(lp)))
            {
                bfp_LUSOLtighten(lp);
            }

            int? singular = null;
            /* Reload B and factorize */
            inform = bfp_LUSOLfactorize(lp, ref usedpos, ref rownum, ref singular);

            /* Do some checks */
#if Paranoia
  if (uservars != lp.invB.user_colcount)
  {
	lp.report(lp, SEVERE, "bfp_factorize: User variable count reconciliation failed\n");
	return (singularities);
  }
#endif

            /* Check result and do further remedial action if necessary */
            if (inform != lusol.LUSOL_INFORM_LUSUCCESS)
            {
                int singularcols;
                int replacedcols = 0;
                double hold = new double();

                /* Make sure we do not tighten factorization pivot criteria too often, and simply
                   accept the substitution of slack columns into the basis */
                if ((lp.invB.num_singular + 1) % TIGHTENAFTER == 0)
                {
                    bfp_LUSOLtighten(lp);
                }

                /* Try to restore a non-singular basis by substituting singular columns with slacks */
                while ((inform == lusol.LUSOL_INFORM_LUSINGULAR) && (replacedcols < dimsize))
                {
                    int iLeave;
                    int jLeave;
                    int iEnter;
                    bool isfixed;

                    singularities++;
                    singularcols = LUSOL.luparm[lusol.LUSOL_IP_SINGULARITIES];
                    hold = (double)lp.get_total_iter(lp);
                    string msg = "bfp_factorize: Resolving {0} singularit{1} at refact {2}, iter %.0f\n";
                    lp.report(lp, lp_lib.NORMAL, ref msg, singularcols, lp_types.my_plural_y(singularcols), lp.invB.num_refact, hold);

                    /* Find the failing / singular column(s) and make slack substitutions */
                    for (kcol = 1; kcol <= singularcols; kcol++)
                    {

                        /* Determine leaving and entering columns. */
                        iLeave = lusol.LUSOL_getSingularity(LUSOL, kcol); // This is the singular column as natural index
                        iEnter = iLeave; // This is the target replacement slack
#if true
                        iEnter = LUSOL.iqinv[iEnter];
                        iEnter = LUSOL.ip[iEnter];
#endif
                        iLeave -= LpBFP1.bfp_rowextra(lp); // This is the original B column/basis index
                        jLeave = lp.var_basic[iLeave]; // This is the IA column index in lp_solve

                        /* Express the slack index in original lp_solve [1..rows] reference and check validity */
                        /*       if(B4 != NULL) iEnter = B4->B4_row[iEnter]; v6 FUNCTIONALITY */
                        iEnter -= LpBFP1.bfp_rowextra(lp);
                        if (lp.is_basic[iEnter])
                        {
                            msg = "bfp_factorize: Replacement slack {0} is already basic!\n";
                            lp.report(lp, lp_lib.DETAILED, ref msg, iEnter);

                            /* See if we can find a good alternative slack variable to enter */
                            iEnter = 0;
                            for (inform = 1; inform <= lp.rows; inform++)
                            {
                                if (!lp.is_basic[inform])
                                {
                                    if ((iEnter == 0) || (lp.upbo[inform] > lp.upbo[iEnter]))
                                    {
                                        iEnter = inform;
                                        if (lp_types.my_infinite(lp, lp.upbo[iEnter]))
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            if (iEnter == 0)
                            {
                                msg = "bfp_factorize: Could not find replacement slack variable!\n";
                                lp.report(lp, lp_lib.SEVERE, ref msg);
                                break;
                            }
                        }

                        /* We should update bound states for both the entering and leaving variables.
                           Note that this may cause (primal or dual) infeasibility, but I assume that
                           lp_solve traps this and takes necessary corrective action. */
                        isfixed = is_fixedvar(lp, iEnter);
                        if (isfixed)
                        {
                            lp.fixedvars++;
                        }
                        hold = lp.upbo[jLeave];
                        lp.is_lower[jLeave] = isfixed || (System.Math.Abs(hold) >= lp.infinite) || (lp.rhs[iLeave] < hold);
                        lp.is_lower[iEnter] = true;

                        /* Do the basis replacement */
                        lp.set_basisvar(lp, iLeave, iEnter);

                    }

                    /* Refactorize with slack substitutions */
                    usedpos = null;
                    singular = null;
                    inform = bfp_LUSOLfactorize(lp, ref usedpos, ref rownum, ref singular);
                    replacedcols += singularcols;
                }

                /* Check if we had a fundamental problem */
                if (singularities >= dimsize)
                {
                    string msg = "bfp_factorize: LUSOL was unable to recover from a singular basis\n";
                    lp.report(lp, lp_lib.IMPORTANT, ref msg);
                    lp.spx_status = lp_lib.NUMFAILURE;
                }
            }

            /* NOT REQUIRED
            // Clean up before returning 
            FREE(rownum);
            */

            /* Update statistics */
            /* SETMAX(lp->invB->max_Bsize, (*Bsize)); */
            lp.invB.num_singular += singularities; // The total number of singular updates

            return (singularities);
        }

        private static void bfp_LUSOLtighten(lprec lp)
        {
            int infolevel = lp_lib.DETAILED;
            string msg = "";

            switch (lusol.LUSOL_tightenpivot(lp.invB.LUSOL))
            {
                case 0:
                    msg = "bfp_factorize: Very hard numerics, but cannot tighten LUSOL thresholds further.\n";
                    lp.report(lp, infolevel, ref msg);
                    break;
                case 1:
                    msg = "bfp_factorize: Frequent refact pivot count %d at iter %.0f; tightened thresholds.\n";
                    lp.report(lp, infolevel, ref msg, lp.invB.num_pivots, (double)lp.get_total_iter(lp));
                    break;
                default:
                    msg = "bfp_factorize: LUSOL switched to %s pivoting model to enhance stability.\n";
                    lp.report(lp, infolevel, ref msg, lusol.LUSOL_pivotLabel(lp.invB.LUSOL));
                    break;
            }

        }

        /* LOCAL HELPER ROUTINE */
        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
        //changed from 'ref int rownum' to 'ref int[][] rownum'; need to check at run time
        private static int bfp_LUSOLfactorize(lprec lp, ref bool? usedpos, ref int[][] rownum, ref int? singular)
        {
            int i;
            int j;
            int nz;
            int deltarows = LpBFP1.bfp_rowoffset(lp);
            INVrec invB = lp.invB;

            /* Handle normal, presumed nonsingular case */
            if (singular == null)
            {

                /* Optionally do a symbolic minimum degree ordering;
                   not that slack variables should not be processed */
                /*#define UsePreprocessMDO*/
#if UsePreprocessMDO
	int[] mdo;
	mdo = LpBFP1.bfp_createMDO(lp, usedpos, lp.rows, 1);
	if (mdo != null)
	{
	  for (i = 1; i <= lp.rows; i++)
	  {
		LpCls.set_basisvar(lp, i, mdo[i]);
	  }
	  FREE(mdo);
	}
#endif

                /* Reset the factorization engine */
                lusol.LUSOL_clear(invB.LUSOL, true);

                /* Add the basis columns in the original order */
                for (i = 1; i <= invB.dimcount; i++)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //changed from 'rownum' to 'rownum[0][0]'; need to check at run time
                    nz = LpCls.get_basiscolumn(lp, i, rownum[0][0], invB.value);
                    lusol.LUSOL_loadColumn(invB.LUSOL, rownum, i, invB.value, nz, 0);
                    if ((i > deltarows) && (lp.var_basic[i - deltarows] > lp.rows))
                    {
                        lp.invB.user_colcount++;
                    }
                }

                /* Factorize */
                i = lusol.LUSOL_factorize(invB.LUSOL);
            }

            /* Handle case where a column may be singular */
            else
            {
                LLrec map = null;

                /* Reset the factorization engine */
                i = bfp_LUSOLidentity(lp, rownum);

                usedpos = null;
                /* Build map of available columns */
                nz = lp_utils.createLink(lp.rows, map, ref usedpos);
                for (i = 1; i <= lp.rows; i++)
                {
                    if (lp.var_basic[i] <= lp.rows)
                    {
                        lp_utils.removeLink(map, i);
                    }
                }

                /* Rebuild the basis, column by column, while skipping slack columns */
                j = lp_utils.firstActiveLink(map);
                for (i = 1; i <= lp.rows; i++)
                {
                    if (lp.var_basic[i] <= lp.rows)
                    {
                        continue;
                    }
                    nz = bfp_LUSOLsetcolumn(lp, j + deltarows, lp.var_basic[i]);
                    if (nz == lusol.LUSOL_INFORM_LUSUCCESS)
                    {
                        lp.invB.user_colcount++;
                    }
                    else
                    {
                        nz = bfp_LUSOLsetcolumn(lp, j + deltarows, i);
                        lp.set_basisvar(lp, i, i);
                    }
                    j = lp_utils.nextActiveLink(map, j);
                }

                /* Sort the basis list */
                /*NOT REQUIRED
                MEMCOPY(rownum, lp.var_basic, lp.rows + 1);
                */
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //changed from 'rownum' to 'rownum[0][0]'; need to check at run time
                commonlib.sortByINT(ref lp.var_basic[0], ref rownum[0][0], lp.rows, 1, 1);
            }
            return (i);
        }

        /* LOCAL HELPER ROUTINE - Replace a basis column with corresponding slack */
        private static int bfp_LUSOLsetcolumn(lprec lp, int posnr, int colnr)
        {
            throw new NotImplementedException();
        }

            /* LOCAL HELPER ROUTINE - force the basis to be the identity matrix */
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            // changed from 'int rownum' to 'int[][] rownum'
            static int bfp_LUSOLidentity(lprec lp, int[][] rownum)
        {
            int i;
            int nz;
            INVrec invB = lp.invB;

            /* Reset the factorization engine */
            lusol.LUSOL_clear(invB.LUSOL, true);

            /* Add the basis columns */
            lp.invB.set_Bidentity = 1;
            for (i = 1; i <= invB.dimcount; i++)
            {
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                // changed from 'rownum' to 'rownum[0][0]'
                nz = LpCls.get_basiscolumn(lp, i, rownum[0][0], invB.value);
                lusol.LUSOL_loadColumn(invB.LUSOL, rownum, i, invB.value, nz, 0);
            }
            lp.invB.set_Bidentity = 0;

            /* Factorize */
            i = lusol.LUSOL_factorize(invB.LUSOL);

            return (i);
        }

        internal bool LUSOL_tightenpivot(LUSOLrec LUSOL)
        {
            throw new NotImplementedException();
        }
    }
        /* typedef */
        public class INVrec
        {

            public const int LUSOL_RP_SMARTRATIO = 0;
            public const int LUSOL_RP_FACTORMAX_Lij = 1;
            public const int LUSOL_RP_UPDATEMAX_Lij = 2;
            public const int LUSOL_RP_ZEROTOLERANCE = 3;
            public const int LUSOL_RP_SMALLDIAG_U = 4;
            public const int LUSOL_RP_EPSDIAG_U = 5;
            public const int LUSOL_RP_COMPSPACE_U = 6;
            public const int LUSOL_RP_MARKOWITZ_CONLY = 7;
            public const int LUSOL_RP_MARKOWITZ_DENSE = 8;
            public const int LUSOL_RP_GAMMA = 9;
            public const int LUSOL_RP_MAXELEM_A = 10;
            public const int LUSOL_RP_MAXMULT_L = 11;
            public const int LUSOL_RP_MAXELEM_U = 12;
            public const int LUSOL_RP_MAXELEM_DIAGU = 13;
            public const int LUSOL_RP_MINELEM_DIAGU = 14;
            public const int LUSOL_RP_MAXELEM_TCP = 15;
            public const int LUSOL_RP_GROWTHRATE = 16;
            public const int LUSOL_RP_USERDATA_1 = 17;
            public const int LUSOL_RP_USERDATA_2 = 18;
            public const int LUSOL_RP_USERDATA_3 = 19;
            public const int LUSOL_RP_RESIDUAL_U = 20;

            public const int LUSOL_IP_USERDATA_0 = 0;
            public const int LUSOL_IP_PRINTUNIT = 1;
            public const int LUSOL_IP_PRINTLEVEL = 2;
            public const int LUSOL_IP_MARKOWITZ_MAXCOL = 3;
            public const int LUSOL_IP_SCALAR_NZA = 4;
            public const int LUSOL_IP_UPDATELIMIT = 5;
            public const int LUSOL_IP_PIVOTTYPE = 6;
            public const int LUSOL_IP_ACCELERATION = 7;
            public const int LUSOL_IP_KEEPLU = 8;
            public const int LUSOL_IP_SINGULARLISTSIZE = 9;
            public const int LUSOL_IP_INFORM = 10;
            public const int LUSOL_IP_SINGULARITIES = 11;
            public const int LUSOL_IP_SINGULARINDEX = 12;
            public const int LUSOL_IP_MINIMUMLENA = 13;
            public const int LUSOL_IP_MAXLEN = 14;
            public const int LUSOL_IP_UPDATECOUNT = 15;
            public const int LUSOL_IP_RANK_U = 16;
            public const int LUSOL_IP_COLCOUNT_DENSE1 = 17;
            public const int LUSOL_IP_COLCOUNT_DENSE2 = 18;
            public const int LUSOL_IP_COLINDEX_DUMIN = 19;
            public const int LUSOL_IP_COLCOUNT_L0 = 20;
            public const int LUSOL_IP_NONZEROS_L0 = 21;
            public const int LUSOL_IP_NONZEROS_U0 = 22;
            public const int LUSOL_IP_NONZEROS_L = 23;
            public const int LUSOL_IP_NONZEROS_U = 24;
            public const int LUSOL_IP_NONZEROS_ROW = 25;
            public const int LUSOL_IP_COMPRESSIONS_LU = 26;
            public const int LUSOL_IP_MARKOWITZ_MERIT = 27;
            public const int LUSOL_IP_TRIANGROWS_U = 28;
            public const int LUSOL_IP_TRIANGROWS_L = 29;
            public const int LUSOL_IP_FTRANCOUNT = 30;
            public const int LUSOL_IP_BTRANCOUNT = 31;
            public const int LUSOL_IP_ROWCOUNT_L0 = 32;

            public int status; // Last operation status code
            public int dimcount; // The actual number of LU rows/columns
            public int dimalloc; // The allocated LU rows/columns size
            public int user_colcount; // The number of user LU columns
            public LUSOLrec LUSOL;
            public int col_enter; // The full index of the entering column
            public int col_leave; // The full index of the leaving column
            public int col_pos; // The B column to be changed at the next update using data in value[.]
                                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                                //ORIGINAL LINE: double *value;
            public double value;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: double *pcol;
            public double pcol; // Reference to the elimination vector
            public double theta_enter; // Value of the entering column theta

            public int max_Bsize; // The largest B matrix of user variables
            public int max_colcount; // The maximum number of user columns in LU
            public int max_LUsize; // The largest NZ-count of LU-files generated
            public int num_refact; // Number of times the basis was refactored
            public int num_timed_refact;
            public int num_dense_refact;
            public double time_refactstart; // Time since start of last refactorization-pivots cyle
            public double time_refactnext; // Time estimated to next refactorization
            public int num_pivots; // Number of pivots since last refactorization
            public int num_singular; // The total number of singular updates
            public string opts;
            public byte is_dirty; // Specifies if a column is incompletely processed
            public bool force_refact; // Force refactorization at the next opportunity
            public byte timed_refact; // Set if timer-driven refactorization should be active
            public byte set_Bidentity; // Force B to be the identity matrix at the next refactorization
        }
    }