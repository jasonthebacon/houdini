namespace Houdini

open Fernet
open System.Text
open System.IO
open Microsoft.AspNetCore.Cryptography.KeyDerivation

module StringByteConversion =
    let toBytes (x: string): byte[] = Encoding.Default.GetBytes(x)
    let fromBytes (bs: byte[]): string = Encoding.Default.GetString(bs)

module Key =
    type Key = Key of value: byte[]

    let generate (password: string) : Key =
        let sixteenBytes = [| for _ in 1..16 do yield 0uy |]
        let derivedKey = KeyDerivation.Pbkdf2(password, sixteenBytes, KeyDerivationPrf.HMACSHA256, 100000, 32)
        Key(derivedKey)

    let getValue (Key value) : byte[] = value

    let from (encoded: string) : Key = Key (encoded.UrlSafe64Decode())

    let fromEnv (varName: string) : Key =
        Key(System.Environment.GetEnvironmentVariable(varName).UrlSafe64Decode())

    let toString (Key value) : string = value.UrlSafe64Encode()

    let print k : unit = printfn "%s" (k |> toString)

module Encryption =
    open StringByteConversion
    let encrypt (key: Key.Key) (input: string) : string =
        SimpleFernet.Encrypt(Key.getValue key, input |> toBytes)

module Decryption =
    open StringByteConversion
    let decrypt (key: Key.Key) (input: string) : string =
        let mutable timestamp = System.DateTime()
        SimpleFernet.Decrypt(Key.getValue key, input, &timestamp) |> fromBytes

module Contents =
    type Contents =
        | Encrypted of contents : string
        | Decrypted of contents : string

    let write (path: string) (input: Contents) : unit =
        match input with
        | Encrypted contents -> File.WriteAllText(path, "ENCRYPTED_" + contents)
        | Decrypted contents -> File.WriteAllText(path, contents)

    let read (path: string) : Contents =
        let rawContents = File.ReadAllText(path)
        let isEncrypted (fileContents: string) : bool =
            ((String.length fileContents) >= 10) && (fileContents.[0..9] = "ENCRYPTED_")
        if (isEncrypted rawContents) then
            Encrypted(rawContents.[10..])
        else
            Decrypted(rawContents)

module Action =
    type Action =
        | Encrypt
        | Decrypt

    let private encryptFile (key: Key.Key) (path: string) : unit =
        match (Contents.read path) with
        | Contents.Encrypted _ -> eprintfn "File at %s already encrypted." path
        | Contents.Decrypted contents ->
            printfn "Encrypting file: %s" path
            contents
            |> Encryption.encrypt key
            |> Contents.Encrypted
            |> Contents.write path

    let private decryptFile (key: Key.Key) (path: string) : unit =
        match (Contents.read path) with
        | Contents.Decrypted _ -> eprintfn "File at %s already decrypted." path
        | Contents.Encrypted contents ->
            printfn "Decrypting file: %s" path
            contents
            |> Decryption.decrypt key
            |> Contents.Decrypted
            |> Contents.write path
    
    let perform (action: Action) =
        match action with
        | Encrypt -> encryptFile
        | Decrypt -> decryptFile

module Run =
    open Microsoft.Extensions.FileSystemGlobbing
    open Microsoft.Extensions.FileSystemGlobbing.Abstractions

    let private loadSearchPatterns (patternFilePath: string) : Set<string> =
        let trim (x: string) = x.Trim()
        let nonEmpty (x: string) = not (System.String.IsNullOrEmpty(x))
        if (not (File.Exists(patternFilePath))) then
            eprintfn "Could not find %s file. No files targeted for encryption/decryption." patternFilePath
            Set.empty
        else
            File.ReadAllLines(patternFilePath)
            |> Array.map trim
            |> Array.filter nonEmpty
            |> Set.ofArray

    let private findFilePaths (basePath: string) (searchPatterns: string seq) : Set<string> =
        let matcher = Matcher()
        matcher.AddIncludePatterns(searchPatterns)
        matcher.Execute(DirectoryInfoWrapper(DirectoryInfo(basePath))).Files
        |> Seq.map (fun (fpm: FilePatternMatch) -> fpm.Path)
        |> Set.ofSeq

    let run (action: Action.Action) (patternFilePath: string) (basePath: string) (key: Key.Key) : unit =
        let paths = findFilePaths basePath (loadSearchPatterns patternFilePath)
        for path in paths do (Action.perform action key path)
