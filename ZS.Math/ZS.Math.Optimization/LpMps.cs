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
    public delegate string MPSnameDelegate(ref string name0, ref string name);

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

        internal bool MPS_readhandle(lprec[] newlp, FileStream filehandle, int typeMPS, int verbose)
        {
            return (MPS_readex(newlp, (object)filehandle, MPS_input, typeMPS, verbose));
        }

        internal bool MPS_readex(lprec[] newlp, object userhandle, lp_lib.read_modeldata_func read_modeldata, int typeMPS, int verbose)
        {

            char[] field1 = null;    //can use string instead?? FIX_284fccea-201d-4c7c-b59c-c873fa7b45aa
            string field2 = new string(new char[lp_lib.BUFSIZ]);
            string field3 = new string(new char[lp_lib.BUFSIZ]);
            string field5 = new string(new char[lp_lib.BUFSIZ]);
            string line = new string(new char[lp_lib.BUFSIZ]);
            string tmp = new string(new char[lp_lib.BUFSIZ]);
            string Last_col_name = new string(new char[lp_lib.BUFSIZ]);
            //string probname = new string(new char[lp_lib.BUFSIZ]);
            string probname;
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
                            probname = "0";
                            //sscanf(line, "NAME %s", probname);
                            if (!objLpCls.set_lp_name(lp, ref probname))
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
                                    ///<summary>
                                    /// ERROR: cannot convert from 'bool' to 'uint'
                                    /// due to: FIX_ca4c2404-e9f4-407d-b791-79776cb8de1f 19/11/18
                                    /// </summary>
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
                                    ///<summary>
                                    /// ERROR: cannot convert from 'bool' to 'uint'
                                    /// due to: FIX_ca4c2404-e9f4-407d-b791-79776cb8de1f 19/11/18
                                    /// </summary>
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
                                    objLpCls.set_binary(lp, @var, true);
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
                                            field3 = string.Format("SOS_{0:D}", LpCls.SOS_count(lp) + 1);
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
                        if (LpCls.is_int(lp, @var))
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
        public bool MPS_writefile(lprec lp, int typeMPS, ref string filename)
        {
            /// <summary>
            /// PREVIOUS: FILE output = stdout;
            /// ERROR IN PREVIOUS: The name 'stdout' does not exist in the current context
            /// FIX 1: FileStream output = null
            /// </summary>
            FileStream output = null;
            bool ok = false;

            if (filename != null)
            {
                File.OpenWrite(filename);
                if (output != null)
                    return ok;
            }
            else
                output = lp.outstream;

            /// <summary> FIX_3aef9b13-7390-44b4-a845-3ab28200caec 22/11/18
            /// PREVIOUS: ok = MPS_writefileex(lp, typeMPS, (object)output, write_lpdata);
            /// ERROR IN PREVIOUS: cannot convert from 'method group' to 'lp_lib.write_modeldata_func'
            /// FIX 1: definition for 'write_lpdata' is not clear, hence sent null for now
            /// </summary>
            ok = MPS_writefileex(lp, typeMPS, (object)output, null);

            if (filename != null)
            {
                output.Close();
            }

            return (ok);

        }

        private static int write_lpdata(object userhandle, ref string buf)
        {
            /// <summary> TODO
            /// PREVIOUS: fputs(buf, (FILE)userhandle);
            /// ERROR IN PREVIOUS: The name 'fputs' does not exist in the current context
            /// FIX 1: 
            /// </summary>
                return 1;
        }
        private static string MPSnameFIXED(ref string name0, ref string name)
        {
            //C++ TO C# CONVERTER TODO TASK: C# does not allow setting maximum string width in format specifiers:
            //ORIGINAL LINE: sprintf(name0, "%-8.8s", name);
            name0 = string.Format("{0,-8}", name);
            return (name0);
        }

        private static string MPSnameFREE(ref string name0, ref string name)
        {
            if (name.Length < 8)
            {
                return (MPSnameFIXED(ref name0, ref name));
            }
            else
            {
                return (name);
            }
        }

        private static void write_data(object userhandle, lp_lib.write_modeldata_func write_modeldata, ref string format, params object[] LegacyParamArray)
        {
            string buff = "";
            //  va_list ap;

            int ParamCount = -1;
            //  va_start(ap, format);
            /* TODO
            vsnprintf(buff, DEF_STRBUFSIZE, format, ap);
            */
            //  va_end(ap);
            write_modeldata(userhandle, buff);
        }        

        private bool MPS_writefileex(lprec lp, int typeMPS, object userhandle, lp_lib.write_modeldata_func write_modeldata)
        {
            int i;
            int j;
            int jj;
            int je;
            int k;
            int marker;
            //NOTED ISSUE: declared as int, TRUE value assignd later
            bool putheader;
            // PREVIOUS CODE: int ChangeSignObj = FALSE
            // CHANGED TO: bool ChangeSignObj = false
            bool ChangeSignObj = false;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *idx;
            int?[] idx=null;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *idx1;
            int idx1;
            bool? ok = true;
            bool? names_used = false;
            double a;
            double[] val=null;
            double val1;
            MPSnameDelegate MPSname;
            string numberbuffer = new string(new char[15]);
            string name0 = new string(new char[9]);
            lp_report objlp_report = new lp_report();
            LpCls objLpCls = new LpCls();
            string msg = "";

            if ((typeMPS & lp_lib.MPSFIXED) == lp_lib.MPSFIXED)
            {
                MPSname = MPSnameFIXED;
                ChangeSignObj = LpCls.is_maxim(lp);
            }
            else if ((typeMPS & lp_lib.MPSFREE) == lp_lib.MPSFREE)
            {
                MPSname = MPSnameFREE;
            }
            else
            {
                msg = "MPS_writefile: unrecognized MPS name type.\n";
                objlp_report.report(lp, lp_lib.IMPORTANT, ref msg);
                return false;
            }

            names_used = lp.names_used;

            if ((typeMPS & lp_lib.MPSFIXED) == lp_lib.MPSFIXED)
            {
                /* Check if there is no variable name where the first 8 charachters are equal to the first 8 characters of another variable */
                if (names_used != null)
                {
                    for (i = 1; (i <= lp.columns) && (ok) != null; i++)
                    {
                        if ((lp.col_name[i] != null) && (lp.col_name[i].name != null) && (!objLpCls.is_splitvar(lp, i)) && (Convert.ToString(lp.col_name[i].name).Length > 8))
                        {
                            for (j = 1; (j < i) && (ok) != null; j++)
                            {
                                if ((lp.col_name[j] != null) && (lp.col_name[j].name != null) && (!objLpCls.is_splitvar(lp, j)))
                                {
                                    if (string.Compare(lp.col_name[i].name, 0, lp.col_name[j].name, 0, 8) == 0)
                                    {
                                        ok = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (ok == null)
            {
                lp.names_used = false;
                ok = true;
            }

            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            /* NOT REQUIRED 
            memset(numberbuffer, 0, sizeof(char));
            */

            marker = 0;

            /* First write metadata in structured comment form (lp_solve style) */
            msg = "*<meta creator='lp_solve v{0}.{1}'>\n";
            write_data(userhandle, write_modeldata, ref msg, (int)lp_lib.MAJORVERSION, (int)lp_lib.MINORVERSION);
            msg = "*<meta rows={0}>\n";
            write_data(userhandle, write_modeldata, ref msg, lp.rows);
            msg = "*<meta columns={0}>\n";
            write_data(userhandle, write_modeldata, ref msg, lp.columns);
            msg = "*<meta equalities={0}>\n";
            write_data(userhandle, write_modeldata, ref msg, lp.equalities);
            if (LpCls.SOS_count(lp) > 0)
            {
                msg = "*<meta SOS={0}>\n";
                write_data(userhandle, write_modeldata, ref msg, LpCls.SOS_count(lp));
            }
            msg = "*<meta integers={0}>\n";
            write_data(userhandle, write_modeldata, ref msg, lp.int_vars);
            if (lp.sc_vars > 0)
            {
                msg = "*<meta scvars={0}>\n";
                write_data(userhandle, write_modeldata, ref msg, lp.sc_vars);
            }
            msg = "*<meta origsense='%s'>\n";
            write_data(userhandle, write_modeldata, ref msg, (LpCls.is_maxim(lp) ? "MAX" : "MIN"));
            msg = "*\n";
            write_data(userhandle, write_modeldata, ref msg);

            /* Write the MPS content */
            msg = "NAME          {0}\n";
            msg = objLpCls.get_lp_name(lp);
            write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref msg));
            if (((typeMPS & lp_lib.MPSFREE) == lp_lib.MPSFREE) && (LpCls.is_maxim(lp)))
            {
                msg = "OBJSENSE\n MAX\n";
                write_data(userhandle, write_modeldata, ref msg);
            }
            msg = "ROWS\n";
            write_data(userhandle, write_modeldata, ref msg);
            for (i = 0; i <= lp.rows; i++)
            {
                if (i == 0)
                {
                    msg = " N  ";
                    write_data(userhandle, write_modeldata, ref msg);
                }
                else if (lp.orig_upbo[i] != 0)
                {
                    if (objLpCls.is_chsign(lp, i))
                    {
                        msg = " G  ";
                        write_data(userhandle, write_modeldata, ref msg);
                    }
                    else
                    {
                        msg = " L  ";
                        write_data(userhandle, write_modeldata, ref msg);
                    }
                }
                else
                {
                    msg = " E  ";
                    write_data(userhandle, write_modeldata, ref msg);
                }
                string getrowname = objLpCls.get_row_name(lp, i);
                msg = "{0}\n";
                write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getrowname));
            }
            /*NOT REQUIRED
            allocREAL(lp, val, 1 + lp.rows, 1);
            allocINT(lp, idx, 1 + lp.rows, 1);
            */
            msg = "COLUMNS\n";
            write_data(userhandle, write_modeldata, ref msg);
            for (i = 1; i <= lp.columns; i++)
            {
                if (!objLpCls.is_splitvar(lp, i))
                {
                    if (LpCls.is_int(lp, i) && (marker % 2) == 0)
                    {
                        msg = "    MARK{0}  'MARKER'                 'INTORG'\n";
                        write_data(userhandle, write_modeldata, ref msg, marker);
                        marker++;
                    }
                    if (!LpCls.is_int(lp, i) && (marker % 2) == 1)
                    {
                        msg = "    MARK{0}  'MARKER'                 'INTEND'\n";
                        write_data(userhandle, write_modeldata, ref msg, marker);
                        marker++;
                    }

                    // Loop over non-zero column entries
                    je = objLpCls.get_columnex(lp, i, ref val, ref idx);
                    for (k = 1, val1 = val[0], idx1 = Convert.ToInt32(idx[0]), jj = 0; jj < je; jj++)
                    {
                        k = 1 - k;
                        j = idx1++;
                        a = val1++;
                        if (k == 0)
                        {
                            string getcolname = objLpCls.get_col_name(lp, i);
                            msg = "    {0}";
                            write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getcolname));
                            msg = "  {0}  {1}";
                            string getrowname = objLpCls.get_row_name(lp, j);
                            write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getrowname), formatnumber12(ref numberbuffer, (double)(a * (j == 0 && ChangeSignObj ? -1 : 1))));
                        }
                        else
                        {
                            string getrowname = objLpCls.get_row_name(lp, j);
                            msg = "   {0}  {1}\n";
                            write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getrowname), formatnumber12(ref numberbuffer, (double)(a * (j == 0 && ChangeSignObj ? -1 : 1))));
                        }
                        /*                          formatnumber12(numberbuffer, (double) a)); */
                    }
                    if (k == 0)
                    {
                        msg = "\n";
                        write_data(userhandle, write_modeldata, ref msg);
                    }
                }
            }
            //C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
            if ((marker % 2) == 1)
            {
                msg = "    MARK%04d  'MARKER'                 'INTEND'\n";
                write_data(userhandle, write_modeldata, ref msg, marker);
                /* marker++; */
                /* marker not used after this */
            }
            /*NOT REQUIRED
            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            FREE(idx);
            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            FREE(val);
            */
            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            msg = "RHS\n";
            write_data(userhandle, write_modeldata, ref msg);
            //C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
            for (k = 1, i = 0; i <= lp.rows; i++)
            {
                a = lp.orig_rhs[i];
                if (a > 0)
                {
                    a = lp_scale.unscaled_value(lp, a, i);
                    if ((i == 0) && ((typeMPS & lp_lib.MPSNEGOBJCONST) == lp_lib.MPSNEGOBJCONST))
                    {
                        a = -a;
                    }
                    if ((i == 0) || objLpCls.is_chsign(lp, i))
                    {
                        a = lp_types.my_flipsign(a);
                    }
                    k = 1 - k;
                    if (k == 0)
                    {
                        string getrowname = objLpCls.get_row_name(lp, i);
                        msg = "    RHS       {0}  {1}";
                        write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getrowname), formatnumber12(ref numberbuffer, (double)a));
                    }
                    else
                    {
                        string getrowname = objLpCls.get_row_name(lp, i);
                        msg = "   {0}  {1}\n";
                        write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getrowname), formatnumber12(ref numberbuffer, (double)a));
                    }
                }
            }
            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            if (k == 0)
                //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
                msg = "\n";
                write_data(userhandle, write_modeldata, ref msg);

            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            putheader = true;
            //C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
            for (k = 1, i = 1; i <= lp.rows; i++)
            {
                a = 0;
                if ((lp.orig_upbo[i] < lp.infinite) && (lp.orig_upbo[i] != 0.0))
                {
                    a = lp.orig_upbo[i];
                }
                if (a > 0)
                {
                    if (putheader)
                    {
                        msg = "RANGES\n";
                        write_data(userhandle, write_modeldata, ref msg);
                        putheader = false;
                    }
                    a = lp_scale.unscaled_value(lp, a, i);
                    k = 1 - k;
                    if (k == 0)
                    {
                        string getrowname = objLpCls.get_row_name(lp, i);
                        msg = "    RGS       {0}  {1}";
                        write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getrowname), formatnumber12(ref numberbuffer, (double)a));
                    }
                    else
                    {
                        string getrowname = objLpCls.get_row_name(lp, i);
                        msg = "   {0}  {1}\n";
                        write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getrowname), formatnumber12(ref numberbuffer, (double)a));
                    }
                }
            }
            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            if (k == 0)
                //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
                msg = "\n";
                write_data(userhandle, write_modeldata, ref msg);

            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            putheader = true;
            //C++ TO C# CONVERTER TODO TASK: The following statement was not recognized, possibly due to an unrecognized macro:
            for (i = lp.rows + 1; i <= lp.sum; i++)
                //C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
                if (!objLpCls.is_splitvar(lp, i - lp.rows))
                {
                    j = i - lp.rows;
                    if ((lp.orig_lowbo[i] != 0) && (lp.orig_upbo[i] < lp.infinite) && (lp.orig_lowbo[i] == lp.orig_upbo[i]))
                    {
                        a = lp.orig_upbo[i];
                        a = lp_scale.unscaled_value(lp, a, i);
                        if (putheader)
                        {
                            msg = "BOUNDS\n";
                            write_data(userhandle, write_modeldata, ref msg);
                            putheader = false;
                        }
                        string getcolname = objLpCls.get_col_name(lp, i);
                        msg = " FX BND       {0}  {1}\n";
                        write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getcolname), formatnumber12(ref numberbuffer, (double)a));
                    }
                    else if (objLpCls.is_binary(lp, j))
                    {
                        if (putheader)
                        {
                            msg = "BOUNDS\n";
                            write_data(userhandle, write_modeldata, ref msg);
                            putheader = false;
                        }
                        string getcolname = objLpCls.get_col_name(lp, i);
                        msg = " BV BND       {0}\n";
                        write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getcolname));
                    }
                    //C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
                    else if (objLpCls.is_unbounded(lp, j))
                    {
                        if (putheader)
                        {
                            msg = "BOUNDS\n";
                            write_data(userhandle, write_modeldata, ref msg);
                            putheader = false;
                        }
                        string getcolname = objLpCls.get_col_name(lp, i);
                        msg = " FR BND       {0}\n";
                        write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getcolname));
                    }
                    else
                    {
                        if ((lp.orig_lowbo[i] != 0) || (LpCls.is_int(lp, j)))
                        { // Some solvers like CPLEX need to have a bound on a variable if it is integer, but not binary else it is interpreted as binary which is not ment
                            a = lp.orig_lowbo[i];
                            a = lp_scale.unscaled_value(lp, a, i);
                            if (putheader)
                            {
                                msg = "BOUNDS\n";
                                write_data(userhandle, write_modeldata, ref msg);
                                putheader = false;
                            }
                            if (lp.orig_lowbo[i] != -lp.infinite)
                            {
                                string getcolname = objLpCls.get_col_name(lp, i);
                                msg = " LO BND       {0}  {1}\n";
                                write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getcolname), formatnumber12(ref numberbuffer, (double)a));
                            }
                            else
                            {
                                string getcolname = objLpCls.get_col_name(lp, i);
                                msg = " MI BND       {0}\n";
                                write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getcolname));
                            }
                        }

                        if ((lp.orig_upbo[i] < lp.infinite) || (LpCls.is_semicont(lp, j)))
                        {
                            a = lp.orig_upbo[i];
                            if (a < lp.infinite)
                            {
                                a = lp_scale.unscaled_value(lp, a, i);
                            }
                            if (putheader)
                            {
                                msg = "BOUNDS\n";
                                write_data(userhandle, write_modeldata, ref msg);
                                putheader = false;
                            }
                            if (LpCls.is_semicont(lp, j))
                            {
                                if (LpCls.is_int(lp, j))
                                {
                                    string getcolname = objLpCls.get_col_name(lp, i);
                                    msg = " SI BND       {0}  {1}\n";
                                    write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getcolname), (a < lp.infinite) ? formatnumber12(ref numberbuffer, (double)a) : "            ");
                                }
                                else
                                {
                                    string getcolname = objLpCls.get_col_name(lp, i);
                                    msg = " SC BND       {0}  {1}\n";
                                    write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getcolname), (a < lp.infinite) ? formatnumber12(ref numberbuffer, (double)a) : "            ");
                                }
                            }
                            else
                            {
                                string getcolname = objLpCls.get_col_name(lp, i);
                                msg = " UP BND       {0}  {1}\n";
                                write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getcolname), formatnumber12(ref numberbuffer, (double)a));
                            }
                        }
                    }
                }

            /* Write optional SOS section */
            putheader = true;
            for (i = 0; i < LpCls.SOS_count(lp); i++)
            {
                SOSgroup SOS = lp.SOS;

                if (putheader)
                {
                    msg = "SOS\n";
                    write_data(userhandle, write_modeldata, ref msg);
                    putheader = false;
                }
                msg = " S%1d SOS       {0}  {1}\n";
                write_data(userhandle, write_modeldata, ref msg, SOS.sos_list[i].type, MPSname(ref name0, ref SOS.sos_list[i].name), formatnumber12(ref numberbuffer, (double)SOS.sos_list[i].priority));
                for (j = 1; j <= SOS.sos_list[i].size; j++)
                {
                    string getrowname = objLpCls.get_col_name(lp, SOS.sos_list[i].members[j]);
                    msg = "    SOS       {0}  {1}\n";
                    write_data(userhandle, write_modeldata, ref msg, MPSname(ref name0, ref getrowname), formatnumber12(ref numberbuffer, (double)SOS.sos_list[i].weights[j]));
                }
            }
            msg = "ENDATA\n";
            write_data(userhandle, write_modeldata, ref msg);

            lp.names_used = (bool)names_used;

            return (bool)ok;
        }

        private static string formatnumber12(ref string numberbuffer, double a)
        {
            #if false
            //  return(sprintf(numberbuffer, "%12g", a));
            #else
            number(ref numberbuffer, a);
            return (numberbuffer);
            #endif
        }

        private static void number(ref string str, double value)
        {
            string __str = new string(new char[80]);
            string _str;
            int i;

            /* sprintf(_str,"%12.6G",value); */
            _str = __str.Substring(2);
            if (value >= 0.0)
            {
                if ((value != 0.0) && ((value > 0.99999999e12) || (value < 0.0001)))
                {
                    int n = 15;

                    do
                    {
                        n--;
                        //C++ TO C# CONVERTER TODO TASK: The following line has a C format specifier which cannot be directly translated to C#:
                        //ORIGINAL LINE: i=sprintf(_str,"%*.*E",n,n-6,(double) value);
                        _str = string.Format("%*.*E", n, n - 6, (double)value);
                        i = _str.Length;
                        if (i > 12)
                        {
                            /*NOTED ISSUE: START
                            //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
                            string ptr = StringFunctions.StrChr(_str, 'E');

                            if (ptr != null)
                            {
                                if (*(++ptr) == '-')
                                {
                                    ptr++;
                                }
                                while ((i > 12) && ((*ptr == '+') || (*ptr == '0')))
                                {
                                    ptr = ptr + 1;
                                    i--;
                                }
                            }
                            //NOTED ISSUE: END*/
                        }
                    } while (i > 12);
                }
                else if (value >= 1.0e10)
                {
                    int n = 13;

                    do
                    {
                        //C++ TO C# CONVERTER TODO TASK: The following line has a C format specifier which cannot be directly translated to C#:
                        //ORIGINAL LINE: i=sprintf(_str,"%*.0f",--n,(double) value);
                        _str = string.Format("%*.0f", --n, (double)value);
                        i = _str.Length;
                    } while (i > 12);
                }
                else
                {

                    /*NOTED ISSUE: START
                     * NEED TO Add C# equivalent for sprintf which will format the input in given format(%12.10f) and return the length
                    if (((i = sprintf(_str, "%12.10f", (double)value)) > 12) && (_str[12] >= '5'))
                    {
                        for (i = 11; i >= 0; i--)
                        {
                            if (_str[i] != '.')
                            {
                                if (++_str[i] > '9')
                                {
                                    _str[i] = '0';
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        if (i < 0)
                        {
                            *(--_str) = '1';
                            *(--_str) = ' ';
                        }
                    }
                    NOTED ISSUE: END*/
                }
            }
            else
            {
                if ((value < -0.99999999e11) || (value > -0.0001))
                {
                    int n = 15;

                    do
                    {
                        n--;
                        //C++ TO C# CONVERTER TODO TASK: The following line has a C format specifier which cannot be directly translated to C#:
                        //ORIGINAL LINE: i=sprintf(_str,"%*.*E",n,n-7,(double) value);
                        _str = string.Format("%*.*E", n, n - 7, (double)value);
                        i = _str.Length;
                        if (i > 12)
                        {
                            /*NOTED ISSUE: START
                            //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
                            char* ptr = StringFunctions.StrChr(_str, 'E');

                            if (ptr != null)
                            {
                                if (*(++ptr) == '-')
                                {
                                    ptr++;
                                }
                                while ((i > 12) && ((*ptr == '+') || (*ptr == '0')))
                                {
                                    ptr = ptr + 1;
                                    i--;
                                }
                            }
                            NOTED ISSUE: END*/
                        }
                    } while (i > 12);
                }
                else if (value <= -1.0e9)
                {
                    int n = 13;

                    do
                    {
                        //C++ TO C# CONVERTER TODO TASK: The following line has a C format specifier which cannot be directly translated to C#:
                        //ORIGINAL LINE: i=sprintf(_str,"%*.0f",--n,(double) value);
                        _str = string.Format("%*.0f", --n, (double)value);
                        i = _str.Length;
                    } while (i > 12);
                }
                else
                {
                    /*NOTED ISSUE: START
                    if (((i = sprintf(_str, "%12.9f", (double)value)) > 12) && (_str[12] >= '5'))
                    {
                        for (i = 11; i >= 1; i--)
                        {
                            if (_str[i] != '.')
                            {
                                if (++_str[i] > '9')
                                {
                                    _str[i] = '0';
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        if (i < 1)
                        {
                            _str = '1';
                            *(--_str) = '-';
                            *(--_str) = ' ';
                        }
                    }
                    NOTED ISSUE: END */
                }
            }
            str = _str.Substring(0, 12);
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


        internal bool MPS_writehandle(lprec lp, int typeMPS, FileStream output)
        {
            bool ok;
            LpCls objLpCls = new LpCls();

            if (output != null)
            {
                objLpCls.set_outputstream(lp, output);
            }

            output = lp.outstream;
            //null: FIX_3aef9b13-7390-44b4-a845-3ab28200caec 22/11/18
            ok = MPS_writefileex(lp, typeMPS, (object)output, null);

            return (ok);

        }

        /* Read and write BAS files */
        public byte MPS_readBAS(lprec lp, int typeMPS, ref string filename, ref string info)
        {
            throw new NotImplementedException();
        }
        public bool MPS_writeBAS(lprec lp, int typeMPS, ref string filename)
        {
            int ib = 0;
            int in1 = 0;
            bool ok;
            string name1 = "";
            string name2 = "";
            FileStream output = null;
            MPSnameDelegate MPSname;
            string name0 = "";
            lp_report objlp_report = new lp_report();
            LpCls objLpCls = new LpCls();
            StreamWriter writer = new StreamWriter(output);
            string msg = "";
            string getrowcolname = "";

            /* Set name formatter */
            if ((typeMPS & lp_lib.MPSFIXED) == lp_lib.MPSFIXED)
            {
                MPSname = MPSnameFIXED;
            }
            else if ((typeMPS & lp_lib.MPSFREE) == lp_lib.MPSFREE)
            {
                MPSname = MPSnameFREE;
            }
            else
            {
                msg = "MPS_writeBAS: unrecognized MPS name type.\n";
                objlp_report.report(lp, lp_lib.IMPORTANT, ref msg);
                return false;
            }

            /* Open the FileStream for writing */
            ok = (bool)((filename == null) || ((output = File.OpenWrite(filename)) != null));
            if (!ok)
            {
                return ok;
            }
            if (filename == null && lp.outstream != null)
            {
                output = lp.outstream;
            }

            //ORIGINAL CODE: fprintf(output, , get_lp_name(lp), lp.rows, lp.columns, (double)get_total_iter(lp));
            writer.Flush();
            writer.Write("NAME          {0} Rows {1} Cols {2} Iters %.0f\n", objLpCls.get_lp_name(lp), lp.rows, lp.columns, (double)LpCls.get_total_iter(lp));

            ib = lp.rows;
            in1 = 0;
            while ((ib < lp.sum) || (in1 < lp.sum))
            {

                /* Find next basic variable (skip slacks) */
                ib++;
                while ((ib <= lp.sum) && !lp.is_basic[ib])
                {
                    ib++;
                }

                /* Find next non-basic variable (skip lower-bounded structural variables) */
                in1++;
                while ((in1 <= lp.sum) && (lp.is_basic[in1] || ((in1 > lp.rows) && lp.is_lower[in1])))
                {
                    in1++;
                }

                /* Check if we have a basic/non-basic variable pair */
                if ((ib <= lp.sum) && (in1 <= lp.sum))
                {
                    getrowcolname = (ib <= lp.rows ? objLpCls.get_row_name(lp, ib) : objLpCls.get_col_name(lp, ib - lp.rows));
                    name1 = MPSname(ref name0, ref getrowcolname);
                    getrowcolname = (in1 <= lp.rows ? objLpCls.get_row_name(lp, in1) : objLpCls.get_col_name(lp, in1 - lp.rows));
                        name2 = MPSname(ref name0, ref getrowcolname);
                    //ORIGINAL CODE: fprintf(output, " %2s %s  %s\n", (lp.is_lower[in1] ? "XL" : "XU"), name1, name2);
                    writer.Flush();
                    writer.Write(" {0} {1}  {2}\n", (lp.is_lower[in1] ? "XL" : "XU"), name1, name2);

                }

                /* Otherwise just write the bound state of the non-basic variable */
                else if (in1 <= lp.sum)
                {
                    getrowcolname = (in1 <= lp.rows ? objLpCls.get_row_name(lp, in1) : objLpCls.get_col_name(lp, in1 - lp.rows));
                                name1 = MPSname(ref name0, ref getrowcolname);
                    //ORIGINAL CODE: fprintf(output, " %2s %s\n", (lp.is_lower[in1] ? "LL" : "UL"), name1);
                    writer.Flush();
                    writer.Write(" {0} {1}\n", (lp.is_lower[in1] ? "LL" : "UL"), name1);
                }

            }
            //ORIGINAL CODE: fprintf(output, "ENDATA\n");
            writer.Flush();
            writer.Write("ENDATA\n");
            
            if (filename != null)
            {
                output.Close();
            }
            return (ok);
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
