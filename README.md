# WinShell - Advanced Windows Shell

WinShell is an advanced Windows shell application available in both GUI and CLI modes.

## Features

- **Standalone CLI Mode**: Runs in its own separate console window
- **GUI Mode**: Windows Forms-based graphical interface  
- **Built-in Commands**: Comprehensive set of shell commands
- **Command History**: Track and recall previous commands
- **Environment Variables**: Full environment variable support
- **Directory Navigation**: Advanced directory management with stack support

## Running WinShell

### Method 1: Using Launch Scripts (Recommended)

#### CLI - Smart Launch with Process Management
```powershell
# PowerShell - Handles existing processes automatically
.\launch-cli.ps1

# Force close existing instances
.\launch-cli.ps1 -Force

# Batch file version
launch-cli.bat

# Direct executable launch
.\launch-cli-direct.ps1
```

#### GUI (Windows Forms)
```bash
dotnet run --project winshell.gui\WinShell.GUI.csproj
```

### Method 2: Using dotnet run (Basic)

#### CLI (Standalone Console Window)
```bash
dotnet run --project winshell.cli\WinShell.CLI.csproj
```

#### GUI (Windows Forms)
```bash
dotnet run --project winshell.gui\WinShell.GUI.csproj
```

### Method 2: Using VS Code Tasks

Press `Ctrl+Shift+P` and run:
- **Build**: `Tasks: Run Task` → `build`
- **Run CLI**: `Tasks: Run Task` → `run-cli` 
- **Run GUI**: `Tasks: Run Task` → `run-gui`
- **Run CLI Standalone**: `Tasks: Run Task` → `run-cli-standalone`

### Method 3: Using Launch Scripts

#### PowerShell
```powershell
.\launch-cli.ps1
```

#### Batch File
```cmd
launch-cli.bat
```

### Method 4: Direct Executable

After building, you can run the executables directly:

```bash
# CLI (opens in new console window)
.\winshell.cli\bin\Debug\net6.0-windows\WinShell.CLI.exe

# GUI (opens Windows Forms application)  
.\winshell.gui\bin\Debug\net6.0-windows\WinShell.GUI.exe
```

## CLI Features & Branding

### Clean WINSHELL Banner
The CLI displays a clean, professional banner that works in all console environments:
```
  +============================================================+
  |                       WINSHELL                            |
  +============================================================+

                WinShell v1.0.0 - CLI Terminal
                Running in standalone console mode
        Type 'help' for available commands, 'exit' or 'quit' to close
================================================================
```

### WS Branded Prompt
Instead of showing the full directory path like PowerShell (`PS D:\Long\Path\Here>`), WinShell uses a clean branded prompt:
```
WS [winshell2]>     # Shows current folder name only
WS [Documents]>     # Clean and professional
WS [src]>          # Easy to read
```

## CLI Commands

Type `help` in the CLI to see available commands:

- `help` - Show available commands
- `exit` or `quit` - Close the shell
- `cls` or `clear` - Clear screen
- `dir` or `ls` - List directory contents
- `cd` - Change directory
- `pwd` - Show current directory
- `echo` - Display text
- `set` - Set environment variable
- `env` - Show environment variables
- `prompt` - Customize prompt template
- And many more...

### Prompt Customization

The `prompt` command allows you to customize your shell prompt:

```bash
# Show current prompt template and help
WS [winshell2]> prompt

# Use different prompt styles
WS [winshell2]> prompt $P$G          # Full path (like PowerShell)
D:\Internet\winshell2> 

WS [winshell2]> prompt $U@$M$G       # User@Machine (like Linux)  
john@DESKTOP-ABC123>

WS [winshell2]> prompt [$T] $G       # Time-based prompt
[14:30:25] >

WS [winshell2]> prompt WS$G          # Back to WS branding (default)
WS [winshell2]>
```

**Prompt Variables:**
- `$P` = Current directory path
- `$G` = Greater than symbol (>)  
- `$D` = Current date
- `$T` = Current time
- `$U` = Username
- `$M` = Machine name
- `WS` = WinShell brand (shows directory name only)

## Building from Source

```bash
# Build entire solution
dotnet build WinShell.sln

# Build specific project
dotnet build winshell.cli\WinShell.CLI.csproj
dotnet build winshell.gui\WinShell.GUI.csproj
```

## Project Structure

```
winshell2/
├── winshell.cli/          # CLI application (standalone console)
├── winshell.gui/          # GUI application (Windows Forms)
├── winshell.core/         # Core shell engine and commands
├── winshell.common/       # Common interfaces and types
├── WinShell.sln           # Solution file
├── launch-cli.bat         # Batch launcher script
└── launch-cli.ps1         # PowerShell launcher script
```

## Key Differences

### CLI Mode
- Runs in a **separate console window** (not within PowerShell)
- Full shell functionality with command history
- Keyboard shortcuts (Ctrl+C for command cancellation)
- Professional terminal experience

### GUI Mode  
- Windows Forms graphical interface
- Visual terminal control
- Mouse interaction support
- Modern windowed experience

Both modes provide the same core shell functionality powered by the WinShell.Core engine.

## Troubleshooting

### Build Errors - DLL File Locked

**Problem**: Build fails with error "Could not copy... The file is locked by: WinShell.CLI"

**Solution**: This happens when the CLI is running and locks the DLL files. Use one of these solutions:

1. **Use the clean-build task** (Recommended):
   ```
   Ctrl+Shift+P → Tasks: Run Task → clean-build
   ```
   This automatically stops CLI processes, cleans, and rebuilds.

2. **Use the stop-cli task**:
   ```
   Ctrl+Shift+P → Tasks: Run Task → stop-cli
   ```

3. **Manual PowerShell command**:
   ```powershell
   Get-Process -Name "WinShell.CLI" -ErrorAction SilentlyContinue | Stop-Process -Force
   ```

4. **Use the smart launch scripts**: They automatically handle existing processes.

### Multiple Instances

The launch scripts (`launch-cli.ps1`, `launch-cli-direct.ps1`) automatically detect and handle existing instances, preventing conflicts and DLL locking issues.