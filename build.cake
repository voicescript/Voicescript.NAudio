#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

// TODO: port the ZipLib, ZipDemo and ZipRelease tasks from the old fake scripts

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = EnvironmentVariable("PACKAGE_VERSION") ?? "1.0.0";


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./NAudio/bin") + Directory(configuration);

var buildLogo = @"  _   _    _             _ _       
 | \ | |  / \  _   _  __| (_) ___  
 |  \| | / _ \| | | |/ _` | |/ _ \ 
 | |\  |/ ___ \ |_| | (_| | | (_) |
 |_| \_/_/   \_\__,_|\__,_|_|\___/ 
"; 
Information(buildLogo);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./**/bin/" + configuration);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./NAudio.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./NAudio.sln", settings =>
        settings
            .SetConfiguration(configuration)
            .WithProperty("Version", version)
            .WithProperty("AssemblyVersion", version)
            .WithProperty("FileVersion", version)
            .WithProperty("PackageVersion", version));
    }
    else
    {
      // Use XBuild - unlikely to work, not tested
      XBuild("./NAudio.sln", settings =>
        settings
            .SetConfiguration(configuration)
            .WithProperty("Version", version)
            .WithProperty("AssemblyVersion", version)
            .WithProperty("FileVersion", version)
            .WithProperty("PackageVersion", version));
    }
});

Task("Collect-NuGet-Packages")
    .IsDependentOn("Build")
    .Does(() =>
{
    // Директория для собранных пакетов
    var nugetDir = "./artifacts/nuget";
    
    // Создаем директорию, если она не существует
    EnsureDirectoryExists(nugetDir);
    
    // Находим все пакеты .nupkg, созданные во время сборки
    var packages = GetFiles("./**/bin/" + configuration + "/**/*.nupkg");
    
    // Выводим информацию о найденных пакетах
    Information("Found {0} NuGet packages:", packages.Count);
    foreach(var package in packages)
    {
        Information("- {0}", package.GetFilename());
    }
    
    // Копируем все пакеты в директорию artifacts
    CopyFiles(packages, nugetDir);
    
    Information("NuGet packages collected to {0}", nugetDir);
});

Task("Publish-NuGet-Packages")
    .IsDependentOn("Collect-NuGet-Packages")
    .Does(() =>
{
    var apiKey = EnvironmentVariable("NUGET_API_KEY");
    if(string.IsNullOrEmpty(apiKey))
    {
        throw new Exception("NUGET_API_KEY is not set");
    }
    
    var nugetSource = "https://nuget.pkg.github.com/voicescript/index.json";
    Information("TOKEN: {0}", apiKey);
    
    // Публикуем все пакеты
    var packages = GetFiles("./artifacts/nuget/*.nupkg");
    foreach(var package in packages)
    {
        Information("Publishing package: {0}", package.GetFilename());
        
        // Using dotnet nuget push instead of NuGetPush
        var exitCode = StartProcess("dotnet", new ProcessSettings {
            Arguments = new ProcessArgumentBuilder()
                .Append("nuget")
                .Append("push")
                .AppendQuoted(package.FullPath)
                .Append("--api-key")
                .AppendQuoted(apiKey)
                .Append("--source")
                .AppendQuoted(nugetSource)
                .Append("--skip-duplicate")
                .Append("--interactive")
        });
        
        if (exitCode != 0)
        {
            throw new Exception($"Failed to publish package {package.GetFilename()}");
        }
    }    
    Information("Packages publication complete");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Publish-NuGet-Packages");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
