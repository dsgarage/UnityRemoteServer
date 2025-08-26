# Contributing to Unity Remote Server

First off, thank you for considering contributing to Unity Remote Server! It's people like you that make Unity Remote Server such a great tool.

## Code of Conduct

By participating in this project, you are expected to uphold our Code of Conduct:
- Be respectful and inclusive
- Welcome newcomers and help them get started
- Focus on constructive criticism
- Show empathy towards other community members

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Unity version**
- **Operating System**
- **Steps to reproduce**
- **Expected behavior**
- **Actual behavior**
- **Console logs** (if applicable)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, include:

- **Use case** - Why is this enhancement needed?
- **Proposed solution** - How do you envision it working?
- **Alternative solutions** - What other solutions have you considered?

### Pull Requests

1. Fork the repo and create your branch from `main`
2. If you've added code that should be tested, add tests
3. Ensure the code compiles without warnings
4. Make sure your code follows the existing style
5. Issue that pull request!

## Development Process

### Setup

1. Clone your fork:
```bash
git clone https://github.com/your-username/UnityRemoteServer.git
cd UnityRemoteServer
```

2. Create a branch:
```bash
git checkout -b feature/my-new-feature
```

3. Make your changes and test in Unity

4. Commit your changes:
```bash
git commit -m "Add: New feature description"
```

### Commit Messages

We use conventional commits:

- `Add:` New feature
- `Fix:` Bug fix
- `Update:` Update existing functionality
- `Remove:` Remove functionality
- `Refactor:` Code refactoring
- `Doc:` Documentation changes
- `Test:` Add or update tests

### Code Style

#### C# Style Guide

```csharp
// Use PascalCase for public members
public class RemoteServer
{
    // Use camelCase for private fields with underscore prefix
    private static HttpListener _listener;
    
    // Properties use PascalCase
    public static bool IsRunning { get; private set; }
    
    // Methods use PascalCase
    public static void StartServer()
    {
        // Use meaningful variable names
        var configuration = LoadConfiguration();
        
        // Always use braces, even for single lines
        if (configuration != null)
        {
            ApplyConfiguration(configuration);
        }
    }
}
```

### Testing

Before submitting:

1. Test in Unity 2020.3 LTS (minimum supported version)
2. Test in the latest Unity LTS version
3. Test on Windows, macOS, and Linux if possible
4. Ensure no compilation errors or warnings
5. Run existing tests if applicable

### Documentation

- Update README.md if needed
- Add XML documentation to public APIs
- Update CHANGELOG.md for notable changes
- Add/update wiki pages for new features

## Project Structure

```
UnityRemoteServer/
├── Editor/                 # Main source code
│   ├── RemoteServer.cs    # Core server implementation
│   ├── LogStore.cs        # Log management
│   └── RemoteOps.cs       # Unity operations
├── Samples~/              # Example code
├── Documentation~/        # Additional docs
├── Tests/                 # Unit tests
└── package.json          # Package manifest
```

## Release Process

1. Update version in `package.json`
2. Update CHANGELOG.md
3. Create a pull request to `main`
4. After merge, tag the release
5. GitHub Actions will create the release

## Questions?

Feel free to open an issue with the label "question" or reach out on our [Discussions](https://github.com/dsgarage/UnityRemoteServer/discussions) page.

## Recognition

Contributors will be recognized in:
- The README.md file
- Release notes
- Our website

Thank you for contributing! 🎉