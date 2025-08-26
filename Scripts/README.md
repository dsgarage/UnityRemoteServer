# Unity Remote Server Scripts

This directory contains utility scripts for controlling Unity via Remote Server API.

## Shell Scripts

### unity-build.sh
Automated build script for Unity projects.

```bash
# Build for Android
./unity-build.sh android Builds/game.apk

# Build for iOS
./unity-build.sh ios Builds/ios

# Build for Windows
./unity-build.sh windows Builds/game.exe
```

### unity-monitor.sh
Real-time Unity console monitoring.

```bash
# Monitor errors only
./unity-monitor.sh error

# Monitor warnings
./unity-monitor.sh warning

# Monitor all logs
./unity-monitor.sh all
```

## Python Script

### unity-control.py
Comprehensive Unity control script with multiple commands.

```bash
# Check server health
python3 unity-control.py health

# Refresh assets
python3 unity-control.py refresh --force

# Wait for compilation
python3 unity-control.py compile --timeout 120

# Get errors
python3 unity-control.py errors --level error --limit 50

# Clear console
python3 unity-control.py clear

# Build project
python3 unity-control.py build android Builds/game.apk

# Monitor console in real-time
python3 unity-control.py monitor --level error --interval 5
```

## Environment Variables

All scripts support environment variables for configuration:

- `UNITY_REMOTE_SERVER` - Server URL (default: http://127.0.0.1:8787)
- `UNITY_BUILD_TIMEOUT` - Build timeout in seconds (default: 300)
- `MONITOR_INTERVAL` - Monitor refresh interval (default: 5)

Example:
```bash
export UNITY_REMOTE_SERVER="http://192.168.1.100:8787"
./unity-build.sh android Builds/game.apk
```

## Requirements

### Shell Scripts
- bash
- curl
- jq (for JSON parsing)

Install on macOS:
```bash
brew install jq
```

Install on Ubuntu/Debian:
```bash
apt-get install jq
```

### Python Script
- Python 3.6+
- requests library

Install requirements:
```bash
pip3 install requests
```

## CI/CD Integration

These scripts are perfect for CI/CD pipelines:

```yaml
# GitHub Actions example
- name: Build Unity Project
  run: |
    ./Scripts/unity-build.sh android Builds/game.apk
    
- name: Check for Errors
  run: |
    python3 Scripts/unity-control.py errors --level error
```

```groovy
// Jenkins example
stage('Build') {
    steps {
        sh './Scripts/unity-build.sh android Builds/game.apk'
    }
}
```

## Troubleshooting

### Connection Refused
Ensure Unity Editor is running with Remote Server package installed.

### Command not found: jq
Install jq for JSON parsing in shell scripts.

### Python module not found
Install requests: `pip3 install requests`