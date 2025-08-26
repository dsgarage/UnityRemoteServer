# Getting Started

This guide will help you get Unity Remote Server up and running in your Unity project.

## Prerequisites

- Unity 2020.3 LTS or higher
- Windows, macOS, or Linux
- Basic knowledge of REST APIs

## Installation

### Method 1: Unity Package Manager (Recommended)

1. Open your Unity project
2. Open Package Manager: **Window > Package Manager**
3. Click the **+** button in the top-left corner
4. Select **Add package from git URL...**
5. Enter: `https://github.com/dsgarage/UnityRemoteServer.git`
6. Click **Add**

The package will be downloaded and imported automatically.

### Method 2: Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/dsgarage/UnityRemoteServer/releases)
2. Extract the archive
3. Copy the `com.dsgarage.unity-remote-server` folder to your project's `Packages` directory
4. Unity will automatically detect and import the package

## Verification

Once installed, the server starts automatically when Unity Editor launches.

### Check Server Status

Open a terminal and run:

```bash
curl http://127.0.0.1:8787/health
```

You should see a response like:

```json
{
  "ok": true,
  "version": "1.0.0"
}
```

### Using Unity Console

You should see this message in the Unity Console:

```
[Remote] Listening on http://127.0.0.1:8787/
```

## Your First API Call

Let's make your first API call to refresh Unity assets:

```bash
curl -X POST http://127.0.0.1:8787/refresh \
  -H "Content-Type: application/json" \
  -d '{"force": false}'
```

This will trigger Unity to refresh its asset database, similar to pressing Ctrl+R (Cmd+R on Mac).

## Basic Usage Examples

### 1. Monitor Compilation Errors

```bash
# Get all errors
curl "http://127.0.0.1:8787/errors?level=error"

# Clear console
curl -X POST http://127.0.0.1:8787/errors/clear
```

### 2. Wait for Compilation

```bash
# Wait up to 60 seconds for compilation to finish
curl -X POST http://127.0.0.1:8787/awaitCompile \
  -H "Content-Type: application/json" \
  -d '{"timeoutSec": 60}'
```

### 3. Build Your Project

```bash
# Build for Android
curl -X POST http://127.0.0.1:8787/build \
  -H "Content-Type: application/json" \
  -d '{
    "platform": "android",
    "outputPath": "Builds/MyGame.apk"
  }'
```

## Using with Scripts

### Bash Script Example

Create a file `unity-build.sh`:

```bash
#!/bin/bash

# Configuration
SERVER="http://127.0.0.1:8787"
PLATFORM="android"
OUTPUT="Builds/game.apk"

echo "🔄 Refreshing assets..."
curl -X POST "$SERVER/refresh" -H "Content-Type: application/json" -d '{"force": true}'

echo "⏳ Waiting for compilation..."
curl -X POST "$SERVER/awaitCompile" -H "Content-Type: application/json" -d '{"timeoutSec": 120}'

echo "🔍 Checking for errors..."
ERRORS=$(curl -s "$SERVER/errors?level=error")
if [ $(echo "$ERRORS" | jq '. | length') -gt 0 ]; then
    echo "❌ Compilation errors found:"
    echo "$ERRORS" | jq '.'
    exit 1
fi

echo "🏗️ Building project..."
curl -X POST "$SERVER/build" \
  -H "Content-Type: application/json" \
  -d "{\"platform\": \"$PLATFORM\", \"outputPath\": \"$OUTPUT\"}"

echo "✅ Build complete!"
```

### Python Script Example

Create a file `unity_control.py`:

```python
import requests
import time
import sys

class UnityController:
    def __init__(self):
        self.base_url = "http://127.0.0.1:8787"
    
    def wait_for_server(self, timeout=30):
        """Wait for Unity server to be ready"""
        start = time.time()
        while time.time() - start < timeout:
            try:
                response = requests.get(f"{self.base_url}/health")
                if response.status_code == 200:
                    print("✅ Unity server is ready")
                    return True
            except:
                pass
            time.sleep(1)
        print("❌ Unity server timeout")
        return False
    
    def refresh_and_compile(self):
        """Refresh assets and wait for compilation"""
        print("🔄 Refreshing assets...")
        requests.post(f"{self.base_url}/refresh", json={"force": True})
        
        print("⏳ Waiting for compilation...")
        requests.post(f"{self.base_url}/awaitCompile", json={"timeoutSec": 120})
        
        # Check for errors
        errors = requests.get(f"{self.base_url}/errors?level=error").json()
        if errors:
            print("❌ Compilation errors:")
            for error in errors:
                print(f"  - {error['message']}")
            return False
        
        print("✅ Compilation successful")
        return True
    
    def build(self, platform, output_path):
        """Build the project"""
        print(f"🏗️ Building {platform}...")
        response = requests.post(
            f"{self.base_url}/build",
            json={
                "platform": platform,
                "outputPath": output_path
            }
        )
        
        if response.status_code == 200:
            print("✅ Build complete!")
            return True
        else:
            print(f"❌ Build failed: {response.text}")
            return False

# Usage
if __name__ == "__main__":
    controller = UnityController()
    
    if not controller.wait_for_server():
        sys.exit(1)
    
    if not controller.refresh_and_compile():
        sys.exit(1)
    
    if not controller.build("android", "Builds/game.apk"):
        sys.exit(1)
```

## Next Steps

- Read the [API Documentation](API-Documentation) for detailed endpoint information
- Learn about [Configuration](Configuration) options
- Create [Custom Endpoints](Custom-Endpoints) for your specific needs
- Set up [CI/CD Integration](CI-CD-Integration)

## Troubleshooting

If the server doesn't start:

1. Check Unity Console for error messages
2. Ensure no other application is using port 8787
3. Try restarting Unity Editor
4. Check [Troubleshooting](Troubleshooting) guide for more solutions

## Need Help?

- [GitHub Issues](https://github.com/dsgarage/UnityRemoteServer/issues)
- [Discussions](https://github.com/dsgarage/UnityRemoteServer/discussions)