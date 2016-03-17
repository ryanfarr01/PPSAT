using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSAT
{
    public class Variable
    {
        /// <summary>
        /// The value of the variable as a boolean value
        /// </summary>
        public bool value;

        /// <summary>
        /// The identifier of the variable
        /// </summary>
        public int ID;

        /// <summary>
        /// Variable constructor, which stores the ID and the value that this variable
        /// will hold.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="value"></param>
        public Variable(int ID, bool value)
        {
            this.ID = ID;
            this.value = value;
        }

        /// <summary>
        /// Creates an exact copy of the variable
        /// </summary>
        /// <returns></returns>
        public Variable Clone() { return new Variable(ID, value); }

        /// <summary>
        /// Hash codes for Variables are simply based on their ID so that we can 
        /// use the expression variables[var] = x for dictionaries where var 
        /// relates to the variable symbolically, but does not have the same address.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID;
        }

        /// <summary>
        /// Tells if the values of the variables are the same
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(Object.ReferenceEquals(null, obj))  return false; 
            Variable v = obj as Variable;
            return v.value == value;
        }

        /// <summary>
        /// Tells if the IDs are the same
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator ==(Variable v1, Variable v2)
        {
            if(!Object.ReferenceEquals(v1, null) && !Object.ReferenceEquals(v2, null))
                return v1.ID == v2.ID;

            if (Object.ReferenceEquals(null, v1) && Object.ReferenceEquals(v2, null))
                return true;

            //Otherwise one is null while the other is not
            return false;
        }

        /// <summary>
        /// Tells if the IDs are NOT the same
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator !=(Variable v1, Variable v2) { return !(v1 == v2); }

        /// <summary>
        /// Returns a the opposite of the current value of the variable
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Variable operator !(Variable v) { return new Variable(v.ID, !v.value); }

        /// <summary>
        /// Returns true if the value is true
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool operator true(Variable v) { return v.value; }

        /// <summary>
        /// Returns true if the value is false
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool operator false(Variable v) { return !v.value; }

        /// <summary>
        /// The & operator so that we can de facto override the && operator. Returns a new
        /// Variable with the ID of v1, but whose value is v1 && v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Variable operator &(Variable v1, Variable v2) { return new Variable(v1.ID, v1.value && v2.value); }

        /// <summary>
        /// The | operator so that we can de facto override the || operator. Returns a new
        /// Variable with the ID of v1, but whose value is v1 || v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Variable operator |(Variable v1, Variable v2) { return new Variable(v1.ID, v1.value || v2.value); }
    }
}
