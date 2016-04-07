import javax.swing.text.SimpleAttributeSet;
import java.io.BufferedReader;
import java.io.File;
import java.io.InputStreamReader;

public class PPSAT_Tester
{
    private static String UNSATISFIABLE = "UNSATISFIABLE";
    private static String SATISFIABLE = "SATISFIABLE";
    private static String MINISAT = "minisat.exe";
    private static String PPSAT_EXE = "PPSAT.exe";
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

        ProcessBuilder p_minisat = new ProcessBuilder(minisat.getAbsolutePath(), "n-queens.txt");
        ProcessBuilder p_PPSAT = new ProcessBuilder(PPSAT.getAbsolutePath(), "n-queens.txt");

        Process proc_minisat = p_minisat.start();
        proc_minisat.waitFor();

        BufferedReader br = new BufferedReader(new InputStreamReader(proc_minisat.getInputStream()));

        Println("Starting minisat");
        String s;
        boolean satisfiable = false;
        while((s = br.readLine()) != null)
        {
            if(s.contains(SATISFIABLE))
                satisfiable = true;
            if(s.contains(UNSATISFIABLE))
                satisfiable = false;

            Println(s);
        }

        Process proc_PPSAT = p_PPSAT.start();
        proc_PPSAT.waitFor();
        br = new BufferedReader(new InputStreamReader((proc_PPSAT.getInputStream())));
        Println("Starting PPSAT");
        boolean ppsat_satisfiable = false;
        while((s = br.readLine()) != null)
        {
            if(s.contains(SATISFIABLE))
                ppsat_satisfiable = true;
            if(s.contains(UNSATISFIABLE))
                ppsat_satisfiable = false;

            Println(s);
        }

        if(ppsat_satisfiable != satisfiable)
            Println("PPSAT returns " + (ppsat_satisfiable ? SATISFIABLE : UNSATISFIABLE) + " instead of " + (satisfiable ? SATISFIABLE : UNSATISFIABLE));
        else
            Println("PPSAT and MiniSat agree");
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
}
