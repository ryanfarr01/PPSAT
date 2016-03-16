using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSAT
{
    public struct Variable
    {
        public bool value;

        public Variable(bool value) { this.value = value; }

        public Variable Copy() { return new Variable(value); }

        public static bool operator ==(Variable v1, Variable v2) { return v1.value == v2.value; }

        public static bool operator !=(Variable v1, Variable v2) { return !(v1.value == v2.value); }

        public static bool operator !(Variable v) { return !v.value; }

        public static bool operator true(Variable v) { return v.value; }

        public static bool operator false(Variable v) { return !v.value; }

        public static Variable operator &(Variable v1, Variable v2) { return new Variable(v1.value && v2.value); }

        public static Variable operator |(Variable v1, Variable v2) { return new Variable(v1.value || v2.value); }
    }
}
