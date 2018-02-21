namespace FsConfig

open FsConfig.Core
open System

type EnvConfig =
  static member private configReader : IConfigReader = {
    new IConfigReader with
      member __.GetValue name =
        let v = Environment.GetEnvironmentVariable name
        if v = null then None else Some v
  }
  static member private fieldNameCanonicalizer customPrefix (Separator separator) : FieldNameCanonicalizer = 
    fun prefix name -> 
      let actualPrefix =
        findActualPrefix customPrefix (Separator separator) prefix
      let subStrings =
        fieldNameSubstrings name
        |> Array.map (fun v -> v.ToUpperInvariant())
      String.Join(separator, subStrings)
      |> sprintf "%s%s" actualPrefix
  static member private defaultPrefix = Prefix ""
  static member private defaultSeparator = Separator "_"
  static member private defaultListSeparator = Separator ","
  static member private defaultFieldNameCanonicalizer =
    EnvConfig.fieldNameCanonicalizer EnvConfig.defaultPrefix EnvConfig.defaultSeparator

  static member Get<'T when 'T :> IConvertible> (envVarName : string) = 
    let (prefix, separator, listSeparator) = 
        getPrefixAndSeparators<'T> EnvConfig.defaultPrefix EnvConfig.defaultSeparator EnvConfig.defaultListSeparator
    parse<'T> EnvConfig.configReader EnvConfig.defaultFieldNameCanonicalizer listSeparator envVarName

  static member Get<'T when 'T : not struct> () =
    let (prefix, separator, listSeparator) = 
        getPrefixAndSeparators<'T> EnvConfig.defaultPrefix EnvConfig.defaultSeparator EnvConfig.defaultListSeparator
    let fieldNameCanonicalizer = 
      EnvConfig.fieldNameCanonicalizer prefix separator
    parse<'T> EnvConfig.configReader fieldNameCanonicalizer listSeparator ""

  static member Get<'T when 'T : not struct> (fieldNameCanonicalizer : FieldNameCanonicalizer) =
    let (prefix, separator, listSeparator) = 
        getPrefixAndSeparators<'T> EnvConfig.defaultPrefix EnvConfig.defaultSeparator EnvConfig.defaultListSeparator
    parse<'T> EnvConfig.configReader fieldNameCanonicalizer listSeparator ""
  