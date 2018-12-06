using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZS.Math.Optimization
{
    public class colamd
    {
        /*PREVIOUS CODE:
        #define COLAMD_RECOMMENDED(nnz, n_row, n_col)                                 \
(                                                                             \
((nnz) < 0 || (n_row) < 0 || (n_col) < 0)                                     \
?                                                                             \
    (-1)                                                                      \
:                                                                             \
    (int) (2 * (nnz) + COLAMD_C(n_col) + COLAMD_R(n_row) + (n_col) + ((nnz) / 5)) \
)
*/
        private static Func<int, int, int, int> COLAMD_RECOMMENDED = (nnz, n_row, n_col) => (((nnz) < 0 || (n_row) < 0 || (n_col) < 0) ? (-1) : (int)(2 * (nnz) + COLAMD_C(n_col) + COLAMD_R(n_row) + (n_col) + ((nnz) / 5)));

        // PREVIOUS CODE: #define COLAMD_C(n_col) (((n_col) + 1) * sizeof (Colamd_Col) / sizeof (int))
        private static Func<int, int> COLAMD_C = (n_col) => n_col + 1;

        //PREVIOUS CODE: #define COLAMD_R(n_row) (((n_row) + 1) * sizeof (Colamd_Row) / sizeof (int))
        private static Func<int, int> COLAMD_R = (n_row) => n_row + 1;

        /*PREVIOUS CODE
        void colamd_set_defaults	// sets default parameters 
(				// knobs argument is modified on output 
    double knobs[COLAMD_KNOBS]	// parameter settings for colamd 
) ;   
    */

        internal static int colamd_recommended(int nnz, int n_row, int n_col)
        {
            return (COLAMD_RECOMMENDED(nnz, n_row, n_col));
        }

    }
}
