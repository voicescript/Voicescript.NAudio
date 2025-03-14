# Publishing NAudio Packages to GitHub NuGet Registry

This document explains how to publish NAudio packages to the GitHub NuGet registry using GitHub Actions.

## Setup

1. The repository has been configured with a GitHub Actions workflow that can automatically build and publish NAudio packages to the GitHub NuGet registry.

2. The workflow is defined in `.github/workflows/publish-nuget.yml`.

## Version Management

Package versions are managed through a central `version.txt` file in the root of the repository. This file contains a single line with the version number (e.g., `1.0.0`).

To update the version:

1. Edit the `version.txt` file
2. Change the version number following [Semantic Versioning](https://semver.org/) principles:
   - Increment the MAJOR version when you make incompatible API changes
   - Increment the MINOR version when you add functionality in a backward compatible manner
   - Increment the PATCH version when you make backward compatible bug fixes

Both the GitHub Actions workflow and the local publishing script will read this file and use the version when building and packing the NuGet packages.

## Required Secrets

Before you can use the workflow, you need to set up the following secret in your GitHub repository:

- `TOKEN`: A GitHub Personal Access Token (PAT) with the `write:packages` scope.

To add this secret:

1. Go to your GitHub repository
2. Click on "Settings"
3. Click on "Secrets and variables" > "Actions"
4. Click on "New repository secret"
5. Name: `TOKEN`
6. Value: Your GitHub Personal Access Token
7. Click "Add secret"

## Creating a Personal Access Token (PAT)

1. Go to your GitHub account settings
2. Click on "Developer settings"
3. Click on "Personal access tokens" > "Tokens (classic)"
4. Click "Generate new token" > "Generate new token (classic)"
5. Give your token a descriptive name
6. Select the `write:packages` scope
7. Click "Generate token"
8. Copy the token value (you won't be able to see it again)

## Triggering the Workflow

The workflow can be triggered in two ways:

1. **Automatically** when a new release is published in the repository
2. **Manually** using the "workflow_dispatch" event

### Manual Trigger

To manually trigger the workflow:

1. Go to your GitHub repository
2. Click on "Actions"
3. Select "Publish NuGet Packages to GitHub Registry" from the workflows list
4. Click "Run workflow"
5. Select the branch you want to run the workflow on
6. Click "Run workflow"

## Using the Published Packages

To use the packages published to the GitHub NuGet registry in your projects:

1. Add the GitHub NuGet registry as a source in your NuGet.config file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/voicescript/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_PAT" />
    </github>
  </packageSourceCredentials>
</configuration>
```

2. Install the packages using NuGet Package Manager or the .NET CLI:

```
dotnet add package NAudio
```

## Local Publishing

You can also use the modified `publish.ps1` script to publish packages locally:

1. Set the `$apiKey` variable to your GitHub Personal Access Token:

```powershell
$apiKey = "YOUR_GITHUB_PAT"
```

2. Run the script:

```powershell
.\publish.ps1
```

This will build, pack, and publish all NAudio packages to the GitHub NuGet registry using the version specified in `version.txt`. 