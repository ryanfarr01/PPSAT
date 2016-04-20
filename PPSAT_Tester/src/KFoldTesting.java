/**
 * Created by Ryn Farr on 4/19/2016.
 */
import java.io.BufferedReader;
import java.io.File;
import java.io.InputStreamReader;

public class KFoldTesting
{
    private static String UNSATISFIABLE = "UNSATISFIABLE";
    private static String SATISFIABLE = "SATISFIABLE";
    private static String PPSAT_EXE = "PPSAT.exe";

    public static void main(String[] args) throws Exception
    {
        File PPSAT = GetExecutable(PPSAT_EXE);
        if(PPSAT == null)
        {
            System.out.println("Unable to get PPSAT.exe");
            return;
        }
        System.out.println("Found PPSAT.exe");

        for(int threads = 1; threads <= 30; threads++)
        {
            System.out.println("Testing number of threads: " + threads);
            long start_time = System.nanoTime();
            for (int i = 0; i < 70; i++)
            {
                String test_file = "Tests/test-" + i + ".cnf";

                TestExecutable(PPSAT, test_file, threads);
                System.gc();
            }
            long end_time = System.nanoTime();

            System.out.println("Finished. Time: " + ((end_time - start_time)/70));
            System.out.println();
        }

        System.out.println("Finished");
    }

    private static boolean TestExecutable(File file, String test_file, int num_threads) throws Exception
    {
        ProcessBuilder pb = new ProcessBuilder(file.getAbsolutePath(), test_file, Integer.toString(num_threads));

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
        }

        br.close();

        return satisfiable;
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
}
