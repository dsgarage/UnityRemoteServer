#!/bin/bash

# Create Unity Package structure for manual import
# This creates a properly structured archive that can be imported as a Unity package

set -e

echo "📦 Creating Unity Package Archive..."

VERSION="v1.0.0"
PACKAGE_DIR="UnityRemoteServer"
OUTPUT_NAME="UnityRemoteServer-${VERSION}.unitypackage"

# Create clean package structure
rm -rf /tmp/unity-package-temp
mkdir -p /tmp/unity-package-temp/$PACKAGE_DIR

# Copy all necessary files with proper structure
echo "📋 Copying package files..."

# Editor scripts
mkdir -p /tmp/unity-package-temp/$PACKAGE_DIR/Editor
cp -r Editor/* /tmp/unity-package-temp/$PACKAGE_DIR/Editor/

# Samples
if [ -d "Samples~" ]; then
    mkdir -p /tmp/unity-package-temp/$PACKAGE_DIR/Samples
    cp -r Samples~/* /tmp/unity-package-temp/$PACKAGE_DIR/Samples/
fi

# Documentation
if [ -d "Documentation~" ]; then
    mkdir -p /tmp/unity-package-temp/$PACKAGE_DIR/Documentation
    cp -r Documentation~/* /tmp/unity-package-temp/$PACKAGE_DIR/Documentation/
fi

# Package files
cp package.json /tmp/unity-package-temp/$PACKAGE_DIR/
cp README.md /tmp/unity-package-temp/$PACKAGE_DIR/
cp LICENSE.md /tmp/unity-package-temp/$PACKAGE_DIR/
cp CHANGELOG.md /tmp/unity-package-temp/$PACKAGE_DIR/

# Assembly definitions
cp *.asmdef /tmp/unity-package-temp/$PACKAGE_DIR/ 2>/dev/null || true
cp *.asmdef.meta /tmp/unity-package-temp/$PACKAGE_DIR/ 2>/dev/null || true

# Create .meta files for Unity
echo "📝 Creating Unity meta files..."

cat > /tmp/unity-package-temp/$PACKAGE_DIR.meta << 'EOF'
fileFormatVersion: 2
guid: f0e1d2c3b4a5968789012345678901234
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
EOF

cat > /tmp/unity-package-temp/$PACKAGE_DIR/Editor.meta << 'EOF'
fileFormatVersion: 2
guid: a1b2c3d4e5f6789012345678901234567
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
EOF

cat > /tmp/unity-package-temp/$PACKAGE_DIR/package.json.meta << 'EOF'
fileFormatVersion: 2
guid: b2c3d4e5f6789012345678901234567890
TextScriptImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
EOF

# Create tarball in Unity package format
echo "🎁 Creating package archive..."
cd /tmp/unity-package-temp
tar -czf "$OUTPUT_NAME" $PACKAGE_DIR

# Move to output directory
mv "$OUTPUT_NAME" /tmp/UnityRemoteServer/

# Also create a standard Unity-importable ZIP
cd /tmp/UnityRemoteServer
rm -f "UnityRemoteServer-${VERSION}-Unity.zip"

# Create temp directory for zip
mkdir -p /tmp/unity-zip-temp
cp -r Editor /tmp/unity-zip-temp/
cp -r Samples~ /tmp/unity-zip-temp/
cp -r Documentation~ /tmp/unity-zip-temp/
cp package.json /tmp/unity-zip-temp/
cp package.json.meta /tmp/unity-zip-temp/
cp README.md /tmp/unity-zip-temp/
cp LICENSE.md /tmp/unity-zip-temp/
cp CHANGELOG.md /tmp/unity-zip-temp/
cp DSGarage.UnityRemoteServer.Editor.asmdef* /tmp/unity-zip-temp/ 2>/dev/null || true

cd /tmp/unity-zip-temp
zip -r "/tmp/UnityRemoteServer/UnityRemoteServer-${VERSION}-Unity.zip" .
cd /tmp/UnityRemoteServer

echo "✅ Package archives created:"
ls -lh /tmp/UnityRemoteServer/UnityRemoteServer-*.unitypackage 2>/dev/null || true
ls -lh /tmp/UnityRemoteServer/UnityRemoteServer-*Unity.zip

# Clean up
rm -rf /tmp/unity-package-temp
rm -rf /tmp/unity-zip-temp

echo "🎉 Unity package creation complete!"
echo ""
echo "Import instructions:"
echo "1. Download UnityRemoteServer-${VERSION}-Unity.zip"
echo "2. Extract to your project's Assets folder"
echo "3. Or use the .unitypackage file for direct import"