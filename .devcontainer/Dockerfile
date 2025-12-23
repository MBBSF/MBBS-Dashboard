# Lightweight base dev container with common tools and vscode user
FROM mcr.microsoft.com/devcontainers/base:ubuntu

# Install GitHub CLI (apt repo method)
RUN sudo apt-get update && \
    sudo apt-get install -y \
      curl \
      apt-transport-https \
      ca-certificates \
      gnupg && \
    curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | \
      sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg && \
    sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg && \
    echo "deb [arch=$(dpkg --print-architecture) \
      signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] \
      https://cli.github.com/packages stable main" | \
      sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null && \
    sudo apt-get update && \
    sudo apt-get install -y gh


