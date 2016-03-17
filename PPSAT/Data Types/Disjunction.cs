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

        public bool Contains(Variable item)
        {
            for(int i = 0; i < vars.Count; i++) //IS THIS NECESSARY?
                if (vars[i] == item) return true;

            return false;
        }


        public int IndexOf(Variable item)
        {
            for(int i = 0; i < vars.Count; i++) //IS THIS NECESSARY?
                if(vars[i] == item)  return i; 

            return -1;
        }

        public bool Remove(Variable item)
        {
            foreach (Variable v in vars) //IS THIS NECESSARY? MAY ALREADY WORK WITH JUST vars.Remove(item)
            {
                if(v == item)
                {
                    vars.Remove(v);
                    return true;
                }
            }

            return false;
        }

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
