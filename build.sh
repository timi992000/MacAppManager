#!/bin/bash
set -e

APP_NAME="AppManager"
PROJECT_NAME="AppManager"
OS=$(uname -s)

if [ "$OS" = "Darwin" ]; then
    APP_BUNDLE="${APP_NAME}.app"
    MACOS_DIR="${APP_BUNDLE}/Contents/MacOS"
    DMG_NAME="${APP_NAME}.dmg"

    if [ "$(uname -m)" = "arm64" ]; then
        RUNTIME="osx-arm64"
        echo "🍏 Apple Silicon detected."
    else
        RUNTIME="osx-x64"
        echo "💻 Intel processor detected."
    fi

    echo "🚀 Building $APP_NAME for $RUNTIME..."
    rm -rf "$APP_BUNDLE" "$DMG_NAME"

    dotnet publish -c Release -r $RUNTIME --self-contained true -p:PublishSingleFile=true -o "$MACOS_DIR"
    find "$MACOS_DIR" -name "*.pdb" -type f -delete

    RESOURCES_DIR="${APP_BUNDLE}/Contents/Resources"
    mkdir -p "$RESOURCES_DIR"
    cp assets/AppIcon.icns "$RESOURCES_DIR/AppIcon.icns"

    cat > "${APP_BUNDLE}/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>$PROJECT_NAME</string>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

    chmod +x "$MACOS_DIR/$PROJECT_NAME"
    echo "✅ App bundle: $(pwd)/$APP_BUNDLE"

    echo "📦 Creating DMG installer..."
    if command -v create-dmg &>/dev/null; then
        create-dmg \
            --volname "$APP_NAME" \
            --window-pos 200 120 \
            --window-size 600 400 \
            --icon-size 100 \
            --icon "$APP_BUNDLE" 150 200 \
            --app-drop-link 450 200 \
            --hide-extension "$APP_BUNDLE" \
            --no-internet-enable \
            "$DMG_NAME" \
            "$APP_BUNDLE"
    else
        echo "  ℹ️  create-dmg not found — basic DMG (run 'brew install create-dmg' for styled installer)"
        STAGING=$(mktemp -d)
        cp -R "$APP_BUNDLE" "$STAGING/"
        ln -s /Applications "$STAGING/Applications"
        hdiutil create -volname "$APP_NAME" -srcfolder "$STAGING" -ov -format UDZO "$DMG_NAME" >/dev/null
        rm -rf "$STAGING"
    fi
    echo "✅ DMG installer: $(pwd)/$DMG_NAME"

elif [ "$OS" = "Linux" ]; then
    if [ "$(uname -m)" = "aarch64" ]; then
        RUNTIME="linux-arm64"
        echo "🐧 Linux ARM64 detected."
    else
        RUNTIME="linux-x64"
        echo "🐧 Linux x64 detected."
    fi

    OUT_DIR="dist"
    echo "🚀 Building $APP_NAME for $RUNTIME..."
    rm -rf "$OUT_DIR"

    dotnet publish -c Release -r $RUNTIME --self-contained true -p:PublishSingleFile=true -o "$OUT_DIR"
    find "$OUT_DIR" -name "*.pdb" -type f -delete
    chmod +x "$OUT_DIR/$PROJECT_NAME"

    echo "✅ Binary: $(pwd)/$OUT_DIR/$PROJECT_NAME"
else
    echo "❌ Unsupported OS: $OS. Use build.ps1 on Windows."
    exit 1
fi
