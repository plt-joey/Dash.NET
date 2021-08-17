﻿module Dash.NET.ComponentGeneration.Prelude

open System
open System.IO
open System.Text.RegularExpressions

//Bind two Async<bool*string*string>'s
let (|@!>) (c1: Async<bool*string*string>) (c2: Async<bool*string*string>) =
    async {
        let! success, o, e = c1
        let! newSuccess, u, r = c2
        return newSuccess && success, sprintf "%s\n%s" o u, sprintf "%s\n%s" e r
    }

//Bind two Async<bool*string*string>'s, but only if the first one succeeded
let (|@>) (c1: Async<bool*string*string>) (c2: Async<bool*string*string>) =
    async {
        let! success, o, e = c1
        if success then
            let! newSuccess, u, r = c2
            return newSuccess, sprintf "%s\n%s" o u, sprintf "%s\n%s" e r
        else
            return success, o, e
    }

let validDULabel = Regex "^[ABCDEFGHIJKLMNOPQRSTUVWXYZ].*"

module Option =
    let bindNone (ifNone: 'a option) (op: 'a option) =
        match op with
        | Some _ -> op
        | None -> ifNone

module String =
    let replace (old: string) (_new: string) (s: string) = s.Replace(old,_new)
    let split (on: string) (s: string) = s.Split(on) |> List.ofArray
    let write path (s: string) = File.WriteAllText(path,s)

    let firstLetter (s: string) = s.ToLower().Substring(0,1)
    let decapitalize (s: string) = (sprintf "%c%s" (Char.ToLowerInvariant(s.[0])) (s.Substring(1)))
    let capitalize (s: string) = (sprintf "%c%s" (Char.ToUpperInvariant(s.[0])) (s.Substring(1)))

    let removeQuotes (s: string) = 
        if Regex.IsMatch(s, "^(['\"`]).*(['\"`])$") then
            s.Substring(1,s.Length - 2)
        else if Regex.IsMatch(s, "^(\\\").*(\\\")$") then
            s.Substring(2,s.Length - 4)
        else
            s

    let escape = replace @"\" @"\\"

    //DU labels have to start with a capital, if a property/value name starts with an _ or other non-letter character
    //then we have to add a letter in front of it
    //TODO There are other rules to DU names that don't apply to normal variable names (like no control characters like " and \)
    let toValidDULabel (s: string) = 
        let capitalizedLabel = s |> capitalize
        if validDULabel.IsMatch(capitalizedLabel) then capitalizedLabel
        else sprintf "Prop%s" s

    let matches (reg: string) (s: string) = Regex.IsMatch(s, reg)

