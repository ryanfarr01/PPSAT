import java.io.BufferedReader;
import java.io.File;
import java.io.InputStreamReader;
import java.io.PrintWriter;

public class PPSAT_Tester
{
    private static String UNSATISFIABLE = "UNSATISFIABLE";
    private static String SATISFIABLE = "SATISFIABLE";
    private static String MINISAT = "minisat.exe";
    private static String PPSAT_EXE = "PPSAT.exe";
    private static int TEST_NUMBER = 0;
    private static int NUM_SAT = 0;
    private static int NUM_UNSAT = 0;
    private static boolean VERBOSE = true;

    public static void main(String[] args) throws Exception
    {
        if(args.length == 1)
        {
            if(args[0].equals("-v") || args[0].equals(("-V")))
                VERBOSE = true;
        }

        File minisat = GetExecutable(MINISAT);
        if(minisat == null)
        {
            Println("Unable to get Minisat");
            return;
        }
        Println("Found Minisat");

        File PPSAT = GetExecutable(PPSAT_EXE);
        if(PPSAT == null)
        {
            Println("Unable to get PPSAT.exe");
            return;
        }
        Println("Found PPSAT.exe");

        int wrong = 0;
        for(int i = 0; i < 10000; i++)
        {
            boolean lines = Math.random() < 0.5;
            int max_vars = 3 + (int)(Math.random() * 5);
            String test_file = CreateTest(lines, max_vars);

            Println("Testing minisat");
            boolean sat_minisat = TestExecutable(minisat, test_file);
            Println("Testing PPSAT");
            boolean sat_ppsat = TestExecutable(PPSAT, test_file);
            Println("Finished");

            if (sat_minisat != sat_ppsat)
            {
                wrong++;
                Println("PPSAT test: " + test_file + " returns " + (sat_ppsat ? SATISFIABLE : UNSATISFIABLE) + " instead of " + (sat_minisat ? SATISFIABLE : UNSATISFIABLE));
            }
            else
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

            Println("");
        }
    }

    private static boolean TestExecutable(File file, String test_file) throws Exception
    {
        ProcessBuilder pb = new ProcessBuilder(file.getAbsolutePath(), test_file);

        Process process = pb.start();
        process.waitFor();

        BufferedReader br = new BufferedReader(new InputStreamReader(process.getInputStream()));

        String s;
        boolean satisfiable = false;
        while((s = br.readLine()) != null)
        {
            if(s.contains(SATISFIABLE))
                satisfiable = true;
            if(s.contains(UNSATISFIABLE))
                satisfiable = false;

//            Println(s);
        }

        br.close();

        return satisfiable;
    }

    /*
    *  Function CreateTest - creates a test CNF file and returns the string filename and path
    */
    private static String CreateTest(boolean set_number_vars, int max_vars_per_line) throws Exception
    {
        String filename = "test-" + TEST_NUMBER++ + ".cnf";
        PrintWriter pw = new PrintWriter(filename, "UTF-8");

        int lines = 350;
        int vars = 81;

        pw.println("p cnf " + vars + " " + lines);
        for(int i = 0; i < lines; i++)
        {
            int num_vars = set_number_vars ? max_vars_per_line : 1 + (int)(Math.random()*max_vars_per_line);
            for(int j = 0; j < num_vars; j++)
            {
                int x = RandomVariable(vars);
                pw.print(x + " ");
            }

            pw.print(0);
            pw.println();
        }

        pw.close();
        return filename;
    }

    private static int RandomVariable(int max_vars)
    {
        int x = (int)(Math.random() * max_vars) + 1;
        if(Math.random() < 0.5)
            x *= -1;

        return x;
    }

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

    private static void Println(String p)
    {
        if(VERBOSE) { System.out.println(p); }
    }

    private static float ToSeconds(long time)
    {
        return (float)(time);
    }
}
