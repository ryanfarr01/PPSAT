using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSAT
{
    public class SAT_Solver
    {
        /// <summary>
        /// Main function, which takes input of file path to the input file
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            List<Disjunction> disjunctions;
            List<Variable> M;
            Dictionary<Variable, HashSet<Disjunction>> var_disjunctions;

            if ((args.Count() > 0) && ReadFile(out disjunctions, out M, out var_disjunctions, args[0]))
            {
                //Do algorithm
            }
        }

        public static bool ReadFile(out List<Disjunction> disjunctions, out List<Variable> M, out Dictionary<Variable, HashSet<Disjunction>> var_disjunctions, string path)
        {
            disjunctions = new List<Disjunction>();
            M = new List<Variable>();
            var_disjunctions = new Dictionary<Variable, HashSet<Disjunction>>();

            return false;
        }
    }
}
