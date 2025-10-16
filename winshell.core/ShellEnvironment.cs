using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WinShell.Core
{
    public class ShellEnvironment
    {
        private Dictionary<string, string> _variables;
        private Stack<string> _directoryStack;
        private List<string> _commandHistory;
        
        public string CurrentDirectory { get; private set; }
        public string UserName { get; private set; }
        public string MachineName { get; private set; }
        public string HomeDirectory { get; private set; }
        
        public IReadOnlyList<string> CommandHistory => _commandHistory.AsReadOnly();
        public IReadOnlyDictionary<string, string> Variables => _variables;

        public ShellEnvironment()
        {
            _variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _directoryStack = new Stack<string>();
            _commandHistory = new List<string>();
            
            Initialize();
        }

        private void Initialize()
        {
            CurrentDirectory = Environment.CurrentDirectory;
            UserName = Environment.UserName;
            MachineName = Environment.MachineName;
            HomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            
            // Load environment variables
            foreach (var key in Environment.GetEnvironmentVariables().Keys)
            {
                var keyStr = key.ToString();
                _variables[keyStr] = Environment.GetEnvironmentVariable(keyStr);
            }
            
            // Add custom variables
            _variables["SHELL"] = "WinShell";
            _variables["VERSION"] = "1.0.0";
            _variables["PROMPT"] = "WS$G";
        }

        public void SetVariable(string name, string value)
        {
            _variables[name] = value;
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        }

        public string GetVariable(string name)
        {
            return _variables.TryGetValue(name, out var value) ? value : null;
        }

        public bool ChangeDirectory(string path)
        {
            try
            {
                string newPath = Path.GetFullPath(path);
                if (Directory.Exists(newPath))
                {
                    CurrentDirectory = newPath;
                    Environment.CurrentDirectory = newPath;
                    return true;
                }
            }
            catch { }
            return false;
        }

        public void PushDirectory(string path)
        {
            _directoryStack.Push(CurrentDirectory);
            ChangeDirectory(path);
        }

        public bool PopDirectory()
        {
            if (_directoryStack.Count > 0)
            {
                return ChangeDirectory(_directoryStack.Pop());
            }
            return false;
        }

        public void AddToHistory(string command)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                _commandHistory.Add(command);
                if (_commandHistory.Count > 1000) // Limit history size
                {
                    _commandHistory.RemoveAt(0);
                }
            }
        }

        public string GetPrompt()
        {
            var prompt = _variables.TryGetValue("PROMPT", out var p) ? p : "WS$G";
            
            // Replace prompt variables
            prompt = prompt.Replace("$P", CurrentDirectory);
            prompt = prompt.Replace("$G", ">");
            prompt = prompt.Replace("$D", DateTime.Now.ToShortDateString());
            prompt = prompt.Replace("$T", DateTime.Now.ToShortTimeString());
            prompt = prompt.Replace("$U", UserName);
            prompt = prompt.Replace("$M", MachineName);
            
            // Special handling for WS prompt - show current directory name only
            if (prompt.StartsWith("WS>"))
            {
                var currentDirName = Path.GetFileName(CurrentDirectory);
                if (string.IsNullOrEmpty(currentDirName))
                {
                    // Root drive case (e.g., C:\)
                    currentDirName = CurrentDirectory.TrimEnd('\\');
                }
                return $"WS [{currentDirName}]> ";
            }
            
            return prompt + " ";
        }
    }
}