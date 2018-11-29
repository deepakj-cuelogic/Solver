#define PricerDefaultOpt
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    class LpCls : lprec

    {
        public const int PricerDefaultOpt = 1;
        /* Return lp_solve version information */
        public new void lp_solve_version(ref int? majorversion, ref int? minorversion, ref int? release, ref int? build)
        {
            if (majorversion != null)
            {
                majorversion = MAJORVERSION;
            }
            if (minorversion != null)
            {
                minorversion = MINORVERSION;
            }
            if (release != null)
            {
                release = RELEASE;
            }
            if (build != null)
            {
                build = BUILD;
            }
        }

        /* ---------------------------------------------------------------------------------- */
        /* Various interaction elements                                                       */
        /* ---------------------------------------------------------------------------------- */
        //TODO: return type can  be changed to bool??
        internal new bool userabort(lprec lp, int message)
        {
            bool abort;
            int spx_save;

            spx_save = lp.spx_status;
            lp.spx_status = RUNNING;
            if (yieldformessages(lp) != 0)
            {
                lp.spx_status = USERABORT;
                if (lp.bb_level > 0)
                {
                    lp.bb_break = 1;
                }
            }
            if ((message > 0) && (lp.usermessage != null) && ((lp.msgmask & message) != 0))
            {
                lp.usermessage(lp, lp.msghandle, message);
            }
            abort = Convert.ToBoolean(lp.spx_status != RUNNING);
            if (!abort)
            {
                lp.spx_status = spx_save;
            }
            return (abort);
        }

        private new static int yieldformessages(lprec lp)
        {
            //changed timeNow() to Convert.ToDouble(DateTime.Now)
            if ((lp.sectimeout > 0) && ((Convert.ToDouble(DateTime.Now) - lp.timestart) - (double)lp.sectimeout > 0))
            {
                lp.spx_status = TIMEOUT;
            }

            if (lp.ctrlc != null)
            {
                int retcode = lp.ctrlc(lp, lp.ctrlchandle);
                /* Check for command to restart the B&B */
                if ((retcode == ACTION_RESTART) && (lp.bb_level > 1))
                {
                    lp.bb_break = DefineConstants.AUTOMATIC;
                    retcode = 0;
                }
                return (retcode);
            }
            else
            {
                return (0);
            }
        }

        //TODO:
        //changed parameter type from 'FILE' to 'FileStream' FIX_24bc38e3-ed68-4876-b4af-8074778b3ab6 19/11/18
        private new void set_outputstream(lprec lp, FileStream stream)
        {
            /// <summary>
            /// can't identify the actual use of stdout, created a static class and declared stdout as FILE type constant
            /// will have to change the implementation accordingly
            /// </summary>
            /// <summary> FIX_4064d4fc-ad5f-457e-959a-95218bb099e1 19/11/18
            /// PREVIOUS: (lp.outstream != corecrt_wstdio.stdout)
            /// ERROR IN PREVIOUS: Operator '!=' cannot be applied to operands of type 'FileStream' and 'FILE'
            /// FIX 1: changed corecrt_wstdio.stdout type from 'FILE' to 'FileStream' 
            /// </summary>
            if ((lp.outstream != null) && (lp.outstream != corecrt_wstdio.stdout))
            {
                //TODO: find alternates for fclose & fflush if necessary
                /*if (lp.streamowned)
                {
                    fclose(lp.outstream);
                }
                else
                {
                    fflush(lp.outstream);
                }*/
            }
            if (stream == null)
            {
                lp.outstream = corecrt_wstdio.stdout;
            }
            else
            {
                //changed parameter type from 'FILE' to 'FileStream' FIX_24bc38e3-ed68-4876-b4af-8074778b3ab6 19/11/18
                lp.outstream = stream;
            }
            lp.streamowned = false;
        }

        private new bool set_outputfile(lprec lp, ref string filename)
        {
            //TODO: datatype(return type) can be changed to bool from byte?
            bool ok;
            //TODO:FILE output = stdout;
            // ORIGINAL LINE: ok = (MYBOOL)((filename == NULL) || (*filename == 0) || ((output = fopen(filename, "w")) != NULL));
            using (StreamWriter sw = new StreamWriter(filename))
            {
                ok = ((filename == null) || (filename == ""));
            };

            if (!ok)
            {
                //TODO: uncomment once stdout issue is solved
                //set_outputstream(lp, output);
                lp.streamowned = ((filename != "") && (filename != ""));
                if (true)
                    if ((filename != null) && (filename == ""))
                    {
                        lp.outstream = null;
                    }
            }
            return ok;
        }

        private new double time_elapsed(lprec lp)
        {
            if (lp.timeend > 0)
                return (lp.timeend - lp.timestart);
            else
                // ORIGINAL LINE: return (timeNow() - lp->timestart);
                return (Convert.ToDouble(DateTime.Now) - lp.timestart);
        }

        private void put_bb_nodefunc(lprec lp, lphandleint_intfunc newnode, object bbnodehandle)
        {
            lp.bb_usenode = newnode;
            lp.bb_nodehandle = bbnodehandle; // User-specified "owner process ID"
        }
        private void put_bb_branchfunc(lprec lp, lphandleint_intfunc newbranch, object bbbranchhandle)
        {
            lp.bb_usebranch = newbranch;
            lp.bb_branchhandle = bbbranchhandle; // User-specified "owner process ID"
        }
        private void put_abortfunc(lprec lp, lphandle_intfunc newctrlc, object ctrlchandle)
        {
            lp.ctrlc = newctrlc;
            lp.ctrlchandle = ctrlchandle; // User-specified "owner process ID"
        }
        private void put_logfunc(lprec lp, lphandlestr_func newlog, object loghandle)
        {
            lp.writelog = newlog;
            lp.loghandle = loghandle; // User-specified "owner process ID"
        }
        private void put_msgfunc(lprec lp, lphandleint_func newmsg, object msghandle, int mask)
        {
            lp.usermessage = newmsg;
            lp.msghandle = msghandle; // User-specified "owner process ID"
            lp.msgmask = mask;
        }

        /* ---------------------------------------------------------------------------------- */
        /* DLL exported function                                                              */
        /* ---------------------------------------------------------------------------------- */
        private new lprec[] read_MPS(ref string filename, int options)
        {
            lprec[] lp = null;
            int typeMPS;

            typeMPS = (options & ~0x07) >> 2;
            if ((typeMPS & (MPSFIXED | MPSFREE)) == 0)
            {
                typeMPS |= MPSFIXED;
            }
            lp_MPS objlp_MPS = new lp_MPS();
            if (objlp_MPS.MPS_readfile(lp, ref filename, typeMPS, options & 0x07))
            {
                return (lp);
            }
            else
            {
                return (null);
            }
        }

        internal new void set_maxim(lprec lp)
        {
            set_sense(lp, true);
        }

        internal new lprec make_lp(int rows, int columns)
        {
            lprec lp;

            if (rows < 0 || columns < 0)
            {
                return (null);
            }

            /*NOT REQUIRED IN C#
            lp = (lprec)calloc(1, sizeof(lprec));
            */
            lp = new lprec();
            if (lp == null)
            {
                return (null);
            }
            string name = "";
            set_lp_name(lp, ref name);
            lp.names_used = false;
            lp.use_row_names = 1;
            lp.use_col_names = 1;
            lp.rowcol_name = null;
            /* Do standard initializations ------------------------------------------------------------ */
            if (true)
                lp.obj_in_basis = lp_lib.DEF_OBJINBASIS;
            else
                lp.obj_in_basis = false;
            lp.verbose = lp_lib.NORMAL;
            set_callbacks(lp);
            name = "";
            set_BFP(lp, ref name);
            name = "";
            set_XLI(lp, ref name);
            //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
            if (lp_lib.libBLAS > 0)
                myblas.init_BLAS();
            //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
            if (lp_lib.libBLAS > 1)
            {
                string libnameBLAS = lp_lib.libnameBLAS;
                if (myblas.is_nativeBLAS() && !myblas.load_BLAS(ref libnameBLAS))
                {
                    /*report(lp, "make_lp: Could not load external BLAS library '%s'.\n", libnameBLAS)*/
                    ;
                }
            }
            /* Define the defaults for key user-settable values --------------------------------------- */
            reset_params(lp);
            /* Do other initializations --------------------------------------------------------------- */
            lp.source_is_file = false;
            lp.model_is_pure = true;
            lp.model_is_valid = 0;
            lp.spx_status = lp_lib.NOTRUN;
            lp.lag_status = lp_lib.NOTRUN;

            lp.workarrays = lp_utils.mempool_create(lp);
            lp.wasPreprocessed = 0;
            lp.wasPresolved = false;
            lp_presolve.presolve_createUndo(lp);

            lp.bb_varactive = null;
            lp.bb_varbranch = null;
            lp.var_priority = null;

            lp.rhsmax = 0.0;
            lp.bigM = 0.0;
            lp.bb_deltaOF = 0.0;

            lp.equalities = 0;
            lp.fixedvars = 0;
            lp.int_vars = 0;
            lp.sc_vars = 0;

            lp.sos_ints = 0;
            lp.sos_vars = 0;
            lp.sos_priority = null;

            lp.rows_alloc = 0;
            lp.columns_alloc = 0;
            lp.sum_alloc = 0;

            lp.rows = rows;
            lp.columns = columns;
            lp.sum = rows + columns;
            varmap_clear(lp);
            lp.matA = lp_matrix.mat_create(lp, rows, columns, lprec.epsvalue);
            lp.matL = null;
            lp.invB = null;
            lp.duals = null;
            lp.dualsfrom = null;
            lp.dualstill = null;
            lp.objfromvalue = null;
            lp.objfrom = null;
            lp.objtill = null;

            inc_col_space(lp, columns + 1);
            inc_row_space(lp, rows + 1);

            /* Avoid bound-checker uninitialized variable error */
            lp.orig_lowbo[0] = 0;

            lp.rootbounds = null;
            lp.bb_bounds = null;
            lp.bb_basis = null;

            lp.basis_valid = 0;
            lp.simplex_mode = SIMPLEX_DYNAMIC;
            lp.scaling_used = false;
            lp.columns_scaled = false;
            lp.P1extraDim = 0;
            lp.P1extraVal = 0.0;
            lp.bb_strongbranches = 0;
            lp.current_iter = 0;
            lp.total_iter = 0;
            lp.current_bswap = 0;
            lp.total_bswap = 0;
            lp.solutioncount = 0;
            lp.solvecount = 0;

            /*NOT REQUIRED IN C#
            allocINT(lp, lp.rejectpivot, DEF_MAXPIVOTRETRY + 1, 1);
            */

            set_minim(lp);
            set_infiniteex(lp, DEF_INFINITE, 1);

            LpPricePSE.initPricer(lp);

            /* Call-back routines by KE */
            lp.ctrlc = null;
            lp.ctrlchandle = null;
            lp.writelog = null;
            lp.loghandle = null;
            lp.debuginfo = null;
            lp.usermessage = null;
            lp.msgmask = MSG_NONE;
            lp.msghandle = null;

            lp.timecreate = Convert.ToDouble(DateTime.Now);

            return (lp);

        }

        private static void set_infiniteex(lprec lp, double infinite, byte? init)
        {
            int i;

            infinite = System.Math.Abs(infinite);
            if ((init) != null || is_infinite(lp, lp.bb_heuristicOF))
            {
                lp.bb_heuristicOF = lp_types.my_chsign(is_maxim(lp), infinite);
            }
            if ((init) != null || is_infinite(lp, lp.bb_breakOF))
            {
                lp.bb_breakOF = lp_types.my_chsign(is_maxim(lp), -infinite);
            }
            for (i = 0; i <= lp.sum; i++)
            {
                if ((init == null) && is_infinite(lp, lp.orig_lowbo[i]))
                {
                    lp.orig_lowbo[i] = -infinite;
                }
                if ((init) != null || is_infinite(lp, lp.orig_upbo[i]))
                {
                    lp.orig_upbo[i] = infinite;
                }
            }
            lp.infinite = infinite;

        }

        public new bool set_lp_name(lprec lp, ref string name)
        {
            if (name == "")
                lp.lp_name = null;
            else
                lp.lp_name = name;
            return true;
        }

        /* Write and read lp_solve parameters (placeholders) - see lp_params.c */
        private new void reset_params(lprec lp)
        {
            int mode;

            lp.epsmachine = DEF_EPSMACHINE;
            lp.epsperturb = DEF_PERTURB;
            lp.lag_accept = DEF_LAGACCEPT;
            set_epslevel(lp, EPS_DEFAULT);

            lp.tighten_on_set = false;
            lp.negrange = DEF_NEGRANGE;

#if false
//  lp->do_presolve       = PRESOLVE_ROWS | PRESOLVE_COLS | PRESOLVE_MERGEROWS |
//                          PRESOLVE_REDUCEGCD |
//                          PRESOLVE_ROWDOMINATE;
#else
            lp.do_presolve = PRESOLVE_NONE;
#endif
            lp.presolveloops = (int)DEF_MAXPRESOLVELOOPS;

            lp.scalelimit = DEF_SCALINGLIMIT;
            //C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
            lp.scalemode = SCALE_INTEGERS |
#if false
//                          SCALE_POWER2 |
//                          SCALE_LOGARITHMIC | SCALE_MEAN;
#else
                          SCALE_LINEAR | SCALE_GEOMETRIC | SCALE_EQUILIBRATE;
#endif

            lp.crashmode = CRASH_NONE;

            lp.max_pivots = 0;
            lp.simplex_strategy = SIMPLEX_DUAL_PRIMAL;
            //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if PricerDefaultOpt == true
            mode = PRICER_DEVEX;
            //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#elif PricerDefaultOpt == false
  mode = PRICER_STEEPESTEDGE;
  mode |= PRICE_TRUENORMINIT;
#else
                mode = PRICER_STEEPESTEDGE | PRICE_PRIMALFALLBACK;
#endif
            mode |= PRICE_ADAPTIVE;
#if EnableRandomizedPricing
  mode |= PRICE_RANDOMIZE;
#endif
            set_pivoting(lp, mode);

            lp.improve = IMPROVE_DEFAULT;
            lp.anti_degen = ANTIDEGEN_DEFAULT;

            lp.bb_floorfirst = BRANCH_AUTOMATIC;
            //C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
            lp.bb_rule = NODE_DYNAMICMODE | NODE_GREEDYMODE | NODE_GAPSELECT |
#if true
                      //C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
                      NODE_PSEUDOCOSTSELECT |
#else
                                        //C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
                                        NODE_PSEUDOFEASSELECT |
#endif
                          NODE_RCOSTFIXING;
            lp.bb_limitlevel = DEF_BB_LIMITLEVEL;
            lp.bb_PseudoUpdates = (int)DEF_PSEUDOCOSTUPDATES;

            lp.bb_heuristicOF = lp_types.my_chsign(is_maxim(lp), commonlib.MAX(DEF_INFINITE, lp.infinite));
            lp.bb_breakOF = -lp.bb_heuristicOF;

            lp.sectimeout = 0;
            lp.solutionlimit = 1;

            set_outputstream(lp, null); // Set to default output stream
            lp.verbose = NORMAL;
            lp.print_sol = false; // Can be FALSE, TRUE, AUTOMATIC (only non-zeros printed)
            lp.spx_trace = false;
            lp.lag_trace = false;
            lp.bb_trace = false;

        }

        private new bool set_callbacks(lprec lp)
        {
            /*NOT REQUIRED
            // Assign API functions to lp structure (mainly for XLIs) 
            lp.add_column = add_column;
            lp.add_columnex = add_columnex;
            lp.add_constraint = add_constraint;
            lp.add_constraintex = add_constraintex;
            lp.add_lag_con = add_lag_con;
            lp.add_SOS = add_SOS;
            lp.column_in_lp = column_in_lp;
            lp.copy_lp = copy_lp;
            lp.default_basis = default_basis;
            lp.del_column = del_column;
            lp.del_constraint = del_constraint;
            lp.delete_lp = delete_lp;
            lp.dualize_lp = dualize_lp;
            lp.free_lp = free_lp;
            lp.get_anti_degen = get_anti_degen;
            lp.get_basis = get_basis;
            lp.get_basiscrash = get_basiscrash;
            lp.get_bb_depthlimit = get_bb_depthlimit;
            lp.get_bb_floorfirst = get_bb_floorfirst;
            lp.get_bb_rule = get_bb_rule;
            lp.get_bounds_tighter = get_bounds_tighter;
            lp.get_break_at_value = get_break_at_value;
            lp.get_col_name = get_col_name;
            lp.get_columnex = get_columnex;
            lp.get_constr_type = get_constr_type;
            lp.get_constr_value = get_constr_value;
            lp.get_constraints = get_constraints;
            lp.get_dual_solution = get_dual_solution;
            lp.get_epsb = get_epsb;
            lp.get_epsd = get_epsd;
            lp.get_epsel = get_epsel;
            lp.get_epsint = get_epsint;
            lp.get_epsperturb = get_epsperturb;
            lp.get_epspivot = get_epspivot;
            lp.get_improve = get_improve;
            lp.get_infinite = get_infinite;
            lp.get_lambda = get_lambda;
            lp.get_lowbo = get_lowbo;
            lp.get_lp_index = get_lp_index;
            lp.get_lp_name = get_lp_name;
            lp.get_Lrows = get_Lrows;
            lp.get_mat = get_mat;
            lp.get_mat_byindex = get_mat_byindex;
            lp.get_max_level = get_max_level;
            lp.get_maxpivot = get_maxpivot;
            lp.get_mip_gap = get_mip_gap;
            lp.get_multiprice = get_multiprice;
            lp.get_nameindex = get_nameindex;
            lp.get_Ncolumns = get_Ncolumns;
            lp.get_negrange = get_negrange;
            lp.get_nonzeros = get_nonzeros;
            lp.get_Norig_columns = get_Norig_columns;
            lp.get_Norig_rows = get_Norig_rows;
            lp.get_Nrows = get_Nrows;
            lp.get_obj_bound = get_obj_bound;
            lp.get_objective = get_objective;
            lp.get_orig_index = get_orig_index;
            lp.get_origcol_name = get_origcol_name;
            lp.get_origrow_name = get_origrow_name;
            lp.get_partialprice = get_partialprice;
            lp.get_pivoting = get_pivoting;
            lp.get_presolve = get_presolve;
            lp.get_presolveloops = get_presolveloops;
            lp.get_primal_solution = get_primal_solution;
            lp.get_print_sol = get_print_sol;
            lp.get_pseudocosts = get_pseudocosts;
            lp.get_ptr_constraints = get_ptr_constraints;
            lp.get_ptr_dual_solution = get_ptr_dual_solution;
            lp.get_ptr_lambda = get_ptr_lambda;
            lp.get_ptr_primal_solution = get_ptr_primal_solution;
            lp.get_ptr_sensitivity_obj = get_ptr_sensitivity_obj;
            lp.get_ptr_sensitivity_objex = get_ptr_sensitivity_objex;
            lp.get_ptr_sensitivity_rhs = get_ptr_sensitivity_rhs;
            lp.get_ptr_variables = get_ptr_variables;
            lp.get_rh = get_rh;
            lp.get_rh_range = get_rh_range;
            lp.get_row = get_row;
            lp.get_rowex = get_rowex;
            lp.get_row_name = get_row_name;
            lp.get_scalelimit = get_scalelimit;
            lp.get_scaling = get_scaling;
            lp.get_sensitivity_obj = get_sensitivity_obj;
            lp.get_sensitivity_objex = get_sensitivity_objex;
            lp.get_sensitivity_rhs = get_sensitivity_rhs;
            lp.get_simplextype = get_simplextype;
            lp.get_solutioncount = get_solutioncount;
            lp.get_solutionlimit = get_solutionlimit;
            lp.get_status = get_status;
            lp.get_statustext = get_statustext;
            lp.get_timeout = get_timeout;
            lp.get_total_iter = get_total_iter;
            lp.get_total_nodes = get_total_nodes;
            lp.get_upbo = get_upbo;
            lp.get_var_branch = get_var_branch;
            lp.get_var_dualresult = get_var_dualresult;
            lp.get_var_primalresult = get_var_primalresult;
            lp.get_var_priority = get_var_priority;
            lp.get_variables = get_variables;
            lp.get_verbose = get_verbose;
            lp.get_working_objective = get_working_objective;
            lp.has_BFP = has_BFP;
            lp.has_XLI = has_XLI;
            lp.is_add_rowmode = is_add_rowmode;
            lp.is_anti_degen = is_anti_degen;
            lp.is_binary = is_binary;
            lp.is_break_at_first = is_break_at_first;
            lp.is_constr_type = is_constr_type;
            lp.is_debug = is_debug;
            lp.is_feasible = is_feasible;
            lp.is_unbounded = is_unbounded;
            lp.is_infinite = is_infinite;
            lp.is_int = is_int;
            lp.is_integerscaling = is_integerscaling;
            lp.is_lag_trace = is_lag_trace;
            lp.is_maxim = is_maxim;
            lp.is_nativeBFP = is_nativeBFP;
            lp.is_nativeXLI = is_nativeXLI;
            lp.is_negative = is_negative;
            lp.is_obj_in_basis = is_obj_in_basis;
            lp.is_piv_mode = is_piv_mode;
            lp.is_piv_rule = is_piv_rule;
            lp.is_presolve = is_presolve;
            lp.is_scalemode = is_scalemode;
            lp.is_scaletype = is_scaletype;
            lp.is_semicont = is_semicont;
            lp.is_SOS_var = is_SOS_var;
            lp.is_trace = is_trace;
            lp.lp_solve_version = lp_solve_version;
            lp.make_lp = make_lp;
            lp.print_constraints = print_constraints;
            lp.print_debugdump = print_debugdump;
            lp.print_duals = print_duals;
            lp.print_lp = print_lp;
            lp.print_objective = print_objective;
            lp.print_scales = print_scales;
            lp.print_solution = print_solution;
            lp.print_str = print_str;
            lp.print_tableau = print_tableau;
            lp.put_abortfunc = put_abortfunc;
            lp.put_bb_nodefunc = put_bb_nodefunc;
            lp.put_bb_branchfunc = put_bb_branchfunc;
            lp.put_logfunc = put_logfunc;
            lp.put_msgfunc = put_msgfunc;
            lp.read_LP = read_LP;
            lp.read_MPS = read_MPS;
            lp.read_XLI = read_XLI;
            lp.read_basis = read_basis;
            lp.reset_basis = reset_basis;
            lp.read_params = read_params;
            lp.reset_params = reset_params;
            lp.resize_lp = resize_lp;
            lp.set_action = set_action;
            lp.set_add_rowmode = set_add_rowmode;
            lp.set_anti_degen = set_anti_degen;
            lp.set_basisvar = set_basisvar;
            lp.set_basis = set_basis;
            lp.set_basiscrash = set_basiscrash;
            lp.set_bb_depthlimit = set_bb_depthlimit;
            lp.set_bb_floorfirst = set_bb_floorfirst;
            lp.set_bb_rule = set_bb_rule;
            lp.set_BFP = set_BFP;
            lp.set_binary = set_binary;
            lp.set_bounds = set_bounds;
            lp.set_bounds_tighter = set_bounds_tighter;
            lp.set_break_at_first = set_break_at_first;
            lp.set_break_at_value = set_break_at_value;
            lp.set_col_name = set_col_name;
            lp.set_constr_type = set_constr_type;
            lp.set_debug = set_debug;
            lp.set_epsb = set_epsb;
            lp.set_epsd = set_epsd;
            lp.set_epsel = set_epsel;
            lp.set_epsint = set_epsint;
            lp.set_epslevel = set_epslevel;
            lp.set_epsperturb = set_epsperturb;
            lp.set_epspivot = set_epspivot;
            lp.set_unbounded = set_unbounded;
            lp.set_improve = set_improve;
            lp.set_infinite = set_infinite;
            lp.set_int = set_int;
            lp.set_lag_trace = set_lag_trace;
            lp.set_lowbo = set_lowbo;
            lp.set_lp_name = set_lp_name;
            lp.set_mat = set_mat;
            lp.set_maxim = set_maxim;
            lp.set_maxpivot = set_maxpivot;
            lp.set_minim = set_minim;
            lp.set_mip_gap = set_mip_gap;
            lp.set_multiprice = set_multiprice;
            lp.set_negrange = set_negrange;
            lp.set_obj = set_obj;
            lp.set_obj_bound = set_obj_bound;
            lp.set_obj_fn = set_obj_fn;
            lp.set_obj_fnex = set_obj_fnex;
            lp.set_obj_in_basis = set_obj_in_basis;
            lp.set_outputfile = set_outputfile;
            lp.set_outputstream = set_outputstream;
            lp.set_partialprice = set_partialprice;
            lp.set_pivoting = set_pivoting;
            lp.set_preferdual = set_preferdual;
            lp.set_presolve = set_presolve;
            lp.set_print_sol = set_print_sol;
            lp.set_pseudocosts = set_pseudocosts;
            lp.set_rh = set_rh;
            lp.set_rh_range = set_rh_range;
            lp.set_rh_vec = set_rh_vec;
            lp.set_row = set_row;
            lp.set_rowex = set_rowex;
            lp.set_row_name = set_row_name;
            lp.set_scalelimit = set_scalelimit;
            lp.set_scaling = set_scaling;
            lp.set_semicont = set_semicont;
            lp.set_sense = set_sense;
            lp.set_simplextype = set_simplextype;
            lp.set_solutionlimit = set_solutionlimit;
            lp.set_timeout = set_timeout;
            lp.set_trace = set_trace;
            lp.set_upbo = set_upbo;
            lp.set_var_branch = set_var_branch;
            lp.set_var_weights = set_var_weights;
            lp.set_verbose = set_verbose;
            lp.set_XLI = set_XLI;
            lp.solve = solve;
            lp.str_add_column = str_add_column;
            lp.str_add_constraint = str_add_constraint;
            lp.str_add_lag_con = str_add_lag_con;
            lp.str_set_obj_fn = str_set_obj_fn;
            lp.str_set_rh_vec = str_set_rh_vec;
            lp.time_elapsed = time_elapsed;
            lp.unscale = unscale;
            lp.write_lp = write_lp;
            lp.write_LP = write_LP;
            lp.write_mps = write_mps;
            lp.write_freemps = write_freemps;
            lp.write_MPS = write_MPS;
            lp.write_freeMPS = write_freeMPS;
            lp.write_XLI = write_XLI;
            lp.write_basis = write_basis;
            lp.write_params = write_params;

            // Utility functions (mainly for BFPs) 
            lp.userabort = userabort;
            lp.report = report;
            lp.explain = explain;
            lp.set_basisvar = set_basisvar;
            lp.get_lpcolumn = obtain_column;
            lp.get_basiscolumn = get_basiscolumn;
            lp.get_OF_active = get_OF_active;
            lp.getMDO = getMDO;
            lp.invert = invert;
            lp.set_action = set_action;
            lp.clear_action = clear_action;
            lp.is_action = is_action;
            */
            return true;
        }

        private new bool set_XLI(lprec lp, ref string filename)
        /* (Re)mapping of external language interface variant methods is done here */
        {
            int result = lp_types.LIB_LOADED;

            //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if LoadLanguageLib == TRUE
  if (lp.hXLI != null)
  {
#if WIN32
//C++ TO C# CONVERTER NOTE: There is no C# equivalent to 'FreeLibrary':
//	FreeLibrary(lp.hXLI);
#else
	dlclose(lp.hXLI);
#endif
	lp.hXLI = null;
  }
#endif

            if (filename == null)
            {
                if (!is_nativeXLI(lp))
                {
                    return false;
                }
#if !ExcludeNativeLanguage
                lp.xli_name = xli_name;
                lp.xli_compatible = xli_compatible;
                lp.xli_readmodel = xli_readmodel;
                lp.xli_writemodel = xli_writemodel;
#endif
            }
            else
            {

                //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if LoadLanguageLib == TRUE
#if WIN32
   /* Get a handle to the Windows DLL module. */
	lp.hXLI = LoadLibrary(filename);

   /* If the handle is valid, try to get the function addresses. */
	if (lp.hXLI != null)
	{
	  lp.xli_compatible = (XLIbool_lpintintint) GetProcAddress(lp.hXLI, "xli_compatible");
	  if (lp.xli_compatible == null)
	  {
		result = LIB_NOINFO;
	  }
	  else if (lp.xli_compatible(lp, XLIVERSION, MAJORVERSION, sizeof(REAL)))
	  {

		lp.xli_name = (XLIchar) GetProcAddress(lp.hXLI, "xli_name");
		lp.xli_readmodel = (XLIbool_lpcharcharcharint) GetProcAddress(lp.hXLI, "xli_readmodel");
		lp.xli_writemodel = (XLIbool_lpcharcharbool) GetProcAddress(lp.hXLI, "xli_writemodel");
	  }
	  else
	  {
		result = LIB_VERINVALID;
	  }
	}
#else
   /* First standardize UNIX .SO library name format. */
	string xliname = new string(new char[260]);
//C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
	char * ptr;

	xliname = filename;
	if ((ptr = StringFunctions.StrRChr(filename, '/')) == null)
	{
	  ptr = filename;
	}
	else
	{
	  ptr++;
	}
	xliname = xliname.Substring(0, (int)(ptr - filename));
	if (string.Compare(ptr, 0, "lib", 0, 3))
	{
	  xliname += "lib";
	}
	xliname += ptr;
	if (string.Compare(xliname.Substring(xliname.Length) - 3, ".so"))
	{
	  xliname += ".so";
	}

   /* Get a handle to the module. */
	lp.hXLI = dlopen(xliname, RTLD_LAZY);

   /* If the handle is valid, try to get the function addresses. */
	if (lp.hXLI != null)
	{
	  lp.xli_compatible = (XLIbool_lpintintint) dlsym(lp.hXLI, "xli_compatible");
	  if (lp.xli_compatible == null)
	  {
		result = LIB_NOINFO;
	  }
	  else if (lp.xli_compatible(lp, XLIVERSION, MAJORVERSION, sizeof(REAL)))
	  {

		lp.xli_name = (XLIchar) dlsym(lp.hXLI, "xli_name");
		lp.xli_readmodel = (XLIbool_lpcharcharcharint) dlsym(lp.hXLI, "xli_readmodel");
		lp.xli_writemodel = (XLIbool_lpcharcharbool) dlsym(lp.hXLI, "xli_writemodel");
	  }
	  else
	  {
		result = LIB_VERINVALID;
	  }
	}
#endif
	else
	{
	  result = LIB_NOTFOUND;
	}
#endif
                /* Do validation */
                if ((result != lp_types.LIB_LOADED) || ((lp.xli_name == null) || (lp.xli_compatible == null) || (lp.xli_readmodel == null) || (lp.xli_writemodel == null)))
                {
                    filename = null;
                    set_XLI(lp, ref filename);
                    if (result == lp_types.LIB_LOADED)
                    {
                        result = lp_types.LIB_NOFUNCTION;
                    }
                }
            }
            //C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
            if (filename != null)
            {
                string info = new string(new char[lp_types.LIB_STR_MAXLEN + 1]);
                switch (result)
                {
                    case lp_types.LIB_NOTFOUND:
                        info = lp_types.LIB_STR_NOTFOUND;
                        break;
                    case lp_types.LIB_NOINFO:
                        info = lp_types.LIB_STR_NOINFO;
                        break;
                    case lp_types.LIB_NOFUNCTION:
                        info = lp_types.LIB_STR_NOFUNCTION;
                        break;
                    case lp_types.LIB_VERINVALID:
                        info = lp_types.LIB_STR_VERINVALID;
                        break;
                    default:
                        info = lp_types.LIB_STR_LOADED;
                        break;
                }
                string msg = "set_XLI: {0} '{1}'\n";
                report(lp, IMPORTANT, ref msg, info, filename);
            }
            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            return ((bool)(result == lp_types.LIB_LOADED));
        }

        //----------------------------------------------------------------------------------------
        //	Copyright © 2006 - 2018 Tangible Software Solutions, Inc.
        //	This class can be used by anyone provided that the copyright notice remains intact.
        //
        //	This class provides the ability to replicate various classic C string functions
        //	which don't have exact equivalents in the .NET Framework.
        //----------------------------------------------------------------------------------------
        internal static class StringFunctions
        {
            //------------------------------------------------------------------------------------
            //	This method allows replacing a single character in a string, to help convert
            //	C++ code where a single character in a character array is replaced.
            //------------------------------------------------------------------------------------
            public static string ChangeCharacter(string sourceString, int charIndex, char newChar)
            {
                return (charIndex > 0 ? sourceString.Substring(0, charIndex) : "")
                    + newChar.ToString() + (charIndex < sourceString.Length - 1 ? sourceString.Substring(charIndex + 1) : "");
            }

            //------------------------------------------------------------------------------------
            //	This method replicates the classic C string function 'isxdigit' (and 'iswxdigit').
            //------------------------------------------------------------------------------------
            public static bool IsXDigit(char character)
            {
                if (char.IsDigit(character))
                    return true;
                else if ("ABCDEFabcdef".IndexOf(character) > -1)
                    return true;
                else
                    return false;
            }

            //------------------------------------------------------------------------------------
            //	This method replicates the classic C string function 'strchr' (and 'wcschr').
            //------------------------------------------------------------------------------------
            public static string StrChr(string stringToSearch, char charToFind)
            {
                int index = stringToSearch.IndexOf(charToFind);
                if (index > -1)
                    return stringToSearch.Substring(index);
                else
                    return null;
            }

            //------------------------------------------------------------------------------------
            //	This method replicates the classic C string function 'strrchr' (and 'wcsrchr').
            //------------------------------------------------------------------------------------
            public static string StrRChr(string stringToSearch, char charToFind)
            {
                int index = stringToSearch.LastIndexOf(charToFind);
                if (index > -1)
                    return stringToSearch.Substring(index);
                else
                    return null;
            }

            //------------------------------------------------------------------------------------
            //	This method replicates the classic C string function 'strstr' (and 'wcsstr').
            //------------------------------------------------------------------------------------
            public static string StrStr(string stringToSearch, string stringToFind)
            {
                int index = stringToSearch.IndexOf(stringToFind);
                if (index > -1)
                    return stringToSearch.Substring(index);
                else
                    return null;
            }

            //------------------------------------------------------------------------------------
            //	This method replicates the classic C string function 'strtok' (and 'wcstok').
            //	Note that the .NET string 'Split' method cannot be used to replicate 'strtok' since
            //	it doesn't allow changing the delimiters between each token retrieval.
            //------------------------------------------------------------------------------------
            private static string activeString;
            private static int activePosition;
            public static string StrTok(string stringToTokenize, string delimiters)
            {
                if (stringToTokenize != null)
                {
                    activeString = stringToTokenize;
                    activePosition = -1;
                }

                //the stringToTokenize was never set:
                if (activeString == null)
                    return null;

                //all tokens have already been extracted:
                if (activePosition == activeString.Length)
                    return null;

                //bypass delimiters:
                activePosition++;
                while (activePosition < activeString.Length && delimiters.IndexOf(activeString[activePosition]) > -1)
                {
                    activePosition++;
                }

                //only delimiters were left, so return null:
                if (activePosition == activeString.Length)
                    return null;

                //get starting position of string to return:
                int startingPosition = activePosition;

                //read until next delimiter:
                do
                {
                    activePosition++;
                } while (activePosition < activeString.Length && delimiters.IndexOf(activeString[activePosition]) == -1);

                return activeString.Substring(startingPosition, activePosition - startingPosition);
            }
        }


        /// <summary>
        /// changed from 'ref double row' to 'ref double[] row' FIX_90b96e5c-2dba-4335-95bd-b1fcc95f1b55 19/11/18
        /// </summary>
        public new bool add_constraint(lprec lp, ref double?[] row, int constr_type, double rh)
        {
            return (add_constraintex(lp, 0, ref row, null, constr_type, rh));
        }
        internal new void set_minim(lprec lp)
        {
            set_sense(lp, false);
        }

        /// <summary>
        /// changed from ref int colno to ref object colno on 12/11/18
        /// changed from 'ref double row' to ref double[] row FIX_90b96e5c-2dba-4335-95bd-b1fcc95f1b55 19/11/18
        /// changed from 'object colno' to 'int?[] colno' FIX_90b96e5c-2dba-4335-95bd-b1fcc95f1b55 19/11/18
        /// </summary>
        private bool add_constraintex(lprec lp, int count, ref double?[] row, int?[] colno, int constr_type, double rh)
        {
            int n;
            bool status = false;
            lp_report objlp_report = new lp_report();

            if (!(constr_type == LE || constr_type == GE || constr_type == EQ))
            {
                string msg = "add_constraintex: Invalid {0} constraint type\n";
                objlp_report.report(lp, IMPORTANT, ref msg, constr_type);
                return (status);
            }

            /* Prepare for a new row */
            if (!append_rows(lp, 1))
            {
                return (status);
            }

            /* Set constraint parameters, fix the slack */
            if ((constr_type & ROWTYPE_CONSTRAINT) == EQ)
            {
                lp.equalities++;
                lp.orig_upbo[lp.rows] = 0;
                lp.upbo[lp.rows] = 0;
            }
            lp.row_type[lp.rows] = constr_type;

            if (is_chsign(lp, lp.rows) && (rh != 0))
            {
                lp.orig_rhs[lp.rows] = -rh;
            }
            else
            {
                lp.orig_rhs[lp.rows] = rh;
            }

            /* Insert the non-zero constraint values */
            if (colno == null && row != null)
            {
                n = lp.columns;
            }
            else
            {
                n = count;
            }
            ///<summary> FIX_90b96e5c-2dba-4335-95bd-b1fcc95f1b55 19/11/18
            /// PREVIOUS: lp_matrix.mat_appendrow(lp.matA, n, row, colno, lp_types.my_chsign(is_chsign(lp, lp.rows), 1.0), true);
            /// ERROR IN PREVIOUS: cannot convert from 'double' to 'double[]'
            /// FIX 1: changed parameter from 'ref double row' to ref double[] row
            /// ERROR 2: cannot convert from 'object' to 'int?[]'
            /// FIX 2: changed colno type from object to int?[]
            ///</summary>
            lp_matrix.mat_appendrow(lp.matA, n, row, colno, lp_types.my_chsign(is_chsign(lp, lp.rows), 1.0), true);
            if (!lp.varmap_locked)
            {
                lp_presolve.presolve_setOrig(lp, lp.rows, lp.columns);
            }

            //#if Paranoia
            if (lp.matA.is_roworder)
            {
                n = lp.matA.columns;
            }
            else
            {
                n = (lp.matA.rows != null) ? Convert.ToInt32(lp.matA.rows) : 0;
            }
            if (lp.rows != n)
            {
                string msg = "add_constraintex: Row count mismatch {0} vs {1}\n";
                report(lp, SEVERE, ref msg, lp.rows, n);
            }
            else if (is_BasisReady(lp) && !verify_basis(lp))
            {
                string msg = "add_constraintex: Invalid basis detected for row {0}\n";
                report(lp, SEVERE, ref msg, lp.rows);
            }
            else
            {
                //#endif
                status = true;
            }
            return (status);
        }
        internal new bool set_unbounded(lprec lp, int colnr)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "set_unbounded: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return false;
            }

            return (set_bounds(lp, colnr, -lp.infinite, lp.infinite));
        }
        int get_nonzeros(lprec lp)
        {
            return (lp_matrix.mat_nonzeros(lp.matA));
        }

        internal bool set_bounds(lprec lp, int colnr, double lower, double upper)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "set_bounds: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return false;
            }
            /// <summary>
            /// changed from lp.epsvalue to lprec.epsvalue on 13/11/18
            /// </summary>
            if (System.Math.Abs(upper - lower) < lprec.epsvalue)
            {
                if (lower < 0)
                {
                    lower = upper;
                }
                else
                {
                    upper = lower;
                }
            }
            else if (lower > upper)
            {
                string msg = "set_bounds: Column {0} upper bound must be >= lower bound\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return false;
            }

            colnr += lp.rows;

            if (lower < -lp.infinite)
            {
                lower = -lp.infinite;
            }
            else if (lp.scaling_used)
            {
                lower = lp_scale.scaled_value(lp, lower, colnr);
                /*#if DoBorderRounding
                    lower = my_avoidtiny(lower, lp.matA.epsvalue);
                #endif*/
            }

            if (upper > lp.infinite)
            {
                upper = lp.infinite;
            }
            else if (lp.scaling_used)
            {
                upper = lp_scale.scaled_value(lp, upper, colnr);
                /*#if DoBorderRounding
                    upper = my_avoidtiny(upper, lp.matA.epsvalue);
                #endif*/
            }

            lp.orig_lowbo[colnr] = lower;
            lp.orig_upbo[colnr] = upper;
            set_action(ref lp.spx_action, ACTION_REBASE);

            return true;
        }


        internal new bool set_lowbo(lprec lp, int colnr, double value)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "set_lowbo: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return false;
            }

            /*#if DoBorderRounding
              if (Math.Abs(value) < lp.infinite)
              {
	            value = my_avoidtiny(value, lp.matA.epsvalue);
              }
            #endif*/
            value = lp_scale.scaled_value(lp, value, lp.rows + colnr);
            if (lp.tighten_on_set)
            {
                if (value > lp.orig_upbo[lp.rows + colnr])
                {
                    string msg = "set_lowbo: Upper bound must be >= lower bound\n";
                    report(lp, IMPORTANT, ref msg);
                    return false;
                }
                if ((value < 0) || (value > lp.orig_lowbo[lp.rows + colnr]))
                {
                    set_action(ref lp.spx_action, ACTION_REBASE);
                    lp.orig_lowbo[lp.rows + colnr] = value;
                }
            }
            else
            {
                set_action(ref lp.spx_action, ACTION_REBASE);
                if (value < -lp.infinite)
                {
                    value = -lp.infinite;
                }
                lp.orig_lowbo[lp.rows + colnr] = value;
            }
            return true;
        }

        internal new double get_lowbo(lprec lp, int colnr)
        {
            double value;

            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "get_lowbo: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return (0);
            }

            value = lp.orig_lowbo[lp.rows + colnr];
            value = lp_scale.unscaled_value(lp, value, lp.rows + colnr);
            return (value);
        }


        internal new static bool is_int(lprec lp, int colnr)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                lp_report objlp_report = new lp_report();
                string msg = "is_int: Column {0} out of range\n";
                objlp_report.report(lp, IMPORTANT, ref msg, colnr);
                return false;
            }
            return ((lp.var_type[colnr] && ISINTEGER));
        }

        internal new bool set_rh(lprec lp, int rownr, double value)
        {
            if ((rownr > lp.rows) || (rownr < 0))
            {
                string msg = "set_rh: Row {0} out of range\n";
                report(lp, IMPORTANT, ref msg, rownr);
                return false;
            }

            if (((rownr == 0) && (!is_maxim(lp))) || ((rownr > 0) && is_chsign(lp, rownr))) // setting of RHS of OF IS meaningful
            {
                value = lp_types.my_flipsign(value);
            }
            if (System.Math.Abs(value) > lp.infinite)
            {
                if (value < 0)
                {
                    value = -lp.infinite;
                }
                else
                {
                    value = lp.infinite;
                }
            }
            /*#if DoBorderRounding
              else
              {
	            value = my_avoidtiny(value, lp.matA.epsvalue);
              }
            #endif*/
            value = lp_scale.scaled_value(lp, value, rownr);
            lp.orig_rhs[rownr] = value;
            set_action(ref lp.spx_action, ACTION_RECOMPUTE);
            return true;
        }

        internal new bool set_row_name(lprec lp, int rownr, ref string new_name)
        {
            if ((rownr < 0) || (rownr > lp.rows + 1))
            {
                lp_report objlp_report = new lp_report();
                string msg = "set_row_name: Row {0} out of range";
                objlp_report.report(lp, IMPORTANT, ref msg, rownr);
                return false;
            }

            /* Prepare for a new row */
            if ((rownr > lp.rows) && !append_rows(lp, rownr - lp.rows))
            {
                return false;
            }
            if (!lp.names_used)
            {
                if (!init_rowcol_names(lp))
                {
                    return false;
                }
            }
            ///<summary> FIX_6cf0db98-4dd5-40fa-9afe-ddc2e2c94eb2 19/11/18
            /// PREVIOUS: private bool rename_var(lprec lp, int varindex, ref string new_name, hashelem[] list, hashtable[] ht)
            /// ERROR IN PREVIOUS: cannot convert from 'ZS.Math.Optimization.hashtable' to 'ZS.Math.Optimization.hashtable[]'
            /// FIX 1: changed rename_var parameter from 'hashtable[] ht' to 'hashtable ht'
            /// </summary>
            rename_var(lp, rownr, ref new_name, lp.row_name, lp.rowname_hashtab);

            return true;
        }

        private bool rename_var(lprec lp, int varindex, ref string new_name, hashelem[] list, hashtable ht)
        {
            hashelem hp;
            bool newitem;

            hp = list[varindex];
            newitem = (bool)(hp == null);
            if (newitem)
            {
                // chanegd from 'ht[0]' to 'ht' FIX_6cf0db98-4dd5-40fa-9afe-ddc2e2c94eb2 19/11/18
                hp = lp_Hash.puthash(new_name, varindex, list, ht);
            }
            else if ((hp.name.Length != new_name.Length) || (string.Compare(hp.name, new_name) != 0))
            {
                hashtable newht;
                hashtable oldht;

                //NOT REQUIRED: allocCHAR(lp, hp.name, (int)(new_name.Length + 1), AUTOMATIC);
                hp.name = new_name;
                // chanegd from 'ht[0]' to 'ht' FIX_6cf0db98-4dd5-40fa-9afe-ddc2e2c94eb2 19/11/18
                oldht = ht;
                ///<summary> 19/11/18
                /// PREVIOUS: newht = lp_Hash.copy_hash_table(oldht, list, oldht.size);
                /// ERROR IN PREVIOUS: cannot convert from 'ZS.Math.Optimization.hashelem[]' to 'ZS.Math.Optimization.hashelem'
                /// FIX 1: changed from 'list' to 'list[0]'
                /// </summary>
                newht = lp_Hash.copy_hash_table(oldht, list[0], oldht.size);
                // chanegd from 'ht[0]' to 'ht' FIX_6cf0db98-4dd5-40fa-9afe-ddc2e2c94eb2 19/11/18
                ht = newht;
                lp_Hash.free_hash_table(oldht);
            }
            return (newitem);
        }

        private static void varmap_add(lprec lp, int @base, int delta)
        {
            int i;
            int ii;
            presolveundorec psundo = lp.presolve_undo;

            /* Don't do anything if variables aren't locked yet */
            if (!lp.varmap_locked)
            {
                return;
            }

            /* Set new constraints/columns to have an "undefined" mapping to original
               constraints/columns (assumes that counters have NOT yet been updated) */
            for (i = lp.sum; i >= @base; i--)
            {
                ii = i + delta;
                psundo.var_to_orig[ii] = psundo.var_to_orig[i];
            }

            /* Initialize map of added rows/columns */
            for (i = 0; i < delta; i++)
            {
                ii = @base + i;
                psundo.var_to_orig[ii] = 0;
            }
        }

        internal new bool append_rows(lprec lp, int deltarows)
        {
            if (!inc_row_space(lp, deltarows))
            {
                return false;
            }
            varmap_add(lp, lp.rows + 1, deltarows);
            shift_rowdata(lp, lp.rows + 1, deltarows, null);

            return true;
        }

        internal static new bool is_infinite(lprec lp, double value)
        {
            /// <summary>
            /// commented on 12/11/18
            /// </summary>
            /*#if 1
            return (System.Math.Abs(value) >= lp.infinite);
            #else*/
            if (System.Math.Abs(value) >= lp.infinite)
                return true;
            else
                return false;
            //#endif
        }

        internal static new int SOS_count(lprec lp)
        {
            if (lp.SOS == null)
            {
                return (0);
            }
            else
            {
                return (lp.SOS.sos_count);
            }
        }

        internal new int add_SOS(lprec lp, ref string name, int sostype, int priority, int count, int?[] sosvars, double? weights)
        {
            SOSrec SOS;
            int k;
            lp_report objlp_report = new lp_report();

            if ((sostype < 1) || (count < 0))
            {
                string msg = "add_SOS: Invalid SOS type definition {0}\n";
                objlp_report.report(lp, IMPORTANT, ref msg, sostype);
                return (0);
            }

            /* Make sure SOSes of order 3 and higher are properly defined */
            if (sostype > 2)
            {
                int j;
                for (k = 0; k < count; k++)
                {
                    ///<summary>
                    /// PREVIOUS: j = sosvars[k];
                    /// ERROR IN PREVIOUS: Cannot implicitly convert type 'int?' to 'int'.An explicit conversion exists (are you missing a cast?)
                    /// </summary>
                    j = (sosvars[k] != null) ? Convert.ToInt32(sosvars[k]) : 0;
                    ///<summary> FIX_77f08594-fb68-4eb9-9ded-bfcd976801ba 19/11/18
                    /// PREVIOUS: if (!is_int(lp, j) || !is_semicont(lp, j))
                    /// ERROR IN PREVIOUS: Operator '!' cannot be applied to operand of type 'byte'
                    /// FIX 1: changed is_semicont return type from byte to bool
                    /// </summary>
                    if (!is_int(lp, j) || !is_semicont(lp, j))
                    {
                        string msg = "add_SOS: SOS3+ members all have to be integer or semi-continuous.\n";
                        objlp_report.report(lp, IMPORTANT, ref msg);
                        return 0;
                    }
                }
            }

            /* Make size in the list to handle another SOS record */
            if (lp.SOS == null)
            {
                lp.SOS = lp_SOS.create_SOSgroup(lp);
            }

            /* Create and append SOS to list */
            ///<summary> FIX_1e4d8424-dcfd-4e3a-9936-fda7b883b7d8 19/11/18
            /// PREVIOUS: SOS = lp_SOS.create_SOSrec(lp.SOS, ref name, sostype, priority, count, ref sosvars, ref weights);
            /// ERROR IN PREVIOUS: cannot convert from 'ref int?[]' to 'ref int'.
            /// FIX 1: changed create_SOSrec parameter from 'ref int variables' to 'ref int?[] variables'
            /// ERROR 2: cannot convert from 'ref double?' to 'ref double'
            /// FIX 2: changed create_SOSrec parameter from 'ref double weights' to 'ref double? weights'
            /// </summary>
            SOS = lp_SOS.create_SOSrec(lp.SOS, ref name, sostype, priority, count, ref sosvars, ref weights);
            k = lp_SOS.append_SOSgroup(lp.SOS, SOS);

            return (k);
        }

        /// <summary>
        /// changed return type from byte to bool FIX_77f08594-fb68-4eb9-9ded-bfcd976801ba 19/11/18
        /// </summary>
        internal static new bool is_semicont(lprec lp, int colnr)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                lp_report objlp_report = new lp_report();
                string msg = "is_semicont: Column {0} out of range\n";
                objlp_report.report(lp, IMPORTANT, ref msg, colnr);
                return (false);
            }
            ///<summary> 19/11/18
            /// PREVIOUS: return ((lp.var_type[colnr] && ISSEMI));
            /// ERROR IN PREVIOUS: Operator '!=' cannot be applied to operands of type 'bool' and 'int'
            /// FIX 1: ISSEMI > 0; need to changed at the time of testing based on the situation
            /// </summary>
            return ((lp.var_type[colnr] && ISSEMI > 0));

        }

        internal static new bool is_maxim(lprec lp)
        {
            return ((bool)((lp.row_type != null) && ((lp.row_type[0] & ROWTYPE_CHSIGN) == ROWTYPE_GE)));
        }

        internal new void set_sense(lprec lp, bool maximize)
        {
            maximize = (bool)(maximize != false);
            if (is_maxim(lp) != maximize)
            {
                int i;
                if (is_infinite(lp, lp.bb_heuristicOF))
                {
                    lp.bb_heuristicOF = lp_types.my_chsign(maximize, lp.infinite);
                }
                if (is_infinite(lp, lp.bb_breakOF))
                {
                    lp.bb_breakOF = lp_types.my_chsign(maximize, -lp.infinite);
                }
                lp.orig_rhs[0] = lp_types.my_flipsign(lp.orig_rhs[0]);
                for (i = 1; i <= lp.columns; i++)
                {
                    lp.orig_obj[i] = lp_types.my_flipsign(lp.orig_obj[i]);
                }
                set_action(ref lp.spx_action, ACTION_REINVERT | ACTION_RECOMPUTE);
            }
            if (maximize)
            {
                lp.row_type[0] = ROWTYPE_OFMAX;
            }
            else
            {
                lp.row_type[0] = ROWTYPE_OFMIN;
            }
        }

        private new bool set_semicont(lprec lp, int colnr, uint must_be_sc)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "set_semicont: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return false;
            }

            if (lp.sc_lobound[colnr] != 0)
            {
                lp.sc_vars--;
                ///<summary>
                /// PREVIOUS: lp.var_type[colnr] &= ~ISSEMI;
                /// ERROR IN PREVIOUS: Operator '&=' cannot be applied to operands of type 'bool' and 'int'
                /// FIX 1: lp.var_type[colnr] &= ~ISSEMI > 0;
                /// unclear; need to check while implementing
                /// </summary>
                lp.var_type[colnr] &= ~ISSEMI > 0;
            }
            ///<summary> FIX_ca4c2404-e9f4-407d-b791-79776cb8de1f 19/11/18
            /// PREVIOUS: lp.sc_lobound[colnr] = must_be_sc;
            /// ERROR IN PREVIOUS: Cannot implicitly convert type 'bool' to 'double'
            /// FIX 1: changed set_semicont parameter from 'bool' to 'uint' 
            /// REF: lp_types.h: definition for MYBOOL says 'could be unsigned int'
            /// need to check while implementing
            /// </summary>
            lp.sc_lobound[colnr] = must_be_sc;
            ///<summary>
            /// PREVIOUS: if (must_be_sc)
            /// ERROR IN PREVIOUS: Cannot implicitly convert type 'uint' to 'bool' 
            /// error occured due to FIX_ca4c2404-e9f4-407d-b791-79776cb8de1f 19/11/18
            /// FIX 1: changed to must_be_sc > 0
            /// need to check while implementing
            /// </summary>
            if (must_be_sc > 0)
            {
                ///<summary> 19/11/18
                /// PREVIOUS: lp.var_type[colnr] |= ISSEMI;
                /// ERROR IN PREVIOUS: Operator '|=' cannot be applied to operands of type 'bool' and 'int'
                /// FIX 1: changed '|=' to '= ||'
                /// unclear; need to check while implementing
                /// </summary>
                lp.var_type[colnr] = lp.var_type[colnr] || ISSEMI > 0;
                lp.sc_vars++;
            }
            return true;
        }

        internal new bool set_constr_type(lprec lp, int rownr, int con_type)
        {
            bool oldchsign;

            if (rownr > lp.rows + 1 || rownr < 1)
            {
                string msg = "set_constr_type: Row {0} out of range\n";
                report(lp, IMPORTANT, ref msg, rownr);
                return false;
            }

            /* Prepare for a new row */
            if ((rownr > lp.rows) && !append_rows(lp, rownr - lp.rows))
            {
                return false;
            }

            /* Update the constraint type data */
            if (is_constr_type(lp, rownr, EQ))
            {
                lp.equalities--;
            }

            if ((con_type & ROWTYPE_CONSTRAINT) == EQ)
            {
                lp.equalities++;
                lp.orig_upbo[rownr] = 0;
            }
            else if (((con_type & LE) > 0) || ((con_type & GE) > 0) || (con_type == FR))
            {
                lp.orig_upbo[rownr] = lp.infinite;
            }
            else
            {
                string msg = "set_constr_type: Constraint type {0} not implemented (row {1})\n";
                report(lp, IMPORTANT, ref msg, con_type, rownr);
                return false;
            }

            /* Change the signs of the row, if necessary */
            oldchsign = is_chsign(lp, rownr);
            if (con_type == FR)
            {
                lp.row_type[rownr] = LE;
            }
            else
            {
                lp.row_type[rownr] = con_type;
            }
            if (oldchsign != is_chsign(lp, rownr))
            {
                MATrec mat = lp.matA;

                if (mat.is_roworder)
                {
                    lp_matrix.mat_multcol(mat, rownr, -1, false);
                }
                else
                {
                    lp_matrix.mat_multrow(mat, rownr, -1);
                }
                if (lp.orig_rhs[rownr] != 0)
                {
                    lp.orig_rhs[rownr] *= -1;
                }
                set_action(ref lp.spx_action, ACTION_RECOMPUTE);
            }
            if (con_type == FR)
            {
                lp.orig_rhs[rownr] = lp.infinite;
            }

            set_action(ref lp.spx_action, ACTION_REINVERT);
            lp.basis_valid = 0;

            return true;
        }

        private new static int get_Lrows(lprec lp)
        {
            if (lp.matL == null)
            {
                return (0);
            }
            else
            {
                return ((lp.matL.rows != null) ? Convert.ToInt32(lp.matL.rows) : 0);
            }
        }


        private static new bool is_constr_type(lprec lp, int rownr, int mask)
        {
            if ((rownr < 0) || (rownr > lp.rows))
            {
                lp_report objlp_report = new lp_report();
                string msg = "is_constr_type: Row {0} out of range\n";
                objlp_report.report(lp, IMPORTANT, ref msg, rownr);
                return false;
            }
            return (((lp.row_type[rownr] & ROWTYPE_CONSTRAINT) == mask));
        }



        private bool set_int(lprec lp, int colnr, bool var_type)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "set_int: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return false;
            }
            ///<summary> 19/11/18
            /// PREVIOUS: lp.var_type[colnr] &= ~ISINTEGER;
            /// ERROR IN PREVIOUS: Operator '~' cannot be applied to operand of type 'bool'
            /// FIX 1: changed from '&= ~ISINTEGER' to '&= ISINTEGER'
            /// </summary>
            if ((lp.var_type[colnr] & ISINTEGER))
            {
                lp.int_vars--;
                lp.var_type[colnr] &= ISINTEGER;
            }
            if (var_type)
            {
                lp.var_type[colnr] |= ISINTEGER;
                lp.int_vars++;
                if (lp.columns_scaled && !is_integerscaling(lp))
                {
                    lp_scale.unscale_columns(lp);
                }
            }
            return true;
        }


        public new bool set_upbo(lprec lp, int colnr, double value)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "set_upbo: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return false;
            }

            /*#if DoBorderRounding
              if (Math.Abs(value) < lp.infinite)
              {
	            value = my_avoidtiny(value, lp.matA.epsvalue);
              }
            #endif*/
            value = lp_scale.scaled_value(lp, value, lp.rows + colnr);
            if (lp.tighten_on_set)
            {
                if (value < lp.orig_lowbo[lp.rows + colnr])
                {
                    string msg = "set_upbo: Upperbound must be >= lowerbound\n";
                    report(lp, IMPORTANT, ref msg);
                    return false;
                }
                if (value < lp.orig_upbo[lp.rows + colnr])
                {
                    set_action(ref lp.spx_action, ACTION_REBASE);
                    lp.orig_upbo[lp.rows + colnr] = value;
                }
            }
            else
            {
                set_action(ref lp.spx_action, ACTION_REBASE);
                if (value > lp.infinite)
                {
                    value = lp.infinite;
                }
                lp.orig_upbo[lp.rows + colnr] = value;
            }
            return true;
        }

        internal new bool str_add_constraint(lprec lp, ref string row_string, int constr_type, double rh)
        {
            int i;
            string p;
            string newp = "";
            double?[] aRow = null;
            bool status = false;
            lp_report objlp_report = new lp_report();
            LpCls objLpCls = new LpCls();

            /*NOT REQUIRED IN C#
            allocREAL(lp, aRow, lp.columns + 1, 0);
            */
            p = row_string;

            for (i = 1; i <= lp.columns; i++)
            {
                aRow[i] = Convert.ToDouble(p);  //ORIGINAL CODE: 	aRow[i] = (double) strtod(p, newp);
                if (p == newp)
                {
                    string msg = "str_add_constraint: Bad string '{0}'\n";
                    objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, p);
                    lp.spx_status = lp_lib.DATAIGNORED;
                    break;
                }
                else
                {
                    p = newp;
                }
            }
            if (lp.spx_status != lp_lib.DATAIGNORED)
            {
                status = objLpCls.add_constraint(lp, ref aRow, constr_type, rh);
            }
            //NOT REQUIRED IN C#
            //FREE(aRow);

            return (status);
        }

        internal int get_rowex(lprec lp, int rownr, ref double[] row, ref int?[] colno)
        {
            lp_report objlp_report = new lp_report();


            if ((rownr < 0) || (rownr > lp.rows))
            {
                string msg = "get_rowex: Row {0} out of range\n";
                objlp_report.report(lp, IMPORTANT, ref msg, rownr);
                return (-1);
            }

            if (rownr != 0 && lp.matA.is_roworder)
            {
                return (mat_getcolumn(lp, rownr, ref row, ref colno));
            }
            else
            {
                return (mat_getrow(lp, rownr, ref row, ref colno));
            }
        }

        internal static int mat_getcolumn(lprec lp, int colnr, ref double[] column, ref int?[] nzrow)
        {
            int n = 0;
            int i;
            int ii;
            int ie;
            //ORIGINAL LINE: int *rownr;
            int rownr;
            double hold;
            //ORIGINAL LINE: double *value;
            double value;
            MATrec mat = lp.matA;

            if (nzrow == null)
            {
                //NOT REQUIRED
                // MEMCLEAR(column, lp.rows + 1);
            }
            if (!mat.is_roworder)
            {
                /* Add the objective function */
                hold = lp.get_mat(lp, 0, colnr);
                if (nzrow == null)
                {
                    column[n] = hold;
                    if (hold != 0)
                    {
                        n++;
                    }
                }
                else if (hold != 0)
                {
                    column[n] = hold;
                    nzrow[n] = 0;
                    n++;
                }
            }
            //Cannot implicitly convert type 'int[]' to 'int'
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set first [] as 0 for now; need to check at run time
            i = lp.matA.col_end[0][colnr - 1];
            //set first [] as 0 for now
            ie = lp.matA.col_end[0][colnr];
            if (nzrow == null)
            {
                n += ie - i;
            }
            rownr = lp_matrix.COL_MAT_ROWNR(i);
            value = lp_matrix.COL_MAT_VALUE(i);
            for (; i < ie; i++, rownr += lp_matrix.matRowColStep, value += lp_matrix.matValueStep)
            {
                ii = rownr;

                hold = lp_types.my_chsign(lp.is_chsign(lp, (mat.is_roworder) ? colnr : ii), value);
                hold = lp_scale.unscaled_mat(lp, hold, ii, colnr);
                if (nzrow == null)
                {
                    column[ii] = hold;
                }
                else if (hold != 0)
                {
                    column[n] = hold;
                    nzrow[n] = ii;
                    n++;
                }
            }
            return (n);
        }

        public new double get_mat(lprec lp, int rownr, int colnr)
        {
            double value;
            int elmnr;
            int colnr1 = colnr;
            int rownr1 = rownr;

            lp_report objlp_report = new lp_report();

            if ((rownr < 0) || (rownr > lp.rows))
            {
                string msg = "get_mat: Row {0} out of range";
                objlp_report.report(lp, IMPORTANT, ref msg, rownr);
                return (0);
            }
            if ((colnr < 1) || (colnr > lp.columns))
            {
                string msg = "get_mat: Row {0} out of range";
                objlp_report.report(lp, IMPORTANT, ref msg, colnr);
                return (0);
            }
            if (rownr == 0)
            {
                value = lp.orig_obj[colnr];
                value = lp_types.my_chsign(is_chsign(lp, rownr), value);
                value = lp_scale.unscaled_mat(lp, value, rownr, colnr);
            }
            else
            {
                if (lp.matA.is_roworder)
                {
                    lp_utils.swapINT(ref colnr1, ref rownr1);
                }
                elmnr = lp_matrix.mat_findelm(lp.matA, rownr1, colnr1);
                if (elmnr >= 0)
                {
                    MATrec mat = lp.matA;
                    value = lp_types.my_chsign(is_chsign(lp, rownr), lp_matrix.COL_MAT_VALUE(elmnr));
                    value = lp_scale.unscaled_mat(lp, value, rownr, colnr);
                }
                else
                {
                    value = 0;
                }
            }
            return (value);
        }

        internal static int mat_getrow(lprec lp, int rownr, ref double[] row, ref int?[] colno)
        {
            bool isnz;
            int j;
            int countnz = 0;
            double a;

            if ((rownr == 0) || !lp_matrix.mat_validate(lp.matA))
            {
                for (j = 1; j <= lp.columns; j++)
                {
                    a = lp.get_mat(lp, rownr, j);
                    isnz = (a != 0);
                    if (colno == null)
                    {
                        row[j] = a;
                    }
                    else if (isnz)
                    {
                        row[countnz] = a;
                        colno[countnz] = j;
                    }
                    if (isnz)
                    {
                        countnz++;
                    }
                }
            }
            else
            {
                bool chsign = false;
                int ie;
                int i;
                MATrec mat = lp.matA;

                if (colno == null)
                {
                    /*
                     * NOT REQUIRED
                    MEMCLEAR(row, lp.columns + 1);
                    */
                }
                if (mat.is_roworder)
                {
                    /* Add the objective function */
                    a = lp.get_mat(lp, 0, rownr);
                    if (colno == null)
                    {
                        row[countnz] = a;
                        if (a != 0)
                        {
                            countnz++;
                        }
                    }
                    else if (a != 0)
                    {
                        row[countnz] = a;
                        colno[countnz] = 0;
                        countnz++;
                    }
                }
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                i = mat.row_end[rownr - 1][0];
                //set second [] as 0 for now
                ie = mat.row_end[rownr][0];
                if (!lp.matA.is_roworder)
                {
                    chsign = lp.is_chsign(lp, rownr);
                }
                for (; i < ie; i++)
                {
                    j = lp_matrix.ROW_MAT_COLNR(i);
                    a = lp.get_mat_byindex(lp, i, 1, 0);
                    if (lp.matA.is_roworder)
                    {
                        chsign = lp.is_chsign(lp, j);
                    }
                    a = lp_types.my_chsign(chsign, a);
                    if (colno == null)
                    {
                        row[j] = a;
                    }
                    else
                    {
                        row[countnz] = a;
                        colno[countnz] = j;
                    }
                    countnz++;
                }
            }
            return (countnz);
        }

        internal new bool is_action(int actionvar, int testmask)
        {
            return ((bool)((actionvar & testmask) != 0));
        }

        internal bool ISMASKSET(int variable, int mask)
        {
            return (bool)(((variable) & (mask)) != 0);
        }
        internal void SETMASK(int variable, int mask)
        {
            variable |= mask;
        }
        internal void CLEARMASK(int variable, int mask)
        {
            variable &= ~(mask);
        }
        internal new bool is_anti_degen(lprec lp, int testmask)
        {
            return ((bool)((lp.anti_degen == testmask) || ((lp.anti_degen & testmask) != 0)));
        }
        internal static new long get_total_iter(lprec lp)
        {
            return (lp.total_iter + lp.current_iter);
        }

        internal new static string get_str_piv_rule(int rule)
        {
            string[] pivotText = { "Bland first index", "Dantzig", "Devex", "Steepest Edge" };
            return (pivotText[rule]);
        }

        private new int get_piv_rule(lprec lp)
        {
            return ((lp.piv_strategy | PRICE_STRATEGYMASK) ^ PRICE_STRATEGYMASK);
        }

        internal new static bool isBasisVarFeasible(lprec lp, double tol, int basis_row)
        {
            int col;
            double x;
            bool Ok = true;
            bool? doSC = false;

            col = lp.var_basic[basis_row];
            x = lp.rhs[basis_row]; // The current solution of basic variables stored here!
            if ((x < -tol) || (x > lp.upbo[col] + tol))
            {
                Ok = false;
            }
            else if (doSC != null && (col > lp.rows) && (System.Math.Abs(lp.sc_lobound[col - lp.rows]) > 0))
            {
                if ((x > tol) && (x < System.Math.Abs(lp.sc_lobound[col - lp.rows]) - tol))
                {
                    Ok = false;
                }
            }
            return (Ok);
        }

        private new bool is_chsign(lprec lp, int rownr)
        {
            return ((bool)((lp.row_type[rownr] & ROWTYPE_CONSTRAINT) == ROWTYPE_CHSIGN));
        }

        internal bool add_columnex(lprec lp, int count, ref double?[] column, ref int?[] rowno)
        {
            /* This function adds a data column to the current model; three cases handled:

            1: Prepare for column data by setting column = NULL
            2: Dense vector indicated by (rowno == NULL) over 0..count+get_Lrows() elements
            3: Sparse vector set over row vectors rowno, over 0..count-1 elements.

           NB! If the column has only one entry, this should be handled as
               a bound, but this currently is not the case  */

            bool status = false;
            string msg;
            /* Prepare and shift column vectors */
            if (!append_columns(lp, 1))
            {
                return (status);
            }

            /* Append sparse regular constraint values */
            if (lp_matrix.mat_appendcol(lp.matA, count, column, rowno, 1.0, true) < 0)
            {
                msg = "add_columnex: Data column %d supplied in non-ascending row index order.\n";
                lp.report(lp, SEVERE, ref msg, lp.columns);
            }
            else
            {
                //C++ TO JAVA CONVERTER TODO TASK: There is no preprocessor in Java:
                ///#if Paranoia
                if (lp.columns != (lp.matA.is_roworder ? lp.matA.rows : lp.matA.columns))
                {
                    msg = "add_columnex: Column count mismatch %d vs %d\n";
                    lp.report(lp, SEVERE, ref msg, lp.columns, (lp.matA.is_roworder ? lp.matA.rows : lp.matA.columns));
                }
                else if (is_BasisReady(lp) && (lp.P1extraDim == 0) && !verify_basis(lp))
                {
                    msg = "add_columnex: Invalid basis detected for column %d\n";
                    lp.report(lp, SEVERE, ref msg, lp.columns);
                }
                else
                {
                    ///#endif
                    status = true;
                }
            }

            if (!lp.varmap_locked)
            {
                lp_presolve.presolve_setOrig(lp, lp.rows, lp.columns);
            }

            return (status);
        }

        internal new int set_basisvar(lprec lp, int basisPos, int enteringCol)
        {
            int leavingCol;
            string msg;

            leavingCol = lp.var_basic[basisPos];

            ///#if Paranoia
            if ((basisPos < 1) || (basisPos > lp.rows))
            {
                msg = "set_basisvar: Invalid leaving basis position %d specified at iter %.0f\n";
                lp.report(lp, SEVERE, ref msg, basisPos, (double)get_total_iter(lp));
            }
            if ((leavingCol < 1) || (leavingCol > lp.sum))
            {
                msg = "set_basisvar: Invalid leaving column %d referenced at iter %.0f\n";
                lp.report(lp, SEVERE, ref msg, leavingCol, (double)get_total_iter(lp));
            }
            if ((enteringCol < 1) || (enteringCol > lp.sum))
            {
                msg = "set_basisvar: Invalid entering column %d specified at iter %.0f\n";
                lp.report(lp, SEVERE, ref msg, enteringCol, (double)get_total_iter(lp));
            }
            ///#endif

            ///#if ParanoiaXY
            if (!lp.is_basic[leavingCol])
            {
                msg = "set_basisvar: Leaving variable %d is not basic at iter %.0f\n";
                lp.report(lp, IMPORTANT, ref msg, leavingCol, (double)get_total_iter(lp));
            }
            //NOTED ISSUE:
            if (enteringCol > lp.rows && lp.is_basic[enteringCol])
            {
                msg = "set_basisvar: Entering variable %d is already basic at iter %.0f\n";
                report(lp, IMPORTANT, ref msg, enteringCol, (double)get_total_iter(lp));
            }
            ///#endif

            lp.var_basic[0] = 0; // Set to signal that this is a non-default basis
            lp.var_basic[basisPos] = enteringCol;
            //NOTED ISSUE
            lp.is_basic[leavingCol] = false;
            lp.is_basic[enteringCol] = true;
            if (lp.bb_basis != null)
            {
                lp.bb_basis.pivots++;
            }

            return (leavingCol);
        }

        internal static new void set_action(ref int actionvar, int actionmask)
        {
            actionvar |= actionmask;
        }

        private new bool is_piv_mode(lprec lp, int testmask)
        {
            return ((bool)(((testmask & PRICE_STRATEGYMASK) != 0) && ((lp.piv_strategy & testmask) != 0)));
        }

        /* INLINE */
        internal new bool is_splitvar(lprec lp, int colnr)
        {
            /* Two cases handled by var_is_free:

           1) LB:-Inf / UB:<Inf variables
              No helper column created, sign of var_is_free set negative with index to itself.
           2) LB:-Inf / UB: Inf (free) variables
              Sign of var_is_free set positive with index to new helper column,
              helper column created with negative var_is_free with index to the original column.

           This function helps identify the helper column in 2).
        */
            //NOTED ISSUE:
            return ((bool)((lp.var_is_free != null) && (lp.var_is_free[colnr] < 0) && (-lp.var_is_free[colnr] != colnr)));
        }

        private new int get_columnex(lprec lp, int colnr, ref double[] column, ref int?[] nzrow)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "get_columnex: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return (-1);
            }

            if (lp.matA.is_roworder)
                return (mat_getrow(lp, colnr, ref column, ref nzrow));
            else
                return (mat_getcolumn(lp, colnr, ref column, ref nzrow));
        }

        internal new bool is_binary(lprec lp, int colnr)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "is_binary: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return false;
            }

            return ((bool)(((lp.var_type[colnr] & ISINTEGER)) && (get_lowbo(lp, colnr) == 0) && (System.Math.Abs(get_upbo(lp, colnr) - 1) < lprec.epsprimal)));
        }

        internal new bool is_unbounded(lprec lp, int colnr)
        {
            bool test;

            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "is_unbounded: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return false;
            }

            test = is_splitvar(lp, colnr);
            if (!test)
            {
                colnr += lp.rows;
                test = (bool)((lp.orig_lowbo[colnr] <= -lp.infinite) && (lp.orig_upbo[colnr] >= lp.infinite));
            }
            return (test);
        }

        internal new bool write_mps(lprec lp, ref string filename)
        {
            lp_MPS objlp_MPS = new lp_MPS();
            return (objlp_MPS.MPS_writefile(lp, MPSFIXED, ref filename));
        }

        private new static int get_basisOF(lprec lp, int[] coltarget, double[] crow, int[] colno)
        {

            /* Fill vector of basic OF values or subtract incoming values from these.
               This function is called twice during reduced cost updates when the basis
               does not contain the basic OF vector as the top row.  The colno[] array
               is filled with the count of non-zero values and the index to those. */

            int i;
            int n = lp.rows;
            int nz = 0;
            double[] obj = lp.obj;

            //ORIGINAL LINE: register REAL epsvalue = lp->epsvalue;
            double epsvalue = lprec.epsvalue;

            /* Compute offset over the specified objective indeces (step 2) */
            if (coltarget != null)
            {
                //ORIGINAL LINE: register int ix, m = coltarget[0];
                int ix;
                int m = coltarget[0];
                //ORIGINAL LINE: register REAL value;
                double value = new double();

                for (i = 1, coltarget[0]++; i <= m; i++, coltarget[0]++)
                {
                    ix = coltarget[0];
                    /* Finalize the computation of the reduced costs, based on the format that
                       duals are computed as negatives, ref description for step 1 above */
                    value = crow[ix];
                    if (ix > n)
                    {
                        value += obj[ix - n];
                    }
                    /*      if(value != 0) { */
                    if (System.Math.Abs(value) > epsvalue)
                    {
                        nz++;
                        if (colno != null)
                        {
                            colno[nz] = ix;
                        }
                    }
                    else
                    {
                        value = 0.0;
                    }
                    crow[ix] = value;
                }
            }

            /* Get the basic objective function values (step 1) */
            else
            {
                //ORIGINAL LINE: register int *basvar = lp->var_basic;

                int basvar = lp.var_basic[0];

                for (i = 1, crow[0]++, basvar++; i <= n; i++, crow[0]++, basvar++)
                {
                    /* Load the objective value of the active basic variable; note that we
                       change the sign of the value to maintain computational compatibility with
                       the calculation of duals using in-basis storage of the basic OF values */
                    if (basvar <= n)
                    {
                        crow[0] = 0;
                    }
                    else
                    {
                        crow[0] = -obj[(basvar) - n];
                    }
                    if (crow[0] != 0)
                    {
                        /*      if(fabs(*crow) > epsvalue) { */
                        nz++;
                        if (colno != null)
                        {
                            colno[nz] = i;
                        }
                    }
                }
            }
            if (colno != null)
            {
                colno[0] = nz;
            }
            return (nz);
        }

        /* Retrieve a column vector from the data matrix [1..rows, rows+1..rows+columns];
           needs __WINAPI call model since it may be called from BFPs */
        internal int obtain_column(lprec lp, int varin, ref double?[] pcol, ref int[] nzlist, ref int maxabs)
        {
            double value = lp_types.my_chsign(lp.is_lower[0], -1);
            if (varin > lp.rows)
            {
                varin -= lp.rows;
                varin = expand_column(lp, varin, ref pcol, ref nzlist, value, ref maxabs);
            }
            else if (lp.obj_in_basis || (varin > 0))
            {
                varin = singleton_column(lp, varin, ref pcol, ref nzlist, value, ref maxabs);
            }
            else
            {
                varin = get_basisOF(lp, null, pcol, nzlist);
            }

            return (varin);
        }

        internal static int expand_column(lprec lp, int col_nr, ref double?[] column, ref int[] nzlist, double mult, ref int maxabs)
        {
            int i;
            int ie;
            int j;
            int maxidx;
            int nzcount;
            double value;
            double maxval;
            MATrec mat = lp.matA;

            double matValue = new double();

            int matRownr;

            /* Retrieve a column from the user data matrix A */
            maxval = 0;
            maxidx = -1;
            if (nzlist == null)
            {
                //NOT REQUIRED
                //MEMCLEAR(column, lp.rows + 1);
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set first [] as 0 for now; need to check at run time
                i = mat.col_end[0][col_nr - 1];
                //set first [] as 0 for now
                ie = mat.col_end[0][col_nr];
                matRownr = lp_matrix.COL_MAT_ROWNR(i);
                matValue = lp_matrix.COL_MAT_VALUE(i);
                nzcount = i;
                for (; i < ie; i++, matRownr += commonlib.matRowColStep, matValue += commonlib.matValueStep)
                {
                    j = matRownr;
                    value = matValue;
                    if (j > 0)
                    {
                        value *= mult;
                        if (System.Math.Abs(value) > maxval)
                        {
                            maxval = System.Math.Abs(value);
                            maxidx = j;
                        }
                    }
                    column[j] = value;
                }
                nzcount = i - nzcount;

                /* Get the objective as row 0, optionally adjusting the objective for phase 1 */
                if (lp.obj_in_basis)
                {
                    column[0] = lp.get_OF_active(lp, lp.rows + col_nr, mult);
                    if (column[0] != 0)
                    {
                        nzcount++;
                    }
                }
            }
            else
            {
                nzcount = 0;

                /* Get the objective as row 0, optionally adjusting the objective for phase 1 */
                if (lp.obj_in_basis)
                {
                    value = lp.get_OF_active(lp, lp.rows + col_nr, mult);
                    if (value != 0)
                    {
                        nzcount++;
                        nzlist[nzcount] = 0;
                        column[nzcount] = value;
                    }
                }

                /* Loop over the non-zero column entries */
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set first [] as 0 for now; need to check at run time
                i = mat.col_end[0][col_nr - 1];
                ie = mat.col_end[0][col_nr];
                matRownr = lp_matrix.COL_MAT_ROWNR(i);
                matValue = lp_matrix.COL_MAT_VALUE(i);
                for (; i < ie; i++, matRownr += commonlib.matRowColStep, matValue += commonlib.matValueStep)
                {
                    j = matRownr;
                    value = (matValue) * mult;
                    nzcount++;
                    nzlist[nzcount] = j;
                    column[nzcount] = value;
                    if (System.Math.Abs(value) > maxval)
                    {
                        maxval = System.Math.Abs(value);
                        maxidx = nzcount;
                    }
                }
            }

            if (maxabs != null)
            {
                maxabs = maxidx;
            }
            return (nzcount);
        }

        internal static new double get_OF_active(lprec lp, int varnr, double mult)
        {
            int colnr = varnr - lp.rows;
            double holdOF = 0;
            string msg = "";

            ///#if Paranoia
            if ((colnr <= 0) || (colnr > lp.columns))
            {
                msg = "get_OF_active: Invalid column index %d supplied\n";
                lp.report(lp, SEVERE, ref msg, colnr);
            }
            else
            {
                ///#endif
                if (lp.obj == null)
                {
                    if (colnr > 0)
                    {
                        holdOF = lp.orig_obj[colnr];
                    }
                    modifyOF1(lp, varnr, ref holdOF, mult);
                }
                else if (colnr > 0)
                {
                    holdOF = lp.obj[colnr] * mult;
                }
            }

            return (holdOF);
        }

        internal static int singleton_column(lprec lp, int row_nr, ref double?[] column, ref int[] nzlist, double value, ref int maxabs)
        {
            int nz = 1;

            if (nzlist == null)
            {
                //NOT REQUIRED
                //MEMCLEAR(column, lp.rows + 1);
                column[row_nr] = value;
            }
            else
            {
                column[nz] = value;
                nzlist[nz] = row_nr;
            }

            if (maxabs != null)
            {
                maxabs = row_nr;
            }
            return (nz);
        }

        internal new static bool refactRecent(lprec lp)
        {
            int pivcount = lp.bfp_pivotcount(lp);
            if (pivcount == 0)
            {
                return (Convert.ToBoolean(DefineConstants.AUTOMATIC));
            }
            else if (pivcount < 2 * DEF_MAXPIVOTRETRY)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        internal static new void clear_action(ref int actionvar, int actionmask)
        {
            actionvar &= ~actionmask;
        }

        internal new bool del_column(lprec lp, int colnr)
        {
            bool preparecompact = (bool)(colnr < 0);
            string msg;

            if (preparecompact != null)
            {
                colnr = -colnr;
            }
            if ((colnr > lp.columns) || (colnr < 1))
            {
                msg = "del_column: Column %d out of range\n";
                lp.report(lp, IMPORTANT, ref msg, colnr);
                return (false);
            }
            /*
            if(lp->matA->is_roworder) {
              report(lp, IMPORTANT, "del_column: Cannot delete column while in row entry mode.\n");
              return(FALSE);
            }
            */

            if ((lp.var_is_free != null) && (Convert.ToInt32(lp.var_is_free) > 0))
            {
                del_column(lp, Convert.ToInt32(lp.var_is_free)); // delete corresponding split column (is always after this column)
            }

            varmap_delete(lp, Convert.ToInt32(lp_types.my_chsign(preparecompact, lp.rows + colnr)), -1, null);
            shift_coldata(lp, Convert.ToInt32(lp_types.my_chsign(preparecompact, colnr)), -1, null);
            if (!lp.varmap_locked)
            {
                lp_presolve.presolve_setOrig(lp, lp.rows, lp.columns);
                if (lp.names_used)
                {
                    del_varnameex(lp, lp.col_name, lp.columns, lp.colname_hashtab, colnr, null);
                }
            }
            ///#if Paranoia
            if (is_BasisReady(lp) && (lp.P1extraDim == 0) && !verify_basis(lp))
            {
                msg = "del_column: Invalid basis detected at column %d (%d)\n";
                lp.report(lp, SEVERE, ref msg, colnr, lp.columns);
            }
            ///#endif

            return (true);
        }

        internal new static bool verify_basis(lprec lp)
        {
            int i;
            int ii;
            int k = 0;
            bool result = false;

            for (i = 1; i <= lp.rows; i++)
            {
                ii = lp.var_basic[i];
                if ((ii < 1) || (ii > lp.sum) || !lp.is_basic[ii])
                {
                    k = i;
                    ii = 0;
                    goto Done;
                }
            }

            ii = lp.rows;
            for (i = 1; i <= lp.sum; i++)
            {
                if (lp.is_basic[i])
                {
                    ii--;
                }
            }
            //ORIGINAL LINE: result = (MYBOOL)(ii == 0);
            result = ((bool)(ii == 0));
            Done:
            ///#if false
            //  if(!result)
            //    ii = 0;
            ///#endif
            return (result);
        }

        internal new static void set_OF_p1extra(lprec lp, double p1extra)
        {
            int i;
            string msg;
            double value = new double();

            if (lp.spx_trace)
            {
                msg = "set_OF_p1extra: Set dual objective offset to %g at iter %.0f.\n";
                lp.report(lp, DETAILED, ref msg, p1extra, (double)lp.get_total_iter(lp));
            }
            lp.P1extraVal = p1extra;
            if (lp.obj == null)
            {
                //NOT REQUIRED
                //  allocREAL(lp, lp.obj, lp.columns_alloc + 1, 1);
            }
            for (i = 1, value = lp.obj[0] + 1; i <= lp.columns; i++, value++)
            {
                value = lp.orig_obj[i];
                lp.modifyOF1(lp, lp.rows + i, ref value, 1.0);
            }
        }

        internal static new bool modifyOF1(lprec lp, int index, ref double ofValue, double mult)
        /* Adjust objective function values for primal/dual phase 1, if appropriate */
        {
            bool accept = true;

            /* Primal simplex: Set user variables to zero or BigM-scaled */
            if (((lp.simplex_mode & SIMPLEX_Phase1_PRIMAL) != 0) && (System.Math.Abs(lp.P1extraDim) > 0))
            {
                ///#if ! Phase1EliminateRedundant
                if (lp.P1extraDim < 0)
                {
                    if (index > lp.sum + lp.P1extraDim)
                    {
                        accept = false;
                    }
                }
                else
                {
                    ///#endif
                    if ((index <= lp.sum - lp.P1extraDim) || (mult == 0))
                    {
                        if ((mult == 0) || (lp.bigM == 0))
                        {
                            accept = false;
                        }
                        else
                        {
                            (ofValue) /= lp.bigM;
                        }
                    }
                }
            }

            /* Dual simplex: Subtract P1extraVal from objective function values */
            else if (((lp.simplex_mode & SIMPLEX_Phase1_DUAL) != 0) && (index > lp.rows))
            {
                ///#if 1
                //                Can it introduce degeneracy in some cases? */

                if ((lp.P1extraVal != 0) && (lp.orig_obj[index - lp.rows] > 0))
                {
                    ofValue = 0;
                }
                else
                ///#endif
                {
                    ofValue -= lp.P1extraVal;

                    ///#if false
                    //      if(is_action(lp->anti_degen, ANTIDEGEN_RHSPERTURB))
                    //        *ofValue -= rand_uniform(lp, lp->epsperturb);
                    ///#endif
                }
            }

            /* Do scaling and test for zero */
            if (accept != null)
            {
                (ofValue) *= mult;
                if (System.Math.Abs(ofValue) < lp.epsmachine)
                {
                    (ofValue) = 0;
                    accept = false;
                }
            }
            else
            {
                (ofValue) = 0;
            }

            return (accept);
        }

        internal new static bool is_OF_nz(lprec lp, int colnr)
        {
            return ((bool)(lp.orig_obj[colnr] != 0));
        }

        /* This routine recomputes the basic variables using the full inverse */
        internal static void recompute_solution(lprec lp, bool shiftbounds)
        {
            /* Compute RHS = b - A(n)*x(n) */
            initialize_solution(lp, shiftbounds);

            /* Compute x(b) = Inv(B)*RHS (Ref. lp_solve inverse logic and Chvatal p. 121) */
            lp.bfp_ftran_normal(lp, ref lp.rhs[0], 0);
            if (!lp.obj_in_basis)
            {
                int i;
                int ib;
                int n = lp.rows;
                for (i = 1; i <= n; i++)
                {
                    ib = lp.var_basic[i];
                    if (ib > n)
                    {
                        lp.rhs[0] -= get_OF_active(lp, ib, lp.rhs[i]);
                    }
                }
            }

            /* Round the values (should not be greater than the factor used in bfp_pivotRHS) */
            lp_utils.roundVector(ref lp.rhs, lp.rows, lprec.epsvalue);

            clear_action(ref lp.spx_action, ACTION_RECOMPUTE);
        }

        /* Transform RHS by adjusting for the bound state of variables;
         optionally rebase upper bound, and account for this in later calls */
        internal static void initialize_solution(lprec lp, bool shiftbounds)
        {
            int i = 0;
            int k1;
            int k2;

            //ORIGINAL LINE: int *matRownr;
            int matRownr;
            int colnr;
            double theta = new double();
            double value = new double();
            double matValue;
            double loB = new double();
            double upB = new double();
            MATrec mat = lp.matA;
            string msg;
            LpCls objLpCls = new LpCls();
            lp_report objlp_report = new lp_report();

            /* Set bounding status indicators */
            if (lp.bb_bounds != null)
            {
                // FIX_f704961d-fce1-40f9-82dd-f53b69d68636 26/11/18
                if (shiftbounds && (INITSOL_SHIFTZERO == 0))
                {
                    if (lp.bb_bounds.UBzerobased)
                    {
                        msg = "initialize_solution: The upper bounds are already zero-based at refactorization %d\n";
                        lp.report(lp, SEVERE, ref msg, lp.bfp_refactcount(lp, commonlib.BFP_STAT_REFACT_TOTAL));
                    }
                    lp.bb_bounds.UBzerobased = true;
                }
                else if (!lp.bb_bounds.UBzerobased)
                {
                    msg = "initialize_solution: The upper bounds are not zero-based at refactorization %d\n";
                    lp.report(lp, SEVERE, ref msg, lp.bfp_refactcount(lp, commonlib.BFP_STAT_REFACT_TOTAL));
                }
            }

            /* Initialize the working RHS/basic variable solution vector */
            //NOTED IDDUE: Commented temp
            //i = objLpCls.is_action(lp.anti_degen, lp_lib.ANTIDEGEN_RHSPERTURB) && (lp.monitor != null) && lp.monitor.active;
            //NOTED IDDUE:
            // sizeof() NOT REQUIRED
            if (lp.rhs == lp.orig_rhs && i == 0)
            {
                //NOT REQUIRED
                //MEMCOPY(lp.rhs, lp.orig_rhs, lp.rows + 1);
            }
            else if (i > 0)
            {
                lp.rhs[0] = lp.orig_rhs[0];
                for (i = 1; i <= lp.rows; i++)
                {
                    if (objLpCls.is_constr_type(lp, i, EQ))
                    {
                        theta = lp_utils.rand_uniform(lp, lprec.epsvalue);
                    }
                    else
                    {
                        theta = lp_utils.rand_uniform(lp, lp.epsperturb);
                        /*        if(lp->orig_upbo[i] < lp->infinite)
                                  lp->orig_upbo[i] += theta; */
                    }
                    lp.rhs[i] = lp.orig_rhs[i] + theta;
                }
            }
            else
            {
                for (i = 0; i <= lp.rows; i++)
                {
                    lp.rhs[i] = lp.orig_rhs[i];
                }
            }

            /* Adjust active RHS for variables at their active upper/lower bounds */
            for (i = 1; i <= lp.sum; i++)
            {

                upB = lp.upbo[i];
                loB = lp.lowbo[i];

                /* Shift to "ranged" upper bound, tantamount to defining zero-based variables */
                /// <summary> FIX_f704961d-fce1-40f9-82dd-f53b69d68636 26/11/18
                /// PREVIOUS: if (shiftbounds == INITSOL_SHIFTZERO)
                /// ERROR IN PREVIOUS: Operator '==' cannot be applied to operands of type 'bool' and 'int'
                /// FIX 1: if (shiftbounds && (INITSOL_SHIFTZERO == 0))
                /// </ summary >
                if (shiftbounds && (INITSOL_SHIFTZERO == 0))
                {
                    if ((loB > -lp.infinite) && (upB < lp.infinite))
                    {
                        lp.upbo[i] -= loB;
                    }
                    if (lp.upbo[i] < 0)
                    {
                        msg = "initialize_solution: Invalid rebounding; variable {0} at refact {1}, iter %.0f\n";
                        objlp_report.report(lp, SEVERE, ref msg, i, lp.bfp_refactcount(lp, LpBFPLib.BFP_STAT_REFACT_TOTAL), (double)get_total_iter(lp));
                    }
                }

                /* Use "ranged" upper bounds */
                // FIX_f704961d-fce1-40f9-82dd-f53b69d68636 26/11/18
                else if (shiftbounds && (INITSOL_SHIFTZERO == 0))
                {
                    if ((loB > -lp.infinite) && (upB < lp.infinite))
                    {
                        upB += loB;
                    }
                }

                /* Shift upper bound back to original value */
                // FIX_f704961d-fce1-40f9-82dd-f53b69d68636 26/11/18
                else if (shiftbounds && (INITSOL_SHIFTZERO == 0))
                {
                    if ((loB > -lp.infinite) && (upB < lp.infinite))
                    {
                        lp.upbo[i] += loB;
                        upB += loB;
                    }
                    continue;
                }
                else
                {
                    msg = "initialize_solution: Invalid option value '{0}'\n";
                    objlp_report.report(lp, SEVERE, ref msg, shiftbounds);
                }

                /* Set the applicable adjustment */
                if (lp.is_lower[i])
                {
                    theta = loB;
                }
                else
                {
                    theta = upB;
                }


                /* Check if we need to pass through the matrix;
                   remember that basis variables are always lower-bounded */
                if (theta == 0)
                {
                    continue;
                }

                /* Do user and artificial variables */
                if (i > lp.rows)
                {

                    /* Get starting and ending indeces in the NZ vector */
                    colnr = i - lp.rows;
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set first [] as 0 for now; need to check at run time
                    k1 = mat.col_end[0][colnr - 1];
                    //set first [] as 0 for now
                    k2 = mat.col_end[0][colnr];
                    matRownr = lp_matrix.COL_MAT_ROWNR(k1);
                    matValue = lp_matrix.COL_MAT_VALUE(k1);

                    /* Get the objective as row 0, optionally adjusting the objective for phase 1 */
                    value = get_OF_active(lp, i, theta);
                    lp.rhs[0] -= value;

                    /* Do the normal case */
                    for (; k1 < k2; k1++, matRownr += lp_matrix.matRowColStep, matValue += lp_matrix.matValueStep)
                    {
                        lp.rhs[matRownr] -= theta * matValue;
                    }
                }

                /* Do slack variables (constraint "bounds")*/
                else
                {
                    lp.rhs[i] -= theta;
                }

            }

            /* Do final pass to get the maximum value */
            i = myblas.idamax(lp.rows, ref lp.rhs, 1);
            lp.rhsmax = System.Math.Abs(lp.rhs[i]);

            // FIX_f704961d-fce1-40f9-82dd-f53b69d68636 26/11/18
            if (shiftbounds && (INITSOL_SHIFTZERO == 0))
            {
                clear_action(ref lp.spx_action, ACTION_REBASE);
            }
        }

        internal new bool write_MPS(lprec lp, FileStream output)
        {
            lp_MPS objlp_MPS = new lp_MPS();
            return (objlp_MPS.MPS_writehandle(lp, MPSFIXED, output));
        }
        internal new bool write_freemps(lprec lp, ref string filename)
        {
            lp_MPS objlp_MPS = new lp_MPS();
            return (objlp_MPS.MPS_writefile(lp, MPSFREE, ref filename));
        }

        internal new bool write_freeMPS(lprec lp, FileStream output)
        {
            lp_MPS objlp_MPS = new lp_MPS();
            return (objlp_MPS.MPS_writehandle(lp, MPSFREE, output));
        }

        internal new bool write_lp(lprec lp, ref string filename)
        {
            lp_wlp objlp_wlp = new lp_wlp();
            return (objlp_wlp.LP_writefile(lp, ref filename));
        }

        internal new bool write_LP(lprec lp, FileStream output)
        {
            lp_wlp objlp_wlp = new lp_wlp();
            return (objlp_wlp.LP_writehandle(lp, output));
        }

#if !PARSER_LP
        internal new bool LP_readhandle(lprec[] lp, FileStream filename, int verbose, ref string lp_name)
        {
            return false;
        }
        internal new lprec read_lp(FileStream filename, int verbose, ref string lp_name)
        {
            return null;
        }
        internal new lprec read_LP(ref string filename, int verbose, ref string lp_name)
        {
            return (null);
        }
#endif

        internal new bool write_basis(lprec lp, ref string filename)
        {
            lp_MPS objlp_MPS = new lp_MPS();
            int typeMPS = MPSFIXED;
            return (objlp_MPS.MPS_writeBAS(lp, typeMPS, ref filename));
        }

        internal new bool read_basis(lprec lp, ref string filename, ref string info)
        {
            /*int typeMPS = MPSFIXED;

            lp_MPS objlp_MPS = new lp_MPS();
            typeMPS = MPS_readBAS(lp, typeMPS, filename, info);*/

            /* Code basis */
            /*if (typeMPS != 0)
            {
                set_action(lp.spx_action, ACTION_REBASE | ACTION_REINVERT | ACTION_RECOMPUTE);
                lp.basis_valid = 1; // Do not re-initialize basis on entering Solve
                lp.var_basic[0] = 0; // Set to signal that this is a non-default basis
            }
            return ((bool)typeMPS);*/
            throw new NotImplementedException();
        }

        internal new bool set_col_name(lprec lp, int colnr, ref string new_name)
        {
            lp_report objlp_report = new lp_report();
            if ((colnr > lp.columns + 1) || (colnr < 1))
            {
                string msg = "set_col_name: Column {0} out of range";
                objlp_report.report(lp, IMPORTANT, ref msg, colnr);
            }

            if ((colnr > lp.columns) && !append_columns(lp, colnr - lp.columns))
                return false;
            if (!lp.names_used)
                init_rowcol_names(lp);
            rename_var(lp, colnr, ref new_name, lp.col_name, lp.colname_hashtab);

            return true;
        }

        internal static new bool init_rowcol_names(lprec lp)
        {
            if (!lp.names_used)
            {
                //C++ TO C# CONVERTER TODO TASK: The memory management function 'calloc' has no equivalent in C#:
                lp.row_name = new hashelem[System.Runtime.InteropServices.Marshal.SizeOf(lp.col_name)];
                //C++ TO C# CONVERTER TODO TASK: The memory management function 'calloc' has no equivalent in C#:
                lp.col_name = new hashelem[System.Runtime.InteropServices.Marshal.SizeOf(lp.col_name)];
                lp.rowname_hashtab = lp_Hash.create_hash_table(lp.rows_alloc + 1, 0);
                lp.colname_hashtab = lp_Hash.create_hash_table(lp.columns_alloc + 1, 1);
                lp.names_used = true;
            }
            return true;
        }

        internal new bool set_add_rowmode(lprec lp, bool turnon)
        {
            if ((lp.solvecount == 0) && (turnon ^ lp.matA.is_roworder))
                return (lp_matrix.mat_transpose(lp.matA));
            else
                return false;
        }

        internal new string get_row_name(lprec lp, int rownr)
        {
            lp_report objlp_report = new lp_report();
            if ((rownr < 0) || (rownr > lp.rows + 1))
            {
                string msg = "get_row_name: Row {0} out of range";
                objlp_report.report(lp, IMPORTANT, ref msg, rownr);
                return (null);
            }

            if ((lp.presolve_undo.var_to_orig != null) && lp.wasPresolved)
            {
                if (lp.presolve_undo.var_to_orig[rownr] == 0)
                {
                    rownr = -rownr;
                }
                else
                {
                    rownr = lp.presolve_undo.var_to_orig[rownr];
                }
            }
            return (get_origrow_name(lp, rownr));
        }

        internal new string get_col_name(lprec lp, int colnr)
        {
            lp_report objlp_report = new lp_report();
            if ((colnr > lp.columns + 1) || (colnr < 1))
            {
                string msg = "get_col_name: Column {0} out of range";
                objlp_report.report(lp, IMPORTANT, ref msg, colnr);
                return (null);
            }

            if ((lp.presolve_undo.var_to_orig != null) && lp.wasPresolved)
            {
                if (lp.presolve_undo.var_to_orig[lp.rows + colnr] == 0)
                {
                    colnr = -colnr;
                }
                else
                {
                    colnr = lp.presolve_undo.var_to_orig[lp.rows + colnr];
                }
            }
            return (get_origcol_name(lp, colnr));
        }

        internal new bool set_BFP(lprec lp, ref string filename)
        /* (Re)mapping of basis factorization variant methods is done here */
        {
            int result = lp_types.LIB_LOADED;
            lp_report objlp_report = new lp_report();
            string msg = "";

            /* Release the BFP and basis if we are active */
            if (lp.invB != null)
                bfp_free(lp);

            //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if LoadInverseLib == TRUE
  if (lp.hBFP != null)
  {
#if WIN32
//C++ TO C# CONVERTER NOTE: There is no C# equivalent to 'FreeLibrary':
//	FreeLibrary(lp.hBFP);
#else
	dlclose(lp.hBFP);
#endif
	lp.hBFP = null;
  }
#endif


            if (filename == null)
            {
                if (!is_nativeBFP(lp))
                {
                    return true;
                }
#if !ExcludeNativeInverse
                lp.bfp_name = bfp_name;
                lp.bfp_compatible = bfp_compatible;
                lp.bfp_free = bfp_free;
                lp.bfp_resize = bfp_resize;
                lp.bfp_nonzeros = bfp_nonzeros;
                lp.bfp_memallocated = bfp_memallocated;
                lp.bfp_restart = bfp_restart;
                lp.bfp_mustrefactorize = bfp_mustrefactorize;
                lp.bfp_preparefactorization = bfp_preparefactorization;
                lp.bfp_factorize = bfp_factorize;
                lp.bfp_finishupdate = bfp_finishupdate;
                lp.bfp_ftran_normal = bfp_ftran_normal;
                lp.bfp_ftran_prepare = bfp_ftran_prepare;
                lp.bfp_btran_normal = bfp_btran_normal;
                lp.bfp_status = bfp_status;
                lp.bfp_implicitslack = bfp_implicitslack;
                lp.bfp_indexbase = bfp_indexbase;
                lp.bfp_rowoffset = bfp_rowoffset;
                lp.bfp_pivotmax = bfp_pivotmax;
                lp.bfp_init = bfp_init;
                lp.bfp_pivotalloc = bfp_pivotalloc;
                lp.bfp_colcount = bfp_colcount;
                lp.bfp_canresetbasis = bfp_canresetbasis;
                lp.bfp_finishfactorization = bfp_finishfactorization;
                lp.bfp_updaterefactstats = bfp_updaterefactstats;
                lp.bfp_prepareupdate = bfp_prepareupdate;
                lp.bfp_pivotRHS = bfp_pivotRHS;
                lp.bfp_btran_double = bfp_btran_double;
                lp.bfp_efficiency = bfp_efficiency;
                lp.bfp_pivotvector = bfp_pivotvector;
                lp.bfp_pivotcount = bfp_pivotcount;
                lp.bfp_refactcount = bfp_refactcount;
                lp.bfp_isSetI = bfp_isSetI;
                lp.bfp_findredundant = bfp_findredundant;
#endif
            }
            else
            {
                //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if LoadInverseLib == TRUE
#if WIN32
   /* Get a handle to the Windows DLL module. */
	lp.hBFP = LoadLibrary(filename);

   /* If the handle is valid, try to get the function addresses. */
	if (lp.hBFP != null)
	{
	  lp.bfp_compatible = (BFPbool_lpintintint) GetProcAddress(lp.hBFP, "bfp_compatible");
	  if (lp.bfp_compatible == null)
	  {
		result = LIB_NOINFO;
	  }
	  else if (lp.bfp_compatible(lp, BFPVERSION, MAJORVERSION, sizeof(REAL)))
	  {

	  lp.bfp_name = (BFPchar) GetProcAddress(lp.hBFP, "bfp_name");
	  lp.bfp_free = (BFP_lp) GetProcAddress(lp.hBFP, "bfp_free");
	  lp.bfp_resize = (BFPbool_lpint) GetProcAddress(lp.hBFP, "bfp_resize");
	  lp.bfp_nonzeros = (BFPint_lpbool) GetProcAddress(lp.hBFP, "bfp_nonzeros");
	  lp.bfp_memallocated = (BFPint_lp) GetProcAddress(lp.hBFP, "bfp_memallocated");
	  lp.bfp_restart = (BFPbool_lp) GetProcAddress(lp.hBFP, "bfp_restart");
	  lp.bfp_mustrefactorize = (BFPbool_lp) GetProcAddress(lp.hBFP, "bfp_mustrefactorize");
	  lp.bfp_preparefactorization = (BFPint_lp) GetProcAddress(lp.hBFP, "bfp_preparefactorization");
	  lp.bfp_factorize = (BFPint_lpintintboolbool) GetProcAddress(lp.hBFP, "bfp_factorize");
	  lp.bfp_finishupdate = (BFPbool_lpbool) GetProcAddress(lp.hBFP, "bfp_finishupdate");
	  lp.bfp_ftran_normal = (BFP_lprealint) GetProcAddress(lp.hBFP, "bfp_ftran_normal");
	  lp.bfp_ftran_prepare = (BFP_lprealint) GetProcAddress(lp.hBFP, "bfp_ftran_prepare");
	  lp.bfp_btran_normal = (BFP_lprealint) GetProcAddress(lp.hBFP, "bfp_btran_normal");
	  lp.bfp_status = (BFPint_lp) GetProcAddress(lp.hBFP, "bfp_status");
    lp.bfp_implicitslack = (BFPbool_lp) GetProcAddress(lp.hBFP, "bfp_implicitslack");
	  lp.bfp_indexbase = (BFPint_lp) GetProcAddress(lp.hBFP, "bfp_indexbase");
	  lp.bfp_rowoffset = (BFPint_lp) GetProcAddress(lp.hBFP, "bfp_rowoffset");
	  lp.bfp_pivotmax = (BFPint_lp) GetProcAddress(lp.hBFP, "bfp_pivotmax");
	  lp.bfp_init = (BFPbool_lpintintchar) GetProcAddress(lp.hBFP, "bfp_init");
	  lp.bfp_pivotalloc = (BFPbool_lpint) GetProcAddress(lp.hBFP, "bfp_pivotalloc");
	  lp.bfp_colcount = (BFPint_lp) GetProcAddress(lp.hBFP, "bfp_colcount");
	  lp.bfp_canresetbasis = (BFPbool_lp) GetProcAddress(lp.hBFP, "bfp_canresetbasis");
	  lp.bfp_finishfactorization = (BFP_lp) GetProcAddress(lp.hBFP, "bfp_finishfactorization");
	  lp.bfp_updaterefactstats = (BFP_lp) GetProcAddress(lp.hBFP, "bfp_updaterefactstats");
	  lp.bfp_prepareupdate = (BFPlreal_lpintintreal) GetProcAddress(lp.hBFP, "bfp_prepareupdate");
	  lp.bfp_pivotRHS = (BFPreal_lplrealreal) GetProcAddress(lp.hBFP, "bfp_pivotRHS");
	  lp.bfp_btran_double = (BFP_lprealintrealint) GetProcAddress(lp.hBFP, "bfp_btran_double");
	  lp.bfp_efficiency = (BFPreal_lp) GetProcAddress(lp.hBFP, "bfp_efficiency");
	  lp.bfp_pivotvector = (BFPrealp_lp) GetProcAddress(lp.hBFP, "bfp_pivotvector");
	  lp.bfp_pivotcount = (BFPint_lp) GetProcAddress(lp.hBFP, "bfp_pivotcount");
	  lp.bfp_refactcount = (BFPint_lpint) GetProcAddress(lp.hBFP, "bfp_refactcount");
	  lp.bfp_isSetI = (BFPbool_lp) GetProcAddress(lp.hBFP, "bfp_isSetI");
	  lp.bfp_findredundant = (BFPint_lpintrealcbintint) GetProcAddress(lp.hBFP, "bfp_findredundant");
}
	  else
		result = LIB_VERINVALID;
	}
#else
   /* First standardize UNIX .SO library name format. */
	string bfpname = new string(new char[260]);
//C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
	char * ptr;

	bfpname = filename;
	if ((ptr = StringFunctions.StrRChr(filename, '/')) == null)
	{
	  ptr = filename;
	}
	else
	{
	  ptr++;
	}
	bfpname = bfpname.Substring(0, (int)(ptr - filename));
	if (string.Compare(ptr, 0, "lib", 0, 3))
	{
	  bfpname += "lib";
	}
	bfpname += ptr;
	if (string.Compare(bfpname.Substring(bfpname.Length) - 3, ".so"))
	{
	  bfpname += ".so";
	}

   /* Get a handle to the module. */
	lp.hBFP = dlopen(bfpname, RTLD_LAZY);

   /* If the handle is valid, try to get the function addresses. */
	if (lp.hBFP != null)
	{
	  lp.bfp_compatible = (BFPbool_lpintintint) dlsym(lp.hBFP, "bfp_compatible");
	  if (lp.bfp_compatible == null)
	  {
		result = LIB_NOINFO;
	  }
	  else if (lp.bfp_compatible(lp, BFPVERSION, MAJORVERSION, sizeof(REAL)))
	  {

	  lp.bfp_name = (BFPchar) dlsym(lp.hBFP, "bfp_name");
	  lp.bfp_free = (BFP_lp) dlsym(lp.hBFP, "bfp_free");
	  lp.bfp_resize = (BFPbool_lpint) dlsym(lp.hBFP, "bfp_resize");
	  lp.bfp_nonzeros = (BFPint_lpbool) dlsym(lp.hBFP, "bfp_nonzeros");
	  lp.bfp_memallocated = (BFPint_lp) dlsym(lp.hBFP, "bfp_memallocated");
	  lp.bfp_restart = (BFPbool_lp) dlsym(lp.hBFP, "bfp_restart");
	  lp.bfp_mustrefactorize = (BFPbool_lp) dlsym(lp.hBFP, "bfp_mustrefactorize");
	  lp.bfp_preparefactorization = (BFPint_lp) dlsym(lp.hBFP, "bfp_preparefactorization");
	  lp.bfp_factorize = (BFPint_lpintintboolbool) dlsym(lp.hBFP, "bfp_factorize");
	  lp.bfp_finishupdate = (BFPbool_lpbool) dlsym(lp.hBFP, "bfp_finishupdate");
	  lp.bfp_ftran_normal = (BFP_lprealint) dlsym(lp.hBFP, "bfp_ftran_normal");
	  lp.bfp_ftran_prepare = (BFP_lprealint) dlsym(lp.hBFP, "bfp_ftran_prepare");
	  lp.bfp_btran_normal = (BFP_lprealint) dlsym(lp.hBFP, "bfp_btran_normal");
	  lp.bfp_status = (BFPint_lp) dlsym(lp.hBFP, "bfp_status");
	  lp.bfp_implicitslack = (BFPbool_lp) dlsym(lp.hBFP, "bfp_implicitslack");
	  lp.bfp_indexbase = (BFPint_lp) dlsym(lp.hBFP, "bfp_indexbase");
	  }

//----------------------------------------------------------------------------------------
//	Copyright © 2006 - 2018 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class provides the ability to replicate various classic C string functions
//	which don't have exact equivalents in the .NET Framework.
//----------------------------------------------------------------------------------------
internal static class StringFunctions
{
	//------------------------------------------------------------------------------------
	//	This method allows replacing a single character in a string, to help convert
	//	C++ code where a single character in a character array is replaced.
	//------------------------------------------------------------------------------------
	public static string ChangeCharacter(string sourceString, int charIndex, char newChar)
	{
		return (charIndex > 0 ? sourceString.Substring(0, charIndex) : "")
			+ newChar.ToString() + (charIndex < sourceString.Length - 1 ? sourceString.Substring(charIndex + 1) : "");
	}

	//------------------------------------------------------------------------------------
	//	This method replicates the classic C string function 'isxdigit' (and 'iswxdigit').
	//------------------------------------------------------------------------------------
	public static bool IsXDigit(char character)
	{
		if (char.IsDigit(character))
			return true;
		else if ("ABCDEFabcdef".IndexOf(character) > -1)
			return true;
		else
			return false;
	}

	//------------------------------------------------------------------------------------
	//	This method replicates the classic C string function 'strchr' (and 'wcschr').
	//------------------------------------------------------------------------------------
	public static string StrChr(string stringToSearch, char charToFind)
	{
		int index = stringToSearch.IndexOf(charToFind);
		if (index > -1)
			return stringToSearch.Substring(index);
		else
			return null;
	}

	//------------------------------------------------------------------------------------
	//	This method replicates the classic C string function 'strrchr' (and 'wcsrchr').
	//------------------------------------------------------------------------------------
	public static string StrRChr(string stringToSearch, char charToFind)
	{
		int index = stringToSearch.LastIndexOf(charToFind);
		if (index > -1)
			return stringToSearch.Substring(index);
		else
			return null;
	}

	//------------------------------------------------------------------------------------
	//	This method replicates the classic C string function 'strstr' (and 'wcsstr').
	//------------------------------------------------------------------------------------
	public static string StrStr(string stringToSearch, string stringToFind)
	{
		int index = stringToSearch.IndexOf(stringToFind);
		if (index > -1)
			return stringToSearch.Substring(index);
		else
			return null;
	}

	//------------------------------------------------------------------------------------
	//	This method replicates the classic C string function 'strtok' (and 'wcstok').
	//	Note that the .NET string 'Split' method cannot be used to replicate 'strtok' since
	//	it doesn't allow changing the delimiters between each token retrieval.
	//------------------------------------------------------------------------------------
	private static string activeString;
	private static int activePosition;
	public static string StrTok(string stringToTokenize, string delimiters)
	{
		if (stringToTokenize != null)
		{
			activeString = stringToTokenize;
			activePosition = -1;
		}

		//the stringToTokenize was never set:
		if (activeString == null)
			return null;

		//all tokens have already been extracted:
		if (activePosition == activeString.Length)
			return null;

		//bypass delimiters:
		activePosition++;
		while (activePosition < activeString.Length && delimiters.IndexOf(activeString[activePosition]) > -1)
		{
			activePosition++;
		}

		//only delimiters were left, so return null:
		if (activePosition == activeString.Length)
			return null;

		//get starting position of string to return:
		int startingPosition = activePosition;

		//read until next delimiter:
		do
		{
			activePosition++;
		} while (activePosition < activeString.Length && delimiters.IndexOf(activeString[activePosition]) == -1);

		return activeString.Substring(startingPosition, activePosition - startingPosition);
	}
    lp.bfp_rowoffset = (BFPint_lp) dlsym(lp.hBFP, "bfp_rowoffset");
	  lp.bfp_pivotmax = (BFPint_lp) dlsym(lp.hBFP, "bfp_pivotmax");
	  lp.bfp_init = (BFPbool_lpintintchar) dlsym(lp.hBFP, "bfp_init");
	  lp.bfp_pivotalloc = (BFPbool_lpint) dlsym(lp.hBFP, "bfp_pivotalloc");
	  lp.bfp_colcount = (BFPint_lp) dlsym(lp.hBFP, "bfp_colcount");
	  lp.bfp_canresetbasis = (BFPbool_lp) dlsym(lp.hBFP, "bfp_canresetbasis");
	  lp.bfp_finishfactorization = (BFP_lp) dlsym(lp.hBFP, "bfp_finishfactorization");
	  lp.bfp_updaterefactstats = (BFP_lp) dlsym(lp.hBFP, "bfp_updaterefactstats");
	  lp.bfp_prepareupdate = (BFPlreal_lpintintreal) dlsym(lp.hBFP, "bfp_prepareupdate");
	  lp.bfp_pivotRHS = (BFPreal_lplrealreal) dlsym(lp.hBFP, "bfp_pivotRHS");
	  lp.bfp_btran_double = (BFP_lprealintrealint) dlsym(lp.hBFP, "bfp_btran_double");
	  lp.bfp_efficiency = (BFPreal_lp) dlsym(lp.hBFP, "bfp_efficiency");
	  lp.bfp_pivotvector = (BFPrealp_lp) dlsym(lp.hBFP, "bfp_pivotvector");
	  lp.bfp_pivotcount = (BFPint_lp) dlsym(lp.hBFP, "bfp_pivotcount");
	  lp.bfp_refactcount = (BFPint_lpint) dlsym(lp.hBFP, "bfp_refactcount");
	  lp.bfp_isSetI = (BFPbool_lp) dlsym(lp.hBFP, "bfp_isSetI");
	  lp.bfp_findredundant = (BFPint_lpintrealcbintint) dlsym(lp.hBFP, "bfp_findredundant");
}
	  else
		result = LIB_VERINVALID;
	}
#endif
	else
	  result = lp_types.LIB_NOTFOUND;
#endif
                /* Do validation */
                if ((result != lp_types.LIB_LOADED) || ((lp.bfp_name == null) || (lp.bfp_compatible == null) || (lp.bfp_free == null) || (lp.bfp_resize == null) || (lp.bfp_nonzeros == null) || (lp.bfp_memallocated == null) || (lp.bfp_restart == null) || (lp.bfp_mustrefactorize == null) || (lp.bfp_preparefactorization == null) || (lp.bfp_factorize == null) || (lp.bfp_finishupdate == null) || (lp.bfp_ftran_normal == null) || (lp.bfp_ftran_prepare == null) || (lp.bfp_btran_normal == null) || (lp.bfp_status == null) || (lp.bfp_implicitslack == null) || (lp.bfp_indexbase == null) || (lp.bfp_rowoffset == null) || (lp.bfp_pivotmax == null) || (lp.bfp_init == null) || (lp.bfp_pivotalloc == null) || (lp.bfp_colcount == null) || (lp.bfp_canresetbasis == null) || (lp.bfp_finishfactorization == null) || (lp.bfp_updaterefactstats == null) || (lp.bfp_prepareupdate == null) || (lp.bfp_pivotRHS == null) || (lp.bfp_btran_double == null) || (lp.bfp_efficiency == null) || (lp.bfp_pivotvector == null) || (lp.bfp_pivotcount == null) || (lp.bfp_refactcount == null) || (lp.bfp_isSetI == null) || (lp.bfp_findredundant == null)))
                {
                    filename = null;
                    set_BFP(lp, ref filename);
                    if (result == lp_types.LIB_LOADED)
                    {
                        result = lp_types.LIB_NOFUNCTION;
                    }
                }
            }
            if (filename != null)
            {
                string info = new string(new char[lp_types.LIB_STR_MAXLEN + 1]);
                switch (result)
                {
                    case lp_types.LIB_NOTFOUND:
                        info = lp_types.LIB_STR_NOTFOUND;
                        break;
                    case lp_types.LIB_NOINFO:
                        info = lp_types.LIB_STR_NOINFO;
                        break;
                    case lp_types.LIB_NOFUNCTION:
                        info = lp_types.LIB_STR_NOFUNCTION;
                        break;
                    case lp_types.LIB_VERINVALID:
                        info = lp_types.LIB_STR_VERINVALID;
                        break;
                    default:
                        info = lp_types.LIB_STR_LOADED;
                        break;
                }
                msg = "set_BFP: {0} '{1}'\n";
                objlp_report.report(lp, IMPORTANT, ref msg, info, filename);
            }
            return ((bool)(result == lp_types.LIB_LOADED));
        }

        internal new bool is_piv_rule(lprec lp, int rule)
        {
            return ((bool)(get_piv_rule(lp) == rule));
        }

        internal new static bool check_degeneracy(lprec lp, ref double?[] pcol, ref int degencount)
        {
            /* Check if the entering column Pi=Inv(B)*a is likely to produce improvement;
           (cfr. Istvan Maros: CTOTSM p. 233) */

            int i;
            int ndegen;

            double rhs = new double();
            double? sdegen = new double();
            double epsmargin = lprec.epsprimal;

            sdegen = 0;
            ndegen = 0;
            //changed on 28/11/18 (D)
            rhs = lp.rhs[0];
            for (i = 1; i <= lp.rows; i++)
            {
                rhs++;
                pcol[i]++;
                if (System.Math.Abs(rhs) < epsmargin)
                {
                    sdegen += pcol[i];
                    ndegen++;
                }
                else if (System.Math.Abs((rhs) - lp.upbo[lp.var_basic[i]]) < epsmargin)
                {
                    sdegen -= pcol[i];
                    ndegen++;
                }
            }
            if (degencount != null)
            {
                degencount = ndegen;
            }
            /*  sdegen += epsmargin*ndegen; */
            return ((bool)(sdegen <= 0));
        }

        internal static int compute_theta(lprec lp, int rownr, ref double theta, bool isupbound, double HarrisScalar, bool primal)
        {
            /* The purpose of this routine is to compute the non-basic bound state / value of
           the leaving variable. Note that the incoming theta is "d" in Chvatal-terminology */

            int colnr = lp.var_basic[rownr];

            //ORIGINAL LINE: register double x = lp->rhs[rownr];
            double x = lp.rhs[rownr];
            double lb = 0; // Primal feasibility tolerance
            double ub = lp.upbo[colnr];
            double eps = lprec.epsprimal;

            /* Compute theta for the primal simplex */
            HarrisScalar *= eps;
            if (primal != null)
            {

                if (theta > 0)
                {
                    x -= lb - HarrisScalar; // A positive number
                }
                else if (ub < lp.infinite)
                {
                    x -= ub + HarrisScalar; // A negative number
                }
                else
                {
                    theta = -lp.infinite;
                    return (colnr);
                }
            }
            /* Compute theta for the dual simplex */
            else
            {

                if (isupbound != false)
                {
                    theta = -(theta);
                }

                /* Current value is below or equal to its lower bound */
                if (x < lb + eps)
                {
                    x -= lb - HarrisScalar;
                }

                /* Current value is above or equal to its upper bound */
                else if (x > ub - eps)
                {
                    if (ub >= lp.infinite)
                    {
                        theta = lp.infinite * lp_types.my_sign(theta);
                        return (colnr);
                    }
                    else
                    {
                        x -= ub + HarrisScalar;
                    }
                }
            }
            lp_types.my_roundzero(x, lp.epsmachine);
            theta = x / theta;

            ///#if EnforcePositiveTheta
            /* Check if we have negative theta due to rounding or an internal error */
            if (theta < 0)
            {
                if (primal != null && (ub == lb))
                {
                    lp.rhs[rownr] = lb;
                }
                else
                {
                    ///#if Paranoia
                    if (theta < -eps)
                    {
                        string msg = "compute_theta: Negative theta (%g) not allowed in base-0 version of lp_solve\n";
                        lp.report(lp, DETAILED, ref msg, theta);
                    }
                }
                ///#endif
                theta = 0;
            }
            ///#endif

            return (colnr);
        }

        private new static void varmap_clear(lprec lp)
        {
            lp_presolve.presolve_setOrig(lp, 0, 0);
            lp.varmap_locked = false;
        }

        internal new static bool inc_col_space(lprec lp, int deltacols)
        {
            int i;
            int colsum;
            int oldcolsalloc;

            i = lp.columns_alloc + deltacols;
            if (lp.matA.is_roworder)
            {
                i -= lp.matA.rows_alloc;
                commonlib.SETMIN(i, deltacols);
                if (i > 0)
                {
                    lp_matrix.inc_matrow_space(lp.matA, i);
                }
                colsum = lp.matA.rows_alloc;
            }
            else
            {
                i -= lp.matA.columns_alloc;
                commonlib.SETMIN(i, deltacols);
                if (i > 0)
                {
                    lp_matrix.inc_matcol_space(lp.matA, i);
                }
                colsum = lp.matA.columns_alloc;
            }

            if (lp.columns + deltacols >= lp.columns_alloc)
            {

                colsum++;
                oldcolsalloc = lp.columns_alloc;
                lp.columns_alloc = colsum;
                deltacols = colsum - oldcolsalloc;
                colsum++;

                /* Adjust hash name structures */
                if (lp.names_used && (lp.col_name != null))
                {

                    /* First check the hash table */
                    if (lp.colname_hashtab.size < lp.columns_alloc)
                    {
                        hashtable ht;

                        ht = lp_Hash.copy_hash_table(lp.colname_hashtab, lp.col_name[0], lp.columns_alloc + 1);
                        if (ht != null)
                        {
                            lp_Hash.free_hash_table(lp.colname_hashtab);
                            lp.colname_hashtab = ht;
                        }
                    }

                    /* Then the string storage (i.e. pointer to the item's hash structure) */
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'realloc' has no equivalent in C#:
                    /* NOT REQUIRED
                    lp.col_name = (hashelem)realloc(lp.col_name, (colsum) * sizeof(*lp.col_name));
                    */
                    for (i = oldcolsalloc + 1; i < colsum; i++)
                    {
                        lp.col_name[i] = null;
                    }
                }
                /* NOT REQUIRED
                if (!allocREAL(lp, lp.orig_obj, colsum, AUTOMATIC) || !allocbool(lp, lp.var_type, colsum, AUTOMATIC) || !allocREAL(lp, lp.sc_lobound, colsum, AUTOMATIC) || ((lp.obj != null) && !allocREAL(lp, lp.obj, colsum, AUTOMATIC)) || ((lp.var_priority != null) && !allocINT(lp, lp.var_priority, colsum - 1, AUTOMATIC)) || ((lp.var_is_free != null) && !allocINT(lp, lp.var_is_free, colsum, AUTOMATIC)) || ((lp.bb_varbranch != null) && !allocbool(lp, lp.bb_varbranch, colsum - 1, AUTOMATIC)))
                {
                    return (0);
                }
                */

                /* NOT REQUIRED
                 //Make sure that Lagrangean constraints have the same number of columns 
                if (get_Lrows(lp) > 0)
                {
                    inc_lag_space(lp, 0, 0);
                }
                */

                /* Update column pointers */
                for (i = (int)commonlib.MIN(oldcolsalloc, lp.columns) + 1; i < colsum; i++)
                {
                    lp.orig_obj[i] = 0;
                    if (lp.obj != null)
                    {
                        lp.obj[i] = 0;
                    }
                    lp.var_type[i] = ISREAL;
                    lp.sc_lobound[i] = 0;
                    if (lp.var_priority != null)
                    {
                        lp.var_priority[i - 1] = i;
                    }
                }

                if (lp.var_is_free != null)
                {
                    for (i = oldcolsalloc + 1; i < colsum; i++)
                    {
                        lp.var_is_free[i] = 0;
                    }
                }

                if (lp.bb_varbranch != null)
                {
                    for (i = oldcolsalloc; i < colsum - 1; i++)
                    {
                        lp.bb_varbranch[i] = BRANCH_DEFAULT;
                    }
                }

                inc_rowcol_space(lp, deltacols, false);

            }
            return true;
        }

        private static bool inc_rowcol_space(lprec lp, int delta, bool isrows)
        {
            int i;
            int oldrowcolalloc;
            int rowcolsum;

            /*NOT REQUIRED
            // Get rid of dual arrays 
            if (lp.solvecount > 0)
            {
                free_duals(lp);
            }
            */

            /* Set constants */
            oldrowcolalloc = lp.sum_alloc;
            lp.sum_alloc += delta;
            rowcolsum = lp.sum_alloc + 1;

            /*NOT REQUIRED
            // Reallocate lp memory 
            if (!allocREAL(lp, lp.upbo, rowcolsum, AUTOMATIC) || !allocREAL(lp, lp.orig_upbo, rowcolsum, AUTOMATIC) || !allocREAL(lp, lp.lowbo, rowcolsum, AUTOMATIC) || !allocREAL(lp, lp.orig_lowbo, rowcolsum, AUTOMATIC) || !allocREAL(lp, lp.solution, rowcolsum, AUTOMATIC) || !allocREAL(lp, lp.best_solution, rowcolsum, AUTOMATIC) || !allocbool(lp, lp.is_basic, rowcolsum, AUTOMATIC) || !allocbool(lp, lp.is_lower, rowcolsum, AUTOMATIC) || ((lp.scalars != null) && !allocREAL(lp, lp.scalars, rowcolsum, AUTOMATIC)))
            {
                return (0);
            }
            */

            /* Fill in default values, where appropriate */
            for (i = oldrowcolalloc + 1; i < rowcolsum; i++)
            {
                lp.upbo[i] = lp.infinite;
                lp.orig_upbo[i] = lp.upbo[i];
                lp.lowbo[i] = 0;
                lp.orig_lowbo[i] = lp.lowbo[i];
                lp.is_basic[i] = false;
                lp.is_lower[i] = true;
            }

            /* Deal with scalars; the vector can be NULL and also contains Lagrangean information */
            if (lp.scalars != null)
            {
                for (i = oldrowcolalloc + 1; i < rowcolsum; i++)
                {
                    lp.scalars[i] = 1;
                }
                if (oldrowcolalloc == 0)
                {
                    lp.scalars[0] = 1;
                }
            }

            return (lp_presolve.inc_presolve_space(lp, delta, isrows) && LpPricePSE.resizePricer(lp));
        }

        internal new static bool inc_row_space(lprec lp, int deltarows)
        {
            int i;
            int rowsum;
            int oldrowsalloc;
            bool ok = true;

            /* Adjust lp row structures */
            i = lp.rows_alloc + deltarows;
            if (lp.matA.is_roworder)
            {
                i -= lp.matA.columns_alloc;
                commonlib.SETMIN(i, deltarows);
                if (i > 0)
                {
                    lp_matrix.inc_matcol_space(lp.matA, i);
                }
                rowsum = lp.matA.columns_alloc;
            }
            else
            {
#if false
//    if((lp->rows_alloc > 0) && (lp->rows + deltarows > lp->rows_alloc))
//      i = deltarows; // peno 25/12/06 
//    else
#endif
                i -= lp.matA.rows_alloc;
                commonlib.SETMIN(i, deltarows);
                if (i > 0)
                {
                    lp_matrix.inc_matrow_space(lp.matA, i);
                }
                rowsum = lp.matA.rows_alloc;
            }
            if (lp.rows + deltarows > lp.rows_alloc)
            {

                rowsum++;
                oldrowsalloc = lp.rows_alloc;
                lp.rows_alloc = rowsum;
                deltarows = rowsum - oldrowsalloc;
                rowsum++;

                /*NOT REQUIRED
                if (!allocREAL(lp, lp.orig_rhs, rowsum, lp_types.AUTOMATIC) || !allocLREAL(lp, lp.rhs, rowsum, lp_types.AUTOMATIC) || !allocINT(lp, lp.row_type, rowsum, lp_types.AUTOMATIC) || !allocINT(lp, lp.var_basic, rowsum, lp_types.AUTOMATIC))
                {
                    return (0);
                }
                */

                if (oldrowsalloc == 0)
                {
                    lp.var_basic[0] = lp_types.AUTOMATIC; // Indicates default basis
                    lp.orig_rhs[0] = 0;
                    lp.row_type[0] = ROWTYPE_OFMIN;
                }
                for (i = oldrowsalloc + 1; i < rowsum; i++)
                {
                    lp.orig_rhs[i] = 0;
                    lp.rhs[i] = 0;
                    lp.row_type[i] = ROWTYPE_EMPTY;
                    lp.var_basic[i] = i;
                }

                /* Adjust hash name structures */
                if (lp.names_used && (lp.row_name != null))
                {

                    /* First check the hash table */
                    if (lp.rowname_hashtab.size < lp.rows_alloc)
                    {
                        hashtable ht;

                        ht = lp_Hash.copy_hash_table(lp.rowname_hashtab, lp.row_name[0], lp.rows_alloc + 1);
                        if (ht == null)
                        {
                            lp.spx_status = NOMEMORY;
                            return false;
                        }
                        lp_Hash.free_hash_table(lp.rowname_hashtab);
                        lp.rowname_hashtab = ht;
                    }

                    /* Then the string storage (i.e. pointer to the item's hash structure) */
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'realloc' has no equivalent in C#:
                    /* NOT REQUIRED
                    lp.row_name = (hashelem)realloc(lp.row_name, (rowsum) * sizeof(*lp.row_name));
                    */
                    if (lp.row_name == null)
                    {
                        lp.spx_status = NOMEMORY;
                        return false;
                    }
                    for (i = oldrowsalloc + 1; i < rowsum; i++)
                    {
                        lp.row_name[i] = null;
                    }
                }

                ok = inc_rowcol_space(lp, deltarows, true);

            }
            return (ok);
        }

        internal new lprec[] read_freemps(FileStream filename, int options)
        {
            lprec[] lp = null;
            int typeMPS;
            lp_MPS objlp_MPS = new lp_MPS();

            typeMPS = (options & ~0x07) >> 2;
            typeMPS &= ~MPSFIXED;
            typeMPS |= MPSFREE;
            if (objlp_MPS.MPS_readhandle(lp, filename, typeMPS, options & 0x07))
            {
                return (lp);
            }
            else
            {
                return (null);
            }
        }

        internal new lprec[] read_mpsex(object userhandle, read_modeldata_func read_modeldata, int options)
        {
            lprec[] lp = null;
            int typeMPS;
            lp_MPS objlp_MPS = new lp_MPS();

            typeMPS = (options & ~0x07) >> 2;
            if ((typeMPS & (MPSFIXED | MPSFREE)) == 0)
            {
                typeMPS |= MPSFIXED;
            }
            if (objlp_MPS.MPS_readex(lp, userhandle, read_modeldata, typeMPS, options & 0x07))
            {
                return (lp);
            }
            else
            {
                return (null);
            }
        }

        internal new static bool append_columns(lprec lp, int deltacolumns)
        {
            if (!inc_col_space(lp, deltacolumns))
            {
                return false;
            }
            varmap_add(lp, lp.sum + 1, deltacolumns);
            shift_coldata(lp, lp.columns + 1, deltacolumns, null);
            return true;
        }

        internal new static bool shift_coldata(lprec lp, int @base, int delta, LLrec usedmap)
        /* Note: Assumes that "lp->columns" has NOT been updated to the new count */
        {
            int i;
            int ii;

            /*NOT REQUIRED
            if (lp.bb_totalnodes == 0)
            {
                free_duals(lp);
            }
            */

            /* Shift A matrix data */
            if (lp.matA.is_roworder)
            {
                lp_matrix.mat_shiftrows(lp.matA, ref @base, delta, usedmap);
            }
            else
            {
                lp_matrix.mat_shiftcols(lp.matA, ref @base, delta, usedmap);
            }

            /* Shift data right (insert), and set default values in positive delta-gap */
            if (delta > 0)
            {

                /* Fix variable priority data */
                if ((lp.var_priority != null) && (@base <= lp.columns))
                {
                    for (i = 0; i < lp.columns; i++)
                    {
                        if (lp.var_priority[i] >= @base)
                        {
                            lp.var_priority[i] += delta;
                        }
                    }
                }
                if ((lp.sos_priority != null) && (@base <= lp.columns))
                {
                    for (i = 0; i < lp.sos_vars; i++)
                    {
                        if (lp.sos_priority[i] >= @base)
                        {
                            lp.sos_priority[i] += delta;
                        }
                    }
                }

                /* Fix invalid split variable data */
                if ((lp.var_is_free != null) && (@base <= lp.columns))
                {
                    for (i = 1; i <= lp.columns; i++)
                    {
                        if (System.Math.Abs((sbyte)lp.var_is_free[i]) >= @base)
                        {
                            lp.var_is_free[i] += (int)lp_types.my_chsign(lp.var_is_free[i] < 0, delta);
                        }
                    }
                }

                /* Shift column data right */
                for (ii = lp.columns; ii >= @base; ii--)
                {
                    i = ii + delta;
                    lp.var_type[i] = lp.var_type[ii];
                    lp.sc_lobound[i] = lp.sc_lobound[ii];
                    lp.orig_obj[i] = lp.orig_obj[ii];
                    if (lp.obj != null)
                    {
                        lp.obj[i] = lp.obj[ii];
                    }
                    /*
                          if(lp->objfromvalue != NULL)
                            lp->objfromvalue[i] = lp->objfromvalue[ii];
                          if(lp->objfrom != NULL)
                            lp->objfrom[i] = lp->objfrom[ii];
                          if(lp->objtill != NULL)
                            lp->objtill[i] = lp->objtill[ii];
                    */
                    if (lp.var_priority != null)
                    {
                        lp.var_priority[ii - 1] = ii;
                    }
                    if (lp.bb_varbranch != null)
                    {
                        lp.bb_varbranch[ii - 1] = BRANCH_DEFAULT;
                    }
                    if (lp.var_is_free != null)
                    {
                        lp.var_is_free[ii] = 0;
                    }
                    if (lp.best_solution != null)
                    {
                        lp.best_solution[lp.rows + ii] = 0;
                    }
                    /* Shift data left (delete) */
                    else if (usedmap != null)
                    {
                        /* Assume there is no need to handle split columns, since we are doing
                           this only from presolve, which comes before splitting of columns. */

                        /* First update counts */
                        if (lp.int_vars + lp.sc_vars > 0)
                        {
                            for (ii = lp_utils.firstInactiveLink(usedmap); ii != 0; ii = lp_utils.nextInactiveLink(usedmap, ii))
                            {
                                if (is_int(lp, ii))
                                {
                                    lp.int_vars--;
                                    if (lp_SOS.SOS_is_member(lp.SOS, 0, ii))
                                    {
                                        lp.sos_ints--;
                                    }
                                }
                                if (is_semicont(lp, ii))
                                {
                                    lp.sc_vars--;
                                }
                            }
                        }
                        /* Shift array members */
                        for (i = 1, ii = lp_utils.firstActiveLink(usedmap); ii != 0; i++, ii = lp_utils.nextActiveLink(usedmap, ii))
                        {
                            if (i == ii)
                            {
                                continue;
                            }
                            lp.var_type[i] = lp.var_type[ii];
                            lp.sc_lobound[i] = lp.sc_lobound[ii];
                            lp.orig_obj[i] = lp.orig_obj[ii];
                            if (lp.obj != null)
                            {
                                lp.obj[i] = lp.obj[ii];
                            }
                            /*
                                  if(lp->objfromvalue != NULL)
                                    lp->objfromvalue[i] = lp->objfromvalue[ii];
                                  if(lp->objfrom != NULL)
                                    lp->objfrom[i] = lp->objfrom[ii];
                                  if(lp->objtill != NULL)
                                    lp->objtill[i] = lp->objtill[ii];
                            */
                            if (lp.bb_varbranch != null)
                            {
                                lp.bb_varbranch[i - 1] = lp.bb_varbranch[ii - 1];
                            }
                            if (lp.var_is_free != null)
                            {
                                lp.var_is_free[i] = lp.var_is_free[ii];
                            }
                            if (lp.best_solution != null)
                            {
                                lp.best_solution[lp.rows + i] = lp.best_solution[lp.rows + ii];
                            }
                        }
                        /* Shift variable priority data */
                        if ((lp.var_priority != null) || (lp.sos_priority != null))
                        {
                            //int[] colmap = null;
                            //int k;
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                            //changed from 'int[] colmap' to 'int[][] colmap'; need to check at run time
                            int[][] colmap = null;
                            int k;
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18; need to check at run time
                            lp_utils.allocINT(lp, colmap, lp.columns + 1, 1);
                            for (i = 1, ii = 0; i <= lp.columns; i++)
                            {
                                if (lp_utils.isActiveLink(usedmap, i))
                                {
                                    ii++;
                                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                                    //set second [] as 0 for now; need to check at run time
                                    colmap[i][0] = ii;
                                }
                            }
                            if (lp.var_priority != null)
                            {
                                for (i = 0, ii = 0; i < lp.columns; i++)
                                {
                                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                                    //set second [] as 0 for now; need to check at run time
                                    k = colmap[(lp.var_priority[i] != null) ? Convert.ToInt32(lp.var_priority[i]) : 0][0];
                                    if (k > 0)
                                    {
                                        lp.var_priority[ii] = k;
                                        ii++;
                                    }
                                }
                            }
                            if (lp.sos_priority != null)
                            {
                                for (i = 0, ii = 0; i < lp.sos_vars; i++)
                                {
                                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                                    //set second [] as 0 for now; need to check at run time
                                    k = colmap[(lp.sos_priority[i] != null) ? Convert.ToInt32(lp.sos_priority[i]) : 0][0];
                                    if (k > 0)
                                    {
                                        lp.sos_priority[ii] = k;
                                        ii++;
                                    }
                                }
                                lp.sos_vars = ii;
                            }
                            /*NOT REQUIRED
                            FREE(colmap);
                            */
                        }

                        delta = i - lp.columns - 1;
                    }
                    else if (delta < 0)
                    {

                        /* Fix invalid split variable data */
                        if (lp.var_is_free != null)
                        {
                            for (i = 1; i <= lp.columns; i++)
                            {
                                if (System.Math.Abs((sbyte)lp.var_is_free[i]) >= @base)
                                {
                                    lp.var_is_free[i] -= (int)lp_types.my_chsign(lp.var_is_free[i] < 0, delta);
                                }
                            }
                        }

                        /* Shift column data (excluding the basis) */
                        for (i = @base; i < @base - delta; i++)
                        {
                            if (is_int(lp, i))
                            {
                                lp.int_vars--;
                                if (lp_SOS.SOS_is_member(lp.SOS, 0, i))
                                {
                                    lp.sos_ints--;
                                }
                            }
                            if (is_semicont(lp, i))
                            {
                                lp.sc_vars--;
                            }
                        }
                        for (i = @base; i <= lp.columns + delta; i++)
                        {
                            ii = i - delta;
                            lp.var_type[i] = lp.var_type[ii];
                            lp.sc_lobound[i] = lp.sc_lobound[ii];
                            lp.orig_obj[i] = lp.orig_obj[ii];
                            if (lp.obj != null)
                            {
                                lp.obj[i] = lp.obj[ii];
                            }
                            /*
                                  if(lp->objfromvalue != NULL)
                                    lp->objfromvalue[i] = lp->objfromvalue[ii];
                                  if(lp->objfrom != NULL)
                                    lp->objfrom[i] = lp->objfrom[ii];
                                  if(lp->objtill != NULL)
                                    lp->objtill[i] = lp->objtill[ii];
                            */
                            if (lp.var_priority != null)
                            {
                                lp.var_priority[i - 1] = lp.var_priority[ii - 1];
                            }
                            if (lp.bb_varbranch != null)
                            {
                                lp.bb_varbranch[i - 1] = lp.bb_varbranch[ii - 1];
                            }
                            if (lp.var_is_free != null)
                            {
                                lp.var_is_free[i] = lp.var_is_free[ii];
                            }
                            if (lp.best_solution != null)
                            {
                                lp.best_solution[lp.rows + i] = lp.best_solution[lp.rows + ii];
                            }
                        }

                        /* Fix invalid variable priority data */
                        if (lp.var_priority != null)
                        {
                            for (i = 0, ii = 0; i < lp.columns; i++)
                            {
                                if (lp.var_priority[i] > @base - delta)
                                {
                                    lp.var_priority[ii++] = lp.var_priority[i] + delta;
                                }
                                else if (lp.var_priority[i] < @base)
                                {
                                    lp.var_priority[ii++] = lp.var_priority[i];
                                }
                            }
                        }
                        if (lp.sos_priority != null)
                        {
                            for (i = 0, ii = 0; i < lp.sos_vars; i++)
                            {
                                if (lp.sos_priority[i] > @base - delta)
                                {
                                    lp.sos_priority[ii++] = lp.sos_priority[i] + delta;
                                }
                                else if (lp.sos_priority[i] < @base)
                                {
                                    lp.sos_priority[ii++] = lp.sos_priority[i];
                                }
                            }
                            lp.sos_vars = ii;
                        }

                    }
                    shift_basis(lp, lp.rows + @base, delta, usedmap, false);
                    if (SOS_count(lp) > 0)
                    {
                        lp_SOS.SOS_shift_col(lp.SOS, 0, @base, delta, usedmap, false);
                    }
                    shift_rowcoldata(lp, lp.rows + @base, delta, usedmap, false);
                    inc_columns(lp, delta);
                }
            }
            return true;
        }

        internal static new bool shift_basis(lprec lp, int @base, int delta, LLrec usedmap, bool isrow)
        /* Note: Assumes that "lp->sum" and "lp->rows" HAVE NOT been updated to the new counts */
        {
            int i;
            int ii;
            bool Ok = true;

            /* Don't bother to shift the basis if it is not yet ready */
            if (!is_BasisReady(lp))
            {
                return (Ok);
            }

            /* Basis adjustments due to insertions (after actual row/column insertions) */
            if (delta > 0)
            {

                /* Determine if the basis becomes invalidated */
                if (isrow)
                {
                    set_action(ref lp.spx_action, ACTION_REBASE | ACTION_REINVERT);
                }

                /* Shift and fix invalid basis references (increment higher order basic variable index) */
                if (@base <= lp.sum)
                {
                    /*NOT REQUIRED
                    MEMMOVE(lp.is_basic + @base + delta, lp.is_basic + @base, lp.sum - @base + 1);
                    */
                }

                /* Prevent CPU-expensive basis updating if this is the initial model creation */
                if (!lp.model_is_pure || (lp.solvecount > 0))
                {
                    for (i = 1; i <= lp.rows; i++)
                    {
                        ii = lp.var_basic[i];
                        if (ii >= @base)
                        {
                            lp.var_basic[i] += delta;
                        }
                    }
                }

                /* Update the basis (shift and extend) */
                for (i = 0; i < delta; i++)
                {
                    ii = @base + i;
                    lp.is_basic[ii] = isrow;
                    if (isrow)
                    {
                        lp.var_basic[lp.rows + 1 + i] = ii;
                    }
                }

            }
            /* Basis adjustments due to deletions (after actual row/column deletions) */
            else
            {
                int j;
                int k;

                /* Fix invalid basis references (decrement high basic slack variable indexes),
                   but reset the entire basis if a deleted variable is found in the basis */
                k = 0;
                for (i = 1; i <= lp.rows; i++)
                {
                    ii = lp.var_basic[i];
                    lp.is_basic[ii] = false;
                    if (ii >= @base)
                    {
                        /* Skip to next basis variable if this one is to be deleted */
                        if (ii < @base - delta)
                        {
                            set_action(ref lp.spx_action, ACTION_REBASE);
                            continue;
                        }
                        /* Otherwise, update the index of the basic variable for deleted variables */
                        ii += delta;
                    }
                    k++;
                    lp.var_basic[k] = ii;
                }

                /* Set the new basis indicators */
                i = k;
                if (isrow)
                {
                    i = (int)commonlib.MIN(k, lp.rows + delta);
                }
                for (; i > 0; i--)
                {
                    j = lp.var_basic[i];
                    lp.is_basic[j] = true;
                }

                /* If a column was deleted from the basis then simply add back a non-basic
                   slack variable; do two scans, if necessary to avoid adding equality slacks */
                if (!isrow && (k < lp.rows))
                {
                    for (j = 0; j <= 1; j++)
                    {
                        for (i = 1; (i <= lp.rows) && (k < lp.rows); i++)
                        {
                            if (!lp.is_basic[i])
                            {
                                if (!is_constr_type(lp, i, EQ) || (j == 1))
                                {
                                    k++;
                                    lp.var_basic[k] = i;
                                    lp.is_basic[i] = true;
                                }
                            }
                        }
                    }
                    k = 0;
                }

                /* We are left with "k" indexes; if no basis variable was deleted, k=rows and the
                   inverse is still valid, if k+delta < 0 we do not have a valid
                   basis and must create one (in most usage modes this should not happen,
                   unless there is a bug) */
                if (k + delta < 0)
                {
                    Ok = false;
                }
                if (isrow || (k != lp.rows))
                {
                    set_action(ref lp.spx_action, ACTION_REINVERT);
                }

            }
            return (Ok);
        }

        /* Utility group for shifting row and column data */
        internal static new bool shift_rowcoldata(lprec lp, int @base, int delta, LLrec usedmap, bool isrow)
        /* Note: Assumes that "lp->sum" and "lp->rows" HAVE NOT been updated to the new counts */
        {
            int i;
            int ii;
            double lodefault;

            /* Shift data right/down (insert), and set default values in positive delta-gap */
            if (delta > 0)
            {

                /* Determine if we can take the easy way out */
                bool easyout = (bool)((lp.solvecount == 0) && (@base > lp.rows));

                /* Shift the row/column data */
                /*NOT REQUIRED
                MEMMOVE(lp.orig_upbo + @base + delta, lp.orig_upbo + @base, lp.sum - @base + 1);
                MEMMOVE(lp.orig_lowbo + @base + delta, lp.orig_lowbo + @base, lp.sum - @base + 1);
                
                if (!easyout)
                {
                    MEMMOVE(lp.upbo + @base + delta, lp.upbo + @base, lp.sum - @base + 1);
                    MEMMOVE(lp.lowbo + @base + delta, lp.lowbo + @base, lp.sum - @base + 1);
                    if (lp.model_is_valid)
                    {
                        MEMMOVE(lp.solution + @base + delta, lp.solution + @base, lp.sum - @base + 1);
                        MEMMOVE(lp.best_solution + @base + delta, lp.best_solution + @base, lp.sum - @base + 1);
                    }
                    MEMMOVE(lp.is_lower + @base + delta, lp.is_lower + @base, lp.sum - @base + 1);
                }
                */
                /* Deal with scalars; the vector can be NULL */
                if (lp.scalars != null)
                {
                    if (!easyout)
                    {
                        for (ii = lp.sum; ii >= @base; ii--)
                        {
                            i = ii + delta;
                            lp.scalars[i] = lp.scalars[ii];
                        }
                    }
                    for (ii = @base; ii < @base + delta; ii++)
                    {
                        lp.scalars[ii] = 1;
                    }
                }

                /* Set defaults */
#if SlackInitMinusInf
	if (isrow)
	{
	  lodefault = -lp.infinite;
	}
	else
	{
#endif
                lodefault = 0;

                for (i = 0; i < delta; i++)
                {
                    ii = @base + i;
                    lp.orig_upbo[ii] = lp.infinite;
                    lp.orig_lowbo[ii] = lodefault;
                    if (!easyout)
                    {
                        lp.upbo[ii] = lp.orig_upbo[ii];
                        lp.lowbo[ii] = lp.orig_lowbo[ii];
                        lp.is_lower[ii] = true;
                    }
                }
            }

            /* Shift data left/up (delete) */
            else if (usedmap != null)
            {
                int k;
                int offset = 0;
                if (!isrow)
                {
                    offset += lp.rows;
                }
                i = offset + 1;
                for (k = lp_utils.firstActiveLink(usedmap); k != 0; i++, k = lp_utils.nextActiveLink(usedmap, k))
                {
                    ii = k + offset;
                    if (ii == i)
                    {
                        continue;
                    }
                    lp.upbo[i] = lp.upbo[ii];
                    lp.orig_upbo[i] = lp.orig_upbo[ii];
                    lp.lowbo[i] = lp.lowbo[ii];
                    lp.orig_lowbo[i] = lp.orig_lowbo[ii];
                    lp.solution[i] = lp.solution[ii];
                    lp.best_solution[i] = lp.best_solution[ii];
                    lp.is_lower[i] = lp.is_lower[ii];
                    if (lp.scalars != null)
                    {
                        lp.scalars[i] = lp.scalars[ii];
                    }
                }
                if (isrow)
                {
                    @base = lp.rows + 1;
                    /*NOT REQUIRED
                    MEMMOVE(lp.upbo + i, lp.upbo + @base, lp.columns);
                    MEMMOVE(lp.orig_upbo + i, lp.orig_upbo + @base, lp.columns);
                    MEMMOVE(lp.lowbo + i, lp.lowbo + @base, lp.columns);
                    MEMMOVE(lp.orig_lowbo + i, lp.orig_lowbo + @base, lp.columns);
                    if (lp.model_is_valid)
                    {
                        MEMMOVE(lp.solution + i, lp.solution + @base, lp.columns);
                        MEMMOVE(lp.best_solution + i, lp.best_solution + @base, lp.columns);
                    }
                    MEMMOVE(lp.is_lower + i, lp.is_lower + @base, lp.columns);
                    if (lp.scalars != null)
                    {
                        MEMMOVE(lp.scalars + i, lp.scalars + @base, lp.columns);
                    }
                    */
                }
            }

            else if (delta < 0)
            {

                /* First make sure we don't cross the sum count border */
                if (@base - delta - 1 > lp.sum)
                {
                    delta = @base - lp.sum - 1;
                }

                /* Shift the data*/
                for (i = @base; i <= lp.sum + delta; i++)
                {
                    ii = i - delta;
                    lp.upbo[i] = lp.upbo[ii];
                    lp.orig_upbo[i] = lp.orig_upbo[ii];
                    lp.lowbo[i] = lp.lowbo[ii];
                    lp.orig_lowbo[i] = lp.orig_lowbo[ii];
                    lp.solution[i] = lp.solution[ii];
                    lp.best_solution[i] = lp.best_solution[ii];
                    lp.is_lower[i] = lp.is_lower[ii];
                    if (lp.scalars != null)
                    {
                        lp.scalars[i] = lp.scalars[ii];
                    }
                }

            }

            lp.sum += delta;

            lp.matA.row_end_valid = false;

            return true;
        }

        internal static new void inc_columns(lprec lp, int delta)
        {
            int i;

            if (lp.names_used && (lp.col_name != null))
            {
                for (i = lp.columns + delta; i > lp.columns; i--)
                {
                    lp.col_name[i] = null;
                }
            }

            lp.columns += delta;
            if (lp.matA.is_roworder)
            {
                lp.matA.rows += delta;
            }
            else
            {
                lp.matA.columns += delta;
            }
            if (get_Lrows(lp) > 0)
            {
                lp.matL.columns += delta;
            }
        }

        internal new bool is_obj_in_basis(lprec lp)
        {
            return (lp.obj_in_basis);
        }

        //Changed By: CS Date:28/11/2018
        internal new static bool isP1extra(lprec lp)
        {
            return ((bool)((lp.P1extraDim > 0) || (lp.P1extraVal != 0)));
        }

        //Changed By: CS Date:28/11/2018
        internal new static bool feasiblePhase1(lprec lp, double epsvalue)
        {
            double gap;
            bool test;

            gap = System.Math.Abs(lp.rhs[0] - lp.orig_rhs[0]);
            test = (bool)(gap < epsvalue);
            return (test);
        }

        internal new static int MIP_count(lprec lp)
        {
            return (lp.int_vars + lp.sc_vars + SOS_count(lp));
        }

        internal new bool del_constraint(lprec lp, int rownr)
        {
            bool preparecompact = (bool)(rownr < 0);
            string msg = "";

            if (preparecompact)
            {
                rownr = -rownr;
            }
            if ((rownr < 1) || (rownr > lp.rows))
            {
                msg = "del_constraint: Attempt to delete non-existing constraint %d\n";
                lp.report(lp, IMPORTANT, ref msg, rownr);
                return (false);
            }
            /*
            if(lp->matA->is_roworder) {
              report(lp, IMPORTANT, "del_constraint: Cannot delete constraint while in row entry mode.\n");
              return(FALSE);
            }
            */

            if (is_constr_type(lp, rownr, EQ) && (lp.equalities > 0))
            {
                lp.equalities--;
            }

            varmap_delete(lp, Convert.ToInt32(lp_types.my_chsign(preparecompact, rownr)), -1, null);
            shift_rowdata(lp, Convert.ToInt32(lp_types.my_chsign(preparecompact, rownr)), -1, null);

            /*
               peno 04.10.07
               Fixes a problem with del_constraint.
               Constraints names were not shifted and reported variable result was incorrect.
               See UnitTest1, UnitTest2

               min: -2 x3;

               c1: +x2 -x1 <= 10;
               c: 0 x3 <= 0;
               c2: +x3 +x2 +x1 <= 20;

               2 <= x3 <= 3;
               x1 <= 30;

               // del_constraint(lp, 2);

               // See write_LP and print_solution result

               // To fix, commented if(!lp->varmap_locked)

            */
            if (!lp.varmap_locked)
            {
                lp_presolve.presolve_setOrig(lp, lp.rows, lp.columns);
                if (lp.names_used)
                {
                    del_varnameex(lp, lp.row_name, lp.rows, lp.rowname_hashtab, rownr, null);
                }
            }

#if Paranoia
          if (is_BasisReady(lp) && !verify_basis(lp))
          {
	        report(lp, SEVERE, "del_constraint: Invalid basis detected at row %d\n", rownr);
          }
#endif

            return (true);
        }

        //Changed By: CS Date:28/11/2018
        internal static bool performiteration(lprec lp, int rownr, int varin, double theta, bool primal, bool allowminit, ref double prow, ref int nzprow, ref double pcol, ref int nzpcol, ref int boundswaps)
        {
            int varout;
            double pivot;
            double epsmargin;
            double leavingValue;
            double leavingUB;
            double enteringUB;
            bool leavingToUB = false;
            bool enteringFromUB = new bool();
            bool enteringIsFixed = new bool();
            bool leavingIsFixed = new bool();

            //ORIGINAL LINE: bool *islower = &(lp->is_lower[varin]);
            bool islower = (lp.is_lower[varin]);
            bool minitNow = false;
            bool minitStatus = ITERATE_MAJORMAJOR;
            double deltatheta = theta;
            LpCls objLpCls = new LpCls();
            string msg;

            if (objLpCls.userabort(lp, MSG_ITERATION))
            {
                return (minitNow);
            }

#if Paranoia
                  if (rownr > lp.rows)
                  {
	                if (lp.spx_trace)
	                {
	                  report(lp, IMPORTANT, "performiteration: Numeric instability encountered!\n");
	                }
	                lp.spx_status = NUMFAILURE;
	                return (0);
                  }
#endif
            varout = lp.var_basic[rownr];
#if Paranoia
                  if (!lp.is_lower[varout])
                  {
	                report(lp, SEVERE, "performiteration: Leaving variable %d was at its upper bound at iter %.0f\n", varout, (double) get_total_iter(lp));
                  }
#endif
            /* Theta is the largest change possible (strictest constraint) for the entering
     variable (Theta is Chvatal's "t", ref. Linear Programming, pages 124 and 156) */
            lp.current_iter++;

            /* Test if it is possible to do a cheap "minor iteration"; i.e. set entering
               variable to its opposite bound, without entering the basis - which is
               obviously not possible for fixed variables! */
            epsmargin = lprec.epsprimal;
            enteringFromUB = !(islower);
            enteringUB = lp.upbo[varin];
            leavingUB = lp.upbo[varout];
            enteringIsFixed = (bool)(System.Math.Abs(enteringUB) < epsmargin);
            leavingIsFixed = (bool)(System.Math.Abs(leavingUB) < epsmargin);
#if _PRICE_NOBOUNDFLIP
  allowminit &= !ISMASKSET(lp.piv_strategy, PRICE_NOBOUNDFLIP);
#endif
#if Paranoia
  if (enteringUB < 0)
  {
	report(lp, SEVERE, "performiteration: Negative range for entering variable %d at iter %.0f\n", varin, (double) get_total_iter(lp));
  }
  if (leavingUB < 0)
  {
	report(lp, SEVERE, "performiteration: Negative range for leaving variable %d at iter %.0f\n", varout, (double) get_total_iter(lp));
  }
#endif

            /* Handle batch bound swaps with the dual long-step algorithm;
               Loop over specified bound swaps; update RHS and Theta for bound swaps */
            if ((boundswaps != null) && (boundswaps > 0))
            {

                int i;
                int boundvar;
                double[] hold = null;

                /* Allocate and initialize accumulation array */
                //NOT REQUIRED
                //allocREAL(lp, hold, lp.rows + 1, 1);

                /* Accumulate effective bound swaps and update flag */
                for (i = 1; i <= boundswaps; i++)
                {
                    boundvar = boundswaps;
                    deltatheta = lp_types.my_chsign(!lp.is_lower[boundvar], lp.upbo[boundvar]);
                    lp_matrix.mat_multadd(lp.matA, hold, boundvar, deltatheta);
                    lp.is_lower[boundvar] = !lp.is_lower[boundvar];
                }
                lp.current_bswap += boundswaps;
                lp.current_iter += boundswaps;

                /* Solve for bound flip update vector (note that this does not
                   overwrite the stored update vector for the entering variable) */
                lp_matrix.ftran(lp, hold, 0, lp.epsmachine);
                if (!lp.obj_in_basis)
                {
                    hold[0] = 0; // The correct reduced cost goes here (adjusted for bound state) ******
                }

                /* Update the RHS / basic variable values and set revised thetas */
                //NOTED ISSUE
                pivot = lp.bfp_pivotRHS(lp, 1, ref hold);
                deltatheta = LpPrice.multi_enteringtheta(lp.longsteps);
                theta = deltatheta;

                //NOT REQUIRED
                //FREE(hold);
            }
            /* Otherwise to traditional check for single bound swap */
            else if (allowminit && !enteringIsFixed)
            {

                /*    pivot = epsmargin; */
                pivot = lprec.epsdual;
                /* #define v51mode */
                /* Enable this for v5.1 operation mode */
#if v51mode
	if (((lp.simplex_mode & SIMPLEX_Phase1_DUAL) == 0) || !is_constr_type(lp, rownr, EQ)) // *** DEBUG CODE KE
	{
#endif
                if (enteringUB - theta < -pivot)
                {

#if !v51mode
                    if (System.Math.Abs(enteringUB - theta) < pivot)
                    {
                        minitStatus = Convert.ToBoolean(lp_lib.ITERATE_MINORMAJOR);
                    }
                    else
                    {
#endif
                        minitStatus = Convert.ToBoolean(lp_lib.ITERATE_MINORRETRY);
                    }
                    minitNow = (bool)(minitStatus != ITERATE_MAJORMAJOR);
                }
            }

            /* Process for traditional style single minor iteration */
            if (minitNow)
            {

                /* Set the new values (note that theta is set to always be positive) */
                theta = Convert.ToDouble(commonlib.MIN(System.Math.Abs(theta), enteringUB));

                /* Update the RHS / variable values and do bound-swap */
                double Parameter = 0;
                pivot = lp.bfp_pivotRHS(lp, theta, ref Parameter);
                islower = !(islower);

                lp.current_bswap++;

            }

            /* Process for major iteration */
            else
            {

                /* Update the active pricer for the current pivot */
                LpPricePSE.updatePricer(lp, rownr, varin, lp.bfp_pivotvector(lp), ref prow, ref nzprow);

                /* Update the current basic variable values */
                double Para = 0;
                pivot = lp.bfp_pivotRHS(lp, theta, ref Para);

                /* See if the leaving variable goes directly to its upper bound. */
                leavingValue = lp.rhs[rownr];
                leavingToUB = (bool)(leavingValue > 0.5 * leavingUB);
                lp.is_lower[varout] = leavingIsFixed || !leavingToUB;

                /* Set the value of the entering varible (theta always set to be positive) */
                if (enteringFromUB)
                {
                    lp.rhs[rownr] = enteringUB - deltatheta;

                    islower = true;
                }
                else
                {
                    lp.rhs[rownr] = deltatheta;
                }

                lp_types.my_roundzero(lp.rhs[rownr], epsmargin);

                /* Update basis indeces */
                varout = objLpCls.set_basisvar(lp, rownr, varin);

                /* Finalize the update in preparation for next major iteration */
                lp.bfp_finishupdate(lp, Convert.ToByte(enteringFromUB));

            }
            /* Show pivot tracking information, if specified */
            if ((lp.verbose > NORMAL) && (MIP_count(lp) == 0) && ((lp.current_iter % commonlib.MAX(2, lp.rows / 10)) == 0))
            {
                msg = "Objective value " + lp_types.RESULTVALUEMASK + " at iter %10.0f.\n";
                lp.report(lp, NORMAL, ref msg, lp.rhs[0], (double)get_total_iter(lp));
            }

#if false
            //  if(verify_solution(lp, FALSE, my_if(minitNow, "MINOR", "MAJOR")) >= 0) {
            //    if(minitNow)
            //      pivot = get_obj_active(lp, varin);
            //    else
            //      pivot = get_obj_active(lp, varout);
            //  }
#endif
#if false
            //  if((lp->longsteps != NULL) && (boundswaps[0] > 0) && lp->longsteps->objcheck &&
            //    ((pivot = fabs(my_reldiff(lp->rhs[0], lp->longsteps->obj_last))) > lp->epssolution)) {
            //    report(lp, IMPORTANT, "performiteration: Objective value gap %8.6f found at iter %6.0f (%d bound flips, %d)\n",
            //                          pivot, (double) get_total_iter(lp), boundswaps[0], enteringFromUB);
            //  }
#endif

            if (lp.spx_trace)
            {
                if (minitNow)
                {
                    msg = "I:%5.0f - minor - %5d ignored,          %5d flips  from %s with THETA=%g and OBJ=%g\n";
                    lp.report(lp, NORMAL, ref msg, (double)get_total_iter(lp), varout, varin, (enteringFromUB ? "UPPER" : "LOWER"), theta, lp.rhs[0]);
                }
                else
                {
                    msg = "I:%5.0f - MAJOR - %5d leaves to %s,  %5d enters from %s with THETA=%g and OBJ=%g\n";
                    lp.report(lp, NORMAL, ref msg, (double)get_total_iter(lp), varout, (leavingToUB ? "UPPER" : "LOWER"), varin, (enteringFromUB ? "UPPER" : "LOWER"), theta, lp.rhs[0]);
                }
                if (minitNow)
                {
                    if (!lp.is_lower[varin])
                    {
                        msg = "performiteration: Variable %d changed to its lower bound at iter %.0f (from %g)\n";
                        lp.report(lp, DETAILED, ref msg, varin, (double)get_total_iter(lp), enteringUB);
                    }
                    else
                    {
                        msg = "performiteration: Variable %d changed to its upper bound at iter %.0f (to %g)\n";
                        lp.report(lp, DETAILED, ref msg, varin, (double)get_total_iter(lp), enteringUB);
                    }
                }
                else
                {
                    msg = "performiteration: Variable %d entered basis at iter %.0f at " + lp_types.RESULTVALUEMASK + "\n";
                    lp.report(lp, NORMAL, ref msg, varin, (double)get_total_iter(lp), lp.rhs[rownr]);
                }
                if (!primal)
                {
                    pivot = compute_feasibilitygap(lp, (bool)!primal, 1);
                    msg = "performiteration: Feasibility gap at iter %.0f is " + lp_types.RESULTVALUEMASK + "\n";
                    lp.report(lp, NORMAL, ref msg, (double)get_total_iter(lp), pivot);
                }
                else
                {
                    msg = "performiteration: Current objective function value at iter %.0f is " + lp_types.RESULTVALUEMASK + "\n";
                    lp.report(lp, NORMAL, ref msg, (double)get_total_iter(lp), lp.rhs[0]);
                }
            }

            return (minitStatus);

        }

        //Changed By: CS Date:28/11/2018
        internal static double compute_feasibilitygap(lprec lp, bool isdual, bool dosum)
        {
            double f = 0;

            /* This computes the primal feasibility gap (for use with the dual simplex phase 1) */
            if (isdual)
            {
                int i;
                double g = new double();

                for (i = 1; i <= lp.rows; i++)
                {
                    if (lp.rhs[i] < 0)
                    {
                        g = lp.rhs[i];
                    }
                    else if (lp.rhs[i] > lp.upbo[lp.var_basic[i]])
                    {
                        g = lp.rhs[i] - lp.upbo[lp.var_basic[i]];
                    }
                    else
                    {
                        g = 0;
                    }
                    if (dosum)
                    {
                        f += g;
                    }
                    else
                    {
                        commonlib.SETMAX(Convert.ToInt32(f), Convert.ToInt32(g));
                    }
                }
            }
            /* This computes the dual feasibility gap (for use with the primal simplex phase 1) */
            else
            {
                f = compute_dualslacks(lp, SCAN_USERVARS + USE_ALLVARS, null, null, dosum);
            }

            return (f);
        }

        //Changed By: CS Date:28/11/2018
        internal new static double compute_dualslacks(lprec lp, int target, double[][] dvalues, int[][] nzdvalues, bool dosum)
        {
            /* Note that this function is similar to the compute_reducedcosts function in lp_price.c */

            int i;
            int varnr;
            //ORIGINAL LINE: int *coltarget;
            int coltarget;
            int[][] nzduals;

            //ORIGINAL LINE: int *nzvtemp = null;
            int[][] nzvtemp = null;
            double d;
            double g = 0;
            double[][] duals;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: double *vtemp = null;
            double[][] vtemp = null;
            bool localREAL = (bool)(dvalues == null);
            bool localINT = (bool)(nzdvalues == null);
            LpCls objLpCls = new LpCls();

            if (objLpCls.is_action(lp.spx_action, ACTION_REBASE) || objLpCls.is_action(lp.spx_action, ACTION_REINVERT) || !lp.basis_valid)
            {
                return (g);
            }

            /* Initialize */
            if (!localREAL)
            {
                duals = dvalues;
                nzduals = nzdvalues;
            }
            else
            {
                duals = vtemp;
                nzduals = nzvtemp;
            }
            if (localINT || (nzduals[0] == null))
            {
                //NOT REQUIRED
                //allocINT(lp, nzduals, lp.columns + 1, AUTOMATIC);
            }
            if (localREAL || (duals[0] == null))
            {
                //NOT REQUIRED
                //allocREAL(lp, duals, lp.sum + 1, AUTOMATIC);
            }
            if (target == 0)
            {
                target = SCAN_ALLVARS + USE_NONBASICVARS;
            }

            /* Define variable target list and compute the reduced costs */
            //NOTED ISSUE
            coltarget = (int)lp_utils.mempool_obtainVector(lp.workarrays, lp.columns + 1, sizeof(int));
            if (!lp_matrix.get_colIndexA(lp, target, coltarget, false))
            {
                lp_utils.mempool_releaseVector(lp.workarrays, (string)coltarget, 0);
                return (0);
            }
            int? Parameter = null;
            lp_matrix.bsolve(lp, 0, ref duals[0], ref Parameter, lp.epsmachine * DOUBLEROUND, 1.0);
            lp_matrix.prod_xA(lp, coltarget, duals[0], null, lp.epsmachine, 1.0, duals[0], nzduals[0], lp_matrix.MAT_ROUNDDEFAULT | lp_matrix.MAT_ROUNDRC);
            lp_utils.mempool_releaseVector(lp.workarrays, (string)coltarget, 0);

            /* Compute sum or maximum infeasibility as specified */
            for (i = 1; i <= Convert.ToInt32(nzduals[0]); i++)
            {
                varnr = Convert.ToInt32(nzduals[i]);
                d = lp_types.my_chsign(!lp.is_lower[varnr], Convert.ToDouble(duals[varnr]));
                if (d < 0)
                {
                    if (dosum)
                    {
                        g += -d; // Compute sum as a positive number
                    }
                    else
                    {
                        commonlib.SETMIN(Convert.ToInt32(g), Convert.ToInt32(d)); // Compute gap as a negative number
                    }
                }
            }

            /* Clean up */
            if (localREAL)
            {
                //NOT REQUIRED
                // FREEduals;
            }
            if (localINT)
            {
                //NOT REQUIRED
                //FREEnzduals;
            }

            return (g);
        }

        //Changed By: CS Date:28/11/2018
        internal new static bool restore_basis(lprec lp)
        /* Restore values from the previously pushed / saved basis without popping it */
        {
            bool ok;
            int i;
            LpCls objLpCls = new LpCls();

            ok = (bool)(lp.bb_basis != null);
            if (ok)
            {
                //NOT REQUIRED
                //MEMCOPY(lp.var_basic, lp.bb_basis.var_basic, lp.rows + 1);
                //NOT REQUIRED
                /*
                #if BasisStorageModel == 0
                                MEMCOPY(lp.is_basic, lp.bb_basis.is_basic, lp.sum + 1);
                #else
                    MEMCLEAR(lp.is_basic, lp.sum + 1);
                    for (i = 1; i <= lp.rows; i++)
                    {
                      lp.is_basic[lp.var_basic[i]] = 1;
                    }
                #endif */
                //NOT REQUIRED
                /*#if LowerStorageModel == 0;
                                MEMCOPY(lp.is_lower, lp.bb_basis.is_lower, lp.sum + 1);
                #else
                    for (i = 1; i <= lp.sum; i++)
                    {
                      lp.is_lower[i] = is_biton(lp.bb_basis.is_lower, i);
                    }
                #endif*/
                objLpCls.set_action(ref lp.spx_action, ACTION_REBASE | ACTION_REINVERT);
            }
            return (ok);
        }

        internal new static bool is_BasisReady(lprec lp)
        {
            return ((bool)(lp.var_basic[0] != lp_types.AUTOMATIC));
        }
    }
}
