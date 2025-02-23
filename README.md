# Windows Environment Variables Backup

A modern WPF application for backing up and restoring Windows environment variables with a dark-themed Material Design interface.

![License](https://img.shields.io/github/license/AlestackOverglow/envbackup)
![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)

## Features

- Backup both system and user environment variables
- View detailed content of backups
- Restore environment variables from backups
- Modern dark Material Design interface
- Automatic backup storage in user's AppData folder
- Requires administrator privileges for system variables management

## Requirements

- Windows OS
- .NET 8.0 Runtime
- Administrator privileges

## Installation

1. Download the latest release from the [Releases](../../releases) page
2. Run the application as administrator

## Building from Source

```bash
# Clone the repository
git clone https://github.com/AlestackOverglow/envbackup.git

# Navigate to project directory
cd envbackup

# Build the project
dotnet build

# Run the application
dotnet run
```

## Usage

1. Launch the application (requires administrator privileges)
2. Click "Create Backup" to save current environment variables
3. Select a backup from the list to:
   - View its contents
   - Restore variables from it
   - Delete it
4. Use "Delete All Backups" to clear the backup history

## Screenshots

[Add screenshots here]

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

Created by [AlestackOverglow](https://alestackoverglow.github.io/) 