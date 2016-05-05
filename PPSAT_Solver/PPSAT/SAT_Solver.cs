using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace PPSAT
{
    public class SAT_Solver
    {
        /// <summary>
        /// The final list of variables to print out
        /// </summary>
        public static List<Variable> finalModel; 

        /// <summary>
        /// An enum to indicate the state of the program
        /// </summary>
        public enum state { UNSOLVED, SAT, UNSAT }; 

        /// <summary>
        /// The current state of the program
        /// </summary>
        public static state ready = state.UNSOLVED;

        /// <summary>
        /// The number of threads to run this program concurrently
        /// </summary>
        private static int _NumThreads = 1;

        /// <summary>
        /// The number fo threads that can run at once per program
        /// </summary>
        private static int _ThreadsPerProgram = 4;

        /// <summary>
        /// The object that we lock on in order to make sure that the
        /// current available number of threads doesn't improperly update
        /// </summary>
        private static Object _LockObj = new Object();

        private static List<Thread> _ExtraThreads = new List<Thread>();

        /// <summary>
        /// Main function, which takes input of file path to the input file
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            HashSet<Disjunction> disjunctions; //The list of disjunctive clauses in the CNF
            Dictionary<int, HashSet<Disjunction>> var_disjunctions; //Maps variables to a hashset of all of the disjunctions they're in
            
            //Check to see if the filepath was passed in
            if (args.Count() > 0)
            {
                //Try to read the file
                if (!ReadFile(out disjunctions, out var_disjunctions, args[0]))
                {
                    Console.WriteLine("Could not read file. Exiting...");
                    return;
                }

                for(int i = 1; i < args.Length; i++)
                {
                    if(args[i].Replace(" ", "").Equals("-t"))
                    {
                        bool success = int.TryParse(args[i + 1], out _NumThreads);
                        if(!success || _NumThreads <= 0)
                        {
                            Console.WriteLine("Failed to get the threads value or value was less than 1, using default of 1");
                            _NumThreads = 1;
                        }
                    }
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

            Thread[] threads = new Thread[_NumThreads];
            Random r = new Random(); //A random number generate to generate the seeds of the other random number generators

            //Create all of the threads
            for(int i = 0; i < _NumThreads; i++)
            {
                //Set up a frame so that we don't share references
                Frame f = new Frame(disjunctions, new List<Variable>(), new Variable(-1, false));
                Random rt = new Random(r.Next());

                //Grab all of the data
                HashSet<Disjunction> d = f.disjunctions;
                Dictionary<int, HashSet<Disjunction>> vd = f.var_disjunctions;
                List<Variable> Mt = new List<Variable>();

                //Start the thread
                threads[i] = new Thread(() => DetermineSAT(ref d, ref vd, out Mt, rt, 0.1f));
                threads[i].Start();
            }

            //While we aren't done
            while (ready == state.UNSOLVED)
            {
                Thread.Sleep(1); //suspend for x milliseconds while we wait for one of the threads to solve it 
            }

            //Check if it was satisfiable
            if(ready == state.SAT)
            {
                Console.WriteLine("SATISFIABLE");

                foreach (Variable v in finalModel)
                    Console.WriteLine(v.ID + " : " + v.value);
            }
            else //Otherwise it was unsatisfiable
            {
                Console.WriteLine("UNSATISFIABLE");
            }

            Console.WriteLine("Number of threads: " + threads.Length);

            //Close all threads
            foreach (Thread t in threads)
                t.Abort();
            foreach (Thread t in _ExtraThreads)
                t.Abort();

            //Console.ReadLine();
        }

        private static bool ReadFile(out HashSet<Disjunction> disjunctions, out Dictionary<int, HashSet<Disjunction>> var_disjunctions, string path)
        {
            disjunctions = new HashSet<Disjunction>();
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

        private static bool DetermineSAT(ref HashSet<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions,
                                         out List<Variable> M, Random r, float max_time)
        {
            M = new List<Variable>();
            Stack<Frame> frames = new Stack<Frame>();
            Variable false_var = new Variable(-1, false);
            Frame start_frame = new Frame(disjunctions, M, false_var);

            return Solve(ref disjunctions, ref var_disjunctions, ref M, ref frames, ref r, start_frame, false_var, max_time, true);
        }

        private static bool Solve(ref HashSet<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M, 
                                  ref Stack<Frame> frames, ref Random r, Frame start_frame, Variable false_var, float max_time, bool main_thread)
        {
            DateTime start_time = DateTime.Now;
            //return false;

            //Go until it is complete
            while (!Complete(disjunctions))
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
                                    if (main_thread)
                                    {
                                        ready = state.UNSAT;
                                        finalModel = M;
                                    }
                                    return false;
                                }
                            }
                        }
                    }
                    else //Decide
                    {
                        if (!Decide(ref disjunctions, ref var_disjunctions, ref M, ref frames, ref r, start_frame, false_var, max_time))
                        {
                            if (!Fail_Or_Backtrack(ref disjunctions, ref var_disjunctions, ref M, ref frames))
                            {
                                if(main_thread)
                                {
                                    ready = state.UNSAT;
                                    finalModel = M;
                                }
                                return false;
                            }
                        }
                    }
                }
                else //Restart, we timed out
                {
                    max_time *= 3;
                    disjunctions = start_frame.disjunctions;
                    var_disjunctions = start_frame.var_disjunctions;
                    M = start_frame.M;

                    start_frame = new Frame(disjunctions, M, false_var);
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
        private static bool Decide(ref HashSet<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M,
                                   ref Stack<Frame> frames, ref Random r, Frame start_frame, Variable false_var, float max_time)
        {
            //foreach(KeyValuePair<int, HashSet<Disjunction>> kp in var_disjunctions)
            foreach(Disjunction d in disjunctions)
            {
                if (d.Count == 0)
                    continue;

                Variable v = d[r.Next() % d.Count];

                if(_ThreadsPerProgram > 0)
                    return ConcurrentDecide(ref disjunctions, ref var_disjunctions, ref M, ref frames, ref r, start_frame, v, false_var, max_time);

                //If we cannot add a variable or its negation, then we need to return false
                frames.Push(new Frame(disjunctions, M, v));
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

            //If we don't return at this point, then all of the disjunctions have been taken care of
            //and we need to return SATISFIABLE back to the user
            return true;
        }

        private static bool ConcurrentDecide(ref HashSet<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M,
                                             ref Stack<Frame> frames, ref Random r, Frame start_frame, Variable v, Variable false_var, float max_time)
        {
            lock(_LockObj)
            {
                _ThreadsPerProgram--;
            }

            Frame frame1 = new Frame(disjunctions, M, v);
            Frame frame2 = new Frame(disjunctions, M, !v);
            Frame new_start_frame = new Frame(start_frame.disjunctions, start_frame.M, start_frame.decision_variable);

            HashSet<Disjunction> d = frame1.disjunctions;
            Dictionary<int, HashSet<Disjunction>> vd = frame1.var_disjunctions;
            List<Variable> m = frame1.M;
            Stack<Frame> new_frames1 = new Stack<Frame>();
            Random rand = r;

            Thread t1 = new Thread(() => AddVarAndDecide(ref d, ref vd, ref m, ref new_frames1, ref rand, start_frame, v, false_var, max_time));
            _ExtraThreads.Add(t1);
            t1.Start();

            HashSet<Disjunction> d2 = frame2.disjunctions;
            Dictionary<int, HashSet<Disjunction>> vd2 = frame2.var_disjunctions;
            List<Variable> m2 = frame2.M;
            Stack<Frame> new_frames2 = new Stack<Frame>();
            Thread t2 = new Thread(()=> AddVarAndDecide(ref d2, ref vd2, ref m2, ref new_frames2, ref rand, new_start_frame, !v, false_var, max_time));
            _ExtraThreads.Add(t2);
            t2.Start();

            t1.Join();
            t2.Join();

            lock (_LockObj)
            {
                _ThreadsPerProgram++;
            }

            if (ready == state.UNSOLVED)
                return false;

            return true;
        }

        private static void AddVarAndDecide(ref HashSet<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M,
                                             ref Stack<Frame> frames, ref Random r, Frame start_frame, Variable v, Variable false_var, float max_time)
        {
            if (!AddVariable(ref disjunctions, ref var_disjunctions, ref M, v))
                return; //This couldn't be added

            Solve(ref disjunctions, ref var_disjunctions, ref M, ref frames, ref r, start_frame, false_var, max_time, false);
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
        private static bool Fail_Or_Backtrack(ref HashSet<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, 
                                              ref List<Variable> M, ref Stack<Frame> frames)
        {
            if (frames.Count == 0) return false;

            Frame f = frames.Pop();
            disjunctions = f.disjunctions;
            var_disjunctions = f.var_disjunctions;
            M = f.M;
            
            //BE CAREFUL WITH THIS RECURSION. SHOULD CONSIDER DOING IT IN A LOOP
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
        private static Variable Propagate(ref HashSet<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M)
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
        private static bool AddVariable(ref HashSet<Disjunction> disjunctions, ref Dictionary<int, HashSet<Disjunction>> var_disjunctions, ref List<Variable> M, Variable v)
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
        private static bool Complete(HashSet<Disjunction> disjunctions)
        {
            //bool empty = true;
            if (disjunctions.Count == 0)
                return true;

            return false;
        }
    }
}
