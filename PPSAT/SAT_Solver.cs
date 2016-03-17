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
            Dictionary<int, HashSet<Disjunction>> var_disjunctions;
            List<Variable> M = new List<Variable>();

            if (args.Count() > 0)
            {
                if (!ReadFile(out disjunctions, out var_disjunctions, args[0]))
                {
                    Console.WriteLine("Could not read file. Exiting...");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Please input file path: ");
                String path = Console.ReadLine();

                if (!ReadFile(out disjunctions, out var_disjunctions, path))
                {
                    Console.WriteLine("Could not read file. Exiting...");
                    return;
                }
            }
        }

        public static bool ReadFile(out List<Disjunction> disjunctions, out Dictionary<int, HashSet<Disjunction>> var_disjunctions, string path)
        {
            disjunctions = new List<Disjunction>();
            var_disjunctions = new Dictionary<int, HashSet<Disjunction>>();

            int lines, vars;
            string line;
            System.IO.StreamReader file = null;
            try { file = new System.IO.StreamReader(path); }
            catch (Exception) { return false; }

            while ((line = file.ReadLine()) != null)
            {
                string[] parts = line.Split(' ');
                if (parts.Count() > 0)
                {
                    //A comment in the file
                    if (parts[0] == "c") continue;

                    //The first line, showing the number of lines and variables
                    if (parts.Count() >= 4 && parts[0] == "p" && parts[1] == "cnf" &&
                       int.TryParse(parts[2], out vars) && int.TryParse(parts[3], out lines))
                    {
                        continue;
                    }

                    //Otherwise we need to store variables
                    Disjunction d = new Disjunction();
                    foreach (String s in parts)
                    {
                        int i;
                        if (int.TryParse(s, out i))
                        {
                            //0 indicates the end of the clause
                            if (i == 0) break;

                            //Make the variable
                            Variable v = new Variable(Math.Abs(i), i > 0);

                            //add the variable to the disjunction
                            d.Add(v);

                            //If we don't already have a hashset for v, make one
                            if (!var_disjunctions.ContainsKey(v.ID))
                            {
                                var_disjunctions.Add(v.ID, new HashSet<Disjunction>());
                            }

                            //Note that, although the clause is not yet complete,
                            //we're actually passing a reference, which means it will be correct
                            //once we finish with this clause
                            var_disjunctions[v.ID].Add(d);
                        }
                        else return false;
                    }
                    if (d.Count > 0) { disjunctions.Add(d); }
                }
                else return false;
            }

            return true;
        }
    }
}
