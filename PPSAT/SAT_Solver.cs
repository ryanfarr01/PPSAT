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
            List<Variable> M;

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

            if(Solve(ref disjunctions, ref var_disjunctions, out M))
            {
                Console.WriteLine("Satisfiable");

                foreach(Variable v in M)
                    Console.WriteLine(v.ID + " : " + v.value);
            }
            else Console.WriteLine("Unsatisfiable");

            Console.ReadLine();
        }

        private static bool ReadFile(out List<Disjunction> disjunctions, out Dictionary<int, HashSet<Disjunction>> var_disjunctions, string path)
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

        private static bool Solve(ref List<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, out List<Variable> M)
        {
            M = new List<Variable>();
            Stack<Frame> frames = new Stack<Frame>();

            //Go until it is complete
            while(!Complete(var_disjunctions))
            {
                Variable v = Propagate(ref disjunctions, ref var_disjunctions, ref M);
                if(v != null) //Check to see if we can propagate
                {
                    //Try to add the variable
                    if(!AddVariable(ref disjunctions, ref var_disjunctions, ref M, v))
                    {
                        //If we can't, try to add the variable with the opposite value
                        if(!AddVariable(ref disjunctions, ref var_disjunctions, ref M, !v))
                        {
                            //If both of these fail, then we need to fail or backtrack
                            if (!Fail_Or_Backtrack(ref disjunctions, ref var_disjunctions, ref M, ref frames))
                                return false;
                        }
                    }
                }
                else //Decide
                {
                    if(!Decide(ref disjunctions, ref var_disjunctions, ref M, ref frames))
                    {
                        if (!Fail_Or_Backtrack(ref disjunctions, ref var_disjunctions, ref M, ref frames))
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Picks a variable and chooses its value. Automatically updates the frames stack
        /// and returns true if it is successfull. Returns false if it cannot decide.
        /// </summary>
        /// <param name="disjunctions"></param>
        /// <param name="var_disjunctions"></param>
        /// <param name="M"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        private static bool Decide(ref List<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M, ref Stack<Frame> frames)
        {
            foreach(KeyValuePair<int, HashSet<Disjunction>> kp in var_disjunctions)
            {
                //Find a variable that has at least one disjunction it can get rid of and try out a value
                if(kp.Value.Count > 0)
                {
                    //If we cannot add a variable or its negation, then we need to return false
                    Variable v = new Variable(kp.Key, true);
                    frames.Push(new Frame(disjunctions, M, var_disjunctions, v));
                    if (!AddVariable(ref disjunctions, ref var_disjunctions, ref M, new Variable(kp.Key, true)))
                    {
                        //If that didn't work, then this isn't a case where we can decide. We have to use the other value
                        frames.Pop();
                        if (!AddVariable(ref disjunctions, ref var_disjunctions, ref M, new Variable(kp.Key, false)))
                            return false;
                    }

                    //If we get to this point, we were able to successfully change a variable
                    return true;
                }
            }

            //If we don't return at this point, then all of the disjunctions have been taken care of
            //and we need to return SATISFIABLE back to the user
            return true;
        }

        /// <summary>
        /// Function that automatically back tracks if it's possible and returns true, or returns
        /// false if it is not possible to backtrack.
        /// </summary>
        /// <param name="disjunctions"></param>
        /// <param name="var_disjunctions"></param>
        /// <param name="M"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        private static bool Fail_Or_Backtrack(ref List<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M, ref Stack<Frame> frames)
        {
            if (frames.Count == 0) return false;

            Frame f = frames.Pop();
            disjunctions = f.disjunctions;
            var_disjunctions = f.var_disjunctions;
            M = f.M;
            
            //BE CAREFUL WITH THIS RECURSION. SHOULD CONSIDER DOING IT IN A LOOP (TAIL RECURSION)
            if (!AddVariable(ref disjunctions, ref var_disjunctions, ref M, !f.decision_variable))
                return Fail_Or_Backtrack(ref disjunctions, ref var_disjunctions, ref M, ref frames);

            return true;
        }

        /// <summary>
        /// Searches through the disjunctions to determine if there are any clauses with only 
        /// one variable. If one is found, it's returned. Otherwise null is returned
        /// </summary>
        /// <param name="disjunctions"></param>
        /// <param name="var_disjunctions"></param>
        /// <param name="M"></param>
        /// <returns></returns>
        private static Variable Propagate(ref List<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M)
        {
            foreach(Disjunction d in disjunctions)
                if (d.Count == 1) return d[0];

            return null;
        }

        /// <summary>
        /// Adds a variable to M and updates the disjunctions hash table. If there exists a contradiction,
        /// this will return false.
        /// </summary>
        /// <param name="M"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        private static bool AddVariable(ref List<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M, Variable v)
        {
            //Check to see if there's a contradiction
            for(int i = 0; i < M.Count; i++)
                if ((M[i] == v) && !(M[i].Equals(v))) return false;

            //Check to see if this leads to a contradiction
            HashSet<Disjunction> h = var_disjunctions[v.ID];
            foreach(Disjunction d in h)
            {
                if (d.Count == 1 && d[0] == v && !d[0].Equals(v))
                    return false;
            }

            //If we didn't return false, then we can successfully add this variable
            Disjunction[] temp = h.ToArray();
            for(int i = 0; i < temp.Count(); i++)
            {
                Disjunction d = temp[i];
                if(d[d.IndexOf(v)].Equals(v))
                {
                    //Remove from the current hash set
                    h.Remove(d);

                    //Remove from the hash sets of all other involved variables
                    foreach(Variable var in d)
                        var_disjunctions[var.ID].Remove(d);

                    //Remove from the disjunctions list
                    disjunctions.Remove(d);
                }
                else
                {
                    //Otherwise simply remove the current variable from the clause so that we don't consider it in the future
                    d.Remove(v);

                    //If we remove that, we need to remove the disjunction from h
                    h.Remove(d);
                }
            }

            //Add to M and return true
            M.Add(v);
            return true;
        }

        /// <summary>
        /// Function that determines if the given dictionary is empty. If it is, then all 
        /// clauses have been satisfied, indicating that this was satisfiable.
        /// </summary>
        /// <param name="var_disjunctions"></param>
        /// <returns></returns>
        private static bool Complete(Dictionary<int, HashSet<Disjunction>> var_disjunctions)
        {
            bool empty = true;
            foreach(KeyValuePair<int, HashSet<Disjunction>> kp in var_disjunctions)
                if (kp.Value.Count > 0) empty = false;

            return empty;
        }
    }
}
