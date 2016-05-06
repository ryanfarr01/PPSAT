using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSAT
{
    /// <summary>
    /// Frame is a snapshot of all data at a given moment in time. Duplicates all data
    /// given to the constructor so that the memory does not reference the same areas.
    /// 
    /// This is used for backtracking in the DPLL algorithm.
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// The variable that was chosen to be put in the model on Decide
        /// </summary>
        public Variable decision_variable                              { get { return _decision_variable_; } }

        /// <summary>
        /// The set of disjunctions that form the expression
        /// </summary>
        public HashSet<Disjunction> disjunctions                       { get { return _disjunctions_; } }

        /// <summary>
        /// The model
        /// </summary>
        public List<Variable> M                                        { get { return _M_; } }

        /// <summary>
        /// The dictionary that maps every variable to the set of disjunctions that it is within
        /// </summary>
        public Dictionary<int, HashSet<Disjunction>> var_disjunctions  { get { return _var_disjunctions_; } }

        //The actual set of data so that this is not altered by the user
        private HashSet<Disjunction> _disjunctions_;
        private List<Variable> _M_;
        private Dictionary<int, HashSet<Disjunction>> _var_disjunctions_;
        private Variable _decision_variable_;

        /// <summary>
        /// Constructor duplicates the current inputs and stores them. This should be done
        /// BEFORE the decision variable is added to M.
        /// </summary>
        /// <param name="disjunctions"></param>
        /// <param name="M"></param>
        /// <param name="var_disjunctions"></param>
        /// <param name="v"></param>
        public Frame(HashSet<Disjunction> disjunctions, List<Variable> M, Variable v)
        {
            //Duplicate the disjunctions
            HashSet<Disjunction> d = new HashSet<Disjunction>();
            Dictionary<int, HashSet<Disjunction>> dict = new Dictionary<int, HashSet<Disjunction>>();

            foreach (Disjunction disj in disjunctions)
            {
                Disjunction temp_d = disj.Clone();
                d.Add(temp_d);

                foreach (Variable var in disj)
                {
                    //Check to see if a dictionary doesn't arleady exist
                    if (!dict.ContainsKey(var.ID))
                        dict.Add(var.ID, new HashSet<Disjunction>());

                    //Add the disjunction to the variable's hashset within the dictionary
                    dict[var.ID].Add(temp_d);
                }
            }

            //Duplicate M
            List<Variable> M_vars = new List<Variable>();
            foreach (Variable var in M)
                M_vars.Add(var);

            //Put in the storage objects
            _disjunctions_ = d;
            _M_ = M_vars;
            _var_disjunctions_ = dict;
            _decision_variable_ = v.Clone();
        }
    }
}
