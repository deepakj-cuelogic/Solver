using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ZS.Math.Optimization
{
    public static class lp_SOS
    {
        public const int SOS1 = 1;
        public const int SOS2 = 2;
        public const int SOS3 = -1;
        public const int SOSn = 35567;
        public const int SOS_START_SIZE = 10; // Start size of SOS_list array; realloced if needed
        public const int SOS3_INCOMPLETE = -2;
        public const int SOS_INCOMPLETE = -1;
        public const int SOS_COMPLETE = 0;
        public const int SOS_INFEASIBLE = 1;
        public const int SOS_INTERNALERROR = 2;

        /* SOS storage structure */
        internal static SOSgroup create_SOSgroup(lprec lp)
        {
            SOSgroup group = null;
            //C++ TO C# CONVERTER TODO TASK: The memory management function 'calloc' has no equivalent in C#:
            /*NOT REQUIRED
            group = (SOSgroup)calloc(1, sizeof(SOSgroup));*/
            group.lp = lp;
            group.sos_alloc = SOS_START_SIZE;
            //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            /*NOT REQUIRED
            group.sos_list = (SOSrec)malloc((group.sos_alloc) * sizeof(*group.sos_list));*/
            return (group);
        }
        internal static void resize_SOSgroup(SOSgroup group)
        {
        throw new NotImplementedException();}
        internal static int append_SOSgroup(SOSgroup group, SOSrec SOS)
        {
            int i;
            int k;
            SOSrec SOSHold;

            /* Check if we should resize */
            resize_SOSgroup(group);

            /* First append to the end of the list */
            group.sos_list[group.sos_count] = SOS;
            group.sos_count++;
            i = System.Math.Abs(SOS.type);
           commonlib.SETMAX(group.maxorder, i);
            if (i == 1)
            {
                group.sos1_count++;
            }
            k = group.sos_count;
            SOS.tagorder = k;

            /* Sort the SOS list by given priority */
            for (i = group.sos_count - 1; i > 0; i--)
            {
                if (group.sos_list[i].priority < group.sos_list[i - 1].priority)
                {
                    SOSHold = group.sos_list[i];
                    group.sos_list[i] = group.sos_list[i - 1];
                    group.sos_list[i - 1] = SOSHold;
                    if (SOSHold == SOS)
                    {
                        k = i; // This is the index in the [1..> range
                    }
                }
                else
                {
                    break;
                }
            }
            /* Return the list index of the new SOS */
            return (k);
        }
        internal static int clean_SOSgroup(SOSgroup group, byte forceupdatemap)
        {
        throw new NotImplementedException();}
        internal static void free_SOSgroup(SOSgroup[] group)
        {
        throw new NotImplementedException();}

        /// <summary>
        /// FIX_1e4d8424-dcfd-4e3a-9936-fda7b883b7d8 19/11/18
        /// changed from 'ref int variables' to 'ref int?[] variables'
        /// changed from 'ref double weights' to 'ref double? weights'
        /// </summary>
        internal static SOSrec create_SOSrec(SOSgroup group, ref string name, int type, int priority, int size, ref int?[] variables, ref double? weights)
        {
        throw new NotImplementedException();}
        internal static byte delete_SOSrec(SOSgroup group, int sosindex)
        {
        throw new NotImplementedException();}
        internal static int append_SOSrec(SOSrec SOS, int size, ref int variables, ref double weights)
        {
        throw new NotImplementedException();}
        internal static void free_SOSrec(SOSrec SOS)
        {
        throw new NotImplementedException();}

        /* SOS utilities */
        internal static int make_SOSchain(lprec lp, byte forceresort)
        {
        throw new NotImplementedException();}
        internal static int SOS_member_updatemap(SOSgroup group)
        {
        throw new NotImplementedException();}
        internal static byte SOS_member_sortlist(SOSgroup group, int sosindex)
        {
        throw new NotImplementedException();}
        internal static bool SOS_shift_col(SOSgroup group, int sosindex, int column, int delta, LLrec usedmap, bool forceresort)
        /* Routine to adjust SOS indeces for variable insertions or deletions;
Note: SOS_shift_col must be called before make_SOSchain! */

        {
            int i;
            int ii;
            int n;
            int nn;
            int nr;
            int changed;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *list;
            int[] list;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: double *weights;
            double[] weights;
#if Paranoia
  lprec lp = group.lp;
  if ((sosindex < 0) || (sosindex > group.sos_count))
  {
	report(lp, IMPORTANT, "SOS_shift_col: Invalid SOS index %d\n", sosindex);
	return (0);
  }
  else if ((column < 1) || (delta == 0))
  {
	report(lp, IMPORTANT, "SOS_shift_col: Invalid column %d specified with delta %d\n", column, delta);
	return (0);
  }
#endif
            if ((sosindex == 0) && (group.sos_count == 1))
            {
                sosindex = 1;
            }
            if (sosindex == 0)
            {
                for (i = 1; i <= group.sos_count; i++)
                {
                    if (!SOS_shift_col(group, i, column, delta, usedmap, forceresort))
                    {
                        return false;
                    }
                }
            }
            else
            {
                list = group.sos_list[sosindex - 1].members;
                weights = group.sos_list[sosindex - 1].weights;
                n = list[0];
                nn = list[n + 1];
                /* Case where variable indeces are to be incremented */
                if (delta > 0)
                {
                    for (i = 1; i <= n; i++)
                    {
                        if (list[i] >= column)
                        {
                            list[i] += delta;
                        }
                    }
                }
                /* Case where variables are to be deleted/indeces decremented */
                else
                {
                    changed = 0;
                    if (usedmap != null)
                    {
                        //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                        //changed from 'int[] newidx' to 'int[][] newidx'; need to check at run time
                        int[][] newidx = null;
                        /* Defer creation of index mapper until we are sure that a
                           member of this SOS is actually targeted for deletion */
                        if (newidx == null)
                        {
                            //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                            lp_utils.allocINT(group.lp, newidx, group.lp.columns + 1, 1);
                            for (i = lp_utils.firstActiveLink(usedmap), ii = 1; i != 0; i = lp_utils.nextActiveLink(usedmap, i), ii++)
                            {
                                //FIX_6ad741b5-fc42-4544-98cc-df9342f14f9c 27/11/18
                                //set second [] as 0 for now; need to check at run time
                                newidx[i][0] = ii;
                            }
                        }
                        for (i = 1, ii = 0; i <= n; i++)
                        {
                            nr = list[i];
                            /* Check if this SOS variable should be deleted */
                            if (!lp_utils.isActiveLink(usedmap, nr))
                            {
                                continue;
                            }
                            /* If the index is "high" then make adjustment and shift */
                            changed++;
                            ii++;
                            list[ii] = newidx[nr];
                            weights[ii] = weights[i];
                        }
                        /*NOT REQUIRED
                        FREE(newidx);
                        */
                    }
                    else
                    {
                        for (i = 1, ii = 0; i <= n; i++)
                        {
                            nr = list[i];
                            /* Check if this SOS variable should be deleted */
                            if ((nr >= column) && (nr < column - delta))
                            {
                                continue;
                            }
                            /* If the index is "high" then decrement */
                            if (nr > column)
                            {
                                changed++;
                                nr += delta;
                            }
                            ii++;
                            list[ii] = nr;
                            weights[ii] = weights[i];
                        }
                    }
                    /* Update the SOS length / type indicators */
                    if (ii < n)
                    {
                        list[0] = ii;
                        list[ii + 1] = nn;
                    }
                    /* Update mapping arrays to search large SOS's faster */
                    if (forceresort && ((ii < n) || (changed > 0)))
                    {
                        SOS_member_sortlist(group, sosindex);
                    }
                }
            }
            return true;
        }
        public static int SOS_member_delete(SOSgroup group, int sosindex, int member)
        {
        throw new NotImplementedException();}
        public static int SOS_get_type(SOSgroup group, int sosindex)
        {
        throw new NotImplementedException();}
        public static int SOS_infeasible(SOSgroup group, int sosindex)
        {
        throw new NotImplementedException();}
        public static int SOS_member_index(SOSgroup group, int sosindex, int member)
        {
        throw new NotImplementedException();}
        public static int SOS_member_count(SOSgroup group, int sosindex)
        {
        throw new NotImplementedException();}
        public static int SOS_memberships(SOSgroup group, int column)
        {
        throw new NotImplementedException();}
        //ORIGINAL LINE: int *SOS_get_candidates(SOSgroup *group, int sosindex, int column, byte excludetarget, double *upbound, double *lobound)
        public static int[] SOS_get_candidates(SOSgroup group, int sosindex, int column, bool excludetarget, ref double[] upbound, ref double[] lobound)
        {
            int i;
            int ii;
            int j;
            int n;
            int nn = 0;
            
            //ORIGINAL LINE: int *list;
            int[] list;
            
            //ORIGINAL LINE: int *candidates = null;
            int[] candidates = null;
            lprec lp = group.lp;

            if (group == null)
            {
                return (candidates);
            }

#if Paranoia
  if (sosindex > group.sos_count)
  {
	report(lp, IMPORTANT, "SOS_get_candidates: Invalid index %d\n", sosindex);
	return (candidates);
  }
#endif

            /* Determine SOS target(s); note that if "sosindex" is negative, only
               the first non-empty SOS where "column" is a member is processed */
            if (sosindex <= 0)
            {
                i = 0;
                ii = group.sos_count;
            }
            else
            {
                i = sosindex - 1;
                ii = sosindex;
            }

            /* Tally candidate usage */
            //allocINT(lp, candidates, lp.columns + 1, 1);
            for (; i < ii; i++)
            {
                if (!SOS_is_member(group, i + 1, column))
                {
                    continue;
                }
                list = group.sos_list[i].members;
                n = list[0];
                while (n > 0)
                {
                    j = list[n];
                    if ((j > 0) && (upbound[lp.rows + j] > 0))
                    {
                        if (lobound[lp.rows + j] > 0)
                        {
                            string msg = "SOS_get_candidates: Invalid non-zero lower bound setting\n";
                            lp.report(lp, lp_lib.IMPORTANT, ref msg);
                            n = 0;
                            goto Finish;
                        }
                        if (candidates[j] == 0)
                        {
                            nn++;
                        }
                        candidates[j]++;
                    }
                    n--;
                }
                if ((sosindex < 0) && (nn > 1))
                {
                    break;
                }
            }

            /* Condense the list into indeces */
            n = 0;
            for (i = 1; i <= lp.columns; i++)
            {
                if ((candidates[i] > 0) && (!excludetarget || (i != column)))
                {
                    n++;
                    candidates[n] = i;
                }
            }

        /* Finalize */
        Finish:
            candidates[0] = n;
            if (n == 0)
            {
                //NOT REQUIRED
                //FREE(candidates);
            }

            return (candidates);

        }
        public static bool SOS_is_member(SOSgroup group, int sosindex, int column)
        {
            int i;
            bool n = false;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *list;
            int[] list;
            lprec lp;

            if (group == null)
            {
                return false;
            }
            lp = group.lp;

#if Paranoia
  if ((sosindex < 0) || (sosindex > group.sos_count))
  {
	report(lp, IMPORTANT, "SOS_is_member: Invalid SOS index %d\n", sosindex);
	return (n);
  }
#endif

            if (sosindex == 0)
            {
                if (lp.var_type[column])    // why to compare constant values?? & (lp_lib.ISSOS | lp_lib.ISGUB)) != 0
                {
                    n = (bool)(SOS_memberships(group, column) > 0);
                }
            }
            else if (lp.var_type[column])   // & (ISSOS | ISGUB)
            {

                /* Search for the variable */
                i = SOS_member_index(group, sosindex, column);

                /* Signal active status if found, otherwise return FALSE */
                if (i > 0)
                {
                    list = group.sos_list[sosindex - 1].members;
                    if (list[i] < 0)
                    {
                        n = false;
                    }
                    else
                    {
                        n = true;
                    }
                }
            }
            return n;
        }
        public static byte SOS_is_member_of_type(SOSgroup group, int column, int sostype)
        {
        throw new NotImplementedException();}
        public static byte SOS_set_GUB(SOSgroup group, int sosindex, byte state)
        {
        throw new NotImplementedException();}
        public static byte SOS_is_GUB(SOSgroup group, int sosindex)
        {
        throw new NotImplementedException();}
        public static bool SOS_is_marked(SOSgroup group, int sosindex, int column)
        {
            int i;
            int k;
            int n;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *list;
            int list;
            lprec lp;

            if (group == null)
            {
                return false;
            }
            lp = group.lp;

#if Paranoia
  if ((sosindex < 0) || (sosindex > group.sos_count))
  {
	report(lp, IMPORTANT, "SOS_is_marked: Invalid SOS index %d\n", sosindex);
	return (0);
  }
#endif

            if (lp.var_type[column])  // & (lp_lib.ISSOS | lp_lib.ISGUB)) == 0)
            {
                return false;
            }

            if (sosindex == 0)
            {
                for (i = group.memberpos[column - 1]; i < group.memberpos[column]; i++)
                {
                    k = group.membership[i];
                    // TODO_12/10/2018
                    n = SOS_is_marked(group, k, column);
                    if (n != 0)
                    {
                        return (1);
                    }
                }
            }
            else
            {
                list = group.sos_list[sosindex - 1].members;
                n = list[0];

                /* Search for the variable (normally always faster to do linear search here) */
                column = -column;
                for (i = 1; i <= n; i++)
                {
                    if (list[i] == column)
                    {
                        return (1);
                    }
                }
            }
            return (0);

        }
        public static byte SOS_is_active(SOSgroup group, int sosindex, int column)
        {
            int i;
            int n;
            int nn;
            
            //ORIGINAL LINE: int *list;
            int[] list;
            lprec lp = group.lp;

#if Paranoia
  if ((sosindex < 0) || (sosindex > group.sos_count))
  {
	report(lp, IMPORTANT, "SOS_is_active: Invalid SOS index %d\n", sosindex);
	return (0);
  }
#endif

            if ((lp.var_type[column] & (lp_lib.ISSOS | lp_lib.ISGUB) == 0) )
            {
                return (0);
            }

            if (sosindex == 0)
            {
                for (i = group.memberpos[column - 1]; i < group.memberpos[column]; i++)
                {
                    nn = group.membership[i];
                    n = SOS_is_active(group, nn, column);
                    if (n != 0)
                    {
                        return (1);
                    }
                }
            }
            else
            {

                list = group.sos_list[sosindex - 1].members;
                n = list[0] + 1;
                nn = list[n];

                /* Scan the active (non-zero) SOS index list */
                for (i = 1; (i <= nn) && (list[n + i] != 0); i++)
                {
                    if (list[n + i] == column)
                    {
                        return (1);
                    }
                }
            }
            return (0);

        }
        public static bool SOS_is_full(SOSgroup group, int sosindex, int column, byte activeonly)
        {
        throw new NotImplementedException();}
        public static byte SOS_can_activate(SOSgroup group, int sosindex, int column)
        {
            int i;
            int n;
            int nn;
            int nz;
            //ORIGINAL LINE: int *list;
            int[] list;
            lprec lp;

            if (group == null)
            {
                return (0);
            }
            lp = group.lp;

#if Paranoia
  if ((sosindex < 0) || (sosindex > group.sos_count))
  {
	report(lp, IMPORTANT, "SOS_can_activate: Invalid SOS index %d\n", sosindex);
	return (0);
  }
#endif

            if (((lp.var_type[column]) & (lp_lib.ISSOS | lp_lib.ISGUB) == 0) )
            {
                return (0);
            }

            if (sosindex == 0)
            {
                for (i = group.memberpos[column - 1]; i < group.memberpos[column]; i++)
                {
                    nn = group.membership[i];
                    n = SOS_can_activate(group, nn, column);
                    if (n == 0)
                    {
                        return (0);
                    }
                }
            }
            else if (SOS_is_member(group, sosindex, column))
            {

                list = group.sos_list[sosindex - 1].members;
                n = list[0] + 1;
                nn = list[n];

#if false
// /* Accept if the SOS is empty */
//    if(list[n+1] == 0)
//      return(TRUE);
#endif

                /* Cannot activate a variable if the SOS is full */
                if (list[n + nn] != 0)
                {
                    return (0);
                }

                /* Check if there are variables quasi-active via non-zero lower bounds */
                nz = 0;
                for (i = 1; i < n; i++)
                {
                    if (lp.bb_bounds.lowbo[lp.rows + System.Math.Abs(list[i])] > 0)
                    {
                        nz++;
                        /* Reject outright if selected column has a non-zero lower bound */
                        if (list[i] == column)
                        {
                            return (0);
                        }
                    }
                }
#if Paranoia
	if (nz > nn)
	{
	  report(lp, SEVERE, "SOS_can_activate: Found too many non-zero member variables for SOS index %d\n", sosindex);
	}
#endif
                for (i = 1; i <= nn; i++)
                {
                    if (list[n + i] == 0)
                    {
                        break;
                    }
                    if (lp.bb_bounds.lowbo[lp.rows + list[n + i]] == 0)
                    {
                        nz++;
                    }
                }
                if (nz == nn)
                {
                    return (0);
                }

                /* Accept if the SOS is empty */
                if (list[n + 1] == 0)
                {
                    return (1);
                }

                /* Check if we can set variable active in SOS2..SOSn
                  (must check left and right neighbours if one variable is already active) */
                if (nn > 1)
                {

                    /* Find the variable that was last activated;
                      Also check that the candidate variable is not already active */
                    for (i = 1; i <= nn; i++)
                    {
                        if (list[n + i] == 0)
                        {
                            break;
                        }
                        if (list[n + i] == column)
                        {
                            return (0);
                        }
                    }
                    i--;
                    nn = list[n + i];

                    /* SOS accepts an additional variable; confirm neighbourness of candidate;
                       Search for the SOS set index of the last activated variable */
                    n = list[0];
                    for (i = 1; i <= n; i++)
                    {
                        if (System.Math.Abs(list[i]) == nn)
                        {
                            break;
                        }
                    }
                    if (i > n)
                    {
                        string msg = "SOS_can_activate: Internal index error at SOS %d\n";
                        lp.report(lp, lp_lib.CRITICAL, ref msg, sosindex);
                        return (0);
                    }
                    /* SOS accepts an additional variable; confirm neighbourness of candidate */

                    /* Check left neighbour */
                    if ((i > 1) && (list[i - 1] == column))
                    {
                        return (1);
                    }
                    /* Check right neighbour */
                    if ((i < n) && (list[i + 1] == column))
                    {
                        return (1);
                    }

                    /* It is not the right neighbour; return false */
                    return (0);
                }
            }
            return (1);
        }
        public static byte SOS_set_marked(SOSgroup group, int sosindex, int column, byte asactive)
        {
        throw new NotImplementedException();}
        public static bool SOS_unmark(SOSgroup group, int sosindex, int column)
        {
            int i;
            int n;
            int nn;
            //ORIGINAL LINE: int *list;
            int[] list;
            bool isactive;
            lprec lp = group.lp;
            LpCls objLpCls = new LpCls();

#if Paranoia
  if ((sosindex < 0) || (sosindex > group.sos_count))
  {
	report(lp, IMPORTANT, "SOS_unmark: Invalid SOS index %d\n", sosindex);
	return (0);
  }
#endif

            if ((lp.var_type[column] & (lp_lib.ISSOS | lp_lib.ISGUB) == 0))
            {
                return (false);
            }


            if (sosindex == 0)
            {

                /* Undefine a SOS3 member variable that has temporarily been set as integer */
                //ORIGINAL LINE: if ((lp.var_type[column] & lp_lib.ISSOSTEMPINT) != 0)
                if ((lp.var_type[column]!=false) & (lp_lib.ISSOSTEMPINT) != 0)
                {
                    //NOTED ISSUE
                    lp.var_type[column] = lp_lib.ISSOSTEMPINT;
                    objLpCls.set_int(lp, column, 0);
                }

                nn = 0;
                for (i = group.memberpos[column - 1]; i < group.memberpos[column]; i++)
                {
                    n = group.membership[i];
                    if (SOS_unmark(group, n, column))
                    {
                        nn++;
                    }
                }
                return ((bool)(nn == group.sos_count));
            }
            else
            {
                list = group.sos_list[sosindex - 1].members;
                n = list[0] + 1;
                nn = list[n];

                /* Search for the variable */
                i = SOS_member_index(group, sosindex, column);

                /* Restore sign in main list */
                if ((i > 0) && (list[i] < 0))
                {
                    list[i] *= -1;
                }
                else
                {
                    return (true);
                }

                /* Find the variable in the active list... */
                isactive = Convert.ToBoolean(SOS_is_active(group, sosindex, column));
                if (isactive)
                {
                    for (i = 1; i <= nn; i++)
                    {
                        if (list[n + i] == column)
                        {
                            break;
                        }
                    }
                    /* ...shrink the list if found, otherwise return error */
                    if (i <= nn)
                    {
                        for (; i < nn; i++)
                        {
                            list[n + i] = list[n + i + 1];
                        }
                        list[n + nn] = 0;
                        return (true);
                    }
                    return (false);
                }
                else
                {
                    return (true);
                }
            }
        }
        public static int SOS_fix_unmarked(SOSgroup group, int sosindex, int variable, ref double bound, double value, byte isupper, ref int diffcount, DeltaVrec changelog)
        {
        throw new NotImplementedException();}
        public static int SOS_fix_list(SOSgroup group, int sosindex, int variable, ref double bound, ref int varlist, byte isleft, DeltaVrec changelog)
        {
        throw new NotImplementedException();}
        public static int SOS_is_satisfied(SOSgroup group, int sosindex, ref double[] solution)
        /* Determine if the SOS is satisfied for the current solution vector;
The return code is in the range [-2..+2], depending on the type of
satisfaction.  Positive return value means too many non-zero values,
negative value means set incomplete:

          -2: Set member count not full (SOS3)
          -1: Set member count not full
           0: Set is full (also returned if the SOS index is invalid)
           1: Too many non-zero sequential variables
           2: Set consistency error

*/
        {
            int i;
            int n;
            int nn;
            int count;
            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //ORIGINAL LINE: int *list;
            int[] list=null;
            int type;
            int status = 0;
            lprec lp = group.lp;

#if Paranoia
  if ((sosindex < 0) || (sosindex > group.sos_count))
  {
	report(lp, IMPORTANT, "SOS_is_satisfied: Invalid index %d\n", sosindex);
	return (SOS_COMPLETE);
  }
#endif

            if ((sosindex == 0) && (group.sos_count == 1))
            {
                sosindex = 1;
            }

            if (sosindex == 0)
            {
                for (i = 1; i <= group.sos_count; i++)
                {
                    status = SOS_is_satisfied(group, i, ref solution);
                    if ((status != SOS_COMPLETE) && (status != SOS_INCOMPLETE))
                    {
                        break;
                    }
                }
            }
            else
            {
                type = SOS_get_type(group, sosindex);
                list = group.sos_list[sosindex - 1].members;
                n = list[0] + 1;
                nn = list[n];

                /* Count the number of active SOS variables */
                for (i = 1; i <= nn; i++)
                {
                    if (list[n + i] == 0)
                    {
                        break;
                    }
                }
                count = i - 1;
                if (count == nn)
                {
                    status = SOS_COMPLETE; // Set is full
                }
                else
                {
                    status = SOS_INCOMPLETE; // Set is partial
                }

                /* Find index of the first active variable; fail if some are non-zero */
                if (count > 0)
                {
                    nn = list[n + 1];
                    for (i = 1; i < n; i++)
                    {
                        if ((System.Math.Abs(list[i]) == nn) || (solution[lp.rows + System.Math.Abs(list[i])] != 0))
                        {
                            break;
                        }
                    }
                    if (System.Math.Abs(list[i]) != nn)
                    {
                        status = SOS_INTERNALERROR; // Set consistency error (leading set variables are non-zero)
                    }
                    else
                    {
                        /* Scan active SOS variables until we find a non-zero value */
                        while (count > 0)
                        {
                            if (solution[lp.rows + System.Math.Abs(list[i])] != 0)
                            {
                                break;
                            }
                            i++;
                            count--;
                        }
                        /* Scan active non-zero SOS variables; break at first non-zero (rest required to be zero) */
                        while (count > 0)
                        {
                            if (solution[lp.rows + System.Math.Abs(list[i])] == 0)
                            {
                                break;
                            }
                            i++;
                            count--;
                        }
                        if (count > 0)
                        {
                            status = SOS_INTERNALERROR; // Set consistency error (active set variables are zero)
                        }
                    }
                }
                else
                {
                    i = 1;
                    /* There are no active variables; see if we have happened to find a valid header */
                    while ((i < n) && (solution[lp.rows + System.Math.Abs(list[i])] == 0))
                    {
                        i++;
                    }
                    count = 0;
                    while ((i < n) && (count <= nn) && (solution[lp.rows + System.Math.Abs(list[i])] != 0))
                    {
                        count++;
                        i++;
                    }
                    if (count > nn)
                    {
                        status = SOS_INFEASIBLE; // Too-many sequential non-zero variables
                    }
                }

                /* Scan the trailing set of SOS variables; fail if some are non-zero */
                if (status <= 0)
                {
                    n--;
                    while (i <= n)
                    {
                        if (solution[lp.rows + System.Math.Abs(list[i])] != 0)
                        {
                            break;
                        }
                        i++;
                    }
                    if (i <= n)
                    {
                        status = SOS_INFEASIBLE; // Too-many sequential non-zero variables
                    }

                    /* Code member deficiency for SOS3 separately */
                    else if ((status == -1) && (type <= SOS3))
                    {
                        status = SOS3_INCOMPLETE;
                    }
                }

            }
            return (status);
        }
        public static byte SOS_is_feasible(SOSgroup group, int sosindex, ref double solution)
        {
        throw new NotImplementedException();}

    }
}
