﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace PPSAT
{
    public class SAT_Solver
    {
        public static List<Variable> finalModel; //The final list of variables to print out
        public static state ready = state.UNSOLVED; //0 indicates that we haven't determined if it's SAT or UNSAT

        public enum state { UNSOLVED, SAT, UNSAT };

        /// <summary>
        /// Main function, which takes input of file path to the input file
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            List<Disjunction> disjunctions; //The list of disjunctive clauses in the CNF
            Dictionary<int, HashSet<Disjunction>> var_disjunctions; //Maps variables to a hashset of all of the disjunctions they're in
            List<Variable> M; //The model - storing 

            //Check to see if the filepath was passed in
            if (args.Count() > 0)
            {
                //Ttry to read the file
                if (!ReadFile(out disjunctions, out var_disjunctions, args[0]))
                {
                    Console.WriteLine("Could not read file. Exiting...");
                    return;
                }
            }
            else
            {
                //If the filepath wasn't passed in, request it
                Console.WriteLine("Please input file path: ");
                String path = Console.ReadLine();

                //Try to read the file
                if (!ReadFile(out disjunctions, out var_disjunctions, path))
                {
                    Console.WriteLine("Could not read file. Exiting...");
                    Console.ReadLine();
                    return;
                }
            }

            //Create the frames in order to store copies of all data
            Frame f1 = new Frame(disjunctions, new List<Variable>(), var_disjunctions, new Variable(-1, false));
            Frame f2 = new Frame(disjunctions, new List<Variable>(), var_disjunctions, new Variable(-1, false));
            Frame f3 = new Frame(disjunctions, new List<Variable>(), var_disjunctions, new Variable(-1, false));
            Frame f4 = new Frame(disjunctions, new List<Variable>(), var_disjunctions, new Variable(-1, false));

            Random r1 = new Random(1);
            Random r2 = new Random(2);
            Random r3 = new Random(7);
            Random r4 = new Random(19);

            //Set up the first thread
            List<Disjunction> d1 = f1.disjunctions;
            Dictionary<int, HashSet<Disjunction>> vd1 = f1.var_disjunctions;
            List<Variable> M1 = new List<Variable>();
            Thread t1 = new Thread(() => Solve(ref d1, ref vd1, out M1, r1, 0.2d));
            t1.Start();

            //Set up the second thread
            List<Disjunction> d2 = f2.disjunctions;
            Dictionary<int, HashSet<Disjunction>> vd2 = f2.var_disjunctions;
            List<Variable> M2 = new List<Variable>();
            Thread t2 = new Thread(() => Solve(ref d2, ref vd2, out M2, r2, 0.1d));
            t2.Start();

            //Set up the second thread
            List<Disjunction> d3 = f3.disjunctions;
            Dictionary<int, HashSet<Disjunction>> vd3 = f3.var_disjunctions;
            List<Variable> M3 = new List<Variable>();
            Thread t3 = new Thread(() => Solve(ref d3, ref vd3, out M3, r3, 0.01d));
            t3.Start();

            //Set up the second thread
            List<Disjunction> d4 = f4.disjunctions;
            Dictionary<int, HashSet<Disjunction>> vd4 = f4.var_disjunctions;
            List<Variable> M4 = new List<Variable>();
            Thread t4 = new Thread(() => Solve(ref d4, ref vd4, out M4, r4, 0.5d));
            t4.Start();

            //Random r = new Random();
            //Solve(ref disjunctions, ref var_disjunctions, out M, r, 0.2d);
            while (ready == state.UNSOLVED)
            {
                Thread.Sleep(2); //suspend for 10 milliseconds while we wait for one of the threads to solve it 
            }

            if(ready == state.SAT)
            {
                Console.WriteLine("SATISFIABLE");

                foreach (Variable v in finalModel)
                    Console.WriteLine(v.ID + " : " + v.value);
            }
            else
            {
                Console.WriteLine("UNSATISFIABLE");
            }

            t1.Abort();
            t2.Abort();
            t3.Abort();
            t4.Abort();

            //Console.ReadLine(); //DELETE
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

                            int index = -1;
                            if((index = d.IndexOf(v)) != -1)
                            {
                                //If this disjunction has v or ~v it is a tautology and we can ignore it
                                if(!d[index].SameSign(v))
                                {
                                    //Remove this disjunction and get rid of the disjunction in every applicable variable's dictionary
                                    foreach(Variable var in d)
                                    {
                                        HashSet<Disjunction> h = var_disjunctions[var.ID];
                                        h.Remove(d);
                                    }

                                    d.Clear();
                                    break;
                                }

                                //If we reach this point, this variable is indeed in the disjunction, but has the same value as the other v. So we'll just continue
                                continue;
                            }
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

        private static bool Solve(ref List<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, out List<Variable> M, Random r, double max_time)
        {
            M = new List<Variable>();
            Stack<Frame> frames = new Stack<Frame>();
            Variable false_var = new Variable(-1, false);
            Frame start_frame = new Frame(disjunctions, M, var_disjunctions, false_var);

            DateTime start_time = DateTime.Now;

            //Go until it is complete
            while(!Complete(var_disjunctions, disjunctions))
            {
                if ((DateTime.Now - start_time).TotalSeconds < max_time)
                {
                    Variable v = Propagate(ref disjunctions, ref var_disjunctions, ref M);
                    if (v != null) //Check to see if we can propagate
                    {
                        //Try to add the variable
                        if (!AddVariable(ref disjunctions, ref var_disjunctions, ref M, v))
                        {
                            //If we can't, try to add the variable with the opposite value
                            if (!AddVariable(ref disjunctions, ref var_disjunctions, ref M, !v))
                            {
                                //If both of these fail, then we need to fail or backtrack
                                if (!Fail_Or_Backtrack(ref disjunctions, ref var_disjunctions, ref M, ref frames))
                                {
                                    ready = state.UNSAT;
                                    finalModel = M;
                                    return false;
                                }
                            }
                        }
                    }
                    else //Decide
                    {
                        if (!Decide(ref disjunctions, ref var_disjunctions, ref M, ref frames, r))
                        {
                            if (!Fail_Or_Backtrack(ref disjunctions, ref var_disjunctions, ref M, ref frames))
                            {
                                ready = state.UNSAT;
                                finalModel = M;
                                return false;
                            }
                        }
                    }
                }
                else //Restart, we timed out
                {
                    max_time *= 2;
                    disjunctions = start_frame.disjunctions;
                    var_disjunctions = start_frame.var_disjunctions;
                    M = start_frame.M;

                    start_frame = new Frame(disjunctions, M, var_disjunctions, false_var);
                }
            }

            ready = state.SAT;
            finalModel = M;
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
        private static bool Decide(ref List<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M, ref Stack<Frame> frames, Random r)
        {
            //foreach(KeyValuePair<int, HashSet<Disjunction>> kp in var_disjunctions)
            foreach(Disjunction d in disjunctions)
            {
                if (d.Count == 0)
                    continue;

                Variable v = d[r.Next() % d.Count];

                //Find a variable that has at least one disjunction it can get rid of and try out a value
                //if(kp.Value.Count > 0)
                {
                    //If we cannot add a variable or its negation, then we need to return false
                    //Variable v = new Variable(kp.Key, true);
                    frames.Push(new Frame(disjunctions, M, var_disjunctions, v));
                    if (!AddVariable(ref disjunctions, ref var_disjunctions, ref M, v))
                    {
                        //If that didn't work, then this isn't a case where we can decide. We have to use the other value
                        frames.Pop();
                        if (!AddVariable(ref disjunctions, ref var_disjunctions, ref M, !v))
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
                if ((M[i] == v) && !(M[i].SameSign(v))) return false;

            //Check to see if this leads to a contradiction
            HashSet<Disjunction> h = var_disjunctions[v.ID];
            foreach(Disjunction d in h)
            {
                //Check to see if there's a disjunction with only ~v (meaning there's a contradiction)
                if (d.Count == 1 && d[0] == v && !d[0].SameSign(v))
                    return false;
            }

            //If we didn't return false, then we can successfully add this variable
            Disjunction[] temp = h.ToArray(); //Every disjunction that contains 
            for(int i = 0; i < temp.Count(); i++)
            {
                Disjunction d = temp[i];

                //If this disjunction does contain at least one instance of v rather than it exclusively containing ~v
                //Then let's get rid of the disjunction (it has been satisfied) and update our DSs.
                if (d[d.IndexOf(v)].SameSign(v))
                {
                    //Remove from the current hash set
                    h.Remove(d);

                    //Remove from the hash sets of all other involved variables
                    foreach(Variable var in d)
                        var_disjunctions[var.ID].Remove(d);

                    //Remove the disjunction from the disjunctions list
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
        private static bool Complete(Dictionary<int, HashSet<Disjunction>> var_disjunctions, List<Disjunction> disjunctions)
        {
            //bool empty = true;
            if (disjunctions.Count == 0)
                return true;

            return false;
        }
    }
}
