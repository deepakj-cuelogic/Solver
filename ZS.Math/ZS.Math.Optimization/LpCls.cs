using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    class LpCls : lprec

    {
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
        private new byte userabort(lprec lp, int message)
        {
            byte abort;
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
            abort = Convert.ToByte(lp.spx_status != RUNNING);
            if (abort == 0)
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
        private new void set_outputstream(lprec lp, FILE stream)
        {
            /// <summary>
            /// can't identify the actual use of stdout, created a static class and declared stdout as FILE type constant
            /// will have to change the implementation accordingly
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
        private lprec[] read_MPS(ref string filename, int options)
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

        internal lprec make_lp(int rows, int columns)
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
            lp.model_is_pure = 1;
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
            varmap_clear(lp); lp.matA = lp_matrix.mat_create(lp, rows, columns, lprec.epsvalue);
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

        private static void set_infiniteex(lprec lp, double infinite, byte init)
        {
            throw new NotImplementedException();
        }

        public new bool set_lp_name(lprec lp, ref string name)
        {
            if (name == "")
                lp.lp_name = null;
            else
                lp.lp_name = name;
            return true;
        }

        private void reset_params(lprec lp)
        {
            throw new NotImplementedException();
        }

        private bool set_callbacks(lprec lp)
        {
            throw new NotImplementedException();
        }

        private bool set_BFP(lprec lp, ref string filename)
        {
            throw new NotImplementedException();
        }
        private bool set_XLI(lprec lp, ref string filename)
        {
            throw new NotImplementedException();
        }
        public new bool add_constraint(lprec lp, ref double row, int constr_type, double rh)
        {
            return (add_constraintex(lp, 0, ref row, null, constr_type, rh));
        }
        internal new void set_minim(lprec lp)
        {
            set_sense(lp, false);
        }

        /// <summary>
        /// changed from ref int colno to ref object colno on 12/11/18
        /// </summary>
        private bool add_constraintex(lprec lp, int count, ref double row, object colno, int constr_type, double rh)
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
                n = lp.matA.rows;
            }
            if (lp.rows != n)
            {
                string msg = "add_constraintex: Row count mismatch {0} vs {1}\n";
                report(lp, SEVERE, ref msg , lp.rows, n);
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
        internal bool set_unbounded(lprec lp, int colnr)
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


        internal new bool is_int(lprec lp, int colnr)
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
            value = scaled_value(lp, value, rownr);
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
            rename_var(lp, rownr, ref new_name, lp.row_name, lp.rowname_hashtab);

            return true;
        }

        private new bool rename_var(lprec lp, int varindex, ref string new_name, hashelem[] list, hashtable[] ht)
        {
            hashelem hp;
            bool newitem;

            hp = list[varindex];
            newitem = (bool)(hp == null);
            if (newitem)
            {
                hp = lp_Hash.puthash(new_name, varindex, list, ht[0]);
            }
            else if ((hp.name.Length != new_name.Length) || (string.Compare(hp.name, new_name) != 0))
            {
                hashtable newht;
                hashtable oldht;

                //NOT REQUIRED: allocCHAR(lp, hp.name, (int)(new_name.Length + 1), AUTOMATIC);
                hp.name = new_name;
                oldht = ht[0];
                newht = lp_Hash.copy_hash_table(oldht, list, oldht.size);
                ht[0] = newht;
                lp_Hash.free_hash_table(oldht);
            }
            return (newitem);
        }

        private void varmap_add(lprec lp, int @base, int delta)
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

        internal new bool is_infinite(lprec lp, double value)
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

        internal new int SOS_count(lprec lp)
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

        internal new int add_SOS(lprec lp, ref string name, int sostype, int priority, int count, ref int[] sosvars, ref double weights)
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
                    j = sosvars[k];
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
            SOS = lp_SOS.create_SOSrec(lp.SOS, ref name, sostype, priority, count, ref sosvars, ref weights);
            k = lp_SOS.append_SOSgroup(lp.SOS, SOS);

            return (k);
        }

        internal new byte is_semicont(lprec lp, int colnr)
        {
            if ((colnr > lp.columns) || (colnr < 1))
            {
                lp_report objlp_report = new lp_report();
                string msg = "is_semicont: Column {0} out of range\n";
                objlp_report.report(lp, IMPORTANT, ref msg, colnr);
                return (0);
            }

            return ((lp.var_type[colnr] & ISSEMI) != 0);

        }

        internal new bool is_maxim(lprec lp)
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

        private bool set_semicont(lprec lp, int colnr, bool must_be_sc)
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
                lp.var_type[colnr] &= ~ISSEMI;
            }
            lp.sc_lobound[colnr] = must_be_sc;
            if (must_be_sc)
            {
                lp.var_type[colnr] |= ISSEMI;
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
                   lp_matrix.mat_multcol(mat, rownr, -1, 0);
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

        private int get_Lrows(lprec lp)
        {
            if (lp.matL == null)
            {
                return (0);
            }
            else
            {
                return (lp.matL.rows);
            }
        }


        private bool is_constr_type(lprec lp, int rownr, int mask)
        {
            if ((rownr < 0) || (rownr > lp.rows))
            {
                string msg = "is_constr_type: Row {0} out of range\n";
                report(lp, IMPORTANT, ref msg, rownr);
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

            if ((lp.var_type[colnr] & ISINTEGER) != 0)
            {
                lp.int_vars--;
                lp.var_type[colnr] &= ~ISINTEGER;
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

        internal bool str_add_constraint(lprec lp, ref string row_string, int constr_type, double rh)
        {
            int i;
            string p;
            string newp = "";
            double[] aRow = null;
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
                status = objLpCls.add_constraint(lp, ref aRow[0], constr_type, rh);
            }
            //NOT REQUIRED IN C#
            //FREE(aRow);

            return (status);
        }


    }
}
