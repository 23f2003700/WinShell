#  WinShell - Modern Windows Terminal

A powerful, modern Windows terminal application with dual CLI and GUI interfaces. Features 30+ built-in commands, native pipeline support, background job management, and a rich theming system.

##  Features

### Core Capabilities
- **30+ Built-in Commands** - File operations, process control, environment management, and utilities
- **Native Pipelines** - Text-based pipeline support with `|` operator
- **I/O Redirection** - Full support for `>`, `>>`, and `<` operators
- **Background Jobs** - Run commands with `&`, manage with `jobs`, `fg`, `bg`, `kill`
- **Async Execution** - All commands run asynchronously with cancellation support
- **Dual Interface** - Both CLI and GUI versions available

### GUI Features
- 6 Beautiful themes (Classic, Dark, Ocean, Sunset, Forest, Cyberpunk)
- Real-time process monitoring
- Command history browser
- Quick navigation
- Settings panel

##  Installation

### Method 1: NuGet Global Tool (Recommended)

```bash
dotnet tool install --global WinShell.CLI
winshell
```

**Update:**
```bash
dotnet tool update --global WinShell.CLI
```

**Uninstall:**
```bash
dotnet tool uninstall --global WinShell.CLI
```

### Method 2: Download from GitHub Releases

1. Visit [Releases](https://github.com/23f2003700/WinShell/releases)
2. Download:
   - `WinShell-CLI-vX.X.X-win-x64.zip` - CLI only
   - `WinShell-GUI-vX.X.X-win-x64.zip` - GUI only
   - `WinShell-Complete-vX.X.X-win-x64.zip` - Both
3. Extract and run

### Method 3: Build from Source

```bash
git clone https://github.com/23f2003700/WinShell.git
cd WinShell
dotnet build -c Release
```

##  Quick Start

```bash
# Start WinShell
winshell

# Basic commands
> help              # Show all commands
> ls                # List files
> pwd               # Current directory
> echo Hello World  # Print text

# Pipeline
> dir | findstr .txt

# Background job
> notepad &
> jobs

# Exit
> exit
```

##  Built-in Commands

### File Operations
- `dir`, `ls` - List directory
- `mkdir` - Create directory
- `rmdir` - Remove directory
- `del`, `rm` - Delete files
- `copy`, `cp` - Copy files
- `move`, `mv` - Move files
- `type`, `cat` - Display file

### Navigation
- `cd`, `chdir` - Change directory
- `pwd` - Print working directory
- `pushd` - Push to directory stack
- `popd` - Pop from directory stack

### Process Control
- `jobs` - List background jobs
- `fg` - Foreground job
- `bg` - Background job
- `kill` - Terminate process

### Environment
- `set` - Set variable
- `env` - List variables
- `prompt` - Customize prompt

### Utilities
- `echo` - Print text
- `cls`, `clear` - Clear screen
- `help` - Show help
- `history` - Command history
- `exit` - Exit shell

##  System Requirements

**Minimum:**
- Windows 10 (version 1809+)
- x64 architecture
- .NET 6.0 Runtime
- 4 GB RAM

**Recommended:**
- Windows 11
- 8 GB RAM

##  Academic Project

Developed at **MBM University, Jodhpur** by:
- AASHITA BHANDARI
- HARSH RAJANI
- AARYAN CHOUDHARY

##  For Developers

### Publishing a New Version

1. **Update version** in `winshell.cli/WinShell.CLI.csproj`

2. **Create and push tag:**
```bash
git tag v1.0.1
git push origin v1.0.1
```

3. **GitHub Actions automatically:**
   - Builds solution
   - Creates NuGet package
   - Creates ZIP files
   - Creates GitHub Release
   - Uploads artifacts

### Manual Build

```powershell
# Build everything
.\build-release.ps1 -Version "1.0.0" -CreateNuGetPackage -CreateZipPackages

# Publish to NuGet (requires API key)
.\publish-nuget.ps1 -ApiKey "YOUR_KEY" -Version "1.0.0"
```

### GitHub Secrets Setup

Add to repository secrets:

- `NUGET_API_KEY` - Your NuGet API key

##  Troubleshooting

### "dotnet tool not found"
Ensure .NET 6.0 SDK/Runtime is installed: <https://dotnet.microsoft.com/download/dotnet/6.0>

### "winshell command not found"
Add to PATH: `$env:USERPROFILE\.dotnet\tools`

### Build errors
```bash
dotnet clean
dotnet restore
dotnet build
```

##  License

MIT License - See [LICENSE](LICENSE) file

Copyright  2025 WinShell Project

##  Links

- **Repository:** <https://github.com/23f2003700/WinShell>
- **Issues:** <https://github.com/23f2003700/WinShell/issues>
- **NuGet:** <https://www.nuget.org/packages/WinShell.CLI/>

##  Release Notes

### Version 1.0.0 (November 2025)

**Initial Release:**
- 30+ built-in commands
- Native pipeline support
- Background job management
- I/O redirection
- Async execution
- Dual CLI/GUI interface
- 6 themes for GUI
- Command history
- Process monitoring

---

**Made with  in India**