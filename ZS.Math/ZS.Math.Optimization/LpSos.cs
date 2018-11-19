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
        internal static byte SOS_shift_col(SOSgroup group, int sosindex, int column, int delta, LLrec usedmap, byte forceresort)
        {
        throw new NotImplementedException();}
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
        public static int SOS_get_candidates(SOSgroup group, int sosindex, int column, byte excludetarget, ref double upbound, ref double lobound)
        {
        throw new NotImplementedException();}
        public static int SOS_is_member(SOSgroup group, int sosindex, int column)
        {
        throw new NotImplementedException();}
        public static byte SOS_is_member_of_type(SOSgroup group, int column, int sostype)
        {
        throw new NotImplementedException();}
        public static byte SOS_set_GUB(SOSgroup group, int sosindex, byte state)
        {
        throw new NotImplementedException();}
        public static byte SOS_is_GUB(SOSgroup group, int sosindex)
        {
        throw new NotImplementedException();}
        public static byte SOS_is_marked(SOSgroup group, int sosindex, int column)
        {
        throw new NotImplementedException();}
        public static byte SOS_is_active(SOSgroup group, int sosindex, int column)
        {
        throw new NotImplementedException();}
        public static byte SOS_is_full(SOSgroup group, int sosindex, int column, byte activeonly)
        {
        throw new NotImplementedException();}
        public static byte SOS_can_activate(SOSgroup group, int sosindex, int column)
        {
        throw new NotImplementedException();}
        public static byte SOS_set_marked(SOSgroup group, int sosindex, int column, byte asactive)
        {
        throw new NotImplementedException();}
        public static byte SOS_unmark(SOSgroup group, int sosindex, int column)
        {
        throw new NotImplementedException();}
        public static int SOS_fix_unmarked(SOSgroup group, int sosindex, int variable, ref double bound, double value, byte isupper, ref int diffcount, DeltaVrec changelog)
        {
        throw new NotImplementedException();}
        public static int SOS_fix_list(SOSgroup group, int sosindex, int variable, ref double bound, ref int varlist, byte isleft, DeltaVrec changelog)
        {
        throw new NotImplementedException();}
        public static int SOS_is_satisfied(SOSgroup group, int sosindex, ref double solution)
        {
        throw new NotImplementedException();}
        public static byte SOS_is_feasible(SOSgroup group, int sosindex, ref double solution)
        {
        throw new NotImplementedException();}

    }
}
