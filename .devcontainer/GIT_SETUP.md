# Git Configuration for Development

This project supports both SSH and HTTPS Git authentication methods. Choose the approach that works best for you.

## Option 1: HTTPS Authentication (Recommended for new users)

### Setup
1. **Repository URL**: `https://github.com/MBBSF/MBBS-Dashboard.git`
2. **Authentication**: Personal Access Token (PAT)

### Create Personal Access Token
1. Go to GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Click "Generate new token (classic)"
3. Set expiration and select scopes:
   - ✅ `repo` (Full control of private repositories)
   - ✅ `workflow` (Update GitHub Action workflows)
4. Copy the token (you won't see it again!)

### Using the Token
When prompted for credentials:
- **Username**: Your GitHub username
- **Password**: Your personal access token (not your GitHub password)

### First-time Setup
```bash
git clone https://github.com/MBBSF/MBBS-Dashboard.git
cd MBBS-Dashboard

# Configure your identity
git config user.name "Your Name"
git config user.email "your.email@example.com"

# Store credentials (in devcontainer)
git config credential.helper 'store --file=/tmp/git-credentials'
```

### Daily Usage
```bash
# Normal git commands work
git pull origin main
git push origin feature/your-branch

# First push will ask for username/token, then it's stored
```

## Option 2: SSH Authentication (For advanced users)

### Setup
1. **Repository URL**: `git@github.com:MBBSF/MBBS-Dashboard.git`
2. **Authentication**: SSH Keys

### SSH Key Setup
```bash
# Generate SSH key
ssh-keygen -t ed25519 -C "your.email@example.com"

# Copy public key
cat ~/.ssh/id_ed25519.pub
```

1. Copy the public key output
2. Go to GitHub → Settings → SSH and GPG keys
3. Click "New SSH key" and paste your public key

### Change Remote to SSH (if needed)
```bash
git remote set-url origin git@github.com:MBBSF/MBBS-Dashboard.git
```

## DevContainer Configuration

The devcontainer supports both approaches:

### HTTPS Users
- No additional setup needed
- Credentials are stored in `/tmp/git-credentials` (container-safe)
- Personal access tokens work seamlessly

### SSH Users  
- SSH keys are persisted in `devcontainer-ssh` volume
- Keys are automatically configured with correct permissions
- Works across container rebuilds

## Troubleshooting

### HTTPS Issues
```bash
# Clear stored credentials
rm /tmp/git-credentials
git config --unset credential.helper

# Reconfigure
git config credential.helper 'store --file=/tmp/git-credentials'
```

### SSH Issues
```bash
# Test SSH connection
ssh -T git@github.com

# Check SSH key permissions
ls -la ~/.ssh/
```

### Switch Between Methods
```bash
# Switch to HTTPS
git remote set-url origin https://github.com/MBBSF/MBBS-Dashboard.git

# Switch to SSH
git remote set-url origin git@github.com:MBBSF/MBBS-Dashboard.git
```

## Recommendation

**For most developers**: Use HTTPS with Personal Access Tokens
- ✅ Easier setup
- ✅ Works on all platforms
- ✅ No SSH key management
- ✅ Better for temporary access

**For power users**: Use SSH keys if you prefer
- ✅ More secure long-term
- ✅ No token expiration concerns
- ✅ Faster authentication