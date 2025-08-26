# API Documentation

This page provides detailed documentation for all Unity Remote Server API endpoints.

## Base URL

```
http://127.0.0.1:8787
```

## Authentication

No authentication is required as the server only binds to localhost.

## Response Format

All responses are in JSON format with the following structure:

### Success Response
```json
{
  "ok": true,
  "data": { ... },
  "message": "Success message"
}
```

### Error Response
```json
{
  "ok": false,
  "error": "Error message",
  "details": { ... }
}
```

## Endpoints

### GET /health

Check if the server is running and responsive.

**Request:**
```bash
curl http://127.0.0.1:8787/health
```

**Response:**
```json
{
  "ok": true,
  "version": "1.0.0",
  "unity": "2022.3.10f1",
  "platform": "StandaloneWindows64"
}
```

---

### POST /refresh

Refresh Unity assets and project files.

**Request:**
```bash
curl -X POST http://127.0.0.1:8787/refresh \
  -H "Content-Type: application/json" \
  -d '{"force": true}'
```

**Parameters:**
- `force` (boolean, optional): Force reimport all assets. Default: false

**Response:**
```json
{
  "ok": true,
  "message": "Assets refreshed"
}
```

---

### POST /awaitCompile

Wait for Unity compilation to complete.

**Request:**
```bash
curl -X POST http://127.0.0.1:8787/awaitCompile \
  -H "Content-Type: application/json" \
  -d '{"timeoutSec": 120}'
```

**Parameters:**
- `timeoutSec` (integer, optional): Maximum wait time in seconds. Default: 120

**Response:**
```json
{
  "ok": true,
  "compiling": false,
  "duration": 2.5
}
```

---

### GET /errors

Retrieve Unity console logs.

**Request:**
```bash
curl "http://127.0.0.1:8787/errors?level=error&limit=50"
```

**Query Parameters:**
- `level` (string, optional): Filter by log level
  - `error`: Only errors and exceptions
  - `warning`: Only warnings
  - `log`: Only regular logs
  - `all`: All log types (default)
- `limit` (integer, optional): Maximum entries to return. Default: 200

**Response:**
```json
[
  {
    "time": "14:23:45.123",
    "message": "NullReferenceException: Object reference not set",
    "stack": "at MyScript.Update() (Assets/Scripts/MyScript.cs:42)",
    "type": "Error"
  }
]
```

---

### POST /errors/clear

Clear all console logs.

**Request:**
```bash
curl -X POST http://127.0.0.1:8787/errors/clear
```

**Response:**
```json
{
  "ok": true,
  "message": "Logs cleared"
}
```

---

### POST /build

Build the Unity project for a target platform.

**Request:**
```bash
curl -X POST http://127.0.0.1:8787/build \
  -H "Content-Type: application/json" \
  -d '{
    "platform": "android",
    "outputPath": "Builds/game.apk",
    "scenes": ["Assets/Scenes/Main.unity", "Assets/Scenes/Menu.unity"],
    "development": false
  }'
```

**Parameters:**
- `platform` (string, required): Target platform
  - `android`: Android APK/AAB
  - `ios`: iOS Xcode project
  - `windows`: Windows executable
  - `mac`: macOS application
  - `linux`: Linux executable
  - `webgl`: WebGL build
- `outputPath` (string, required): Output file/directory path
- `scenes` (array, optional): Scene paths to include. Default: scenes in build settings
- `development` (boolean, optional): Development build. Default: false
- `compress` (boolean, optional): Compress build. Default: true

**Response:**
```json
{
  "ok": true,
  "message": "Build completed",
  "outputPath": "Builds/game.apk",
  "duration": 180.5,
  "size": 54234567
}
```

## Error Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 400 | Bad Request - Invalid parameters |
| 404 | Not Found - Endpoint doesn't exist |
| 408 | Request Timeout - Operation timed out |
| 500 | Internal Server Error |

## Rate Limiting

No rate limiting is implemented as the server is localhost-only.

## Examples

### Python Client

```python
import requests
import json

class UnityRemoteClient:
    def __init__(self, host='127.0.0.1', port=8787):
        self.base_url = f'http://{host}:{port}'
    
    def health_check(self):
        return requests.get(f'{self.base_url}/health').json()
    
    def refresh_assets(self, force=False):
        return requests.post(
            f'{self.base_url}/refresh',
            json={'force': force}
        ).json()
    
    def get_errors(self, level='error'):
        return requests.get(
            f'{self.base_url}/errors',
            params={'level': level}
        ).json()
    
    def build(self, platform, output_path):
        return requests.post(
            f'{self.base_url}/build',
            json={
                'platform': platform,
                'outputPath': output_path
            }
        ).json()

# Usage
client = UnityRemoteClient()
print(client.health_check())
```

### Node.js Client

```javascript
const axios = require('axios');

class UnityRemoteClient {
    constructor(host = '127.0.0.1', port = 8787) {
        this.baseURL = `http://${host}:${port}`;
    }
    
    async healthCheck() {
        const response = await axios.get(`${this.baseURL}/health`);
        return response.data;
    }
    
    async refreshAssets(force = false) {
        const response = await axios.post(`${this.baseURL}/refresh`, { force });
        return response.data;
    }
    
    async getErrors(level = 'error') {
        const response = await axios.get(`${this.baseURL}/errors`, {
            params: { level }
        });
        return response.data;
    }
    
    async build(platform, outputPath) {
        const response = await axios.post(`${this.baseURL}/build`, {
            platform,
            outputPath
        });
        return response.data;
    }
}

// Usage
const client = new UnityRemoteClient();
client.healthCheck().then(console.log);
```

## WebSocket Support (Future)

WebSocket support for real-time log streaming is planned for future releases.