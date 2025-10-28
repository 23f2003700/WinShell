using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WinShell.CLI
{
    class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();
        
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        private const int SW_MAXIMIZE = 3;
        private const int SW_NORMAL = 1;

        static async Task Main(string[] args)
        {
            // Allocate a new console for this application
            if (AllocConsole())
            {
                // Set console title
                Console.Title = "WinShell CLI";
                
                // Get console window handle and show it normally
                IntPtr consoleHandle = GetConsoleWindow();
                if (consoleHandle != IntPtr.Zero)
                {
                    ShowWindow(consoleHandle, SW_NORMAL);
                }
                
                // Redirect console streams
                Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                Console.SetError(new System.IO.StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
                Console.SetIn(new System.IO.StreamReader(Console.OpenStandardInput()));
                
                var cli = new CliInterface();
                await cli.RunAsync();
                
                // Keep console open until user closes it
                Console.WriteLine("\nPress any key to close...");
                Console.ReadKey();
                
                // Free the console when done
                FreeConsole();
            }
            else
            {
                // Fallback if console allocation fails
                System.Windows.Forms.MessageBox.Show("Failed to create console window!", "WinShell CLI Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}