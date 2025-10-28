using System;
using System.Threading;
using System.Threading.Tasks;
using WinShell.Core;

namespace WinShell.CLI
{
    public class CliInterface
    {
        private readonly CommandEngine _engine;
        private bool _running;
        private CancellationTokenSource _currentCommandCts;

        public CliInterface()
        {
            _engine = new CommandEngine();
            _running = true;
            
            // Handle application shutdown events
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Console.CancelKeyPress += OnCancelKeyPress;
        }

        public async Task RunAsync()
        {
            PrintBanner();
            
            while (_running)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(_engine.Environment.GetPrompt());
                    Console.ResetColor();
                    
                    var input = Console.ReadLine();
                    
                    if (string.IsNullOrWhiteSpace(input))
                        continue;
                    
                    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
                        input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    {
                        _running = false;
                        break;
                    }
                    
                    _currentCommandCts = new CancellationTokenSource();
                    
                    var result = await _engine.ExecuteCommandAsync(input, _currentCommandCts.Token);
                    
                    // Check for special clear screen command
                    if (result.Output == "[CLEAR_SCREEN]")
                    {
                        Console.Clear();
                        PrintBanner(); // Reprint banner after clearing
                    }
                    // Check for ASCII art with image marker (CLI only displays ASCII text)
                    else if (!string.IsNullOrEmpty(result.Output) && result.Output.StartsWith("[ASCII_ART_IMAGE:"))
                    {
                        // Extract and display only the ASCII text, ignore image path
                        int endIndex = result.Output.IndexOf("]");
                        if (endIndex > 0)
                        {
                            string asciiText = result.Output.Substring(endIndex + 2); // Skip ]\n
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine(asciiText);
                            Console.ResetColor();
                        }
                    }
                    else if (!string.IsNullOrEmpty(result.Output))
                    {
                        Console.WriteLine(result.Output);
                    }
                    
                    if (!string.IsNullOrEmpty(result.Error))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine(result.Error);
                        Console.ResetColor();
                    }
                    
                    if (!result.Success && result.ExitCode != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Command exited with code: {result.ExitCode}");
                        Console.ResetColor();
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
                finally
                {
                    _currentCommandCts?.Dispose();
                    _currentCommandCts = null;
                }
            }
            
            Console.WriteLine("\nGoodbye!");
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (_currentCommandCts != null && !_currentCommandCts.IsCancellationRequested)
            {
                e.Cancel = true;
                _currentCommandCts.Cancel();
                Console.WriteLine("\n^C - Command cancelled");
            }
            else
            {
                _running = false;
                e.Cancel = true; // Prevent immediate termination, allow graceful shutdown
                Shutdown();
            }
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void Shutdown()
        {
            _running = false;
            _currentCommandCts?.Cancel();
            _currentCommandCts?.Dispose();
            
            // Cleanup any resources
            try
            {
                _engine?.Dispose();
            }
            catch { /* Ignore disposal errors */ }
        }

        private void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("                          WinShell CLI");
            Console.WriteLine();
            Console.WriteLine("  ================================================================");
            Console.WriteLine("                          WINSHELL");
            Console.WriteLine("  ================================================================");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("                WinShell v1.0.0 - CLI Terminal");
            Console.WriteLine("                Running in standalone console mode");
            Console.WriteLine("        Type 'help' for available commands, 'exit' or 'quit' to close");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("        TIP: Use Ctrl + Mouse Wheel or Ctrl +/- to zoom in/out");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ================================================================\n");
            Console.ResetColor();
        }
    }
}