#!/bin/bash

# Unity Remote Server - Monitor Script
# Continuously monitors Unity console for errors and warnings

# Configuration
SERVER="${UNITY_REMOTE_SERVER:-http://127.0.0.1:8787}"
INTERVAL="${MONITOR_INTERVAL:-5}"
LOG_LEVEL="${1:-error}"

# Colors
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${GREEN}Unity Remote Server - Console Monitor${NC}"
echo "Server: $SERVER"
echo "Log Level: $LOG_LEVEL"
echo "Refresh Interval: ${INTERVAL}s"
echo ""
echo "Press Ctrl+C to stop monitoring"
echo "----------------------------------------"

# Track seen messages to avoid duplicates
SEEN_FILE="/tmp/unity-monitor-seen.txt"
> "$SEEN_FILE"

while true; do
    # Fetch errors
    LOGS=$(curl -s "$SERVER/errors?level=$LOG_LEVEL" 2>/dev/null)
    
    if [ $? -eq 0 ]; then
        # Process each log entry
        echo "$LOGS" | jq -r '.[] | "\(.time)|\(.type)|\(.message)"' 2>/dev/null | while IFS='|' read -r time type message; do
            # Create unique ID for this message
            MSG_ID=$(echo "$time$type$message" | md5sum | cut -d' ' -f1)
            
            # Check if we've seen this message
            if ! grep -q "$MSG_ID" "$SEEN_FILE"; then
                echo "$MSG_ID" >> "$SEEN_FILE"
                
                # Color based on type
                case "$type" in
                    "Error"|"Exception")
                        echo -e "${RED}[$time] ERROR: $message${NC}"
                        ;;
                    "Warning")
                        echo -e "${YELLOW}[$time] WARN: $message${NC}"
                        ;;
                    "Log")
                        echo -e "${BLUE}[$time] INFO: $message${NC}"
                        ;;
                    *)
                        echo "[$time] $type: $message"
                        ;;
                esac
            fi
        done
    else
        echo -e "${RED}Failed to connect to Unity Remote Server${NC}"
    fi
    
    sleep "$INTERVAL"
done