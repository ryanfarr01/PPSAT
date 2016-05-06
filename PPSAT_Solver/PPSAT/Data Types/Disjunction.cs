using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSAT
{
    /// <summary>
    /// Class to represent a disjunction. This is actually a List of 
    /// variables.
    /// </summary>
    public class Disjunction : IList<Variable>
    {
        /// <summary>
        /// The actual data being stored
        /// </summary>
        private List<Variable> vars = new List<Variable>();

        /// <summary>
        /// Returns the Variable at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Variable this[int index]
        {
            get { return vars[index]; }
            set { vars[index] = value; }
        }

        /// <summary>
        /// Clones the disjunction. 
        /// 
        /// Note: Keeps the same Variable references rather than creating new ones
        /// </summary>
        /// <returns></returns>
        public Disjunction Clone()
        {
            Disjunction r = new Disjunction();

            for (int i = 0; i < vars.Count; i++)
                r.Add(vars[i]);

            return r;
        }

        /// <summary>
        /// Tells if the Variable is in the list
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(Variable item) { return vars.Contains(item); }

        /// <summary>
        /// Returns the index of the Variable if it is in teh list
        /// or -1 if it is not
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(Variable item) { return vars.IndexOf(item); }

        /// <summary>
        /// Removes a variable from the disjunction. Returns true if the
        /// variable was within the list or false otherwise.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(Variable item) { return vars.Remove(item); }

        /// <summary>
        /// Inserts a variable at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, Variable item) { vars.Insert(index, item); }

        /// <summary>
        /// Copies the disjunction to the array starting
        /// at arrayIndex
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(Variable[] array, int arrayIndex) { vars.CopyTo(array, arrayIndex); }

        /// <summary>
        /// Removes the variable that's at the given index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index) { vars.RemoveAt(index); }

        /// <summary>
        /// Returns the number of Variables currently in the
        /// disjunction
        /// </summary>
        public int Count { get { return vars.Count; } }

        /// <summary>
        /// Tells if the Disjunction is readonly, which it is not
        /// </summary>
        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Adds a variable to the disjunction
        /// </summary>
        /// <param name="v"></param>
        public void Add(Variable v) { vars.Add(v); }

        /// <summary>
        /// Clears the disjunction of all variables
        /// </summary>
        public void Clear() { vars = new List<Variable>(); }

        /// <summary>
        /// Returns an enumerator to go through all variables in the disjunction
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Variable> GetEnumerator() { return vars.GetEnumerator(); }

        /// <summary>
        /// Returns the enumerator to go through all variables in teh disjunction
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() { return vars.GetEnumerator(); }
    }
}
