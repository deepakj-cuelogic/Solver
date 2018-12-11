#define PricerDefaultOpt
#define LowerStorageModel 
#define BasisStorageModel 
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
        internal static new bool userabort(lprec lp, int message)
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
                    lp.bb_break = true;
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
            if ((lp.sectimeout > 0) && ((Convert.ToDouble(DateTime.Now) - Convert.ToDouble(lp.timestart)) - (double)lp.sectimeout > 0))
            {
                lp.spx_status = TIMEOUT;
            }

            if (lp.ctrlc != null)
            {
                int retcode = lp.ctrlc(lp, lp.ctrlchandle);
                /* Check for command to restart the B&B */
                if ((retcode == ACTION_RESTART) && (lp.bb_level > 1))
                {
                    lp.bb_break = lp_types.AUTOMATIC;
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
            // ORIGINAL LINE: ok = (bool)((filename == NULL) || (*filename == 0) || ((output = fopen(filename, "w")) != NULL));
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
                return (lp.timeend - Convert.ToDouble(lp.timestart));
            else
                // ORIGINAL LINE: return (timeNow() - lp->timestart);
                return (Convert.ToDouble(DateTime.Now) - Convert.ToDouble(lp.timestart));
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

        internal static void set_obj_in_basis(lprec lp, bool obj_in_basis)
        {
            lp.obj_in_basis = (bool)(obj_in_basis == true);
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
            lp.wasPreprocessed = false;
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

            lp.basis_valid = false;
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
        new int get_nonzeros(lprec lp)
        {
            return (lp_matrix.mat_nonzeros(lp.matA));
        }

        internal new bool set_bounds(lprec lp, int colnr, double lower, double upper)
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

        internal static new double get_lowbo(lprec lp, int colnr)
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

        internal static new bool set_rh(lprec lp, int rownr, double value)
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
                newht = lp_Hash.copy_hash_table(oldht, list, oldht.size);
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

        internal static bool str_set_rh_vec(lprec lp, string rh_string)
        {
            throw new NotImplementedException();
        }

        internal static new void set_rh_vec(lprec lp, ref double rh)
        {
            throw new NotImplementedException();
        }

        internal static new double get_rh_range(lprec lp, int rownr)
        {
            throw new NotImplementedException();
        }

        internal static new bool set_rh_upper(lprec lp, int rownr, double value)
        {
            throw new NotImplementedException();
        }

        internal static new double get_rh_upper(lprec lp, int rownr)
        {
            throw new NotImplementedException();
        }

        internal static new bool set_var_priority(lprec lp)
        /* Experimental automatic variable ordering/priority setting */
        {
            throw new NotImplementedException();
        }

        internal static new bool is_negative(lprec lp, int colnr)
        {
            throw new NotImplementedException();
        }

        internal static new bool is_SOS_var(lprec lp, int colnr)
        {
            throw new NotImplementedException();
        }

        internal static new bool get_bounds(lprec lp, int column, ref double lower, ref double upper)
        {
            throw new NotImplementedException();
        }

        internal static new bool get_bounds_tighter(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static new int get_simplextype(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static new void set_simplextype(lprec lp, int simplextype)
        {
            throw new NotImplementedException();
        }

        internal static new bool del_columnex(lprec lp, LLrec colmap)
        {
            throw new NotImplementedException();
        }

        internal static bool del_varnameex(lprec lp, hashelem[][] namelist, int items, hashtable ht, int varnr, LLrec varmap)
        {
            throw new NotImplementedException();
        }

        internal static bool str_add_column(lprec lp, string col_string)
        {
            throw new NotImplementedException();
        }

        internal static new bool add_column(lprec lp, ref double column)
        {
            throw new NotImplementedException();
        }

        internal static new bool set_column(lprec lp, int colnr, ref double column)
        {
            throw new NotImplementedException();
        }
        internal static new bool del_constraintex(lprec lp, LLrec rowmap)
        {
            throw new NotImplementedException();
        }
        internal static new bool set_rowex(lprec lp, int rownr, int count, ref double row, ref int colno)
        {
            throw new NotImplementedException();
        }
        internal static new bool set_row(lprec lp, int rownr, ref double row)
        {
            throw new NotImplementedException();
        }
        internal static new bool set_obj_fn(lprec lp, ref double row)
        {
            throw new NotImplementedException();
        }

        internal static new void varmap_delete(lprec lp, int @base, int delta, LLrec varmap)
        {
            int i;
            int ii;
            int j;
            bool preparecompact = (bool)(varmap != null);
            presolveundorec psundo = lp.presolve_undo;

            /* Set the model "dirty" if we are deleting row of constraint */
            lp.model_is_pure &= (bool)((lp.solutioncount == 0) && !preparecompact);

            /* Don't do anything if
               1) variables aren't locked yet, or
               2) the constraint was added after the variables were locked */
            if (!lp.varmap_locked)
            {
#if false
//   if(lp->names_used)
//     varmap_lock(lp);
//   else
//     return;
#else
                if (!lp.model_is_pure && lp.names_used)
                {
                    varmap_lock(lp);
                }
#endif
            }

            /* Do mass deletion via a linked list */
            preparecompact = (bool)(varmap != null);
            if (preparecompact)
            {
                preparecompact = (bool)(@base > lp.rows); // Set TRUE for columns
                for (j = lp_utils.firstInactiveLink(varmap); j != 0; j = lp_utils.nextInactiveLink(varmap, j))
                {
                    i = j;
                    if (preparecompact)
                    {
#if Paranoia
		if (SOS_is_member(lp.SOS, 0, j))
		{
		  report(lp, SEVERE, "varmap_delete: Deleting variable %d, which is in a SOS!\n", j);
		}
#endif
                        i += lp.rows;
                    }
                    ii = psundo.var_to_orig[i];
                    if (ii > 0) // It was an original variable; reverse sign of index to flag deletion
                    {
                        psundo.var_to_orig[i] = -ii;
                    }
                    else // It was a non-original variable; add special code for deletion
                    {
                        psundo.var_to_orig[i] = -(psundo.orig_rows + psundo.orig_columns + i);
                    }
                }
                return;
            }

            /* Do legacy simplified version if we are doing batch delete operations */
            preparecompact = (bool)(@base < 0);
            if (preparecompact)
            {
                @base = -@base;
                if (@base > lp.rows)
                {
                    @base += (psundo.orig_rows - lp.rows);
                }
                for (i = @base; i < @base - delta; i++)
                {
                    ii = psundo.var_to_orig[i];
                    if (ii > 0) // It was an original variable; reverse sign of index to flag deletion
                    {
                        psundo.var_to_orig[i] = -ii;
                    }
                    else // It was a non-original variable; add special code for deletion
                    {
                        psundo.var_to_orig[i] = -(psundo.orig_rows + psundo.orig_columns + i);
                    }
                }
                return;
            }

            /* We are deleting an original constraint/column;
               1) clear mapping of original to deleted
               2) shift the deleted variable to original mappings left
               3) decrement all subsequent original-to-current pointers
            */
            if (varmap_canunlock(lp))
            {
                lp.varmap_locked = false;
            }
            for (i = @base; i < @base - delta; i++)
            {
                ii = psundo.var_to_orig[i];
                if (ii > 0)
                {
                    psundo.orig_to_var[ii] = 0;
                }
            }
            for (i = @base; i <= lp.sum + delta; i++)
            {
                ii = i - delta;
                psundo.var_to_orig[i] = psundo.var_to_orig[ii];
            }

            i = 1;
            j = psundo.orig_rows;
            if (@base > lp.rows)
            {
                i += j;
                j += psundo.orig_columns;
            }
            ii = @base - delta;
            for (; i <= j; i++)
            {
                if (psundo.orig_to_var[i] >= ii)
                {
                    psundo.orig_to_var[i] += delta;
                }
            }

        }

        /* Utility routine group for constraint and column deletion/insertion
   mapping in relation to the original set of constraints and columns */
        internal static new void varmap_lock(lprec lp)
        {
            lp_presolve.presolve_fillUndo(lp, lp.rows, lp.columns, true);
            lp.varmap_locked = true;
        }


        internal static new bool varmap_validate(lprec lp, int varno)
        {
            throw new NotImplementedException();
        }

        internal static new void varmap_compact(lprec lp, int prev_rows, int prev_cols)
        {
            throw new NotImplementedException();
        }


        internal static new bool shift_rowdata(lprec lp, int @base, int delta, LLrec usedmap)
        /* Note: Assumes that "lp->rows" HAS NOT been updated to the new count */
        {
            int i;
            int ii;

            /* Shift sparse matrix row data */
            if (lp.matA.is_roworder)
            {
                lp_matrix.mat_shiftcols(lp.matA, ref @base, delta, usedmap);
            }
            else
            {
               lp_matrix.mat_shiftrows(lp.matA, ref @base, delta, usedmap);
            }

            /* Shift data down (insert row), and set default values in positive delta-gap */
            if (delta > 0)
            {

                /* Shift row data */
                for (ii = lp.rows; ii >= @base; ii--)
                {
                    i = ii + delta;
                    lp.orig_rhs[i] = lp.orig_rhs[ii];
                    lp.rhs[i] = lp.rhs[ii];
                    lp.row_type[i] = lp.row_type[ii];
                }

                /* Set defaults (actual basis set in separate procedure) */
                for (i = 0; i < delta; i++)
                {
                    ii = @base + i;
                    lp.orig_rhs[ii] = 0;
                    lp.rhs[ii] = 0;
                    lp.row_type[ii] = ROWTYPE_EMPTY;
                }
            }

            /* Shift data up (delete row) */
            else if (usedmap != null)
            {
                for (i = 1, ii = lp_utils.firstActiveLink(usedmap); ii != 0; i++, ii = lp_utils.nextActiveLink(usedmap, ii))
                {
                    if (i == ii)
                    {
                        continue;
                    }
                    lp.orig_rhs[i] = lp.orig_rhs[ii];
                    lp.rhs[i] = lp.rhs[ii];
                    lp.row_type[i] = lp.row_type[ii];
                }
                delta = i - lp.rows - 1;
            }
            else if (delta < 0)
            {

                /* First make sure we don't cross the row count border */
                if (@base - delta - 1 > lp.rows)
                {
                    delta = @base - lp.rows - 1;
                }

                /* Shift row data (don't shift basis indexes here; done in next step) */
                for (i = @base; i <= lp.rows + delta; i++)
                {
                    ii = i - delta;
                    lp.orig_rhs[i] = lp.orig_rhs[ii];
                    lp.rhs[i] = lp.rhs[ii];
                    lp.row_type[i] = lp.row_type[ii];
                }
            }

            shift_basis(lp, @base, delta, usedmap, true);
            shift_rowcoldata(lp, @base, delta, usedmap, true);
            inc_rows(lp, delta);

            return true;
        }


        /* Utility group for incrementing row and column vector storage space */
        internal static new void inc_rows(lprec lp, int delta)
        {
            throw new NotImplementedException();
        }

        /* Problem manipulation routines */
        internal static new bool set_obj(lprec lp, int colnr, double value)
        {
            throw new NotImplementedException();
        }

        internal static bool str_set_obj_fn(lprec lp, string row_string)
        {
            throw new NotImplementedException();
        }

        internal static new bool add_lag_con(lprec lp, ref double row, int con_type, double rhs)
        {
            throw new NotImplementedException();
        }

        internal static bool str_add_lag_con(lprec lp, string row_string, int con_type, double rhs)
        {
            throw new NotImplementedException();
        }

        internal static new bool set_columnex(lprec lp, int colnr, int count, ref double column, ref int rowno)
        {
            throw new NotImplementedException();
        }

        internal static void set_preferdual(lprec lp, bool dodual)
        {
            throw new NotImplementedException();
        }

        internal static void set_bounds_tighter(lprec lp, bool tighten)
        {
            throw new NotImplementedException();
        }

        internal static new bool set_var_weights(lprec lp, ref double weights)
        {
            throw new NotImplementedException();
        }

        internal static new double get_rh_lower(lprec lp, int rownr)
        {
            throw new NotImplementedException();
        }

        internal static new bool set_rh_range(lprec lp, int rownr, double deltavalue)
        {
            throw new NotImplementedException();
        }

        internal static new bool set_rh_lower(lprec lp, int rownr, double value)
        {
            throw new NotImplementedException();
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
            /// REF: lp_types.h: definition for bool says 'could be unsigned int'
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
            lp.basis_valid = false;

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



        internal bool set_int(lprec lp, int colnr, bool var_type)
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

                hold = lp_types.my_chsign(is_chsign(lp, (mat.is_roworder) ? colnr : ii), value);
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

        public static new double get_mat(lprec lp, int rownr, int colnr)
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
                    a = get_mat_byindex(lp, i, true, false);
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

        internal static new bool is_action(int actionvar, int testmask)
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

        internal static new int get_piv_rule(lprec lp)
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

        private static new bool is_chsign(lprec lp, int rownr)
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

        internal static new int set_basisvar(lprec lp, int basisPos, int enteringCol)
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
                lp.report(lp, IMPORTANT, ref msg, enteringCol, (double)get_total_iter(lp));
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

        internal new static int get_basisOF(lprec lp, int[] coltarget, double[] crow, int[] colno)
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
        internal static int obtain_column(lprec lp, int varin, ref double?[] pcol, ref int[] nzlist, ref int maxabs)
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
                varin = get_basisOF(lp, null, pcol, nzlist);

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
                    column[0] = get_OF_active(lp, lp.rows + col_nr, mult);
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
                    value = get_OF_active(lp, lp.rows + col_nr, mult);
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
            //ORIGINAL LINE: result = (bool)(ii == 0);
            result = ((bool)(ii == 0));
            Done:
            ///#if false
            //  if(!result)
            //    ii = 0;
            ///#endif
            return (result);
        }

        internal static new bool is_slackbasis(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static new void set_OF_override(lprec lp, ref double ofVector)
        /* The purpose of this function is to set, or clear if NULL, the
           ofVector[0..columns] as the active objective function instead of
           the one stored in the A-matrix. See also lag_solve().*/
        {
            throw new NotImplementedException();
        }

        internal static new string get_str_constr_class(lprec lp, int con_class)
        {
            throw new NotImplementedException();
        }

        internal static new double get_constr_value(lprec lp, int rownr, int count, ref double primsolution, ref int nzindex)
        {
            throw new NotImplementedException();
        }


        internal static new string get_str_constr_type(lprec lp, int con_type)
        {
            throw new NotImplementedException();
        }

        internal static new bool get_column(lprec lp, int colnr, ref double column)
        {
            throw new NotImplementedException();
        }

        internal static new int get_constr_class(lprec lp, int rownr)
        {
            throw new NotImplementedException();
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
                    if (LpCls.is_constr_type(lp, i, EQ))
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
            i = myblas.idamax(lp.rows, ref lp.rhs[0], 1);
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

        internal static new string get_col_name(lprec lp, int colnr)
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

        internal static bool check_degeneracy(lprec lp, ref double?[] pcol, ref int degencount)
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

                        ht = lp_Hash.copy_hash_table(lp.colname_hashtab, lp.col_name, lp.columns_alloc + 1);
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

        internal new static void free_duals(lprec lp)
        {
            /*NOT REQUIRED
            FREE(lp.duals);
            FREE(lp.full_duals);
            FREE(lp.dualsfrom);
            FREE(lp.dualstill);
            FREE(lp.objfromvalue);
            FREE(lp.objfrom);
            FREE(lp.objtill);
            */
        }

        internal static void replaceBasisVar(lprec lp, int rownr, int @var, int[] var_basic, bool[] is_basic)
        {
            int @out;

            @out = var_basic[rownr];
            var_basic[rownr] = @var;
            is_basic[@out] = false;
            is_basic[@var] = true;
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

        internal static new bool dualize_lp(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static new int get_Ncolumns(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static new int get_Norig_rows(lprec lp)
        {
            throw new NotImplementedException();
        }
        internal static new int get_Nrows(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static new int get_solutioncount(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static new int get_solutionlimit(lprec lp)
        {
            throw new NotImplementedException();
        }
        internal static new bool get_sensitivity_obj(lprec lp, ref double objfrom, ref double objtill)
        {
            throw new NotImplementedException();
        }

        internal static new void set_solutionlimit(lprec lp, int limit)
        {
            throw new NotImplementedException();
        }

        internal static new int get_Norig_columns(lprec lp)
        {
            throw new NotImplementedException();
        }

        /* ---------------------------------------------------------------------------------- */
        /* Core routines for lp_solve                                                         */
        /* ---------------------------------------------------------------------------------- */
        internal static new int get_status(lprec lp)
        {
            return (lp.spx_status);
        }

        internal static new bool resize_lp(lprec lp, int rows, int columns)
        {
            throw new NotImplementedException();
        }

        internal static new string get_statustext(lprec lp, int statuscode)
        {
            throw new NotImplementedException();
        }

        internal static new void free_lp(lprec[] plp)
        {
            if (plp != null)
            {
                lprec lp = plp[0];
                if (lp != null)
                {
                    delete_lp(lp);
                }
                plp[0] = null;
            }
        }


        internal static bool get_SOS(lprec lp, int index, ref string name, ref int sostype, ref int priority, ref int count, ref int sosvars, ref double weights)
        {
            throw new NotImplementedException();
        }


        /* Make a copy of the existing model using (mostly) high-level
   construction routines to simplify future maintainance. */
        internal static new lprec copy_lp(lprec lp)
        {
            throw new NotImplementedException();
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
        internal static bool performiteration(lprec lp, int rownr, int varin, double theta, bool primal, bool allowminit, ref double[] prow, ref int nzprow, ref double?[] pcol, ref int nzpcol, ref int boundswaps)
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

            if (userabort(lp, MSG_ITERATION))
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
                pivot = lp.bfp_pivotRHS(lp, 1, ref hold[0]);
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
                LpPricePSE.updatePricer(lp, rownr, varin, lp.bfp_pivotvector(lp), ref prow[0], ref nzprow);

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
                varout = LpCls.set_basisvar(lp, rownr, varin);

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
                    pivot = compute_feasibilitygap(lp, (bool)!primal, true);
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
            //FIX_133d8fd1-d4bf-4e73-ac14-1bb037ba574f 29/11/18
            int coltarget = 0;
            string memvector = "";
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

            if (LpCls.is_action(lp.spx_action, ACTION_REBASE) || LpCls.is_action(lp.spx_action, ACTION_REINVERT) || !lp.basis_valid)
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
            /// <summary> FIX_133d8fd1-d4bf-4e73-ac14-1bb037ba574f 29/11/18
            /// PREVIOUS: coltarget = (int)lp_utils.mempool_obtainVector(lp.workarrays, lp.columns + 1, sizeof(int));
            /// ERROR IN PREVIOUS: Cannot convert type 'string' to 'int'
            /// FIX 1: 
            /// </summary>
            int id = 0;
            bool res = int.TryParse(lp_utils.mempool_obtainVector(lp.workarrays, lp.columns + 1, sizeof(int)), out id);
            if (res)
            {
                if (!lp_matrix.get_colIndexA(lp, target, coltarget, false))
                {
                    memvector = coltarget.ToString();
                    lp_utils.mempool_releaseVector(lp.workarrays, ref memvector, 0);
                    return (0);
                }
            }
            else
                throw new Exception("lp_utils.mempool_obtainVector");
            int? Parameter = null;
            lp_matrix.bsolve(lp, 0, ref duals[0], ref Parameter, lp.epsmachine * DOUBLEROUND, 1.0);
            int nzinput = 0;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            lp_matrix.prod_xA(lp, ref coltarget, ref duals[0][0], ref nzinput, lp.epsmachine, 1.0, ref duals[0][0], ref nzduals[0][0], lp_matrix.MAT_ROUNDDEFAULT | lp_matrix.MAT_ROUNDRC);
            //FIX_133d8fd1-d4bf-4e73-ac14-1bb037ba574f 29/11/18
            memvector = coltarget.ToString();
            lp_utils.mempool_releaseVector(lp.workarrays, ref memvector, 0);

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

        internal new static bool compare_basis(lprec lp)
        /* Compares the last pushed basis with the currently active basis */
        {
            int i;
            int j;
            bool same_basis = true;

            if (lp.bb_basis == null)
                return false;

            /* Loop over basis variables until a mismatch (order can be different) */
            i = 1;
            while (same_basis && (i <= lp.rows))
            {
                j = 1;
                while (same_basis && (j <= lp.rows))
                {
                    same_basis = (bool)(lp.bb_basis.var_basic[i] != lp.var_basic[j]);
                    j++;
                }
                same_basis = !same_basis;
                i++;
            }
            /* Loop over bound status indicators until a mismatch */
            i = 1;
            while (same_basis && (i <= lp.sum))
            {
                same_basis = (lp.bb_basis.is_lower[i] && lp.is_lower[i]);
                i++;
            }

            return (same_basis);
        }

        private static bool validate_bounds(lprec lp, double[] upbo, double[] lowbo)
        /* Check if all bounds are Explicitly set working bounds to given vectors without pushing or popping */
        {
            bool ok;
            int i;

            ok = (bool)((upbo != null) || (lowbo != null));
            if (ok)
            {
                for (i = 1; i <= lp.sum; i++)
                {
                    if ((lowbo[i] > upbo[i]) || (lowbo[i] < lp.orig_lowbo[i]) || (upbo[i] > lp.orig_upbo[i]))
                    {
                        break;
                    }
                }
                ok = (bool)(i > lp.sum);
            }
            return (ok);
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
                LpCls.set_action(ref lp.spx_action, ACTION_REBASE | ACTION_REINVERT);
            }
            return (ok);
        }

        internal static new void set_infinite(lprec lp, double infinite)
        { throw new NotImplementedException(); }

        internal static new double get_infinite(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_epsperturb(lprec lp, double epsperturb)
        { throw new NotImplementedException(); }

        internal static new double get_epsperturb(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_epspivot(lprec lp, double epspivot)
        { throw new NotImplementedException(); }

        internal static new double get_epspivot(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_epsint(lprec lp, double epsint)
        { throw new NotImplementedException(); }

        internal static new double get_epsint(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_epsb(lprec lp, double epsb)
        { throw new NotImplementedException(); }

        internal static new double get_epsb(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_epsd(lprec lp, double epsd)
        { throw new NotImplementedException(); }

        internal static new double get_epsd(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_epsel(lprec lp, double epsel)
        { throw new NotImplementedException(); }

        internal static new double get_epsel(lprec lp)
        { throw new NotImplementedException(); }

        internal static new bool set_epslevel(lprec lp, int epslevel)
        { throw new NotImplementedException(); }

        internal static new void set_scaling(lprec lp, int scalemode)
        { throw new NotImplementedException(); }

        internal static new int get_scaling(lprec lp)
        { throw new NotImplementedException(); }

        internal static new bool is_scaletype(lprec lp, int scaletype)
        { throw new NotImplementedException(); }

        internal static new void set_scalelimit(lprec lp, double scalelimit)
        { throw new NotImplementedException(); }

        internal static new double get_scalelimit(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_improve(lprec lp, int improve)
        { throw new NotImplementedException(); }

        internal static new int get_improve(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_lag_trace(lprec lp, byte lag_trace)
        { throw new NotImplementedException(); }

        internal static new bool is_lag_trace(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_pivoting(lprec lp, int piv_rule)
        { throw new NotImplementedException(); }

        internal static new int get_pivoting(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_break_at_first(lprec lp, byte break_at_first)
        { throw new NotImplementedException(); }

        internal static new bool is_break_at_first(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_bb_floorfirst(lprec lp, int bb_floorfirst)
        { throw new NotImplementedException(); }

        internal static new int get_bb_floorfirst(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_break_at_value(lprec lp, double break_at_value)
        { throw new NotImplementedException(); }

        internal static new double get_break_at_value(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_negrange(lprec lp, double negrange)
        { throw new NotImplementedException(); }

        internal static new double get_negrange(lprec lp)
        { throw new NotImplementedException(); }

        internal static new int get_max_level(lprec lp)
        { throw new NotImplementedException(); }

        internal static new double get_working_objective(lprec lp)
        { throw new NotImplementedException(); }

        internal static new double get_var_primalresult(lprec lp, int index)
        { throw new NotImplementedException(); }

        internal static new double get_var_dualresult(lprec lp, int index)
        { throw new NotImplementedException(); }

        internal static new bool get_ptr_variables(lprec lp, double[][] @var)
        { throw new NotImplementedException(); }

        internal static new bool get_ptr_constraints(lprec lp, double[][] constr)
        { throw new NotImplementedException(); }

        internal static new bool get_constraints(lprec lp, ref double constr)
        { throw new NotImplementedException(); }

        internal static new bool get_sensitivity_rhs(lprec lp, ref double duals, ref double dualsfrom, ref double dualstill)
        { throw new NotImplementedException(); }

        internal static bool get_basis(lprec lp, ref int bascolumn, bool nonbasic)
        {
            throw new NotImplementedException();
        }

        internal static new bool get_sensitivity_objex(lprec lp, ref double objfrom, ref double objtill, ref double objfromvalue, ref double objtillvalue)
        {
            throw new NotImplementedException();
        }

        internal static int bin_count(lprec lp, bool working)
        {
            throw new NotImplementedException();
        }

        internal static new string get_origcol_name(lprec lp, int colnr)
        {
            throw new NotImplementedException();
        }

        internal static new string get_origrow_name(lprec lp, int rownr)
        {
            throw new NotImplementedException();
        }

        internal static new bool get_dual_solution(lprec lp, ref double rc)
        {
            throw new NotImplementedException();
        }

        internal static bool write_XLI(lprec lp, ref string filename, ref string options, bool results)
        {
            throw new NotImplementedException();
        }

        /* External language interface routines */
        /* DON'T MODIFY */
        internal static new lprec read_XLI(ref string xliname, ref string modelname, ref string dataname, ref string options, int verbose)
        {
            throw new NotImplementedException();
        }


        internal static new bool has_XLI(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static bool get_primal_solution(lprec lp, double[] pv)
        {
            throw new NotImplementedException();
        }

        internal static new bool get_ptr_primal_solution(lprec lp, double[][] pv)
        {
            throw new NotImplementedException();
        }

        internal static int get_nameindex(lprec lp, ref string varname, bool isrow)
        {
            throw new NotImplementedException();
        }

        internal static void set_use_names(lprec lp, bool isrow, bool use_names)
        {
            throw new NotImplementedException();
        }

        internal static new bool get_ptr_dual_solution(lprec lp, double[][] rc)
        {
            throw new NotImplementedException();
        }

        internal static bool is_use_names(lprec lp, bool isrow)
        {
            throw new NotImplementedException();
        }

        internal static new bool get_lambda(lprec lp, ref double lambda)
        {
            throw new NotImplementedException();
        }

        internal static new bool get_ptr_lambda(lprec lp, double[][] lambda)
        {
            throw new NotImplementedException();
        }

        internal static new int get_lp_index(lprec lp, int orig_index)
        {
            throw new NotImplementedException();
        }

        internal static new int get_orig_index(lprec lp, int lp_index)
        {
            throw new NotImplementedException();
        }

        internal static new bool is_feasible(lprec lp, ref double values, double threshold)
        /* Recommend to use threshold = lp->epspivot */
        {
            throw new NotImplementedException();
        }

        internal static new int column_in_lp(lprec lp, ref double testcolumn)
        {
            throw new NotImplementedException();
        }

        internal static double compute_violation(lprec lp, int row_nr)
        /* Returns the bound violation of a given basic variable; the return
           value is negative if it is below is lower bound, it is positive
           if it is greater than the upper bound, and zero otherwise. */
        {
            throw new NotImplementedException();
        }

        internal static bool isPrimalSimplex(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static bool isPhase1(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static int findBasicFixedvar(lprec lp, int afternr, bool slacksonly)
        {
            throw new NotImplementedException();
        }

        internal static bool isDegenerateBasis(lprec lp, int basisvar)
        {
            throw new NotImplementedException();
        }

        internal static new void default_basis(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static new int get_basiscrash(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static new void reset_basis(lprec lp)
        {
            throw new NotImplementedException();
        }

        internal static new void set_basiscrash(lprec lp, int mode)
        {
            throw new NotImplementedException();
        }

        internal static bool set_basis(lprec lp, ref int bascolumn, bool nonbasic) // Added by KE
        {
            throw new NotImplementedException();
        }


        internal new static bool is_BasisReady(lprec lp)
        {
            return ((bool)(lp.var_basic[0] != lp_types.AUTOMATIC));
        }

        private bool set_obj_fnex(lprec lp, int count, double[] row, int[] colno)
        {
            bool chsgn = is_maxim(lp);
            int i;
            int ix;
            double value;

            if (row == null)
                return false;

            else if (colno == null)
            {
                if (count <= 0)
                    count = lp.columns;
                for (i = 1; i <= count; i++)
                {
                    value = row[i];
#if DoMatrixRounding
	  value = roundToPrecision(value, lp.matA.epsvalue);
#endif
                    lp.orig_obj[i] = lp_types.my_chsign(chsgn, lp_scale.scaled_mat(lp, value, 0, i));
                }
            }
            else
            {
                /*NOT REQUIRED
                MEMCLEAR(lp.orig_obj, lp.columns + 1);
                */
                for (i = 0; i < count; i++)
                {
                    ix = colno[i];
                    value = row[i];
#if DoMatrixRounding
	  value = roundToPrecision(value, lp.matA.epsvalue);
#endif
                    lp.orig_obj[ix] = lp_types.my_chsign(chsgn, lp_scale.scaled_mat(lp, value, 0, ix));
                }
            }

            return true;
        }

        internal new bool set_binary(lprec lp, int colnr, bool must_be_bin)
        {
            bool status = false;

            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "set_binary: Column {0} out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return (status);
            }

            status = set_int(lp, colnr, must_be_bin);
            if (status && must_be_bin)
            {
                status = set_bounds(lp, colnr, 0, 1);
            }
            return (status);
        }

        internal new void set_verbose(lprec lp, int verbose)
        {
            lp.verbose = verbose;
        }

        internal new int solve(lprec lp)
        {
#if FPUexception
  catchFPU(_EM_INVALID | _EM_ZERODIVIDE | _EM_OVERFLOW | _EM_UNDERFLOW);
#endif
            if (has_BFP(lp))
            {
                lp.solvecount++;
                if (is_add_rowmode(lp))
                {
                    set_add_rowmode(lp, false);
                }
                return (lp_simplex.lin_solve(lp));
            }
            else
            {
                return (NOBFP);
            }
        }

        /* SUPPORT FUNCTION FOR BASIS FACTORIZATION PACKAGES */
        internal new bool has_BFP(lprec lp)
        {
            //C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
            return (is_nativeBFP(lp)
       //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if LoadInverseLib == TRUE
//C++ TO C# CONVERTER TODO TASK: Statements that are interrupted by preprocessor statements are not converted by C++ to C# Converter:
	   || (bool)(lp.hBFP != null)
#endif
       );
        }

        internal new bool is_add_rowmode(lprec lp)
        {
            return (lp.matA.is_roworder);
        }

        internal new static void unset_OF_p1extra(lprec lp)
        {
            lp.P1extraVal = 0;
            /*NOT REQUIRED
            FREE(lp->obj);
            */
        }

        private void get_partialprice(lprec lp, ref int blockcount, ref int[] blockstart, bool isrow)
        {
            partialrec blockdata;

            /* Determine partial target (rows or columns) */
            if (isrow)
            {
                blockdata = lp.rowblocks;
            }
            else
            {
                blockdata = lp.colblocks;
            }

            blockcount = LpPrice.partial_countBlocks(lp, isrow);
            if ((blockdata != null) && (blockstart != null))
            {
                int i = 0;
                int k = blockcount;
                if (!isrow)
                {
                    i++;
                }
                /*NOT REQUIRED
                MEMCOPY(blockstart, blockdata.blockend + i, k - i);
                */
                if (!isrow)
                {
                    k -= i;
                    for (i = 0; i < k; i++)
                    {
                        blockstart[i] -= lp.rows;
                    }
                }
            }

        }


        /* Solution-related functions */
        internal new static bool bb_better(lprec lp, int target, int mode)
        /* Must handle four modes (logic assumes Min!):
              -----|--.--|----->
           1  ++++++-----------  LHS exclusive test point is better
           2  +++++++++--------  LHS inclusive
           3  ++++++-----++++++  LHS+RHS exclusive
           4  --------+++++++++  RHS inclusive
           5  -----------++++++  RHS exclusive
        */
        {
            double epsvalue;
            double offset = lprec.epsprimal;
            double refvalue = lp.infinite;
            double testvalue = lp.solution[0];
            bool ismax = is_maxim(lp);
            bool relgap = is_action(mode, OF_TEST_RELGAP);
            bool fcast = is_action(target, OF_PROJECTED);
            bool delta = is_action(target, OF_DELTA);
            lp_report objlp_report = new lp_report();
            string msg = "";

            if (relgap)
            {
                epsvalue = lp.mip_relgap;
                clear_action(ref mode, OF_TEST_RELGAP);
            }
            else
            {
                epsvalue = lp.mip_absgap;
            }

            if (delta)
            {
                clear_action(ref target, OF_DELTA);
            }
            if (fcast)
            {
                clear_action(ref target, OF_PROJECTED);
            }
#if Paranoia
  if ((mode < OF_TEST_BT) || (mode > OF_TEST_WT))
  {
	report(lp, SEVERE, "bb_better: Passed invalid mode '%d'\n", mode);
  }
#endif

            switch (target)
            {
                case OF_RELAXED:
                    refvalue = lp.real_solution;
                    break;
                case OF_INCUMBENT:
                    refvalue = (lp.best_solution[0] != null) ? Convert.ToDouble(lp.best_solution[0]) : 0;
                    break;
                case OF_WORKING:
                    refvalue = lp_types.my_chsign(!ismax, lp.bb_workOF);
                    if (fcast)
                    {
                        testvalue = lp_types.my_chsign(!ismax, lp.longsteps.obj_last) - epsvalue;
                    }
                    else
                    {
                        testvalue = lp_types.my_chsign(!ismax, lp.rhs[0]);
                    }
                    break;
                case OF_USERBREAK:
                    refvalue = lp.bb_breakOF;
                    break;
                case OF_HEURISTIC:
                    refvalue = lp.bb_heuristicOF;
                    break;
                case OF_DUALLIMIT:
                    refvalue = lp.bb_limitOF;
                    break;
                default:
                    msg = "bb_better: Passed invalid test target '{0}'\n";
                    objlp_report.report(lp, SEVERE, ref msg, target);
                    return false;
            }

            /* Adjust the test value for the desired acceptability window */
            if (delta)
            {
                commonlib.doubleSETMAX(epsvalue, lp.bb_deltaOF - epsvalue);
            }
            else
            {
                epsvalue = lp_types.my_chsign(target >= OF_USERBREAK, epsvalue); // *** This seems Ok, but should be verified
            }
            testvalue += lp_types.my_chsign(ismax, epsvalue);

            /* Compute the raw test value */
            if (relgap)
            {
                testvalue = lp_types.my_reldiff(testvalue, refvalue);
            }
            else
            {
                testvalue -= refvalue;
            }

            /* Make test value adjustment based on the selected option */
            if (mode == OF_TEST_NE)
            {
                relgap = (bool)(System.Math.Abs(testvalue) >= offset);
            }
            else
            {
                testvalue = lp_types.my_chsign(mode > OF_TEST_NE, testvalue);
                testvalue = lp_types.my_chsign(ismax, testvalue);
                relgap = (bool)(testvalue < offset);
            }
            return (relgap);
        }

        internal new static double get_objective(lprec lp)
        {
            lp_report objlp_report = new lp_report();
            if (lp.spx_status == OPTIMAL)
            {
                ;
            }
            else if (!lp.basis_valid)
            {
                string msg = "get_objective: Not a valid basis\n";
                objlp_report.report(lp, CRITICAL, ref msg);
                return (0.0);
            }

            return ((lp.best_solution[0] != null) ? Convert.ToDouble(lp.best_solution[0]) : 0);
        }

        internal bool get_variables(lprec lp, double @var)
        {
            lp_report objlp_report = new lp_report();
            if (lp.spx_status == OPTIMAL)
            {
                ;
            }
            else if (!lp.basis_valid)
            {
                string msg = "get_variables: Not a valid basis\n";
                objlp_report.report(lp, CRITICAL, ref msg);
                return false;
            }
            /*NOT REQUIRED
            MEMCOPY(@var, lp.best_solution + (1 + lp.rows), lp.columns);
            */
            return true;
        }

        internal static int unload_basis(lprec lp, bool restorelast)
        {
            int levelsunloaded = 0;

            if (lp.bb_basis != null)
            {
                while (pop_basis(lp, restorelast))
                {
                    levelsunloaded++;
                }
            }
            return (levelsunloaded);
        }

        internal new static int unload_BB(lprec lp)
        {
            int levelsunloaded = 0;

            /* uncomment if required // TODO_12/11/2018
            ERROR: Cannot convert type 'ZS.Math.Optimization.BBrec' to 'bool'
            if (lp.bb_bounds != null)
            {
                while ((bool)lp_mipbb.pop_BB(lp.bb_bounds))
                {
                    levelsunloaded++;
                }
            }
            */
            return (levelsunloaded);
        }

        /* Bounds updating and unloading routines; requires that the
   current values for upbo and lowbo are in the original base. */
        private static int perturb_bounds(lprec lp, BBrec perturbed, bool doRows, bool doCols, bool includeFIXED)
        {
            int i;
            int ii;
            int n = 0;
            double new_lb;
            double new_ub;
            double[] upbo;
            double[] lowbo;

            if (perturbed == null)
            {
                return (n);
            }

            /* Map reference bounds to previous state, i.e. cumulate
               perturbations in case of persistent problems */
            upbo = perturbed.upbo;
            lowbo = perturbed.lowbo;

            /* Set appropriate target variable range */
            i = 1;
            ii = lp.rows;
            if (!doRows)
            {
                i += ii;
            }
            if (!doCols)
            {
                ii = lp.sum;
            }

            /* Perturb (expand) finite variable bounds randomly */
            for (; i <= ii; i++)
            {

                /* Don't perturb regular slack variables */
                if ((i <= lp.rows) && (lowbo[i] == 0) && (upbo[i] >= lp.infinite))
                {
                    continue;
                }

                new_lb = lowbo[i];
                new_ub = upbo[i];

                /* Don't perturb fixed variables if not specified */
                if (!includeFIXED && (new_ub == new_lb))
                {
                    continue;
                }

                /* Lower bound for variables (consider implementing RHS here w/contentmode== AUTOMATIC) */
                if ((i > lp.rows) && (new_lb < lp.infinite))
                {
                    new_lb = lp_utils.rand_uniform(lp, RANDSCALE) + 1;
                    new_lb *= lp.epsperturb;
                    lowbo[i] -= new_lb;
                    n++;
                }

                /* Upper bound */
                if (new_ub < lp.infinite)
                {
                    new_ub = lp_utils.rand_uniform(lp, RANDSCALE) + 1;
                    new_ub *= lp.epsperturb;
                    upbo[i] += new_ub;
                    n++;
                }
            }

            /* Make sure we start from scratch */
            set_action(ref lp.spx_action, ACTION_REBASE);

            return (n);
        }


        private new static bool impose_bounds(lprec lp, ref double upbo, ref double lowbo)
        /* Explicitly set working bounds to given vectors without pushing or popping */
        {
            bool ok;

            ok = (bool)((upbo != null) || (lowbo != null));
            if (ok)
            {
                /*NOT REQUIRED
                if ((upbo != null) && (upbo != lp.upbo))
                {
                    MEMCOPY(lp.upbo, upbo, lp.sum + 1);
                }
                if ((lowbo != null) && (lowbo != lp.lowbo))
                {
                    MEMCOPY(lp.lowbo, lowbo, lp.sum + 1);
                }
                */
                if (lp.bb_bounds != null)
                {
                    lp.bb_bounds.UBzerobased = false;
                }
                set_action(ref lp.spx_action, ACTION_REBASE);
            }
            set_action(ref lp.spx_action, ACTION_RECOMPUTE);
            return (ok);
        }


        internal static new void delete_lp(lprec lp)
        {
            if (lp == null)
                return;
            /*NOT REQUIRED
            FREE(lp.rowcol_name);
            FREE(lp.lp_name);
            FREE(lp.ex_status);
            
            if (lp.names_used)
            {
                FREE(lp.row_name);
                FREE(lp.col_name);
                free_hash_table(lp.rowname_hashtab);
                free_hash_table(lp.colname_hashtab);
            }

            mat_free(lp.matA);
            lp.bfp_free(lp);
            //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if LoadInverseLib == TRUE
  if (lp.hBFP != null)
  {
	set_BFP(lp, null);
  }
#endif
            //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if LoadLanguageLib == TRUE
  if (lp.hXLI != null)
  {
	set_XLI(lp, null);
  }
#endif

            unset_OF_p1extra(lp);
            FREE(lp.orig_obj);
            FREE(lp.orig_rhs);
            FREE(lp.rhs);
            FREE(lp.var_type);
            set_var_weights(lp, null);
            FREE(lp.bb_varbranch);
            FREE(lp.sc_lobound);
            FREE(lp.var_is_free);
            FREE(lp.orig_upbo);
            FREE(lp.orig_lowbo);
            FREE(lp.upbo);
            FREE(lp.lowbo);
            FREE(lp.var_basic);
            FREE(lp.is_basic);
            FREE(lp.is_lower);
            if (lp.bb_PseudoCost != null)
            {
                //   report(lp, SEVERE, "delete_lp: The B&B pseudo-cost array was not cleared on delete\n");
                free_pseudocost(lp);
            }
            if (lp.bb_bounds != null)
            {
                report(lp, SEVERE, "delete_lp: The stack of B&B levels was not empty (failed at %.0f nodes)\n", (double)lp.bb_totalnodes);
                unload_BB(lp);
            }
            if (lp.bb_basis != null)
            {
                   //report(lp, SEVERE, "delete_lp: The stack of saved bases was not empty on delete\n"); 
                unload_basis(lp, 0);
            }

            FREE(lp.rejectpivot);
            partial_freeBlocks((lp.rowblocks));
            partial_freeBlocks((lp.colblocks));
            multi_free((lp.multivars));
            multi_free((lp.longsteps));

            FREE(lp.solution);
            FREE(lp.best_solution);
            FREE(lp.full_solution);

            presolve_freeUndo(lp);
            mempool_free((lp.workarrays));

            freePricer(lp);

            FREE(lp.drow);
            FREE(lp.nzdrow);

            FREE(lp.duals);
            FREE(lp.full_duals);
            FREE(lp.dualsfrom);
            FREE(lp.dualstill);
            FREE(lp.objfromvalue);
            FREE(lp.objfrom);
            FREE(lp.objtill);
            FREE(lp.row_type);

            if (lp.sos_vars > 0)
            {
                FREE(lp.sos_priority);
            }
            free_SOSgroup((lp.SOS));
            free_SOSgroup((lp.GUB));
            freecuts_BB(lp);

            if (lp.scaling_used)
            {
                FREE(lp.scalars);
            }
            if (lp.matL != null)
            {
                FREE(lp.lag_rhs);
                FREE(lp.lambda);
                FREE(lp.lag_con_type);
                mat_free(lp.matL);
            }
            if (lp.streamowned)
            {
                set_outputstream(lp, null);
            }

            //C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#if libBLAS > 0
  if (!is_nativeBLAS())
  {
	unload_BLAS();
  }
#endif

            FREE(lp);

#if FORTIFY
	// Fortify_LeaveScope(); 
# endif
*/
        }

        internal new static bool isPrimalFeasible(lprec lp, double tol, int[] infeasibles, ref double feasibilitygap)
        {
            int i;
            bool feasible = true;

            /* This is a short-hand call to rowdual() to check for primal infeasibility */

#if false
// /* Traditional indexing style */
//  for(i = 1; i <= lp->rows; i++) {
//    feasible = isBasisVarFeasible(lp, tol, i);
#else
            /* Fast array pointer style */
            double rhsptr = new double();
            LpCls objLpCls = new LpCls();

            int idxptr;

            if (infeasibles != null)
            {
                infeasibles[0] = 0;
            }
            for (i = 1, rhsptr = lp.rhs[0] + 1, idxptr = lp.var_basic[0] + 1; (i <= lp.rows); i++, rhsptr++, idxptr++)
            {
                feasible = true;
                /*    if(((*rhsptr) < lp->lowbo[*idxptr]-tol) || ((*rhsptr) > lp->upbo[*idxptr]+tol)) */
                if (((rhsptr) < -tol) || ((rhsptr) > lp.upbo[idxptr] + tol))
                {
                    feasible = false;
                }
#endif
                if (!feasible)
                {
                    if (infeasibles == null)
                    {
                        break;
                    }
                    infeasibles[0]++;
                    infeasibles[infeasibles[0]] = i;
                }
            }

            /* Compute feasibility gap (could actually do this calculation above) */
            if (feasibilitygap != null)
            {
                if (feasible)
                {
                    feasibilitygap = 0.0;
                }
                else
                {
                    feasibilitygap = objLpCls.feasibilityOffset(lp, 0);
                }
            }

            return (feasible);
        }

        internal static double feasibilityOffset(lprec lp, bool isdual)
        {
            int i;
            int j;
            double f = new double();
            double Extra = new double();

            Extra = 0;
            if (isdual)
            {
                /* This section computes a OF offset to ensure that the dual phase 1 is
                   feasible.  It is used to compute a primal feasible base that can be
                   passed to the primal simplex in phase 2. */
#if false
//
// /* This is the legacy (v3.2-) P1extraVal logic that sets Extra to be the
//    smallest negative reduced cost. Note that the reduced costs are the
//    values of the dual slacks, which are [0..Inf> for feasibility.
//    If we have negative reduced costs for bounded non-basic variables, we
//    can simply switch the bound to obtain feasibility and possibly avoid
//    having to set Extra. */
//    if(!isDualFeasible(lp, lp->epsprimal, NULL, NULL, &f)
//      Extra = f;
//
#else
                /* Find the most negative of the objective coefficients. We will subtract this
                   value from every element of the objective row, making it non-negative and
                   the problem therefore dual feasible. */
                for (i = 1; i <= lp.columns; i++)
                {
                    f = lp.orig_obj[i];
                    if (f < Extra)
                    {
                        //ORIGINAL LINE: Extra = f;
                        Extra = (f);
                    }
                }
#endif
            }

            else
            {
                /* Set Extra to be the index of the most negative of the net RHS coefficients;
                   this approach can be used in the primal phase 1 followed by the dual phase 2
                   and when there are no ranged constraints.  When there are ranged constraints,
                   additional artificial variables must be introduced. */
                Extra = 0;
                j = 0;
                Extra = lp.infinite;
                for (i = 1; i <= lp.rows; i++)
                {
                    f = lp.rhs[i];
                    if (f < Extra)
                    {
                        //ORIGINAL LINE: Extra = f;
                        Extra = (f);
                        j = i;
                    }
                }
                Extra = j;
            }

            return (Extra);

        }

        internal static bool isDualFeasible(lprec lp, double tol, ref int boundflipcount, int[] infeasibles, double feasibilitygap)
        {
            int i;
            int varnr;
            int n = 0;
            int m = 0;
            int target = SCAN_ALLVARS + USE_NONBASICVARS;
            double f = 0;
            bool feasible;
            bool islower;

            LpCls objLpCls = new LpCls();

            /* The reduced costs are the values of the dual slacks, which
               are [0..Inf> for feasibility.  If we have negative reduced costs
               for bounded non-basic variables, we can simply switch the bound
               of bounded variables to obtain dual feasibility and possibly avoid
               having to use dual simplex phase 1. */
            if ((infeasibles != null) || (boundflipcount != null))
            {
                //int[][] nzdcol_new and double[][] dcol_new are taken new for dual matrix, need to check at runtime.
                int[][] nzdcol_new = null;
                double[][] dcol_new = null;
                int[] nzdcol = null;
                double d = new double();
                double[] dcol = null;

                //ORIGINAL LINE: f = compute_dualslacks(lp, target, dcol, nzdcol, false);
                f = compute_dualslacks(lp, target, dcol_new, nzdcol_new, false);
                if (nzdcol != null)
                {
                    for (i = 1; i <= nzdcol[0]; i++)
                    {
                        varnr = nzdcol[i];
                        islower = lp.is_lower[varnr];
                        d = lp_types.my_chsign(!islower, dcol[varnr]);

                        /* Don't bother with uninteresting non-basic variables */
                        if ((d > -tol) || lp_types.my_unbounded(lp, varnr) || objLpCls.is_fixedvar(lp, varnr)) // Equality slack or a fixed variable ("type 3")
                        {
                            continue;
                        }

                        /* Check if we have non-flippable bounds, i.e. an unbounded variable
                           (types 2+4), or bounded variables (type 3), and if the counter is NULL. */
                        if ((boundflipcount == null) || ((lp.bb_level <= 1) && (lp_types.my_rangebo(lp, varnr) > System.Math.Abs(lp.negrange))) || (islower && lp_types.my_infinite(lp, lp.upbo[varnr])) || (!islower && lp_types.my_infinite(lp, lp_types.my_lowbo(lp, varnr))))
                        {
                            m++;
                            if (infeasibles != null)
                            {
                                infeasibles[m] = varnr;
                            }
                        }
                        /* Only do bound flips if the user-provided counter is non-NULL */
                        else
                        {
                            lp.is_lower[varnr] = !islower;
                            n++;
                        }
                    }
                }
                if (infeasibles != null)
                {
                    infeasibles[0] = m;
                }

                //NOT REQUIRED
                //FREE(dcol);
                //FREE(nzdcol);
                if (n > 0)
                {
                    LpCls.set_action(ref lp.spx_action, ACTION_RECOMPUTE);
                    if (m == 0)
                    {
                        f = 0;
                    }
                }
            }
            else
            {
                f = compute_dualslacks(lp, target, null, null, false);
            }
            /*    f = feasibilityOffset(lp, TRUE); */
            /* Safe legacy mode */

            /* Do an extra scan to see if there are bounded variables in the OF not present in any constraint;
               Most typically, presolve fixes such cases, so this is rarely encountered. */

            varnr = lp.rows + 1;
            for (i = 1; i <= lp.columns; i++, varnr++)
            {
                if (lp_matrix.mat_collength(lp.matA, i) == 0)
                {
                    islower = lp.is_lower[varnr];
                    if ((lp_types.my_chsign(islower, lp.orig_obj[i]) > 0) && !lp_SOS.SOS_is_member(lp.SOS, 0, i))
                    {
                        lp.is_lower[varnr] = !islower;
                        if ((islower && lp_types.my_infinite(lp, lp.upbo[varnr])) || (!islower && lp_types.my_infinite(lp, lp_types.my_lowbo(lp, varnr))))
                        {
                            lp.spx_status = UNBOUNDED;
                            break;
                        }
                        /* lp->is_lower[varnr] = !islower; */
                        n++;
                    }
                }
            }

            /* Return status */

            if (boundflipcount != null)
            {
                boundflipcount = n;
            }
            if (feasibilitygap != null)
            {
                lp_types.my_roundzero(f, tol);
                feasibilitygap = f;
            }
            feasible = (bool)((f == 0) && (m == 0));

            return (feasible);
        }
        internal static new bool is_fixedvar(lprec lp, int varnr)
        {
            if (lp.bb_bounds == null)
            {
                if (varnr <= lp.rows)
                {
                    return ((bool)(lp.orig_upbo[varnr] < lp.epsmachine));
                }
                else
                {
                    return ((bool)(lp.orig_upbo[varnr] - lp.orig_lowbo[varnr] < lp.epsmachine));
                }
            }
            else if ((varnr <= lp.rows) || (lp.bb_bounds.UBzerobased == true))
            {
                return ((bool)(lp.upbo[varnr] < lprec.epsvalue));
            }
            else
            {
                return ((bool)(lp.upbo[varnr] - lp.lowbo[varnr] < lprec.epsvalue));
            }
        }

        internal static int get_basiscolumn(lprec lp, int j, int[] rn, double?[] bj)
        /* This routine returns sparse vectors for all basis
           columns, including the OF dummy (index 0) and slack columns.
           NOTE that the index usage is nonstandard for lp_solve, since
           the array offset is 1, not 0. */
        {
            int k = lp.bfp_rowoffset(lp);
            int matbase = lp.bfp_indexbase(lp);

            /* Do target index adjustment (etaPFI with matbase==0 is special case) */
            if (matbase > 0)
            {
                matbase += k - 1;
            }

            /* Convert index of slack and user columns */
            j -= k;
            if ((j > 0) && !lp.bfp_isSetI(lp))
            {
                j = lp.var_basic[j];
            }

            /* Process OF dummy and slack columns (always at lower bound) */
            if (j <= lp.rows)
            {
                rn[1] = j + matbase;
                bj[1] = 1.0;
                k = 1;
            }
            /* Process user columns (negated if at lower bound) */
            else
            {
                int maxabs = 0;
                k = obtain_column(lp, j, ref bj, ref rn, ref maxabs);
                if (matbase != 0)
                {
                    for (j = 1; j <= k; j++)
                    {
                        rn[j] += matbase;
                    }
                }
            }

            return (k);
        }

        public new bool is_presolve(lprec lp, int testmask)
        {
            return ((bool)((lp.do_presolve == testmask) || ((lp.do_presolve & testmask) != 0)));
        }

        internal static new bool set_mat(lprec lp, int rownr, int colnr, double value)
        {
            string msg = "";
            if ((rownr < 0) || (rownr > lp.rows))
            {
                msg = "set_mat: Row %d out of range\n";
                lp.report(lp, IMPORTANT, ref msg, rownr);
                return (false);
            }
            if ((colnr < 1) || (colnr > lp.columns))
            {
                msg = "set_mat: Column %d out of range\n";
                lp.report(lp, IMPORTANT, ref msg, colnr);
                return (false);
            }

#if DoMatrixRounding
  if (rownr == 0)
  {
	value = roundToPrecision(value, lp.matA.epsvalue);
  }
#endif
            value = lp_scale.scaled_mat(lp, value, rownr, colnr);
            if (rownr == 0)
            {
                lp.orig_obj[colnr] = lp_types.my_chsign(is_chsign(lp, rownr), value);
                return (true);
            }
            else
            {
                return (lp_matrix.mat_setvalue(lp.matA, rownr, colnr, value, false));
            }
        }

        internal static new double get_upbo(lprec lp, int colnr)
        {
            double value;

            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "get_upbo: Column %d out of range\n";
                lp.report(lp, IMPORTANT, ref msg, colnr);
                return (0);
            }

            value = lp.orig_upbo[lp.rows + colnr];
            value = lp_scale.unscaled_value(lp, value, lp.rows + colnr);
            return (value);
        }

        internal static bool inc_lag_space(lprec lp, int deltarows, bool ignoreMAT)
        {
            int newsize;

            if (deltarows > 0)
            {

                newsize = get_Lrows(lp) + deltarows;

                //NOT REQUIRED
                /* Reallocate arrays */
                /*if (!allocREAL(lp, lp.lag_rhs, newsize + 1, AUTOMATIC) || !allocREAL(lp, lp.lambda, newsize + 1, AUTOMATIC) || !allocINT(lp, lp.lag_con_type, newsize + 1, AUTOMATIC))
                {
                    return (0);
                }*/

                /* Reallocate the matrix (note that the row scalars are stored at index 0) */
                if (!ignoreMAT)
                {
                    if (lp.matL == null)
                    {
                        lp.matL = lp_matrix.mat_create(lp, newsize, lp.columns, lprec.epsvalue);
                    }
                    else
                    {
                        lp_matrix.inc_matrow_space(lp.matL, deltarows);
                    }
                }
                lp.matL.rows += deltarows;

            }
            /* Handle column count expansion as special case */
            else if (!ignoreMAT)
            {
                lp_matrix.inc_matcol_space(lp.matL, lp.columns_alloc - lp.matL.columns_alloc + 1);
            }


            return (true);
        }

        internal new bool get_ptr_sensitivity_rhs(lprec lp, double[][] duals, double[][] dualsfrom, double[][] dualstill)
        {
            string msg = "";
            if (!lp.basis_valid)
            {
                msg = "get_ptr_sensitivity_rhs: Not a valid basis\n";
                report(lp, CRITICAL, ref msg);
                return (false);
            }

            if (duals != null)
            {
                if (lp.duals == null)
                {
                    if ((MIP_count(lp) > 0) && (lp.bb_totalnodes > 0))
                    {
                        msg = "get_ptr_sensitivity_rhs: Sensitivity unknown\n";
                        report(lp, CRITICAL, ref msg);
                        return (false);
                    }
                    if (!construct_duals(lp))
                    {
                        return (false);
                    }
                }
                duals[0][0] = Convert.ToDouble(lp.duals[0] + 1);
            }

            if ((dualsfrom != null) || (dualstill != null))
            {
                if ((lp.dualsfrom == null) || (lp.dualstill == null))
                {
                    if ((MIP_count(lp) > 0) && (lp.bb_totalnodes > 0))
                    {
                        msg = "get_ptr_sensitivity_rhs: Sensitivity unknown\n";
                        report(lp, CRITICAL, ref msg);
                        return (false);
                    }
                    construct_sensitivity_duals(lp);
                    if ((lp.dualsfrom == null) || (lp.dualstill == null))
                    {
                        return (false);
                    }
                }
                if (dualsfrom != null)
                {
                    dualsfrom[0][0] = Convert.ToDouble(lp.dualsfrom) + 1;
                }
                if (dualstill != null)
                {
                    dualstill[0][0] = Convert.ToDouble(lp.dualstill) + 1;
                }
            }
            return (true);
        }

        internal new static bool construct_duals(lprec lp)
        {
            int i;
            int n;
            //ORIGINAL LINE: int *coltarget;
            int coltarget = 0;
            string memVector = "";
            double scale0;
            double value;
            double dualOF;
            LpCls objLpCls = new LpCls();

            if (lp.duals != null)
            {
                //NOT REQUIRED
                // free_duals(lp);
            }

            //ORIGINAL LINE: if (is_action(lp.spx_action, ACTION_REBASE) || is_action(lp.spx_action, ACTION_REINVERT) || (!lp.basis_valid) || !allocREAL(lp, (lp.duals), lp.sum + 1, AUTOMATIC))
            if (is_action(lp.spx_action, ACTION_REBASE) || is_action(lp.spx_action, ACTION_REINVERT) || (!lp.basis_valid))
            {
                return (false);
            }

            /* Initialize */
            /// <summary> FIX_133d8fd1-d4bf-4e73-ac14-1bb037ba574f 29/11/18
            /// PREVIOUS: coltarget = (int)mempool_obtainVector(lp.workarrays, lp.columns + 1, sizeof(int));
            /// ERROR IN PREVIOUS: Cannot convert type 'string' to 'int'
            /// FIX 1: 
            /// </summary>
            int id = 0;
            bool res = int.TryParse(lp_utils.mempool_obtainVector(lp.workarrays, lp.columns + 1, sizeof(int)), out id);
            if (res)
            {
                if (!lp_matrix.get_colIndexA(lp, SCAN_USERVARS + USE_NONBASICVARS, coltarget, false))
                {
                    memVector = coltarget.ToString();
                    lp_utils.mempool_releaseVector(lp.workarrays, ref memVector, 0);
                    return (false);
                }
            }
            else
                throw new Exception("lp_utils.mempool_obtainVector");

            //coltarget = (int)mempool_obtainVector(lp.workarrays, lp.columns + 1, sizeof(int));
            int? Parameter1 = null;
            int Parameter2 = 0;
            memVector = coltarget.ToString();
            lp_matrix.bsolve(lp, 0, ref lp.duals, ref Parameter1, lp.epsmachine * DOUBLEROUND, 1.0);
            lp_matrix.prod_xA(lp, ref coltarget, ref lp.duals[0], ref Parameter2, lp.epsmachine, 1.0, ref lp.duals[0], ref Parameter2, lp_matrix.MAT_ROUNDDEFAULT | lp_matrix.MAT_ROUNDRC);
            lp_utils.mempool_releaseVector(lp.workarrays, ref memVector, 0);


            /* The (Lagrangean) dual values are the reduced costs of the primal slacks;
               when the slack is at its upper bound, change the sign. */
            n = lp.rows;
            for (i = 1; i <= n; i++)
            {
                if (lp.is_basic[i])
                {
                    lp.duals[i] = 0;
                }
                /* Added a test if variable is different from 0 because sometime you get -0 and this
                   is different from 0 on for example INTEL processors (ie 0 != -0 on INTEL !) PN */
                else if ((objLpCls.is_chsign(lp, 0) == objLpCls.is_chsign(lp, i)) && lp.duals[i] != 0)
                {
                    lp.duals[i] = lp_types.my_flipsign(lp.duals[i]);
                }
            }
            if (is_maxim(lp))
            {
                n = lp.sum;
                for (i = lp.rows + 1; i <= n; i++)
                {
                    lp.duals[i] = lp_types.my_flipsign(lp.duals[i]);
                }
            }

            /* If we presolved, then reconstruct the duals */
            n = lp.presolve_undo.orig_sum;
            //ORIGINAL LINE: if (((lp.do_presolve & PRESOLVE_LASTMASKMODE) != PRESOLVE_NONE) && allocREAL(lp, (lp.full_duals), n + 1, 1))
            if (((lp.do_presolve & PRESOLVE_LASTMASKMODE) != PRESOLVE_NONE))
            {
                int ix;
                int ii = lp.presolve_undo.orig_rows;

                n = lp.sum;
                for (ix = 1; ix <= n; ix++)
                {
                    i = lp.presolve_undo.var_to_orig[ix];
                    if (ix > lp.rows)
                    {
                        i += ii;
                    }
#if Paranoia
	  /* Check for index out of range due to presolve */
	  if (i > lp.presolve_undo.orig_sum)
	  {
		report(lp, SEVERE, "construct_duals: Invalid presolve variable mapping found\n");
	  }
#endif
                    lp.full_duals[i] = lp.duals[ix];
                }
                lp_presolve.presolve_rebuildUndo(lp, 0);
            }

            /* Calculate the dual OF and do scaling adjustments to the duals */
            if (lp.scaling_used)
            {
                scale0 = lp.scalars[0];
            }
            else
            {
                scale0 = 1;
            }
            dualOF = lp_types.my_chsign(is_maxim(lp), lp.orig_rhs[0]) / scale0;
            for (i = 1; i <= lp.sum; i++)
            {
                value = lp_scale.scaled_value(lp, lp.duals[i] / scale0, i);
                lp_types.my_roundzero(value, lprec.epsprimal);
                lp.duals[i] = value;
                if (i <= lp.rows)
                {
                    dualOF += value * lp.solution[i];
                }
            }

#if false
// /* See if we can make use of the dual OF;
//    note that we do not currently adjust properly for presolve */
//  if(lp->rows == lp->presolve_undo->orig_rows)
//  if(MIP_count(lp) > 0) {
//    if(is_maxim(lp)) {
//      SETMIN(lp->bb_limitOF, dualOF);
//    }
//    else {
//      SETMAX(lp->bb_limitOF, dualOF);
//    }
//  }
//  else if(fabs(my_reldiff(dualOF, lp->solution[0])) > lp->epssolution)
//    report(lp, IMPORTANT, "calculate_duals: Check for possible suboptimal solution!\n");
#endif

            return (true);
        } // construct_duals

        internal new int get_constr_type(lprec lp, int rownr)
        {
            if ((rownr < 0) || (rownr > lp.rows))
            {
                string msg = "get_constr_type: Row %d out of range\n";
                report(lp, IMPORTANT, ref msg, rownr);
                return (-1);
            }
            return (lp.row_type[rownr]);
        }

        internal static int get_multiprice(lprec lp, bool getabssize)
        {
            if ((lp.multivars == null) || (lp.multivars.used == 0))
            {
                return (0);
            }
            if (getabssize)
            {
                return (lp.multivars.size);
            }
            else
            {
                return (lp.multiblockdiv);
            }
        }


        internal bool set_partialprice(lprec lp, int blockcount, ref int? blockstart, bool isrow)
        {
            int ne;
            int i;
            int items;
            //ORIGINAL LINE: partialrec[] blockdata;
            partialrec blockdata = new partialrec();

            /* Determine partial target (rows or columns) */
            if (isrow)
            {
                blockdata = (lp.rowblocks);
            }
            else
            {
                blockdata = (lp.colblocks);
            }

            /* See if we are resetting partial blocks */
            ne = 0;
            items = (int)commonlib.IF(isrow, lp.rows, lp.columns);
            if (blockcount == 1)
            {
                //NOT REQUIRED
                //partial_freeBlocks(blockdata);
            }

            /* Set a default block count if this was not specified */
            else if (blockcount <= 0)
            {
                blockstart = null;
                if (items < DEF_PARTIALBLOCKS * DEF_PARTIALBLOCKS)
                {
                    blockcount = items / DEF_PARTIALBLOCKS + 1;
                }
                else
                {
                    blockcount = DEF_PARTIALBLOCKS;
                }
                ne = items / blockcount;
                if (ne * blockcount < items)
                {
                    ne++;
                }
            }

            /* Fill partial block arrays;
               Note: These will be modified during preprocess to reflect
                     presolved columns and the handling of slack variables. */
            if (blockcount > 1)
            {
                bool isNew = (bool)(blockdata == null);

                /* Provide for extra block with slack variables in the column mode */
                i = 0;
                if (!isrow)
                {
                    i++;
                }

                /* (Re)-allocate memory */
                if (isNew != null)
                {
                    blockdata = LpPrice.partial_createBlocks(lp, isrow);
                }

                //NOT REQUIRED
                //allocINT(lp, (blockdata.blockend), blockcount + i + 1, AUTOMATIC);
                //allocINT(lp, (blockdata.blockpos), blockcount + i + 1, AUTOMATIC);

                /* Copy the user-provided block start positions */
                if (blockstart != null)
                {
                    //NOT REQUIRED
                    //MEMCOPY(blockdata.blockend + i, blockstart, blockcount + i + 1);
                    if (!isrow)
                    {
                        blockcount++;
                        blockdata.blockend[0] = 1;
                        for (i = 1; i < blockcount; i++)
                        {
                            blockdata.blockend[i] += lp.rows;
                        }
                    }
                }

                /* Fill the block ending positions if they were not specified */
                else
                {
                    blockdata.blockend[0] = 1;
                    blockdata.blockpos[0] = 1;
                    if (ne == 0)
                    {
                        ne = items / blockcount;
                        /* Increase the block size if we have a fractional value */
                        while (ne * blockcount < items)
                        {
                            ne++;
                        }
                    }
                    i = 1;
                    if (!isrow)
                    {
                        blockdata.blockend[i] = blockdata.blockend[i - 1] + lp.rows;
                        blockcount++;
                        i++;
                        items += lp.rows;
                    }
                    for (; i < blockcount; i++)
                    {
                        blockdata.blockend[i] = blockdata.blockend[i - 1] + ne;
                    }

                    /* Let the last block handle the "residual" */
                    blockdata.blockend[blockcount] = items + 1;
                }

                /* Fill starting positions (used in multiple partial pricing) */
                for (i = 1; i <= blockcount; i++)
                {
                    blockdata.blockpos[i] = blockdata.blockend[i - 1];
                }

            }

            /* Update block count */
            blockdata.blockcount = blockcount;


            return (true);
        } // set_partialprice



        internal new static bool set_multiprice(lprec lp, int multiblockdiv)
        {
            /* See if we are resetting multiply priced column structures */
            if (multiblockdiv != lp.multiblockdiv)
            {
                if (multiblockdiv < 1)
                {
                    multiblockdiv = 1;
                }
                lp.multiblockdiv = multiblockdiv;
                LpPrice.multi_free((lp.multivars));
            }
            return true;
        }

        internal static bool solution_is_int(lprec lp, int index, bool checkfixed)
        {
#if true
  return ((bool)(lp_utils.isINT(lp, lp.solution[index]) && (!checkfixed || is_fixedvar(lp, index))));
#else
            if (lp_utils.isINT(lp, lp.solution[index]))
            {
                if (checkfixed)
                {
                    return (is_fixedvar(lp, index));
                }
                else
                {
                    return true;
                }
            }
            return false;
#endif
        } // solution_is_int


        internal new bool preprocess(lprec lp)
        {
            int i;
            int j;
            int k;
            bool ok = true;
            //ORIGINAL LINE: int *new_index = null;
            int? new_index = null;
            double hold;
            //ORIGINAL LINE: double *new_column = null;
            double? new_column = null;
            bool scaled;
            bool primal1;
            bool primal2;
            string msg = "";
            int? Parameter1 = null;
            LpCls objLpCls = new LpCls();

            /* do not process if already preprocessed */
            if (lp.wasPreprocessed)
            {
                return (Convert.ToInt32(ok));
            }

            /* Write model statistics and optionally initialize partial pricing structures */
            if (lp.lag_status != RUNNING)
            {
                bool doPP = new bool();

                /* Extract the user-specified simplex strategy choices */
                primal1 = Convert.ToBoolean((lp.simplex_strategy & SIMPLEX_Phase1_PRIMAL));
                primal2 = Convert.ToBoolean((lp.simplex_strategy & SIMPLEX_Phase2_PRIMAL));

                /* Initialize partial pricing structures */
                doPP = is_piv_mode(lp, PRICE_PARTIAL | PRICE_AUTOPARTIAL);
                /*    doPP &= (bool) (lp->columns / 2 > lp->rows); */
                if (doPP != null)
                {
                    i = LpPrice.partial_findBlocks(lp, false, false);
                    if (i < 4)
                    {
                        i = (int)(5 * System.Math.Log((double)lp.columns / lp.rows));
                    }
                    msg = "The model is %s to have %d column blocks/stages.\n";
                    report(lp, NORMAL, ref msg, (i > 1 ? "estimated" : "set"), i);
                    set_partialprice(lp, i, ref Parameter1, false);
                }
                /*    doPP &= (bool) (lp->rows / 4 > lp->columns); */
                if (doPP != null)
                {
                    i = LpPrice.partial_findBlocks(lp, false, true);
                    if (i < 4)
                    {
                        i = (int)(5 * System.Math.Log((double)lp.rows / lp.columns));
                    }
                    msg = "The model is %s to have %d row blocks/stages.\n";
                    report(lp, NORMAL, ref msg, (i > 1 ? "estimated" : "set"), i);
                    set_partialprice(lp, i, ref Parameter1, true);
                }

                /* Check for presence of valid pricing blocks if partial pricing
                  is defined, but not autopartial is not set */
                if (doPP == null && is_piv_mode(lp, PRICE_PARTIAL))
                {
                    if ((lp.rowblocks == null) || (lp.colblocks == null))
                    {
                        msg = "Ignoring partial pricing, since block structures are not defined.\n";
                        report(lp, IMPORTANT, ref msg);
                        clear_action(ref lp.piv_strategy, PRICE_PARTIAL);
                    }
                }

                /* Initialize multiple pricing block divisor */
#if false
//    if(primal1 || primal2)
//      lp->piv_strategy |= PRICE_MULTIPLE | PRICE_AUTOMULTIPLE;
#endif
                if (is_piv_mode(lp, PRICE_MULTIPLE) && (primal1 || primal2))
                {
                    doPP = is_piv_mode(lp, PRICE_AUTOMULTIPLE);
                    if (doPP != null)
                    {
                        i = (int)(2.5 * System.Math.Log((double)lp.sum));
                        commonlib.SETMAX(i, 1);
                        set_multiprice(lp, i);
                    }
                    if (lp.multiblockdiv > 1)
                    {
                        msg = "Using %d-candidate primal simplex multiple pricing block.\n";
                        report(lp, NORMAL, ref msg, lp.columns / lp.multiblockdiv);
                    }
                }
                else
                {
                    set_multiprice(lp, 1);
                }

                msg = "Using %s simplex for phase 1 and %s simplex for phase 2.\n";
                //ORIGINAL LINE: report(lp, NORMAL, ref msg, lp_types.my_if(primal1, "PRIMAL", "DUAL"), my_if(primal2, "PRIMAL", "DUAL"));
                //Solution: Currently parameters after ref msg are ignored, need to check at runtime
                report(lp, NORMAL, ref msg);
                i = get_piv_rule(lp);
                if ((i == PRICER_STEEPESTEDGE) && is_piv_mode(lp, PRICE_PRIMALFALLBACK))
                {
                    msg = "The pricing strategy is set to '%s' for the dual and '%s' for the primal.\n";
                    report(lp, NORMAL, ref msg, get_str_piv_rule(i), get_str_piv_rule(i - 1));
                }
                else
                {
                    msg = "The primal and dual simplex pricing strategy set to '%s'.\n";
                    report(lp, NORMAL, ref msg, get_str_piv_rule(i));
                }
                msg = " \n";
                report(lp, NORMAL, ref msg);
            }

            /* Compute a minimum step improvement step requirement */
            pre_MIPOBJ(lp);

            /* First create extra columns for FR variables or flip MI variables */
            for (j = 1; j <= lp.columns; j++)
            {

#if Paranoia
	if ((lp.rows != lp.matA.rows) || (lp.columns != lp.matA.columns))
	{
	  report(lp, SEVERE, "preprocess: Inconsistent variable counts found\n");
	}
#endif

                /* First handle sign-flipping of variables:
                    1) ... with a finite upper bound and a negative Inf-bound (since basis variables are lower-bounded)
                    2) ... with bound assymetry within negrange limits (for stability reasons) */
                i = lp.rows + j;
                hold = lp.orig_upbo[i];
                /*
                    if((hold <= 0) || (!is_infinite(lp, lp->negrange) &&
                                       (hold < -lp->negrange) &&
                                       (lp->orig_lowbo[i] <= lp->negrange)) ) {
                */

                //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
                //ORIGINAL LINE: #define fullybounded FALSE
                if (((hold < lp.infinite) && lp_types.my_infinite(lp, lp.orig_lowbo[i])) || (!false && !lp_types.my_infinite(lp, lp.negrange) && (hold < -lp.negrange) && (lp.orig_lowbo[i] <= lp.negrange)))
                {
                    /* Delete split sibling variable if one existed from before */
                    if ((lp.var_is_free != null) && (lp.var_is_free[j] > 0))
                    {
                        del_column(lp, (int)lp.var_is_free[j]);
                    }
                    /* Negate the column / flip to the positive range */
                    lp_matrix.mat_multcol(lp.matA, j, -1, true);
                    if (lp.var_is_free == null)
                    {
                        if (!lp_utils.allocINT(lp, lp.var_is_free, (int)commonlib.MAX(lp.columns, lp.columns_alloc) + 1, 1))
                        {
                            return false;
                        }
                    }
                    lp.var_is_free[j] = -j; // Indicator UB and LB are switched, with no helper variable added
                    lp.orig_upbo[i] = lp_types.my_flipsign(lp.orig_lowbo[i]);
                    lp.orig_lowbo[i] = lp_types.my_flipsign(hold);
                    /* Check for presence of negative ranged SC variable */
                    if (lp.sc_lobound[j] > 0)
                    {
                        lp.sc_lobound[j] = lp.orig_lowbo[i];
                        lp.orig_lowbo[i] = 0;
                    }
                }
                /* Then deal with -+, full-range/FREE variables by creating a helper variable */
                else if ((lp.orig_lowbo[i] <= lp.negrange) && (hold >= -lp.negrange))
                {
                    if (lp.var_is_free == null)
                    {
                        if (!lp_utils.allocINT(lp, (int)lp.var_is_free, commonlib.MAX(lp.columns, lp.columns_alloc) + 1, 1))
                        {
                            return (0);
                        }
                    }
                    if (lp.var_is_free[j] <= 0)
                    { // If this variable wasn't split yet ...
                        if (lp_SOS.SOS_is_member(lp.SOS, 0, i - lp.rows))
                        { // Added
                            msg = "preprocess: Converted negative bound for SOS variable %d to zero";
                            report(lp, IMPORTANT, ref msg, i - lp.rows);
                            lp.orig_lowbo[i] = 0;
                            continue;
                        }
                        if (new_column == null)
                        {
                            if (!lp_utils.allocREAL(lp, new_column, lp.rows + 1, 0) || !lp_utils.allocINT(lp, new_index, lp.rows + 1, 0))
                            {
                                ok = 0;
                                break;
                            }
                        }
                        /* Avoid precision loss by turning off unscaling and rescaling */
                        /* in get_column and add_column operations; also make sure that */
                        /* full scaling information is preserved */
                        scaled = lp.scaling_used;
                        lp.scaling_used = false;
                        k = get_columnex(lp, j, ref new_column, ref new_index);
                        if (!add_columnex(lp, k, ref new_column, ref new_index))
                        {
                            ok = 0;
                            break;
                        }
                        lp_matrix.mat_multcol(lp.matA, lp.columns, -1, 1);
                        if (scaled)
                        {
                            lp.scalars[lp.rows + lp.columns] = lp.scalars[i];
                        }
                        lp.scaling_used = (bool)scaled;
                        /* Only create name if we are not clearing a pre-used item, since this
                           variable could have been deleted by presolve but the name is required
                           for solution reconstruction. */
                        if (lp.names_used && (lp.col_name[j] == null))
                        {
                            string fieldn = new string(new char[50]);

                            fieldn = string.Format("__AntiBodyOf({0:D})__", j);
                            if (!set_col_name(lp, lp.columns, ref fieldn))
                            {
                                /*          if (!set_col_name(lp, lp->columns, get_col_name(lp, j))) { */
                                ok = 0;
                                break;
                            }
                        }
                        /* Set (positive) index to the original column's split / helper and back */
                        lp.var_is_free[j] = lp.columns;
                    }
                    lp.orig_upbo[lp.rows + (int)lp.var_is_free[j]] = lp_types.my_flipsign(lp.orig_lowbo[i]);
                    lp.orig_lowbo[i] = 0;

                    /* Negative index indicates x is split var and -var_is_free[x] is index of orig var */
                    lp.var_is_free[(int)lp.var_is_free[j]] = -j;
                    lp.var_type[(int)lp.var_is_free[j]] = lp.var_type[j];
                }
                /* Check for positive ranged SC variables */
                else if (lp.sc_lobound[j] > 0)
                {
                    lp.sc_lobound[j] = lp.orig_lowbo[i];
                    lp.orig_lowbo[i] = 0;
                }

                /* Tally integer variables in SOS'es */
                if (lp_SOS.SOS_is_member(lp.SOS, 0, j) && is_int(lp, j))
                {
                    lp.sos_ints++;
                }
            }

            //NOT REQUIRED
            //FREE(new_column);
            //FREE(new_index);

            /* Fill lists of GUB constraints, if appropriate */
            if ((LpCls.MIP_count(lp) > 0) && is_bb_mode(lp, NODE_GUBMODE) && (identify_GUB(lp, lp_types.AUTOMATIC) > 0))
            {
                prepare_GUB(lp);
            }

            /* (Re)allocate reduced cost arrays */
            ok = lp_utils.allocREAL(lp, (lp.drow), lp.sum + 1, lp_types.AUTOMATIC) && lp_utils.allocINT(lp, (lp.nzdrow), lp.sum + 1, lp_types.AUTOMATIC);
            if (ok)
            {
                lp.nzdrow[0][0] = 0;
            }

            /* Minimize memory usage */
            memopt_lp(lp, 0, 0, 0);

            lp.wasPreprocessed = true;

            return (Convert.ToInt32(ok));

        }

        /* Optimize memory usage */
        internal static new bool memopt_lp(lprec lp, int rowextra, int colextra, int nzextra)
        {
            bool status = false;

            if (lp == null)
            {
                return (status);
            }

            status = lp_matrix.mat_memopt(lp.matA, rowextra, colextra, nzextra) && (++rowextra > 0) && (++colextra > 0) && (++nzextra > 0);

#if false
//  if(status) {
//    int colalloc = lp->columns_alloc - MIN(lp->columns_alloc, lp->columns + colextra),
//        rowalloc = lp->rows_alloc    - MIN(lp->rows_alloc,    lp->rows + rowextra);
//
//    status = inc_lag_space(lp, rowalloc, FALSE) &&
//             inc_row_space(lp, rowalloc) &&
//             inc_col_space(lp, colalloc);
//  }
#endif

            return (status);
        }


        /* Preprocessing and postprocessing functions */
        private static int identify_GUB(lprec lp, bool mark)
        {
            int i;
            int j;
            int jb;
            int je;
            int k;
            int knint;
            int srh;
            double rh;
            double mv;
            double tv;
            double bv;
            MATrec mat = lp.matA;

            if ((lp.equalities == 0) || !lp_matrix.mat_validate(mat))
            {
                return (0);
            }

            k = 0;
            for (i = 1; i <= lp.rows; i++)
            {

                /* Check if it is an equality constraint */
                if (!is_constr_type(lp, i, EQ))
                {
                    continue;
                }

                rh = get_rh(lp, i);
                srh = (int)lp_types.my_sign(rh);
                knint = 0;
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                je = mat.row_end[i][0];
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                for (jb = mat.row_end[i - 1][0]; jb < je; jb++)
                {
                    j = lp_matrix.ROW_MAT_COLNR(jb);

                    /* Check for validity of the equation elements */
                    if (!is_int(lp, j))
                    {
                        knint++;
                    }
                    if (knint > 1)
                    {
                        break;
                    }

                    mv = get_mat_byindex(lp, jb, true, false);
                    if (System.Math.Abs(lp_types.my_reldiff(mv, rh)) > epsprimal)
                    {
                        break;
                    }

                    tv = mv * get_upbo(lp, j);
                    bv = get_lowbo(lp, j);
#if false
//      if((fabs(my_reldiff(tv, rh)) > lp->epsprimal) || (bv != 0))
#else
                    if ((srh * (tv - rh) < - epsprimal) || (bv != 0))
                    {
#endif
                        break;
                    }
                }

                /* Update GUB count and optionally mark the GUB */
                if (jb == je)
                {
                    k++;
                    if (mark)
                    {
                        lp.row_type[i] |= ROWTYPE_GUB;
                    }
                    else if (!mark) // == lp_types.AUTOMATIC
                    {
                        break;
                    }
                }

            }
            return (k);
        }

        /* This routine compares an existing basic solution to a recomputed one;
   Note that the routine must provide for the possibility that the order of the
   basis variables can be changed by the inversion engine. */
        private static int verify_solution(lprec lp, bool reinvert, ref string info)
        {
            int i;
            int ii;
            int n;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'int[] oldmap' to 'int[][] oldmap'; need to check at run time
            int[][] oldmap = null;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *newmap;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'int newmap' to 'int[][] newmap'; need to check at run time
            int[][] newmap = null;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *refmap = null;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'int? refmap' to 'int?[][] refmap'; need to check at run time
            int[][] refmap = null;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: double *oldrhs, err, errmax;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'double oldrhs' to 'double[][] oldrhs'; need to check at run time
            double[][] oldrhs = null;
            double err;
            double errmax;

            lp_utils.allocINT(lp, oldmap, lp.rows + 1, 0);
            lp_utils.allocINT(lp, newmap, lp.rows + 1, 0);
            lp_utils.allocREAL(lp, oldrhs, lp.rows + 1, 0);

            /* Get sorted mapping of the old basis */
            for (i = 0; i <= lp.rows; i++)
            {
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                oldmap[i][0] = i;
            }
            if (reinvert)
            {
                lp_utils.allocINT(lp, refmap, lp.rows + 1, 0);
                /*NOT REQUIRED
                MEMCOPY(refmap, lp.var_basic, lp.rows + 1);
                */
               commonlib.sortByINT(oldmap[0], refmap[0], lp.rows, 1, true);
            }

            /* Save old and calculate the new RHS vector */
            /*NOT REQUIRED
            MEMCOPY(oldrhs, lp.rhs, lp.rows + 1);
            */
            if (reinvert)
            {
                lp_matrix.invert(lp, INITSOL_USEZERO, false);
            }
            else
            {
                recompute_solution(lp, INITSOL_USEZERO);
            }

            /* Get sorted mapping of the new basis */
            for (i = 0; i <= lp.rows; i++)
            {
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                newmap[i][0] = i;
            }
            if (reinvert)
            {
                /*NOT REQUIRED
                MEMCOPY(refmap, lp.var_basic, lp.rows + 1);
                */
                commonlib.sortByINT(newmap[0], refmap[0], lp.rows, 1, true);
            }

            /* Identify any gap */
            errmax = 0;
            ii = -1;
            n = 0;
            for (i = lp.rows; i > 0; i--)
            {
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] (3) as 0 for now; need to check at run time
                err = System.Math.Abs(lp_types.my_reldiff(oldrhs[oldmap[i][0]][0], lp.rhs[newmap[i][0]]));
                if (err > epsprimal)
                {
                    n++;
                    if (err > errmax)
                    {
                        ii = i;
                        errmax = err;
                    }
                }
            }
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] (2) as 0 for now; need to check at run time
            err = System.Math.Abs(lp_types.my_reldiff(oldrhs[i][0], lp.rhs[i]));
            if (err < lp.epspivot)
            {
                i--;
                err = 0;
            }
            else
            {
                n++;
                if (ii < 0)
                {
                    ii = 0;
                    errmax = err;
                }
            }
            if (n > 0)
            {
                lp.report(lp, IMPORTANT, "verify_solution: Iter %.0f %s - %d errors; OF %g, Max @row %d %g\n", (double)get_total_iter(lp), my_if(info == null, "", info), n, err, newmap[ii], errmax);
            }
            /*NOT REQUIRED
            // Copy old results back (not possible for inversion) 
            if (!reinvert)
            {
                MEMCOPY(lp.rhs, oldrhs, lp.rows + 1);
            }

            FREE(oldmap);
            FREE(newmap);
            FREE(oldrhs);
            if (reinvert)
            {
                FREE(refmap);
            }
            */
            return (ii);
        }


        /* Pre- and post processing functions, i.a. splitting free variables */
        internal static bool pre_MIPOBJ(lprec lp)
        {
#if MIPboundWithOF
  if (MIP_count(lp) > 0)
  {
	int i = 1;
	while ((i <= lp.rows) && !mat_equalRows(lp.matA, 0, i) && !is_constr_type(lp, i, EQ))
	{
	  i++;
	}
	if (i <= lp.rows)
	{
	  lp.constraintOF = i;
	}
  }
#endif
            lp.bb_deltaOF = MIP_stepOF(lp);
            return (true);
        }

        internal static double MIP_stepOF(lprec lp)
        /* This function tries to find a non-zero minimum improvement
           if the OF contains all integer variables (logic only applies if we are
           looking for a single solution, not possibly several equal-valued ones). */
        {
            bool OFgcd;
            int colnr;
            int rownr;
            int n;
            int ib;
            int ie;
            int pluscount = 0;
            int intcount = 0;
            int intval = 0;
            int maxndec = 0;
            double value = 0;
            double valOF;
            double divOF = 0;
            double valGCD = 0;
            MATrec mat = lp.matA;

            if ((lp.int_vars > 0) && (lp.solutionlimit == 1) && lp_matrix.mat_validate(mat))
            {

                /* Get statistics for integer OF variables and compute base stepsize */
                n = row_intstats(lp, 0, 0, ref maxndec, ref pluscount, ref intcount, ref intval, ref valGCD, ref divOF);
                if ((n == 0) || (maxndec < 0))
                {
                    return (value);
                }
                OFgcd = (bool)(intval > 0);
                if (OFgcd)
                {
                    value = valGCD;
                }

                /* Check non-ints in the OF to see if we can get more info */
                if (n - intcount > 0)
                {
                    int nrv = n - intcount; // Number of real variables in the objective
                    int niv = 0; // Number of real variables identified as integer
                    int nrows = lp.rows;

                    /* See if we have equality constraints */
                    for (ib = 1; ib <= nrows; ib++)
                    {
                        if (is_constr_type(lp, ib, EQ))
                        {
                            break;
                        }
                    }

                    /* If so, there may be a chance to find an improved stepsize */
                    if (ib < nrows)
                    {
                        for (colnr = 1; colnr <= lp.columns; colnr++)
                        {

                            /* Go directly to the next variable if this is an integer or
                              there is no row candidate to explore for hidden bounds for
                              real-valued variables (limit scan to one row/no recursion) */
                            if ((lp.orig_obj[colnr] == 0) || is_int(lp, colnr))
                            {
                                continue;
                            }

                            /* Scan equality constraints */
                            ib = Convert.ToInt32(mat.col_end[colnr - 1]);
                            ie = Convert.ToInt32(mat.col_end[colnr]);
                            while (ib < ie)
                            {
                                if (is_constr_type(lp, (rownr = lp_matrix.COL_MAT_ROWNR(ib)), EQ))
                                {

                                    /* Get "child" row statistics, but break out if we don't
                                      find enough information, i.e. no integers with coefficients of proper type */
                                    n = row_intstats(lp, rownr, colnr, ref maxndec, ref pluscount, ref intcount, ref intval, ref valGCD, ref divOF);
                                    if ((intval < n - 1) || (maxndec < 0))
                                    {
                                        value = 0;
                                        break;
                                    }
                                    niv++;

                                    /* We can update */
                                    valOF = lp_scale.unscaled_mat(lp, lp.orig_obj[colnr], 0, colnr);
                                    valOF = System.Math.Abs(valOF * (valGCD / divOF));
                                    if (OFgcd)
                                    {
                                        commonlib.SETMIN(value, valOF);
                                    }
                                    else
                                    {
                                        OFgcd = true;
                                        value = valOF;
                                    }
                                }
                                ib++;
                            }

                            /* No point in continuing scan if we failed in current column */
                            if (value == 0)
                            {
                                break;
                            }
                        }
                    }

                    /* Check if we found information for any real-valued variable;
                       if not, then we must set the improvement delta to 0 */
                    if (nrv > niv)
                    {
                        value = 0;
                    }
                }
            }
            return (value);
        }

        public void print_str(lprec lp, ref string str)
        { throw new NotImplementedException(); }

        internal static new byte print_debugdump(lprec lp, ref string filename)
        { throw new NotImplementedException(); }

        internal static new void print_scales(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void print_duals(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void print_constraints(lprec lp, int columns)
        { throw new NotImplementedException(); }

        internal static new void print_solution(lprec lp, int columns)
        { throw new NotImplementedException(); }

        internal static new void print_objective(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void print_lp(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void print_tableau(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void unscale(lprec lp)
        { throw new NotImplementedException(); }

        internal static new lprec read_freempsex(object userhandle, read_modeldata_func read_modeldata, int options)
        { throw new NotImplementedException(); }

        internal static new lprec read_freeMPS(ref string filename, int options)
        { throw new NotImplementedException(); }

        internal static new lprec read_mps(FILE filename, int options)
        { throw new NotImplementedException(); }



        /* ---------------------------------------------------------------------------------- */
        /* Parameter setting and retrieval functions                                          */
        /* ---------------------------------------------------------------------------------- */
        internal static new void set_timeout(lprec lp, int sectimeout)
        { throw new NotImplementedException(); }

        internal static new int get_timeout(lprec lp)
        { throw new NotImplementedException(); }

        internal static new int get_verbose(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_print_sol(lprec lp, int print_sol)
        { throw new NotImplementedException(); }

        internal static new int get_print_sol(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_debug(lprec lp, byte debug)
        { throw new NotImplementedException(); }

        internal static new bool is_debug(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_trace(lprec lp, byte trace)
        { throw new NotImplementedException(); }

        internal static new bool is_trace(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_anti_degen(lprec lp, int anti_degen)
        { throw new NotImplementedException(); }

        internal static new int get_anti_degen(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_presolve(lprec lp, int presolvemode, int maxloops)
        { throw new NotImplementedException(); }

        internal static new int get_presolve(lprec lp)
        { throw new NotImplementedException(); }

        internal static new int get_presolveloops(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_maxpivot(lprec lp, int max_num_inv)
        { throw new NotImplementedException(); }

        internal static new int get_maxpivot(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_bb_rule(lprec lp, int bb_rule)
        { throw new NotImplementedException(); }

        internal static new int get_bb_rule(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_bb_depthlimit(lprec lp, int bb_maxlevel)
        { throw new NotImplementedException(); }

        internal static new int get_bb_depthlimit(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_obj_bound(lprec lp, double obj_bound)
        { throw new NotImplementedException(); }

        internal static new double get_obj_bound(lprec lp)
        { throw new NotImplementedException(); }

        internal static new void set_mip_gap(lprec lp, byte absolute, double mip_gap)
        { throw new NotImplementedException(); }

        internal static new double get_mip_gap(lprec lp, byte absolute)
        { throw new NotImplementedException(); }

        internal static new bool set_var_branch(lprec lp, int colnr, int branch_mode)
        { throw new NotImplementedException(); }

        private static int row_intstats(lprec lp, int rownr, int pivcolnr, ref int maxndec, ref int plucount, ref int intcount, ref int intval, ref double valGCD, ref double pivcolval)
        {
            int jb;
            int je;
            int jj;
            int nn = 0;
            int multA = 0;
            int multB = 0;
            int intGCD = 0;
            double rowval;
            double inthold = 0;
            double intfrac = 0;
            MATrec mat = lp.matA;
            LpCls objLpCls = new LpCls();

            /* Do we have a valid matrix? */
            if (lp_matrix.mat_validate(mat))
            {

                /* Get smallest fractional row value */
                maxndec = row_decimals(lp, rownr, Convert.ToBoolean(lp_types.AUTOMATIC), ref intfrac);

                /* Get OF row starting and ending positions, as well as the first column index */
                if (rownr == 0)
                {
                    jb = 1;
                    je = lp.columns + 1;
                }
                else
                {
                    jb = Convert.ToInt32(mat.row_end[rownr - 1]);
                    je = Convert.ToInt32(mat.row_end[rownr]);
                }
                nn = je - jb;
                pivcolval = 1.0;
                plucount = 0;
                intcount = 0;
                intval = 0;
                for (; jb < je; jb++)
                {

                    if (rownr == 0)
                    {
                        if (lp.orig_obj[jb] == 0)
                        {
                            nn--;
                            continue;
                        }
                        jj = jb;
                    }
                    else
                    {
                        jj = lp_matrix.ROW_MAT_COLNR(jb);
                    }

                    /* Pick up the value of the pivot column and continue */
                    if (jj == pivcolnr)
                    {
                        if (rownr == 0)
                        {
                            pivcolval = lp_scale.unscaled_mat(lp, lp.orig_obj[jb], 0, jb);
                        }
                        else
                        {
                            pivcolval = get_mat_byindex(lp, jb, true, false);
                        }
                        continue;
                    }
                    if (!is_int(lp, jj))
                    {
                        continue;
                    }

                    /* Update the count of integer columns */
                    intcount++;

                    /* Update the count of positive parameter values */
                    if (rownr == 0)
                    {
                        rowval = lp_scale.unscaled_mat(lp, lp.orig_obj[jb], 0, jb);
                    }
                    else
                    {
                        rowval = get_mat_byindex(lp, jb, true, false);
                    }
                    if (rowval > 0)
                    {
                        plucount++;
                    }

                    /* Check if the parameter value is integer and update the row's GCD */
                    rowval = System.Math.Abs(rowval) * intfrac;
                    rowval += rowval * lp.epsmachine;
                    rowval = lp_utils.modf(rowval, inthold);
                    if (rowval < lprec.epsprimal)
                    {
                        intval++;
                        if (intval == 1)
                        {
                            intGCD = (int)inthold;
                        }
                        else
                        {
                            intGCD = commonlib.gcd(intGCD, (int)inthold, ref multA, ref multB);
                        }
                    }
                }
                valGCD = intGCD;
                valGCD /= intfrac;
            }

            return (nn);
        }

        /* Find the smallest fractional value in a given row of the OF/constraint matrix */
        internal static int row_decimals(lprec lp, int rownr, bool intsonly, ref double intscalar)
        {
            int basi;
            int i;
            int j;
            int ncols = lp.columns;
            double f;
            double epsvalue = lprec.epsprimal;
            LpCls objLpCls = new LpCls();

            basi = 0;
            for (j = 1; j <= ncols; j++)
            {
                if (intsonly && !is_int(lp, j))
                {
                    if (intsonly == true)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                f = System.Math.Abs(get_mat(lp, rownr, j));
                /* f = fmod(f, 1); */
                f -= System.Math.Floor(f + epsvalue);
                /*
                    if(f <= epsvalue)
                      continue;
                    g = f;
                */
                for (i = 0; (i <= MAX_FRACSCALE) && (f > epsvalue); i++)
                {
                    f *= 10;
                    /* g = fmod(f, 1); */
                    f -= System.Math.Floor(f + epsvalue);
                }
                if (i > MAX_FRACSCALE)
                {
                    /* i = MAX_FRACSCALE */
                    break;
                }
                commonlib.SETMAX(basi, i);
            }
            if (j > ncols)
            {
                intscalar = System.Math.Pow(10.0, basi);
            }
            else
            {
                basi = -1;
                intscalar = 1;
            }
            return (basi);
        }

        internal static double get_mat_byindex(lprec lp, int matindex, bool isrow, bool adjustsign)
        /* Note that this function does not adjust for sign-changed GT constraints! */
        {
            //ORIGINAL LINE: int *rownr, *colnr;
            int[] rownr = null;
            //ORIGINAL LINE: int *colnr;
            int[] colnr = null;
            //ORIGINAL LINE: double *value, result;
            double[][] value = null;
            double result;

            lp_matrix.mat_get_data(lp, matindex, isrow, rownr, colnr, value);
            if (adjustsign)
            {
                result = value[0][0] * (double)(is_chsign(lp, Convert.ToInt32(rownr)) ? -1 : 1);
            }
            else
            {
                result = value[0][0];
            }
            if (lp.scaling_used)
            {
                return (lp_scale.unscaled_mat(lp, result, rownr[0], colnr[0]));
            }
            else
            {
                return (result);
            }
        }

        internal static new bool is_bb_mode(lprec lp, int bb_mask)
        {
            return ((bool)((lp.bb_rule & bb_mask) > 0));
        }

        internal new static double scaled_ceil(lprec lp, int colnr, double value, double epsscale)
        {
            LpCls objLpCls = new LpCls();
            value = System.Math.Ceiling(value);
            if (value != 0)
            {
                if (lp.columns_scaled && is_integerscaling(lp))
                {
                    value = lp_scale.scaled_value(lp, value, colnr);
                    if (epsscale != 0)
                    {
                        value -= epsscale * lp.epsmachine;
                    }
                    /*      value -= epsscale*lp->epsprimal; */
                    /*    value = restoreINT(value, lp->epsint); */
                }
            }
            return (value);
        }

        internal static new bool is_integerscaling(lprec lp)
        {
            return (is_scalemode(lp, SCALE_INTEGERS));
        }
        internal static new bool is_scalemode(lprec lp, int testmask)
        {
            return ((bool)((lp.scalemode & testmask) != 0));
        }

        internal static basisrec push_basis(lprec lp, ref int basisvar, ref bool isbasic, bool[] islower)
        /* Save the ingoing basis and push it onto the stack */
        {
            int sum = lp.sum + 1;
            basisrec newbasis = null;
            lp_bit objLp_bit = new lp_bit();

            //NOT REQUIRED
            //newbasis = (basisrec)calloc(sizeof(basisrec), 1);

            //ORIGINAL LINES: 
            /*if ((newbasis != null) &&
            {

#if LowerStorageModel == 0

                allocMYBOOL(lp, newbasis.is_lower, sum, 0) &&
#else
	allocMYBOOL(lp, newbasis.is_lower, (sum + 8) / 8, 1) &&
#endif
#if BasisStorageModel == 0
                allocMYBOOL(lp, newbasis.is_basic, sum, 0) &&
#endif
    allocINT(lp, newbasis.var_basic, lp.rows + 1, 0)) */
            //SOLUTION:
            if (newbasis != null)
            {
                if (islower == null)
                {
                    islower = lp.is_lower;
                }
                if (isbasic == null)
                {
                    isbasic = Convert.ToBoolean(lp.is_basic);
                }
                if (basisvar == null)
                {
                    basisvar = Convert.ToInt32(lp.var_basic);
                }

#if LowerStorageModel
                /*NOT REQUIRED
                MEMCOPY(newbasis.is_lower, islower, sum);
                */
#else
                for (sum = 1; sum <= lp.sum; sum++)
                {
                    if (islower[sum])
                    {
                        objLp_bit.set_biton(newbasis.is_lower, sum);
                    }
                }
#endif

#if BasisStorageModel
                /*NOT REQUIRED
                    MEMCOPY(newbasis.is_basic, isbasic, lp.sum + 1);
                    */
#endif
                //MEMCOPY(newbasis.var_basic, basisvar, lp.rows + 1);

                newbasis.previous = lp.bb_basis;
                if (lp.bb_basis == null)
                {
                    newbasis.level = 0;
                }
                else
                {
                    newbasis.level = lp.bb_basis.level + 1;
                }
                newbasis.pivots = 0;

                lp.bb_basis = newbasis;
            }
            return (newbasis);
        }

        internal new int get_var_branch(lprec lp, int colnr)
        {
            if (colnr > lp.columns || colnr < 1)
            {
                string msg = "get_var_branch: Column %d out of range\n";
                report(lp, IMPORTANT, ref msg, colnr);
                return (lp.bb_floorfirst);
            }

            if (lp.bb_varbranch == null)
            {
                return (lp.bb_floorfirst);
            }
            if (lp.bb_varbranch[colnr - 1] == BRANCH_DEFAULT)
            {
                return (lp.bb_floorfirst);
            }
            else
            {
                return (Convert.ToInt32(lp.bb_varbranch[colnr - 1]));
            }
        }

        internal static bool set_pseudocosts(lprec lp, double[] clower, double[] cupper, ref int updatelimit)
        {
            int i;

            if ((lp.bb_PseudoCost == null) || ((clower == null) && (cupper == null)))
            {
                return false;
            }
            for (i = 1; i <= lp.columns; i++)
            {
                if (clower != null)
                {
                    lp.bb_PseudoCost.LOcost[i].value = clower[i];
                }
                if (cupper != null)
                {
                    lp.bb_PseudoCost.UPcost[i].value = cupper[i];
                }
            }
            if (updatelimit != null)
            {
                lp.bb_PseudoCost.updatelimit = updatelimit;
            }
            return true;
        }


        internal static bool get_pseudocosts(lprec lp, double[] clower, double[] cupper, ref int updatelimit)
        {
            int i;

            if ((lp.bb_PseudoCost == null) || ((clower == null) && (cupper == null)))
            {
                return false;
            }
            for (i = 1; i <= lp.columns; i++)
            {
                if (clower != null)
                {
                    clower[i] = lp.bb_PseudoCost.LOcost[i].value;
                }
                if (cupper != null)
                {
                    cupper[i] = lp.bb_PseudoCost.UPcost[i].value;
                }
            }
            if (updatelimit != null)
            {
                updatelimit = lp.bb_PseudoCost.updatelimit;
            }
            return true;
        }


        internal new static double get_pseudorange(BBPSrec pc, int mipvar, int varcode)
        {
            if (varcode == BB_SC)
            {
                return (lp_scale.unscaled_value(pc.lp, pc.lp.sc_lobound[mipvar], pc.lp.rows + mipvar));
            }
            else
            {
                return (1.0);
            }
        }

        /* INLINE */
        internal static new bool is_bb_rule(lprec lp, int bb_rule)
        {
            return ((bool)((lp.bb_rule & NODE_STRATEGYMASK) == bb_rule));
        }


        internal static void update_pseudocost(BBPSrec pc, int mipvar, int varcode, bool capupper, double varsol)
        {
            double OFsol = 1;
            double uplim;
            MATitem PS;
            bool nonIntSelect = is_bb_rule(pc.lp, NODE_PSEUDONONINTSELECT);

            /* Establish input values;
               Note: The pseudocosts are normalized to the 0-1 range! */
            uplim = get_pseudorange(pc, mipvar, varcode);
            varsol = ((varsol / uplim) % OFsol);

            /* Set reference value according to pseudocost mode */
            if (nonIntSelect)
            {
                OFsol = pc.lp.bb_bounds.lastvarcus; // The count of MIP infeasibilities
            }
            else
            {
                OFsol = pc.lp.solution[0]; // The problem's objective function value
            }

            if (varsol == double.NaN)
            {
                pc.lp.bb_parentOF = OFsol;
                return;
            }

            /* Point to the applicable (lower or upper) bound and increment attempted update count */
            if (capupper)
            {
                PS = pc.LOcost[mipvar];
            }
            else
            {
                PS = pc.UPcost[mipvar];
                varsol = 1 - varsol;
            }
            PS.colnr++;

            /* Make adjustment to divisor if we are using the ratio pseudo-cost approach */
            if (is_bb_rule(pc.lp, NODE_PSEUDORATIOSELECT))
            {
                // TODO_12/10/2018
                varsol *= capupper;
            }

            /* Compute the update (consider weighting in favor of most recent) */
            mipvar = pc.updatelimit;
            if (((mipvar <= 0) || (PS.rownr < mipvar)) && (System.Math.Abs(varsol) > pc.lp.epspivot))
            {
                /* We are interested in the change in the MIP measure (contribution to increase
                   or decrease, as the case may be) and not its last value alone. */
                PS.value = PS.value * PS.rownr + (pc.lp.bb_parentOF - OFsol) / (varsol * uplim);
                PS.rownr++;
                PS.value /= PS.rownr;
                /* Check if we have enough information to restart */
                if (PS.rownr == mipvar)
                {
                    pc.updatesfinished++;
                    if (is_bb_mode(pc.lp, NODE_RESTARTMODE) && (pc.updatesfinished / (2.0 * pc.lp.int_vars) > pc.restartlimit))
                    {
                        pc.lp.bb_break = lp_types.AUTOMATIC;
                        pc.restartlimit *= 2.681; // KE: Who can figure this one out?
                        if (pc.restartlimit > 1)
                        {
                            pc.lp.bb_rule -= NODE_RESTARTMODE;
                        }
                        string msg = "update_pseudocost: Restarting with updated pseudocosts\n";
                        pc.lp.report(pc.lp, NORMAL, ref msg);
                    }
                }
            }
            pc.lp.bb_parentOF = OFsol;
        }


        internal static double get_pseudobranchcost(BBPSrec pc, int mipvar, bool dofloor)
        {
            if (dofloor)
            {
                return (pc.LOcost[mipvar].value);
            }
            else
            {
                return (pc.UPcost[mipvar].value);
            }
        }


        internal new static double get_pseudonodecost(BBPSrec pc, int mipvar, int vartype, double varsol)
        {
            double hold = 0;
            double uplim;

            uplim = get_pseudorange(pc, mipvar, vartype);
            varsol = lp_utils.modf(varsol / uplim, hold);
            if (varsol == double.NaN)
            {
                varsol = 0;
            }

            hold = pc.LOcost[mipvar].value * varsol + pc.UPcost[mipvar].value * (1 - varsol);

            return (hold * uplim);
        }

        internal new static double scaled_floor(lprec lp, int colnr, double value, double epsscale)
        {
            LpCls objLpCls = new LpCls();
            value = System.Math.Floor(value);
            if (value != 0)
            {
                if (lp.columns_scaled && is_integerscaling(lp))
                {
                    value = lp_scale.scaled_value(lp, value, colnr);
                    if (epsscale != 0)
                    {
                        value += epsscale * lp.epsmachine;
                    }
                    /*      value += epsscale*lp->epsprimal; */
                    /*    value = restoreINT(value, lp->epsint); */
                }
            }
            return (value);
        }

        internal new static void free_pseudocost(lprec lp)
        {
            if ((lp != null) && (lp.bb_PseudoCost != null))
            {
                while (free_pseudoclass((lp.bb_PseudoCost)))
                {
                    ;
                }
            }
        }
        internal static bool free_pseudoclass(BBPSrec PseudoClass)
        {
            BBPSrec target = PseudoClass;

            //NOT REQUIRED
            //FREE(target.LOcost);
            //FREE(target.UPcost);
            target = target.secondary;
            //FREE(PseudoClass);
            PseudoClass = target;

            return ((bool)(target != null));
        }

        private static int find_int_bbvar(lprec lp, ref int count, BBrec BB, ref bool isfeasible)
        {
            int i;
            int ii;
            int n;
            int k;
            int bestvar;
            int depthmax;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'int nonint' to 'int[][] nonint'; need to check at run time
            int[][] nonint = null;
            double hold;
            double holdINT=0;
            double bestval;
            double OFval;
            double randval;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: double *lowbo = BB->lowbo;
            double[] lowbo = BB.lowbo;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: double *upbo = BB->upbo;
            double[] upbo = BB.upbo;
            bool reversemode = false;
            bool greedymode = false;
            bool depthfirstmode = false;
            bool breadthfirstmode = false;
            bool randomizemode = false;
            bool? rcostmode = null;
            bool pseudocostmode = false;
            bool? pseudocostsel = null;
            bool pseudostrong = false;
            bool isINT = false;
            bool valINT = false;

            if ((lp.int_vars == 0) || (count > 0))
            {
                return (0);
            }
            if (lp.bb_usenode != null)
            {
                i = lp.bb_usenode(lp, lp.bb_nodehandle, BB_INT);
                if (i >= 0)
                {
                    if (i > 0)
                    {
                        (count)++;
                    }
                    return (i);
                }
            }

            reversemode = is_bb_mode(lp, NODE_WEIGHTREVERSEMODE);
            greedymode = is_bb_mode(lp, NODE_GREEDYMODE);
            randomizemode = is_bb_mode(lp, NODE_RANDOMIZEMODE);
            depthfirstmode = is_bb_mode(lp, NODE_DEPTHFIRSTMODE);
            breadthfirstmode = is_bb_mode(lp, NODE_BREADTHFIRSTMODE) && (bool)(lp.bb_level <= lp.int_vars);
            rcostmode = (bool)(BB.lp.solutioncount > 0) && is_bb_mode(lp, NODE_RCOSTFIXING); // 15/2/8 peno enabled NODE_RCOSTFIXING again because a fix is found. See lp_simplex.c NODE__RCOSTFIXING fix
            pseudocostmode = is_bb_mode(lp, NODE_PSEUDOCOSTMODE);
            pseudocostsel = is_bb_rule(lp, NODE_PSEUDOCOSTSELECT) || is_bb_rule(lp, NODE_PSEUDONONINTSELECT) || is_bb_rule(lp, NODE_PSEUDORATIOSELECT);
            pseudostrong = false && pseudocostsel != null && rcostmode == null && is_bb_mode(lp, NODE_STRONGINIT);

            /* Fill list of non-ints */
            lp_utils.allocINT(lp, nonint, lp.columns + 1, 0);
            n = 0;
            depthmax = -1;
            if (isfeasible != null)
            {
                isfeasible = true;
            }
            BB.lastrcf = 0;
            for (k = 1; (k <= lp.columns); k++)
            {
                ii = get_var_priority(lp, k);
                isINT = is_int(lp, ii);
                i = lp.rows + ii;

                /* Tally reduced cost fixing opportunities for ranged non-basic nonINTs */
                if (isINT == null)
                {
#if UseMilpExpandedRCF
	  if (rcostmode != null)
	  {
		bestvar = rcfbound_BB(BB, i, isINT, null, isfeasible);
		if (bestvar != FR)
		{
		  BB.lastrcf++;
		}
	  }
#endif
                }
                else
                {

                    valINT = solution_is_int(lp, i, false);

                    /* Skip already fixed variables */
                    if (lowbo[i] == upbo[i])
                    {

                        /* Check for validity */
#if Paranoia
		if (valINT == null)
		{
		  report(lp, IMPORTANT, "find_int_bbvar: INT var %d was fixed at %d, but computed as %g at node %.0f\n", ii, (int) lowbo[i], lp.solution[i], (double) lp.bb_totalnodes);
		  lp.bb_break = 1;
		  lp.spx_status = UNKNOWNERROR;
		  bestvar = 0;
		  goto Done;
		}
#endif
                    }

                    /* The variable has not yet been fixed */
                    else
                    {

                        /* Tally reduced cost fixing opportunities (also when the
                           variables are integer-valued at the current relaxation) */
                        if (rcostmode != null)
                        {
                            bestvar = lp_mipbb.rcfbound_BB(BB, i, isINT, 0, ref isfeasible);
                            if (bestvar != FR)
                            {
                                BB.lastrcf++;
                            }
                        }
                        else
                        {
                            bestvar = FR;
                        }

                        /* Only qualify variable as branching node if it is non-integer and
                           it will not be subsequently fixed via reduced cost fixing logic */
                        if (valINT == null && (bestvar >= FR))
                        {

                            n++;
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                            //set second [] as 0 for now; need to check at run time
                            nonint[n][0] = ii;
                            commonlib.SETMAX(depthmax, lp.bb_varactive[ii]);
                        }
                    }

                }
            }
#if UseMilpSlacksRCF
  /* Optionally also tally slacks */
  if (rcostmode)
  {
	for (i = 1; (i <= lp.rows) && (BB.lastrcf == 0); i++)
	{
	  /* Skip already fixed slacks (equalities) */
	  if (lowbo[i] < upbo[i])
	  {
		bestvar = rcfbound_BB(BB, i, 0, null, isfeasible);
		if (bestvar != FR)
		{
		  BB.lastrcf++;
		}
	  }
	}
  }
#endif
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //set second [] as 0 for now; need to check at run time
            nonint[0][0] = n;
            count = n;
            bestvar = 0;
            if (n == 0) // No non-integers found
            {
                goto Done;
            }

            bestval = -lp.infinite;
            hold = 0;
            randval = 1;

            /* Sort non-ints by depth in case we have breadthfirst or depthfirst modes */
            if ((lp.bb_level > 1) && (depthmax > 0) && (depthfirstmode || breadthfirstmode))
            {
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //changed from 'int[] depths' to 'int[][] depths'; need to check at run time
                int[][] depths = null;

                /* Fill attribute array and make sure ordinal order breaks ties during sort */
                lp_utils.allocINT(lp, depths, n + 1, 0);
                for (i = 1; i <= n; i++)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 (2) for now; need to check at run time
                    depths[i][0] = (depthfirstmode ? n + 1 - i : i) + (n + 1) * lp.bb_varactive[nonint[i][0]];
                }

               commonlib.hpsortex(depths, n, 1, nonint.Length, depthfirstmode, commonlib.compareINT, ref nonint[0]);
                /*NOT REQUIRED
                FREE(depths);
                */
            }
            /* Do simple firstselect handling */
            if (is_bb_rule(lp, NODE_FIRSTSELECT))
            {
                if (reversemode)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0  (2) for now; need to check at run time
                    bestvar = lp.rows + nonint[nonint[0][0]][0];
                }
                else
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    bestvar = lp.rows + nonint[1][0];
                }
            }

            else
            {
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                for (n = 1; n <= nonint[0][0]; n++)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    ii = nonint[n][0];
                    i = lp.rows + ii;

                    /* Do the naive detection */
                    if (n == 1)
                    {
                        bestvar = i;
                    }

                    /* Should we do a "strong" pseudo-cost initialization or an incremental update? */
                    if (pseudostrong && (commonlib.MAX(lp.bb_PseudoCost.LOcost[ii].rownr, lp.bb_PseudoCost.UPcost[ii].rownr) < lp.bb_PseudoCost.updatelimit) && (MAX(lp.bb_PseudoCost.LOcost[ii].colnr, lp.bb_PseudoCost.UPcost[ii].colnr) < 5 * lp.bb_PseudoCost.updatelimit))
                    {
                        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                        //set second [] as 0 for now; need to check at run time
                        lp_mipbb. strongbranch_BB(lp, BB, ii, BB_INT, nonint[0][0]);
                    }

                    /* Select default pricing/weighting mode */
                    if (pseudocostmode)
                    {
                        OFval = get_pseudonodecost(lp.bb_PseudoCost, ii, BB_INT, lp.solution[i]);
                    }
                    else
                    {
                        OFval = lp_types.my_chsign(is_maxim(lp), get_mat(lp, 0, ii));
                    }

                    if (randomizemode)
                    {
                        randval = System.Math.Exp(lp_utils.rand_uniform(lp, 1.0));
                    }

                    /* Find the maximum pseudo-cost of a variable (don't apply pseudocostmode here) */
                    if ((pseudocostsel != null) ? pseudocostmode : false)
                    {
                        if (pseudocostmode)
                        {
                            hold = OFval;
                        }
                        else
                        {
                            hold = get_pseudonodecost(lp.bb_PseudoCost, ii, BB_INT, lp.solution[i]);
                        }
                        hold *= randval;
                        if (greedymode)
                        {
                            if (pseudocostmode) // Override!
                            {
                                OFval = lp_types.my_chsign(is_maxim(lp), get_mat(lp, 0, ii));
                            }
                            hold *= OFval;
                        }
                        hold = lp_types.my_chsign(reversemode, hold);
                    }
                    else
                    {
                        /* Find the variable with the largest gap to its bounds (distance from being fixed) */
                        if (is_bb_rule(lp, NODE_GAPSELECT))
                        {
                            hold = lp.solution[i];
                            holdINT = hold - lp_scale.unscaled_value(lp, upbo[i], i);
                            hold -= lp_scale.unscaled_value(lp, lowbo[i], i);
                            if (System.Math.Abs(holdINT) > hold)
                            {
                                hold = holdINT;
                            }
                            if (greedymode)
                            {
                                hold *= OFval;
                            }
                            hold = lp_types.my_chsign(reversemode, hold) * randval;
                        }
                        else
                        {
                            /* Find the variable with the largest integer gap (closest to 0.5) */
                            if (is_bb_rule(lp, NODE_FRACTIONSELECT))
                            {
                                hold = lp.solution[i] % holdINT;
                                holdINT = hold - 1;
                                if (System.Math.Abs(holdINT) > hold)
                                {
                                    hold = holdINT;
                                }
                                if (greedymode)
                                {
                                    hold *= OFval;
                                }
                                hold = lp_types.my_chsign(reversemode, hold) * randval;
                            }
                            else
                            {
                                /* Find the "range", most flexible variable */
                                if (is_bb_rule(lp, NODE_RANGESELECT))
                                {
                                    hold = lp_scale.unscaled_value(lp, upbo[i] - lowbo[i], i);
                                    if (greedymode)
                                    {
                                        hold *= OFval;
                                    }
                                    hold = lp_types.my_chsign(reversemode, hold) * randval;
                                }
                            }
                        }
                    }

                    /* Select better, check for ties, and split by proximity to 0.5 */
                    if (hold > bestval)
                    {
                        if ((hold > bestval + epsprimal) || (System.Math.Abs(lp.solution[i] % holdINT) - 0.5) < System.Math.Abs(lp.solution[bestvar] % holdINT) - 0.5)
                        {
                            bestval = hold;
                            bestvar = i;
                        }
                    }
                }
            }

            Done:
            /*NOT REQUIRED
            FREE(nonint);
            */
            return (bestvar);
        }

        internal static int find_sos_bbvar(lprec lp, ref int count, bool intsos)
        {
            int k;
            int i;
            int j;
            int @var;

            @var = 0;
            if ((lp.SOS == null) || (count > 0))
            {
                return (@var);
            }

            /* Check if the SOS'es happen to already be satisified */
            i = lp_SOS.SOS_is_satisfied(lp.SOS, 0, ref lp.solution);
            if ((i == lp_SOS.SOS_COMPLETE) || (i == lp_SOS.SOS_INCOMPLETE))
            {
                return (-1);
            }

            /* Otherwise identify a SOS variable to enter B&B */
            for (k = 0; k < lp.sos_vars; k++)
            {
                i = (lp.sos_priority[k] != null) ? Convert.ToInt32(lp.sos_priority[k]) : 0;
#if Paranoia
	if ((i < 1) || (i > lp.columns))
	{
	  report(lp, SEVERE, "find_sos_bbvar: Invalid SOS variable map %d at %d\n", i, k);
	}
#endif
                j = lp.rows + i;
                if (!lp_SOS.SOS_is_marked(lp.SOS, 0, i) && !lp_SOS.SOS_is_full(lp.SOS, 0, i, 0))
                {
                    /*    if(!SOS_is_marked(lp->SOS, 0, i) && !SOS_is_full(lp->SOS, 0, i, TRUE)) { */
                    if (!intsos || is_int(lp, i))
                    {
                        count++;
                        if (@var == 0)
                        {
                            @var = j;
                            break;
                        }
                    }
                }
            }
#if Paranoia
  if ((@var > 0) && !SOS_is_member(lp.SOS, 0, @var - lp.rows))
  {
	 report(lp, SEVERE, "find_sos_bbvar: Found variable %d, which is not a SOS!\n", @var);
  }
#endif
            return (@var);
        }

        internal static new int find_sc_bbvar(lprec lp, ref int count)
        {
            int i;
            int ii;
            int n;
            int bestvar;
            int firstsc;
            int lastsc;
            double hold;
            double holdINT = 1;
            double bestval;
            double OFval;
            double randval;
            double scval;
            bool reversemode;
            bool greedymode;
            bool randomizemode;
            bool pseudocostmode;
            bool pseudocostsel;
            bestvar = 0;
            if ((lp.sc_vars == 0) || (count > 0))
            {
                return (bestvar);
            }

            reversemode = is_bb_mode(lp, NODE_WEIGHTREVERSEMODE);
            greedymode = is_bb_mode(lp, NODE_GREEDYMODE);
            randomizemode = is_bb_mode(lp, NODE_RANDOMIZEMODE);
            pseudocostmode = is_bb_mode(lp, NODE_PSEUDOCOSTMODE);
            pseudocostsel = is_bb_rule(lp, NODE_PSEUDOCOSTSELECT) || is_bb_rule(lp, NODE_PSEUDONONINTSELECT) || is_bb_rule(lp, NODE_PSEUDORATIOSELECT);

            bestvar = 0;
            bestval = -lp.infinite;
            hold = 0;
            randval = 1;
            firstsc = 0;
            lastsc = lp.columns;

            for (n = 1; n <= lp.columns; n++)
            {
                ii = get_var_priority(lp, n);
                i = lp.rows + ii;
                if (lp.bb_varactive[ii] != 0 && is_sc_violated(lp, ii) && ! lp_SOS.SOS_is_marked(lp.SOS, 0, ii))
                {

                    /* Do tallies */
                    count++;
                    lastsc = i;
                    if (firstsc <= 0)
                    {
                        firstsc = i;
                    }
                    scval = get_pseudorange(lp.bb_PseudoCost, ii, BB_SC);

                    /* Select default pricing/weighting mode */
                    if (pseudocostmode)
                    {
                        OFval = get_pseudonodecost(lp.bb_PseudoCost, ii, BB_SC, lp.solution[i]);
                    }
                    else
                    {
                        OFval = lp_types.my_chsign(is_maxim(lp), get_mat(lp, 0, ii));
                    }

                    if (randomizemode)
                    {
                        randval = System.Math.Exp(lp_utils.rand_uniform(lp, 1.0));
                    }

                    /* Find the maximum pseudo-cost of a variable (don't apply pseudocostmode here) */
                    if (pseudocostsel)
                    {
                        if (pseudocostmode)
                        {
                            hold = OFval;
                        }
                        else
                        {
                            hold = get_pseudonodecost(lp.bb_PseudoCost, ii, BB_SC, lp.solution[i]);
                        }
                        hold *= randval;
                        if (greedymode)
                        {
                            if (pseudocostmode) // Override!
                            {
                                OFval = lp_types.my_chsign(is_maxim(lp), get_mat(lp, 0, ii));
                            }
                            hold *= OFval;
                        }
                        hold = lp_types.my_chsign(reversemode, hold);
                    }
                    else
                    {
                        /* Find the variable with the largest sc gap (closest to the sc mean) */
                        if (is_bb_rule(lp, NODE_FRACTIONSELECT))
                        {
                            hold = (lp.solution[i] / scval) % holdINT;
                            holdINT = hold - 1;
                            if (System.Math.Abs(holdINT) > hold)
                            {
                                hold = holdINT;
                            }
                            if (greedymode)
                            {
                                hold *= OFval;
                            }
                            hold = lp_types.my_chsign(reversemode, hold) * scval * randval;
                        }
                        else
                        /* Do first or last violated sc index selection (default) */
                        /* if(is_bb_rule(lp, NODE_FIRSTSELECT)) */
                        {
                            if (reversemode)
                            {
                                continue;
                            }
                            else
                            {
                                bestvar = i;
                                break;
                            }
                        }
                    }

                    /* Select better, check for ties, and split by proximity to 0.5*sc_lobound */
                    if (hold > bestval)
                    {
                        if ((bestvar == 0) || (hold > bestval + epsprimal) || (System.Math.Abs((lp.solution[i] / scval) % holdINT) - 0.5) < System.Math.Abs((lp.solution[bestvar] / get_pseudorange(lp.bb_PseudoCost, bestvar - lp.rows, BB_SC)) % holdINT) - 0.5)
                        {
                            bestval = hold;
                            bestvar = i;
                        }
                    }
                }
            }

            if (is_bb_rule(lp, NODE_FIRSTSELECT) && reversemode)
                bestvar = lastsc;
            return (bestvar);
        }

        internal static bool is_sc_violated(lprec lp, int column)
        {
            int varno;
            double tmpreal;

            varno = lp.rows + column;
            tmpreal = lp_scale.unscaled_value(lp, lp.sc_lobound[column], varno);
            return ((bool)((tmpreal > 0) && (lp.solution[varno] < tmpreal) && (lp.solution[varno] > 0))); // ...and the Z lowerbound is violated
        }


        internal static new int get_var_priority(lprec lp, int colnr)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                string msg = "get_var_priority: Column {0} out of range\n";
                lp.report(lp, IMPORTANT, ref msg, colnr);
                return (0);
            }

            if (lp.var_priority == null)
            {
                return (colnr);
            }
            else
            {
                return ((lp.var_priority[colnr - 1] != null) ? Convert.ToInt32(lp.var_priority[colnr - 1]) : 0);
            }

        }



        internal new static BBPSrec init_pseudocost(lprec lp, int pseudotype)
        {
            int i;
            double PSinitUP;
            double PSinitLO;
            BBPSrec newitem;
            bool isPSCount;

            /* Allocate memory */
            newitem = new BBPSrec();
            newitem.lp = lp;
            /*NOT REQUIRED
            newitem.LOcost = (MATitem)malloc((lp.columns + 1) * sizeof(*newitem.LOcost));
            newitem.UPcost = (MATitem)malloc((lp.columns + 1) * sizeof(*newitem.UPcost));
            */
            newitem.secondary = null;

            /* Initialize with OF values */
            newitem.pseodotype = (pseudotype & NODE_STRATEGYMASK);
            isPSCount = ((pseudotype & NODE_PSEUDONONINTSELECT) != 0);
            for (i = 1; i <= lp.columns; i++)
            {
                newitem.LOcost[i].rownr = 1; // Actual updates
                newitem.LOcost[i].colnr = 1; // Attempted updates
                newitem.UPcost[i].rownr = 1;
                newitem.UPcost[i].colnr = 1;

                /* Initialize with the plain OF value as conventional usage suggests, or
                   override in case of pseudo-nonint count strategy */
                PSinitUP = lp_types.my_chsign(is_maxim(lp), get_mat(lp, 0, i));
                PSinitLO = -PSinitUP;
                if (isPSCount)
                {
                    /* Set default assumed reduction in the number of non-ints by choosing this variable;
                       KE changed from 0 on 30 June 2004 and made two-sided selectable.  Note that the
                       typical value range is <0..1>, with a positive bias for an "a priori" assumed
                       fast-converging (low "MIP-complexity") model. Very hard models may require
                       negative initialized values for one or both. */
                    PSinitUP = 0.1 * 0;
#if false
//      PSinitUP = my_chsign(PSinitUP < 0, PSinitUP);
//      PSinitLO = -PSinitUP;
#else
                    PSinitLO = PSinitUP;
#endif
                }
                newitem.UPcost[i].value = PSinitUP;
                newitem.LOcost[i].value = PSinitLO;
            }
            newitem.updatelimit = lp.bb_PseudoUpdates;
            newitem.updatesfinished = 0;
            newitem.restartlimit = DEF_PSEUDOCOSTRESTART;

            /* Let the user get an opportunity to initialize pseudocosts */
            if (userabort(lp, MSG_INITPSEUDOCOST))
            {
                lp.spx_status = USERABORT;
            }

            return (newitem);
        }


        internal static bool pop_basis(lprec lp, bool restore)
        /* Pop / free, and optionally restore the previously "pushed" / saved basis */
        {
            bool ok;
            basisrec oldbasis;

            ok = (bool)(lp.bb_basis != null);
            if (ok)
            {
                oldbasis = lp.bb_basis;
                if (oldbasis != null)
                {
                    lp.bb_basis = oldbasis.previous;
                    //FREE(oldbasis.var_basic);

#if BasisStorageModel
                    FREE(oldbasis.is_basic);
#endif
                    //FREE(oldbasis.is_lower);
                    //FREE(oldbasis);
                }
                if (restore && (lp.bb_basis != null))
                {
                    restore_basis(lp);
                }
            }
            return (ok);
        }

        internal new void postprocess(lprec lp)
        {
            int i;
            int ii;
            int j;
            double hold;
            string msg;

            /* Check if the problem actually was preprocessed */
            if (!lp.wasPreprocessed)
            {
                return;
            }

            /* Must compute duals here in case we have free variables; note that in
               this case sensitivity analysis is not possible unless done here */
            if ((lp.bb_totalnodes == 0) && (lp.var_is_free == null))
            {
                if (is_presolve(lp, PRESOLVE_DUALS))
                {
                    construct_duals(lp);
                }
                if (is_presolve(lp, PRESOLVE_SENSDUALS))
                {
                    if (!construct_sensitivity_duals(lp) || !construct_sensitivity_obj(lp))
                    {
                        msg = "postprocess: Unable to allocate working memory for duals.\n";
                        report(lp, IMPORTANT, ref msg);
                    }
                }
            }

            /* Loop over all columns */
            for (j = 1; j <= lp.columns; j++)
            {
                i = lp.rows + j;
                /* Reconstruct strictly negative values */
                if ((lp.var_is_free != null) && (lp.var_is_free[j] < 0))
                {
                    /* Check if we have the simple case where the UP and LB are negated and switched */
                    if (-lp.var_is_free[j] == j)
                    {
                        lp_matrix.mat_multcol(lp.matA, j, -1, 1);
                        hold = lp.orig_upbo[i];
                        lp.orig_upbo[i] = lp_types.my_flipsign(lp.orig_lowbo[i]);
                        lp.orig_lowbo[i] = lp_types.my_flipsign(hold);
                        lp.best_solution[i] = lp_types.my_flipsign(Convert.ToDouble(lp.best_solution[i]));
                        transfer_solution_var(lp, j);

                        /* hold = lp->objfrom[j];
                        lp->objfrom[j] = my_flipsign(lp->objtill[j]);
                        lp->objtill[j] = my_flipsign(hold); */
                        /* under investigation <peno> */

                        /* lp->duals[i] = my_flipsign(lp->duals[i]);
                        hold = lp->dualsfrom[i];
                        lp->dualsfrom[i] = my_flipsign(lp->dualstill[i]);
                        lp->dualstill[i] = my_flipsign(hold); */
                        /* under investigation <peno> */
                        /* Bound switch undone, so clear the status */
                        lp.var_is_free[j] = 0;
                        /* Adjust negative ranged SC */
                        if (lp.sc_lobound[j] > 0)
                        {
                            lp.orig_lowbo[lp.rows + j] = -lp.sc_lobound[j];
                        }
                    }
                    /* Ignore the split / helper columns (will be deleted later) */
                }
                /* Condense values of extra columns of quasi-free variables split in two */
                else if ((lp.var_is_free != null) && (lp.var_is_free[j] > 0))
                {
                    ii = Convert.ToInt32(lp.var_is_free[j]); // Index of the split helper var
                                                             /* if(lp->objfrom[j] == -lp->infinite)
                                                               lp->objfrom[j] = -lp->objtill[ii];
                                                             lp->objtill[ii] = lp->infinite;
                                                             if(lp->objtill[j] == lp->infinite)
                                                               lp->objtill[j] = my_flipsign(lp->objfrom[ii]);
                                                             lp->objfrom[ii] = -lp->infinite; */
                                                             /* under investigation <peno> */

                    ii += lp.rows;
                    lp.best_solution[i] -= lp.best_solution[ii]; // join the solution again
                    transfer_solution_var(lp, j);
                    lp.best_solution[ii] = 0;

                    /* if(lp->duals[i] == 0)
                      lp->duals[i] = my_flipsign(lp->duals[ii]);
                    lp->duals[ii] = 0;
                    if(lp->dualsfrom[i] == -lp->infinite)
                      lp->dualsfrom[i] = my_flipsign(lp->dualstill[ii]);
                    lp->dualstill[ii] = lp->infinite;
                    if(lp->dualstill[i] == lp->infinite)
                      lp->dualstill[i] = my_flipsign(lp->dualsfrom[ii]);
                    lp->dualsfrom[ii] = -lp->infinite; */
                    /* under investigation <peno> */

                    /* Reset to original bound */
                    lp.orig_lowbo[i] = lp_types.my_flipsign(lp.orig_upbo[ii]);
                }
                /* Adjust for semi-continuous variables */
                else if (lp.sc_lobound[j] > 0)
                {
                    lp.orig_lowbo[i] = lp.sc_lobound[j];
                }
            }

            /* Remove any split column helper variables */
            del_splitvars(lp);
            post_MIPOBJ(lp);

            /* Do extended reporting, if specified */
            if (lp.verbose > NORMAL)
            {
                lp_report objlp_report = new lp_report();
                objlp_report.REPORT_extended(lp);

            }

            lp.wasPreprocessed = false;
        }

        internal new static void transfer_solution_var(lprec lp, int uservar)
        {
            if (lp.varmap_locked && (bool)((lp.do_presolve & PRESOLVE_LASTMASKMODE) != PRESOLVE_NONE))
            {
                uservar += lp.rows;
                lp.full_solution[lp.presolve_undo.orig_rows + lp.presolve_undo.var_to_orig[uservar]] = (lp.best_solution[uservar] != null) ? Convert.ToDouble(lp.best_solution[uservar]): 0;
            }
        }


        /* Calculate sensitivity duals */
        internal new static bool construct_sensitivity_duals(lprec lp)
        {
            int k;
            int varnr;
            bool ok = true;
            //ORIGINAL LINE: int *workINT = null;
            int[] workINT = null;
            //ORIGINAL LINE: double *pcol,a,infinite,epsvalue,from,till,objfromvalue;
            double[] pcol;
            double a;
            double infinite;
            double epsvalue;
            double from;
            double till;
            double objfromvalue;
            LpCls objLpCls = new LpCls();

            /* one column of the matrix */
            //NOT REQUIRED
            //FREE(lp.objfromvalue);
            //FREE(lp.dualsfrom);
            //FREE(lp.dualstill);
            //BELOW CODE IS NOT REQUIRED
            if (!lp_utils.allocREAL(lp, pcol, lp.rows + 1, 1) || !lp_utils.allocREAL(lp, lp.objfromvalue, lp.columns + 1, lp_types.AUTOMATIC) || !lp_utils.allocREAL(lp, lp.dualsfrom, lp.sum + 1, lp_types.AUTOMATIC) || !lp_utils.allocREAL(lp, lp.dualstill, lp.sum + 1, lp_types.AUTOMATIC))
            {
                // FREE(pcol);
                //FREE(lp.objfromvalue);
                // FREE(lp.dualsfrom);
                // FREE(lp.dualstill);
                ok = 0;
            }
            else
            {
                infinite = lp.infinite;
                epsvalue = lp.epsmachine;
                for (varnr = 1; varnr <= lp.sum; varnr++)
                {
                    from = infinite;
                    till = infinite;
                    objfromvalue = infinite;
                    if (!lp.is_basic[varnr])
                    {
                        if (!lp_matrix.fsolve(lp, varnr, pcol, workINT, epsvalue, 1.0, 0))
                        { // construct one column of the tableau
                            ok = 0;
                            break;
                        }
                        /* Search for the rows(s) which first result in further iterations */
                        for (k = 1; k <= lp.rows; k++)
                        {
                            if (System.Math.Abs(pcol[k]) > epsvalue)
                            {
                                a = lp.rhs[k] / pcol[k];
                                if ((varnr > lp.rows) && (System.Math.Abs(lp.solution[varnr]) <= epsvalue) && (a < objfromvalue) && (a >= lp.lowbo[varnr]))
                                {
                                    objfromvalue = a;
                                }
                                if ((a <= 0.0) && (pcol[k] < 0.0) && (-a < from))
                                {
                                    from = lp_types.my_flipsign(a);
                                }
                                if ((a >= 0.0) && (pcol[k] > 0.0) && (a < till))
                                {
                                    till = a;
                                }
                                if (lp.upbo[lp.var_basic[k]] < infinite)
                                {
                                    a = (double)((lp.rhs[k] - lp.upbo[lp.var_basic[k]]) / pcol[k]);
                                    if ((varnr > lp.rows) && (System.Math.Abs(lp.solution[varnr]) <= epsvalue) && (a < objfromvalue) && (a >= lp.lowbo[varnr]))
                                    {
                                        objfromvalue = a;
                                    }
                                    if ((a <= 0.0) && (pcol[k] > 0.0) && (-a < from))
                                    {
                                        from = lp_types.my_flipsign(a);
                                    }
                                    if ((a >= 0.0) && (pcol[k] < 0.0) && (a < till))
                                    {
                                        till = a;
                                    }
                                }
                            }
                        }

                        if (!lp.is_lower[varnr])
                        {
                            a = from;
                            from = till;
                            till = a;
                        }
                        if ((varnr <= lp.rows) && (!objLpCls.is_chsign(lp, varnr)))
                        {
                            a = from;
                            from = till;
                            till = a;
                        }
                    }

                    if (from != infinite)
                    {
                        lp.dualsfrom[varnr] = lp.solution[varnr] - lp_scale.unscaled_value(lp, from, varnr);
                    }
                    else
                    {
                        lp.dualsfrom[varnr] = -infinite;
                    }
                    if (till != infinite)
                    {
                        lp.dualstill[varnr] = lp.solution[varnr] + lp_scale.unscaled_value(lp, till, varnr);
                    }
                    else
                    {
                        lp.dualstill[varnr] = infinite;
                    }

                    if (varnr > lp.rows)
                    {
                        if (objfromvalue != infinite)
                        {
                            if ((!lp_lib.sensrejvar) || (lp.upbo[varnr] != 0.0))
                            {
                                if (!lp.is_lower[varnr])
                                {
                                    objfromvalue = lp.upbo[varnr] - objfromvalue;
                                }
                                if ((lp.upbo[varnr] < infinite) && (objfromvalue > lp.upbo[varnr]))
                                {
                                    objfromvalue = lp.upbo[varnr];
                                }
                            }
                            objfromvalue += lp.lowbo[varnr];
                            objfromvalue = lp_scale.unscaled_value(lp, objfromvalue, varnr);
                        }
                        else
                        {
                            objfromvalue = -infinite;
                        }
                        lp.objfromvalue[varnr - lp.rows] = objfromvalue;
                    }

                }
                //NOT REQUIRED
                //FREE(pcol);
            }
            return (ok);
        } // construct_sensitivity_duals

        /* Calculate sensitivity objective function */
        internal new static bool construct_sensitivity_obj(lprec lp)
        {
            int i;
            int l;
            int varnr;
            int row_nr;
            bool ok = true;
            int coltarget = 0;
            int? Parameter = null;
            int Parameter2 = 0;
            string memVector = "";

            //ORIGINAL LINE: double *OrigObj = null, *drow = null, *prow = null, sign, a, min1, min2, infinite, epsvalue, from, till;
            double[] OrigObj;

            //ORIGINAL LINE: double *drow = null;
            double[] drow;

            //ORIGINAL LINE: double *prow = null;
            double[] prow = null;
            double sign;
            double a;
            double min1;
            double min2;
            double infinite = 0;
            double epsvalue = 0;
            double from;
            double till;
            LpCls objLpCls = new LpCls();

            /* objective function */
            // FREE(lp.objfrom);
            // FREE(lp.objtill);
            if (!lp_utils.allocREAL(lp, drow, lp.sum + 1, 1) || !lp_utils.allocREAL(lp, OrigObj, lp.columns + 1, 0) || !lp_utils.allocREAL(lp, prow, lp.sum + 1, 1) || !lp_utils.allocREAL(lp, lp.objfrom, lp.columns + 1, lp_types.AUTOMATIC) || !lp_utils.allocREAL(lp, lp.objtill, lp.columns + 1, lp_types.AUTOMATIC))
            {
                Abandon:
                //FREE(drow);
                //FREE(OrigObj);
                //FREE(prow);
                //FREE(lp.objfrom);
                //FREE(lp.objtill);
                ok = false;
            }
            else
            {
                //ORIGINAL LINE: int *coltarget;



                infinite = lp.infinite;
                epsvalue = lp.epsmachine;

                /// <summary> FIX_133d8fd1-d4bf-4e73-ac14-1bb037ba574f 29/11/18
                /// PREVIOUS: coltarget = coltarget = (int)mempool_obtainVector(lp.workarrays, lp.columns + 1, sizeof(int));
                /// ERROR IN PREVIOUS: Cannot convert type 'string' to 'int'
                /// FIX 1: 
                /// </summary>
                int id = 0;
                bool res = int.TryParse(lp_utils.mempool_obtainVector(lp.workarrays, lp.columns + 1, sizeof(int)), out id);
                if (res)
                {
                    if (!lp_matrix.get_colIndexA(lp, SCAN_USERVARS + USE_NONBASICVARS, coltarget, false))
                    {
                        memVector = coltarget.ToString();
                        lp_utils.mempool_releaseVector(lp.workarrays, ref memVector, 0);
                        //NOTED ISSUE
                        goto Abandon;
                    }
                }
                else
                    throw new Exception("lp_utils.mempool_obtainVector");



                memVector = coltarget.ToString();
                lp_matrix.bsolve(lp, 0, ref drow, ref Parameter, epsvalue * DOUBLEROUND, 1.0);
                lp_matrix.prod_xA(lp, ref coltarget, ref drow[0], ref Parameter2, epsvalue, 1.0, ref drow[0], ref Parameter2, lp_matrix.MAT_ROUNDDEFAULT | lp_matrix.MAT_ROUNDRC);
            }

            /* original (unscaled) objective function */
            objLpCls.get_row(lp, 0, ref OrigObj);
            for (i = 1; i <= lp.columns; i++)
            {
                from = -infinite;
                till = infinite;
                varnr = lp.rows + i;
                if (!lp.is_basic[varnr])
                {
                    /* only the coeff of the objective function of column i changes. */
                    a = lp_scale.unscaled_mat(lp, drow[varnr], 0, i);
                    if (is_maxim(lp))
                    {
                        a = -a;
                    }
                    if ((!sensrejvar) && (lp.upbo[varnr] == 0.0))
                    {
                        /* ignore, because this case doesn't results in further iterations */
                        ;
                    }
                    else if (((lp.is_lower[varnr] != false) == (!LpCls.is_maxim(lp))) && (a > -epsvalue))
                    {
                        from = OrigObj[i] - a; // less than this value gives further iterations
                    }
                    else
                    {
                        till = OrigObj[i] - a; // bigger than this value gives further iterations
                    }
                }
                else
                {
                    /* all the coeff of the objective function change. Search the minimal change needed for further iterations */
                    for (row_nr = 1; (row_nr <= lp.rows) && (lp.var_basic[row_nr] != varnr); row_nr++)
                    {
                        /* Search on which row the variable exists in the basis */
                        ;
                    }
                    if (row_nr <= lp.rows)
                    { // safety test; should always be found ...
                      /* Construct one row of the tableau */
                        lp_matrix.bsolve(lp, row_nr, ref prow, ref Parameter, epsvalue * DOUBLEROUND, 1.0);
                        lp_matrix.prod_xA(lp, ref coltarget, ref prow[0], ref Parameter2, epsvalue, 1.0, ref prow[0], ref Parameter2, lp_matrix.MAT_ROUNDDEFAULT);
                        /* sign = my_chsign(is_chsign(lp, row_nr), -1); */
                        sign = lp_types.my_chsign(lp.is_lower[row_nr], -1);
                        min1 = infinite;
                        min2 = infinite;
                        for (l = 1; l <= lp.sum; l++) // search for the column(s) which first results in further iterations
                        {
                            if ((!lp.is_basic[l]) && (lp.upbo[l] > 0.0) && (System.Math.Abs(prow[l]) > epsvalue) && (drow[l] * (lp.is_lower[l] ? -1 : 1) < epsvalue))
                            {
                                a = lp_scale.unscaled_mat(lp, System.Math.Abs(drow[l] / prow[l]), 0, i);
                                if (prow[l] * sign * (lp.is_lower[l] ? 1 : -1) < 0.0 != false != false)
                                {
                                    if (a < min1)
                                    {
                                        min1 = a;
                                    }
                                }
                                else
                                {
                                    if (a < min2)
                                    {
                                        min2 = a;
                                    }
                                }
                            }
                        }
                        if ((lp.is_lower[varnr] = false) == (!LpCls.is_maxim(lp)))
                        {
                            a = min1;
                            min1 = min2;
                            min2 = a;
                        }
                        if (min1 < infinite)
                        {
                            from = OrigObj[i] - min1;
                        }
                        if (min2 < infinite)
                        {
                            till = OrigObj[i] + min2;
                        }
                        a = lp.solution[varnr];
                        if (is_maxim(lp))
                        {
                            if (a - lp.lowbo[varnr] < epsvalue)
                            {
                                from = -infinite; // if variable is at lower bound then decrementing objective coefficient will not result in extra iterations because it would only extra decrease the value, but since it is at its lower bound ...
                            }
                            else if (((!sensrejvar) || (lp.upbo[varnr] != 0.0)) && (lp.lowbo[varnr] + lp.upbo[varnr] - a < epsvalue))
                            {
                                till = infinite; // if variable is at upper bound then incrementing objective coefficient will not result in extra iterations because it would only extra increase the value, but since it is at its upper bound ...
                            }
                        }
                        else
                        {
                            if (a - lp.lowbo[varnr] < epsvalue)
                            {
                                till = infinite; // if variable is at lower bound then incrementing objective coefficient will not result in extra iterations because it would only extra decrease the value, but since it is at its lower bound ...
                            }
                            else if (((!sensrejvar) || (lp.upbo[varnr] != 0.0)) && (lp.lowbo[varnr] + lp.upbo[varnr] - a < epsvalue))
                            {
                                from = -infinite; // if variable is at upper bound then decrementing objective coefficient will not result in extra iterations because it would only extra increase the value, but since it is at its upper bound ...
                            }
                        }
                    }
                }
                lp.objfrom = from;
                lp.objtill = till;
            }
            lp_utils.mempool_releaseVector(lp.workarrays, ref memVector, 0);
            //FREE(prow);
            //FREE(OrigObj);
            //FREE(drow);

            return (ok);

        }

        internal bool get_row(lprec lp, int rownr, ref double[] row)
        {
            int?[] var = null;
            return ((bool)(get_rowex(lp, rownr, ref row, ref var) >= 0));
        }

        internal void del_splitvars(lprec lp)
        {
            int j;
            int jj;
            int i;

            if (lp.var_is_free != null)
            {
                for (j = lp.columns; j >= 1; j--)
                {
                    if (is_splitvar(lp, j))
                    {
                        /* Check if we need to modify the basis */
                        jj = lp.rows + System.Math.Abs(Convert.ToByte(lp.var_is_free[j]));
                        i = lp.rows + j;
                        if (lp.is_basic[i] && !lp.is_basic[jj])
                        {
                            int var = 0;
                            i = findBasisPos(lp, i, ref var);
                            set_basisvar(lp, i, jj);
                        }
                        /* Delete the helper column */
                        del_column(lp, j);
                    }
                }
                //FREE(lp.var_is_free);
            }
        }

        internal static int findBasisPos(lprec lp, int notint, int[] var_basic)
        {
            int i;

            if (var_basic == null)
            {
                var_basic = lp.var_basic;
            }
            for (i = lp.rows; i > 0; i--)
            {
                if (var_basic[i] == notint)
                {
                    break;
                }
            }
            return (i);
        }

        internal new static bool check_if_less(lprec lp, double x, double y, int variable)
        {
            if (y < x - lp_scale.scaled_value(lp, lp.epsint, variable))
            {
                if (lp.bb_trace)
                {
                    string msg = "check_if_less: Invalid new bound {0} should be < {1} for {2}\n";
                   lp.report(lp, NORMAL, ref msg, x, y, get_col_name(lp, variable));
                }
                return false;
            }
            else
            {
                return true;
            }
        }


        /* Various basis utility routines */
        internal static int findNonBasicSlack(lprec lp, bool[] is_basic)
        {
            int i;

            for (i = lp.rows; i > 0; i--)
            {
                if (!is_basic[i])
                {
                    break;
                }
            }
            return (i);
        }



        internal static bool post_MIPOBJ(lprec lp)
        {
#if MIPboundWithOF
/*
  if(lp->constraintOF) {
    del_constraint(lp, lp->rows);
    if(is_BasisReady(lp) && !verify_basis(lp))
      return( FALSE );
  }
*/
#endif
            return (true);
        }

        internal new bool get_ptr_sensitivity_obj(lprec lp, double[][] objfrom, double[][] objtill)
        {
            return (get_ptr_sensitivity_objex(lp, objfrom, objtill, null, null));
        }

        internal new bool get_ptr_sensitivity_objex(lprec lp, double[][] objfrom, double[][] objtill, double[][] objfromvalue, double[][] objtillvalue)
        {
            string msg;
            if (!lp.basis_valid)
            {
                msg = "get_ptr_sensitivity_objex: Not a valid basis\n";
                lp.report(lp, CRITICAL, ref msg);
                return (false);
            }

            if ((objfrom != null) || (objtill != null))
            {
                if ((lp.objfrom == null) || (lp.objtill == null))
                {
                    if ((MIP_count(lp) > 0) && (lp.bb_totalnodes > 0))
                    {
                        msg = "get_ptr_sensitivity_objex: Sensitivity unknown\n";
                        lp.report(lp, CRITICAL, ref msg);
                        return (false);
                    }
                    construct_sensitivity_obj(lp);
                    if ((lp.objfrom == null) || (lp.objtill == null))
                    {
                        return (false);
                    }
                }
                if (objfrom != null)
                {
                    objfrom[0][0] = Convert.ToDouble(lp.objfrom + 1);
                }
                if (objtill != null)
                {
                    objtill[0][0] = Convert.ToDouble(lp.objtill + 1);
                }
            }

            if ((objfromvalue != null))
            {
                if ((lp.objfromvalue == null))
                {
                    if ((MIP_count(lp) > 0) && (lp.bb_totalnodes > 0))
                    {
                        msg = "get_ptr_sensitivity_objex: Sensitivity unknown\n";
                        lp.report(lp, CRITICAL, ref msg);
                        return false;
                    }
                    construct_sensitivity_duals(lp);
                    if ((lp.objfromvalue == null))
                    {
                        return (false);
                    }
                }
            }

            if (objfromvalue != null)
            {
                objfromvalue[0][0] = Convert.ToDouble(lp.objfromvalue[0] + 1);
            }

            if (objtillvalue != null)
            {
                objtillvalue = null;
            }

            return (true);

        }

        private static new bool is_nativeBFP(lprec lp)
        {
#if ExcludeNativeInverse
  return (0);
//C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#elif LoadInverseLib == TRUE
  return ((bool)(lp.hBFP == null));
#else
            return true;
#endif
        }

        private static new bool is_nativeXLI(lprec lp)
        {
#if ExcludeNativeLanguage
  return (0);
//C++ TO C# CONVERTER TODO TASK: C# does not allow setting or comparing #define constants:
#elif LoadLanguageLib == TRUE
  return ((bool)(lp.hXLI == null));
#else
            return true;
#endif
        }

        internal new static void construct_solution(lprec lp, ref double target)
        {
            int i;
            int j;
            int basi;
            double f;
            double epsvalue = epsprimal;
            double[] solution = null;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: double *value;
            double value;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *rownr;
            int rownr;
            MATrec mat = lp.matA;

            if (target == null)
            {
                solution = lp.solution;
            }
            else
            {
                solution[0] = target;
            }

            /* Initialize OF and slack variables. */
            for (i = 0; i <= lp.rows; i++)
            {
#if LegacySlackDefinition
	if (i == 0)
	{
	  f = unscaled_value(lp, -lp.orig_rhs[i], i);
	}
	else
	{
	  j = lp.presolve_undo.var_to_orig[i];
	  if (j > 0)
	  {
		f = lp.presolve_undo.fixed_rhs[j];
		f = unscaled_value(lp, f, i);
	  }
	  else
	  {
		f = 0;
	  }
	}
#else
                f = lp.orig_rhs[i];
                if ((i > 0) && !lp.is_basic[i] && !lp.is_lower[i])
                {
#if SlackInitMinusInf
	  f -= my_chsign(is_chsign(lp, i), Math.Abs(lp.upbo[i]));
	}
#else
                    f -= lp_types.my_chsign(is_chsign(lp, i), System.Math.Abs(lp.lowbo[i] + lp.upbo[i]));
#endif
                    f = lp_scale.unscaled_value(lp, -f, i);
#endif
                    solution[i] = f;
                }

                /* Initialize user variables to their lower bounds. */
                for (i = lp.rows + 1; i <= lp.sum; i++)
                {
                    solution[i] = lp.lowbo[i];
                }

                /* Add values of user basic variables. */
                for (i = 1; i <= lp.rows; i++)
                {
                    basi = lp.var_basic[i];
                    if (basi > lp.rows)
                    {
                        solution[basi] += lp.rhs[i];
                    }
                }

                /* 1. Adjust non-basic variables at their upper bounds,
                   2. Unscale all user variables,
                   3. Optionally do precision management. */
                for (i = lp.rows + 1; i <= lp.sum; i++)
                {
                    if (!lp.is_basic[i] && !lp.is_lower[i])
                    {
                        solution[i] += lp.upbo[i];
                    }
                    solution[i] = lp_scale.unscaled_value(lp, solution[i], i);
#if xImproveSolutionPrecision
	if (is_int(lp, i - lp.rows))
	{
	  solution[i] = restoreINT(solution[i], lp.epsint);
	}
	else
	{
	  solution[i] = restoreINT(solution[i], lp.epsprimal);
	}
#endif
                }

                /* Compute the OF and slack values "in extentio" */
                for (j = 1; j <= lp.columns; j++)
                {
                    f = solution[lp.rows + j];
                    if (f != 0)
                    {
                        solution[0] += f * lp_scale.unscaled_mat(lp, lp.orig_obj[j], 0, j);
                        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                        //set second [] as 0 for now; need to check at run time
                        i = mat.col_end[j - 1][0];
                        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                        //set second [] as 0 for now; need to check at run time
                        basi = mat.col_end[j][0];
                        rownr = lp_matrix.COL_MAT_ROWNR(i);
                        value = lp_matrix.COL_MAT_VALUE(i);
                        for (; i < basi; i++, rownr += lp_matrix.matRowColStep, value += lp_matrix.matValueStep)
                        {
                            solution[rownr] += f * lp_scale.unscaled_mat(lp, value, rownr, j);
                        }
                    }
                }

                /* Do slack precision management and sign reversal if necessary */
                for (i = 0; i <= lp.rows; i++)
                {
#if ImproveSolutionPrecision
	my_roundzero(solution[i], epsvalue);
#endif
                    if (is_chsign(lp, i))
                    {
                        solution[i] = lp_types.my_flipsign(solution[i]);
                    }
                }

                /* Record the best real-valued solution and compute a simple MIP solution limit */
                if (target == null)
                {
                    if (is_infinite(lp, lp.real_solution))
                    {
                        lp.bb_workOF = lp.rhs[0];
                        lp.real_solution = solution[0];
                        if (is_infinite(lp, lp.bb_limitOF))
                        {
                            lp.bb_limitOF = lp.real_solution;
                        }
                        else
                        {
                            if (is_maxim(lp))
                            {
                               commonlib.SETMIN(lp.bb_limitOF, lp.real_solution);
                            }
                            else
                            {
                                commonlib.SETMAX(lp.bb_limitOF, lp.real_solution);
                            }
                        }

                        /* Do MIP-related tests and computations */
                        if ((lp.int_vars > 0) && lp_matrix.mat_validate(lp.matA))
                        { // && !lp->wasPresolved uncommented by findings of William H. Patton. The code was never executed when the test was there. The code has effect in an integer model with all integer objective coeff. to cut-off optimization and thus make it faster
                            double fixedOF = lp_scale.unscaled_value(lp, lp.orig_rhs[0], 0);

                            /* Check if we have an all-integer OF */
                            basi = lp.columns;
                            for (j = 1; j <= basi; j++)
                            {
                                f = System.Math.Abs(get_mat(lp, 0, j)) + lp.epsint / 2;
                                if (f > lp.epsint)
                                { // If coefficient is 0 then it doesn't influence OF, even it variable is not integer
                                    if (!is_int(lp, j) || (f % 1 > lp.epsint))
                                    {
                                        break;
                                    }
                                }
                            }

                            /* If so, we can round up the fractional OF */
                            if (j > basi)
                            {
                                f = lp_types.my_chsign(is_maxim(lp), lp.real_solution) + fixedOF;
                                f = System.Math.Floor(f + (1 - epsvalue));
                                f = lp_types.my_chsign(is_maxim(lp), f - fixedOF);
                                if (is_infinite(lp, lp.bb_limitOF))
                                {
                                    lp.bb_limitOF = f;
                                }
                                else if (is_maxim(lp))
                                {
                                   commonlib.SETMIN(lp.bb_limitOF, f);
                                }
                                else
                                {
                                    commonlib.SETMAX(lp.bb_limitOF, f);
                                }
                            }
                        }

                        /* Check that a user limit on the OF is feasible */
                        if ((lp.int_vars > 0) && (lp_types.my_chsign(is_maxim(lp), lp_types.my_reldiff((lp.best_solution[0] != null) ? Convert.ToDouble(lp.best_solution[0]) : 0,  lp.bb_limitOF)) < -epsvalue))
                        {
                            lp.spx_status = INFEASIBLE;
                            lp.bb_break = true;
                        }
                    }
                }
            }
            }/* construct_solution */


        internal static int check_solution(lprec lp, int lastcolumn, double?[] solution, double[] upbo, double[] lowbo, double tolerance)
        {
            /*#define UseMaxValueInCheck*/
            bool isSC;

            double test, value, hold, diff, maxdiff = 0.0, maxerr = 0.0, matValue;
            double[] plusum = null, negsum = null;

            int i;
            int j;
            int n;
            int errlevel = IMPORTANT;
            int errlimit = 10;
            
            int matRownr;
            
            int matColnr;
            MATrec mat = lp.matA;
            string msg;
            LpCls objLpCls = new LpCls();

            msg = " \n";
            lp.report(lp, NORMAL, ref msg);
            if (MIP_count(lp) > 0)
            {
                msg = "%s solution  " + lp_types.RESULTVALUEMASK+ " after %10.0f iter, %9.0f nodes (gap %.1f%%).\n";
                //ORIGINAL LINE: lp.report(lp, NORMAL, ref msg, lp_types.my_if(lp.bb_break && !bb_better(lp, OF_DUALLIMIT, OF_TEST_BE) && bb_better(lp, OF_RELAXED, OF_TEST_NE), "Subopt.", "Optimal"), solution[0], (double)lp.total_iter, (double)lp.bb_totalnodes, 100.0 * Math.Abs(my_reldiff(solution[0], lp.bb_limitOF)));
                //String has been passed to the parameters double, hence temporary not considered other optional pameters in report. Need to check at runtime.
                lp.report(lp, NORMAL, ref msg);
            }
            else
            {
                msg = "Optimal solution  " + lp_types.RESULTVALUEMASK + " after %10.0f iter.\n";
                lp.report(lp, NORMAL, ref msg, solution[0], (double)lp.total_iter);
            }

            /* Find the signed sums and the largest absolute product in the matrix (exclude the OF for speed) */
#if UseMaxValueInCheck
  allocREAL(lp, maxvalue, lp.rows + 1, 0);
  for (i = 0; i <= lp.rows; i++)
  {
	maxvalue[i] = Math.Abs(get_rh(lp, i));
  }
#else
            //NOT REQUIRED
            //allocREAL(lp, plusum, lp.rows + 1, 1);
            //allocREAL(lp, negsum, lp.rows + 1, 1);
#endif
            n = objLpCls.get_nonzeros(lp);
            matRownr = lp_matrix.COL_MAT_ROWNR(0);
            matColnr = lp_matrix.COL_MAT_COLNR(0);
            matValue = lp_matrix.COL_MAT_VALUE(0);
            for (i = 0; i < n; i++, matRownr += lp_matrix.matRowColStep, matColnr += lp_matrix.matRowColStep, matValue += lp_matrix.matValueStep)
            {
                test = lp_scale.unscaled_mat(lp, matValue, matRownr, matColnr);
                test *= solution[lp.rows + (matColnr)];
#if UseMaxValueInCheck
	test = Math.Abs(test);
	if (test > maxvalue[*matRownr])
	{
	  maxvalue[*matRownr] = test;
	}
#else
                if (test > 0)
                {
                    plusum[matRownr] += test;
                }
                else
                {
                    negsum[matRownr] += test;
                }
#endif
            }


            /* Check if solution values are within the bounds; allowing a margin for numeric errors */
            n = 0;
            for (i = lp.rows + 1; i <= lp.rows + lastcolumn; i++)
            {

                value = solution[i];

                /* Check for case where we are testing an intermediate solution
                   (variables shifted to the origin) */
                if (lowbo == null)
                {
                    test = 0;
                }
                else
                {
                    test = lp_scale.unscaled_value(lp, lowbo[i], i);
                }

                isSC = is_semicont(lp, i - lp.rows);
                diff = lp_types.my_reldiff(value, test);
                if (diff < 0)
                {
                    if (isSC && (value < test / 2))
                    {
                        test = 0;
                    }
                    commonlib.SETMAX(maxerr, System.Math.Abs(value - test));
                    commonlib.SETMAX(maxdiff, System.Math.Abs(diff));
                }
                if ((diff < -tolerance) && !isSC)
                {
                    if (n < errlimit)
                    {
                        msg = "check_solution: Variable   %s = " + lp_types.RESULTVALUEMASK + " is below its lower bound "+ lp_types.RESULTVALUEMASK + "\n";
                        lp.report(lp, errlevel, ref msg, objLpCls.get_col_name(lp, i - lp.rows), value, test);
                    }
                    n++;
                }

                test = lp_scale.unscaled_value(lp, upbo[i], i);
                diff = lp_types.my_reldiff(value, test);
                if (diff > 0)
                {
                    commonlib.SETMAX(maxerr, System.Math.Abs(value - test));
                    commonlib.SETMAX(maxdiff, System.Math.Abs(diff));
                }
                if (diff > tolerance)
                {
                    if (n < errlimit)
                    {
                        msg = "check_solution: Variable   %s = " + lp_types.RESULTVALUEMASK+ " is above its upper bound "+ lp_types.RESULTVALUEMASK+ "\n";
                        lp.report(lp, errlevel, ref msg, objLpCls.get_col_name(lp, i - lp.rows), value, test);
                    }
                    n++;
                }
            }

            /* Check if constraint values are within the bounds; allowing a margin for numeric errors */
            for (i = 1; i <= lp.rows; i++)
            {

                test = lp.orig_rhs[i];
                if (is_infinite(lp, test))
                {
                    continue;
                }

#if LegacySlackDefinition
	j = lp.presolve_undo.var_to_orig[i];
	if (j != 0)
	{
	  if (is_infinite(lp, lp.presolve_undo.fixed_rhs[j]))
	  {
		continue;
	  }
	  test += lp.presolve_undo.fixed_rhs[j];
	}
#endif

                if (objLpCls.is_chsign(lp, i))
                {
                    test = lp_types.my_flipsign(test);
                    test += System.Math.Abs(upbo[i]);
                }
                value = solution[i];
                test = lp_scale.unscaled_value(lp, test, i);
#if !LegacySlackDefinition
                value += test;
#endif
                /*    diff = my_reldiff(value, test); */
#if UseMaxValueInCheck
	hold = maxvalue[i];
#else
                hold = plusum[i] - negsum[i];
#endif
                if (hold < lprec.epsvalue)
                {
                    hold = 1;
                }
                diff = lp_types.my_reldiff((value + 1) / hold, (test + 1) / hold);
                if (diff > 0)
                {
                    commonlib.SETMAX(maxerr, System.Math.Abs(value - test));
                    commonlib.SETMAX(maxdiff, System.Math.Abs(diff));
                }
                if (diff > tolerance)
                {
                    if (n < errlimit)
                    {
                        msg = "check_solution: Constraint %s = " + lp_types.RESULTVALUEMASK+ " is above its %s "+ lp_types.RESULTVALUEMASK +"\n";
                        lp.report(lp, errlevel, ref msg, objLpCls.get_row_name(lp, i), value, (is_constr_type(lp, i, EQ) ? "equality of" : "upper bound"), test);
                    }
                    n++;
                }

                test = lp.orig_rhs[i];
#if LegacySlackDefinition
	j = lp.presolve_undo.var_to_orig[i];
	if (j != 0)
	{
	  if (is_infinite(lp, lp.presolve_undo.fixed_rhs[j]))
	  {
		continue;
	  }
	  test += lp.presolve_undo.fixed_rhs[j];
	}
#endif

                value = solution[i];
                if (objLpCls.is_chsign(lp, i))
                {
                    test = lp_types.my_flipsign(test);
                }
                else
                {
                    if (is_infinite(lp, upbo[i]))
                    {
                        continue;
                    }
                    test -= System.Math.Abs(upbo[i]);
#if !LegacySlackDefinition
                    value = System.Math.Abs(upbo[i]) - value;
#endif
                }
                test = lp_scale.unscaled_value(lp, test, i);
#if !LegacySlackDefinition
                value += test;
#endif
                /*    diff = my_reldiff(value, test); */
#if UseMaxValueInCheck
	hold = maxvalue[i];
#else
                hold = plusum[i] - negsum[i];
#endif
                if (hold < lprec.epsvalue)
                {
                    hold = 1;
                }
                diff = lp_types.my_reldiff((value + 1) / hold, (test + 1) / hold);
                if (diff < 0)
                {
                    commonlib.SETMAX(maxerr, System.Math.Abs(value - test));
                    commonlib.SETMAX(maxdiff, System.Math.Abs(diff));
                }
                if (diff < -tolerance)
                {
                    if (n < errlimit)
                    {
                        msg = "check_solution: Constraint %s = " + lp_types.RESULTVALUEMASK+ " is below its %s " + lp_types.RESULTVALUEMASK + "\n";
                        lp.report(lp, errlevel, ref msg, objLpCls.get_row_name(lp, i), value, (is_constr_type(lp, i, EQ) ? "equality of" : "lower bound"), test);
                    }
                    n++;
                }
            }

#if UseMaxValueInCheck
  FREE(maxvalue);
#else
            //NOT REQUIRED
            //FREE(plusum);
            //FREE(negsum);
#endif

            if (n > 0)
            {
                msg = "\nSeriously low accuracy found ||*|| = %g (rel. error %g)\n";
                lp.report(lp, IMPORTANT, ref msg, maxerr, maxdiff);
                return (NUMFAILURE);
            }
            else
            {
                if (maxerr > 1.0e-7)
                {
                    msg = "\nMarginal numeric accuracy ||*|| = %g (rel. error %g)\n";
                    lp.report(lp, NORMAL, ref msg, maxerr, maxdiff);
                }
                else if (maxerr > 1.0e-9)
                {
                    msg = "\nReasonable numeric accuracy ||*|| = %g (rel. error %g)\n";
                    lp.report(lp, NORMAL, ref msg, maxerr, maxdiff);
                }
                else if (maxerr > 1.0e11)
                {
                    msg = "\nVery good numeric accuracy ||*|| = %g\n";
                    lp.report(lp, NORMAL, ref msg, maxerr);
                }
                else
                {
                    msg = "\nExcellent numeric accuracy ||*|| = %g\n";
                    lp.report(lp, NORMAL, ref msg, maxerr);
                }

                return (OPTIMAL);
            }

        }

        internal new static bool varmap_canunlock(lprec lp)
        {
            /* Don't do anything if variables aren't locked yet */
            if (lp.varmap_locked)
            {
                int i;
                presolveundorec psundo = lp.presolve_undo;

                /* Check for the obvious */
                if ((psundo.orig_columns > lp.columns) || (psundo.orig_rows > lp.rows))
                {
                    return (false);
                }

                /* Check for deletions */
                for (i = psundo.orig_rows + psundo.orig_columns; i > 0; i--)
                {
                    if (psundo.orig_to_var[i] == 0)
                    {
                        return (false);
                    }
                }

                /* Check for insertions */
                for (i = lp.sum; i > 0; i--)
                {
                    if (psundo.var_to_orig[i] == 0)
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }

        internal static double get_refactfrequency(lprec lp, bool final)
        {
            //ORIGINAL CODE:
            //COUNTER iters = new COUNTER();
            long iters;
            int refacts;

            /* Get numerator and divisor information */
            iters = (lp.total_iter + lp.current_iter) - (lp.total_bswap + lp.current_bswap);
            refacts = lp.bfp_refactcount(lp, commonlib.BFP_STAT_REFACT_TOTAL);

            /* Return frequency for different cases:
                1) Actual frequency in case final statistic is desired
                2) Dummy if we are in a B&B process
                3) Frequency with added initialization offsets which
                   are diluted in course of the solution process */
            if (final)
            {
                return ((double)(iters) / commonlib.MAX(1, refacts));
            }
            else if (lp.bb_totalnodes > 0)
            {
                return ((double)lp.bfp_pivotmax(lp));
            }
            else
            {
                return ((double)(lp.bfp_pivotmax(lp) + iters) / (1 + refacts));
            }
        }

        public new long get_total_nodes(lprec lp)
        {
            return (lp.bb_totalnodes);
        }

        internal new static int GUB_count(lprec lp)
        {
            if (lp.GUB == null)
            {
                return (0);
            }
            else
            {
                return (lp.GUB.sos_count);
            }
        }

        private static new double get_rh(lprec lp, int rownr)
        {
            double value;

            if ((rownr > lp.rows) || (rownr < 0))
            {
                string msg = "get_rh: Row %d out of range";
                lp.report(lp, IMPORTANT, ref msg, rownr);
                return (0.0);
            }

            value = lp.orig_rhs[rownr];
            if (((rownr == 0) && !is_maxim(lp)) || ((rownr > 0) && is_chsign(lp, rownr))) // setting of RHS of OF IS meaningful
            {
                value = lp_types.my_flipsign(value);
            }
            value = lp_scale.unscaled_value(lp, value, rownr);
            return (value);
        }

        internal new void print_lp(lprec lp)
        {
            lp_report objReport = new lp_report();
            objReport.REPORT_lp(lp);
        }

        internal new string get_lp_name(lprec lp)
        {
            return ((lp.lp_name != null) ? lp.lp_name : (string)"");
        }

        internal static void transfer_solution(lprec lp, bool dofinal)
        {
            int i;
            int ii;

            /*NOT REQUIRED
            MEMCOPY(lp.best_solution, lp.solution, lp.sum + 1);
            */

            /* Round integer solution values to actual integers */
            if (is_integerscaling(lp) && (lp.int_vars > 0))
            {
                for (i = 1; i <= lp.columns; i++)
                {
                    if (is_int(lp, i))
                    {
                        ii = lp.rows + i;
                        lp.best_solution[ii] = System.Math.Floor((lp.best_solution[ii] != null) ? Convert.ToDouble(lp.best_solution[ii]) : 0.0 + 0.5);
                    }
                }
            }

            /* Transfer to full solution vector in the case of presolved eliminations */
            if (dofinal != null && lp.varmap_locked && (bool)((lp.do_presolve & PRESOLVE_LASTMASKMODE) != PRESOLVE_NONE))
            {
                presolveundorec psundo = lp.presolve_undo;

                lp.full_solution[0] = (lp.best_solution[0] != null) ? Convert.ToDouble(lp.best_solution[0]) : 0.0;
                for (i = 1; i <= lp.rows; i++)
                {
                    ii = psundo.var_to_orig[i];
#if Paranoia
	  if ((ii < 0) || (ii > lp.presolve_undo.orig_rows))
	  {
		report(lp, SEVERE, "transfer_solution: Invalid mapping of row index %d to original index '%d'\n", i, ii);
	  }
#endif
                    lp.full_solution[ii] = (lp.best_solution[i] != null) ? Convert.ToDouble(lp.best_solution[i]) : 0.0;
                }
                for (i = 1; i <= lp.columns; i++)
                {
                    ii = psundo.var_to_orig[lp.rows + i];
#if Paranoia
	  if ((ii < 0) || (ii > lp.presolve_undo.orig_columns))
	  {
		report(lp, SEVERE, "transfer_solution: Invalid mapping of column index %d to original index '%d'\n", i, ii);
	  }
#endif
                    lp.full_solution[psundo.orig_rows + ii] = (lp.best_solution[lp.rows + i] != null) ? Convert.ToDouble(lp.best_solution[lp.rows + i]) : 0;
                }
            }

        }

        private static new int prepare_GUB(lprec lp)
        {
            int i;
            int j;
            int jb;
            int je;
            int k;
            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
            //changed from 'int[] members' to 'int[][] members' need to check at run time
            int[][] members = null;
            double rh;
            string GUBname = new string(new char[16]);
            MATrec mat = lp.matA;

            if ((lp.equalities == 0) || ! lp_utils.allocINT(lp, members, lp.columns + 1, 1) || ! lp_matrix.mat_validate(mat))
            {
                return (0);
            }

            for (i = 1; i <= lp.rows; i++)
            {

                /* Check if it has been marked as a GUB */
                if (((lp.row_type[i] & ROWTYPE_GUB) == 0))

                {
                    continue;
                }

                /* Pick up the GUB column indeces */
                k = 0;
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                je = mat.row_end[i][0];
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set second [] as 0 for now; need to check at run time
                for (jb = mat.row_end[i - 1][0], k = 0; jb < je; jb++)
                {
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    members[k][0] = lp_matrix.ROW_MAT_COLNR(jb);
                    k++;
                }

                /* Add the GUB */
                j = GUB_count(lp) + 1;
                GUBname = string.Format("GUB_{0:D}", i);
                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                //set [][] as 0 for now; need to check at run time
                add_GUB(lp, ref GUBname, j, k, ref members[0][0]);

                /* Unmark the GUBs */
                clear_action(ref (lp.row_type[i]), ROWTYPE_GUB);

                /* Standardize coefficients to 1 if necessary */
                rh = get_rh(lp, i);
                if (System.Math.Abs(lp_types.my_reldiff(rh, 1)) > epsprimal)
                {
                    set_rh(lp, i, 1);
                    //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                    //set second [] as 0 for now; need to check at run time
                    for (jb = mat.row_end[i - 1][0]; jb < je; jb++)
                    {
                        j = lp_matrix.ROW_MAT_COLNR(jb);
                        set_mat(lp, i, j, 1);
                    }
                }

            }
            /*NOT REQUIRED
            FREE(members);
            */
            return (GUB_count(lp));
        }

        private static new int add_GUB(lprec lp, ref string name, int priority, int count, ref int gubvars)
        {
            SOSrec GUB;
            int k;

#if Paranoia
  if (count < 0)
  {
	report(lp, IMPORTANT, "add_GUB: Invalid GUB member count %d\n", count);
	return (0);
  }
#endif

            /* Make size in the list to handle another GUB record */
            if (lp.GUB == null)
            {
                lp.GUB = lp_SOS.create_SOSgroup(lp);
            }

            /* Create and append GUB to list */
            double? weights = null;
            int?[] gv = null;
            gv[0] = gubvars;
            GUB = lp_SOS.create_SOSrec(lp.GUB, ref name, 1, priority, count, ref gv, ref weights);
            GUB.isGUB = 1;
            k = lp_SOS.append_SOSgroup(lp.GUB, GUB);

            return (k);

        }


    }
}