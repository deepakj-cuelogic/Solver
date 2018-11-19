using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    /// <summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
    /// changed from 'ref string field1' to 'ref char[] field1' on 15/11/18
    /// </summary>
    public delegate int scan_lineDelegate(lprec lp, int section, ref string line, ref char[] field1, ref string field2, ref string field3, ref double field4, ref string field5, ref double field6);
    
    public class FILE
    {
        public object _Placeholder;
    }

    public class lp_MPS
    {
        scan_lineDelegate scan_line;

        public const string ROWNAMEMASK = "R%d";
        public const string ROWNAMEMASK2 = "r%d";
        public const string COLNAMEMASK = "C%d";
        public const string COLNAMEMASK2 = "c%d";
        /* Read an MPS file */
        public bool MPS_readfile(lprec[] newlp, ref string filename, int typeMPS, int verbose)
        {
            bool status = false;

            FileStream fpin = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read);
            if (fpin != null)
            {
                status = MPS_readhandle(newlp, fpin, typeMPS, verbose);
                ///<summary>
                /// PREVIOUS: fclose(fpin);
                /// ERROR IN PREVIOUS: The name 'fclose' does not exist in the current context
                /// FIX 1: fpin.Close();
                ///</summary>
                fpin.Close();
            }
            return (status);
        }

        private bool MPS_readhandle(lprec[] newlp, FileStream filehandle, int typeMPS, int verbose)
        {
            return (MPS_readex(newlp, (object)filehandle, MPS_input, typeMPS, verbose));
        }

        private bool MPS_readex(lprec[] newlp, object userhandle, lp_lib.read_modeldata_func read_modeldata, int typeMPS, int verbose)
        {

            char[] field1 = null;    //can use string instead?? FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
            string field2 = new string(new char[lp_lib.BUFSIZ]);
            string field3 = new string(new char[lp_lib.BUFSIZ]);
            string field5 = new string(new char[lp_lib.BUFSIZ]);
            string line = new string(new char[lp_lib.BUFSIZ]);
            string tmp = new string(new char[lp_lib.BUFSIZ]);
            string Last_col_name = new string(new char[lp_lib.BUFSIZ]);
            //string probname = new string(new char[lp_lib.BUFSIZ]);
            char probname;
            string OBJNAME = new string(new char[lp_lib.BUFSIZ]);
            char ptr;   //previously string ptr; changed on 12/11/18
            int items;
            int row;
            int Lineno;
            int @var;
            int section = lp_lib.MPSUNDEF;
            int variant = 0;
            int NZ = 0;
            int SOS = 0;
            bool Int_section;
            bool Column_ready;
            bool Column_ready1;
            bool Unconstrained_rows_found = false;
            bool OF_found = false;
            bool CompleteStatus = false;
            double field4 = 0;
            double field6 = 0;
            //ORIGINAL LINE: double *Last_column = null;
            double?[] Last_column = null;
            int count = 0;
            ///<summary>
            /// PREVIOUS: int?[] Last_columnno = 0;
            /// ERROR IN PREVIOUS: Cannot implicitly convert type 'int' to 'int?[]'
            /// FIX 1: int?[] Last_columnno = null; NO ERROR
            /// </summary>
            int?[] Last_columnno = null;
            int OBJSENSE = lp_lib.ROWTYPE_EMPTY;
            lprec lp;
            //lp_lib objlpLib = new lp_lib();
            LpCls objLpCls = new LpCls();

            if (newlp == null)
                return (CompleteStatus);
            else if (newlp[0] == null)
                lp = objLpCls.make_lp(0, 0);
            else
                lp = newlp[0];

            if ((typeMPS & lp_lib.MPSFIXED) == lp_lib.MPSFIXED)
            {
                scan_line = scan_lineFIXED;
            }
            else if ((typeMPS & lp_lib.MPSFREE) == lp_lib.MPSFREE)
            {
                scan_line = scan_lineFREE;
            }
            else
            {
                lp_report objlp_report = new lp_report();
                string msg = "MPS_readfile: Unrecognized MPS line type.\n";
                objlp_report.report(lp, lp_lib.IMPORTANT, ref msg);
                if (newlp[0] == null)
                {
                    lp_lib objlp_lib = new lp_lib();
                    objlp_lib.delete_lp(lp);
                }
                return (CompleteStatus);
            }

            if (lp != null)
            {
                lp.source_is_file = true;
                lp.verbose = verbose;
                Last_col_name = "";
                OBJNAME = "";
                Int_section = false;
                Column_ready = false;
                Lineno = 0;

                /*ORIGINAL LINE: // let's initialize line to all zero's 
                //NOT REQUIRED IN C# MEMCLEAR(line, BUFSIZ);*/
                lp_report objlp_report = new lp_report();
                while (read_modeldata(userhandle, line, lp_lib.BUFSIZ - 1) == 1)
                {
                    Lineno++;

                    /*ORIGINAL LINE:for (ptr = line; (*ptr) && (isspace((unsigned char) * ptr)) ; ptr++);
                     COMMENTS:seems useless
                    for (ptr = line; ptr && (char.IsWhiteSpace(ptr)); ptr++)
                    {
                        ;
                    }*/
                    string msg = "";

                    //ORIGINAL LINE:for (ptr = line; (*ptr) && (isspace((unsigned char) * ptr)) ; ptr++);
                    for (ptr = line[0]; ptr < line.Length; ptr++)
                    {
                        /* skip lines which start with "*", they are comment */
                        if ((ptr == '*') || (ptr == 0) || (ptr == '\n') || (ptr == '\r'))
                        {
                            msg = "Comment on line {0}: {1}";
                            objlp_report.report(lp, lp_lib.FULL, ref msg, Lineno, line);
                            continue;
                        }
                    }

                    msg = "Line {0}: {1}";
                    objlp_report.report(lp, lp_lib.FULL, ref msg, Lineno, line);

                    /* first check for "special" lines: NAME, ROWS, BOUNDS .... */
                    /* this must start in the first position of line */
                    if (line[0] != ' ')
                    {
                        //sscanf(line, "%s", tmp);
                        if (string.Compare(tmp, "NAME") == 0)
                        {
                            section = lp_lib.MPSNAME;
                            probname = (char)0;
                            //sscanf(line, "NAME %s", probname);
                            if (!objLpCls.set_lp_name(lp, probname))
                            {
                                break;
                            }
                        }
                        else if (((typeMPS & lp_lib.MPSFREE) == lp_lib.MPSFREE) && (string.Compare(tmp, "OBJSENSE") == 0))
                        {
                            section = lp_lib.MPSOBJSENSE;
                            msg = "Switching to OBJSENSE section\n";
                            objlp_report.report(lp, lp_lib.FULL, ref msg);
                        }
                        else if (((typeMPS & lp_lib.MPSFREE) == lp_lib.MPSFREE) && (string.Compare(tmp, "OBJNAME") == 0))
                        {
                            section = lp_lib.MPSOBJNAME;
                            msg = "Switching to OBJNAME section\n";
                            objlp_report.report(lp, lp_lib.FULL, ref msg);
                        }
                        else if (string.Compare(tmp, "ROWS") == 0)
                        {
                            section = lp_lib.MPSROWS;
                            msg = "Switching to ROWS section\n";
                            objlp_report.report(lp, lp_lib.FULL, ref msg);
                        }
                        else if (string.Compare(tmp, "COLUMNS") == 0)
                        {
                            /* NOT REQUIRED IN C#
                            allocREAL(lp, Last_column, lp.rows + 1, 1);
                            allocINT(lp, Last_columnno, lp.rows + 1, 1);
                            */
                            count = 0;
                            if ((Last_column == null) || (Last_columnno == null))
                            {
                                break;
                            }
                            section = lp_lib.MPSCOLUMNS;
                            msg = "Switching to COLUMNS section\n";
                            objlp_report.report(lp, lp_lib.FULL, ref msg);
                        }
                        else if (string.Compare(tmp, "RHS") == 0)
                        {
                            if (!addmpscolumn(lp, Int_section, typeMPS, ref Column_ready, ref count, Last_column, ref Last_columnno, ref Last_col_name))
                            {
                                break;
                            }
                            section = lp_lib.MPSRHS;
                            msg = "Switching to RHS section\n";
                            objlp_report.report(lp, lp_lib.FULL, ref msg);
                        }
                        else if (string.Compare(tmp, "BOUNDS") == 0)
                        {
                            section = lp_lib.MPSBOUNDS;
                            msg = "Switching to BOUNDS section\n";
                            objlp_report.report(lp, lp_lib.FULL, ref msg);
                        }
                        else if (string.Compare(tmp, "RANGES") == 0)
                        {
                            section = lp_lib.MPSRANGES;
                            msg = "Switching to RANGES section\n";
                            objlp_report.report(lp, lp_lib.FULL, ref msg);
                        }
                        else if ((string.Compare(tmp, "SOS") == 0) || (string.Compare(tmp, "SETS") == 0))
                        {
                            section = lp_lib.MPSSOS;
                            if (string.Compare(tmp, "SOS") == 0)
                            {
                                variant = 0;
                            }
                            else
                            {
                                variant = 1;
                            }
                            msg = "Switching to {0} section\n";
                            objlp_report.report(lp, lp_lib.FULL, ref msg, tmp);
                        }
                        else if (string.Compare(tmp, "ENDATA") == 0)
                        {
                            msg = "Finished reading MPS file\n";
                            objlp_report.report(lp, lp_lib.FULL, ref msg);
                            CompleteStatus = true;
                            break;
                        }
                        else
                        { // line does not start with space and does not match above
                            msg = "Unrecognized MPS line {0}: {1}\n";
                            objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, Lineno, line);
                            break;
                        }
                    }
                    else   /* normal line, process */
                    {
                        items = scan_line(lp, section, ref line, ref field1, ref field2, ref field3, ref field4, ref field5, ref field6);
                        if (items < 0)
                        {
                            msg = "Syntax error on line {0}: {1}\n";
                            objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, Lineno, line);
                            break;
                        }
                        switch (section)
                        {
                            case lp_lib.MPSNAME:
                                msg = "Error, extra line under NAME line\n";
                                objlp_report.report(lp, lp_lib.IMPORTANT, ref msg);
                                break;

                            case lp_lib.MPSOBJSENSE:
                                if (OBJSENSE != lp_lib.ROWTYPE_EMPTY)
                                {
                                    msg = "Error, extra line under OBJSENSE line\n";
                                    objlp_report.report(lp, lp_lib.IMPORTANT, ref msg);
                                    break;
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                if ((string.Compare(field1.ToString(), "MAXIMIZE") == 0) || (string.Compare(field1.ToString(), "MAX") == 0))
                                {
                                    OBJSENSE = lp_lib.ROWTYPE_OFMAX;
                                    objLpCls.set_maxim(lp);
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if ((string.Compare(field1.ToString(), "MINIMIZE") == 0) || (string.Compare(field1.ToString(), "MIN") == 0))
                                {
                                    OBJSENSE = lp_lib.ROWTYPE_OFMIN;
                                    objLpCls.set_minim(lp);
                                }
                                else
                                {
                                    msg = "Unknown OBJSENSE direction '{0}' on line {1}\n";
                                    objlp_report.report(lp, lp_lib.SEVERE, ref msg, field1, Lineno);
                                    break;
                                }
                                continue;

                            case lp_lib.MPSOBJNAME:
                                if (OBJNAME == "")        //TODO:Need to check this condition later
                                {
                                    msg = "Error, extra line under OBJNAME line\n";
                                    objlp_report.report(lp, lp_lib.IMPORTANT, ref msg);
                                    break;
                                }
                                /// <summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                /// </summary>
                                OBJNAME = field1.ToString();
                                continue;

                            /* Process entries in the ROWS section */
                            case lp_lib.MPSROWS:
                                /* field1: rel. operator; field2: name of constraint */
                                msg = "Row   {0}: {1} {2}\n";
                                objlp_report.report(lp, lp_lib.FULL, ref msg, lp.rows + 1, field1, field2);
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                if (string.Compare(field1.ToString(), "N") == 0)
                                {
                                    if ((OBJNAME == "") && (string.Compare(field2, OBJNAME) > 0))   //TODO:Need to check this condition later
                                    {
                                        /* Ignore this objective name since it is not equal to the OBJNAME name */
                                        ;
                                    }
                                    else if (!OF_found)
                                    { // take the first N row as OF, ignore others
                                        if (!objLpCls.set_row_name(lp, 0, ref field2))
                                        {
                                            break;
                                        }
                                        OF_found = true;
                                    }
                                    else if (!Unconstrained_rows_found)
                                    {
                                        msg = "Unconstrained row {0} ignored\n";
                                        objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, field2);
                                        msg = "Further messages of this kind will be suppressed\n";
                                        objlp_report.report(lp, lp_lib.IMPORTANT, ref msg);
                                        Unconstrained_rows_found = true;
                                    }
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "L") == 0)
                                {
                                    msg = "";
                                    if ((!objLpCls.str_add_constraint(lp, ref msg, lp_lib.LE, 0)) || (!objLpCls.set_row_name(lp, lp.rows, ref field2)))
                                    {
                                        break;
                                    }
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "G") == 0)
                                {
                                    msg = "";
                                    if ((!objLpCls.str_add_constraint(lp, ref msg, lp_lib.GE, 0)) || (!objLpCls.set_row_name(lp, lp.rows, ref field2)))
                                    {
                                        break;
                                    }
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "E") == 0)
                                {
                                    msg = "";
                                    if ((!objLpCls.str_add_constraint(lp, ref msg, lp_lib.EQ, 0)) || (!objLpCls.set_row_name(lp, lp.rows, ref field2)))
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    msg = "Unknown relation code '{0}' on line {1}\n";
                                    objlp_report.report(lp, lp_lib.SEVERE, ref msg, field1, Lineno);
                                    break;
                                }

                                continue;

                            /* Process entries in the COLUMNS section */
                            case lp_lib.MPSCOLUMNS:
                                /* field2: variable; field3: constraint; field4: coef */
                                /* optional: field5: constraint; field6: coef */
                                msg = "Column {0}: {1} {2} {3} {4} {5}\n";
                                objlp_report.report(lp, lp_lib.FULL, ref msg, lp.columns + 1, field2, field3, field4, field5, field6);

                                if ((items == 4) || (items == 5) || (items == 6))
                                {
                                    if (NZ == 0)
                                    {
                                        Last_col_name = field2;
                                    }
                                    else if (field2 != "")  //Need to check this condition
                                    {
                                        Column_ready1 = (bool)(string.Compare(field2, Last_col_name) != 0);
                                        if (Column_ready1)
                                        {
                                            if (lp_Hash.find_var(lp, field2, false) >= 0)
                                            {
                                                msg = "Variable name ({0}) is already used!\n";
                                                objlp_report.report(lp, lp_lib.SEVERE, ref msg, field2);
                                                break;
                                            }

                                            if (Column_ready)
                                            { // Added ability to handle non-standard "same as above" column name
                                                if (addmpscolumn(lp, Int_section, typeMPS, ref Column_ready, ref count, Last_column, ref Last_columnno, ref Last_col_name))
                                                {
                                                    Last_col_name = field2;
                                                    NZ = 0;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (items == 5)
                                    { // there might be an INTEND or INTORG marker
                                      /* look for "    <name>  'MARKER'                 'INTORG'"
                                               or "    <name>  'MARKER'                 'INTEND'"  */
                                        if (string.Compare(field3, "'MARKER'") != 0)
                                        {
                                            break;
                                        }
                                        if (string.Compare(field5, "'INTORG'") == 0)
                                        {
                                            Int_section = true;
                                            msg = "Switching to integer section\n";
                                            objlp_report.report(lp, lp_lib.FULL, ref msg);
                                        }
                                        else if (string.Compare(field5, "'INTEND'") == 0)
                                        {
                                            Int_section = false;
                                            msg = "Switching to non-integer section\n";
                                            objlp_report.report(lp, lp_lib.FULL, ref msg);
                                        }
                                        else
                                        {
                                            msg = "Unknown marker (ignored) at line {0}: {1}\n";
                                            objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, Lineno, field5);
                                        }
                                    }
                                    else if ((row = lp_Hash.find_row(lp, field3, Unconstrained_rows_found)) >= 0)
                                    {
                                        if (row > lp.rows)
                                        {
                                            msg = "Invalid row %s encountered in the MPS file\n";
                                            objlp_report.report(lp, lp_lib.CRITICAL, ref msg, field3);
                                        }
                                        Last_columnno[count] = row;
                                        Last_column[count] = (double)field4;
                                        if (appendmpsitem(ref count, Last_columnno, Last_column))
                                        {
                                            NZ++;
                                            Column_ready = true;
                                        }
                                    }
                                }
                                if (items == 6)
                                {
                                    if ((row = lp_Hash.find_row(lp, field5, Unconstrained_rows_found)) >= 0)
                                    {
                                        if (row > lp.rows)
                                        {
                                            msg = "Invalid row %s encountered in the MPS file\n";
                                            objlp_report.report(lp, lp_lib.CRITICAL, ref msg, field5);
                                        }
                                        Last_columnno[count] = row;
                                        Last_column[count] = (double)field6;
                                        if (appendmpsitem(ref count, Last_columnno, Last_column))
                                        {
                                            NZ++;
                                            Column_ready = true;
                                        }
                                    }
                                }

                                if ((items < 4) || (items > 6))
                                { // Wrong!
                                    msg = "Wrong number of items ({0}) in COLUMNS section (line {1})\n";
                                    objlp_report.report(lp, lp_lib.CRITICAL, ref msg, items, Lineno);
                                    break;
                                }

                                continue;

                            /* Process entries in the RHS section */
                            /* field2: uninteresting name; field3: constraint name */
                            /* field4: value */
                            /* optional: field5: constraint name; field6: value */
                            case lp_lib.MPSRHS:
                                msg = "RHS line: {0} {1} {2} {3} {4}\n";
                                objlp_report.report(lp, lp_lib.FULL, ref msg, field2, field3, field4, field5, field6);

                                if ((items != 4) && (items != 6))
                                {
                                    msg = "Wrong number of items ({0}) in RHS section line {1}\n";
                                    objlp_report.report(lp, lp_lib.CRITICAL, ref msg, items, Lineno);
                                    break;
                                }

                                if ((row = lp_Hash.find_row(lp, field3, Unconstrained_rows_found)) >= 0)
                                {
                                    if ((row == 0) && ((typeMPS & lp_lib.MPSNEGOBJCONST) == lp_lib.MPSNEGOBJCONST))
                                    {
                                        field4 = -field4;
                                    }
                                    objLpCls.set_rh(lp, row, field4);
                                }
                                if (items == 6)
                                {
                                    if ((row = lp_Hash.find_row(lp, field5, Unconstrained_rows_found)) >= 0)
                                    {
                                        if ((row == 0) && ((typeMPS & lp_lib.MPSNEGOBJCONST) == lp_lib.MPSNEGOBJCONST))
                                        {
                                            field6 = -field6;
                                        }
                                        objLpCls.set_rh(lp, row, field6);
                                    }
                                }

                                continue;

                            /* Process entries in the BOUNDS section */
                            /* field1: bound type; field2: uninteresting name; */
                            /* field3: variable name; field4: value */
                            case lp_lib.MPSBOUNDS:
                                msg = "BOUNDS line: {0} {1} {2} {3}\n";
                                objlp_report.report(lp, lp_lib.FULL, ref msg, field1, field2, field3, field4);

                                @var = lp_Hash.find_var(lp, field3, false);
                                if (@var < 0)
                                { // bound on undefined var in COLUMNS section ...
                                    Column_ready = true;
                                    if (!addmpscolumn(lp, false, typeMPS, ref Column_ready, ref count, Last_column, ref Last_columnno, ref field3))
                                    {
                                        break;
                                    }
                                    Column_ready = true;
                                    @var = lp_Hash.find_var(lp, field3, true);
                                }
                                if (@var < 0)
                                {
                                    ;
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "UP") == 0)
                                {
                                    /* upper bound */
                                    /* if(!set_bounds(lp, var, get_lowbo(lp, var), field4)) */
                                    if (!objLpCls.set_upbo(lp, @var, field4))
                                    {
                                        break;
                                    }
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "SC") == 0)
                                {
                                    /* upper bound */
                                    if (field4 == 0)
                                    {
                                        field4 = lp.infinite;
                                    }
                                    /* if(!set_bounds(lp, var, get_lowbo(lp, var), field4)) */
                                    if (!objLpCls.set_upbo(lp, @var, field4))
                                    {
                                        break;
                                    }
                                    objLpCls.set_semicont(lp, @var, 1);
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "SI") == 0)
                                {
                                    /* upper bound */
                                    if (field4 == 0)
                                    {
                                        field4 = lp.infinite;
                                    }
                                    /* if(!set_bounds(lp, var, get_lowbo(lp, var), field4)) */
                                    if (!objLpCls.set_upbo(lp, @var, field4))
                                    {
                                        break;
                                    }
                                    objLpCls.set_int(lp, @var, 1);
                                    objLpCls.set_semicont(lp, @var, 1);
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "LO") == 0)
                                {
                                    /* lower bound */
                                    /* if(!set_bounds(lp, var, field4, get_upbo(lp, var))) */
                                    if (!objLpCls.set_lowbo(lp, @var, field4))
                                    {
                                        break;
                                    }
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "PL") == 0)
                                { // plus-ranged variable
                                  /* if(!set_bounds(lp, var, get_lowbo(lp, var), lp->infinite)) */
                                    if (!objLpCls.set_upbo(lp, @var, lp.infinite))
                                    {
                                        break;
                                    }
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "MI") == 0)
                                { // minus-ranged variable
                                  /* if(!set_bounds(lp, var, -lp->infinite, get_upbo(lp, var))) */
                                    if (!objLpCls.set_lowbo(lp, @var, -lp.infinite))
                                    {
                                        break;
                                    }
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "FR") == 0)
                                { // free variable
                                    objLpCls.set_unbounded(lp, @var);
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "FX") == 0)
                                {
                                    /* fixed, upper _and_ lower  */
                                    if (!objLpCls.set_bounds(lp, @var, field4, field4))
                                    {
                                        break;
                                    }
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "BV") == 0)
                                { // binary variable
                                    objLpCls.set_binary(lp, @var, 1);
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                /* AMPL bounds type UI and LI added by E.Imamura (CRIEPI)  */
                                else if (string.Compare(field1.ToString(), "UI") == 0)
                                { // upper bound for integer variable
                                  /* if(!set_bounds(lp, var, get_lowbo(lp, var), field4)) */
                                    if (!objLpCls.set_upbo(lp, @var, field4))
                                    {
                                        break;
                                    }
                                    objLpCls.set_int(lp, @var, 1);
                                }
                                ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                /// changed from 'field1' to 'field1.ToString()' on 15/11/18
                                ///</summary>
                                else if (string.Compare(field1.ToString(), "LI") == 0)
                                { // lower bound for integer variable - corrected by KE
                                  /* if(!set_bounds(lp, var, field4, get_upbo(lp, var))) */
                                    if (!objLpCls.set_lowbo(lp, @var, field4))
                                    {
                                        break;
                                    }
                                    objLpCls.set_int(lp, @var, 1);
                                }
                                else
                                {
                                    msg = "BOUND type {0} on line {1} is not supported";
                                    objlp_report.report(lp, lp_lib.CRITICAL, ref msg, field1, Lineno);
                                    break;
                                }

                                continue;

                            /* Process entries in the BOUNDS section */

                            /* We have to implement the following semantics:

                            D. The RANGES section is for constraints of the form: h <=
                            constraint <= u .  The range of the constraint is r = u - h .  The
                            value of r is specified in the RANGES section, and the value of u or
                            h is specified in the RHS section.  If b is the value entered in the
                            RHS section, and r is the value entered in the RANGES section, then
                            u and h are thus defined:

                            row type       sign of r       h          u
                            ----------------------------------------------
                           G            + or -         b        b + |r|
                           L            + or -       b - |r|      b
                           E              +            b        b + |r|
                           E              -          b - |r|      b            */

                            /* field2: uninteresting name; field3: constraint name */
                            /* field4: value */
                            /* optional: field5: constraint name; field6: value */

                            case lp_lib.MPSRANGES:
                                msg = "RANGES line: {0} {1} {2} {3} {4}";
                                objlp_report.report(lp, lp_lib.FULL, ref msg, field2, field3, field4, field5, field6);

                                if ((items != 4) && (items != 6))
                                {
                                    msg = "Wrong number of items ({0}) in RANGES section line {1}";
                                    objlp_report.report(lp, lp_lib.CRITICAL, ref msg, items, Lineno);
                                    break;
                                }

                                if ((row = lp_Hash.find_row(lp, field3, Unconstrained_rows_found)) >= 0)
                                {
                                    /* Determine constraint type */

                                    if (System.Math.Abs(field4) >= lp.infinite)
                                    {
                                        msg = "Warning, Range for row {0} >= infinity (value {1}) on line {2}, ignored";
                                        objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, field3, field4, Lineno);
                                    }
                                    else if (field4 == 0)
                                    {
                                        /* Change of a GE or LE to EQ */
                                        if (lp.orig_upbo[row] != 0)
                                        {
                                            objLpCls.set_constr_type(lp, row, lp_lib.EQ);
                                        }
                                    }
                                    else if (objLpCls.is_chsign(lp, row))
                                    {
                                        /* GE */
                                        lp.orig_upbo[row] = System.Math.Abs(field4);
                                    }
                                    else if ((lp.orig_upbo[row] == 0) && (field4 >= 0))
                                    {
                                        /*  EQ with positive sign of r value */
                                        objLpCls.set_constr_type(lp, row, lp_lib.GE);
                                        lp.orig_upbo[row] = field4;
                                    }
                                    else if (lp.orig_upbo[row] == lp.infinite)
                                    {
                                        /* LE */
                                        lp.orig_upbo[row] = System.Math.Abs(field4);
                                    }
                                    else if ((lp.orig_upbo[row] == 0) && (field4 < 0))
                                    {
                                        /* EQ with negative sign of r value */
                                        objLpCls.set_constr_type(lp, row, lp_lib.LE);
                                        lp.orig_upbo[row] = lp_types.my_flipsign(field4);
                                    }
                                    else
                                    { // let's be paranoid
                                        msg = "Cannot figure out row type, row = {0}, objlp_lib.is_chsign = {1}, upbo = {2} on line {3}";
                                        objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, row, objLpCls.is_chsign(lp, row), (double)lp.orig_upbo[row], Lineno);
                                    }
                                }

                                if (items == 6)
                                {
                                    if ((row = lp_Hash.find_row(lp, field5, Unconstrained_rows_found)) >= 0)
                                    {
                                        /* Determine constraint type */

                                        if (System.Math.Abs(field6) >= lp.infinite)
                                        {
                                            msg = "Warning, Range for row {0} >= infinity (value {1}) on line {2}, ignored";
                                            objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, field5, field6, Lineno);
                                        }
                                        else if (field6 == 0)
                                        {
                                            /* Change of a GE or LE to EQ */
                                            if (lp.orig_upbo[row] != 0)
                                            {
                                                objLpCls.set_constr_type(lp, row, lp_lib.EQ);
                                            }
                                        }
                                        else if (objLpCls.is_chsign(lp, row))
                                        {
                                            /* GE */
                                            lp.orig_upbo[row] = System.Math.Abs(field6);
                                        }
                                        else if (lp.orig_upbo[row] == 0 && field6 >= 0)
                                        {
                                            /*  EQ with positive sign of r value */
                                            objLpCls.set_constr_type(lp, row, lp_lib.GE);
                                            lp.orig_upbo[row] = field6;
                                        }
                                        else if (lp.orig_upbo[row] == lp.infinite)
                                        {
                                            /* LE */
                                            lp.orig_upbo[row] = System.Math.Abs(field6);
                                        }
                                        else if ((lp.orig_upbo[row] == 0) && (field6 < 0))
                                        {
                                            /* EQ with negative sign of r value */
                                            objLpCls.set_constr_type(lp, row, lp_lib.LE);
                                            lp.orig_upbo[row] = lp_types.my_flipsign(field6);
                                        }
                                        else
                                        { // let's be paranoid
                                            msg = "Cannot figure out row type, row = {0}, objlp_lib.is_chsign = {1}, upbo = {2} on line {3}";
                                            objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, row, objLpCls.is_chsign(lp, row), (double)lp.orig_upbo[row], Lineno);
                                        }
                                    }
                                }

                                continue;

                            /* Process entries in the SOS section */

                            /* We have to implement the following semantics:

                              E. The SOS section is for ordered variable sets of the form:
                          x1, x2, x3 ... xn where only a given number of consequtive variables
                              may be non-zero.  Each set definition is prefaced by type, name
                          and priority data.  Each set member has an optional weight that
                          determines its order.  There are two forms supported; a full format
                          and a reduced CPLEX-like format.                                       */

                            case lp_lib.MPSSOS:
                                msg = "SOS line: {0} {1} {2} {3} {4}";
                                objlp_report.report(lp, lp_lib.FULL, ref msg, field2, field3, field4, field5, field6);

                                if ((items == 0) || (items > 4))
                                {
                                    msg = "Invalid number of items ({0}) in SOS section line {1}\n";
                                    objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, items, Lineno);
                                    break;
                                }

                                if (field1.Length == 0)
                                {
                                    items--; // fix scanline anomoly!
                                }

                                /* Check if this is the start of a new SOS */
                                if (items == 1 || items == 4)
                                {
                                    row = (int)(field1[1] - '0');
                                    if ((row <= 0) || (row > 9))
                                    {
                                        msg = "Error: Invalid SOS type {0} line {1}\n";
                                        objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, field1, Lineno);
                                        break;
                                    }
                                    ///<summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
                                    /// PREVIOUS: field1[0] = '\0';
                                    /// ERROR IN PREVIOUS: Property or indexer 'string.this[int]' cannot be assigned to --it is read only
                                    /// FIX 1: changed from 'string field1 = new string(new char[lp_lib.BUFSIZ]);' to 
                                    /// char[] field1 = null; 
                                    ///</summary>
                                    field1[0] = '\0'; // fix scanline anomoly!

                                    /* lp_solve needs a name for the SOS */
                                    if (variant == 0)
                                    {
                                        if (field3.Length == 0) // CPLEX format does not provide a SOS name; create one
                                        {
                                            field3 = string.Format("SOS_{0:D}", objLpCls.SOS_count(lp) + 1);
                                        }
                                    }
                                    else
                                    { // Remap XPRESS format name
                                        field3 = field1.ToString();
                                    }
                                    /* Obtain the SOS priority */
                                    if (items == 4)
                                    {
                                        SOS = (int)field4;
                                    }
                                    else
                                    {
                                        SOS = 1;
                                    }

                                    /* Define a new SOS instance */

                                    SOS = objLpCls.add_SOS(lp, ref field3, (int)row, SOS, 0, null, null);
                                }
                                /* Otherwise, add set members to the active SOS */
                                else
                                {
                                    string field = (items == 3) ? field3 : field2;

                                    @var = lp_Hash.find_var(lp, field, false); // Native lp_solve and XPRESS formats
                                    if (@var < 0)
                                    { // SOS on undefined var in COLUMNS section ...
                                        Column_ready = true;
                                        if (!addmpscolumn(lp, false, typeMPS, ref Column_ready, ref count, Last_column, ref Last_columnno, ref field))
                                        {
                                            break;
                                        }
                                        Column_ready = true;
                                        @var = lp_Hash.find_var(lp, field, true);
                                    }
                                    if ((@var < 0) || (SOS < 1))
                                    {
                                        ;
                                    }
                                    else
                                    {
                                        lp_SOS.append_SOSrec(lp.SOS.sos_list[SOS - 1], 1, ref @var, ref field4);
                                    }
                                }

                                continue;
                        }

                        /* If we got here there was an error "upstream" */
                        msg = "Error: Cannot handle line {0}\n";
                        objlp_report.report(lp, lp_lib.IMPORTANT, ref msg, Lineno);
                        break;
                    }
                }

                if ((OBJNAME != "") && (!OF_found)) //need to check the first condition
                {
                    string msg = "Error: Objective function specified by OBJNAME card not found\n";
                    objlp_report.report(lp, lp_lib.IMPORTANT, ref msg);
                    CompleteStatus = false;
                }

                /// <summary>
                /// logic to free up the memory used by the lp object; NOT REQUIRED
                /// </summary>
                /*if (CompleteStatus == 0)
                {

                    if (*newlp == null)
                    {
                        
                        //objLpCls.delete_lp(lp);   
                    }
                }*/
                /*else
                {*/
                if ((typeMPS & lp_lib.MPSIBM) != 0)
                {
                    double lower;
                    double upper;

                    for (@var = 1; @var <= lp.columns; @var++)
                    {
                        if (objLpCls.is_int(lp, @var))
                        {
                            lower = objLpCls.get_lowbo(lp, @var);
                            upper = objLpCls.get_upbo(lp, @var);
                            if ((lower == 10.0 / lp_lib.DEF_INFINITE) && (upper == lp_lib.DEF_INFINITE / 10.0))
                            {
                                upper = 1.0;
                            }
                            if (lower == 10.0 / lp_lib.DEF_INFINITE)
                            {
                                lower = 0.0;
                            }
                            if (upper == lp_lib.DEF_INFINITE / 10.0)
                            {
                                upper = lp.infinite;
                            }
                            objLpCls.set_bounds(lp, @var, lower, upper);
                        }
                    }
                }
                ///<summary>
                /// PREVIOUS: newlp = lp;
                /// ERROR IN PREVIOUS: Cannot implicitly convert type 'ZS.Math.Optimization.lprec' to 'ZS.Math.Optimization.lprec[]'
                /// FIX 1: changed to 'newlp[0] = lp;'
                ///</summary>
                newlp[0] = lp;
            //}
                /*NOT REQUIRED
                if (Last_column != null)
                {
                    FREE(Last_column);
                }
                if (Last_columnno != null)
                {
                    FREE(Last_columnno);
                }
                */
            }

            return (CompleteStatus);
        }

        private static bool appendmpsitem(ref int count, int?[] rowIndex, double?[] rowValue)
        {
            int i = count;

            /* Check for non-negativity of the index */
            if (rowIndex[i] < 0)
            {
                return false;
            }

            /* Move the element so that the index list is sorted ascending */
            while ((i > 0) && (rowIndex[i] < rowIndex[i - 1]))
            {
                ///<summary>
                /// PREVIOUS: lp_utils.swapINT(rowIndex + i, rowIndex + i - 1);
                /// ERROR IN PREVIOUS: Operator '+' cannot be applied to operands of type 'int?[]' and 'int' 
                /// FIX 1: int param1 = (rowIndex != null) ? Convert.ToInt32(rowIndex) + i : 0 + i;
                ///        int param2 = (rowIndex != null) ? Convert.ToInt32(rowIndex) + i - 1 : 0 + i - 1;
                /// lp_utils.swapINT(ref param1, ref param2);
                /// </summary>
                int intParam1 = (rowIndex != null) ? Convert.ToInt32(rowIndex) + i : 0 + i;
                int intParam2 = (rowIndex != null) ? Convert.ToInt32(rowIndex) + i - 1 : 0 + i - 1;
                lp_utils.swapINT(ref intParam1, ref intParam2);
                ///<summary>
                /// PREVIOUS: lp_utils.swapREAL(rowValue + i, rowValue + i - 1);
                /// ERROR IN PREVIOUS: Operator '+' cannot be applied to operands of type 'double?[]' and 'int'
                /// FIX 1: double doubleParam1 = (rowValue != null) ? Convert.ToInt32(rowValue) + i : 0 + i;
                /// double doubleParam2 = (rowValue != null) ? Convert.ToInt32(rowValue) + i - 1 : 0 + i - 1;
                /// lp_utils.swapREAL(ref doubleParam1, ref doubleParam2);
                /// </summary>
                double doubleParam1 = (rowValue != null) ? Convert.ToInt32(rowValue) + i : 0 + i;
                double doubleParam2 = (rowValue != null) ? Convert.ToInt32(rowValue) + i - 1 : 0 + i - 1;
                lp_utils.swapREAL(ref doubleParam1, ref doubleParam2);
                i--;
            }

            /* Add same-indexed items (which is rarely encountered), and shorten the list */
            if ((i < count) && (rowIndex[i] == rowIndex[i + 1]))
            {
                int ii = i + 1;
                rowValue[i] += rowValue[ii];
                count--;
                while (ii < count)
                {
                    rowIndex[ii] = rowIndex[ii + 1];
                    rowValue[ii] = rowValue[ii + 1];
                    ii++;
                }
            }

            /* Update the count and return */
            count++;
            return true;
        }


        /// <summary>
        /// changed from 'ref string buf' to 'string buf' on 15/11/18 
        /// </summary>
        private static int MPS_input(object fpin, string buf, int max_size)
        {
            //ORIGINAL LINE: return (fgets(buf, max_size, (FILE*)fpin) != NULL);
            int count = 0;
            using (StreamReader reader = File.OpenText("foo.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    count++;
                }
            }
            return count;
        }

        /* Write a MPS file to output */
        public byte MPS_writefile(lprec lp, int typeMPS, ref string filename)
        {
            throw new NotImplementedException();
        }
        public byte MPS_writehandle(lprec lp, int typeMPS, FILE output)
        {
            throw new NotImplementedException();
        }

        /* Read and write BAS files */
        public byte MPS_readBAS(lprec lp, int typeMPS, ref string filename, ref string info)
        {
            throw new NotImplementedException();
        }
        public byte MPS_writeBAS(lprec lp, int typeMPS, ref string filename)
        {
            throw new NotImplementedException();
        }

        /// <summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
        /// changed from 'ref string field1' to 'ref char[] field1' on 15/11/18
        /// </summary>
        private int scan_lineFIXED(lprec lp, int section, ref string line, ref char[] field1, ref string field2, ref string field3, ref double field4, ref string field5, ref double field6)
        {
            throw new NotImplementedException();
        }

        /// <summary> FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
        /// changed from 'ref string field1' to 'ref char[] field1' on 15/11/18
        /// </summary>
        private int scan_lineFREE(lprec lp, int section, ref string line, ref char[] field1, ref string field2, ref string field3, ref double field4, ref string field5, ref double field6)
        {
            throw new NotImplementedException();
        }
        private bool addmpscolumn(lprec lp, bool Int_section, int typeMPS, ref bool Column_ready, ref int count, double?[] Last_column, ref int?[] Last_columnno, ref string Last_col_name)
        {
            throw new NotImplementedException();
        }


    }
}
