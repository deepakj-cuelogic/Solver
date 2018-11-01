using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    public class lprec
    {
        /// <summary>
        /// Convert code from 1116 to 1359
        /// </summary>

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
        public byte source_is_file;     // The base model was read from a file
        public byte model_is_pure;      // The model has been built entirely from row and column additions
        public byte model_is_valid;     // Has this lp pased the 'test'
        public byte tighten_on_set;     // Specify if bounds will be tightened or overriden at bound setting
        public byte names_used;         // Flag to indicate if names for rows and columns are used
        public byte use_row_names;      // Flag to indicate if names for rows are used
        public byte use_col_names;      // Flag to indicate if names for columns are used

        public byte lag_trace;          // Print information on Lagrange progression
        public byte spx_trace;          // Print information on simplex progression
        public byte bb_trace;           // TRUE to print extra debug information
        public byte streamowned;        // TRUE if the handle should be closed at delete_lp()
        public byte obj_in_basis;       // TRUE if the objective function is in the basis matrix

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
        public double duals; // rows_alloc+1 : The dual variables of the last LP
        public double full_duals; // sum_alloc+1: Final duals array expanded for deleted variables
        public double dualsfrom; /* sum_alloc+1 :The sensitivity on dual variables/reduced costs
                                   of the last LP */
        public double dualstill; /* sum_alloc+1 :The sensitivity on dual variables/reduced costs
                                   of the last LP */
        public double objfrom; /* columns_alloc+1 :The sensitivity on objective function
                                   of the last LP */
        public double objtill; /* columns_alloc+1 :The sensitivity on objective function
                                   of the last LP */
        public double objfromvalue; /* columns_alloc+1 :The value of the variables when objective value
                                   is at its from value of the last LP */
        public double orig_obj; // Unused pointer - Placeholder for OF not part of B
        public double obj; // Special vector used to temporarily change the OF vector

        public ulong current_iter; // Number of iterations in the current/last simplex
        public ulong total_iter; // Number of iterations over all B&B steps
        public ulong current_bswap; // Number of bound swaps in the current/last simplex
        public ulong total_bswap; // Number of bount swaps over all B&B steps
        public int solvecount; // The number of solve() performed in this model
        public int max_pivots; // Number of pivots between refactorizations of the basis

        /* Various execution parameters */
        public int simplex_strategy; // Set desired combination of primal and dual simplex algorithms
        public int simplex_mode; // Specifies the current simplex mode during solve; see simplex_strategy
        public int verbose; // Set amount of run-time messages and results
        public int print_sol; // TRUE to print optimal solution; AUTOMATIC skips zeros
        public FILE outstream; // Output stream, initialized to STDOUT

        /* Main Branch and Bound settings */
        public byte bb_varbranch; /* Determines branching strategy at the individual variable level;
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
        public byte var_type; // sum_alloc+1 : TRUE if variable must be integer

        /* Data for multiple pricing */
        public multirec multivars;
        public int multiblockdiv; // The divisor used to set or augment pricing block

        /* Variable (column) parameters */
        public int fixedvars; // The current number of basic fixed variables in the model
        public int int_vars; // Number of variables required to be integer

        public int sc_vars; // Number of semi-continuous variables
        public double sc_lobound; /* sum_columns+1 : TRUE if variable is semi-continuous;
                                   value replaced by conventional lower bound during solve */
                                  //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                                  //ORIGINAL LINE: int *var_is_free;
        public int var_is_free; // columns+1: Index of twin variable if variable is free
                                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                                //ORIGINAL LINE: int *var_priority;
        public int var_priority; // columns: Priority-mapping of variables

        public SOSgroup GUB; // Pointer to record containing GUBs

        public int sos_vars; // Number of variables in the sos_priority list
        public int sos_ints; // Number of integers in SOS'es above
        public SOSgroup SOS; // Pointer to record containing all SOS'es
        
        //ORIGINAL LINE: int *sos_priority;
        public int sos_priority; // Priority-sorted list of variables (no duplicates)

        /* Optionally specify list of active rows/columns used in multiple pricing */
        public double bsolveVal; // rows+1: bsolved solution vector for reduced costs

        //ORIGINAL LINE: int *bsolveIdx;
        public int bsolveIdx; // rows+1: Non-zero indeces of bsolveVal

        /* RHS storage */
        public double orig_rhs; /* rows_alloc+1 : The RHS after scaling and sign
                                   changing, but before 'Bound transformation' */
        /// <summary>
        /// 1501
        /// </summary>
        public double rhs; // rows_alloc+1 : The RHS of the current simplex tableau

        /* Row (constraint) parameters */
        //ORIGINAL LINE: int *row_type;
        public int row_type; // rows_alloc+1 : Row/constraint type coding

        /* Optionally specify data for dual long-step */
        public multirec longsteps;

        /* Original and working row and variable bounds */
        public double orig_upbo; // sum_alloc+1 : Bound before transformations
        public double upbo; //  " " : Upper bound after transformation and B&B work
        public double orig_lowbo; //  "       "

        /* User data and basis factorization matrices (ETA or LU, product form) */
        MATrec matA;
        INVrec invB;

        /* Basis and bounds */
        BBrec bb_bounds;         /* The linked list of B&B bounds */
        BBrec rootbounds;        /* The bounds at the lowest B&B level */
        basisrec bb_basis;          /* The linked list of B&B bases */
        basisrec rootbasis;
        OBJmonrec monitor;           /* Objective monitoring record for stalling/degeneracy handling */

        /* Scaling parameters */
        double[] scalars;           /* sum_alloc+1:0..Rows the scaling of the rows,
                                   Rows+1..Sum the scaling of the columns */
        byte scaling_used;       /* TRUE if scaling is used */
        byte columns_scaled;     /* TRUE if the columns are scaled too */
        byte varmap_locked;      /* Determines whether the var_to_orig and orig_to_var are fixed */

        /* Variable state information */
        byte basis_valid;        /* TRUE is the basis is still valid */
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
        int bb_strongbranches;  /* The number of strong B&B branches performed */
        int is_strongbranch;    /* Are we currently in a strong branch mode? */
        int bb_improvements;    /* The number of discrete B&B objective improvement steps */

        /* Solver working variables */
        double rhsmax;             /* The maximum |value| of the rhs vector at any iteration */
        double suminfeas;          /* The working sum of primal and dual infeasibilities */
        double bigM;               /* Original objective weighting in primal phase 1 */
        double P1extraVal;         /* Phase 1 OF/RHS offset for feasibility */
        int P1extraDim;         /* Phase 1 additional columns/rows for feasibility */
        int spx_action;         /* ACTION_ variables for the simplex routine */
        byte spx_perturbed;      /* The variable bounds were relaxed/perturbed into this simplex */
        byte bb_break;           /* Solver working variable; signals break of the B&B */
        byte wasPreprocessed;    /* The solve preprocessing was performed */
        byte wasPresolved;       /* The solve presolver was invoked */
        int INTfuture2;

        /* Lagragean solver storage and parameters */
        MATrec matL;
        double[] lag_rhs;           /* Array of Lagrangean rhs vector */
        int[] lag_con_type;      /* Array of GT, LT or EQ */
        double[] lambda;            /* Lambda values (Lagrangean multipliers) */
        double lag_bound;          /* The Lagrangian lower OF bound */
        double lag_accept;         /* The Lagrangian convergence criterion */

        /* Solver thresholds */
        double infinite;           /* Limit for dynamic range */
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
        int bb_level;           /* Solver B&B working variable (recursion depth) */
        int bb_maxlevel;        /* The deepest B&B level of the last solution */
        int bb_limitlevel;      /* The maximum B&B level allowed */
        long bb_totalnodes;      /* Total number of nodes processed in B&B */
        int bb_solutionlevel;   /* The B&B level of the last / best solution */
        int bb_cutpoolsize;     /* Size of the B&B cut pool */
        int bb_cutpoolused;     /* Currently used cut pool */
        int bb_constraintOF;    /* General purpose B&B parameter (typically for testing) */
        int[] bb_cuttype;        /* The type of the currently used cuts */
        int[] bb_varactive;      /* The B&B state of the variable; 0 means inactive */
        DeltaVrec bb_upperchange;    /* Changes to upper bounds during the B&B phase */
        DeltaVrec bb_lowerchange;    /* Changes to lower bounds during the B&B phase */

        double bb_deltaOF;         /* Minimum OF step value; computed at beginning of solve() */

        double bb_breakOF;         /* User-settable value for the objective function deemed
                               to be sufficiently good in an integer problem */
        double bb_limitOF;         /* "Dual" bound / limit to final optimal MIP solution */
        double bb_heuristicOF;     /* Set initial "at least better than" guess for objective function
                               (can significantly speed up B&B iterations) */
        double bb_parentOF;        /* The OF value of the previous BB simplex */
        double bb_workOF;          /* The unadjusted OF value for the current best solution */

        /* Internal work arrays allocated as required */
        presolveundorec presolve_undo;
        workarraysrec workarrays;

        /* MIP parameters */
        double epsint;             /* Margin of error in determining if a float value is integer */
        double mip_absgap;         /* Absolute MIP gap */
        double mip_relgap;         /* Relative MIP gap */

        /* Time/timer variables and extended status text */
        double timecreate;
        double timestart;
        double timeheuristic;
        double timepresolved;
        double timeend;
        long sectimeout;

        /* Extended status message text set via explain() */
        string ex_status;

        /// Start convert from 1636 to 1710


        /* replacement of static variables */
        string rowcol_name;       /* The name of a row/column */

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
        public const int ISINTEGER = 1;
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
        public const int ANTIDEGEN_BOUNDFLIP = 512;
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
        public const int ITERATE_MAJORMAJOR = 0;
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
        public const int PRICE_ADAPTIVE = 32; /* Temporarily use alternative strategy if cycling is detected */
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
        public const double DEF_TIMEDREFACT = 2; /* Default for timed refactorization in BFPs; can be FALSE, TRUE or AUTOMATIC (dynamic) */

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
        public const double DEF_INFINITE = 1.0e+30;  /* Limit for dynamic range */
        public const double DEF_EPSVALUE = 1.0e-12;  /* High accuracy and feasibility preserving tolerance */
        public const double DEF_EPSPRIMAL = 1.0e-10;  /* For rounding primal/RHS values to 0 */
        public const double DEF_EPSDUAL = 1.0e-09;  /* For rounding reduced costs to 0 */
        public const double DEF_EPSPIVOT = 2.0e-07;  /* Pivot reject threshold */
        public const double DEF_PERTURB = 1.0e-05;  /* Perturbation scalar for degenerate problems; must at least be RANDSCALE greater than EPSPRIMAL */
        public const double DEF_EPSSOLUTION = 1.0e-05;  /* Margin of error for solution bounds */
        public const double DEF_EPSINT = 1.0e-07;  /* Accuracy for considering a float value as integer */

#elif ActivePARAM == OriginalPARAM //* PARAMETER SET FOR LEGACY VERSIONS                  */
        public const double DEF_INFINITE = 1.0e+24;  /* Limit for dynamic range */
        public const double DEF_EPSVALUE = 1.0e-08;  /* High accuracy and feasibility preserving tolerance */
        public const double DEF_EPSPRIMAL = 5.01e-07;  /* For rounding primal/RHS values to 0, infeasibility */
        public const double DEF_EPSDUAL = 1.0e-06;  /* For rounding reduced costs to 0 */
        public const double DEF_EPSPIVOT = 1.0e-04;  /* Pivot reject threshold */
        public const double DEF_PERTURB = 1.0e-05;  /* Perturbation scalar for degenerate problems; must at least be RANDSCALE greater than EPSPRIMAL */
        public const double DEF_EPSSOLUTION = 1.0e-02;  /* Margin of error for solution bounds */
        public const double DEF_EPSINT = 1.0e-03;  /* Accuracy for considering a float value as integer */

#elif ActivePARAM == ChvatalPARAM     //* PARAMETER SET EXAMPLES FROM Vacek Chvatal          */
        public const double DEF_INFINITE = 1.0e+30; /* Limit for dynamic range */
        public const double DEF_EPSVALUE = 1.0e-10; /* High accuracy and feasibility preserving tolerance */
        public const double DEF_EPSPRIMAL = 10e-07; /* For rounding primal/RHS values to 0 */
        public const double DEF_EPSDUAL = 10e-05; /* For rounding reduced costs to 0 */
        public const double DEF_EPSPIVOT = 10e-05; /* Pivot reject threshold */
        public const double DEF_PERTURB = 10e-03; /* Perturbation scalar for degenerate problems; must at least be RANDSCALE greater than EPSPRIMAL */
        public const double DEF_EPSSOLUTION = 1.0e-05; /* Margin of error for solution bounds */
        public const double DEF_EPSINT = 5.0e-03; /* Accuracy for considering a float value as integer */

#elif ActivePARAM == LoosePARAM       //* PARAMETER SET FOR LOOSE TOLERANCES                 */
        public const double DEF_INFINITE = 1.0e+30; /* Limit for dynamic range */
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


    }
}
