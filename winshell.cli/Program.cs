using System;
using System.Threading.Tasks;

namespace WinShell.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                // Set console title
                Console.Title = "WinShell CLI";
                
                var cli = new CliInterface();
                await cli.RunAsync();
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                return 1;
            }
        }
    }
}