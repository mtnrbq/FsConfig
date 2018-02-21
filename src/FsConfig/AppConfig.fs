namespace FsConfig

open FsConfig.Core
open System.Configuration
open System

type AppConfig =
  static member private configReader : IConfigReader = {
    new IConfigReader with
      member __.GetValue name =
        let v = ConfigurationManager.AppSettings.Get name
        if v = null then None else Some v
  }

  static member private defaultPrefix = Prefix ""
  static member private defaultSeparator = Separator ""
  static member private defaultListSeparator = Separator ","
  static member private fieldNameCanonicalizer customPrefix (Separator separator) : FieldNameCanonicalizer = 
    fun prefix name -> 
      let actualPrefix =
        findActualPrefix customPrefix (Separator separator) prefix
      String.Join(separator, (fieldNameSubstrings name))
      |> sprintf "%s%s" actualPrefix

  static member private defaultFieldNameCanonicalizer =
    AppConfig.fieldNameCanonicalizer AppConfig.defaultPrefix AppConfig.defaultSeparator


  static member Get<'T when 'T :> IConvertible> (appSettingsName : string) = 
    let (prefix, separator, listSeparator) = 
        getPrefixAndSeparators<'T> AppConfig.defaultPrefix AppConfig.defaultSeparator AppConfig.defaultListSeparator
    parse<'T> AppConfig.configReader AppConfig.defaultFieldNameCanonicalizer listSeparator appSettingsName

  static member Get<'T when 'T : not struct> () =
    let (prefix, separator, listSeparator) = 
        getPrefixAndSeparators<'T> AppConfig.defaultPrefix AppConfig.defaultSeparator AppConfig.defaultListSeparator
    let fieldNameCanonicalizer = 
      AppConfig.fieldNameCanonicalizer prefix separator
    parse<'T> AppConfig.configReader fieldNameCanonicalizer listSeparator ""

  static member Get<'T when 'T : not struct> (fieldNameCanonicalizer : FieldNameCanonicalizer) =
  let (prefix, separator, listSeparator) = 
        getPrefixAndSeparators<'T> AppConfig.defaultPrefix AppConfig.defaultSeparator AppConfig.defaultListSeparator
  parse<'T> AppConfig.configReader fieldNameCanonicalizer listSeparator "" 