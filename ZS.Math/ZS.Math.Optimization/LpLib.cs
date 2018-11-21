using System;
using System.IO;

namespace ZS.Math.Optimization
{

    /* Partial pricing block data */
    public class partialrec
    {
        lprec lp;
        int blockcount;         /* ## The number of logical blocks or stages in the model */
        int blocknow;           /* The currently active block */
        int[] blockend;         /* Array of column indeces giving the start of each block */
        int[] blockpos;         /* Array of column indeces giving the start scan position */
        byte isrow;
    }


    public class SOSrec
    {
        public SOSgroup parent;
        public int tagorder;
        public string name;
        public int type;
        public byte isGUB;
        public int size;
        public int priority;
        public int[] members;
        public double[] weights;
        public int[] membersSorted;
        public int[] membersMapped;
    }

    /* typedef */
    public class SOSgroup
    {
        public lprec lp;                         // Pointer to owner 
        public SOSrec[] sos_list;          // Array of pointers to SOS lists
        public int sos_alloc;               // Size allocated to specially ordered sets (SOS1, SOS2...)
        public int sos_count;               // Number of specially ordered sets (SOS1, SOS2...)
        public int maxorder;                // The highest-order SOS in the group
        public int sos1_count;              // Number of the lowest order SOS in the group
        public int[] membership;            // Array of variable-sorted indeces to SOSes that the variable is member of
        //ORIGINAL LINE: int *memberpos;
        public int[] memberpos;               // Starting positions of the each column's membership list
    }

    /* Basis storage (mainly for B&B) */
    public class basisrec
    {
        public int level;
        //ORIGINAL LINE: int *var_basic;
        public int var_basic;
        //ORIGINAL LINE: byte *is_basic;
        public byte is_basic;
        //ORIGINAL LINE: byte *is_lower;
        public byte is_lower;
        public int pivots;
        public basisrec previous;
    }

    /* Presolve undo data storage */
    public class presolveundorec
    {
        public lprec lp;
        public int orig_rows;
        public int orig_columns;
        public int orig_sum;
        //ORIGINAL LINE: int *var_to_orig;
        public int[] var_to_orig; /* sum_alloc+1 : Mapping of variables from solution to
                                   best_solution to account for removed variables and
                                   rows during presolve; a non-positive value indicates
                                   that the constraint or variable was removed */
        //ORIGINAL LINE: int *orig_to_var;
        public int[] orig_to_var; /* sum_alloc+1 : Mapping from original variable index to
                                   current / working index number */
        //ORIGINAL LINE: double *fixed_rhs;
        public double[] fixed_rhs; // rows_alloc+1 : Storage of values of presolved fixed colums
        //ORIGINAL LINE: double *fixed_obj;
        public double[] fixed_obj; // columns_alloc+1: Storage of values of presolved fixed rows
        public DeltaVrec deletedA; // A matrix of eliminated data from matA
        public DeltaVrec primalundo; // Affine translation vectors for eliminated primal variables
        public DeltaVrec dualundo; // Affine translation vectors for eliminated dual variables
        public byte OFcolsdeleted;
    }

    /* Pseudo-cost arrays used during B&B */
    public class BBPSrec
    {
        public lprec lp;
        public int pseodotype;
        public int updatelimit;
        public int updatesfinished;
        public double restartlimit;
        public MATitem UPcost;
        public MATitem LOcost;
        public BBPSrec secondary;
    }


    public class lprec : lp_lib
    {
        /// <summary>
        /// Convert code from 1116 to 1359
        /// </summary>

        /* Full list of exported functions made available in a quasi object-oriented fashion */
        add_column_func add_column;
        add_columnex_func add_columnex;
        add_constraint_func add_constraint;
        add_constraintex_func add_constraintex;
        add_lag_con_func add_lag_con;
        add_SOS_func add_SOS;
        column_in_lp_func column_in_lp;
        copy_lp_func copy_lp;
        default_basis_func default_basis;
        del_column_func del_column;
        del_constraint_func del_constraint;
        delete_lp_func delete_lp;
        dualize_lp_func dualize_lp;
        free_lp_func free_lp;
        get_anti_degen_func get_anti_degen;
        get_basis_func get_basis;
        get_basiscrash_func get_basiscrash;
        get_bb_depthlimit_func get_bb_depthlimit;
        get_bb_floorfirst_func get_bb_floorfirst;
        get_bb_rule_func get_bb_rule;
        get_bounds_tighter_func get_bounds_tighter;
        get_break_at_value_func get_break_at_value;
        get_col_name_func get_col_name;
        get_columnex_func get_columnex;
        get_constr_type_func get_constr_type;
        get_constr_value_func get_constr_value;
        get_constraints_func get_constraints;
        get_dual_solution_func get_dual_solution;
        get_epsb_func get_epsb;
        get_epsd_func get_epsd;
        get_epsel_func get_epsel;
        get_epsint_func get_epsint;
        get_epsperturb_func get_epsperturb;
        get_epspivot_func get_epspivot;
        get_improve_func get_improve;
        get_infinite_func get_infinite;
        get_lambda_func get_lambda;
        get_lowbo_func get_lowbo;
        get_lp_index_func get_lp_index;
        get_lp_name_func get_lp_name;
        get_Lrows_func get_Lrows;
        get_mat_func get_mat;
        get_mat_byindex_func get_mat_byindex;
        get_max_level_func get_max_level;
        get_maxpivot_func get_maxpivot;
        get_mip_gap_func get_mip_gap;
        get_multiprice_func get_multiprice;
        get_nameindex_func get_nameindex;
        get_Ncolumns_func get_Ncolumns;
        get_negrange_func get_negrange;
        get_nz_func get_nonzeros;
        get_Norig_columns_func get_Norig_columns;
        get_Norig_rows_func get_Norig_rows;
        get_Nrows_func get_Nrows;
        get_obj_bound_func get_obj_bound;
        get_objective_func get_objective;
        get_orig_index_func get_orig_index;
        get_origcol_name_func get_origcol_name;
        get_origrow_name_func get_origrow_name;
        get_partialprice_func get_partialprice;
        get_pivoting_func get_pivoting;
        get_presolve_func get_presolve;
        get_presolveloops_func get_presolveloops;
        get_primal_solution_func get_primal_solution;
        get_print_sol_func get_print_sol;
        get_pseudocosts_func get_pseudocosts;
        get_ptr_constraints_func get_ptr_constraints;
        get_ptr_dual_solution_func get_ptr_dual_solution;
        get_ptr_lambda_func get_ptr_lambda;
        get_ptr_primal_solution_func get_ptr_primal_solution;
        get_ptr_sensitivity_obj_func get_ptr_sensitivity_obj;
        get_ptr_sensitivity_objex_func get_ptr_sensitivity_objex;
        get_ptr_sensitivity_rhs_func get_ptr_sensitivity_rhs;
        get_ptr_variables_func get_ptr_variables;
        get_rh_func get_rh;
        get_rh_range_func get_rh_range;
        get_row_func get_row;
        get_rowex_func get_rowex;
        get_row_name_func get_row_name;
        get_scalelimit_func get_scalelimit;
        get_scaling_func get_scaling;
        get_sensitivity_obj_func get_sensitivity_obj;
        get_sensitivity_objex_func get_sensitivity_objex;
        get_sensitivity_rhs_func get_sensitivity_rhs;
        get_simplextype_func get_simplextype;
        get_solutioncount_func get_solutioncount;
        get_solutionlimit_func get_solutionlimit;
        get_status_func get_status;
        get_statustext_func get_statustext;
        get_timeout_func get_timeout;
        get_total_iter_func get_total_iter;
        get_total_nodes_func get_total_nodes;
        get_upbo_func get_upbo;
        get_var_branch_func get_var_branch;
        get_var_dualresult_func get_var_dualresult;
        get_var_primalresult_func get_var_primalresult;
        get_var_priority_func get_var_priority;
        get_variables_func get_variables;
        get_verbose_func get_verbose;
        get_working_objective_func get_working_objective;
        has_BFP_func has_BFP;
        has_XLI_func has_XLI;
        is_add_rowmode_func is_add_rowmode;
        is_anti_degen_func is_anti_degen;
        is_binary_func is_binary;
        is_break_at_first_func is_break_at_first;
        is_constr_type_func is_constr_type;
        is_debug_func is_debug;
        is_feasible_func is_feasible;
        is_infinite_func is_infinite;
        is_int_func is_int;
        is_integerscaling_func is_integerscaling;
        is_lag_trace_func is_lag_trace;
        is_maxim_func is_maxim;
        is_nativeBFP_func is_nativeBFP;
        is_nativeXLI_func is_nativeXLI;
        is_negative_func is_negative;
        is_obj_in_basis_func is_obj_in_basis;
        is_piv_mode_func is_piv_mode;
        is_piv_rule_func is_piv_rule;
        is_presolve_func is_presolve;
        is_scalemode_func is_scalemode;
        is_scaletype_func is_scaletype;
        is_semicont_func is_semicont;
        is_SOS_var_func is_SOS_var;
        is_trace_func is_trace;
        is_unbounded_func is_unbounded;
        is_use_names_func is_use_names;
        lp_solve_version_func lp_solve_version;
        make_lp_func make_lp;
        print_constraints_func print_constraints;
        print_debugdump_func print_debugdump;
        print_duals_func print_duals;
        print_lp_func print_lp;
        print_objective_func print_objective;
        print_scales_func print_scales;
        print_solution_func print_solution;
        print_str_func print_str;
        print_tableau_func print_tableau;
        put_abortfunc_func put_abortfunc;
        put_bb_nodefunc_func put_bb_nodefunc;
        put_bb_branchfunc_func put_bb_branchfunc;
        put_logfunc_func put_logfunc;
        put_msgfunc_func put_msgfunc;
        read_LP_func read_LP;
        read_MPS_func read_MPS;
        read_XLI_func read_XLI;
        read_params_func read_params;
        read_basis_func read_basis;
        reset_basis_func reset_basis;
        reset_params_func reset_params;
        resize_lp_func resize_lp;
        set_add_rowmode_func set_add_rowmode;
        set_anti_degen_func set_anti_degen;
        set_basisvar_func set_basisvar;
        set_basis_func set_basis;
        set_basiscrash_func set_basiscrash;
        set_bb_depthlimit_func set_bb_depthlimit;
        set_bb_floorfirst_func set_bb_floorfirst;
        set_bb_rule_func set_bb_rule;
        set_BFP_func set_BFP;
        set_binary_func set_binary;
        set_bounds_func set_bounds;
        set_bounds_tighter_func set_bounds_tighter;
        set_break_at_first_func set_break_at_first;
        set_break_at_value_func set_break_at_value;
        set_column_func set_column;
        set_columnex_func set_columnex;
        set_col_name_func set_col_name;
        set_constr_type_func set_constr_type;
        set_debug_func set_debug;
        set_epsb_func set_epsb;
        set_epsd_func set_epsd;
        set_epsel_func set_epsel;
        set_epsint_func set_epsint;
        set_epslevel_func set_epslevel;
        set_epsperturb_func set_epsperturb;
        set_epspivot_func set_epspivot;
        set_unbounded_func set_unbounded;
        set_improve_func set_improve;
        set_infinite_func set_infinite;
        set_int_func set_int;
        set_lag_trace_func set_lag_trace;
        set_lowbo_func set_lowbo;
        set_lp_name_func set_lp_name;
        set_mat_func set_mat;
        set_maxim_func set_maxim;
        set_maxpivot_func set_maxpivot;
        set_minim_func set_minim;
        set_mip_gap_func set_mip_gap;
        set_multiprice_func set_multiprice;
        set_negrange_func set_negrange;
        set_obj_bound_func set_obj_bound;
        set_obj_fn_func set_obj_fn;
        set_obj_fnex_func set_obj_fnex;
        set_obj_func set_obj;
        set_obj_in_basis_func set_obj_in_basis;
        set_outputfile_func set_outputfile;
        set_outputstream_func set_outputstream;
        set_partialprice_func set_partialprice;
        set_pivoting_func set_pivoting;
        set_preferdual_func set_preferdual;
        set_presolve_func set_presolve;
        set_print_sol_func set_print_sol;
        set_pseudocosts_func set_pseudocosts;
        set_rh_func set_rh;
        set_rh_range_func set_rh_range;
        set_rh_vec_func set_rh_vec;
        set_row_func set_row;
        set_rowex_func set_rowex;
        set_row_name_func set_row_name;
        set_scalelimit_func set_scalelimit;
        set_scaling_func set_scaling;
        set_semicont_func set_semicont;
        set_sense_func set_sense;
        set_simplextype_func set_simplextype;
        set_solutionlimit_func set_solutionlimit;
        set_timeout_func set_timeout;
        set_trace_func set_trace;
        set_upbo_func set_upbo;
        set_use_names_func set_use_names;
        set_var_branch_func set_var_branch;
        set_var_weights_func set_var_weights;
        set_verbose_func set_verbose;
        set_XLI_func set_XLI;
        solve_func solve;
        str_add_column_func str_add_column;
        str_add_constraint_func str_add_constraint;
        str_add_lag_con_func str_add_lag_con;
        str_set_obj_fn_func str_set_obj_fn;
        str_set_rh_vec_func str_set_rh_vec;
        time_elapsed_func time_elapsed;
        unscale_func unscale;
        write_lp_func write_lp;
        write_LP_func write_LP;
        write_mps_func write_mps;
        write_MPS_func write_MPS;
        write_freemps_func write_freemps;
        write_freeMPS_func write_freeMPS;
        write_XLI_func write_XLI;
        write_basis_func write_basis;
        write_params_func write_params;

        /* Spacer */
        //ORIGINAL LINE: int *alignmentspacer;
        public int alignmentspacer;

        /* Problem description */
        public string lp_name;              // The name of the model

        /* Problem sizes */
        public int sum;                     // The total number of variables, including slacks
        public int rows;
        public int columns;
        public int equalities;              // No of non-Lagrangean equality constraints in the problem
        public int boundedvars;             // Count of bounded variables
        public int INTfuture1;

        /* Memory allocation sizes */
        public int sum_alloc;               // The allocated memory for row+column-sized data
        public int rows_alloc;              // The allocated memory for row-sized data
        public int columns_alloc;           // The allocated memory for column-sized data

        /* Model status and solver result variables */
        public bool source_is_file;     // The base model was read from a file
        public byte model_is_pure;      // The model has been built entirely from row and column additions
        public byte model_is_valid;     // Has this lp pased the 'test'
        public bool tighten_on_set;     // Specify if bounds will be tightened or overriden at bound setting
        public bool names_used;         // Flag to indicate if names for rows and columns are used
        public byte use_row_names;      // Flag to indicate if names for rows are used
        public byte use_col_names;      // Flag to indicate if names for columns are used

        public byte lag_trace;          // Print information on Lagrange progression
        public byte spx_trace;          // Print information on simplex progression
        public byte bb_trace;           // TRUE to print extra debug information
        /// <summary>
        /// changed from byte to bool 6/11/18
        /// </summary>
        public bool streamowned;        // TRUE if the handle should be closed at delete_lp()
        public bool obj_in_basis;       // TRUE if the objective function is in the basis matrix

        public int spx_status;          // Simplex solver feasibility/mode code
        public int lag_status;          // Extra status variable for lag_solve
        public int solutioncount;       // number of equal-valued solutions found (up to solutionlimit)
        public int solutionlimit;       // upper number of equal-valued solutions kept track of

        public double real_solution; // Optimal non-MIP solution base
        public double solution; /* sum_alloc+1 : Solution array of the next to optimal LP,
                                   Index   0           : Objective function value,
                                   Indeces 1..rows     : Slack variable values,
                                   Indeced rows+1..sum : Variable values */
        public double best_solution; /* sum_alloc+1 : Solution array of optimal 'Integer' LP,
                                   structured as the solution array above */
        public double full_solution; // sum_alloc+1 : Final solution array expanded for deleted variables
        public double edgeVector; // Array of reduced cost scaling norms (DEVEX and Steepest Edge)

        public double drow; // sum+1: Reduced costs of the last simplex
        //ORIGINAL LINE: int *nzdrow;
        public int nzdrow; // sum+1: Indeces of non-zero reduced costs of the last simplex
        public double? duals; // rows_alloc+1 : The dual variables of the last LP
        public double full_duals; // sum_alloc+1: Final duals array expanded for deleted variables
        public double? dualsfrom; /* sum_alloc+1 :The sensitivity on dual variables/reduced costs
                                   of the last LP */
        public double? dualstill; /* sum_alloc+1 :The sensitivity on dual variables/reduced costs
                                   of the last LP */
        public double? objfrom; /* columns_alloc+1 :The sensitivity on objective function
                                   of the last LP */
        public double? objtill; /* columns_alloc+1 :The sensitivity on objective function
                                   of the last LP */
        public double? objfromvalue; /* columns_alloc+1 :The value of the variables when objective value
                                   is at its from value of the last LP */
        public double[] orig_obj; // Unused pointer - Placeholder for OF not part of B
        public double obj; // Special vector used to temporarily change the OF vector

        public long current_iter; // Number of iterations in the current/last simplex
        public long total_iter; // Number of iterations over all B&B steps
        public ulong current_bswap; // Number of bound swaps in the current/last simplex
        public ulong total_bswap; // Number of bount swaps over all B&B steps
        public int solvecount; // The number of solve() performed in this model
        public int max_pivots; // Number of pivots between refactorizations of the basis

        /* Various execution parameters */
        public int simplex_strategy; // Set desired combination of primal and dual simplex algorithms
        public int simplex_mode; // Specifies the current simplex mode during solve; see simplex_strategy
        public int verbose; // Set amount of run-time messages and results
        public int print_sol; // TRUE to print optimal solution; AUTOMATIC skips zeros

        //changed from 'FILE outstream' to 'FileStream outstream' FIX_bcf78206-4a66-43e8-95c0-85b129a41196 19/11/18 
        public FileStream outstream; // Output stream, initialized to STDOUT

        /* Main Branch and Bound settings */
        /// <summary>
        /// changed byte? from byte as set to null
        /// </summary>
        public byte? bb_varbranch; /* Determines branching strategy at the individual variable level;
                                   the setting here overrides the bb_floorfirst setting */
        public int piv_strategy; // Strategy for selecting row and column entering/leaving
        public int _piv_rule_; // Internal working rule-part of piv_strategy above
        public int bb_rule; // Rule for selecting B&B variables
        public byte bb_floorfirst; /* Set BRANCH_FLOOR for B&B to set variables to floor bound first;
                                   conversely with BRANCH_CEILING, the ceiling value is set first */
        public byte bb_breakfirst; // TRUE to stop at first feasible solution
        public byte _piv_left_; // Internal variable indicating active pricing loop order
        public byte BOOLfuture1;

        public double scalelimit; // Relative convergence criterion for iterated scaling
        public int scalemode; // OR-ed codes for data scaling
        public int improve; // Set to non-zero for iterative improvement
        public int anti_degen; // Anti-degen strategy (or none) TRUE to avoid cycling
        public int do_presolve; // PRESOLVE_ parameters for LP presolving
        public int presolveloops; // Maximum number of presolve loops

        public int perturb_count; // The number of bound relaxation retries performed

        /* Row and column names storage variables */
        public hashelem[] row_name; // rows_alloc+1
        public hashelem[] col_name; // columns_alloc+1
        public hashtable rowname_hashtab; // hash table to store row names
        public hashtable colname_hashtab; // hash table to store column names

        /* Optionally specify continuous rows/column blocks for partial pricing */
        public partialrec rowblocks;
        public partialrec colblocks;

        /* Row and column type codes */
        public bool[] var_type; // sum_alloc+1 : TRUE if variable must be integer

        /* Data for multiple pricing */
        public multirec multivars;
        public int multiblockdiv; // The divisor used to set or augment pricing block

        /* Variable (column) parameters */
        public int fixedvars; // The current number of basic fixed variables in the model
        public int int_vars; // Number of variables required to be integer

        public int sc_vars; // Number of semi-continuous variables
        public double[] sc_lobound; /* sum_columns+1 : TRUE if variable is semi-continuous;
                                   value replaced by conventional lower bound during solve */
                                  //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                                  //ORIGINAL LINE: int *var_is_free;
        public int var_is_free; // columns+1: Index of twin variable if variable is free
                                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                                //ORIGINAL LINE: int *var_priority;
        /// <summary>
        /// changed int? from int as set to null
        /// </summary>
        public int? var_priority; // columns: Priority-mapping of variables

        public SOSgroup GUB; // Pointer to record containing GUBs

        public int sos_vars; // Number of variables in the sos_priority list
        public int sos_ints; // Number of integers in SOS'es above
        public SOSgroup SOS; // Pointer to record containing all SOS'es

        //ORIGINAL LINE: int *sos_priority;
        /// <summary>
        /// changed int? from int as set to null
        /// </summary>
        public int? sos_priority; // Priority-sorted list of variables (no duplicates)

        /* Optionally specify list of active rows/columns used in multiple pricing */
        public double bsolveVal; // rows+1: bsolved solution vector for reduced costs

        //ORIGINAL LINE: int *bsolveIdx;
        public int bsolveIdx; // rows+1: Non-zero indeces of bsolveVal

        /* RHS storage */
        public double[] orig_rhs; /* rows_alloc+1 : The RHS after scaling and sign
                                   changing, but before 'Bound transformation' */
        /// <summary>
        /// 1501
        /// </summary>
        public double rhs; // rows_alloc+1 : The RHS of the current simplex tableau

        /* Row (constraint) parameters */
        //ORIGINAL LINE: int *row_type;
        public int[] row_type; // rows_alloc+1 : Row/constraint type coding

        /* Optionally specify data for dual long-step */
        public multirec longsteps;

        /* Original and working row and variable bounds */
        public double[] orig_upbo; // sum_alloc+1 : Bound before transformations
        public double[] upbo; //  " " : Upper bound after transformation and B&B work
        /// <summary>
        /// changed from double to double[] as required in LpCls.cs
        /// </summary>
        public double[] orig_lowbo; //  "       "
        //ORIGINAL LINE: REAL* lowbo;             /*  " " : Lower bound after transformation and B&B work */
        public double lowbo;


        /* User data and basis factorization matrices (ETA or LU, product form) */
        internal MATrec matA;
        internal INVrec invB;

        /* Basis and bounds */
        internal BBrec bb_bounds;         /* The linked list of B&B bounds */
        internal BBrec rootbounds;        /* The bounds at the lowest B&B level */
        internal basisrec bb_basis;          /* The linked list of B&B bases */
        basisrec rootbasis;
        internal OBJmonrec monitor;           /* Objective monitoring record for stalling/degeneracy handling */

        /* Scaling parameters */
        internal double[] scalars;           /* sum_alloc+1:0..Rows the scaling of the rows,
                                   Rows+1..Sum the scaling of the columns */
        internal bool scaling_used;       /* TRUE if scaling is used */
        internal bool columns_scaled;     /* TRUE if the columns are scaled too */
        internal bool varmap_locked;      /* Determines whether the var_to_orig and orig_to_var are fixed */

        /* Variable state information */
        internal byte basis_valid;        /* TRUE is the basis is still valid */
        int crashmode;          /* Basis crashing mode (or none) */
        int[] var_basic;         /* rows_alloc+1: The list of columns in the basis */
        double[] val_nonbasic;      /* Array to store current values of non-basic variables */
        byte is_basic;          /* sum_alloc+1: TRUE if the column is in the basis */
        byte is_lower;          /*  "       " : TRUE if the variable is at its
                                   lower bound (or in the basis), FALSE otherwise */

        /* Simplex basis indicators */
        int[] rejectpivot;       /* List of unacceptable pivot choices due to division-by-zero */
        BBPSrec bb_PseudoCost;     /* Data structure for costing of node branchings */
        int bb_PseudoUpdates;   /* Maximum number of updates for pseudo-costs */
        internal int bb_strongbranches;  /* The number of strong B&B branches performed */
        int is_strongbranch;    /* Are we currently in a strong branch mode? */
        int bb_improvements;    /* The number of discrete B&B objective improvement steps */

        /* Solver working variables */
        internal double rhsmax;             /* The maximum |value| of the rhs vector at any iteration */
        internal double suminfeas;          /* The working sum of primal and dual infeasibilities */
        internal double bigM;               /* Original objective weighting in primal phase 1 */
        internal double P1extraVal;         /* Phase 1 OF/RHS offset for feasibility */
        internal int P1extraDim;         /* Phase 1 additional columns/rows for feasibility */
        internal int spx_action;         /* ACTION_ variables for the simplex routine */
        byte spx_perturbed;      /* The variable bounds were relaxed/perturbed into this simplex */
        /// <summary>
        /// changed access modifier to internal due to inaccessibility 6/11/18
        /// </summary>
        internal byte bb_break;           /* Solver working variable; signals break of the B&B */
        internal byte wasPreprocessed;    /* The solve preprocessing was performed */
        internal bool wasPresolved;       /* The solve presolver was invoked */
        int INTfuture2;

        /* Lagragean solver storage and parameters */
        internal MATrec matL;
        double[] lag_rhs;           /* Array of Lagrangean rhs vector */
        int[] lag_con_type;      /* Array of GT, LT or EQ */
        double[] lambda;            /* Lambda values (Lagrangean multipliers) */
        double lag_bound;          /* The Lagrangian lower OF bound */
        double lag_accept;         /* The Lagrangian convergence criterion */

        /* Solver thresholds */
        internal double infinite;           /* Limit for object range */
        double negrange;           /* Limit for negative variable range */
        double epsmachine;         /* Default machine accuracy */

        /// <summary>
        /// Assign to zero due to unaccesible to other class
        /// TODO: Please check assignment as per logic
        /// </summary>
        public const double epsvalue = 0;           /* Input data precision / rounding of data values to 0 */

        /// <summary>
        /// Assign to zero due to unaccesible to other class
        /// TODO: Please check assignment as per logic
        /// </summary>
        public const double epsprimal = 0;          /* For rounding RHS values to 0/infeasibility */

        /// <summary>
        /// Assign to zero due to unaccesible to other class
        /// TODO: Please check assignment as per logic
        /// </summary>
        public const double epsdual = 0;            /* For rounding reduced costs to zero */

        

        double epspivot;           /* Pivot reject tolerance */
        double epsperturb;         /* Perturbation scalar */
        double epssolution;        /* The solution tolerance for final validation */

        /* Branch & Bound working parameters */
        int bb_status;          /* Indicator that the last solvelp() gave an improved B&B solution */
        /// <summary>
        /// changed access modifier to internal due to inaccessibility 6/11/18
        /// </summary>
        internal int bb_level;           /* Solver B&B working variable (recursion depth) */
        int bb_maxlevel;        /* The deepest B&B level of the last solution */
        int bb_limitlevel;      /* The maximum B&B level allowed */
        long bb_totalnodes;      /* Total number of nodes processed in B&B */
        int bb_solutionlevel;   /* The B&B level of the last / best solution */
        int bb_cutpoolsize;     /* Size of the B&B cut pool */
        int bb_cutpoolused;     /* Currently used cut pool */
        int bb_constraintOF;    /* General purpose B&B parameter (typically for testing) */
        int[] bb_cuttype;        /* The type of the currently used cuts */
        internal int[] bb_varactive;      /* The B&B state of the variable; 0 means inactive */
        DeltaVrec bb_upperchange;    /* Changes to upper bounds during the B&B phase */
        DeltaVrec bb_lowerchange;    /* Changes to lower bounds during the B&B phase */

        internal double bb_deltaOF;         /* Minimum OF step value; computed at beginning of solve() */

        internal double bb_breakOF;         /* User-settable value for the objective function deemed
                               to be sufficiently good in an integer problem */
        double bb_limitOF;         /* "Dual" bound / limit to final optimal MIP solution */
        internal double bb_heuristicOF;     /* Set initial "at least better than" guess for objective function
                               (can significantly speed up B&B iterations) */
        double bb_parentOF;        /* The OF value of the previous BB simplex */
        double bb_workOF;          /* The unadjusted OF value for the current best solution */

        /* Internal work arrays allocated as required */
        internal presolveundorec presolve_undo;
        internal workarraysrec workarrays;

        /* MIP parameters */
        double epsint;             /* Margin of error in determining if a float value is integer */
        double mip_absgap;         /* Absolute MIP gap */
        double mip_relgap;         /* Relative MIP gap */

        /* Time/timer variables and extended status text */
        internal double timecreate;
        /// <summary>
        /// changed access modifier to internal due to inaccessibility 6/11/18
        /// changed datatype from double to DateTime because unable to subtract from a datetime variale at yieldformessages() 6/11/18
        /// </summary>
        internal double timestart;
        double timeheuristic;
        double timepresolved;
        internal double timeend;
        public double sectimeout;

        /* Extended status message text set via explain() */
        string ex_status;

        /// Start convert from 1636 to 1710
        /* Prototypes for basis inverse/factorization libraries                      */
        /* ------------------------------------------------------------------------- */
        // ORIGINAL LINE: typedef char   *(BFP_CALLMODEL BFPchar)(void);
        public delegate string BFPchar();
        // ORIGINAL LINE: typedef MYBOOL(BFP_CALLMODEL BFPbool_lpintintint)(lprec* lp, int size, int deltasize, int sizeofvar);
        public delegate byte BFPbool_lpintintint(lprec lp, int size, int deltasize, int sizeofvar);
        // ORIGINAL LINE: typedef MYBOOL(BFP_CALLMODEL BFPbool_lpintintchar)(lprec* lp, int size, int deltasize, char* options);
        public delegate byte BFPbool_lpintintchar(lprec lp, int size, int deltasize, ref string options);
        // ORIGINAL LINE: typedef void   (BFP_CALLMODEL BFP_lp)(lprec *lp);
        public delegate void BFP_lp(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL(BFP_CALLMODEL BFPbool_lpint)(lprec* lp, int size);
        public delegate byte BFPbool_lpint(lprec lp, int size);
        // ORIGINAL LINE: typedef int    (BFP_CALLMODEL BFPint_lp)(lprec* lp);
        public delegate int BFPint_lp(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL(BFP_CALLMODEL BFPbool_lp)(lprec* lp);
        public delegate byte BFPbool_lp(lprec lp);
        // ORIGINAL LINE: typedef int    (BFP_CALLMODEL BFPint_lpintintboolbool)(lprec* lp, int uservars, int Bsize, MYBOOL *usedpos, MYBOOL final);
        public delegate int BFPint_lpintintboolbool(lprec lp, int uservars, int Bsize, ref byte usedpos, byte final);
        // ORIGINAL LINE: typedef LREAL(BFP_CALLMODEL BFPlreal_lpintintreal)(lprec* lp, int row_nr, int col_nr, REAL *pcol);
        //TODO: uncertain about using long or double instead of long double datatype in C, hence used double
        //TODO: long is signed 64 bit integer, double is 64 bit floating value, use according to the requirement
        public delegate double BFPlreal_lpintintreal(lprec lp, int row_nr, int col_nr, ref double pcol);
        // ORIGINAL LINE: typedef REAL(BFP_CALLMODEL BFPreal_lplrealreal)(lprec* lp, LREAL theta, REAL* pcol);
        public delegate double BFPreal_lplrealreal(lprec lp, double theta, ref double pcol);
        // ORIGINAL LINE: typedef MYBOOL(BFP_CALLMODEL BFPbool_lpbool)(lprec* lp, MYBOOL changesign);
        public delegate byte BFPbool_lpbool(lprec lp, byte changesign);
        // ORIGINAL LINE: typedef void   (BFP_CALLMODEL BFP_lprealint)(lprec* lp, REAL *pcol, int* nzidx);
        public delegate void BFP_lprealint(lprec lp, ref double pcol, int nzidx);
        // ORIGINAL LINE: typedef void   (BFP_CALLMODEL BFP_lprealintrealint)(lprec* lp, REAL *prow, int* pnzidx, REAL *drow, int* dnzidx);
        public delegate void BFP_lprealintrealint(lprec lp, ref double prow, int pnzidx, double drow, int dnzidx);
        // ORIGINAL LINE: typedef int    (BFP_CALLMODEL BFPint_lpbool)(lprec* lp, MYBOOL maximum);
        public delegate int BFPint_lpbool(lprec lp, byte maximum);
        // ORIGINAL LINE: typedef REAL(BFP_CALLMODEL BFPreal_lp)(lprec* lp);
        public delegate double BFPreal_lp(lprec lp);
        // ORIGINAL LINE: typedef REAL   *(BFP_CALLMODEL BFPrealp_lp)(lprec* lp);
        public delegate double BFPrealp_lp(lprec lp);
        // ORIGINAL LINE: typedef int    (BFP_CALLMODEL BFPint_lpint)(lprec* lp, int kind);
        public delegate int BFPint_lpint(lprec lp, int kind);
        // ORIGINAL LINE: typedef int    (BFP_CALLMODEL getcolumnex_func)(lprec* lp, int colnr, REAL *nzvalues, int* nzrows, int* mapin);
        public delegate int getcolumnex_func(lprec lp, int colnr, ref double nzvalues, ref int nzrows, ref int mapin);
        // ORIGINAL LINE: typedef int    (BFP_CALLMODEL BFPint_lpintrealcbintint)(lprec* lp, int items, getcolumnex_func cb, int* maprow, int* mapcol);
        public delegate int BFPint_lpintrealcbintint(lprec lp, int items, getcolumnex_func cb, ref int maprow, int mapcol);
        
        /* Refactorization engine interface routines (for object DLL/SO BFPs) */
        BFPchar bfp_name;
        BFPbool_lpintintint bfp_compatible;
        BFPbool_lpintintchar bfp_init;
        BFP_lp bfp_free;
        BFPbool_lpint bfp_resize;
        BFPint_lp bfp_memallocated;
        BFPbool_lp bfp_restart;
        BFPbool_lp bfp_mustrefactorize;
        BFPint_lp bfp_preparefactorization;
        BFPint_lpintintboolbool bfp_factorize;
        BFP_lp bfp_finishfactorization;
        BFP_lp bfp_updaterefactstats;
        BFPlreal_lpintintreal bfp_prepareupdate;
        BFPreal_lplrealreal bfp_pivotRHS;
        BFPbool_lpbool bfp_finishupdate;
        BFP_lprealint bfp_ftran_prepare;
        BFP_lprealint bfp_ftran_normal;
        BFP_lprealint bfp_btran_normal;
        BFP_lprealintrealint bfp_btran_double;
        BFPint_lp bfp_status;
        BFPint_lpbool bfp_nonzeros;
        BFPbool_lp bfp_implicitslack;
        BFPint_lp bfp_indexbase;
        BFPint_lp bfp_rowoffset;
        BFPint_lp bfp_pivotmax;
        BFPbool_lpint bfp_pivotalloc;
        BFPint_lp bfp_colcount;
        BFPbool_lp bfp_canresetbasis;
        BFPreal_lp bfp_efficiency;
        BFPrealp_lp bfp_pivotvector;
        BFPint_lp bfp_pivotcount;
        BFPint_lpint bfp_refactcount;
        BFPbool_lp bfp_isSetI;
        BFPint_lpintrealcbintint bfp_findredundant;

        /* Prototypes for external language libraries                                */
        /* ------------------------------------------------------------------------- */
        // ORIGINAL LINE: typedef char*(XLI_CALLMODEL XLIchar)(void);
        public delegate string XLIchar();
        // ORIGINAL LINE: typedef MYBOOL(XLI_CALLMODEL XLIbool_lpintintint)(lprec* lp, int size, int deltasize, int sizevar);
        public delegate byte XLIbool_lpintintint(lprec lp, int size, int deltasize, int sizevar);
        // ORIGINAL LINE: typedef MYBOOL(XLI_CALLMODEL XLIbool_lpcharcharcharint)(lprec* lp, char* modelname, char* dataname, char* options, int verbose);
        public delegate byte XLIbool_lpcharcharcharint(lprec lp, ref string modelname, ref string dataname, ref string options, int verbose);
        // ORIGINAL LINE: typedef MYBOOL(XLI_CALLMODEL XLIbool_lpcharcharbool)(lprec* lp, char* filename, char* options, MYBOOL results);
        public delegate byte XLIbool_lpcharcharbool(lprec lp, ref string filename, ref string options, byte results);

        /* External language interface routines (for object DLL/SO XLIs) */
        XLIchar xli_name;
        XLIbool_lpintintint xli_compatible;
        XLIbool_lpcharcharcharint xli_readmodel;
        XLIbool_lpcharcharbool xli_writemodel;

        /* Prototypes for callbacks from basis inverse/factorization libraries       */
        /* ------------------------------------------------------------------------- */
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI userabortfunc)(lprec* lp, int level);
        public delegate byte userabortfunc(lprec lp, int level);
        // ORIGINAL LINE: typedef void   (__VACALL reportfunc)(lprec* lp, int level, char* format, ...);
        //TODO: '...' is used to provide variable number of arguments to the function; ref: http://c-faq.com/varargs/varargs1.html
        public delegate void reportfunc(lprec lp, int level, ref string format, params object[] reportfuncvar);
        // ORIGINAL LINE: typedef char* (__VACALL explainfunc)(lprec* lp, char* format, ...);
        public delegate string explainfunc(lprec lp, int level, ref string format, params object[] explainfuncvar);
        // ORIGINAL LINE: typedef int    (__WINAPI getvectorfunc)(lprec* lp, int varin, REAL *pcol, int* nzlist, int* maxabs);
        public delegate int getvectorfunc(lprec lp, int varin, ref double pcol, ref int nzlist, ref int maxabs);
        // ORIGINAL LINE: typedef int    (__WINAPI getpackedfunc)(lprec* lp, int j, int rn[], double bj[]);
        public delegate int getpackedfunc(lprec lp, int j, int[] rn, double[] bj);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_OF_activefunc)(lprec* lp, int varnr, REAL mult);
        public delegate double get_OF_activefunc(lprec lp, int varnr, double mult);
        // ORIGINAL LINE: typedef int    (__WINAPI getMDOfunc)(lprec* lp, MYBOOL *usedpos, int* colorder, int* size, MYBOOL symmetric);
        public delegate int getMDOfunc(lprec lp, ref byte usedpos, ref int colorder, ref int size, byte symmetric);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI invertfunc)(lprec* lp, MYBOOL shiftbounds, MYBOOL final);
        public delegate byte invertfunc(lprec lp, byte shiftbounds, byte final);
        // ORIGINAL LINE: typedef void   (__WINAPI set_actionfunc)(int* actionvar, int actionmask);
        public delegate void set_actionfunc(ref int actionvar, int actionmask);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI is_actionfunc)(int actionvar, int testmask);
        public delegate byte is_actionfunc(int actionvar, int testmask);
        // ORIGINAL LINE: typedef void   (__WINAPI clear_actionfunc)(int* actionvar, int actionmask);
        public delegate void clear_actionfunc(ref int actionvar, int actionmask);

        /* Miscellaneous internal functions made available externally */
        userabortfunc userabort;
        internal reportfunc report;
        explainfunc explain;
        getvectorfunc get_lpcolumn;
        getpackedfunc get_basiscolumn;
        get_OF_activefunc get_OF_active;
        getMDOfunc getMDO;
        invertfunc invert;
        set_actionfunc set_action;
        is_actionfunc is_action;
        clear_actionfunc clear_action;

        /* Prototypes for user call-back functions                                   */
        /* ------------------------------------------------------------------------- */
        // ORIGINAL LINE: typedef int    (__WINAPI lphandle_intfunc)(lprec* lp, void* userhandle);
        public delegate int lphandle_intfunc(lprec lp, object userhandle);
        // ORIGINAL LINE: typedef void   (__WINAPI lphandlestr_func)(lprec* lp, void* userhandle, char* buf);
        public delegate void lphandlestr_func(lprec lp, object userhandle, ref string buf);
        // ORIGINAL LINE: typedef void   (__WINAPI lphandleint_func)(lprec* lp, void* userhandle, int message);
        public delegate void lphandleint_func(lprec lp, object userhandle, int message);
        // ORIGINAL LINE: typedef int    (__WINAPI lphandleint_intfunc)(lprec* lp, void* userhandle, int message);
        public delegate int lphandleint_intfunc(lprec lp, object userhandle, int message);
        

        /* API typedef definitions                                                   */
        /* ------------------------------------------------------------------------- */
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI add_column_func)(lprec* lp, REAL *column);
        public delegate byte add_column_func(lprec lp, ref double column);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI add_columnex_func)(lprec* lp, int count, REAL *column, int* rowno);
        public delegate byte add_columnex_func(lprec lp, int count, ref double column, ref int rowno);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI add_constraint_func)(lprec* lp, REAL *row, int constr_type, REAL rh);
        public delegate byte add_constraint_func(lprec lp, ref double row, int constr_type, double rh);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI add_constraintex_func)(lprec* lp, int count, REAL *row, int* colno, int constr_type, REAL rh);
        /// <summary>
        /// changed from 'ref double row' to 'ref double[] row' FIX_90b96e5c-2dba-4335-95bd-b1fcc95f1b55 19/11/18
        /// </summary>
        public delegate byte add_constraintex_func(lprec lp, int count, ref double[] row, ref int colno, int constr_type, double rh);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI add_lag_con_func)(lprec* lp, REAL *row, int con_type, REAL rhs);
        public delegate byte add_lag_con_func(lprec lp, ref double row, int con_type, double rhs);
        // ORIGINAL LINE: typedef int (__WINAPI add_SOS_func)(lprec* lp, char* name, int sostype, int priority, int count, int* sosvars, REAL *weights);
        public delegate int add_SOS_func(lprec lp, ref string name, int sostype, int priority, int count, ref int sosvars, ref double weights);
        // ORIGINAL LINE: typedef int (__WINAPI column_in_lp_func)(lprec* lp, REAL *column);
        public delegate int column_in_lp_func(lprec lp, ref double column);
        // ORIGINAL LINE: typedef lprec * (__WINAPI copy_lp_func)(lprec* lp);
        public delegate lprec copy_lp_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI default_basis_func)(lprec* lp);
        public delegate void default_basis_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI del_column_func)(lprec* lp, int colnr);
        public delegate byte del_column_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI del_constraint_func)(lprec* lp, int rownr);
        public delegate byte del_constraint_func(lprec lp, int rownr);
        // ORIGINAL LINE: typedef void (__WINAPI delete_lp_func)(lprec* lp);
        public delegate void delete_lp_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI dualize_lp_func)(lprec* lp);
        public delegate byte dualize_lp_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI free_lp_func)(lprec** plp);
        public delegate void free_lp_func(lprec[] plp);
        // ORIGINAL LINE: typedef int (__WINAPI get_anti_degen_func)(lprec* lp);
        public delegate int get_anti_degen_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_basis_func)(lprec* lp, int* bascolumn, MYBOOL nonbasic);
        public delegate byte get_basis_func(lprec lp, ref int bascolumn, byte nonbasic);
        // ORIGINAL LINE: typedef int (__WINAPI get_basiscrash_func)(lprec* lp);
        public delegate int get_basiscrash_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_bb_depthlimit_func)(lprec* lp);
        public delegate int get_bb_depthlimit_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_bb_floorfirst_func)(lprec* lp);
        public delegate int get_bb_floorfirst_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_bb_rule_func)(lprec* lp);
        public delegate int get_bb_rule_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_bounds_tighter_func)(lprec* lp);
        public delegate byte get_bounds_tighter_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_break_at_value_func)(lprec* lp);
        public delegate double get_break_at_value_func(lprec lp);
        // ORIGINAL LINE: typedef char* (__WINAPI get_col_name_func)(lprec* lp, int colnr);
        public delegate string get_col_name_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_column_func)(lprec* lp, int colnr, REAL *column);
        public delegate byte get_column_func(lprec lp, int colnr, ref double column);
        // ORIGINAL LINE: typedef int (__WINAPI get_columnex_func)(lprec* lp, int colnr, REAL *column, int* nzrow);
        public delegate int get_columnex_func(lprec lp, int colnr, ref double column, ref int nzrow);
        // ORIGINAL LINE: typedef int (__WINAPI get_constr_type_func)(lprec* lp, int rownr);
        public delegate int get_constr_type_func(lprec lp, int rownr);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_constr_value_func)(lprec* lp, int rownr, int count, REAL *primsolution, int* nzindex);
        public delegate double get_constr_value_func(lprec lp, int rownr, int count, ref double primsolution, ref int nzindex);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_constraints_func)(lprec* lp, REAL *constr);
        public delegate byte get_constraints_func(lprec lp, ref double constr);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_dual_solution_func)(lprec* lp, REAL *rc);
        public delegate byte get_dual_solution_func(lprec lp, ref double rc);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_epsb_func)(lprec* lp);
        public delegate double get_epsb_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_epsd_func)(lprec* lp);
        public delegate double get_epsd_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_epsel_func)(lprec* lp);
        public delegate double get_epsel_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_epsint_func)(lprec* lp);
        public delegate double get_epsint_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_epsperturb_func)(lprec* lp);
        public delegate double get_epsperturb_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_epspivot_func)(lprec* lp);
        public delegate double get_epspivot_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_improve_func)(lprec* lp);
        public delegate int get_improve_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_infinite_func)(lprec* lp);
        public delegate double get_infinite_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_lambda_func)(lprec* lp, REAL *lambda);
        public delegate byte get_lambda_func(lprec lp, ref double lambda);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_lowbo_func)(lprec* lp, int colnr);
        public delegate double get_lowbo_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef int (__WINAPI get_lp_index_func)(lprec* lp, int orig_index);
        public delegate int get_lp_index_func(lprec lp, int orig_index);
        // ORIGINAL LINE: typedef char* (__WINAPI get_lp_name_func)(lprec* lp);
        public delegate string get_lp_name_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_Lrows_func)(lprec* lp);
        public delegate int get_Lrows_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_mat_func)(lprec* lp, int rownr, int colnr);
        public delegate double get_mat_func(lprec lp, int rownr, int colnr);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_mat_byindex_func)(lprec* lp, int matindex, MYBOOL isrow, MYBOOL adjustsign);
        public delegate double get_mat_byindex_func(lprec lp, int matindex, byte isrow, byte adjustsign);
        // ORIGINAL LINE: typedef int (__WINAPI get_max_level_func)(lprec* lp);
        public delegate int get_max_level_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_maxpivot_func)(lprec* lp);
        public delegate int get_maxpivot_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_mip_gap_func)(lprec* lp, MYBOOL absolute);
        public delegate double get_mip_gap_func(lprec lp, byte absolute);
        // ORIGINAL LINE: typedef int (__WINAPI get_multiprice_func)(lprec* lp, MYBOOL getabssize);
        public delegate int get_multiprice_func(lprec lp, byte getabssize);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI is_use_names_func)(lprec* lp, MYBOOL isrow);
        public delegate byte is_use_names_func(lprec lp, byte isrow);
        // ORIGINAL LINE: typedef void (__WINAPI set_use_names_func)(lprec* lp, MYBOOL isrow, MYBOOL use_names);
        public delegate void set_use_names_func(lprec lp, byte isrow, byte use_names);
        // ORIGINAL LINE: typedef int (__WINAPI get_nameindex_func)(lprec* lp, char* varname, MYBOOL isrow);
        public delegate int get_nameindex_func(lprec lp, ref string varname, byte isrow);
        // ORIGINAL LINE: typedef int (__WINAPI get_Ncolumns_func)(lprec* lp);
        public delegate int get_Ncolumns_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_negrange_func)(lprec* lp);
        public delegate double get_negrange_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_nz_func)(lprec* lp);
        public delegate int get_nz_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_Norig_columns_func)(lprec* lp);
        public delegate int get_Norig_columns_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_Norig_rows_func)(lprec* lp);
        public delegate int get_Norig_rows_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_Nrows_func)(lprec* lp);
        public delegate int get_Nrows_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_obj_bound_func)(lprec* lp);
        public delegate double get_obj_bound_func(lprec lp);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_objective_func)(lprec* lp);
        public delegate double get_objective_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_orig_index_func)(lprec* lp, int lp_index);
        public delegate int get_orig_index_func(lprec lp, int lp_index);
        // ORIGINAL LINE: typedef char* (__WINAPI get_origcol_name_func)(lprec* lp, int colnr);
        public delegate string  get_origcol_name_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef char* (__WINAPI get_origrow_name_func)(lprec* lp, int rownr);
        public delegate string get_origrow_name_func(lprec lp, int rownr);
        // ORIGINAL LINE: typedef void (__WINAPI get_partialprice_func)(lprec* lp, int* blockcount, int* blockstart, MYBOOL isrow);
        public delegate void get_partialprice_func(lprec lp, ref int blockcount, ref int blockstart, byte isrow);
        // ORIGINAL LINE: typedef int (__WINAPI get_pivoting_func)(lprec* lp);
        public delegate int get_pivoting_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_presolve_func)(lprec* lp);
        public delegate int get_presolve_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_presolveloops_func)(lprec* lp);
        public delegate int get_presolveloops_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_primal_solution_func)(lprec* lp, REAL *pv);
        public delegate byte get_primal_solution_func(lprec lp, ref double pv);
        // ORIGINAL LINE: typedef int (__WINAPI get_print_sol_func)(lprec* lp);
        public delegate int get_print_sol_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_pseudocosts_func)(lprec* lp, REAL *clower, REAL* cupper, int* updatelimit);
        public delegate byte get_pseudocosts_func(lprec lp, ref double clower, ref double cupper, ref int updatelimit);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_ptr_constraints_func)(lprec* lp, REAL **constr);
        public delegate byte get_ptr_constraints_func(lprec lp, double[][] constr);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_ptr_dual_solution_func)(lprec* lp, REAL **rc);
        public delegate byte get_ptr_dual_solution_func(lprec lp, double[][] rc);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_ptr_lambda_func)(lprec* lp, REAL **lambda);
        public delegate byte get_ptr_lambda_func(lprec lp, double[][] lambda);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_ptr_primal_solution_func)(lprec* lp, REAL **pv);
        public delegate byte get_ptr_primal_solution_func(lprec lp, double[][] pv);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_ptr_sensitivity_obj_func)(lprec* lp, REAL **objfrom, REAL** objtill);
        public delegate byte get_ptr_sensitivity_obj_func(lprec lp, double[][] objfrom, double[][] objtill);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_ptr_sensitivity_objex_func)(lprec* lp, REAL **objfrom, REAL** objtill, REAL **objfromvalue, REAL** objtillvalue);
        public delegate byte get_ptr_sensitivity_objex_func(lprec lp, double[][] objfrom, double[][] objtill, double[][] objfromvalue, double[][] objtillvalue);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_ptr_sensitivity_rhs_func)(lprec* lp, REAL **duals, REAL** dualsfrom, REAL **dualstill);
        public delegate byte get_ptr_sensitivity_rhs_func(lprec lp, double[][] duals, double[][] dualsfrom, double[][] dualstill);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_ptr_variables_func)(lprec* lp, REAL **var);
        public delegate byte get_ptr_variables_func(lprec lp, double[][] var);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_rh_func)(lprec* lp, int rownr);
        public delegate double get_rh_func(lprec lp, int rownr);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_rh_range_func)(lprec* lp, int rownr);
        public delegate double get_rh_range_func(lprec lp, int rownr);
        // ORIGINAL LINE: typedef int (__WINAPI get_rowex_func)(lprec* lp, int rownr, REAL *row, int* colno);
        public delegate int get_rowex_func(lprec lp, int rownr, ref double row, ref int colno);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_row_func)(lprec* lp, int rownr, REAL *row);
        public delegate byte get_row_func(lprec lp, int rownr, ref double row);
        // ORIGINAL LINE: typedef char* (__WINAPI get_row_name_func)(lprec* lp, int rownr);
        public delegate string get_row_name_func(lprec lp, int rownr);
        // ORIGINAL LINE: typedef REAL(__WINAPI get_scalelimit_func)(lprec* lp);
        public delegate double get_scalelimit_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_scaling_func)(lprec* lp);
        public delegate int get_scaling_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_sensitivity_obj_func)(lprec* lp, REAL *objfrom, REAL* objtill);
        public delegate byte get_sensitivity_obj_func(lprec lp, ref double objfrom, ref double objtill);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_sensitivity_objex_func)(lprec* lp, REAL *objfrom, REAL* objtill, REAL *objfromvalue, REAL* objtillvalue);
        public delegate byte get_sensitivity_objex_func(lprec lp, ref double objfrom, ref double objtill, ref double objfromvalue, ref double objtillvalue);
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI get_sensitivity_rhs_func)(lprec* lp, REAL *duals, REAL* dualsfrom, REAL *dualstill);
        public delegate byte get_sensitivity_rhs_func(lprec lp, ref double duals, ref double dualsfrom, ref double dualstill);
        //TODO: converted from converter, need to check for correctness.
        // ORIGINAL LINE: typedef int (__WINAPI get_simplextype_func)(lprec *lp);
        public delegate int get_simplextype_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_solutioncount_func)(lprec *lp);
        public delegate int get_solutioncount_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_solutionlimit_func)(lprec *lp);
        public delegate int get_solutionlimit_func(lprec lp);
        // ORIGINAL LINE: typedef int (__WINAPI get_status_func)(lprec *lp);
        public delegate int get_status_func(lprec lp);
        // ORIGINAL LINE: typedef char * (__WINAPI get_statustext_func)(lprec *lp, int statuscode);
        public delegate string get_statustext_func(lprec lp, int statuscode);
        // ORIGINAL LINE: typedef long (__WINAPI get_timeout_func)(lprec *lp);
        public delegate long get_timeout_func(lprec lp);
        // ORIGINAL LINE: typedef COUNTER (__WINAPI get_total_iter_func)(lprec *lp);
        public delegate long get_total_iter_func(lprec lp);
        // ORIGINAL LINE: typedef COUNTER (__WINAPI get_total_nodes_func)(lprec *lp);
        public delegate long get_total_nodes_func(lprec lp);
        // ORIGINAL LINE: typedef REAL (__WINAPI get_upbo_func)(lprec *lp, int colnr);
        public delegate double get_upbo_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef int (__WINAPI get_var_branch_func)(lprec *lp, int colnr);
        public delegate int get_var_branch_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef REAL (__WINAPI get_var_dualresult_func)(lprec *lp, int index);
        public delegate double get_var_dualresult_func(lprec lp, int index);
        // ORIGINAL LINE: typedef REAL (__WINAPI get_var_primalresult_func)(lprec *lp, int index);
        public delegate double get_var_primalresult_func(lprec lp, int index);
        // ORIGINAL LINE: typedef int (__WINAPI get_var_priority_func)(lprec *lp, int colnr);
        public delegate int get_var_priority_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI get_variables_func)(lprec *lp, REAL *var);
        public delegate byte get_variables_func(lprec lp, ref double var);
        // ORIGINAL LINE: typedef int (__WINAPI get_verbose_func)(lprec *lp);
        public delegate int get_verbose_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI guess_basis_func)(lprec *lp, REAL *guessvector, int *basisvector);
        public delegate byte guess_basis_func(lprec lp, ref double guessvector, ref int basisvector);
        // ORIGINAL LINE: typedef REAL (__WINAPI get_working_objective_func)(lprec *lp);
        public delegate double get_working_objective_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI has_BFP_func)(lprec *lp);
        public delegate byte has_BFP_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI has_XLI_func)(lprec *lp);
        public delegate byte has_XLI_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_add_rowmode_func)(lprec *lp);
        public delegate byte is_add_rowmode_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_anti_degen_func)(lprec *lp, int testmask);
        public delegate byte is_anti_degen_func(lprec lp, int testmask);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_binary_func)(lprec *lp, int colnr);
        public delegate byte is_binary_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_break_at_first_func)(lprec *lp);
        public delegate byte is_break_at_first_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_constr_type_func)(lprec *lp, int rownr, int mask);
        public delegate byte is_constr_type_func(lprec lp, int rownr, int mask);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_debug_func)(lprec *lp);
        public delegate byte is_debug_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_feasible_func)(lprec *lp, REAL *values, REAL threshold);
        public delegate byte is_feasible_func(lprec lp, ref double values, double threshold);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_unbounded_func)(lprec *lp, int colnr);
        public delegate byte is_unbounded_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_infinite_func)(lprec *lp, REAL value);
        public delegate byte is_infinite_func(lprec lp, double value);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_int_func)(lprec *lp, int column);
        public delegate byte is_int_func(lprec lp, int column);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_integerscaling_func)(lprec *lp);
        public delegate byte is_integerscaling_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_lag_trace_func)(lprec *lp);
        public delegate byte is_lag_trace_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_maxim_func)(lprec *lp);
        public delegate byte is_maxim_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_nativeBFP_func)(lprec *lp);
        public delegate byte is_nativeBFP_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_nativeXLI_func)(lprec *lp);
        public delegate byte is_nativeXLI_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_negative_func)(lprec *lp, int colnr);
        public delegate byte is_negative_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_obj_in_basis_func)(lprec *lp);
        public delegate byte is_obj_in_basis_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_piv_mode_func)(lprec *lp, int testmask);
        public delegate byte is_piv_mode_func(lprec lp, int testmask);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_piv_rule_func)(lprec *lp, int rule);
        public delegate byte is_piv_rule_func(lprec lp, int rule);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_presolve_func)(lprec *lp, int testmask);
        public delegate byte is_presolve_func(lprec lp, int testmask);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_scalemode_func)(lprec *lp, int testmask);
        public delegate byte is_scalemode_func(lprec lp, int testmask);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_scaletype_func)(lprec *lp, int scaletype);
        public delegate byte is_scaletype_func(lprec lp, int scaletype);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_semicont_func)(lprec *lp, int colnr);
        public delegate bool is_semicont_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_SOS_var_func)(lprec *lp, int colnr);
        public delegate byte is_SOS_var_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI is_trace_func)(lprec *lp);
        public delegate byte is_trace_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI lp_solve_version_func)(int *majorversion, int *minorversion, int *release, int *build);
        public delegate void lp_solve_version_func(ref int majorversion, ref int minorversion, ref int release, ref int build);
        // ORIGINAL LINE: typedef lprec * (__WINAPI make_lp_func)(int rows, int columns);
        public delegate lprec make_lp_func(int rows, int columns);
        // ORIGINAL LINE: typedef void (__WINAPI print_constraints_func)(lprec *lp, int columns);
        public delegate void print_constraints_func(lprec lp, int columns);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI print_debugdump_func)(lprec *lp, char *filename);
        public delegate byte print_debugdump_func(lprec lp, ref string filename);
        // ORIGINAL LINE: typedef void (__WINAPI print_duals_func)(lprec *lp);
        public delegate void print_duals_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI print_lp_func)(lprec *lp);
        public delegate void print_lp_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI print_objective_func)(lprec *lp);
        public delegate void print_objective_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI print_scales_func)(lprec *lp);
        public delegate void print_scales_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI print_solution_func)(lprec *lp, int columns);
        public delegate void print_solution_func(lprec lp, int columns);
        // ORIGINAL LINE: typedef void (__WINAPI print_str_func)(lprec *lp, char *str);
        public delegate void print_str_func(lprec lp, ref string str);
        // ORIGINAL LINE: typedef void (__WINAPI print_tableau_func)(lprec *lp);
        public delegate void print_tableau_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI put_abortfunc_func)(lprec *lp, lphandle_intfunc newctrlc, void *ctrlchandle);
        public delegate void put_abortfunc_func(lprec lp, lphandle_intfunc newctrlc, object ctrlchandle);
        // ORIGINAL LINE: typedef void (__WINAPI put_bb_nodefunc_func)(lprec *lp, lphandleint_intfunc newnode, void *bbnodehandle);
        public delegate void put_bb_nodefunc_func(lprec lp, lphandleint_intfunc newnode, object bbnodehandle);
        // ORIGINAL LINE: typedef void (__WINAPI put_bb_branchfunc_func)(lprec *lp, lphandleint_intfunc newbranch, void *bbbranchhandle);
        public delegate void put_bb_branchfunc_func(lprec lp, lphandleint_intfunc newbranch, object bbbranchhandle);
        // ORIGINAL LINE: typedef void (__WINAPI put_logfunc_func)(lprec *lp, lphandlestr_func newlog, void *loghandle);
        public delegate void put_logfunc_func(lprec lp, lphandlestr_func newlog, object loghandle);
        // ORIGINAL LINE: typedef void (__WINAPI put_msgfunc_func)(lprec *lp, lphandleint_func newmsg, void *msghandle, int mask);
        public delegate void put_msgfunc_func(lprec lp, lphandleint_func newmsg, object msghandle, int mask);
        // ORIGINAL LINE: typedef lprec * (__WINAPI read_LP_func)(char *filename, int verbose, char *lp_name);
        public delegate lprec read_LP_func(ref string filename, int verbose, ref string lp_name);
        // ORIGINAL LINE: typedef lprec * (__WINAPI read_MPS_func)(char *filename, int options);
        public delegate lprec read_MPS_func(ref string filename, int options);
        // ORIGINAL LINE: typedef lprec * (__WINAPI read_XLI_func)(char *xliname, char *modelname, char *dataname, char *options, int verbose);
        public delegate lprec read_XLI_func(ref string xliname, ref string modelname, ref string dataname, ref string options, int verbose);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI read_basis_func)(lprec *lp, char *filename, char *info);
        public delegate byte read_basis_func(lprec lp, ref string filename, ref string info);
        // ORIGINAL LINE: typedef void (__WINAPI reset_basis_func)(lprec *lp);
        public delegate void reset_basis_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI read_params_func)(lprec *lp, char *filename, char *options);
        public delegate byte read_params_func(lprec lp, ref string filename, ref string options);
        // ORIGINAL LINE: typedef void (__WINAPI reset_params_func)(lprec *lp);
        public delegate void reset_params_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI resize_lp_func)(lprec *lp, int rows, int columns);
        public delegate byte resize_lp_func(lprec lp, int rows, int columns);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_add_rowmode_func)(lprec *lp, MYBOOL turnon);
        public delegate byte set_add_rowmode_func(lprec lp, byte turnon);
        // ORIGINAL LINE: typedef void (__WINAPI set_anti_degen_func)(lprec *lp, int anti_degen);
        public delegate void set_anti_degen_func(lprec lp, int anti_degen);
        // ORIGINAL LINE: typedef int  (__WINAPI set_basisvar_func)(lprec *lp, int basisPos, int enteringCol);
        public delegate int set_basisvar_func(lprec lp, int basisPos, int enteringCol);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_basis_func)(lprec *lp, int *bascolumn, MYBOOL nonbasic);
        public delegate byte set_basis_func(lprec lp, ref int bascolumn, byte nonbasic);
        // ORIGINAL LINE: typedef void (__WINAPI set_basiscrash_func)(lprec *lp, int mode);
        public delegate void set_basiscrash_func(lprec lp, int mode);
        // ORIGINAL LINE: typedef void (__WINAPI set_bb_depthlimit_func)(lprec *lp, int bb_maxlevel);
        public delegate void set_bb_depthlimit_func(lprec lp, int bb_maxlevel);
        // ORIGINAL LINE: typedef void (__WINAPI set_bb_floorfirst_func)(lprec *lp, int bb_floorfirst);
        public delegate void set_bb_floorfirst_func(lprec lp, int bb_floorfirst);
        // ORIGINAL LINE: typedef void (__WINAPI set_bb_rule_func)(lprec *lp, int bb_rule);
        public delegate void set_bb_rule_func(lprec lp, int bb_rule);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_BFP_func)(lprec *lp, char *filename);
        public delegate byte set_BFP_func(lprec lp, ref string filename);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_binary_func)(lprec *lp, int colnr, MYBOOL must_be_bin);
        public delegate byte set_binary_func(lprec lp, int colnr, byte must_be_bin);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_bounds_func)(lprec *lp, int colnr, REAL lower, REAL upper);
        public delegate byte set_bounds_func(lprec lp, int colnr, double lower, double upper);
        // ORIGINAL LINE: typedef void (__WINAPI set_bounds_tighter_func)(lprec *lp, MYBOOL tighten);
        public delegate void set_bounds_tighter_func(lprec lp, byte tighten);
        // ORIGINAL LINE: typedef void (__WINAPI set_break_at_first_func)(lprec *lp, MYBOOL break_at_first);
        public delegate void set_break_at_first_func(lprec lp, byte break_at_first);
        // ORIGINAL LINE: typedef void (__WINAPI set_break_at_value_func)(lprec *lp, REAL break_at_value);
        public delegate void set_break_at_value_func(lprec lp, double break_at_value);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_column_func)(lprec *lp, int colnr, REAL *column);
        public delegate byte set_column_func(lprec lp, int colnr, ref double column);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_columnex_func)(lprec *lp, int colnr, int count, REAL *column, int *rowno);
        public delegate byte set_columnex_func(lprec lp, int colnr, int count, ref double column, ref int rowno);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_col_name_func)(lprec *lp, int colnr, char *new_name);
        public delegate byte set_col_name_func(lprec lp, int colnr, ref string new_name);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_constr_type_func)(lprec *lp, int rownr, int con_type);
        public delegate byte set_constr_type_func(lprec lp, int rownr, int con_type);
        // ORIGINAL LINE: typedef void (__WINAPI set_debug_func)(lprec *lp, MYBOOL debug);
        public delegate void set_debug_func(lprec lp, byte debug);
        // ORIGINAL LINE: typedef void (__WINAPI set_epsb_func)(lprec *lp, REAL epsb);
        public delegate void set_epsb_func(lprec lp, double epsb);
        // ORIGINAL LINE: typedef void (__WINAPI set_epsd_func)(lprec *lp, REAL epsd);
        public delegate void set_epsd_func(lprec lp, double epsd);
        // ORIGINAL LINE: typedef void (__WINAPI set_epsel_func)(lprec *lp, REAL epsel);
        public delegate void set_epsel_func(lprec lp, double epsel);
        // ORIGINAL LINE: typedef void (__WINAPI set_epsint_func)(lprec *lp, REAL epsint);
        public delegate void set_epsint_func(lprec lp, double epsint);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_epslevel_func)(lprec *lp, int epslevel);
        public delegate byte set_epslevel_func(lprec lp, int epslevel);
        // ORIGINAL LINE: typedef void (__WINAPI set_epsperturb_func)(lprec *lp, REAL epsperturb);
        public delegate void set_epsperturb_func(lprec lp, double epsperturb);
        // ORIGINAL LINE: typedef void (__WINAPI set_epspivot_func)(lprec *lp, REAL epspivot);
        public delegate void set_epspivot_func(lprec lp, double epspivot);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_unbounded_func)(lprec *lp, int colnr);
        public delegate byte set_unbounded_func(lprec lp, int colnr);
        // ORIGINAL LINE: typedef void (__WINAPI set_improve_func)(lprec *lp, int improve);
        public delegate void set_improve_func(lprec lp, int improve);
        // ORIGINAL LINE: typedef void (__WINAPI set_infinite_func)(lprec *lp, REAL infinite);
        public delegate void set_infinite_func(lprec lp, double infinite);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_int_func)(lprec *lp, int colnr, MYBOOL must_be_int);
        public delegate byte set_int_func(lprec lp, int colnr, byte must_be_int);
        // ORIGINAL LINE: typedef void (__WINAPI set_lag_trace_func)(lprec *lp, MYBOOL lag_trace);
        public delegate void set_lag_trace_func(lprec lp, byte lag_trace);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_lowbo_func)(lprec *lp, int colnr, REAL value);
        public delegate byte set_lowbo_func(lprec lp, int colnr, double value);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_lp_name_func)(lprec *lp, char *lpname);
        public delegate byte set_lp_name_func(lprec lp, ref string lpname);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_mat_func)(lprec *lp, int row, int column, REAL value);
        public delegate byte set_mat_func(lprec lp, int row, int column, double value);
        // ORIGINAL LINE: typedef void (__WINAPI set_maxim_func)(lprec *lp);
        public delegate void set_maxim_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI set_maxpivot_func)(lprec *lp, int max_num_inv);
        public delegate void set_maxpivot_func(lprec lp, int max_num_inv);
        // ORIGINAL LINE: typedef void (__WINAPI set_minim_func)(lprec *lp);
        public delegate void set_minim_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI set_mip_gap_func)(lprec *lp, MYBOOL absolute, REAL mip_gap);
        public delegate void set_mip_gap_func(lprec lp, byte absolute, double mip_gap);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_multiprice_func)(lprec *lp, int multiblockdiv);
        public delegate byte set_multiprice_func(lprec lp, int multiblockdiv);
        // ORIGINAL LINE: typedef void (__WINAPI set_negrange_func)(lprec *lp, REAL negrange);
        public delegate void set_negrange_func(lprec lp, double negrange);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_obj_func)(lprec *lp, int colnr, REAL value);
        public delegate byte set_obj_func(lprec lp, int colnr, double value);
        // ORIGINAL LINE: typedef void (__WINAPI set_obj_bound_func)(lprec *lp, REAL obj_bound);
        public delegate void set_obj_bound_func(lprec lp, double obj_bound);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_obj_fn_func)(lprec *lp, REAL *row);
        public delegate byte set_obj_fn_func(lprec lp, ref double row);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_obj_fnex_func)(lprec *lp, int count, REAL *row, int *colno);
        public delegate byte set_obj_fnex_func(lprec lp, int count, ref double row, ref int colno);
        // ORIGINAL LINE: typedef void (__WINAPI set_obj_in_basis_func)(lprec *lp, MYBOOL obj_in_basis);
        public delegate void set_obj_in_basis_func(lprec lp, byte obj_in_basis);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_outputfile_func)(lprec *lp, char *filename);
        public delegate byte set_outputfile_func(lprec lp, ref string filename);
        // ORIGINAL LINE: typedef void (__WINAPI set_outputstream_func)(lprec *lp, FILE *stream);
        public delegate void set_outputstream_func(lprec lp, FILE stream);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_partialprice_func)(lprec *lp, int blockcount, int *blockstart, MYBOOL isrow);
        public delegate byte set_partialprice_func(lprec lp, int blockcount, ref int blockstart, byte isrow);
        // ORIGINAL LINE: typedef void (__WINAPI set_pivoting_func)(lprec *lp, int piv_rule);
        public delegate void set_pivoting_func(lprec lp, int piv_rule);
        // ORIGINAL LINE: typedef void (__WINAPI set_preferdual_func)(lprec *lp, MYBOOL dodual);
        public delegate void set_preferdual_func(lprec lp, byte dodual);
        // ORIGINAL LINE: typedef void (__WINAPI set_presolve_func)(lprec *lp, int presolvemode, int maxloops);
        public delegate void set_presolve_func(lprec lp, int presolvemode, int maxloops);
        // ORIGINAL LINE: typedef void (__WINAPI set_print_sol_func)(lprec *lp, int print_sol);
        public delegate void set_print_sol_func(lprec lp, int print_sol);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_pseudocosts_func)(lprec *lp, REAL *clower, REAL *cupper, int *updatelimit);
        public delegate byte set_pseudocosts_func(lprec lp, ref double clower, ref double cupper, ref int updatelimit);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_rh_func)(lprec *lp, int rownr, REAL value);
        public delegate byte set_rh_func(lprec lp, int rownr, double value);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_rh_range_func)(lprec *lp, int rownr, REAL deltavalue);
        public delegate byte set_rh_range_func(lprec lp, int rownr, double deltavalue);
        // ORIGINAL LINE: typedef void (__WINAPI set_rh_vec_func)(lprec *lp, REAL *rh);
        public delegate void set_rh_vec_func(lprec lp, ref double rh);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_row_func)(lprec *lp, int rownr, REAL *row);
        public delegate byte set_row_func(lprec lp, int rownr, ref double row);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_rowex_func)(lprec *lp, int rownr, int count, REAL *row, int *colno);
        public delegate byte set_rowex_func(lprec lp, int rownr, int count, ref double row, ref int colno);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_row_name_func)(lprec *lp, int rownr, char *new_name);
        public delegate byte set_row_name_func(lprec lp, int rownr, ref string new_name);
        // ORIGINAL LINE: typedef void (__WINAPI set_scalelimit_func)(lprec *lp, REAL scalelimit);
        public delegate void set_scalelimit_func(lprec lp, double scalelimit);
        // ORIGINAL LINE: typedef void (__WINAPI set_scaling_func)(lprec *lp, int scalemode);
        public delegate void set_scaling_func(lprec lp, int scalemode);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_semicont_func)(lprec *lp, int colnr, MYBOOL must_be_sc);
        public delegate byte set_semicont_func(lprec lp, int colnr, byte must_be_sc);
        // ORIGINAL LINE: typedef void (__WINAPI set_sense_func)(lprec *lp, MYBOOL maximize);
        public delegate void set_sense_func(lprec lp, byte maximize);
        // ORIGINAL LINE: typedef void (__WINAPI set_simplextype_func)(lprec *lp, int simplextype);
        public delegate void set_simplextype_func(lprec lp, int simplextype);
        // ORIGINAL LINE: typedef void (__WINAPI set_solutionlimit_func)(lprec *lp, int limit);
        public delegate void set_solutionlimit_func(lprec lp, int limit);
        // ORIGINAL LINE: typedef void (__WINAPI set_timeout_func)(lprec *lp, long sectimeout);
        public delegate void set_timeout_func(lprec lp, long sectimeout);
        // ORIGINAL LINE: typedef void (__WINAPI set_trace_func)(lprec *lp, MYBOOL trace);
        public delegate void set_trace_func(lprec lp, byte trace);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_upbo_func)(lprec *lp, int colnr, REAL value);
        public delegate byte set_upbo_func(lprec lp, int colnr, double value);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_var_branch_func)(lprec *lp, int colnr, int branch_mode);
        public delegate byte set_var_branch_func(lprec lp, int colnr, int branch_mode);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_var_weights_func)(lprec *lp, REAL *weights);
        public delegate byte set_var_weights_func(lprec lp, ref double weights);
        // ORIGINAL LINE: typedef void (__WINAPI set_verbose_func)(lprec *lp, int verbose);
        public delegate void set_verbose_func(lprec lp, int verbose);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI set_XLI_func)(lprec *lp, char *filename);
        public delegate byte set_XLI_func(lprec lp, ref string filename);
        // ORIGINAL LINE: typedef int (__WINAPI solve_func)(lprec *lp);
        public delegate int solve_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI str_add_column_func)(lprec *lp, char *col_string);
        public delegate byte str_add_column_func(lprec lp, ref string col_string);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI str_add_constraint_func)(lprec *lp, char *row_string ,int constr_type, REAL rh);
        public delegate byte str_add_constraint_func(lprec lp, ref string row_string, int constr_type, double rh);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI str_add_lag_con_func)(lprec *lp, char *row_string, int con_type, REAL rhs);
        public delegate byte str_add_lag_con_func(lprec lp, ref string row_string, int con_type, double rhs);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI str_set_obj_fn_func)(lprec *lp, char *row_string);
        public delegate byte str_set_obj_fn_func(lprec lp, ref string row_string);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI str_set_rh_vec_func)(lprec *lp, char *rh_string);
        public delegate byte str_set_rh_vec_func(lprec lp, ref string rh_string);
        // ORIGINAL LINE: typedef REAL (__WINAPI time_elapsed_func)(lprec *lp);
        public delegate double time_elapsed_func(lprec lp);
        // ORIGINAL LINE: typedef void (__WINAPI unscale_func)(lprec *lp);
        public delegate void unscale_func(lprec lp);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI write_lp_func)(lprec *lp, char *filename);
        public delegate byte write_lp_func(lprec lp, ref string filename);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI write_LP_func)(lprec *lp, FILE *output);
        public delegate byte write_LP_func(lprec lp, FILE output);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI write_mps_func)(lprec *lp, char *filename);
        public delegate byte write_mps_func(lprec lp, ref string filename);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI write_MPS_func)(lprec *lp, FILE *output);
        public delegate byte write_MPS_func(lprec lp, FILE output);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI write_freemps_func)(lprec *lp, char *filename);
        public delegate byte write_freemps_func(lprec lp, ref string filename);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI write_freeMPS_func)(lprec *lp, FILE *output);
        public delegate byte write_freeMPS_func(lprec lp, FILE output);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI write_XLI_func)(lprec *lp, char *filename, char *options, MYBOOL results);
        public delegate byte write_XLI_func(lprec lp, ref string filename, ref string options, byte results);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI write_basis_func)(lprec *lp, char *filename);
        public delegate byte write_basis_func(lprec lp, ref string filename);
        // ORIGINAL LINE: typedef MYBOOL (__WINAPI write_params_func)(lprec *lp, char *filename, char *options);
        public delegate byte write_params_func(lprec lp, ref string filename, ref string options);

        /* User program interface callbacks */
        /// <summary>
        /// changed access modifier to internal due to inaccessibility 6/11/18
        /// </summary>
        internal lphandle_intfunc ctrlc; //TODO: delegate instnace??
        internal object ctrlchandle;     /* User-specified "owner process ID" */
        internal lphandlestr_func writelog;
        internal object loghandle;       // User-specified "owner process ID"
        internal lphandlestr_func debuginfo;
        /// <summary>
        /// changed access modifier to internal due to inaccessibility 6/11/18
        /// </summary>
        internal lphandleint_func usermessage;
        internal int msgmask;
        internal object msghandle;       /* User-specified "owner process ID" */
        internal lphandleint_intfunc bb_usenode;
        internal object bb_nodehandle; // User-specified "owner process ID"
        internal lphandleint_intfunc bb_usebranch;
        internal object bb_branchhandle; // User-specified "owner process ID"





        /* replacement of static variables */
        internal string rowcol_name;       /* The name of a row/column */

    }

    public class lp_lib
    {
        public const bool FULLYBOUNDEDSIMPLEX = false;
        public const bool LoadInverseLib = true;
        public const bool DEF_OBJINBASIS = true;
        public const bool LoadLanguageLib = true;
        public const bool ExcludeNativeLanguage = false;
        public const int DoMatrixRounding = 0; // Round A matrix elements to precision
        public const int DoBorderRounding = 0; // Round RHS, bounds and ranges to precision
        public const int Phase1EliminateRedundant = 0; // Remove rows of redundant artificials
        public const int FixViolatedOptimal = 0;
        public const int ImproveSolutionPrecision = 0; // Round optimal solution values
        public const int ResetMinitOnReinvert = 0;
        public const int UseMilpExpandedRCF = 0; // Non-ints in reduced cost bound tightening
        public const int LegacySlackDefinition = 0; // Slack as the "value of the constraint"
        public const int libBLAS = 2; // 0: No, 1: Internal, 2: External
        public const string libnameBLAS = "myBLAS";
        public const int INVERSE_NONE = -1;
        public const int INVERSE_LEGACY = 0;
        public const int INVERSE_ETAPFI = 1;
        public const int INVERSE_LUMOD = 2;
        public const int INVERSE_LUSOL = 3;
        public const int INVERSE_GLPKLU = 4;

        /* Active external language interface logic (default is none)                */
        /* ------------------------------------------------------------------------- */
        public const int LANGUAGE_NONE = -1;
        public const int LANGUAGE_LEGACYLP = 0;
        public const int LANGUAGE_CPLEXLP = 1;
        public const int LANGUAGE_MPSX = 2;
        public const int LANGUAGE_LPFML = 3;
        public const int LANGUAGE_MATHPROG = 4;
        public const int LANGUAGE_AMPL = 5;
        public const int LANGUAGE_GAMS = 6;
        public const int LANGUAGE_ZIMPL = 7;
        public const int LANGUAGE_S = 8;
        public const int LANGUAGE_R = 9;
        public const int LANGUAGE_MATLAB = 10;
        public const int LANGUAGE_OMATRIX = 11;
        public const int LANGUAGE_SCILAB = 12;
        public const int LANGUAGE_OCTAVE = 13;
        public const int LANGUAGE_EMPS = 14;

        /* Default parameters and tolerances                                         */
        /* ------------------------------------------------------------------------- */
        public const int OriginalPARAM = 0;
        public const int ProductionPARAM = 1;
        public const int ChvatalPARAM = 2;
        public const int LoosePARAM = 3;
        public const int ActivePARAM = ProductionPARAM;


        /* Program version data                                                      */
        /* ------------------------------------------------------------------------- */
        public const int MAJORVERSION = 5;
        public const int MINORVERSION = 5;
        public const int RELEASE = 2;
        public const int BUILD = 0;
        public const int BFPVERSION = 12; // Checked against bfp_compatible()
        public const int XLIVERSION = 12; // Checked against xli_compatible()

        /* Definition of program constrants                                          */
        /* ------------------------------------------------------------------------- */
        public const int SIMPLEX_UNDEFINED = 0;
        public const int SIMPLEX_Phase1_PRIMAL = 1;
        public const int SIMPLEX_Phase1_DUAL = 2;
        public const int SIMPLEX_Phase2_PRIMAL = 4;
        public const int SIMPLEX_Phase2_DUAL = 8;
        public const int SIMPLEX_DYNAMIC = 16;
        public const int SIMPLEX_AUTODUALIZE = 32;

        public const int SIMPLEX_PRIMAL_PRIMAL = (SIMPLEX_Phase1_PRIMAL + SIMPLEX_Phase2_PRIMAL);
        public const int SIMPLEX_DUAL_PRIMAL = (SIMPLEX_Phase1_DUAL + SIMPLEX_Phase2_PRIMAL);
        public const int SIMPLEX_PRIMAL_DUAL = (SIMPLEX_Phase1_PRIMAL + SIMPLEX_Phase2_DUAL);
        public const int SIMPLEX_DUAL_DUAL = (SIMPLEX_Phase1_DUAL + SIMPLEX_Phase2_DUAL);
        public const int SIMPLEX_DEFAULT = (SIMPLEX_DUAL_PRIMAL);

        /* Variable codes (internal) */
        public const int ISREAL = 0;
        /// <summary>
        /// changed from int to bool on 14/11/18
        /// </summary>
        public const bool ISINTEGER = true;
        public const int ISSEMI = 2;
        public const int ISSOS = 4;
        public const int ISSOSTEMPINT = 8;
        public const int ISGUB = 16;

        /* Presolve defines */
        public const int PRESOLVE_NONE = 0;
        public const int PRESOLVE_ROWS = 1;
        public const int PRESOLVE_COLS = 2;
        public const int PRESOLVE_LINDEP = 4;
        public const int PRESOLVE_AGGREGATE = 8; // Not implemented
        public const int PRESOLVE_SPARSER = 16; // Not implemented
        public const int PRESOLVE_SOS = 32;
        public const int PRESOLVE_REDUCEMIP = 64;
        public const int PRESOLVE_KNAPSACK = 128; // Implementation not tested completely
        public const int PRESOLVE_ELIMEQ2 = 256;
        public const int PRESOLVE_IMPLIEDFREE = 512;
        public const int PRESOLVE_REDUCEGCD = 1024;
        public const int PRESOLVE_PROBEFIX = 2048;
        public const int PRESOLVE_PROBEREDUCE = 4096;
        public const int PRESOLVE_ROWDOMINATE = 8192;
        public const int PRESOLVE_COLDOMINATE = 16384; // Reduced functionality, should be expanded
        public const int PRESOLVE_MERGEROWS = 32768;
        public const int PRESOLVE_IMPLIEDSLK = 65536;
        public const int PRESOLVE_COLFIXDUAL = 131072;
        public const int PRESOLVE_BOUNDS = 262144;
        public const int PRESOLVE_DUALS = 524288;
        public const int PRESOLVE_SENSDUALS = 1048576;

        /* Basis crash options */
        public const int CRASH_NONE = 0;
        public const int CRASH_NONBASICBOUNDS = 1;
        public const int CRASH_MOSTFEASIBLE = 2;
        public const int CRASH_LEASTDEGENERATE = 3;

        /* Solution recomputation options (internal) */
        public const int INITSOL_SHIFTZERO = 0;
        public const int INITSOL_USEZERO = 1;
        public const int INITSOL_ORIGINAL = 2;

        /* Strategy codes to avoid or recover from degenerate pivots,
         * infeasibility or numeric errors via randomized bound relaxation */
        public const int ANTIDEGEN_NONE = 0;
        public const int ANTIDEGEN_FIXEDVARS = 1;
        public const int ANTIDEGEN_COLUMNCHECK = 2;
        public const int ANTIDEGEN_STALLING = 4;
        public const int ANTIDEGEN_NUMFAILURE = 8;
        public const int ANTIDEGEN_LOSTFEAS = 16;
        public const int ANTIDEGEN_INFEASIBLE = 32;
        public const int ANTIDEGEN_DYNAMIC = 64;
        public const int ANTIDEGEN_DURINGBB = 128;
        public const int ANTIDEGEN_RHSPERTURB = 256;
        internal const int ANTIDEGEN_BOUNDFLIP = 512;
        public const int ANTIDEGEN_DEFAULT = (ANTIDEGEN_FIXEDVARS | ANTIDEGEN_STALLING);

        /* REPORT defines */
        public const int NEUTRAL = 0;
        public const int CRITICAL = 1;
        public const int SEVERE = 2;
        public const int IMPORTANT = 3;
        public const int NORMAL = 4;
        public const int DETAILED = 5;
        public const int FULL = 6;

        /* MESSAGE defines */
        public const int MSG_NONE = 0;
        public const int MSG_PRESOLVE = 1;
        public const int MSG_ITERATION = 2;
        public const int MSG_INVERT = 4;
        public const int MSG_LPFEASIBLE = 8;
        public const int MSG_LPOPTIMAL = 16;
        public const int MSG_LPEQUAL = 32;
        public const int MSG_LPBETTER = 64;
        public const int MSG_MILPFEASIBLE = 128;
        public const int MSG_MILPEQUAL = 256;
        public const int MSG_MILPBETTER = 512;
        public const int MSG_MILPSTRATEGY = 1024;
        public const int MSG_MILPOPTIMAL = 2048;
        public const int MSG_PERFORMANCE = 4096;
        public const int MSG_INITPSEUDOCOST = 8192;

        /* MPS file types */
        public const int MPSFIXED = 1;
        public const int MPSFREE = 2;
        public const int MPSIBM = 4;
        public const int MPSNEGOBJCONST = 8;

        public const int MPS_FREE = (MPSFREE << 2);
        public const int MPS_IBM = (MPSIBM << 2);
        public const int MPS_NEGOBJCONST = (MPSNEGOBJCONST << 2);

        /* MPS defines (internal) */
        public const int MPSUNDEF = -4;
        public const int MPSNAME = -3;
        public const int MPSOBJSENSE = -2;
        public const int MPSOBJNAME = -1;
        public const int MPSROWS = 0;
        public const int MPSCOLUMNS = 1;
        public const int MPSRHS = 2;
        public const int MPSBOUNDS = 3;
        public const int MPSRANGES = 4;
        public const int MPSSOS = 5;

        public const int BUFSIZ = 512;




        public const string MPSVARMASK = "%-8s";
        public const string MPSVALUEMASK = "%12g";

        /* Constraint type codes  (internal) */
        public const int ROWTYPE_EMPTY = 0;
        public const int ROWTYPE_LE = 1;
        public const int ROWTYPE_GE = 2;
        public const int ROWTYPE_EQ = 3;
        public const int ROWTYPE_CONSTRAINT = ROWTYPE_EQ;  /* This is the mask for modes */
        public const int ROWTYPE_OF = 4;
        public const int ROWTYPE_INACTIVE = 8;
        public const int ROWTYPE_RELAX = 16;
        public const int ROWTYPE_GUB = 32;
        public const int ROWTYPE_OFMAX = (ROWTYPE_OF + ROWTYPE_GE);
        public const int ROWTYPE_OFMIN = (ROWTYPE_OF + ROWTYPE_LE);
        public const int ROWTYPE_CHSIGN = ROWTYPE_GE;

        /* Public constraint codes */
        public const int FR = ROWTYPE_EMPTY;
        public const int LE = ROWTYPE_LE;
        public const int GE = ROWTYPE_GE;
        public const int EQ = ROWTYPE_EQ;
        public const int OF = ROWTYPE_OF;

        /* MIP constraint classes */
        public const int ROWCLASS_Unknown = 0;  /* Undefined/unknown */
        public const int ROWCLASS_Objective = 1;  /* The objective function */
        public const int ROWCLASS_GeneralREAL = 2;  /* General real-values constraint */
        public const int ROWCLASS_GeneralMIP = 3;  /* General mixed integer/binary and real valued constraint */
        public const int ROWCLASS_GeneralINT = 4;  /* General integer-only constraint */
        public const int ROWCLASS_GeneralBIN = 5;  /* General binary-only constraint */
        public const int ROWCLASS_KnapsackINT = 6;  /* Sum of positive integer times integer variables <= positive integer */
        public const int ROWCLASS_KnapsackBIN = 7;  /* Sum of positive integer times binary variables <= positive integer */
        public const int ROWCLASS_SetPacking = 8;  /* Sum of binary variables >= 1 */
        public const int ROWCLASS_SetCover = 9;  /* Sum of binary variables <= 1 */
        public const int ROWCLASS_GUB = 10;  /* Sum of binary variables = 1  */
        public const int ROWCLASS_MAX = ROWCLASS_GUB;

        /* Column subsets (internal) */
        public const int SCAN_USERVARS = 1;
        public const int SCAN_SLACKVARS = 2;
        public const int SCAN_ARTIFICIALVARS = 4;
        public const int SCAN_PARTIALBLOCK = 8;
        public const int USE_BASICVARS = 16;
        public const int USE_NONBASICVARS = 32;
        public const int SCAN_NORMALVARS = (SCAN_USERVARS + SCAN_ARTIFICIALVARS);
        public const int SCAN_ALLVARS = (SCAN_SLACKVARS + SCAN_USERVARS + SCAN_ARTIFICIALVARS);
        public const int USE_ALLVARS = (USE_BASICVARS + USE_NONBASICVARS);
        public const int OMIT_FIXED = 64;
        public const int OMIT_NONFIXED = 128;

        /* Improvement defines */
        public const int IMPROVE_NONE = 0;
        public const int IMPROVE_SOLUTION = 1;
        public const int IMPROVE_DUALFEAS = 2;
        public const int IMPROVE_THETAGAP = 4;
        public const int IMPROVE_BBSIMPLEX = 8;
        public const int IMPROVE_DEFAULT = (IMPROVE_DUALFEAS + IMPROVE_THETAGAP);
        public const int IMPROVE_INVERSE = (IMPROVE_SOLUTION + IMPROVE_THETAGAP);


        /* Scaling types */
        public const int SCALE_NONE = 0;
        public const int SCALE_EXTREME = 1;
        public const int SCALE_RANGE = 2;
        public const int SCALE_MEAN = 3;
        public const int SCALE_GEOMETRIC = 4;
        public const int SCALE_FUTURE1 = 5;
        public const int SCALE_FUTURE2 = 6;
        public const int SCALE_CURTISREID = 7;  /* Override to Curtis-Reid "optimal" scaling */

        /* Alternative scaling weights */
        public const int SCALE_LINEAR = 0;
        public const int SCALE_QUADRATIC = 8;
        public const int SCALE_LOGARITHMIC = 16;
        public const int SCALE_USERWEIGHT = 31;
        public const int SCALE_MAXTYPE = (SCALE_QUADRATIC - 1);

        /* Scaling modes */
        public const int SCALE_POWER2 = 32; /* As is or rounded to power of 2 */
        public const int SCALE_EQUILIBRATE = 64; /* Make sure that no scaled number is above 1 */
        public const int SCALE_INTEGERS = 128; /* Apply to integer columns/variables */
        public const int SCALE_DYNUPDATE = 256; /* Apply incrementally every solve() */
        public const int SCALE_ROWSONLY = 512; /* Override any scaling to only scale the rows */
        public const int SCALE_COLSONLY = 1024; /* Override any scaling to only scale the rows */

        /* Standard defines for typical scaling models (no Lagrangeans) */
        public const int SCALEMODEL_EQUILIBRATED = (SCALE_LINEAR + SCALE_EXTREME + SCALE_INTEGERS);
        public const int SCALEMODEL_GEOMETRIC = (SCALE_LINEAR + SCALE_GEOMETRIC + SCALE_INTEGERS);
        public const int SCALEMODEL_ARITHMETIC = (SCALE_LINEAR + SCALE_MEAN + SCALE_INTEGERS);
        public const int SCALEMODEL_DYNAMIC = (SCALEMODEL_GEOMETRIC + SCALE_EQUILIBRATE);
        public const int SCALEMODEL_CURTISREID = (SCALE_CURTISREID + SCALE_INTEGERS + SCALE_POWER2);

        /* Iteration status and strategies (internal) */
        //public const int ITERATE_MAJORMAJOR = 0;
        public const bool ITERATE_MAJORMAJOR = false;
        public const int ITERATE_MINORMAJOR = 1;
        public const int ITERATE_MINORRETRY = 2;

        /* Pricing methods */
        public const int PRICER_FIRSTINDEX = 0;
        public const int PRICER_DANTZIG = 1;
        public const int PRICER_DEVEX = 2;
        public const int PRICER_STEEPESTEDGE = 3;
        public const int PRICER_LASTOPTION = PRICER_STEEPESTEDGE;


        /* Additional settings for pricers (internal) */
        public const double PRICER_RANDFACT = 0.1;
        public const double DEVEX_RESTARTLIMIT = 1.0e+09; // Reset the norms if any value exceeds this limit
        public const double DEVEX_MINVALUE = 0.000; // Minimum weight [0..1] for entering variable, consider 0.01

        /* Pricing strategies */
        public const int PRICE_PRIMALFALLBACK = 4; /* In case of Steepest Edge, fall back to DEVEX in primal */
        public const int PRICE_MULTIPLE = 8; /* Enable multiple pricing (primal simplex) */
        public const int PRICE_PARTIAL = 16; /* Enable partial pricing */
        internal const int PRICE_ADAPTIVE = 32; /* Temporarily use alternative strategy if cycling is detected */
        public const int PRICE_HYBRID = 64; /* NOT IMPLEMENTED */
        public const int PRICE_RANDOMIZE = 128; /* Adds a small randomization effect to the selected pricer */
        public const int PRICE_AUTOPARTIAL = 256; /* Detect and use data on the block structure of the model (primal) */
        public const int PRICE_AUTOMULTIPLE = 512; /* Automatically select multiple pricing (primal simplex) */
        public const int PRICE_LOOPLEFT = 1024; /* Scan entering/leaving columns left rather than right */
        public const int PRICE_LOOPALTERNATE = 2048; /* Scan entering/leaving columns alternatingly left/right */
        public const int PRICE_HARRISTWOPASS = 4096; /* Use Harris' primal pivot logic rather than the default */
        public const int PRICE_FORCEFULL = 8192; /* Non-user option to force full pricing */
        public const int PRICE_TRUENORMINIT = 16384; /* Use true norms for Devex and Steepest Edge initializations */

        /*public const int _PRICE_NOBOUNDFLIP*/
        public const int PRICE_NOBOUNDFLIP = 65536; /* Disallow automatic bound-flip during pivot */

        public const int PRICE_STRATEGYMASK = (PRICE_PRIMALFALLBACK +
                                                PRICE_MULTIPLE + PRICE_PARTIAL +
                                                PRICE_ADAPTIVE + PRICE_HYBRID +
                                                PRICE_RANDOMIZE + PRICE_AUTOPARTIAL + PRICE_AUTOMULTIPLE +
                                                PRICE_LOOPLEFT + PRICE_LOOPALTERNATE +
                                                PRICE_HARRISTWOPASS +
                                                PRICE_FORCEFULL + PRICE_TRUENORMINIT);



        /* B&B active variable codes (internal) */
        public const int BB_REAL = 0;
        public const int BB_INT = 1;
        public const int BB_SC = 2;
        public const int BB_SOS = 3;
        public const int BB_GUB = 4;

        /* B&B strategies */
        public const int NODE_FIRSTSELECT = 0;
        public const int NODE_GAPSELECT = 1;
        public const int NODE_RANGESELECT = 2;
        public const int NODE_FRACTIONSELECT = 3;
        public const int NODE_PSEUDOCOSTSELECT = 4;
        public const int NODE_PSEUDONONINTSELECT = 5;    /* Kjell Eikland #1 - Minimize B&B depth */
        public const int NODE_PSEUDOFEASSELECT = (NODE_PSEUDONONINTSELECT + NODE_WEIGHTREVERSEMODE);
        public const int NODE_PSEUDORATIOSELECT = 6;    /* Kjell Eikland #2 - Minimize a "cost/benefit" ratio */
        public const int NODE_USERSELECT = 7;
        public const int NODE_STRATEGYMASK = (NODE_WEIGHTREVERSEMODE - 1); /* Mask for B&B strategies */
        public const int NODE_WEIGHTREVERSEMODE = 8;
        public const int NODE_BRANCHREVERSEMODE = 16;
        public const int NODE_GREEDYMODE = 32;
        public const int NODE_PSEUDOCOSTMODE = 64;
        public const int NODE_DEPTHFIRSTMODE = 128;
        public const int NODE_RANDOMIZEMODE = 256;
        public const int NODE_GUBMODE = 512;
        public const int NODE_DYNAMICMODE = 1024;
        public const int NODE_RESTARTMODE = 2048;
        public const int NODE_BREADTHFIRSTMODE = 4096;
        public const int NODE_AUTOORDER = 8192;
        public const int NODE_RCOSTFIXING = 16384;
        public const int NODE_STRONGINIT = 32768;

        public const int BRANCH_CEILING = 0;
        public const int BRANCH_FLOOR = 1;
        public const int BRANCH_AUTOMATIC = 2;
        public const int BRANCH_DEFAULT = 3;

        /* Action constants for simplex and B&B (internal) */
        public const int ACTION_NONE = 0;
        public const int ACTION_ACTIVE = 1;
        public const int ACTION_REBASE = 2;
        public const int ACTION_RECOMPUTE = 4;
        public const int ACTION_REPRICE = 8;
        public const int ACTION_REINVERT = 16;
        public const int ACTION_TIMEDREINVERT = 32;
        public const int ACTION_ITERATE = 64;
        public const int ACTION_RESTART = 255;

        /* Solver status values */
        public const int UNKNOWNERROR = -5;
        public const int DATAIGNORED = -4;
        public const int NOBFP = -3;
        public const int NOMEMORY = -2;
        public const int NOTRUN = -1;
        public const int OPTIMAL = 0;
        public const int SUBOPTIMAL = 1;
        public const int INFEASIBLE = 2;
        public const int UNBOUNDED = 3;
        public const int DEGENERATE = 4;
        public const int NUMFAILURE = 5;
        public const int USERABORT = 6;
        public const int TIMEOUT = 7;
        public const int RUNNING = 8;
        public const int PRESOLVED = 9;

        /* Branch & Bound and Lagrangean extra status values (internal) */
        public const int PROCFAIL = 10;
        public const int PROCBREAK = 11;
        public const int FEASFOUND = 12;
        public const int NOFEASFOUND = 13;
        public const int FATHOMED = 14;

        /* Status values internal to the solver (internal) */
        public const int SWITCH_TO_PRIMAL = 20;
        public const int SWITCH_TO_DUAL = 21;
        public const int SINGULAR_BASIS = 22;
        public const int LOSTFEAS = 23;
        public const int MATRIXERROR = 24;

        /* Objective testing options for "bb_better" (internal) */
        public const int OF_RELAXED = 0;
        public const int OF_INCUMBENT = 1;
        public const int OF_WORKING = 2;
        public const int OF_USERBREAK = 3;
        public const int OF_HEURISTIC = 4;
        public const int OF_DUALLIMIT = 5;
        public const int OF_DELTA = 8;  /* Mode */
        public const int OF_PROJECTED = 16;  /* Mode - future, not active */

        public const int OF_TEST_BT = 1;
        public const int OF_TEST_BE = 2;
        public const int OF_TEST_NE = 3;
        public const int OF_TEST_WE = 4;
        public const int OF_TEST_WT = 5;
        public const int OF_TEST_RELGAP = 8;  /* Mode */


        /* Name list and sparse matrix storage parameters (internal) */
        public const int MAT_START_SIZE = 10000;
        public const int DELTACOLALLOC = 100;
        public const int DELTAROWALLOC = 100;
        public const int RESIZEFACTOR = 4;  /* Fractional increase in selected memory allocations */


        /* Default solver parameters and tolerances (internal) */
        public const int DEF_PARTIALBLOCKS = 10; /* The default number of blocks for partial pricing */
        public const int DEF_MAXRELAX = 7; /* Maximum number of non-BB relaxations in MILP */
        public const int DEF_MAXPIVOTRETRY = 10; /* Maximum number of times to retry a div-0 situation */
        public const int DEF_MAXSINGULARITIES = 10; /* Maximum number of singularities in refactorization */
        public const int MAX_MINITUPDATES = 60; /* Maximum number of bound swaps between refactorizations without recomputing the whole vector - contain errors */
        public const int MIN_REFACTFREQUENCY = 5; /* Refactorization frequency indicating an inherent numerical instability of the basis*/
        public const int LAG_SINGULARLIMIT = 5; /* Number of times the objective does not change before it is assumed that the Lagrangean constraints are non-binding, and therefore impossible to converge; upper iteration limit is divided by this threshold*/
        public const double MIN_TIMEPIVOT = 5.0e-02; /* Minimum time per pivot for reinversion optimization purposes; use active monitoring only if a pivot takes more than MINTIMEPIVOT seconds.  5.0e-2 is roughly suitable for a 1GHz system.  */
        public const int MAX_STALLCOUNT = 12; /* The absolute upper limit to the number of stalling or cycling iterations before switching rule*/
        public const int MAX_RULESWITCH = 5; /* The maximum number of times to try an alternate pricing rule to recover from stalling; set negative for no limit. */
        /// <summary>
        /// This value is AUTOMATIC which is referring in lp_types.h
        /// </summary>
        public const double DEF_TIMEDREFACT = 2; /* Default for timed refactorization in BFPs; can be FALSE, TRUE or AUTOMATIC (object) */

        public const int DEF_SCALINGLIMIT = 5; /* The default maximum number of scaling iterations */

        public const double DEF_NEGRANGE = -1.0e+06; /* Downward limit for expanded variable range before the variable is split into positive and negative components */
        public const int DEF_BB_LIMITLEVEL = -50; /* Relative B&B limit to protect against very deep,memory-consuming trees */

        public const int MAX_FRACSCALE = 6; /* The maximum decimal scan range for simulated integers */
        public const int RANDSCALE = 100; /* Randomization scaling range */
        public const double DOUBLEROUND = 0.0e-02; /* Extra rounding scalar used in btran/ftran calculations; the rationale for 0.0 is that prod_xA() uses rounding as well*/
        public const double DEF_EPSMACHINE = 2.22e-16; /* Machine relative precision (doubles) */
        public const double MIN_STABLEPIVOT = 5.0; /* Minimum pivot magnitude assumed to be numerically stable */

        /* Precision macros                                                                       */
        /* -------------------------------------------------------------------------------------- */
        public const double PREC_REDUCEDCOST = lprec.epsvalue;
        public const double PREC_IMPROVEGAP = lprec.epsdual;
        public const double PREC_SUBSTFEASGAP = lprec.epsprimal;
#if true
        public const double PREC_BASICSOLUTION = lprec.epsvalue;  /* Zero-rounding of RHS/basic solution vector */
#else
        public const double PREC_BASICSOLUTION   = lprec.epsmachine;  /* Zero-rounding of RHS/basic solution vector */
#endif
        public const double LIMIT_ABS_REL = 10.0; /* Limit for testing using relative metric */


        /* Parameters constants for short-cut setting of tolerances                           */
        public const int EPS_TIGHT = 0;
        public const int EPS_MEDIUM = 1;
        public const int EPS_LOOSE = 2;
        public const int EPS_BAGGY = 3;
        public const int EPS_DEFAULT = EPS_TIGHT;


#if ActivePARAM == ProductionPARAM    //* PARAMETER SET FOR PRODUCTION                       */
        public const double DEF_INFINITE = 1.0e+30;  /* Limit for object range */
        public const double DEF_EPSVALUE = 1.0e-12;  /* High accuracy and feasibility preserving tolerance */
        public const double DEF_EPSPRIMAL = 1.0e-10;  /* For rounding primal/RHS values to 0 */
        public const double DEF_EPSDUAL = 1.0e-09;  /* For rounding reduced costs to 0 */
        public const double DEF_EPSPIVOT = 2.0e-07;  /* Pivot reject threshold */
        public const double DEF_PERTURB = 1.0e-05;  /* Perturbation scalar for degenerate problems; must at least be RANDSCALE greater than EPSPRIMAL */
        public const double DEF_EPSSOLUTION = 1.0e-05;  /* Margin of error for solution bounds */
        public const double DEF_EPSINT = 1.0e-07;  /* Accuracy for considering a float value as integer */

#elif ActivePARAM == OriginalPARAM //* PARAMETER SET FOR LEGACY VERSIONS                  */
        public const double DEF_INFINITE = 1.0e+24;  /* Limit for object range */
        public const double DEF_EPSVALUE = 1.0e-08;  /* High accuracy and feasibility preserving tolerance */
        public const double DEF_EPSPRIMAL = 5.01e-07;  /* For rounding primal/RHS values to 0, infeasibility */
        public const double DEF_EPSDUAL = 1.0e-06;  /* For rounding reduced costs to 0 */
        public const double DEF_EPSPIVOT = 1.0e-04;  /* Pivot reject threshold */
        public const double DEF_PERTURB = 1.0e-05;  /* Perturbation scalar for degenerate problems; must at least be RANDSCALE greater than EPSPRIMAL */
        public const double DEF_EPSSOLUTION = 1.0e-02;  /* Margin of error for solution bounds */
        public const double DEF_EPSINT = 1.0e-03;  /* Accuracy for considering a float value as integer */

#elif ActivePARAM == ChvatalPARAM     //* PARAMETER SET EXAMPLES FROM Vacek Chvatal          */
        public const double DEF_INFINITE = 1.0e+30; /* Limit for object range */
        public const double DEF_EPSVALUE = 1.0e-10; /* High accuracy and feasibility preserving tolerance */
        public const double DEF_EPSPRIMAL = 10e-07; /* For rounding primal/RHS values to 0 */
        public const double DEF_EPSDUAL = 10e-05; /* For rounding reduced costs to 0 */
        public const double DEF_EPSPIVOT = 10e-05; /* Pivot reject threshold */
        public const double DEF_PERTURB = 10e-03; /* Perturbation scalar for degenerate problems; must at least be RANDSCALE greater than EPSPRIMAL */
        public const double DEF_EPSSOLUTION = 1.0e-05; /* Margin of error for solution bounds */
        public const double DEF_EPSINT = 5.0e-03; /* Accuracy for considering a float value as integer */

#elif ActivePARAM == LoosePARAM       //* PARAMETER SET FOR LOOSE TOLERANCES                 */
        public const double DEF_INFINITE = 1.0e+30; /* Limit for object range */
        public const double DEF_EPSVALUE = 1.0e-10; /* High accuracy and feasibility preserving tolerance */
        public const double DEF_EPSPRIMAL = 5.01e-08; /* For rounding primal/RHS values to 0 */
        public const double DEF_EPSDUAL = 1.0e-07; /* For rounding reduced costs to 0 */
        public const double DEF_EPSPIVOT = 1.0e-05; /* Pivot reject threshold */
        public const double DEF_PERTURB = 1.0e-05; /* Perturbation scalar for degenerate problems; must at least be RANDSCALE greater than EPSPRIMAL */
        public const double DEF_EPSSOLUTION = 1.0e-05; /* Margin of error for solution bounds */
        public const double DEF_EPSINT = 1.0e-04; /* Accuracy for considering a float value as integer */

#endif


        public const double DEF_MIP_GAP = 1.0e-11; /* The default absolute and relative MIP gap */
        public const double SCALEDINTFIXRANGE = 1.6; /* Epsilon range multiplier < 2 for collapsing bounds to fix */
        public const double MIN_SCALAR = 1.0e-10; /* Smallest allowed scaling adjustment */
        public const double MAX_SCALAR = 1.0e+10; /* Largest allowed scaling adjustment */
        public const double DEF_SCALINGEPS = 1.0e-02; /* Relative scaling convergence criterion for auto_scale */
        public const double DEF_LAGACCEPT = 1.0e-03; /* Default Lagrangean convergence acceptance criterion */
        public const double DEF_LAGCONTRACT = 0.90;/* The contraction parameter for Lagrangean iterations */
        public const double DEF_LAGMAXITERATIONS = 100;/* The maximum number of Lagrangean iterations */
        public const double DEF_PSEUDOCOSTUPDATES = 7;/* The default number of times pseudo-costs are recalculated; experiments indicate that costs tend to stabilize*/
        public const double DEF_PSEUDOCOSTRESTART = 0.15;/* The fraction of price updates required for B&B restart when the mode is NODE_RESTARTMODE*/
        public const double DEF_MAXPRESOLVELOOPS = 0;/* Upper limit to the number of loops during presolve, <= 0 for no limit. */












        /* Prototypes for user call-back functions                                   */
        /* ------------------------------------------------------------------------- */
        
        
        

        // ORIGINAL LINE: typedef int    (__WINAPI lphandle_intfunc)(lprec* lp, void* userhandle);
        public delegate int lphandle_intfunc(lprec lp, object userhandle);
        // ORIGINAL LINE: typedef void   (__WINAPI lphandlestr_func)(lprec* lp, void* userhandle, char* buf);
        public delegate int lphandlestr_func(lprec lp, object userhandle, string buf);
        // ORIGINAL LINE: typedef void   (__WINAPI lphandleint_func)(lprec* lp, void* userhandle, int message);
        public delegate int lphandleint_func(lprec lp, object userhandle, int message);
        // ORIGINAL LINE: typedef int    (__WINAPI lphandleint_intfunc)(lprec* lp, void* userhandle, int message);
        public delegate int lphandleint_intfunc(lprec lp, object userhandle, int message);

        /* User and system function interfaces                                       */
        /* ------------------------------------------------------------------------- */

        /// <summary>
        /// changed parameters datatype from int to int? as the implementation 
        /// is cheking for null condition on 6-11-18
        /// </summary>
        public void lp_solve_version(ref int? majorversion, ref int? minorversion, ref int? release, ref int? build)
        { throw new NotImplementedException(); }

        public lprec make_lp(int rows, int columns)
        { throw new NotImplementedException(); }
        public byte resize_lp(lprec lp, int rows, int columns)
        { throw new NotImplementedException(); }
        public int get_status(lprec lp)
        { throw new NotImplementedException(); }
        public string get_statustext(lprec lp, int statuscode)
        { throw new NotImplementedException(); }
        public byte is_obj_in_basis(lprec lp)
        { throw new NotImplementedException(); }
        public void set_obj_in_basis(lprec lp, byte obj_in_basis)
        { throw new NotImplementedException(); }
        /* Create and initialise a lprec structure defaults */

        public lprec copy_lp(lprec lp)
        { throw new NotImplementedException(); }
        public byte dualize_lp(lprec lp)
        { throw new NotImplementedException(); }
        public byte memopt_lp(lprec lp, int rowextra, int colextra, int nzextra)
        { throw new NotImplementedException(); }
        /* Copy or dualize the lp */

        public void delete_lp(lprec lp)
        { throw new NotImplementedException(); }
        public void free_lp(lprec[] plp)
        { throw new NotImplementedException(); }
        /* Remove problem from memory */

        /// <summary>
        /// changed from ref string to char on 12/11/18;
        /// changed return type from byte to bool on 12/11/18 
        /// </summary>
        public bool set_lp_name(lprec lp, char lpname)
        { throw new NotImplementedException(); }
        public string get_lp_name(lprec lp)
        {
            return ((lp.lp_name != null) ? lp.lp_name : (String)"");
        }
        /* Set and get the problem name */

        public byte has_BFP(lprec lp)
        { throw new NotImplementedException(); }
        public byte is_nativeBFP(lprec lp)
        { throw new NotImplementedException(); }
        public byte set_BFP(lprec lp, ref string filename)
        { throw new NotImplementedException(); }
        /* Set basis factorization engine */

        public lprec read_XLI(ref string xliname, ref string modelname, ref string dataname, ref string options, int verbose)
        { throw new NotImplementedException(); }
        public byte write_XLI(lprec lp, ref string filename, ref string options, byte results)
        { throw new NotImplementedException(); }
        public byte has_XLI(lprec lp)
        { throw new NotImplementedException(); }
        public byte is_nativeXLI(lprec lp)
        { throw new NotImplementedException(); }
        public byte set_XLI(lprec lp, ref string filename)
        { throw new NotImplementedException(); }
        /* Set external language interface */

        public byte set_obj(lprec lp, int colnr, double value)
        { throw new NotImplementedException(); }
        public byte set_obj_fn(lprec lp, ref double row)
        { throw new NotImplementedException(); }
        public byte set_obj_fnex(lprec lp, int count, ref double row, ref int colno)
        { throw new NotImplementedException(); }
        /* set the objective function (Row 0) of the matrix */
        public byte str_set_obj_fn(lprec lp, ref string row_string)
        { throw new NotImplementedException(); }
        /* The same, but with string input */
        /// <summary>
        /// changed byte maximize to bool maximize on 12/11/18
        /// </summary>
        public void set_sense(lprec lp, bool maximize)
        {
            throw new NotImplementedException();
        }
        public void set_maxim(lprec lp)
        { throw new NotImplementedException(); }
        public void set_minim(lprec lp)
        { throw new NotImplementedException(); }
        /// <summary>
        /// changed retrun type from byte bool on 12/11/18
        /// </summary>
        public bool is_maxim(lprec lp)
        {
            throw new NotImplementedException();
        }
        /* Set optimization direction for the objective function */

        public bool add_constraint(lprec lp, ref double row, int constr_type, double rh)
        { throw new NotImplementedException(); }

        /// <summary>
        /// changed from ref int colno to ref int? colno on 12/11/18
        /// </summary>
        public byte add_constraintex(lprec lp, int count, ref double row, ref int? colno, int constr_type, double rh)
        { throw new NotImplementedException(); }
        public byte set_add_rowmode(lprec lp, byte turnon)
        { throw new NotImplementedException(); }
        public byte is_add_rowmode(lprec lp)
        { throw new NotImplementedException(); }
        /* Add a constraint to the problem, row is the constraint row, rh is the right hand side,
            constr_type is the type of constraint (LE (<=), GE(>=), EQ(=)) */
        public byte str_add_constraint(lprec lp, ref string row_string, int constr_type, double rh)
        { throw new NotImplementedException(); }
        /* The same, but with string input */

        public byte set_row(lprec lp, int rownr, ref double row)
        { throw new NotImplementedException(); }
        public byte set_rowex(lprec lp, int rownr, int count, ref double row, ref int colno)
        { throw new NotImplementedException(); }
        public byte get_row(lprec lp, int rownr, ref double row)
        { throw new NotImplementedException(); }
        public int get_rowex(lprec lp, int rownr, ref double row, ref int colno)
        { throw new NotImplementedException(); }
        /* Fill row with the row row_nr from the problem */

        public byte del_constraint(lprec lp, int rownr)
        { throw new NotImplementedException(); }
        public byte del_constraintex(lprec lp, LLrec rowmap)
        { throw new NotImplementedException(); }
        /* Remove constrain nr del_row from the problem */

        public byte add_lag_con(lprec lp, ref double row, int con_type, double rhs)
        { throw new NotImplementedException(); }
        /* add a Lagrangian constraint of form Row' x contype Rhs */
        public byte str_add_lag_con(lprec lp, ref string row_string, int con_type, double rhs)
        { throw new NotImplementedException(); }
        /* The same, but with string input */
        public void set_lag_trace(lprec lp, byte lag_trace)
        { throw new NotImplementedException(); }
        public byte is_lag_trace(lprec lp)
        { throw new NotImplementedException(); }
        /* Set debugging/tracing mode of the Lagrangean solver */

        public byte set_constr_type(lprec lp, int rownr, int con_type)
        { throw new NotImplementedException(); }
        public int get_constr_type(lprec lp, int rownr)
        { throw new NotImplementedException(); }
        public double get_constr_value(lprec lp, int rownr, int count, ref double primsolution, ref int nzindex)
        { throw new NotImplementedException(); }
        public byte is_constr_type(lprec lp, int rownr, int mask)
        { throw new NotImplementedException(); }
        public string get_str_constr_type(lprec lp, int con_type)
        { throw new NotImplementedException(); }
        public int get_constr_class(lprec lp, int rownr)
        { throw new NotImplementedException(); }
        public string get_str_constr_class(lprec lp, int con_class)
        { throw new NotImplementedException(); }
        /* Set the type of constraint in row Row (LE, GE, EQ) */

        public byte set_rh(lprec lp, int rownr, double value)
        { throw new NotImplementedException(); }
        public double get_rh(lprec lp, int rownr)
        { throw new NotImplementedException(); }
        /* Set and get the right hand side of a constraint row */
        public byte set_rh_range(lprec lp, int rownr, double deltavalue)
        { throw new NotImplementedException(); }
        public double get_rh_range(lprec lp, int rownr)
        { throw new NotImplementedException(); }
        /* Set the RHS range; i.e. the lower and upper bounds of a constraint row */
        public void set_rh_vec(lprec lp, ref double rh)
        { throw new NotImplementedException(); }
        /* Set the right hand side vector */
        public byte str_set_rh_vec(lprec lp, ref string rh_string)
        { throw new NotImplementedException(); }
        /* The same, but with string input */


        public byte add_column(lprec lp, ref double column)
        { throw new NotImplementedException(); }
        public byte add_columnex(lprec lp, int count, ref double column, ref int rowno)
        { throw new NotImplementedException(); }
        public byte str_add_column(lprec lp, ref string col_string)
        { throw new NotImplementedException(); }
        /* Add a column to the problem */

        public byte set_column(lprec lp, int colnr, ref double column)
        { throw new NotImplementedException(); }
        public byte set_columnex(lprec lp, int colnr, int count, ref double column, ref int rowno)
        { throw new NotImplementedException(); }
        /* Overwrite existing column data */

        public int column_in_lp(lprec lp, ref double column)
        { throw new NotImplementedException(); }
        /* Returns the column index if column is already present in lp, otherwise 0.
            (Does not look at bounds and types, only looks at matrix values */

        public int get_columnex(lprec lp, int colnr, ref double column, ref int nzrow)
        { throw new NotImplementedException(); }
        public byte get_column(lprec lp, int colnr, ref double column)
        { throw new NotImplementedException(); }
        /* Fill column with the column col_nr from the problem */

        public byte del_column(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        public byte del_columnex(lprec lp, LLrec colmap)
        { throw new NotImplementedException(); }
        /* Delete a column */

        public byte set_mat(lprec lp, int rownr, int colnr, double value)
        { throw new NotImplementedException(); }
        /* Fill in element (Row,Column) of the matrix
            Row in [0..Rows] and Column in [1..Columns] */
        public double get_mat(lprec lp, int rownr, int colnr)
        { throw new NotImplementedException(); }
        public double get_mat_byindex(lprec lp, int matindex, byte isrow, byte adjustsign)
        { throw new NotImplementedException(); }
        public int get_nonzeros(lprec lp)
        { throw new NotImplementedException(); }
        /* get a single element from the matrix */
        /* Name changed from "mat_elm" by KE */

        public void set_bounds_tighter(lprec lp, byte tighten)
        { throw new NotImplementedException(); }
        public byte get_bounds(lprec lp, int column, ref double lower, ref double upper)
        { throw new NotImplementedException(); }
        public byte get_bounds_tighter(lprec lp)
        { throw new NotImplementedException(); }
        public byte set_upbo(lprec lp, int colnr, double value)
        { throw new NotImplementedException(); }
        public double get_upbo(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        public byte set_lowbo(lprec lp, int colnr, double value)
        { throw new NotImplementedException(); }
        public double get_lowbo(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        public byte set_bounds(lprec lp, int colnr, double lower, double upper)
        { throw new NotImplementedException(); }
        public byte set_unbounded(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        public byte is_unbounded(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        /* Set the upper and lower bounds of a variable */

        public byte set_int(lprec lp, int colnr, byte must_be_int)
        { throw new NotImplementedException(); }
        public bool is_int(lprec lp, int colnr)
        {
            throw new NotImplementedException();
        }
        public byte set_binary(lprec lp, int colnr, byte must_be_bin)
        { throw new NotImplementedException(); }
        public byte is_binary(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        // chanegd from 'bool' to 'uint' FIX_ca4c2404-e9f4-407d-b791-79776cb8de1f 19/11/18
        public byte set_semicont(lprec lp, int colnr, uint must_be_sc)
        { throw new NotImplementedException(); }
        public bool is_semicont(lprec lp, int colnr)
        {
            throw new NotImplementedException();
        }
        public byte is_negative(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        public byte set_var_weights(lprec lp, ref double weights)
        { throw new NotImplementedException(); }
        public int get_var_priority(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        /* Set the type of variable */

        public byte set_pseudocosts(lprec lp, ref double clower, ref double cupper, ref int updatelimit)
        { throw new NotImplementedException(); }
        public byte get_pseudocosts(lprec lp, ref double clower, ref double cupper, ref int updatelimit)
        { throw new NotImplementedException(); }
        /* Set initial values for, or get computed pseudocost vectors;
            note that setting of pseudocosts can only happen in response to a
            call-back function optionally requesting this */
        /// <summary>
        /// changed from 'ref int[] sosvars, ref double weights' to int?[] sosvars, double? weights 
        /// on 15/11/18
        /// </summary>
        public int add_SOS(lprec lp, ref string name, int sostype, int priority, int count, int?[] sosvars, double? weights)
        {
            throw new NotImplementedException();
        }
        public byte is_SOS_var(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        /* Add SOS constraints */

        public bool set_row_name(lprec lp, int rownr, ref string new_name)
        {
            throw new NotImplementedException();
        }
        public string get_row_name(lprec lp, int rownr)
        { throw new NotImplementedException(); }
        public string get_origrow_name(lprec lp, int rownr)
        { throw new NotImplementedException(); }
        /* Set/Get the name of a constraint row */
        /* Get added by KE */

        public byte set_col_name(lprec lp, int colnr, ref string new_name)
        { throw new NotImplementedException(); }
        public string get_col_name(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        public string get_origcol_name(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        /* Set/Get the name of a variable column */
        /* Get added by KE */

        public void unscale(lprec lp)
        { throw new NotImplementedException(); }
        /* Undo previous scaling of the problem */

        public void set_preferdual(lprec lp, byte dodual)
        { throw new NotImplementedException(); }
        public void set_simplextype(lprec lp, int simplextype)
        { throw new NotImplementedException(); }
        public int get_simplextype(lprec lp)
        { throw new NotImplementedException(); }
        /* Set/Get if lp_solve should prefer the dual simplex over the primal -- added by KE */

        public void default_basis(lprec lp)
        { throw new NotImplementedException(); }
        public void set_basiscrash(lprec lp, int mode)
        { throw new NotImplementedException(); }
        public int get_basiscrash(lprec lp)
        { throw new NotImplementedException(); }
        public int set_basisvar(lprec lp, int basisPos, int enteringCol)
        { throw new NotImplementedException(); }
        public byte set_basis(lprec lp, ref int bascolumn, byte nonbasic)
        { throw new NotImplementedException(); }
        public byte get_basis(lprec lp, ref int bascolumn, byte nonbasic)
        { throw new NotImplementedException(); }
        public void reset_basis(lprec lp)
        { throw new NotImplementedException(); }
        /* Set/Get basis for a re-solved system */
        /* Added by KE */
        public byte guess_basis(lprec lp, ref double guessvector, ref int basisvector)
        { throw new NotImplementedException(); }

        public byte is_feasible(lprec lp, ref double values, double threshold)
        { throw new NotImplementedException(); }
        /* returns TRUE if the vector in values is a feasible solution to the lp */

        public int solve(lprec lp)
        { throw new NotImplementedException(); }
        /* Solve the problem */

        public double time_elapsed(lprec lp)
        { throw new NotImplementedException(); }
        /* Return the number of seconds since start of solution process */



        public void put_bb_nodefunc(lprec lp, lphandleint_intfunc newnode, object bbnodehandle)
        { throw new NotImplementedException(); }
        public void put_bb_branchfunc(lprec lp, lphandleint_intfunc newbranch, object bbbranchhandle)
        { throw new NotImplementedException(); }
        /* Allow the user to override B&B node and branching decisions */

        public void put_abortfunc(lprec lp, lphandle_intfunc newctrlc, object ctrlchandle)
        { throw new NotImplementedException(); }
        /* Allow the user to define an interruption callback function */

        public void put_logfunc(lprec lp, lphandlestr_func newlog, object loghandle)
        { throw new NotImplementedException(); }
        /* Allow the user to define a logging function */

        public void put_msgfunc(lprec lp, lphandleint_func newmsg, object msghandle, int mask)
        { throw new NotImplementedException(); }
        /* Allow the user to define an event-driven message/reporting */

        public byte get_primal_solution(lprec lp, ref double pv)
        { throw new NotImplementedException(); }
        public byte get_ptr_primal_solution(lprec lp, double[][] pv)
        { throw new NotImplementedException(); }
        public byte get_dual_solution(lprec lp, ref double rc)
        { throw new NotImplementedException(); }
        public byte get_ptr_dual_solution(lprec lp, double[][] rc)
        { throw new NotImplementedException(); }
        public byte get_lambda(lprec lp, ref double lambda)
        { throw new NotImplementedException(); }
        public byte get_ptr_lambda(lprec lp, double[][] lambda)
        { throw new NotImplementedException(); }
        /* Get the primal, dual/reduced costs and Lambda vectors */

        /* Read an MPS file */
        public lprec read_MPS(ref string filename, int options)
        { throw new NotImplementedException(); }
        public lprec read_mps(FILE filename, int options)
        { throw new NotImplementedException(); }
        public lprec read_freeMPS(ref string filename, int options)
        { throw new NotImplementedException(); }
        public lprec read_freemps(FILE filename, int options)
        { throw new NotImplementedException(); }

        /* Write a MPS file to output */
        public byte write_mps(lprec lp, ref string filename)
        { throw new NotImplementedException(); }
        public byte write_MPS(lprec lp, FILE output)
        { throw new NotImplementedException(); }
        public byte write_freemps(lprec lp, ref string filename)
        { throw new NotImplementedException(); }
        public byte write_freeMPS(lprec lp, FILE output)
        { throw new NotImplementedException(); }

        public byte write_lp(lprec lp, ref string filename)
        { throw new NotImplementedException(); }
        public byte write_LP(lprec lp, FILE output)
        { throw new NotImplementedException(); }
        /* Write a LP file to output */

        public byte LP_readhandle(lprec[] lp, FILE filename, int verbose, ref string lp_name)
        { throw new NotImplementedException(); }
        public lprec read_lp(FILE filename, int verbose, ref string lp_name)
        { throw new NotImplementedException(); }
        public lprec read_LP(ref string filename, int verbose, ref string lp_name)
        { throw new NotImplementedException(); }
        /* Old-style lp format file parser */

        public byte write_basis(lprec lp, ref string filename)
        { throw new NotImplementedException(); }
        public byte read_basis(lprec lp, ref string filename, ref string info)
        { throw new NotImplementedException(); }
        /* Read and write basis from/to file in CPLEX BAS format */

        public byte write_params(lprec lp, ref string filename, ref string options)
        { throw new NotImplementedException(); }
        public byte read_params(lprec lp, ref string filename, ref string options)
        { throw new NotImplementedException(); }
        public void reset_params(lprec lp)
        { throw new NotImplementedException(); }
        /* Read and write parameter file */

        public void print_lp(lprec lp)
        { throw new NotImplementedException(); }
        public void print_tableau(lprec lp)
        { throw new NotImplementedException(); }
        /* Print the current problem, only useful in very small (test) problems */

        public void print_objective(lprec lp)
        { throw new NotImplementedException(); }
        public void print_solution(lprec lp, int columns)
        { throw new NotImplementedException(); }
        public void print_constraints(lprec lp, int columns)
        { throw new NotImplementedException(); }
        /* Print the solution to stdout */

        public void print_duals(lprec lp)
        { throw new NotImplementedException(); }
        /* Print the dual variables of the solution */

        public void print_scales(lprec lp)
        { throw new NotImplementedException(); }
        /* If scaling is used, print the scaling factors */

        public void print_str(lprec lp, ref string str)
        { throw new NotImplementedException(); }

        //changed lp.set_outputstream parameter from 'FILE stream' to 'FileStream stream' FIX_d5d09c67-2da1-4d77-a987-2b35b04f74cc 19/11/18
        public void set_outputstream(lprec lp, FileStream stream)
        { throw new NotImplementedException(); }
        public byte set_outputfile(lprec lp, ref string filename)
        { throw new NotImplementedException(); }

        public void set_verbose(lprec lp, int verbose)
        { throw new NotImplementedException(); }
        public int get_verbose(lprec lp)
        { throw new NotImplementedException(); }

        public void set_timeout(lprec lp, int sectimeout)
        { throw new NotImplementedException(); }
        public int get_timeout(lprec lp)
        { throw new NotImplementedException(); }

        public void set_print_sol(lprec lp, int print_sol)
        { throw new NotImplementedException(); }
        public int get_print_sol(lprec lp)
        { throw new NotImplementedException(); }

        public void set_debug(lprec lp, byte debug)
        { throw new NotImplementedException(); }
        public byte is_debug(lprec lp)
        { throw new NotImplementedException(); }

        public void set_trace(lprec lp, byte trace)
        { throw new NotImplementedException(); }
        public byte is_trace(lprec lp)
        { throw new NotImplementedException(); }

        public byte print_debugdump(lprec lp, ref string filename)
        { throw new NotImplementedException(); }

        public void set_anti_degen(lprec lp, int anti_degen)
        { throw new NotImplementedException(); }
        public int get_anti_degen(lprec lp)
        { throw new NotImplementedException(); }
        public bool is_anti_degen(lprec lp, int testmask)
        { throw new NotImplementedException(); }

        public void set_presolve(lprec lp, int presolvemode, int maxloops)
        { throw new NotImplementedException(); }
        public int get_presolve(lprec lp)
        { throw new NotImplementedException(); }
        public int get_presolveloops(lprec lp)
        { throw new NotImplementedException(); }
        public byte is_presolve(lprec lp, int testmask)
        { throw new NotImplementedException(); }

        public int get_orig_index(lprec lp, int lp_index)
        { throw new NotImplementedException(); }
        public int get_lp_index(lprec lp, int orig_index)
        { throw new NotImplementedException(); }

        public void set_maxpivot(lprec lp, int max_num_inv)
        { throw new NotImplementedException(); }
        public int get_maxpivot(lprec lp)
        { throw new NotImplementedException(); }

        public void set_obj_bound(lprec lp, double obj_bound)
        { throw new NotImplementedException(); }
        public double get_obj_bound(lprec lp)
        { throw new NotImplementedException(); }

        public void set_mip_gap(lprec lp, byte absolute, double mip_gap)
        { throw new NotImplementedException(); }
        public double get_mip_gap(lprec lp, byte absolute)
        { throw new NotImplementedException(); }

        public void set_bb_rule(lprec lp, int bb_rule)
        { throw new NotImplementedException(); }
        public int get_bb_rule(lprec lp)
        { throw new NotImplementedException(); }

        public byte set_var_branch(lprec lp, int colnr, int branch_mode)
        { throw new NotImplementedException(); }
        public int get_var_branch(lprec lp, int colnr)
        { throw new NotImplementedException(); }

        /// <summary>
        /// changed return type from byte to bool on 12/11/18
        /// </summary>
        public bool is_infinite(lprec lp, double value)
        {
            throw new NotImplementedException();
        }
        public void set_infinite(lprec lp, double infinite)
        { throw new NotImplementedException(); }
        public double get_infinite(lprec lp)
        { throw new NotImplementedException(); }

        public void set_epsint(lprec lp, double epsint)
        { throw new NotImplementedException(); }
        public double get_epsint(lprec lp)
        { throw new NotImplementedException(); }

        public void set_epsb(lprec lp, double epsb)
        { throw new NotImplementedException(); }
        public double get_epsb(lprec lp)
        { throw new NotImplementedException(); }

        public void set_epsd(lprec lp, double epsd)
        { throw new NotImplementedException(); }
        public double get_epsd(lprec lp)
        { throw new NotImplementedException(); }

        public void set_epsel(lprec lp, double epsel)
        { throw new NotImplementedException(); }
        public double get_epsel(lprec lp)
        { throw new NotImplementedException(); }

        public byte set_epslevel(lprec lp, int epslevel)
        { throw new NotImplementedException(); }

        public void set_scaling(lprec lp, int scalemode)
        { throw new NotImplementedException(); }
        public int get_scaling(lprec lp)
        { throw new NotImplementedException(); }
        public byte is_scalemode(lprec lp, int testmask)
        { throw new NotImplementedException(); }
        public byte is_scaletype(lprec lp, int scaletype)
        { throw new NotImplementedException(); }
        /// <summary>
        /// changed from byte to bool on 13/11/18
        /// </summary>
        public bool is_integerscaling(lprec lp)
        { throw new NotImplementedException(); }
        public void set_scalelimit(lprec lp, double scalelimit)
        { throw new NotImplementedException(); }
        public double get_scalelimit(lprec lp)
        { throw new NotImplementedException(); }

        public void set_improve(lprec lp, int improve)
        { throw new NotImplementedException(); }
        public int get_improve(lprec lp)
        { throw new NotImplementedException(); }

        public void set_pivoting(lprec lp, int piv_rule)
        { throw new NotImplementedException(); }
        public int get_pivoting(lprec lp)
        { throw new NotImplementedException(); }
        public byte set_partialprice(lprec lp, int blockcount, ref int blockstart, byte isrow)
        { throw new NotImplementedException(); }
        public void get_partialprice(lprec lp, ref int blockcount, ref int blockstart, byte isrow)
        { throw new NotImplementedException(); }

        public byte set_multiprice(lprec lp, int multiblockdiv)
        { throw new NotImplementedException(); }
        public int get_multiprice(lprec lp, byte getabssize)
        { throw new NotImplementedException(); }

        public byte is_use_names(lprec lp, byte isrow)
        { throw new NotImplementedException(); }
        public void set_use_names(lprec lp, byte isrow, byte use_names)
        { throw new NotImplementedException(); }

        public int get_nameindex(lprec lp, ref string varname, byte isrow)
        { throw new NotImplementedException(); }

        public byte is_piv_mode(lprec lp, int testmask)
        { throw new NotImplementedException(); }
        public byte is_piv_rule(lprec lp, int rule)
        { throw new NotImplementedException(); }

        public void set_break_at_first(lprec lp, byte break_at_first)
        { throw new NotImplementedException(); }
        public byte is_break_at_first(lprec lp)
        { throw new NotImplementedException(); }

        public void set_bb_floorfirst(lprec lp, int bb_floorfirst)
        { throw new NotImplementedException(); }
        public int get_bb_floorfirst(lprec lp)
        { throw new NotImplementedException(); }

        public void set_bb_depthlimit(lprec lp, int bb_maxlevel)
        { throw new NotImplementedException(); }
        public int get_bb_depthlimit(lprec lp)
        { throw new NotImplementedException(); }

        public void set_break_at_value(lprec lp, double break_at_value)
        { throw new NotImplementedException(); }
        public double get_break_at_value(lprec lp)
        { throw new NotImplementedException(); }

        public void set_negrange(lprec lp, double negrange)
        { throw new NotImplementedException(); }
        public double get_negrange(lprec lp)
        { throw new NotImplementedException(); }

        public void set_epsperturb(lprec lp, double epsperturb)
        { throw new NotImplementedException(); }
        public double get_epsperturb(lprec lp)
        { throw new NotImplementedException(); }

        public void set_epspivot(lprec lp, double epspivot)
        { throw new NotImplementedException(); }
        public double get_epspivot(lprec lp)
        { throw new NotImplementedException(); }

        public int get_max_level(lprec lp)
        { throw new NotImplementedException(); }
        public long get_total_nodes(lprec lp)
        { throw new NotImplementedException(); }
        public long get_total_iter(lprec lp)
        { throw new NotImplementedException(); }

        public double get_objective(lprec lp)
        { throw new NotImplementedException(); }
        public double get_working_objective(lprec lp)
        { throw new NotImplementedException(); }

        public double get_var_primalresult(lprec lp, int index)
        { throw new NotImplementedException(); }
        public double get_var_dualresult(lprec lp, int index)
        { throw new NotImplementedException(); }

        public byte get_variables(lprec lp, ref double @var)
        { throw new NotImplementedException(); }
        public byte get_ptr_variables(lprec lp, double[][] @var)
        { throw new NotImplementedException(); }

        public byte get_constraints(lprec lp, ref double constr)
        { throw new NotImplementedException(); }
        public byte get_ptr_constraints(lprec lp, double[][] constr)
        { throw new NotImplementedException(); }

        public byte get_sensitivity_rhs(lprec lp, ref double duals, ref double dualsfrom, ref double dualstill)
        { throw new NotImplementedException(); }
        public byte get_ptr_sensitivity_rhs(lprec lp, double[][] duals, double[][] dualsfrom, double[][] dualstill)
        { throw new NotImplementedException(); }


        public byte get_sensitivity_obj(lprec lp, ref double objfrom, ref double objtill)
        { throw new NotImplementedException(); }
        public byte get_sensitivity_objex(lprec lp, ref double objfrom, ref double objtill, ref double objfromvalue, ref double objtillvalue)
        { throw new NotImplementedException(); }
        public byte get_ptr_sensitivity_obj(lprec lp, double[][] objfrom, double[][] objtill)
        { throw new NotImplementedException(); }
        public byte get_ptr_sensitivity_objex(lprec lp, double[][] objfrom, double[][] objtill, double[][] objfromvalue, double[][] objtillvalue)
        { throw new NotImplementedException(); }

        public void set_solutionlimit(lprec lp, int limit)
        { throw new NotImplementedException(); }
        public int get_solutionlimit(lprec lp)
        { throw new NotImplementedException(); }
        public int get_solutioncount(lprec lp)
        { throw new NotImplementedException(); }

        public int get_Norig_rows(lprec lp)
        { throw new NotImplementedException(); }
        public int get_Nrows(lprec lp)
        { throw new NotImplementedException(); }
        public int get_Lrows(lprec lp)
        { throw new NotImplementedException(); }

        public int get_Norig_columns(lprec lp)
        { throw new NotImplementedException(); }
        public int get_Ncolumns(lprec lp)
        { throw new NotImplementedException(); }

        //ORIGINAL LINE: typedef int (__WINAPI read_modeldata_func)(void* userhandle, char* buf, int max_size);
        public delegate int read_modeldata_func(object userhandle, string buf, int max_size);
        //ORIGINAL LINE: typedef int (__WINAPI write_modeldata_func)(void* userhandle, char* buf);
        public delegate int write_modeldata_func(object userhandle, string buf);


        public byte MPS_readex(lprec[] newlp, object userhandle, read_modeldata_func read_modeldata, int typeMPS, int options)
        { throw new NotImplementedException(); }

        public lprec read_lpex(object userhandle, read_modeldata_func read_modeldata, int verbose, ref string lp_name)
        { throw new NotImplementedException(); }
        public byte write_lpex(lprec lp, object userhandle, write_modeldata_func write_modeldata)
        { throw new NotImplementedException(); }

        public lprec read_mpsex(object userhandle, read_modeldata_func read_modeldata, int options)
        { throw new NotImplementedException(); }
        public lprec read_freempsex(object userhandle, read_modeldata_func read_modeldata, int options)
        { throw new NotImplementedException(); }

        public byte MPS_writefileex(lprec lp, int typeMPS, object userhandle, write_modeldata_func write_modeldata)
        { throw new NotImplementedException(); }












































        /* Forward definitions of functions used internaly by the lp toolkit */
        public byte set_callbacks(lprec lp)
        { throw new NotImplementedException(); }
        public int yieldformessages(lprec lp)
        { throw new NotImplementedException(); }
        public byte userabort(lprec lp, int message)
        { throw new NotImplementedException(); }

        /* Memory management routines */
        public bool append_rows(lprec lp, int deltarows)
        {
            throw new NotImplementedException();
        }
        public byte append_columns(lprec lp, int deltacolumns)
        { throw new NotImplementedException(); }
        public void inc_rows(lprec lp, int delta)
        { throw new NotImplementedException(); }
        public void inc_columns(lprec lp, int delta)
        { throw new NotImplementedException(); }
        public bool init_rowcol_names(lprec lp)
        { throw new NotImplementedException(); }
        public bool inc_row_space(lprec lp, int deltarows)
        { throw new NotImplementedException(); }
        public byte inc_col_space(lprec lp, int deltacols)
        { throw new NotImplementedException(); }
        public byte shift_rowcoldata(lprec lp, int @base, int delta, LLrec usedmap, byte isrow)
        { throw new NotImplementedException(); }
        public byte shift_basis(lprec lp, int @base, int delta, LLrec usedmap, byte isrow)
        { throw new NotImplementedException(); }
        public byte shift_rowdata(lprec lp, int @base, int delta, LLrec usedmap)
        { throw new NotImplementedException(); }
        public byte shift_coldata(lprec lp, int @base, int delta, LLrec usedmap)
        { throw new NotImplementedException(); }

        public bool is_chsign(lprec lp, int rownr)
        { throw new NotImplementedException(); }

        public byte inc_lag_space(lprec lp, int deltarows, byte ignoreMAT)
        { throw new NotImplementedException(); }
        public lprec make_lag(lprec server)
        { throw new NotImplementedException(); }

        public double get_rh_upper(lprec lp, int rownr)
        { throw new NotImplementedException(); }
        public double get_rh_lower(lprec lp, int rownr)
        { throw new NotImplementedException(); }
        public byte set_rh_upper(lprec lp, int rownr, double value)
        { throw new NotImplementedException(); }
        public byte set_rh_lower(lprec lp, int rownr, double value)
        { throw new NotImplementedException(); }
        public int bin_count(lprec lp, byte working)
        { throw new NotImplementedException(); }
        public int MIP_count(lprec lp)
        { throw new NotImplementedException(); }
        public int SOS_count(lprec lp)
        {
            throw new NotImplementedException();
        }
        public int GUB_count(lprec lp)
        { throw new NotImplementedException(); }
        public int identify_GUB(lprec lp, byte mark)
        { throw new NotImplementedException(); }
        public int prepare_GUB(lprec lp)
        { throw new NotImplementedException(); }

        public byte refactRecent(lprec lp)
        { throw new NotImplementedException(); }
        public byte check_if_less(lprec lp, double x, double y, int variable)
        { throw new NotImplementedException(); }
        public byte feasiblePhase1(lprec lp, double epsvalue)
        { throw new NotImplementedException(); }
        public void free_duals(lprec lp)
        { throw new NotImplementedException(); }
        public void initialize_solution(lprec lp, byte shiftbounds)
        { throw new NotImplementedException(); }
        public void recompute_solution(lprec lp, byte shiftbounds)
        { throw new NotImplementedException(); }
        public int verify_solution(lprec lp, byte reinvert, ref string info)
        { throw new NotImplementedException(); }
        public int check_solution(lprec lp, int lastcolumn, ref double solution, ref double upbo, ref double lowbo, double tolerance)
        { throw new NotImplementedException(); }
        public byte is_fixedvar(lprec lp, int variable)
        { throw new NotImplementedException(); }
        public bool is_splitvar(lprec lp, int colnr)
        { throw new NotImplementedException(); }

        public void set_action(ref int actionvar, int actionmask)
        { throw new NotImplementedException(); }
        public void clear_action(ref int actionvar, int actionmask)
        { throw new NotImplementedException(); }
        public bool is_action(int actionvar, int testmask)
        { throw new NotImplementedException(); }

        /* INLINE */
        public byte is_bb_rule(lprec lp, int bb_rule)
        { throw new NotImplementedException(); }
        /* INLINE */
        public byte is_bb_mode(lprec lp, int bb_mask)
        { throw new NotImplementedException(); }
        /* INLINE */
        public int get_piv_rule(lprec lp)
        { throw new NotImplementedException(); }
        public string get_str_piv_rule(int rule)
        { throw new NotImplementedException(); }
        public byte set_var_priority(lprec lp)
        { throw new NotImplementedException(); }
        public int find_sc_bbvar(lprec lp, ref int count)
        { throw new NotImplementedException(); }
        public int find_sos_bbvar(lprec lp, ref int count, byte intsos)
        { throw new NotImplementedException(); }
        public int find_int_bbvar(lprec lp, ref int count, BBrec BB, ref byte isfeasible)
        { throw new NotImplementedException(); }

        /* Solution-related functions */
        public double compute_dualslacks(lprec lp, int target, double[][] dvalues, int[][] nzdvalues, byte dosum)
        { throw new NotImplementedException(); }
        public byte solution_is_int(lprec lp, int index, byte checkfixed)
        { throw new NotImplementedException(); }
        public byte bb_better(lprec lp, int target, int mode)
        { throw new NotImplementedException(); }
        public void construct_solution(lprec lp, ref double target)
        { throw new NotImplementedException(); }
        public void transfer_solution_var(lprec lp, int uservar)
        { throw new NotImplementedException(); }
        public byte construct_duals(lprec lp)
        { throw new NotImplementedException(); }
        public byte construct_sensitivity_duals(lprec lp)
        { throw new NotImplementedException(); }
        public byte construct_sensitivity_obj(lprec lp)
        { throw new NotImplementedException(); }

        public int add_GUB(lprec lp, ref string name, int priority, int count, ref int sosvars)
        { throw new NotImplementedException(); }
        public basisrec push_basis(lprec lp, ref int basisvar, ref byte isbasic, ref byte islower)
        { throw new NotImplementedException(); }
        public byte compare_basis(lprec lp)
        { throw new NotImplementedException(); }
        public byte restore_basis(lprec lp)
        { throw new NotImplementedException(); }
        public byte pop_basis(lprec lp, byte restore)
        { throw new NotImplementedException(); }
        /// <summary>
        /// changed retrun type from byte to bool on 13/11/18
        /// </summary>
        public bool is_BasisReady(lprec lp)
        { throw new NotImplementedException(); }
        public byte is_slackbasis(lprec lp)
        { throw new NotImplementedException(); }
        /// <summary>
        /// changed retrun type from byte to bool on 13/11/18
        /// </summary>
        public bool verify_basis(lprec lp)
        { throw new NotImplementedException(); }
        public int unload_basis(lprec lp, byte restorelast)
        { throw new NotImplementedException(); }

        public int perturb_bounds(lprec lp, BBrec perturbed, byte doRows, byte doCols, byte includeFIXED)
        { throw new NotImplementedException(); }
        public byte validate_bounds(lprec lp, ref double upbo, ref double lowbo)
        { throw new NotImplementedException(); }
        public byte impose_bounds(lprec lp, ref double upbo, ref double lowbo)
        { throw new NotImplementedException(); }
        public int unload_BB(lprec lp)
        { throw new NotImplementedException(); }

        public double feasibilityOffset(lprec lp, byte isdual)
        { throw new NotImplementedException(); }
        public byte isP1extra(lprec lp)
        { throw new NotImplementedException(); }
        public double get_refactfrequency(lprec lp, byte final)
        { throw new NotImplementedException(); }
        public int findBasicFixedvar(lprec lp, int afternr, byte slacksonly)
        { throw new NotImplementedException(); }
        public byte isBasisVarFeasible(lprec lp, double tol, int basis_row)
        { throw new NotImplementedException(); }
        public byte isPrimalFeasible(lprec lp, double tol, int[] infeasibles, ref double feasibilitygap)
        { throw new NotImplementedException(); }
        public byte isDualFeasible(lprec lp, double tol, ref int boundflips, int[] infeasibles, ref double feasibilitygap)
        { throw new NotImplementedException(); }

        /* Main simplex driver routines */
        public int preprocess(lprec lp)
        { throw new NotImplementedException(); }
        public void postprocess(lprec lp)
        { throw new NotImplementedException(); }
        public byte performiteration(lprec lp, int rownr, int varin, double theta, byte primal, byte allowminit, ref double prow, ref int nzprow, ref double pcol, ref int nzpcol, ref int boundswaps)
        { throw new NotImplementedException(); }
        public void transfer_solution(lprec lp, byte dofinal)
        { throw new NotImplementedException(); }

        /* Scaling utilities */
        public double scaled_floor(lprec lp, int colnr, double value, double epsscale)
        { throw new NotImplementedException(); }
        public double scaled_ceil(lprec lp, int colnr, double value, double epsscale)
        { throw new NotImplementedException(); }

        /* Variable mapping utility routines */
        public void varmap_lock(lprec lp)
        { throw new NotImplementedException(); }
        public void varmap_clear(lprec lp)
        { throw new NotImplementedException(); }
        public byte varmap_canunlock(lprec lp)
        { throw new NotImplementedException(); }
        public void varmap_addconstraint(lprec lp)
        { throw new NotImplementedException(); }
        public void varmap_addcolumn(lprec lp)
        { throw new NotImplementedException(); }
        public void varmap_delete(lprec lp, int @base, int delta, LLrec varmap)
        { throw new NotImplementedException(); }
        public void varmap_compact(lprec lp, int prev_rows, int prev_cols)
        { throw new NotImplementedException(); }
        public byte varmap_validate(lprec lp, int varno)
        { throw new NotImplementedException(); }
        /*  unsigned char del_varnameex(lprec *lp, hashelem **namelist, hashtable *ht, int varnr, LLrec *varmap){} */
        public byte del_varnameex(lprec lp, hashelem[] namelist, int items, hashtable ht, int varnr, LLrec varmap)
        { throw new NotImplementedException(); }

        /* Pseudo-cost routines (internal) */
        public BBPSrec init_pseudocost(lprec lp, int pseudotype)
        { throw new NotImplementedException(); }
        public void free_pseudocost(lprec lp)
        { throw new NotImplementedException(); }
        public double get_pseudorange(BBPSrec pc, int mipvar, int varcode)
        { throw new NotImplementedException(); }
        public void update_pseudocost(BBPSrec pc, int mipvar, int varcode, byte capupper, double varsol)
        { throw new NotImplementedException(); }
        public double get_pseudobranchcost(BBPSrec pc, int mipvar, byte dofloor)
        { throw new NotImplementedException(); }
        public double get_pseudonodecost(BBPSrec pc, int mipvar, int vartype, double varsol)
        { throw new NotImplementedException(); }

        /* Matrix access and equation solving routines */
        public void set_OF_override(lprec lp, ref double ofVector)
        { throw new NotImplementedException(); }
        public void set_OF_p1extra(lprec lp, double p1extra)
        { throw new NotImplementedException(); }
        public void unset_OF_p1extra(lprec lp)
        { throw new NotImplementedException(); }
        public byte modifyOF1(lprec lp, int index, ref double ofValue, double mult)
        { throw new NotImplementedException(); }
        public double get_OF_active(lprec lp, int varnr, double mult)
        { throw new NotImplementedException(); }
        public byte is_OF_nz(lprec lp, int colnr)
        { throw new NotImplementedException(); }

        public int get_basisOF(lprec lp, int[] coltarget, double[] crow, int[] colno)
        { throw new NotImplementedException(); }
        public int get_basiscolumn(lprec lp, int j, int[] rn, double[] bj)
        { throw new NotImplementedException(); }
        public int obtain_column(lprec lp, int varin, ref double pcol, ref int nzlist, ref int maxabs)
        { throw new NotImplementedException(); }
        public int compute_theta(lprec lp, int rownr, double theta, int isupbound, double HarrisScalar, byte primal)
        { throw new NotImplementedException(); }

        /* Pivot utility routines */
        public int findBasisPos(lprec lp, int notint, ref int var_basic)
        { throw new NotImplementedException(); }
        public byte check_degeneracy(lprec lp, ref double pcol, ref int degencount)
        { throw new NotImplementedException(); }

        private void varmap_add(lprec lp, int @base, int delta)
        {
            throw new NotImplementedException();
        }

        private bool rename_var(lprec lp, int varindex, ref string new_name, hashelem[] list, hashtable[] ht)
        {
            throw new NotImplementedException();
        }


        
    }
}
