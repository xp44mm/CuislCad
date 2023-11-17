module cuisl.cad.Table 

open System
open Autodesk.AutoCAD.Geometry 
open Autodesk.AutoCAD.DatabaseServices 

open ColumnStyles

//align
let attach (align:Align) =
    match align with
    |Left   -> AttachmentPoint.BaseLeft
    |Right  -> AttachmentPoint.BaseRight
    |Center -> AttachmentPoint.BaseCenter

let horiz (cs:ColumnStyle) offset x =
    match cs.Align with
    | Left  -> x + offset
    | Center-> x + 0.5 * cs.Width
    | Right -> x + cs.Width - offset

let entities (tbl:string[,])(colstyles:ColumnStyle[]) =
    let rowCount =tbl.GetLength(0)
    let colCount =tbl.GetLength(1)

    //文本高度与行高度
    let textHeight = 3.0
    let rowHeight = textHeight * 2.0

    //水平偏移量和垂直偏移量
    let XOffset = textHeight * 0.4
    let YOffset = (rowHeight - textHeight) / 2.0 * 0.9

    //垂直直线的X坐标
    let xVerticalLines =
        let lines =
            colstyles 
            |> Array.scan(fun cord style ->cord + style.Width) 0.0
        lines

    //水平直线的Y坐标
    let yHorizontalLines =
        [|0 .. rowCount|]
        |> Array.map(fun i -> float i * rowHeight)

    //获得表格的框线
    let lines =
        let width  = xVerticalLines.[xVerticalLines.Length - 1]
        let height = yHorizontalLines.[yHorizontalLines.Length - 1]

        let vlines =
            xVerticalLines
            |> Array.mapi(fun i x ->
                let spnt = new Point3d(x,0.0   ,0.0)
                let epnt = new Point3d(x,height,0.0)
                new Line(spnt,epnt))

        let hlines =
            yHorizontalLines
            |> Array.mapi(fun i y ->
                let spnt = new Point3d(0.0  ,y,0.0)
                let epnt = new Point3d(width,y,0.0)
                new Line(spnt,epnt))

        [ yield! vlines;yield! hlines;]
    let dbtexts =
        colstyles
        |>Array.mapi(fun c style ->
            //文字对齐点的X坐标
            let x = horiz style XOffset xVerticalLines.[c]

            [0 .. rowCount-1]
            |>List.map(fun r ->
                let y = yHorizontalLines.[r] + YOffset
                let txt = new DBText(
                            Justify= attach style.Align,
                            TextString = tbl.[r,c],
                            Height = textHeight)

                if txt.Justify = AttachmentPoint.BaseLeft 
                then txt.Position <- new Point3d(x,y,0.0) 
                else txt.AlignmentPoint <- new Point3d(x,y,0.0)                 
                txt))
        |>List.concat 

    dbtexts,lines

let table(dbtexts : DBText list) =
    //
    let step values delta =
        let sorted =values |> Array.sort |> List.ofArray 

        let rec loop acc sorted =
            match sorted with
            | [] -> acc|> List.rev 
            | h::t ->               
                if h - List.head acc > delta then
                    loop (h::acc) t
                else
                    loop acc t
        loop [sorted.Head] sorted.Tail
    //
    let dbtextPoint(txt : DBText) =
        match txt.Justify with
        | AttachmentPoint.BaseLeft -> txt.Position
        | _ -> txt.AlignmentPoint

    //假定所有的文字高度相同，任选一个文本获得其高度
    let textHeight = dbtexts.[0].Height 

    //表格中每一列的x坐标, 和每一行的y坐标
    let colspace = textHeight*1.4
    let rowspace = textHeight*1.1
    let cols,rows =
        let ps = [ for txt in dbtexts -> dbtextPoint txt]
        step [|for p in ps -> p.X|] colspace,
        step [|for p in ps -> p.Y|] rowspace

    let tbl : string[,] = Array2D.zeroCreate rows.Length  cols.Length 
    dbtexts
    |> List.iter(fun txt ->
        let p = dbtextPoint txt
        let c = cols|>List.findIndex(fun col -> abs(p.X-col)<colspace)
        let r = rows|>List.findIndex(fun row -> abs(p.Y-row)<rowspace)
        tbl.[r,c]<-txt.TextString.Replace("%%d", "°").Replace("%%c", "Φ"))
    tbl

//合成输出文本
let output (texts:string[,]) =
    let res =
        [|
            let rows = texts.GetLength(0)
            let cols = texts.GetLength(1)

            for r in [0 .. rows-1] ->
                [| for c in [0 .. cols-1] -> texts.[r,c]|]
        |]

    let rows = 
        res |> Array.map(fun cols -> String.Join("\t",cols))

    String.Join("\n",rows)

