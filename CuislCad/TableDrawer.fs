namespace cuisl.cad

open System
//open System.IO
open System.Windows

open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput
open Autodesk.AutoCAD.Geometry
open Input

//open ColumnStyles
//open Newtonsoft.Json
open FSharp.Idioms

type TableDrawer() =

    ///复制选中的单行文本到剪切板
    [<Runtime.CommandMethod("toClipboard")>]
    member x.toClipboard() =
        let doc, ed, db = init()

        let res =
            let selectOptions =
                new PromptSelectionOptions(
                    AllowDuplicates = true,
                    MessageForAdding = "请选择TEXT对象")
            let filter =
                new SelectionFilter([|new TypedValue(int DxfCode.Start, "TEXT")|])
            ed.GetSelection(selectOptions, filter)

        if res.Status = PromptStatus.OK then
            let textids = res.Value.GetObjectIds()
            //获取对象实体
            let texts =
                let tm = db.TransactionManager
                use trans = tm.StartTransaction()
                textids
                |> Array.map(fun id ->
                    tm.GetObject(id, OpenMode.ForRead, true) 
                    :?> DBText )
                |> Table.fromCAD

            let outp = Tsv.stringify texts
            Clipboard.SetText(outp)
            ed.WriteMessage($"\n剪切板:{Literal.stringify outp}")

    ///从剪贴板读取文本，并绘制单行文本
    [<Runtime.CommandMethod("drawtableWithoutSytles")>]
    member x.drawtableWithoutSytles() =
        let doc,ed,db = init()

        let contents = Clipboard.GetText()

        let texts =
            contents
            |> Tsv.parseText
            |> Array.alignCols
        let tm = db.TransactionManager
        use trans = tm.StartTransaction()

        let dimscale = db.Dimscale
        Input.getOnePoint "请指定表格左下点:\n" (fun p ->
            let vect = p.GetAsVector()
            //绘图空间
            let space =
                trans.GetObject(db.CurrentSpaceId, DatabaseServices.OpenMode.ForWrite)
                :?> BlockTableRecord

            let appendEntity (ent:Entity) =
                ent.TransformBy(Matrix3d.Scaling(dimscale, Point3d.Origin))
                ent.TransformBy(Matrix3d.Displacement(vect))
                space.AppendEntity(ent) |> ignore
                trans.AddNewlyCreatedDBObject(ent, true)

            let dbtexts,lines = Table.entitiesWithoutSytles texts

            //添加直线
            for line in lines do
                appendEntity line

            let xscale =
                let tstr =
                    trans.GetObject(db.Textstyle,OpenMode.ForRead)
                    :?> TextStyleTableRecord
                tstr.XScale

            //给每个文本设置样式，并添加文本到数据库
            for txt in dbtexts do
                txt.TextStyleId <- db.Textstyle
                txt.WidthFactor <- xscale
                appendEntity txt
            trans.Commit()
        )

    /////从剪贴板读取文本，并绘制单行文本
    //[<Runtime.CommandMethod("drawtable")>]
    //member x.drawtable() =
    //    let doc,ed,db = init()

    //    let contents = System.Windows.Clipboard.GetText()
    //    // todo:tsv
    //    let texts =
    //        Cuisl.Array.parse(contents)
    //        |> Cuisl.Array.toArray2D

    //    let style =
    //        let columnStyles =
    //            let filename = @"D:\Application Data\AutoCAD\columnstyles.json"
    //            let s = File.ReadAllText(filename)
    //            JsonConvert.DeserializeObject<Style[]>(s)

    //        let w = new AutoCADWpf.ColumnStyleWindow()
    //        let lst =
    //            w.FindName("lstStyles")
    //            :?> ListBox
    //        lst.DisplayMemberPath <- "Name"
    //        columnStyles
    //        |> Array.iter(fun style ->
    //            lst.Items.Add(style)
    //            |>ignore
    //            )

    //        if w.ShowDialog().Value then
    //            columnStyles.[lst.SelectedIndex]
    //        else
    //            failwith "user cancel operation."

    //    let columnStyles =
    //        let cols =
    //            style
    //            |> fun style -> style.ColumnStyles
    //            |> List.toArray

    //        let len = Array2D.length2 texts
    //        if len > cols.Length then
    //            let arr = Array.create len (ColumnStyle.Default)
    //            cols
    //            |> Array.iteri(fun i col -> arr.[i]<-col)
    //            arr
    //        else
    //            cols.[0 .. len-1]

    //    //获取自定义样式

    //    let tm = db.TransactionManager
    //    use trans = tm.StartTransaction()

    //    let dimscale = db.Dimscale
    //    Input.getOnePoint "请指定表格左下点:\n" (fun p ->
    //        let vect = p.GetAsVector()
    //        //绘图空间
    //        let space =
    //            trans.GetObject(db.CurrentSpaceId, DatabaseServices.OpenMode.ForWrite)
    //            :?> BlockTableRecord

    //        let appendEntity (ent:Entity)=
    //            ent.TransformBy(Matrix3d.Scaling(dimscale, Point3d.Origin))
    //            ent.TransformBy(Matrix3d.Displacement(vect))
    //            space.AppendEntity(ent) |> ignore
    //            trans.AddNewlyCreatedDBObject(ent, true)

    //        let dbtexts,lines = Table.entities texts columnStyles

    //        //添加直线
    //        for line in lines do
    //            appendEntity line

    //        //添加文本
    //        let xscale =
    //            // acad指定的宽带
    //            let tstr =
    //                trans.GetObject(db.Textstyle,OpenMode.ForRead)
    //                :?> TextStyleTableRecord
    //            tstr.XScale

    //        for txt in dbtexts do
    //            txt.TextStyleId <- db.Textstyle
    //            txt.WidthFactor <- xscale
    //            appendEntity txt
    //        trans.Commit()
    //    )






