open System
open System.Diagnostics

let targets = [
    "win-x64";
    "osx-x64";
    "linux-x64";
]
let publishTarget (target: string) = 
    Process.Start("dotnet", "publish --configuration Release --runtime " + target + " --self-contained false -o publish/" + target).WaitForExit()

for target in targets do (publishTarget target)