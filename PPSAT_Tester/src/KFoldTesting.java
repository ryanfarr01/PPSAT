import java.io.BufferedReader;
import java.io.File;
import java.io.InputStreamReader;

/**
 * Author: Ryan Farr
 * Date: May 05, 2016
 * Class: CS 5110/6110 - Rigorous System Design
 *
 * File: KFoldTesting.java: A file to test running times of PPSAT.exe
 *
 *  Note: this assumes that both minisat.exe and PPSAT.exe are in your PATH environment variable
 **/

public class KFoldTesting
{
    private static String UNSATISFIABLE = "UNSATISFIABLE";
    private static String SATISFIABLE = "SATISFIABLE";
    private static String PPSAT_EXE = "PPSAT.exe";
    private static int NUM_FILES = 70;

    /*
    * Function main - runs the timing tests and outputs the results
    *       to std out
    */
    public static void main(String[] args) throws Exception
    {
        //Grab PPSAT.exe
        File PPSAT = GetExecutable(PPSAT_EXE);
        if(PPSAT == null)
        {
            System.out.println("Unable to get PPSAT.exe");
            return;
        }
        System.out.println("Found PPSAT.exe");

        //Test threads 0 through 30 on NUM_FILES tests each and gather the time
        for(int threads = 0; threads <= 30; threads++)
        {
            System.out.println("Testing number of threads: " + threads);
            long start_time = System.nanoTime();
            for (int i = 0; i < NUM_FILES; i++)
            {
                String test_file = "Tests/test-" + i + ".cnf";

                TestExecutable(PPSAT, test_file, 1, threads);
                System.gc();
            }
            long end_time = System.nanoTime();

            //calculate average time and print it out
            System.out.println("Finished. Time: " + ((end_time - start_time)/NUM_FILES));
            System.out.println();
        }

        System.out.println("Finished");
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
}
