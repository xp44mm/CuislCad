[<RequireQualifiedAccess>]
module Cuisl.Array

open System
open System.Text.RegularExpressions

///解析text为Table[][]。
///忽略空行，或以;;开始的注释行；
///分隔符依次尝试'\t' ',' ';' ' '
let parse (text:string) =
    //忽略空行，或以;;开始的注释行
    let lines =
        Regex.Split(text, "\r?\n")
        |> Array.Parallel.partition(fun ln -> Regex.IsMatch(ln,"^\s*$") || Regex.IsMatch(ln,"^\s*;;"))
        |> snd
    let sep =
        if lines |> Array.exists(fun s -> s.Contains("\t")) then
            '\t'
        elif lines |> Array.exists(fun s -> s.Contains(",")) then
            ','
        elif lines |> Array.exists(fun s -> s.Contains(";")) then
            ';'
        elif lines |> Array.exists(fun s -> s.Contains(" ")) then
            ' '
        else
            '\n'
    lines
    |> Array.map(fun ln -> ln.Split(sep))

///注意：输入arr的元素长度必须相等
let toArray2D (arr:'a [][]) =
    let rows = arr.Length
    let cols = arr.[0].Length
//            arr
//            |> Array.map(fun cs -> cs.Length)
//            |> Array.max
    let retArray = Array2D.create rows cols Unchecked.defaultof<'a>
    arr
    |> Array.iteri(fun r cs ->
        cs
        |> Array.iteri(fun c elem ->
            retArray.[r,c]<-elem
        )
    )
    retArray

