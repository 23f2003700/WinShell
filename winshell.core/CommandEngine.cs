using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinShell.Common;

namespace WinShell.Core
{
    public class CommandEngine : IDisposable
    {
        private readonly ShellEnvironment _environment;
        private readonly ProcessManager _processManager;
        private readonly CommandParser _parser;
        private readonly Dictionary<string, Func<string[], Task<CommandResult>>> _builtInCommands;

        public event EventHandler<string> OutputReceived;
        public event EventHandler<string> ErrorReceived;

        public ShellEnvironment Environment => _environment;

        public CommandEngine()
        {
            _environment = new ShellEnvironment();
            _processManager = new ProcessManager(_environment);
            _parser = new CommandParser(_environment);
            _builtInCommands = new Dictionary<string, Func<string[], Task<CommandResult>>>(StringComparer.OrdinalIgnoreCase);
            
            RegisterBuiltInCommands();
            
            _processManager.OutputReceived += (s, e) => OutputReceived?.Invoke(s, e);
            _processManager.ErrorReceived += (s, e) => ErrorReceived?.Invoke(s, e);
        }

        private void RegisterBuiltInCommands()
        {
            _builtInCommands["cd"] = ChangeDirCommand;
            _builtInCommands["chdir"] = ChangeDirCommand;
            _builtInCommands["dir"] = ListDirectoryCommand;
            _builtInCommands["ls"] = ListDirectoryCommand;
            _builtInCommands["pwd"] = PrintWorkingDirectoryCommand;
            _builtInCommands["echo"] = EchoCommand;
            _builtInCommands["set"] = SetVariableCommand;
            _builtInCommands["env"] = EnvironmentCommand;
            _builtInCommands["cls"] = ClearCommand;
            _builtInCommands["clear"] = ClearCommand;
            _builtInCommands["exit"] = ExitCommand;
            _builtInCommands["help"] = HelpCommand;
            _builtInCommands["history"] = HistoryCommand;
            _builtInCommands["pushd"] = PushDirectoryCommand;
            _builtInCommands["popd"] = PopDirectoryCommand;
            _builtInCommands["mkdir"] = MakeDirectoryCommand;
            _builtInCommands["rmdir"] = RemoveDirectoryCommand;
            _builtInCommands["del"] = DeleteCommand;
            _builtInCommands["rm"] = DeleteCommand;
            _builtInCommands["copy"] = CopyCommand;
            _builtInCommands["cp"] = CopyCommand;
            _builtInCommands["move"] = MoveCommand;
            _builtInCommands["mv"] = MoveCommand;
            _builtInCommands["prompt"] = PromptCommand;
            _builtInCommands["type"] = TypeCommand;
            _builtInCommands["cat"] = TypeCommand;
            _builtInCommands["jobs"] = JobsCommand;
            _builtInCommands["fg"] = ForegroundCommand;
            _builtInCommands["bg"] = BackgroundCommand;
            _builtInCommands["kill"] = KillCommand;
            
            // Custom ASCII Art Commands
            _builtInCommands["ag"] = args => ShowAsciiArtCommand("AG.txt", "Ag.jpg", args);
            _builtInCommands["aloksir"] = args => ShowAsciiArtCommand("AlokSir.txt", "alok sir.png", args);
            _builtInCommands["monikamam"] = args => ShowAsciiArtCommand("monika mam.txt", "monika mam.jpg", args);
            _builtInCommands["simranmam"] = args => ShowAsciiArtCommand("simran mam.txt", "simran mam.jpg", args);
            _builtInCommands["ss"] = args => ShowAsciiArtCommand("SS.txt", "SS.png", args);
            _builtInCommands["abhishekgour"] = args => ShowAsciiArtCommand("abhishek gour.txt", "abhishek gour.jpg", args);
            _builtInCommands["ncb"] = args => ShowAsciiArtCommand("NCB.md", "NCB.jpg", args);
            _builtInCommands["logo"] = args => ShowLogoCommand(args);
        }

        public async Task<CommandResult> ExecuteCommandAsync(string input, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new CommandResult { Success = true };

            _environment.AddToHistory(input);
            
            // Parse the command to detect pipes, redirection, and background jobs
            // NO POWERSHELL - we handle everything natively!
            
            CommandResult result;
            
            // Check for piping (highest priority - spans multiple commands)
            if (input.Contains(" | "))
            {
                result = await HandlePipelineAsync(input, cancellationToken);
            }
            // Check for background execution
            else if (input.TrimEnd().EndsWith(" &"))
            {
                result = await HandleBackgroundAsync(input.TrimEnd().TrimEnd('&').Trim(), cancellationToken);
            }
            // Check for redirection
            else if (input.Contains(" > ") || input.Contains(" >> ") || input.Contains(" < "))
            {
                result = await HandleRedirectionAsync(input, cancellationToken);
            }
            else
            {
                // Simple command execution
                var parsed = _parser.Parse(input);
                if (parsed == null)
                    return new CommandResult { Success = false, Error = "Failed to parse command" };

                // Check for built-in commands
                if (_builtInCommands.TryGetValue(parsed.Command, out var builtInCommand))
                {
                    result = await builtInCommand(parsed.Arguments);
                }
                else
                {
                    // Execute as external command - NO WRAPPER
                    result = await _processManager.ExecuteAsync(parsed.Command, parsed.Arguments, cancellationToken);
                }
            }

            return result;
        }

        private async Task<CommandResult> HandlePipelineAsync(string input, CancellationToken cancellationToken)
        {
            // Split by pipe operator
            var commands = input.Split(new[] { " | " }, StringSplitOptions.None);
            var pipeline = new List<(string command, string[] args, bool isBuiltIn)>();

            foreach (var cmd in commands)
            {
                var parsed = _parser.Parse(cmd.Trim());
                if (parsed != null)
                {
                    // Check if it's a built-in command
                    bool isBuiltIn = _builtInCommands.ContainsKey(parsed.Command);
                    pipeline.Add((parsed.Command, parsed.Arguments, isBuiltIn));
                }
            }

            if (pipeline.Count < 2)
            {
                return new CommandResult
                {
                    Success = false,
                    Error = "Invalid pipeline - at least two commands required"
                };
            }

            // Check if first command is built-in
            if (pipeline[0].isBuiltIn)
            {
                // Execute built-in command first, then pipe to external commands
                return await ExecuteBuiltInPipelineAsync(pipeline, cancellationToken);
            }
            else
            {
                // All external commands - use ProcessManager
                var externalPipeline = pipeline.Select(p => (p.command, p.args)).ToList();
                return await _processManager.ExecutePipelineAsync(externalPipeline, cancellationToken);
            }
        }

        private async Task<CommandResult> ExecuteBuiltInPipelineAsync(List<(string command, string[] args, bool isBuiltIn)> pipeline, CancellationToken cancellationToken)
        {
            // Execute the first command (built-in) to get its output
            var firstCmd = pipeline[0];
            var builtInResult = await _builtInCommands[firstCmd.command](firstCmd.args);

            if (!builtInResult.Success || pipeline.Count == 1)
            {
                return builtInResult;
            }

            // Now pipe the output to the remaining commands
            var remainingPipeline = pipeline.Skip(1).Select(p => (p.command, p.args)).ToList();
            
            // Feed the built-in output as input to the external command pipeline
            return await _processManager.ExecutePipelineWithInputAsync(
                remainingPipeline, 
                builtInResult.Output, 
                cancellationToken);
        }

        private async Task<CommandResult> HandleBackgroundAsync(string input, CancellationToken cancellationToken)
        {
            var parsed = _parser.Parse(input);
            if (parsed == null)
                return new CommandResult { Success = false, Error = "Failed to parse command" };

            // Execute in background - returns immediately
            return await _processManager.ExecuteBackgroundAsync(parsed.Command, parsed.Arguments);
        }

        private async Task<CommandResult> HandleRedirectionAsync(string input, CancellationToken cancellationToken)
        {
            string command;
            string outputFile = null;
            string inputFile = null;
            bool append = false;

            // Parse input redirection (<)
            if (input.Contains(" < "))
            {
                var parts = input.Split(new[] { " < " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    command = parts[0].Trim();
                    inputFile = parts[1].Trim();
                }
                else
                {
                    return new CommandResult { Success = false, Error = "Invalid input redirection syntax" };
                }
            }
            // Parse output redirection (> or >>)
            else if (input.Contains(" >> "))
            {
                var parts = input.Split(new[] { " >> " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    command = parts[0].Trim();
                    outputFile = parts[1].Trim();
                    append = true;
                }
                else
                {
                    return new CommandResult { Success = false, Error = "Invalid output redirection syntax" };
                }
            }
            else if (input.Contains(" > "))
            {
                var parts = input.Split(new[] { " > " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    command = parts[0].Trim();
                    outputFile = parts[1].Trim();
                    append = false;
                }
                else
                {
                    return new CommandResult { Success = false, Error = "Invalid output redirection syntax" };
                }
            }
            else
            {
                return new CommandResult { Success = false, Error = "No redirection operator found" };
            }

            // Execute the command
            var parsed = _parser.Parse(command);
            if (parsed == null)
                return new CommandResult { Success = false, Error = "Failed to parse command" };

            CommandResult result;
            
            if (_builtInCommands.TryGetValue(parsed.Command, out var builtInCommand))
            {
                result = await builtInCommand(parsed.Arguments);
            }
            else
            {
                result = await _processManager.ExecuteAsync(parsed.Command, parsed.Arguments, cancellationToken);
            }

            // Handle file I/O
            if (result.Success)
            {
                try
                {
                    // Output redirection
                    if (outputFile != null)
                    {
                        if (append)
                            File.AppendAllText(outputFile, result.Output);
                        else
                            File.WriteAllText(outputFile, result.Output);
                        
                        // Silent on success
                        result.Output = string.Empty;
                    }

                    // Input redirection (for future implementation)
                    if (inputFile != null)
                    {
                        // Input redirection would need to pipe file content to process stdin
                        // This is more complex and can be implemented later
                    }
                }
                catch (Exception ex)
                {
                    result.Error = $"Redirection failed: {ex.Message}";
                    result.Success = false;
                }
            }

            return result;
        }

        #region Built-in Commands

        private async Task<CommandResult> ChangeDirCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length == 0)
            {
                result.Output = _environment.CurrentDirectory;
            }
            else
            {
                var path = string.Join(" ", args);
                if (_environment.ChangeDirectory(path))
                {
                    result.Output = $"Changed directory to: {_environment.CurrentDirectory}";
                }
                else
                {
                    result.Success = false;
                    result.Error = $"Cannot find path: {path}";
                    result.ExitCode = 1;
                }
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> ListDirectoryCommand(string[] args)
        {
            var result = new CommandResult();
            
            try
            {
                var path = args.Length > 0 ? string.Join(" ", args) : _environment.CurrentDirectory;
                var dir = new DirectoryInfo(path);
                
                var output = new System.Text.StringBuilder();
                output.AppendLine($"\n Directory of {dir.FullName}\n");
                output.AppendLine($"{"Mode",-7} {"LastWriteTime",-20} {"Length",10} {"Name"}");
                output.AppendLine(new string('-', 70));
                
                foreach (var d in dir.GetDirectories())
                {
                    output.AppendLine($"{"d-----",-7} {d.LastWriteTime,-20:yyyy-MM-dd HH:mm} {"<DIR>",10} {d.Name}");
                }
                
                foreach (var f in dir.GetFiles())
                {
                    output.AppendLine($"{"-a----",-7} {f.LastWriteTime,-20:yyyy-MM-dd HH:mm} {f.Length,10} {f.Name}");
                }
                
                result.Output = output.ToString();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.ExitCode = 1;
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> PrintWorkingDirectoryCommand(string[] args)
        {
            return await Task.FromResult(new CommandResult 
            { 
                Output = _environment.CurrentDirectory 
            });
        }

        private async Task<CommandResult> EchoCommand(string[] args)
        {
            return await Task.FromResult(new CommandResult 
            { 
                Output = string.Join(" ", args) 
            });
        }

        private async Task<CommandResult> SetVariableCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length == 0)
            {
                var output = new System.Text.StringBuilder();
                foreach (var kvp in _environment.Variables.OrderBy(k => k.Key))
                {
                    output.AppendLine($"{kvp.Key}={kvp.Value}");
                }
                result.Output = output.ToString();
            }
            else
            {
                var arg = string.Join(" ", args);
                var parts = arg.Split('=', 2);
                if (parts.Length == 2)
                {
                    _environment.SetVariable(parts[0].Trim(), parts[1].Trim());
                    result.Output = $"Set {parts[0]}={parts[1]}";
                }
                else
                {
                    result.Success = false;
                    result.Error = "Invalid syntax. Use: set VARIABLE=VALUE";
                    result.ExitCode = 1;
                }
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> EnvironmentCommand(string[] args)
        {
            var result = new CommandResult();
            var output = new System.Text.StringBuilder();
            
            output.AppendLine("\n=== WinShell Environment ===");
            output.AppendLine($"Current Directory: {_environment.CurrentDirectory}");
            output.AppendLine($"User: {_environment.UserName}");
            output.AppendLine($"Machine: {_environment.MachineName}");
            output.AppendLine($"Home: {_environment.HomeDirectory}");
            output.AppendLine("\n=== Variables ===");
            
            foreach (var kvp in _environment.Variables.OrderBy(k => k.Key))
            {
                output.AppendLine($"{kvp.Key}={kvp.Value}");
            }
            
            result.Output = output.ToString();
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> ClearCommand(string[] args)
        {
            // Use a special marker that TerminalControl will recognize
            return await Task.FromResult(new CommandResult 
            { 
                Output = "[CLEAR_SCREEN]" // Special command for terminal to clear
            });
        }

        private async Task<CommandResult> ExitCommand(string[] args)
        {
            System.Environment.Exit(0);
            return await Task.FromResult(new CommandResult());
        }

        private async Task<CommandResult> HelpCommand(string[] args)
        {
            var result = new CommandResult();
            var output = new System.Text.StringBuilder();
            
            output.AppendLine("\n=== WinShell Built-in Commands ===\n");
            output.AppendLine("File & Directory:");
            output.AppendLine("  cd/chdir [path]     - Change directory");
            output.AppendLine("  dir/ls [path]       - List directory contents");
            output.AppendLine("  pwd                 - Print working directory");
            output.AppendLine("  mkdir [path]        - Create directory");
            output.AppendLine("  rmdir [path]        - Remove directory");
            output.AppendLine("  del/rm [file]       - Delete file");
            output.AppendLine("  copy/cp [src] [dst] - Copy file");
            output.AppendLine("  move/mv [src] [dst] - Move file");
            output.AppendLine("  type/cat [file]     - Display file contents");
            output.AppendLine("\nEnvironment & Display:");
            output.AppendLine("  echo [text]         - Display text");
            output.AppendLine("  set [var=value]     - Set/display environment variables");
            output.AppendLine("  env                 - Display environment info");
            output.AppendLine("  cls/clear           - Clear screen");
            output.AppendLine("  prompt [template]   - Set prompt template (WS$G, $P$G, etc.)");
            output.AppendLine("\nNavigation & History:");
            output.AppendLine("  pushd [path]        - Push directory to stack");
            output.AppendLine("  popd                - Pop directory from stack");
            output.AppendLine("  history             - Show command history");
            output.AppendLine("\nProcess Management:");
            output.AppendLine("  jobs                - List background jobs");
            output.AppendLine("  fg [pid]            - Bring background job to foreground");
            output.AppendLine("  bg [pid]            - Send job to background (resume)");
            output.AppendLine("  kill [pid]          - Terminate process by PID");
            output.AppendLine("\nAdvanced Features:");
            output.AppendLine("  command | command   - Pipe output between commands");
            output.AppendLine("  command > file      - Redirect output to file (overwrite)");
            output.AppendLine("  command >> file     - Redirect output to file (append)");
            output.AppendLine("  command &           - Run command in background");
            output.AppendLine("\nCustom ASCII Art:");
            output.AppendLine("  ag                  - Display AG ASCII art");
            output.AppendLine("  aloksir             - Display Alok Sir ASCII art");
            output.AppendLine("  monikamam           - Display Monika Mam ASCII art");
            output.AppendLine("  simranmam           - Display Simran Mam ASCII art");
            output.AppendLine("  ss                  - Display SS ASCII art");
            output.AppendLine("  abhishekgour        - Display Abhishek Gour ASCII art");
            output.AppendLine("  ncb                 - Display NCB ASCII art");
            output.AppendLine("  logo                - Display WinShell logo (ok.png)");
            output.AppendLine("\nSystem:");
            output.AppendLine("  help                - Show this help");
            output.AppendLine("  exit                - Exit WinShell");
            
            result.Output = output.ToString();
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> HistoryCommand(string[] args)
        {
            var result = new CommandResult();
            var output = new System.Text.StringBuilder();
            
            var history = _environment.CommandHistory;
            for (int i = 0; i < history.Count; i++)
            {
                output.AppendLine($"{i + 1,4}: {history[i]}");
            }
            
            result.Output = output.ToString();
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> PushDirectoryCommand(string[] args)
        {
            var result = new CommandResult();
            var path = args.Length > 0 ? string.Join(" ", args) : _environment.HomeDirectory;
            
            _environment.PushDirectory(path);
            result.Output = _environment.CurrentDirectory;
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> PopDirectoryCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (_environment.PopDirectory())
            {
                result.Output = _environment.CurrentDirectory;
            }
            else
            {
                result.Success = false;
                result.Error = "Directory stack is empty";
                result.ExitCode = 1;
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> MakeDirectoryCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length == 0)
            {
                result.Success = false;
                result.Error = "Missing directory name";
                result.ExitCode = 1;
            }
            else
            {
                try
                {
                    var path = string.Join(" ", args);
                    Directory.CreateDirectory(path);
                    result.Output = $"Directory created: {path}";
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                    result.ExitCode = 1;
                }
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> RemoveDirectoryCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length == 0)
            {
                result.Success = false;
                result.Error = "Missing directory name";
                result.ExitCode = 1;
            }
            else
            {
                try
                {
                    var path = string.Join(" ", args);
                    Directory.Delete(path, recursive: true);
                    result.Output = $"Directory removed: {path}";
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                    result.ExitCode = 1;
                }
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> DeleteCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length == 0)
            {
                result.Success = false;
                result.Error = "Missing file name";
                result.ExitCode = 1;
            }
            else
            {
                try
                {
                    foreach (var file in args)
                    {
                        File.Delete(file);
                    }
                    result.Output = $"Deleted {args.Length} file(s)";
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                    result.ExitCode = 1;
                }
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> CopyCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length < 2)
            {
                result.Success = false;
                result.Error = "Usage: copy SOURCE DESTINATION";
                result.ExitCode = 1;
            }
            else
            {
                try
                {
                    File.Copy(args[0], args[1], overwrite: true);
                    result.Output = $"Copied {args[0]} to {args[1]}";
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                    result.ExitCode = 1;
                }
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> MoveCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length < 2)
            {
                result.Success = false;
                result.Error = "Usage: move SOURCE DESTINATION";
                result.ExitCode = 1;
            }
            else
            {
                try
                {
                    File.Move(args[0], args[1]);
                    result.Output = $"Moved {args[0]} to {args[1]}";
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                    result.ExitCode = 1;
                }
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> TypeCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length == 0)
            {
                result.Success = false;
                result.Error = "Missing file name";
                result.ExitCode = 1;
            }
            else
            {
                try
                {
                    var path = string.Join(" ", args);
                    result.Output = File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                    result.ExitCode = 1;
                }
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> PromptCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length == 0)
            {
                // Show current prompt template
                var currentPrompt = _environment.GetVariable("PROMPT") ?? "WS$G";
                result.Output = $"Current prompt template: {currentPrompt}\n\n" +
                               "Available prompt variables:\n" +
                               "  $P - Current directory path\n" +
                               "  $G - Greater than symbol (>)\n" +
                               "  $D - Current date\n" +
                               "  $T - Current time\n" +
                               "  $U - Username\n" +
                               "  $M - Machine name\n" +
                               "  WS - WinShell brand (shows as 'WS [folder]>')\n\n" +
                               "Examples:\n" +
                               "  prompt WS$G        - WS [folder]> (default)\n" +
                               "  prompt $P$G        - Full path>\n" +
                               "  prompt $U@$M$G     - user@machine>\n" +
                               "  prompt [$T] $G     - [time] >";
            }
            else
            {
                var newPrompt = string.Join(" ", args);
                _environment.SetVariable("PROMPT", newPrompt);
                result.Output = $"Prompt updated to: {newPrompt}";
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> JobsCommand(string[] args)
        {
            var result = new CommandResult();
            var output = new System.Text.StringBuilder();
            
            var backgroundJobs = _processManager.GetBackgroundProcesses().ToList();
            
            if (backgroundJobs.Count == 0)
            {
                result.Output = "No background jobs running.";
            }
            else
            {
                output.AppendLine("\n=== Background Jobs ===\n");
                output.AppendLine($"{"PID",-10} {"Process Name",-30} {"Status"}");
                output.AppendLine(new string('-', 60));
                
                foreach (var process in backgroundJobs)
                {
                    try
                    {
                        var status = process.HasExited ? "Completed" : "Running";
                        output.AppendLine($"{process.Id,-10} {process.ProcessName,-30} {status}");
                    }
                    catch
                    {
                        output.AppendLine($"{process.Id,-10} {"<unknown>",-30} {"Error"}");
                    }
                }
                
                result.Output = output.ToString();
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> ForegroundCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length == 0)
            {
                result.Success = false;
                result.Error = "Usage: fg <PID>\nBring a background job to foreground.";
                result.ExitCode = 1;
            }
            else
            {
                if (int.TryParse(args[0], out int pid))
                {
                    var fgResult = _processManager.BringToForeground(pid);
                    if (fgResult.Success)
                    {
                        result.Output = fgResult.Output;
                        // Wait for the process to complete
                        await Task.Delay(100); // Small delay to allow process attachment
                    }
                    else
                    {
                        result.Success = false;
                        result.Error = fgResult.Error;
                        result.ExitCode = 1;
                    }
                }
                else
                {
                    result.Success = false;
                    result.Error = $"Invalid PID: {args[0]}";
                    result.ExitCode = 1;
                }
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> BackgroundCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length == 0)
            {
                result.Success = false;
                result.Error = "Usage: bg <PID>\nResume a stopped job in the background.";
                result.ExitCode = 1;
            }
            else
            {
                if (int.TryParse(args[0], out int pid))
                {
                    // In a simple shell, bg typically resumes a stopped job
                    // Since we don't have job control (Ctrl+Z), this is informational
                    var backgroundJobs = _processManager.GetBackgroundProcesses().ToList();
                    var job = backgroundJobs.FirstOrDefault(p => p.Id == pid);
                    
                    if (job != null)
                    {
                        result.Output = $"Job {pid} is already running in background.";
                    }
                    else
                    {
                        result.Success = false;
                        result.Error = $"No background job found with PID: {pid}";
                        result.ExitCode = 1;
                    }
                }
                else
                {
                    result.Success = false;
                    result.Error = $"Invalid PID: {args[0]}";
                    result.ExitCode = 1;
                }
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> KillCommand(string[] args)
        {
            var result = new CommandResult();
            
            if (args.Length == 0)
            {
                result.Success = false;
                result.Error = "Usage: kill <PID> [PID2] [PID3] ...\nTerminate one or more processes.";
                result.ExitCode = 1;
            }
            else
            {
                var killed = new List<int>();
                var failed = new List<string>();
                
                foreach (var arg in args)
                {
                    if (int.TryParse(arg, out int pid))
                    {
                        try
                        {
                            _processManager.KillProcess(pid);
                            killed.Add(pid);
                        }
                        catch (Exception ex)
                        {
                            failed.Add($"PID {pid}: {ex.Message}");
                        }
                    }
                    else
                    {
                        failed.Add($"Invalid PID: {arg}");
                    }
                }
                
                var output = new System.Text.StringBuilder();
                if (killed.Count > 0)
                {
                    output.AppendLine($"Successfully killed {killed.Count} process(es): {string.Join(", ", killed)}");
                }
                if (failed.Count > 0)
                {
                    output.AppendLine($"Failed to kill {failed.Count} process(es):");
                    foreach (var error in failed)
                    {
                        output.AppendLine($"  - {error}");
                    }
                    result.Success = failed.Count == 0;
                    result.ExitCode = failed.Count > 0 ? 1 : 0;
                }
                
                result.Output = output.ToString();
            }
            
            return await Task.FromResult(result);
        }

        private async Task<CommandResult> ShowAsciiArtCommand(string asciiFile, string imageFile, string[] args)
        {
            var result = new CommandResult { Success = true };
            
            try
            {
                // Get the base directory (where the executable is)
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string asciiPath = Path.Combine(baseDir, "ascii", asciiFile);
                string imagePath = Path.Combine(baseDir, "ascii", "gui_pics", imageFile);
                
                // For CLI: Return ASCII art text
                // For GUI: Return special marker with image path
                
                if (File.Exists(asciiPath))
                {
                    string asciiContent = await File.ReadAllTextAsync(asciiPath);
                    
                    // Check if we should display image (GUI will handle this)
                    if (File.Exists(imagePath))
                    {
                        // Return both: ASCII for CLI, image path marker for GUI
                        result.Output = $"[ASCII_ART_IMAGE:{imagePath}]\n{asciiContent}";
                    }
                    else
                    {
                        // Just ASCII art
                        result.Output = asciiContent;
                    }
                }
                else
                {
                    result.Success = false;
                    result.Error = $"ASCII art file not found: {asciiFile}";
                    result.ExitCode = 1;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = $"Error loading ASCII art: {ex.Message}";
                result.ExitCode = 1;
            }
            
            return result;
        }

        private async Task<CommandResult> ShowLogoCommand(string[] args)
        {
            var result = new CommandResult { Success = true };
            
            try
            {
                // Get the base directory (where the executable is)
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // Go up to find ok.png in the project root
                string logoPath = Path.Combine(baseDir, "..", "..", "..", "..", "ok.png");
                
                // Try to resolve the absolute path
                if (!File.Exists(logoPath))
                {
                    // Try alternate locations
                    logoPath = Path.Combine(baseDir, "ok.png");
                    if (!File.Exists(logoPath))
                    {
                        logoPath = Path.Combine(baseDir, "..", "ok.png");
                        if (!File.Exists(logoPath))
                        {
                            logoPath = Path.Combine(baseDir, "..", "..", "ok.png");
                        }
                    }
                }
                
                if (File.Exists(logoPath))
                {
                    // For GUI: Return special marker with image path
                    // For CLI: Return text description
                    string logoInfo = @"
╔══════════════════════════════════════════════════════════╗
║                     WinShell Logo                        ║
║                                                          ║
║          Aashita · Aaryan · Harsh · WINSHELL            ║
║                                                          ║
║              Professional Terminal Solution              ║
╚══════════════════════════════════════════════════════════╝
";
                    result.Output = $"[ASCII_ART_IMAGE:{Path.GetFullPath(logoPath)}]\n{logoInfo}";
                }
                else
                {
                    result.Success = false;
                    result.Error = "Logo file (ok.png) not found";
                    result.ExitCode = 1;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = $"Error loading logo: {ex.Message}";
                result.ExitCode = 1;
            }
            
            return await Task.FromResult(result);
        }

        #endregion

        #region Public Methods

        public IEnumerable<System.Diagnostics.Process> GetBackgroundJobs()
        {
            return _processManager.GetBackgroundProcesses();
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _processManager?.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}