// Learn more about F# at http://fsharp.org

open System
open System.IO
open Houdini
open Config

type Mode =
    | Initialize
    | Encrypt of key: Key.Key
    | Decrypt of key: Key.Key
    | Generate of password: string
    | Setup
    | Unsetup
    | Help

let private getPatternFilePath (config: Config) : string = Path.Combine(Directory.GetCurrentDirectory(), config.patternFilename)

let private getEncrypt (config: Config) (args: string[]) : Mode =
    let key = if ((Array.length args) < 2) then Key.fromEnv(config.keyEnvironmentVariableName) else Key.from(args.[1])
    Encrypt(key)

let private getDecrypt (config: Config) (args: string[]) : Mode =
    let key = if ((Array.length args) < 2) then Key.fromEnv(config.keyEnvironmentVariableName) else Key.from(args.[1])
    Decrypt(key)

let private getGenerate (args: string[]) : Mode =
    let password = 
        if ((Array.length args) < 2) then 
            Console.Write("Enter password for key generation: ")
            Console.ReadLine()
        else 
            args.[1]
    Generate(password)

let private discernMode (config: Config) (args: string[]) : Mode =
    if ((Array.length args) < 1 || String.IsNullOrEmpty(args.[0])) then Help else
    let modeString = args.[0].ToLower()
    if "initialize".StartsWith(modeString) then Initialize
    elif "encrypt".StartsWith(modeString) then getEncrypt config args
    elif "decrypt".StartsWith(modeString) then getDecrypt config args
    elif "generate".StartsWith(modeString) then getGenerate args
    elif "setup".StartsWith(modeString) then Setup
    elif "unsetup".StartsWith(modeString) then Unsetup
    elif "help".StartsWith(modeString) then Help
    else
        eprintfn "Unrecognized command: %s" args.[0]
        Help

let getHelpText (config: Config) : string = String.Format(@"
Valid commands:

    initialize              -- Creates a {0} file in the current directory.
    encrypt [key]           -- Encrypts files specified in the current directory's {0} file using [key].
    decrypt [key]           -- Decrypts files specified in the current directory's {0} file using [key].
    generate [password]     -- Generates a new key using [password] and prints it to STDOUT.
    setup                   -- Starts interactive setup for git aliases.
    unsetup                 -- Starts interactive un-setup for git aliases.
    help                    -- Prints this help information.

* If [key] is not provided for encrypt/decrypt, Houdini will extract a key from the {1} environment variable.
* If [password] is not provided for password generation, Houdini will prompt for one.
* Commands are not case-sensitive (e.g. `INITIALIZE` is equivalent to `initialize`).
* All but the first letter of the command word can be omitted (e.g. `init` is equivalent to `initialize`).
* Setup should be run from the directory containing the Houdini binaries. If it is run anywhere else, you will have to provide the path to the directory containing Houdini. The `git` executable must be in your PATH to run Setup.
* The filename {0} and environment variable name {1} are configurable via the Houdini.ini file (in the directory where the Houdini binaries are installed).

", config.patternFilename, config.keyEnvironmentVariableName)

let private handleInitialization (patternFilePath: string) : unit =
    if (File.Exists(patternFilePath)) then
        eprintfn "File already exists at %s" patternFilePath
    else
        File.Create(patternFilePath) |> ignore
        printfn "File created at %s" patternFilePath

let private handleMode (config: Config) (mode: Mode) : unit =
    let currentWorkingDirectory = Directory.GetCurrentDirectory()
    let patternFilePath = getPatternFilePath config
    match mode with
    | Initialize -> handleInitialization(patternFilePath)
    | Encrypt key -> Run.run Action.Encrypt patternFilePath currentWorkingDirectory key
    | Decrypt key -> Run.run Action.Decrypt patternFilePath currentWorkingDirectory key
    | Generate password -> Key.generate(password) |> Key.print
    | Setup -> Setup.setup config
    | Unsetup -> Setup.unsetup()
    | Help -> printfn "Your current working directory is %s.\n%s" currentWorkingDirectory (getHelpText config)

[<EntryPoint>]
let main args =
    let config = getConfig()
    args 
    |> discernMode config
    |> handleMode config
    0
