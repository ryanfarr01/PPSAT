import java.io.BufferedReader;
import java.io.File;
import java.io.InputStreamReader;
import java.io.PrintWriter;

/**
 * Author: Ryan Farr
 * Date: May 05, 2016
 * Class: CS 5110/6110 - Rigorous System Design
 *
 * File: PPSAT_Tester: a differential tester to determine the correctness of PPSAT
 *  by comparing it to minisat. This automatically generates tests. The tests are stored
 *  in a file if it is not passed, or simply deleted otherwise.
 *
 *  Note: this assumes that both minisat.exe and PPSAT.exe are in your PATH environment variable
 **/

public class PPSAT_Tester
{
    private static String UNSATISFIABLE = "UNSATISFIABLE"; //The string indicating that the expression was unsatisfiable
    private static String SATISFIABLE = "SATISFIABLE";     //The string indicating that the expression was satisfiable
    private static String MINISAT = "minisat.exe";         //The minisat executable name
    private static String PPSAT_EXE = "PPSAT.exe";         //The PPSAT executable name
    private static int TEST_NUMBER = 0;                    //The starting test number to be used for test names
    private static int NUM_SAT = 0;                        //Number of satisfiable tests created
    private static int NUM_UNSAT = 0;                      //Number of unsatisfiable tests created
    private static boolean VERBOSE = true;                 //If verbose, prints extra debugging strings

    /*
    * Function main - Begins randomly generating tests, running them through minisat, then running them
    *   through PPSAT. This is random differential testing, and assumes that minisat is always correct
    *
    *   Takes in one optional argument which is -v or -V, which indicates to run in verbose mode. Otherwise
    *   it is assumed that it should not.
    */
    public static void main(String[] args) throws Exception
    {
        //Grab teh verbosity
        if(args.length == 1)
        {
            if(args[0].equals("-v") || args[0].equals(("-V")))
                VERBOSE = true;
        }

        //Grab minisat
        File minisat = GetExecutable(MINISAT);
        if(minisat == null)
        {
            Println("Unable to get Minisat");
            return;
        }
        Println("Found Minisat");

        //Grab PPSAT
        File PPSAT = GetExecutable(PPSAT_EXE);
        if(PPSAT == null)
        {
            Println("Unable to get PPSAT.exe");
            return;
        }
        Println("Found PPSAT.exe");

        //Begin testing
        int wrong = 0;
        for(int i = 0; i < 10000; i++) //Run 10,000 tests for each run
        {
            //Create the random test
            boolean lines = Math.random() < 0.5;
            int max_vars = 3 + (int)(Math.random() * 5);
            String test_file = CreateTest(lines, max_vars);

            //Test on Minisat and then PPSAT
            Println("Testing minisat");
            boolean sat_minisat = TestExecutable(minisat, test_file);
            Println("Testing PPSAT");
            boolean sat_ppsat = TestExecutable(PPSAT, test_file, 4, 0);
            Println("Finished");

            //If don't agree, keep the file and inform the user
            if (sat_minisat != sat_ppsat)
            {
                wrong++;
                Println("PPSAT test: " + test_file + " returns " + (sat_ppsat ? SATISFIABLE : UNSATISFIABLE) + " instead of " + (sat_minisat ? SATISFIABLE : UNSATISFIABLE));
            }
            else //If they do agree, then all is well and we can delete the test
            {
                new File(test_file).delete();
                Println("PPSAT and MiniSat agree on: " + test_file);
            }

            //Set the SAT and UNSAT variables
            if(sat_minisat)
                NUM_SAT++;
            else
                NUM_UNSAT++;

            //Print statistics
            Println("Testing complete. Statistics: ");
            Println("    Number incorrect: " + wrong);
            Println("    Tests that were Satisfiable: " + NUM_SAT);
            Println("    Tests that were Unatisfiable: " + NUM_UNSAT);

            System.gc();
            Println("");
        }
    }

    /*
    * Function testExecutable - Runs the given executable on the test file
    *   and parses through the output to determine if it's satisfiable or
    *   unsatisfiable
    */
    private static boolean TestExecutable(File file, String test_file) throws Exception
    {
        //Create the process
        ProcessBuilder pb = new ProcessBuilder(file.getAbsolutePath(), test_file);

        //Start the process
        Process process = pb.start();
        process.waitFor();

        //Create BR for reading output
        BufferedReader br = new BufferedReader(new InputStreamReader(process.getInputStream()));

        //Search through all lines of the output to determine satisfiability
        String s;
        boolean satisfiable = false;
        while((s = br.readLine()) != null)
        {
            if(s.contains(SATISFIABLE))
                satisfiable = true;
            if(s.contains(UNSATISFIABLE))
                satisfiable = false;
        }

        br.close();

        return satisfiable;
    }

    /*
    * Function TestExecutable - Runs PPSAT on a given test with num_threads
    *   number of threads used for Decision. Returns once it tells if it's
    *   SAT or UNSAT
    */
    private static boolean TestExecutable(File file, String test_file, int num_solve_threads, int num_decision_threads) throws Exception
    {
        //Create the process
        ProcessBuilder pb = new ProcessBuilder(file.getAbsolutePath(), test_file, " -t ", Integer.toString(num_solve_threads), " -dt ", Integer.toString(num_decision_threads));

        //Start PPSAT
        Process process = pb.start();
        process.waitFor();

        //Create BR to read output
        BufferedReader br = new BufferedReader(new InputStreamReader(process.getInputStream()));

        //Read output line by line to find if it's SAT or UNSAT
        String s;
        boolean satisfiable = false;
        while((s = br.readLine()) != null)
        {
            if(s.contains(SATISFIABLE))
                satisfiable = true;
            if(s.contains(UNSATISFIABLE))
                satisfiable = false;
        }

        br.close();

        return satisfiable;
    }

    /*
    *  Function CreateTest - creates a test CNF file and returns the string filename and path
    */
    private static String CreateTest(boolean set_number_vars, int max_vars_per_line) throws Exception
    {
        //Open up the file
        String filename = "test-" + TEST_NUMBER++ + ".cnf";
        PrintWriter pw = new PrintWriter(filename, "UTF-8");

        //The number of total lines and total variables
        int lines = 350;
        int vars = 81;

        //Create the first line of the .CNF file
        pw.println("p cnf " + vars + " " + lines);

        //Go through every line and create a disjunction and store it
        for(int i = 0; i < lines; i++)
        {
            //Can have the disjunctions have fixed or random lengths
            int num_vars = set_number_vars ? max_vars_per_line : 1 + (int)(Math.random()*max_vars_per_line);
            for(int j = 0; j < num_vars; j++)
            {
                int x = RandomVariable(vars);
                pw.print(x + " ");
            }

            //0 indicates the end of the line
            pw.print(0);
            pw.println();
        }

        pw.close();
        return filename;
    }

    /*
    * Function RandomVariable - creates a random variable between -max_vars
    *   and max_vars and returns it
    */
    private static int RandomVariable(int max_vars)
    {
        int x = (int)(Math.random() * max_vars) + 1;
        if(Math.random() < 0.5)
            x *= -1;

        return x;
    }

    /*
    * Function GetExecutable - Grabs an executable with the given
    *   filename. It's assumed that this filename can be found in the
    *   PATH environment variables
    */
    private static File GetExecutable(String executable)
    {
        String path = System.getenv("PATH");
        String[] paths = path.split(";");

        for(String s : paths)
        {
            File f = new File(s, executable);
            if(f.exists()) { return f; }
        }

        return null;
    }

    /*
    * Function Println - prints the given string if in verbose mode
    */
    private static void Println(String p)
    {
        if(VERBOSE) { System.out.println(p); }
    }
}
