#!/usr/bin/env bash
set -e

echo "Setting up Git configuration..."

# Configure Git to use credential helper for HTTPS authentication
git config --global credential.helper 'store --file=/tmp/git-credentials'
git config --global init.defaultBranch main

# Check if we have SSH keys (optional for SSH users)
if [ -d "$HOME/.ssh" ] && [ "$(ls -A $HOME/.ssh)" ]; then
  echo "SSH keys found - configuring SSH permissions..."
  sudo chown -R vscode:vscode "$HOME/.ssh"
  chmod 700 "$HOME/.ssh"
  chmod 600 "$HOME/.ssh"/id_* 2>/dev/null || true
  chmod 644 "$HOME/.ssh"/id_*.pub 2>/dev/null || true
  echo "SSH configuration complete."
else
  echo "No SSH keys found - developers can use HTTPS with personal access tokens."
  mkdir -p "$HOME/.ssh"
  sudo chown vscode:vscode "$HOME/.ssh"
  chmod 700 "$HOME/.ssh"
fi

echo "Git configuration complete. Developers can use either:"
echo "  • HTTPS: https://github.com/MBBSF/MBBS-Dashboard.git (use personal access token)"
echo "  • SSH: git@github.com:MBBSF/MBBS-Dashboard.git (requires SSH keys)"

echo "Verifying .NET 8 SDK installation..."
DOTNET_VERSION=$(dotnet --version)
echo ".NET SDK version: $DOTNET_VERSION"
if [[ ! "$DOTNET_VERSION" =~ ^8\. ]]; then
  echo "WARNING: Expected .NET 8.x SDK, but found $DOTNET_VERSION"
fi

echo "Ensuring npm is on PATH..."
export PATH="/usr/local/share/nvm/current/bin:$PATH"

echo "Installing TypeScript..."

# Ensure npm is available (it will be after Node feature)
npm install -g typescript

echo "TypeScript installation complete."

echo "Installing .NET EF Core tools..."

# Clear any corrupted tool cache
dotnet tool uninstall --global dotnet-ef 2>/dev/null || true

# Clear NuGet cache to avoid corrupted packages
dotnet nuget locals all --clear

# Install EF Core tools with specific version to avoid corruption
dotnet tool install --global dotnet-ef --version 8.0.11

echo ".NET EF Core tools installation complete."

echo "Installing uv..."
curl -LsSf https://astral.sh/uv/install.sh | sh

# Add uv to PATH immediately
export PATH="$HOME/.local/bin:$PATH"

echo "Installing Spec Kit CLI..."
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git

echo "Spec Kit installation complete."