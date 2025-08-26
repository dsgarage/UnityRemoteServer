# Unity Remote Server - Installation Guide

## Installation Methods

### Method 1: Local Package (Recommended for Development)

1. Copy the `com.dsgarage.unity-remote-server` folder to your project's `Packages` directory
2. Unity will automatically detect and import the package
3. The server will start automatically when Unity Editor launches

### Method 2: Package Manager (Git URL)

1. Open Unity Package Manager (Window > Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter: `https://github.com/dsgarage/unity-remote-server.git`
5. Click "Add"

### Method 3: Manual manifest.json

1. Open `Packages/manifest.json` in your project
2. Add to the dependencies section:
```json
{
  "dependencies": {
    "com.dsgarage.unity-remote-server": "1.0.0",
    // ... other dependencies
  }
}
```
3. Save and Unity will import the package

### Method 4: Unity Package File

1. Download the `.unitypackage` file from releases
2. In Unity, go to Assets > Import Package > Custom Package
3. Select the downloaded file
4. Click "Import"

## Verification

After installation, verify the server is running:

1. Check Unity Console for: `[Remote] Listening on http://127.0.0.1:8787/`
2. Test with curl:
```bash
curl http://127.0.0.1:8787/health
```

Expected response:
```json
{"ok":true,"version":"1.0.0"}
```

## CLI Tools Installation

Optional CLI tools for easier interaction:

### macOS/Linux
```bash
# Copy to system path
cp Packages/com.dsgarage.unity-remote-server/Samples~/CLITools/*.sh /usr/local/bin/
chmod +x /usr/local/bin/rc-*.sh

# Or add alias to .bashrc/.zshrc
alias rc-health='bash ~/YourProject/Packages/com.dsgarage.unity-remote-server/Samples~/CLITools/rc-health.sh'
alias rc-refresh='bash ~/YourProject/Packages/com.dsgarage.unity-remote-server/Samples~/CLITools/rc-refresh.sh'
alias rc-errors='bash ~/YourProject/Packages/com.dsgarage.unity-remote-server/Samples~/CLITools/rc-errors.sh'
```

### Windows
```powershell
# Copy to a directory in PATH
Copy-Item "Packages\com.dsgarage.unity-remote-server\Samples~\CLITools\*.ps1" "C:\Tools\"

# Or add to PowerShell profile
notepad $PROFILE
# Add these lines:
function rc-health { & "C:\Path\To\Your\Project\Packages\com.dsgarage.unity-remote-server\Samples~\CLITools\rc-health.ps1" }
```

## Configuration

### Change Default Port

Create `Assets/Editor/RemoteServerConfig.cs`:

```csharp
using UnityEditor;
using DSGarage.UnityRemoteServer;

[InitializeOnLoad]
public static class RemoteServerConfig
{
    static RemoteServerConfig()
    {
        // Change port before server starts
        RemoteServer.Port = 9090;
        RemoteServer.EnableDebugLogging = true;
    }
}
```

### Disable Auto-Start

```csharp
[InitializeOnLoad]
public static class DisableRemoteServer
{
    static DisableRemoteServer()
    {
        RemoteServer.Stop();
    }
}
```

## Troubleshooting

### Server Not Starting

1. **Check Console**: Look for error messages in Unity Console
2. **Port Conflict**: Another application might be using port 8787
   - Change port in configuration
   - Or stop the conflicting application
3. **Firewall**: Ensure localhost connections are allowed

### Cannot Connect

1. **Verify Unity is Running**: Server only runs when Unity Editor is open
2. **Check URL**: Ensure using `127.0.0.1` not `localhost` in some cases
3. **Test Different Port**: Try changing to port 8080 or 9000

### Permission Errors

macOS/Linux:
```bash
# If scripts aren't executable
chmod +x rc-*.sh
```

Windows:
```powershell
# If execution policy blocks scripts
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## Uninstallation

### Package Manager
1. Open Package Manager
2. Select "In Project" from dropdown
3. Find "Unity Remote Server"
4. Click "Remove"

### Manual
1. Delete `Packages/com.dsgarage.unity-remote-server` folder
2. Remove entry from `Packages/manifest.json` if present

## Support

- Issues: https://github.com/dsgarage/unity-remote-server/issues
- Documentation: https://github.com/dsgarage/unity-remote-server/wiki
- Changelog: CHANGELOG.md