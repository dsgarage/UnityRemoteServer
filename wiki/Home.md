# Unity Remote Server Wiki

Welcome to the Unity Remote Server documentation!

## Quick Links

- [Getting Started](Getting-Started)
- [API Documentation](API-Documentation)
- [Configuration Guide](Configuration)
- [Custom Endpoints](Custom-Endpoints)
- [CI/CD Integration](CI-CD-Integration)
- [Troubleshooting](Troubleshooting)
- [FAQ](FAQ)

## What is Unity Remote Server?

Unity Remote Server is a lightweight HTTP server that runs inside Unity Editor, providing REST APIs for remote control and automation. It enables:

- **Automated Testing** - Run tests without manual Unity interaction
- **CI/CD Integration** - Build projects from command line or scripts
- **Remote Development** - Control Unity from external tools
- **Monitoring** - Track compilation status and errors in real-time

## Key Features

### 🚀 Zero Configuration
The server starts automatically when Unity Editor launches. No setup required!

### 🔒 Security First
Only binds to localhost (127.0.0.1) by default, preventing external access.

### 🧵 Thread-Safe
Properly handles Unity's main thread requirements for safe API calls.

### 📝 Extensible
Easy to add custom endpoints for your specific needs.

## Community

- [GitHub Issues](https://github.com/dsgarage/UnityRemoteServer/issues) - Report bugs or request features
- [Discussions](https://github.com/dsgarage/UnityRemoteServer/discussions) - Ask questions and share ideas
- [Contributing](https://github.com/dsgarage/UnityRemoteServer/blob/main/CONTRIBUTING.md) - Help improve the project

## License

MIT License - See [LICENSE](https://github.com/dsgarage/UnityRemoteServer/blob/main/LICENSE.md) for details.