#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.1.4";

Task("Default")
    .IsDependentOn("PinVersion")
    .IsDependentOn("dotnet");

Task("PinVersion")
    .WithCriteria(!BuildSystem.IsLocalBuild)
    .Does(() => {
        foreach (var angel in GetFiles("./src/**/angel.cake")) {
            var content = System.IO.File.ReadAllText(angel.FullPath);
            if (content.IndexOf("{version}") > -1) {
                System.IO.File.WriteAllText(angel.FullPath, content.Replace("{version}", GitVer.NuGetVersion));
            }
        }
    });

RunTarget(Target);
