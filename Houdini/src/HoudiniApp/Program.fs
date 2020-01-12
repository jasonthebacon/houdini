// Learn more about F# at http://fsharp.org

open System
open System.IO
open Houdini

type Mode =
    | Initialize
    | Encrypt of key: Key.Key
    | Decrypt of key: Key.Key
    | Generate of password: string
    | Help

let PATTERN_FILENAME = ".houdini"
let getPatternFilePath () : string = Path.Combine(Directory.GetCurrentDirectory(), PATTERN_FILENAME)

let KEY_ENV_VAR_NAME = "ENCRYPTION_TOOLS_KEY"

let getEncrypt (args: string[]) : Mode =
    let key = if ((Array.length args) < 2) then Key.fromEnv(KEY_ENV_VAR_NAME) else Key.from(args.[1])
    Encrypt(key)

let getDecrypt (args: string[]) : Mode =
    let key = if ((Array.length args) < 2) then Key.fromEnv(KEY_ENV_VAR_NAME) else Key.from(args.[1])
    Decrypt(key)

let getGenerate (args: string[]) : Mode =
    let password = 
        if ((Array.length args) < 2) then 
            Console.Write("Enter password for key generation: ")
            Console.ReadLine()
        else 
            args.[1]
    Generate(password)

let discernMode (args: string[]) : Mode =
    if ((Array.length args) < 1 || String.IsNullOrEmpty(args.[0])) then Help else
    let modeString = args.[0].ToLower()
    if "initialize".StartsWith(modeString) then Initialize
    elif "encrypt".StartsWith(modeString) then getEncrypt(args)
    elif "decrypt".StartsWith(modeString) then getDecrypt(args)
    elif "generate".StartsWith(modeString) then getGenerate(args)
    elif "help".StartsWith(modeString) then Help
    else
        eprintfn "Unrecognized command: %s" args.[0]
        Help

let HELP_TEXT = @"
Valid commands:

    initialize              -- Creates a " + PATTERN_FILENAME + @" file in the current directory.
    encrypt [key]           -- Encrypts files specified in the current directory's " + PATTERN_FILENAME + @" file using [key].
    decrypt [key]           -- Decrypts files specified in the current directory's " + PATTERN_FILENAME + @" file using [key].
    generate [password]     -- Generates a new key using [password] and prints it to STDOUT.
    help                    -- Prints this help information.

* If [key] is not provided for encrypt/decrypt, Houdini will extract a key from the " + KEY_ENV_VAR_NAME + @" environment variable.
* If [password] is not provided for password generation, Houdini will prompt for one.
* Commands are not case-sensitive (e.g. `INITIALIZE` is equivalent to `initialize`).
* All but the first letter of the command word can be omitted (e.g. `init` is equivalent to `initialize`).

"

let handleInitialization (patternFilePath: string) : unit =
    if (File.Exists(patternFilePath)) then
        eprintfn "%s file already exists at %s" PATTERN_FILENAME patternFilePath
    else
        File.Create(patternFilePath) |> ignore
        printfn "%s file created at %s" PATTERN_FILENAME patternFilePath

let handleMode (mode: Mode) : unit =
    let currentWorkingDirectory = Directory.GetCurrentDirectory()
    let patternFilePath = getPatternFilePath()
    match mode with
    | Initialize -> handleInitialization(patternFilePath)
    | Encrypt key -> Run.run Action.Encrypt patternFilePath currentWorkingDirectory key
    | Decrypt key -> Run.run Action.Decrypt patternFilePath currentWorkingDirectory key
    | Generate password -> Key.generate(password) |> Key.print
    | Help -> printfn "Your current working directory is %s.\n%s" currentWorkingDirectory HELP_TEXT

[<EntryPoint>]
let main args =
    args 
    |> discernMode 
    |> handleMode
    0
