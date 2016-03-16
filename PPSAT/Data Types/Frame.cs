using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSAT
{
    public class Frame
    {
        public Variable decision_variable                                   { get { return _decision_variable_; } }
        public List<Disjunction> disjunctions                               { get { return _disjunctions_; } }
        public List<Variable> M                                             { get { return _M_; } }
        public Dictionary<Variable, HashSet<Disjunction>> var_disjunctions  { get { return _var_disjunctions_; } }

        private List<Disjunction> _disjunctions_;
        private List<Variable> _M_;
        private Dictionary<Variable, HashSet<Disjunction>> _var_disjunctions_;
        private Variable _decision_variable_;

        public Frame(List<Disjunction> disjunctions, List<Variable> M, Dictionary<Variable, HashSet<Disjunction>> var_disjunctions, Variable v)
        {
            _disjunctions_ = disjunctions;
            _M_ = M;
            _var_disjunctions_ = var_disjunctions;
            _decision_variable_ = v;
        }
    }
}
