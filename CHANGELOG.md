# Changelog

All notable changes to Unity Remote Server will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-12-26

### Added
- Initial release of Unity Remote Server package
- HTTP server with automatic startup on Unity Editor launch
- Core endpoints: /health, /refresh, /awaitCompile, /errors, /build
- Log monitoring with level filtering (error, warning, log, all)
- Build automation for iOS and Android platforms
- Thread-safe request handling with main thread execution
- Custom endpoint registration system
- CLI tools for easy interaction
- Comprehensive documentation and samples

### Features
- Zero configuration - works out of the box
- Localhost only for security (127.0.0.1:8787)
- Early log subscription from editor startup
- Concurrent request queue processing
- JSON response format for all endpoints
- Error tracking with stack traces
- Compilation status monitoring

### Technical
- Unity 2020.3+ compatibility
- Editor-only assembly definition
- Namespace: DSGarage.UnityRemoteServer
- No external dependencies
- Thread-safe implementation