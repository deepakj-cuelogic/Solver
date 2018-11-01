using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZS.Math.Optimization
{

    public class _tmp_store_struct
    {
        public string name;
        public int row;
        public double value;
        public double rhs_value;
        public short relat;
    }

    public class parse_parm
    {
        public object scanner;
        public int lineno;
        public int Verbose;
        /// <summary>
        /// Not able to understand C code
        /// </summary>
        //public jmp_buf jump_buf = new jmp_buf();
        public int Rows;
        public int Columns;
        public int Non_zeros;
        public int Lin_term_count;
        public rside First_rside;
        public rside rs;
        public short SOStype; // SOS type
        public char Ignore_int_decl;
        public char int_decl;
        public char Ignore_sec_decl;
        public char Ignore_free_decl;
        public char sos_decl;
        public char Maximise;
        public hashtable Hash_tab;
        public hashtable Hash_constraints;
        public structcoldata coldata;
        public structSOS FirstSOS;
        public structSOS LastSOS;
        public _tmp_store_struct tmp_store = new _tmp_store_struct();
        public string title;
        //ORIGINAL LINE: short *relat;
        public short relat;
        public object parse_vars;
    }

    public class hashelem
    {
        public string name;
        public int index;
        public hashelem[] next;
        public hashelem nextelem;
    }

    public class hashtable // _hashtable
    {
        public hashelem[] table;
        public int size;
        public int @base;
        public int count;
        public hashelem first;
        public hashelem last;
    }

    public class structSOSvars
    {
        public string name;
        public int col;
        public double weight;
        public structSOSvars next;
    }

    public class structSOS
    {
        public string name;
        public short type;
        public int Nvars;
        public int weight;
        public structSOSvars SOSvars;
        public structSOSvars LastSOSvars;
        public structSOS next;
    }

    public class SOSrow
    {
        public int col;
        public double value;
        public SOSrow next;
    }

    public class SOSrowdata
    {
        public short type;
        public string name;
        public SOSrow SOSrow;
    }

    public class rside // contains relational operator and rhs value
    {
        public int row;
        public double value;
        public double range_value;
        public rside next;
        public short relat;
        public short range_relat;
        public char negate;
        public short SOStype;
    }

    public class column
    {
        public int row;
        public double value;
        public column next;
        public column prev;
    }

    public class structcoldata
    {
        public int must_be_int;
        public int must_be_sec;
        public int must_be_free;
        public double upbo;
        public double lowbo;
        public column firstcol;
        public column col;
    }


    public class yacc_read
    {

        public void lex_fatal_error(parse_parm UnnamedParameter, object UnnamedParameter2, ref string UnnamedParameter3)
        {
            throw new NotImplementedException();
        }
        public int set_title(parse_parm pp, ref string name)
        {
            throw new NotImplementedException();
        }
        public int add_constraint_name(parse_parm pp, ref string name)
        {
            throw new NotImplementedException();
        }
        public int store_re_op(parse_parm pp, char OP, int HadConstraint, int HadVar, int Had_lineair_sum)
        {
            throw new NotImplementedException();
        }
        public void null_tmp_store(parse_parm pp, int init_Lin_term_count)
        {
            throw new NotImplementedException();
        }
        public int store_bounds(parse_parm pp, int warn)
        {
            throw new NotImplementedException();
        }
        public void storevarandweight(parse_parm pp, ref string name)
        {
            throw new NotImplementedException();
        }
        public int set_sos_type(parse_parm pp, int SOStype)
        {
            throw new NotImplementedException();
        }
        public int set_sos_weight(parse_parm pp, double weight, int sos_decl)
        {
            throw new NotImplementedException();
        }
        public int set_sec_threshold(parse_parm pp, ref string name,  double threshold)
        {
            throw new NotImplementedException();
        }
        public int rhs_store(parse_parm pp, double value, int HadConstraint, int HadVar, int Had_lineair_sum)
        {
            throw new NotImplementedException();
        }
        public int var_store(parse_parm pp, ref string @var, double value, int HadConstraint, int HadVar, int Had_lineair_sum)
        {
            throw new NotImplementedException();
        }
        public int negate_constraint(parse_parm pp)
        {
            throw new NotImplementedException();
        }
        public void add_row(parse_parm pp)
        {
        }
        public void add_sos_row(parse_parm pp, short SOStype)
        {
        }

        public void read_error(parse_parm UnnamedParameter, object UnnamedParameter2, ref string UnnamedParameter3)
        {
        }
        public void check_int_sec_sos_free_decl(parse_parm UnnamedParameter, int UnnamedParameter2, int UnnamedParameter3, int UnnamedParameter4, int UnnamedParameter5)
        {
        }
        public delegate int parseDelegate(parse_parm pp);

        public delegate void delete_allocated_memoryDelegate(parse_parm pp);

        public lprec yacc_read_x(lprec lp, int verbose, ref string lp_name, parseDelegate parse, parse_parm pp, delete_allocated_memoryDelegate delete_allocated_memory)
        {
            throw new NotImplementedException();
        }
    }

}
