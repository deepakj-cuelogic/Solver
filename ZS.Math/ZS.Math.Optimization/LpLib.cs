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
        //dComments: uncertain about using long or double instead of long double datatype in C, hence used double
        //dComments: long is signed 64 bit integer, double is 64 bit floating value, use according to the requirement
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
        /* Prototypes for external language libraries                                */
        /* ------------------------------------------------------------------------- */

        /* Refactorization engine interface routines (for dynamic DLL/SO BFPs) */
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

        /* External language interface routines (for dynamic DLL/SO XLIs) */
        XLIchar xli_name;
        XLIbool_lpintintint xli_compatible;
        XLIbool_lpcharcharcharint xli_readmodel;
        XLIbool_lpcharcharbool xli_writemodel;

        /* Prototypes for callbacks from basis inverse/factorization libraries       */
        /* ------------------------------------------------------------------------- */
        // ORIGINAL LINE: typedef MYBOOL(__WINAPI userabortfunc)(lprec* lp, int level);
        public delegate byte userabortfunc(lprec lp, int level);
        // ORIGINAL LINE: typedef void   (__VACALL reportfunc)(lprec* lp, int level, char* format, ...);
        //dComments: '...' is used to provide variable number of arguments to the function; ref: http://c-faq.com/varargs/varargs1.html
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
        reportfunc report;
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

        /* User program interface callbacks */
        private lphandle_intfunc ctrlc; //dComments: delegate instnace??
        private object ctrlchandle;     /* User-specified "owner process ID" */
        private lphandlestr_func writelog;
        private object loghandle;       // User-specified "owner process ID"
        private lphandlestr_func debuginfo;
        private lphandleint_func usermessage;
        int msgmask;
        private object msghandle;       /* User-specified "owner process ID" */
        private lphandleint_intfunc bb_usenode;
        private object bb_nodehandle; // User-specified "owner process ID"
        private lphandleint_intfunc bb_usebranch;
        private object bb_branchhandle; // User-specified "owner process ID"





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

        public void lp_solve_version(ref int majorversion, ref int minorversion, ref int release, ref int build)
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

        public byte set_lp_name(lprec lp, ref string lpname)
        { throw new NotImplementedException(); }
        public string get_lp_name(lprec lp)
        { throw new NotImplementedException(); }
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
        public void set_sense(lprec lp, byte maximize)
        { throw new NotImplementedException(); }
        public void set_maxim(lprec lp)
        { throw new NotImplementedException(); }
        public void set_minim(lprec lp)
        { throw new NotImplementedException(); }
        public byte is_maxim(lprec lp)
        { throw new NotImplementedException(); }
        /* Set optimization direction for the objective function */

        public byte add_constraint(lprec lp, ref double row, int constr_type, double rh)
        { throw new NotImplementedException(); }
        public byte add_constraintex(lprec lp, int count, ref double row, ref int colno, int constr_type, double rh)
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
        public byte is_int(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        public byte set_binary(lprec lp, int colnr, byte must_be_bin)
        { throw new NotImplementedException(); }
        public byte is_binary(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        public byte set_semicont(lprec lp, int colnr, byte must_be_sc)
        { throw new NotImplementedException(); }
        public byte is_semicont(lprec lp, int colnr)
        { throw new NotImplementedException(); }
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

        public int add_SOS(lprec lp, ref string name, int sostype, int priority, int count, ref int sosvars, ref double weights)
        { throw new NotImplementedException(); }
        public byte is_SOS_var(lprec lp, int colnr)
        { throw new NotImplementedException(); }
        /* Add SOS constraints */

        public byte set_row_name(lprec lp, int rownr, ref string new_name)
        { throw new NotImplementedException(); }
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

        public void set_outputstream(lprec lp, FILE stream)
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
        public byte is_anti_degen(lprec lp, int testmask)
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

        public byte is_infinite(lprec lp, double value)
        { throw new NotImplementedException(); }
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
        public byte is_integerscaling(lprec lp)
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
        public byte append_rows(lprec lp, int deltarows)
        { throw new NotImplementedException(); }
        public byte append_columns(lprec lp, int deltacolumns)
        { throw new NotImplementedException(); }
        public void inc_rows(lprec lp, int delta)
        { throw new NotImplementedException(); }
        public void inc_columns(lprec lp, int delta)
        { throw new NotImplementedException(); }
        public byte init_rowcol_names(lprec lp)
        { throw new NotImplementedException(); }
        public byte inc_row_space(lprec lp, int deltarows)
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

        public byte is_chsign(lprec lp, int rownr)
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
        { throw new NotImplementedException(); }
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
        public byte is_splitvar(lprec lp, int colnr)
        { throw new NotImplementedException(); }

        public void set_action(ref int actionvar, int actionmask)
        { throw new NotImplementedException(); }
        public void clear_action(ref int actionvar, int actionmask)
        { throw new NotImplementedException(); }
        public byte is_action(int actionvar, int testmask)
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
        public byte is_BasisReady(lprec lp)
        { throw new NotImplementedException(); }
        public byte is_slackbasis(lprec lp)
        { throw new NotImplementedException(); }
        public byte verify_basis(lprec lp)
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





    }
}
