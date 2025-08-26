# Unity Remote Server

[![Unity](https://img.shields.io/badge/Unity-2020.3%2B-black.svg?style=flat-square&logo=unity)](https://unity3d.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![GitHub release](https://img.shields.io/github/release/dsgarage/UnityRemoteServer.svg?style=flat-square)](https://github.com/dsgarage/UnityRemoteServer/releases)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-blue?style=flat-square)](https://github.com/dsgarage/UnityRemoteServer)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com)

A lightweight, zero-configuration HTTP server for Unity Editor that enables remote control and automation through REST APIs. Perfect for CI/CD pipelines, automated testing, and remote development workflows.

## ✨ Features

- 🚀 **Zero Configuration** - Automatically starts with Unity Editor
- 🔧 **Build Automation** - Remote builds for iOS, Android, and other platforms
- 📊 **Real-time Log Streaming** - Monitor Unity console logs with level filtering
- 🔄 **Asset Management** - Refresh, reimport, and track compilation status
- 🎯 **Editor Control** - Execute menu items and editor operations remotely
- 🔒 **Secure by Default** - Localhost-only binding for security
- 📝 **Extensible API** - Easy to add custom endpoints
- 🧵 **Thread-Safe** - Proper threading model for Unity's main thread

## 📦 Installation

### Option 1: Unity Package Manager (Recommended)

1. Open Unity Package Manager (Window > Package Manager)
2. Click "+" → "Add package from git URL"
3. Enter: `https://github.com/dsgarage/UnityRemoteServer.git`

### Option 2: Download Release

1. Download the latest `.unitypackage` from [Releases](https://github.com/dsgarage/UnityRemoteServer/releases)
2. Import into Unity via Assets > Import Package > Custom Package

### Option 3: Manual Installation

```bash
cd YourUnityProject/Packages
git clone https://github.com/dsgarage/UnityRemoteServer.git com.dsgarage.unity-remote-server
```

### Option 4: OpenUPM

```bash
openupm add com.dsgarage.unity-remote-server
```

## 🚀 Quick Start

Once installed, the server automatically starts on `http://127.0.0.1:8787` when Unity Editor launches.

### Test Connection

```bash
curl http://127.0.0.1:8787/health
# Response: {"ok":true,"version":"1.0.0"}
```

> **📁 Note:** Example automation scripts are provided in `Examples/Scripts/` directory. These should be used **outside** your Unity project (in CI/CD pipelines, build servers, etc.)

### Basic Usage

```bash
# Refresh assets
curl -X POST http://127.0.0.1:8787/refresh \
  -H "Content-Type: application/json" \
  -d '{"force":true}'

# Get compilation errors
curl "http://127.0.0.1:8787/errors?level=error"

# Build for Android
curl -X POST http://127.0.0.1:8787/build \
  -H "Content-Type: application/json" \
  -d '{
    "platform": "android",
    "outputPath": "Builds/game.apk",
    "scenes": ["Assets/Scenes/Main.unity"]
  }'
```

## 📚 API Reference

### Core Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/health` | GET | Server health check |
| `/refresh` | POST | Refresh Unity assets |
| `/awaitCompile` | POST | Wait for compilation to complete |
| `/errors` | GET | Get Unity console logs |
| `/errors/clear` | POST | Clear console logs |
| `/build` | POST | Build project for target platform |

### Query Parameters

#### GET /errors

- `level`: Filter by log level (`error`, `warning`, `log`, `all`)
- `limit`: Maximum number of entries (default: 200)

Example: `/errors?level=error&limit=50`

## 🔧 Configuration

### Custom Port

```csharp
using DSGarage.UnityRemoteServer;

[InitializeOnLoad]
public static class RemoteServerConfig
{
    static RemoteServerConfig()
    {
        RemoteServer.Port = 9090; // Custom port
        RemoteServer.EnableDebugLogging = true;
    }
}
```

### Custom Endpoints

```csharp
using DSGarage.UnityRemoteServer;

[InitializeOnLoad]
public static class MyCustomEndpoints
{
    static MyCustomEndpoints()
    {
        RemoteServer.RegisterCustomEndpoint("/api/custom", HandleCustomRequest);
    }
    
    static void HandleCustomRequest(HttpListenerContext ctx)
    {
        var response = new { message = "Hello from custom endpoint!" };
        RemoteServer.WriteJson(ctx, 200, JsonUtility.ToJson(response));
    }
}
```

## 📖 Documentation

- [API Documentation](https://github.com/dsgarage/UnityRemoteServer/wiki/API-Documentation)
- [Configuration Guide](https://github.com/dsgarage/UnityRemoteServer/wiki/Configuration)
- [Custom Endpoints](https://github.com/dsgarage/UnityRemoteServer/wiki/Custom-Endpoints)
- [CI/CD Integration](https://github.com/dsgarage/UnityRemoteServer/wiki/CI-CD-Integration)
- [Troubleshooting](https://github.com/dsgarage/UnityRemoteServer/wiki/Troubleshooting)

## 🛠️ Use Cases

### CI/CD Pipeline Integration

```yaml
# GitHub Actions example
- name: Wait for Unity
  run: |
    until curl -s http://127.0.0.1:8787/health; do sleep 1; done
    
- name: Build Game
  run: |
    curl -X POST http://127.0.0.1:8787/build \
      -H "Content-Type: application/json" \
      -d '{"platform": "android", "outputPath": "build.apk"}'
```

### Automated Testing

```javascript
// JavaScript test runner example
async function runUnityTests() {
    await fetch('http://127.0.0.1:8787/refresh', { method: 'POST' });
    await fetch('http://127.0.0.1:8787/awaitCompile', { method: 'POST' });
    
    const errors = await fetch('http://127.0.0.1:8787/errors?level=error');
    if (errors.length > 0) {
        throw new Error('Unity has compilation errors');
    }
}
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details.

## 🙏 Acknowledgments

- Unity Technologies for the Unity Editor
- Contributors and users of this project
- Open source community

## 🔗 Links

- [Releases](https://github.com/dsgarage/UnityRemoteServer/releases)
- [Issues](https://github.com/dsgarage/UnityRemoteServer/issues)
- [Wiki](https://github.com/dsgarage/UnityRemoteServer/wiki)
- [Discussions](https://github.com/dsgarage/UnityRemoteServer/discussions)

## 📊 Stats

![GitHub stars](https://img.shields.io/github/stars/dsgarage/UnityRemoteServer?style=social)
![GitHub forks](https://img.shields.io/github/forks/dsgarage/UnityRemoteServer?style=social)
![GitHub watchers](https://img.shields.io/github/watchers/dsgarage/UnityRemoteServer?style=social)

---

<p align="center">Made with ❤️ by <a href="https://dsgarage.com">DS Garage</a></p>