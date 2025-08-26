#!/bin/bash

# Create Unity Package for distribution
# This script creates a .unitypackage file from the package contents

set -e

echo "🔧 Creating Unity Package..."

# Check if Unity is available
UNITY_PATH="/Applications/Unity/Hub/Editor/6000.1.11f1/Unity.app/Contents/MacOS/Unity"
if [ ! -f "$UNITY_PATH" ]; then
    echo "❌ Unity not found at: $UNITY_PATH"
    echo "Please update the Unity path in this script"
    exit 1
fi

# Create temporary Unity project for packaging
TEMP_PROJECT="/tmp/UnityRemoteServerPackaging"
PACKAGE_NAME="UnityRemoteServer"
OUTPUT_PATH="$(pwd)/UnityRemoteServer.unitypackage"

echo "📁 Creating temporary Unity project..."
rm -rf "$TEMP_PROJECT"
"$UNITY_PATH" -batchmode -quit -createProject "$TEMP_PROJECT" -logFile /tmp/unity_package_create.log

# Wait for project creation
sleep 3

# Copy package contents to Assets folder
echo "📋 Copying package contents..."
mkdir -p "$TEMP_PROJECT/Assets/$PACKAGE_NAME"

# Copy all package contents
cp -r Editor "$TEMP_PROJECT/Assets/$PACKAGE_NAME/"
cp -r Runtime "$TEMP_PROJECT/Assets/$PACKAGE_NAME/"
cp -r Samples~ "$TEMP_PROJECT/Assets/$PACKAGE_NAME/Samples"
cp -r Documentation~ "$TEMP_PROJECT/Assets/$PACKAGE_NAME/Documentation"

# Copy meta files and other files
cp package.json "$TEMP_PROJECT/Assets/$PACKAGE_NAME/"
cp README.md "$TEMP_PROJECT/Assets/$PACKAGE_NAME/"
cp LICENSE.md "$TEMP_PROJECT/Assets/$PACKAGE_NAME/"
cp CHANGELOG.md "$TEMP_PROJECT/Assets/$PACKAGE_NAME/"
cp -r .github "$TEMP_PROJECT/Assets/$PACKAGE_NAME/" 2>/dev/null || true

# Copy assembly definitions
cp *.asmdef "$TEMP_PROJECT/Assets/$PACKAGE_NAME/" 2>/dev/null || true

# Create the .unitypackage
echo "📦 Exporting Unity package..."
"$UNITY_PATH" -batchmode -quit \
    -projectPath "$TEMP_PROJECT" \
    -exportPackage "Assets/$PACKAGE_NAME" "$OUTPUT_PATH" \
    -logFile /tmp/unity_package_export.log

# Check if package was created
if [ -f "$OUTPUT_PATH" ]; then
    echo "✅ Unity package created: $OUTPUT_PATH"
    ls -lh "$OUTPUT_PATH"
else
    echo "❌ Failed to create Unity package"
    echo "Check log at: /tmp/unity_package_export.log"
    cat /tmp/unity_package_export.log
    exit 1
fi

# Clean up
echo "🧹 Cleaning up temporary project..."
rm -rf "$TEMP_PROJECT"

echo "🎉 Unity package creation complete!"