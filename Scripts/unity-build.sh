#!/bin/bash

# Unity Remote Server - Build Script
# Automates Unity project build process via Remote Server API

# Configuration
SERVER="${UNITY_REMOTE_SERVER:-http://127.0.0.1:8787}"
PLATFORM="${1:-android}"
OUTPUT="${2:-Builds/game.apk}"
TIMEOUT="${UNITY_BUILD_TIMEOUT:-300}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Unity Remote Server - Build Script${NC}"
echo "Server: $SERVER"
echo "Platform: $PLATFORM"
echo "Output: $OUTPUT"
echo ""

# Check server health
echo -e "${YELLOW}🔍 Checking server status...${NC}"
if ! curl -s "$SERVER/health" > /dev/null; then
    echo -e "${RED}❌ Unity Remote Server is not responding${NC}"
    echo "Please ensure Unity Editor is running with Remote Server package installed"
    exit 1
fi
echo -e "${GREEN}✅ Server is online${NC}"

# Refresh assets
echo -e "${YELLOW}🔄 Refreshing assets...${NC}"
curl -X POST "$SERVER/refresh" \
    -H "Content-Type: application/json" \
    -d '{"force": true}' \
    -s > /dev/null
echo -e "${GREEN}✅ Assets refreshed${NC}"

# Wait for compilation
echo -e "${YELLOW}⏳ Waiting for compilation...${NC}"
COMPILE_RESULT=$(curl -X POST "$SERVER/awaitCompile" \
    -H "Content-Type: application/json" \
    -d "{\"timeoutSec\": $TIMEOUT}" \
    -s)

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Compilation wait failed${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Compilation complete${NC}"

# Check for errors
echo -e "${YELLOW}🔍 Checking for compilation errors...${NC}"
ERRORS=$(curl -s "$SERVER/errors?level=error")
ERROR_COUNT=$(echo "$ERRORS" | jq '. | length')

if [ "$ERROR_COUNT" -gt 0 ]; then
    echo -e "${RED}❌ Found $ERROR_COUNT compilation errors:${NC}"
    echo "$ERRORS" | jq -r '.[] | "  - \(.message)"'
    exit 1
fi
echo -e "${GREEN}✅ No compilation errors${NC}"

# Build project
echo -e "${YELLOW}🏗️ Building project for $PLATFORM...${NC}"
BUILD_RESULT=$(curl -X POST "$SERVER/build" \
    -H "Content-Type: application/json" \
    -d "{\"platform\": \"$PLATFORM\", \"outputPath\": \"$OUTPUT\"}" \
    -s)

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Build request failed${NC}"
    exit 1
fi

# Check build result
if echo "$BUILD_RESULT" | jq -e '.ok' > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Build completed successfully!${NC}"
    echo "Output: $(echo "$BUILD_RESULT" | jq -r '.outputPath')"
    echo "Duration: $(echo "$BUILD_RESULT" | jq -r '.duration')s"
    echo "Size: $(echo "$BUILD_RESULT" | jq -r '.size') bytes"
else
    echo -e "${RED}❌ Build failed${NC}"
    echo "$BUILD_RESULT" | jq -r '.error'
    exit 1
fi