module Setup

open System
open System.IO
open System.Diagnostics

open Config
open Houdini

let private promptYN (question: string) : bool =
    Console.Write(question + " (Y/n): ")
    let input = Console.ReadLine()
    (input.ToLower() = "yes" || input.ToLower() = "y")

let private prompt (msg: string): string =
    Console.Write(msg + ": "); Console.ReadLine()

let private alias (config: Config) (houdiniPath: string) (key: string option) : unit =
    let keySuffix =
        match key with
        | Some k -> " '" + k + "'"
        | None -> ""
    let timeoutMs = 5000
    Process.Start("git", @"config --global alias.hide ""!dotnet '" + houdiniPath + @"' encrypt" + keySuffix + @"""").WaitForExit(timeoutMs) |> ignore
    Process.Start("git", @"config --global alias.reveal ""!dotnet '" + houdiniPath + @"' decrypt" + keySuffix + @"""").WaitForExit(timeoutMs) |> ignore
    Process.Start("git", @"config --global alias.initEncrypt ""!dotnet '" + houdiniPath + @"' initialize""").WaitForExit(timeoutMs) |> ignore
    Console.WriteLine(@"
Git aliases set -- you can now use

- `git hide`            to encrypt sensitive files in {0}
- `git reveal`          to decrypt sensitive files in {0}
- `git initEncrypt`     to create a {0} file in the current directory
", config.patternFilename)

let private unalias () : unit =
    let timeoutMs = 5000
    Process.Start("git", @"config --global --unset alias.hide").WaitForExit(timeoutMs) |> ignore
    Process.Start("git", @"config --global --unset alias.reveal").WaitForExit(timeoutMs) |> ignore
    Process.Start("git", @"config --global --unset alias.initEncrypt").WaitForExit(timeoutMs) |> ignore

let private getHoudiniPath () : string =
    let cwd = System.Environment.CurrentDirectory
    let validate (fullPath: string) = if (File.Exists(fullPath)) then fullPath else failwithf "Houdini not found at %s" fullPath
    if (promptYN ("Does this directory contain Houdini: " + cwd + " ?")) then
        Path.Combine(cwd, "Houdini.dll") |> validate
    else
        let path = prompt "Enter the path to the directory *containing* Houdini (do NOT use quote marks as delimiters)"
        Path.Combine(path, "Houdini.dll") |> validate

let private tryGetKey () : string option =
    if (promptYN "Do you have a key you want to use?") then 
        Some (prompt "Enter your URL-safe Base64-encoded key")
    elif (promptYN "Do you want to generate a key?") then
        prompt("Enter password for key generation") |> Key.generate |> Key.toString |> Some
    else
        None
        
let private checkEnvForKey (envVarName: string) : string option =
    let envKey = System.Environment.GetEnvironmentVariable(envVarName)
    if (String.IsNullOrEmpty(envKey)) then 
        None 
    else 
        printfn "Found a key in the %s environment variable: %s" envVarName envKey
        Some envKey

let unsetup () : unit =
    if (promptYN "Really un-setup Houdini?") then 
        printfn "Unsetting git aliases..."
        unalias()
        printfn "Unsetup complete."
    else
        ()

let setup (config: Config) : unit =
    let houdiniPath = getHoudiniPath()
    let keyFromEnv =
        match (checkEnvForKey(config.keyEnvironmentVariableName)) with
        | Some key -> if (promptYN ("Do you want to use this as your key? " + key + " ")) then Some key else None
        | _ -> None
    let retrievedKey = if (Option.isSome keyFromEnv) then keyFromEnv else tryGetKey()
    let keyOpt =
        match (retrievedKey) with
        | Some key ->
            printfn "Your URL-safe Base64-encoded key: %s" key
            if (promptYN ("Do you want to put this key in your git aliases?\nNote that anyone with access to your global .gitconfig will be able to see it!")) then
                retrievedKey
            else
                printfn "Okay. You can always set the %s environment variable with your key, and Houdini will read it from there." config.keyEnvironmentVariableName
                None
        | _ ->
            printfn "No key provided to setup. You can always set the %s environment variable with your key, and Houdini will read it from there." config.keyEnvironmentVariableName
            retrievedKey
    printfn "Setting git aliases..."
    alias config houdiniPath keyOpt
    printfn "Setup complete."
