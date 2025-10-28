# WinShell - Modern Terminal Solution for Windows

A comprehensive Windows terminal application developed as part of an academic project, combining the functionality of a graphical user interface and command-line interface into a unified shell environment.

## Project Overview

WinShell represents a complete reimagining of the traditional Windows command-line experience. The project demonstrates advanced software engineering principles by implementing a dual-interface terminal that provides both GUI convenience and CLI efficiency while maintaining a shared core engine architecture.

## Core Architecture

The project is structured using a layered architecture pattern that promotes separation of concerns and maintainability:

### winshell.common
Contains shared interfaces and data structures used throughout the application. This layer defines the contract for command execution results and command implementations, ensuring consistency across all components.

### winshell.core
The heart of the application, containing:
- **CommandEngine**: Orchestrates command parsing, execution, and result handling
- **ProcessManager**: Manages external process lifecycle and background job execution
- **ShellEnvironment**: Maintains session state including current directory and environment variables
- **CommandParser**: Tokenizes and validates command syntax with support for pipes and redirections

### winshell.cli
Command-line interface implementation providing:
- Interactive command prompt with history navigation
- Asynchronous command execution
- Signal handling for process interruption
- Support for Windows Console zoom and formatting
- ASCII art display capabilities

### winshell.gui
Windows Forms-based graphical interface featuring:
- Custom terminal control with rich text formatting
- Theme management system with multiple color schemes
- Visual process monitoring with real-time metrics
- Quick navigation dialog for common system locations
- Image embedding support for enhanced content display
- Zoom functionality for improved accessibility

## Key Features

### Dual Interface Design
Users can choose between GUI and CLI modes depending on their workflow preferences. Both interfaces share the same underlying command engine, ensuring consistent behavior and results.

### Advanced Command Processing
The shell supports sophisticated command patterns including:
- Pipeline operations for chaining commands
- Input and output redirection
- Background job execution
- Built-in command set with native implementation
- External program execution with full console support

### Visual Enhancements
The GUI provides modern features not typically found in traditional terminals:
- Live theme switching without restart
- Real-time process monitoring with sortable columns
- Embedded image support for rich content
- Customizable font sizing with zoom controls

### Custom Commands
Includes specialized commands for displaying ASCII art and images, demonstrating the extensibility of the command system. The logo command showcases project branding while the ASCII art commands illustrate how text and graphics can be integrated into terminal workflows.

## Technical Implementation

### Language and Framework
- Built with C# targeting .NET 6.0
- Windows Forms for GUI implementation
- Asynchronous programming model using async/await
- Strong typing with interface-based design

### Command Execution Model
Commands are executed through a unified pipeline:
1. Input parsing and tokenization
2. Command type resolution (built-in vs external)
3. Asynchronous execution with cancellation support
4. Result aggregation and output formatting
5. Error handling and status reporting

### Process Management
Background processes are tracked and managed through a dedicated ProcessManager component, allowing users to:
- List active background jobs
- Bring jobs to foreground
- Terminate running processes
- Monitor resource usage

## Installation and Usage

### Requirements
- Windows 10 or later (version 1809+)
- .NET 6.0 Runtime (included in self-contained builds)

### Building from Source
```bash
dotnet build WinShell.sln -c Release
```

### Running the Applications
GUI Mode:
```bash
dotnet run --project winshell.gui/WinShell.GUI.csproj
```

CLI Mode:
```bash
dotnet run --project winshell.cli/WinShell.CLI.csproj
```

### Available Commands
The shell includes over 30 built-in commands including:
- File operations: cd, ls, mkdir, rm, cp, mv
- System information: sysinfo, ps, env, path
- Process control: kill, jobs, fg, bg
- Utilities: cat, echo, pwd, history, clear
- Custom: logo, ag, aloksir, monikamam, and others

## Academic Context

This project serves as a demonstration of:
- Software architecture design principles
- Object-oriented programming in C#
- Asynchronous programming patterns
- User interface development
- Process management and system programming
- Version control with Git
- Professional software documentation

## Project Structure

```
WinShell/
├── winshell.cli/          # Command-line interface
│   ├── CliInterface.cs
│   ├── Program.cs
│   └── WinShell.CLI.csproj
├── winshell.common/       # Shared interfaces
│   ├── CommandResult.cs
│   ├── ICommand.cs
│   └── WinShell.Common.csproj
├── winshell.core/         # Core engine
│   ├── CommandEngine.cs
│   ├── CommandParser.cs
│   ├── ProcessManager.cs
│   ├── ShellEnvironment.cs
│   └── WinShell.Core.csproj
└── winshell.gui/          # Graphical interface
    ├── MainWindow.cs
    ├── TerminalControl.cs
    ├── ThemeManager.cs
    ├── ProcessMonitorForm.cs
    └── WinShell.GUI.csproj
```

## Development Team

Developed by students as part of their coursework, demonstrating practical application of software engineering concepts learned in academic settings.

## License

This project is created for educational purposes as part of academic coursework.

## Acknowledgments

Special thanks to the faculty advisors and mentors who provided guidance throughout the development process. The ASCII art feature includes tributes to project mentors and team members.

## Future Enhancements

Potential areas for expansion include:
- Script execution capabilities
- Command completion and suggestions
- Extended plugin architecture
- Cross-platform support
- Remote shell capabilities
- Enhanced debugging tools

---

For more information or to report issues, please use the GitHub repository issue tracker.
