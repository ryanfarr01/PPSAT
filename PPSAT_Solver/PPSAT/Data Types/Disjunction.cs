using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSAT
{
    public class Disjunction : IList<Variable>
    {
        private List<Variable> vars = new List<Variable>();

        public Variable this[int index]
        {
            get { return vars[index]; }
            set { vars[index] = value; }
        }

        public Disjunction Clone()
        {
            Disjunction r = new Disjunction();

            for (int i = 0; i < vars.Count; i++)
                r.Add(vars[i]);

            return r;
        }

        public bool Contains(Variable item) { return vars.Contains(item); }

        public int IndexOf(Variable item) { return vars.IndexOf(item); }

        public bool Remove(Variable item) { return vars.Remove(item); }

        public void Insert(int index, Variable item) { vars.Insert(index, item); }

        public void CopyTo(Variable[] array, int arrayIndex) { vars.CopyTo(array, arrayIndex); }

        public void RemoveAt(int index) { vars.RemoveAt(index); }

        public int Count { get { return vars.Count; } }

        public bool IsReadOnly { get { return false; } }

        public void Add(Variable v) { vars.Add(v); }

        public void Clear() { vars = new List<Variable>(); }

        public IEnumerator<Variable> GetEnumerator() { return vars.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return vars.GetEnumerator(); }
    }
}
