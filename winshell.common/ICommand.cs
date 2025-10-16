using System;
using System.Threading.Tasks;

namespace WinShell.Common
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        Task<CommandResult> ExecuteAsync(string[] args);
    }

    public class CommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public int ExitCode { get; set; }
        public TimeSpan ExecutionTime { get; set; }

        public CommandResult()
        {
            Success = true;
            Output = string.Empty;
            Error = string.Empty;
            ExitCode = 0;
        }
    }

    public class ShellCommand
    {
        public string Command { get; set; }
        public string[] Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public DateTime Timestamp { get; set; }
    }
}