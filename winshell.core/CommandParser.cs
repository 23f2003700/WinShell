using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WinShell.Core
{
    public class CommandParser
    {
        private readonly ShellEnvironment _environment;

        public CommandParser(ShellEnvironment environment)
        {
            _environment = environment;
        }

        public ParsedCommand Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var result = new ParsedCommand { RawInput = input };
            
            // Handle pipes and redirection
            var parts = SplitCommand(input);
            
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("|"))
                {
                    result.IsPiped = true;
                    result.PipedCommands.Add(Parse(trimmed.Substring(1).Trim()));
                }
                else if (trimmed.Contains(">"))
                {
                    var redirectParts = trimmed.Split('>');
                    result.OutputRedirect = redirectParts[1].Trim();
                    result.IsAppend = trimmed.Contains(">>");
                    trimmed = redirectParts[0].Trim();
                }
                else if (trimmed.Contains("<"))
                {
                    var redirectParts = trimmed.Split('<');
                    result.InputRedirect = redirectParts[1].Trim();
                    trimmed = redirectParts[0].Trim();
                }
            }

            // Expand variables
            input = ExpandVariables(parts[0]);
            
            // Parse command and arguments
            var tokens = TokenizeCommand(input);
            if (tokens.Count > 0)
            {
                result.Command = tokens[0];
                result.Arguments = tokens.Skip(1).ToArray();
            }

            return result;
        }

        private string ExpandVariables(string input)
        {
            var pattern = @"\$\{?(\w+)\}?|%(\w+)%";
            return Regex.Replace(input, pattern, match =>
            {
                var varName = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                return _environment.GetVariable(varName) ?? match.Value;
            });
        }

        private List<string> SplitCommand(string input)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;
            var quoteChar = '\0';

            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                
                if ((c == '"' || c == '\'') && (i == 0 || input[i - 1] != '\\'))
                {
                    if (!inQuotes)
                    {
                        inQuotes = true;
                        quoteChar = c;
                    }
                    else if (c == quoteChar)
                    {
                        inQuotes = false;
                        quoteChar = '\0';
                    }
                    current.Append(c);
                }
                else if (!inQuotes && (c == '|' || c == '>' || c == '<'))
                {
                    if (current.Length > 0)
                    {
                        parts.Add(current.ToString());
                        current.Clear();
                    }
                    current.Append(c);
                    if (i + 1 < input.Length && input[i + 1] == c)
                    {
                        current.Append(input[++i]);
                    }
                    parts.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
                parts.Add(current.ToString());

            return parts;
        }

        private List<string> TokenizeCommand(string input)
        {
            var tokens = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;
            var quoteChar = '\0';

            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if ((c == '"' || c == '\'') && (i == 0 || input[i - 1] != '\\'))
                {
                    if (!inQuotes)
                    {
                        inQuotes = true;
                        quoteChar = c;
                    }
                    else if (c == quoteChar)
                    {
                        inQuotes = false;
                        quoteChar = '\0';
                    }
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
                tokens.Add(current.ToString());

            return tokens;
        }
    }

    public class ParsedCommand
    {
        public string RawInput { get; set; }
        public string Command { get; set; }
        public string[] Arguments { get; set; }
        public bool IsPiped { get; set; }
        public List<ParsedCommand> PipedCommands { get; set; }
        public string OutputRedirect { get; set; }
        public string InputRedirect { get; set; }
        public bool IsAppend { get; set; }

        public ParsedCommand()
        {
            Arguments = Array.Empty<string>();
            PipedCommands = new List<ParsedCommand>();
        }
    }
}