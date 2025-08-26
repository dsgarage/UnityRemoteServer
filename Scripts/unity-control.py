#!/usr/bin/env python3

"""
Unity Remote Server - Python Control Script
Provides a command-line interface for Unity Remote Server
"""

import requests
import json
import sys
import time
import argparse
from typing import Optional, Dict, Any

class UnityRemoteClient:
    def __init__(self, host: str = '127.0.0.1', port: int = 8787):
        """Initialize Unity Remote Client"""
        self.base_url = f'http://{host}:{port}'
        self.session = requests.Session()
    
    def health_check(self) -> Dict[str, Any]:
        """Check server health"""
        try:
            response = self.session.get(f'{self.base_url}/health')
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            return {'ok': False, 'error': str(e)}
    
    def refresh_assets(self, force: bool = False) -> Dict[str, Any]:
        """Refresh Unity assets"""
        try:
            response = self.session.post(
                f'{self.base_url}/refresh',
                json={'force': force}
            )
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            return {'ok': False, 'error': str(e)}
    
    def await_compile(self, timeout: int = 120) -> Dict[str, Any]:
        """Wait for compilation to complete"""
        try:
            response = self.session.post(
                f'{self.base_url}/awaitCompile',
                json={'timeoutSec': timeout}
            )
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            return {'ok': False, 'error': str(e)}
    
    def get_errors(self, level: str = 'all', limit: int = 200) -> list:
        """Get Unity console errors"""
        try:
            response = self.session.get(
                f'{self.base_url}/errors',
                params={'level': level, 'limit': limit}
            )
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"Error fetching logs: {e}")
            return []
    
    def clear_console(self) -> Dict[str, Any]:
        """Clear Unity console"""
        try:
            response = self.session.post(f'{self.base_url}/errors/clear')
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            return {'ok': False, 'error': str(e)}
    
    def build(self, platform: str, output_path: str, 
              development: bool = False) -> Dict[str, Any]:
        """Build Unity project"""
        try:
            response = self.session.post(
                f'{self.base_url}/build',
                json={
                    'platform': platform,
                    'outputPath': output_path,
                    'development': development
                }
            )
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            return {'ok': False, 'error': str(e)}
    
    def wait_for_server(self, timeout: int = 30) -> bool:
        """Wait for server to be ready"""
        start = time.time()
        while time.time() - start < timeout:
            result = self.health_check()
            if result.get('ok'):
                return True
            time.sleep(1)
        return False

def main():
    parser = argparse.ArgumentParser(
        description='Unity Remote Server Control Script'
    )
    parser.add_argument(
        '--host', default='127.0.0.1',
        help='Server host (default: 127.0.0.1)'
    )
    parser.add_argument(
        '--port', default=8787, type=int,
        help='Server port (default: 8787)'
    )
    
    subparsers = parser.add_subparsers(dest='command', help='Commands')
    
    # Health command
    subparsers.add_parser('health', help='Check server health')
    
    # Refresh command
    refresh_parser = subparsers.add_parser('refresh', help='Refresh assets')
    refresh_parser.add_argument(
        '--force', action='store_true',
        help='Force reimport all assets'
    )
    
    # Compile command
    compile_parser = subparsers.add_parser('compile', help='Wait for compilation')
    compile_parser.add_argument(
        '--timeout', default=120, type=int,
        help='Timeout in seconds'
    )
    
    # Errors command
    errors_parser = subparsers.add_parser('errors', help='Get console errors')
    errors_parser.add_argument(
        '--level', default='error',
        choices=['all', 'error', 'warning', 'log'],
        help='Log level to fetch'
    )
    errors_parser.add_argument(
        '--limit', default=50, type=int,
        help='Maximum number of entries'
    )
    
    # Clear command
    subparsers.add_parser('clear', help='Clear console')
    
    # Build command
    build_parser = subparsers.add_parser('build', help='Build project')
    build_parser.add_argument('platform', help='Target platform')
    build_parser.add_argument('output', help='Output path')
    build_parser.add_argument(
        '--development', action='store_true',
        help='Development build'
    )
    
    # Monitor command
    monitor_parser = subparsers.add_parser('monitor', help='Monitor console')
    monitor_parser.add_argument(
        '--level', default='error',
        choices=['all', 'error', 'warning', 'log'],
        help='Log level to monitor'
    )
    monitor_parser.add_argument(
        '--interval', default=5, type=int,
        help='Refresh interval in seconds'
    )
    
    args = parser.parse_args()
    
    if not args.command:
        parser.print_help()
        return
    
    # Create client
    client = UnityRemoteClient(args.host, args.port)
    
    # Execute command
    if args.command == 'health':
        result = client.health_check()
        if result.get('ok'):
            print(f"✅ Server is healthy")
            print(f"Version: {result.get('version')}")
            print(f"Unity: {result.get('unity')}")
        else:
            print(f"❌ Server error: {result.get('error')}")
            sys.exit(1)
    
    elif args.command == 'refresh':
        print("🔄 Refreshing assets...")
        result = client.refresh_assets(args.force)
        if result.get('ok'):
            print("✅ Assets refreshed")
        else:
            print(f"❌ Error: {result.get('error')}")
            sys.exit(1)
    
    elif args.command == 'compile':
        print(f"⏳ Waiting for compilation (timeout: {args.timeout}s)...")
        result = client.await_compile(args.timeout)
        if result.get('ok'):
            print(f"✅ Compilation complete (duration: {result.get('duration')}s)")
        else:
            print(f"❌ Error: {result.get('error')}")
            sys.exit(1)
    
    elif args.command == 'errors':
        errors = client.get_errors(args.level, args.limit)
        if errors:
            print(f"Found {len(errors)} {args.level} entries:")
            for error in errors:
                print(f"[{error['time']}] {error['type']}: {error['message']}")
        else:
            print(f"No {args.level} entries found")
    
    elif args.command == 'clear':
        result = client.clear_console()
        if result.get('ok'):
            print("✅ Console cleared")
        else:
            print(f"❌ Error: {result.get('error')}")
            sys.exit(1)
    
    elif args.command == 'build':
        print(f"🏗️ Building for {args.platform}...")
        result = client.build(args.platform, args.output, args.development)
        if result.get('ok'):
            print(f"✅ Build complete!")
            print(f"Output: {result.get('outputPath')}")
            print(f"Duration: {result.get('duration')}s")
            print(f"Size: {result.get('size')} bytes")
        else:
            print(f"❌ Build failed: {result.get('error')}")
            sys.exit(1)
    
    elif args.command == 'monitor':
        print(f"👁️ Monitoring {args.level} logs (interval: {args.interval}s)")
        print("Press Ctrl+C to stop")
        print("-" * 40)
        
        seen_messages = set()
        try:
            while True:
                logs = client.get_errors(args.level, 100)
                for log in logs:
                    msg_id = f"{log['time']}_{log['message']}"
                    if msg_id not in seen_messages:
                        seen_messages.add(msg_id)
                        print(f"[{log['time']}] {log['type']}: {log['message']}")
                
                time.sleep(args.interval)
        except KeyboardInterrupt:
            print("\nMonitoring stopped")

if __name__ == '__main__':
    main()