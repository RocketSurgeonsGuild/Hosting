#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=JetBrains.dotCover.CommandLineTools"
#addin "nuget:?package=Rocket.Surgery.Build.Cake&version=1.0.3"
#load "nuget:?package=Rocket.Surgery.Build.Cake&version=1.0.3";

Task("Default")
    .IsDependentOn("GitVersion")
    .IsDependentOn("CleanArtifacts")
    .IsDependentOn("DotnetCore");

RunTarget(target);
