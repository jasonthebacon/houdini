module Config

open System
open Microsoft.Extensions.Configuration

type Config = {
    keyEnvironmentVariableName : string
    patternFilename : string
}

let getConfig() : Config =
    let iniConfig = ConfigurationBuilder().AddIniFile("Houdini.ini", true).Build()
    let defaultTo (def: string) (x: string) : string = if (String.IsNullOrEmpty(x)) then def else x
    {
        keyEnvironmentVariableName = iniConfig.["key_environment_variable_name"] |> defaultTo "HOUDINI_KEY"
        patternFilename = iniConfig.["pattern_filename"] |> defaultTo ".houdini"
    }
