using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSAT
{
    public class Frame
    {
        public Variable decision_variable                              { get { return _decision_variable_; } }
        public HashSet<Disjunction> disjunctions                       { get { return _disjunctions_; } }
        public List<Variable> M                                        { get { return _M_; } }
        public Dictionary<int, HashSet<Disjunction>> var_disjunctions  { get { return _var_disjunctions_; } }

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
