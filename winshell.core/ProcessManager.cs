using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinShell.Common;

namespace WinShell.Core
{
    public class ProcessManager : IDisposable
    {
        private readonly ConcurrentDictionary<int, Process> _runningProcesses;
        private readonly ConcurrentDictionary<int, Process> _backgroundProcesses;
        private readonly ShellEnvironment _environment;

        public event EventHandler<string> OutputReceived;
        public event EventHandler<string> ErrorReceived;

        public ProcessManager(ShellEnvironment environment)
        {
            _runningProcesses = new ConcurrentDictionary<int, Process>();
            _backgroundProcesses = new ConcurrentDictionary<int, Process>();
            _environment = environment;
        }

        public async Task<CommandResult> ExecuteAsync(string command, string[] arguments, CancellationToken cancellationToken = default)
        {
            var result = new CommandResult();
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var executablePath = ResolveExecutable(command);
                if (executablePath == null)
                {
                    result.Success = false;
                    result.Error = "'" + command + "' is not recognized as an internal or external command.";
                    result.ExitCode = 1;
                    stopwatch.Stop();
                    result.ExecutionTime = stopwatch.Elapsed;
                    return result;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = string.Join(" ", arguments),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _environment.CurrentDirectory
                };

                foreach (var kvp in _environment.Variables)
                {
                    startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
                }
                
                using (var process = new Process { StartInfo = startInfo })
                {
                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();
                    var outputComplete = new TaskCompletionSource<bool>();
                    var errorComplete = new TaskCompletionSource<bool>();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            outputBuilder.AppendLine(e.Data);
                            OutputReceived?.Invoke(this, e.Data);
                        }
                        else
                        {
                            outputComplete.SetResult(true);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            errorBuilder.AppendLine(e.Data);
                            ErrorReceived?.Invoke(this, e.Data);
                        }
                        else
                        {
                            errorComplete.SetResult(true);
                        }
                    };

                    process.Start();
                    _runningProcesses[process.Id] = process;

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await Task.WhenAll(
                        WaitForExitAsync(process, cancellationToken),
                        outputComplete.Task,
                        errorComplete.Task
                    );

                    _runningProcesses.TryRemove(process.Id, out _);

                    result.Output = outputBuilder.ToString();
                    result.Error = errorBuilder.ToString();
                    result.ExitCode = process.ExitCode;
                    result.Success = process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.ExitCode = -1;
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
            }

            return result;
        }

        /// <summary>
        /// Execute a pipeline of commands - 100% NATIVE piping (NO PowerShell)
        /// Example: dir | findstr ".txt" | sort
        /// </summary>
        public async Task<CommandResult> ExecutePipelineAsync(List<(string command, string[] args)> pipeline, CancellationToken cancellationToken = default)
        {
            var result = new CommandResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var processes = new List<Process>();
                Process previousProcess = null;

                // Start all processes in the pipeline
                for (int i = 0; i < pipeline.Count; i++)
                {
                    var (command, args) = pipeline[i];
                    var executablePath = ResolveExecutable(command);
                    
                    if (executablePath == null)
                    {
                        // Clean up already started processes
                        foreach (var p in processes)
                        {
                            try { p.Kill(); p.Dispose(); } catch { }
                        }
                        
                        result.Success = false;
                        result.Error = "'" + command + "' is not recognized as an internal or external command.";
                        result.ExitCode = 1;
                        stopwatch.Stop();
                        result.ExecutionTime = stopwatch.Elapsed;
                        return result;
                    }

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = executablePath,
                        Arguments = string.Join(" ", args),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true,
                        WorkingDirectory = _environment.CurrentDirectory
                    };

                    foreach (var kvp in _environment.Variables)
                    {
                        startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
                    }

                    var process = new Process { StartInfo = startInfo };
                    processes.Add(process);
                    process.Start();
                    _runningProcesses[process.Id] = process;

                    // Connect pipe from previous process to this process's stdin
                    if (previousProcess != null)
                    {
                        var prevProc = previousProcess;
                        var currProc = process;
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await prevProc.StandardOutput.BaseStream.CopyToAsync(currProc.StandardInput.BaseStream);
                                currProc.StandardInput.Close();
                            }
                            catch { /* Pipe broken is normal when process terminates */ }
                        });
                    }

                    previousProcess = process;
                }

                // Capture output from the LAST process in the pipeline
                var lastProcess = processes[processes.Count - 1];
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                var outputTask = Task.Run(async () =>
                {
                    string line;
                    while ((line = await lastProcess.StandardOutput.ReadLineAsync()) != null)
                    {
                        outputBuilder.AppendLine(line);
                        OutputReceived?.Invoke(this, line);
                    }
                });

                var errorTask = Task.Run(async () =>
                {
                    string line;
                    while ((line = await lastProcess.StandardError.ReadLineAsync()) != null)
                    {
                        errorBuilder.AppendLine(line);
                        ErrorReceived?.Invoke(this, line);
                    }
                });

                // Wait for all processes to complete
                await Task.WhenAll(processes.Select(p => WaitForExitAsync(p, cancellationToken)));
                await Task.WhenAll(outputTask, errorTask);

                // Clean up
                foreach (var process in processes)
                {
                    _runningProcesses.TryRemove(process.Id, out _);
                    process.Dispose();
                }

                result.Output = outputBuilder.ToString();
                result.Error = errorBuilder.ToString();
                result.ExitCode = lastProcess.ExitCode;
                result.Success = lastProcess.ExitCode == 0;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.ExitCode = -1;
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
            }

            return result;
        }

        /// <summary>
        /// Execute command in background - returns immediately (NO PowerShell)
        /// Example: ping google.com -t &
        /// </summary>
        public Task<CommandResult> ExecuteBackgroundAsync(string command, string[] arguments)
        {
            var result = new CommandResult();
            
            try
            {
                var executablePath = ResolveExecutable(command);
                if (executablePath == null)
                {
                    result.Success = false;
                    result.Error = "'" + command + "' is not recognized as an internal or external command.";
                    result.ExitCode = 1;
                    return Task.FromResult(result);
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = string.Join(" ", arguments),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _environment.CurrentDirectory
                };

                foreach (var kvp in _environment.Variables)
                {
                    startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
                }

                var process = new Process { StartInfo = startInfo };
                process.Start();
                _backgroundProcesses[process.Id] = process;

                result.Success = true;
                result.Output = "[Background job started with PID: " + process.Id + "]\n";
                result.ExitCode = 0;

                // Monitor background process completion
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await WaitForExitAsync(process, CancellationToken.None);
                        _backgroundProcesses.TryRemove(process.Id, out _);
                        OutputReceived?.Invoke(this, "[Background job " + process.Id + " completed with exit code: " + process.ExitCode + "]");
                        process.Dispose();
                    }
                    catch { }
                });
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.ExitCode = -1;
            }

            return Task.FromResult(result);
        }

        private string ResolveExecutable(string command)
        {
            if (File.Exists(command))
                return command;

            var currentDirPath = Path.Combine(_environment.CurrentDirectory, command);
            if (File.Exists(currentDirPath))
                return currentDirPath;

            var extensions = new[] { ".exe", ".bat", ".cmd", ".com" };
            foreach (var ext in extensions)
            {
                var withExt = command + ext;
                if (File.Exists(withExt))
                    return withExt;
                
                var currentDirWithExt = Path.Combine(_environment.CurrentDirectory, withExt);
                if (File.Exists(currentDirWithExt))
                    return currentDirWithExt;
            }

            var pathVar = _environment.GetVariable("PATH");
            if (!string.IsNullOrEmpty(pathVar))
            {
                var paths = pathVar.Split(Path.PathSeparator);
                foreach (var path in paths)
                {
                    try
                    {
                        var fullPath = Path.Combine(path, command);
                        if (File.Exists(fullPath))
                            return fullPath;

                        foreach (var ext in extensions)
                        {
                            var fullPathWithExt = fullPath + ext;
                            if (File.Exists(fullPathWithExt))
                                return fullPathWithExt;
                        }
                    }
                    catch { }
                }
            }

            return null;
        }

        private async Task WaitForExitAsync(Process process, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(true);
            
            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                await tcs.Task;
            }
        }

        public void KillProcess(int processId)
        {
            if (_runningProcesses.TryGetValue(processId, out var process))
            {
                try
                {
                    process.Kill();
                    _runningProcesses.TryRemove(processId, out _);
                }
                catch { }
            }
        }

        public IEnumerable<Process> GetRunningProcesses()
        {
            return _runningProcesses.Values;
        }

        public IEnumerable<Process> GetBackgroundProcesses()
        {
            return _backgroundProcesses.Values;
        }

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
                    foreach (var process in _runningProcesses.Values)
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                            }
                            process.Dispose();
                        }
                        catch { }
                    }
                    _runningProcesses.Clear();

                    foreach (var process in _backgroundProcesses.Values)
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                            }
                            process.Dispose();
                        }
                        catch { }
                    }
                    _backgroundProcesses.Clear();
                }
                _disposed = true;
            }
        }
    }
}
