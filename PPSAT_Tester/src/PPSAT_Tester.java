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

        for(int i = 0; i < 100; i++)
        {
            String test_file = CreateTest();

            Println("Testing minisat");
            boolean sat_minisat = TestExecutable(minisat, test_file);
            Println("Testing PPSAT");
            boolean sat_ppsat = TestExecutable(PPSAT, test_file);
            Println("Finished");

            if (sat_minisat != sat_ppsat)
                Println("PPSAT returns " + (sat_ppsat ? SATISFIABLE : UNSATISFIABLE) + " instead of " + (sat_minisat ? SATISFIABLE : UNSATISFIABLE));
            else
                Println("PPSAT and MiniSat agree on: " + test_file);

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
    private static String CreateTest() throws Exception
    {
        String filename = "test-" + TEST_NUMBER++ + ".cnf";
        PrintWriter pw = new PrintWriter(filename, "UTF-8");

        int lines = 5;
        int vars = 5;

        pw.println("p cnf " + vars + " " + lines);
        for(int i = 0; i < lines; i++)
        {
            int x = RandomVariable(vars);
            int y = RandomVariable(vars);

            pw.println(x + " " + y + " " + 0);
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
