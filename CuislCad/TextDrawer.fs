namespace FsharpCad

open System

open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Colors
open Input

type TextDrawer() =
    [<Runtime.CommandMethod("SetTextStyleAs")>]
    member this.SetTextStyleAs()=
        let _,ed,db=init()
        use trans = db.TransactionManager.StartTransaction()
        let tst = trans.GetObject(db.TextStyleTableId, OpenMode.ForRead):?>TextStyleTable
        let TextStyleNames=
            let names = new ResizeArray<string>()
            for id in tst do
                let tstr = trans.GetObject(id, OpenMode.ForRead):?>TextStyleTableRecord
                names.Add(tstr.Name)
            names.ToArray()

        let w = new AutoCADWpf.SelectListWindow()
        w.Title<-"统一文字样式"
        w.ListBox.ItemsSource <- TextStyleNames
        
            
        if w.ShowDialog().Value then
            let tsname = string w.ListBox.SelectedItem
            
            let bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead):?>BlockTable
            for btrId in bt do
                let btr = trans.GetObject(btrId, OpenMode.ForRead):?>BlockTableRecord
                for id in btr do
                    let ent = trans.GetObject(id, OpenMode.ForWrite):?>Entity
                    match ent with 
                    | :? DatabaseServices.DBText as txt ->
                        let nm = txt.TextStyleName
                        match nm with
                        |"ROMANS"|"MONO"|"楷体"|"宋体"->()
                        | _ -> txt.TextStyleId <- tst.[tsname]
                    | _ ->()
            trans.Commit()
            ed.Regen()

    [<Runtime.CommandMethod("TextHeight")>]
    member this.TextHeight() =
        let doc,ed,db=init()

        let form = new AutoCADWpf.TextHeightWindow(db.Dimscale)

        if form.ShowDialog().Value then
            let res = 
                let opts=new PromptSelectionOptions(AllowDuplicates = true, MessageForAdding = "请选择TEXT对象")
                let filter= new SelectionFilter([|new TypedValue(int DxfCode.Start, "TEXT")|])
                ed.GetSelection(opts,filter)
                
            use trans = db.TransactionManager.StartTransaction()
            let texts = new ResizeArray<DBText>()
            for id in res.Value.GetObjectIds() do
                let text = db.TransactionManager.GetObject(id, DatabaseServices.OpenMode.ForWrite, true):?>DBText
                texts.Add(text)
                text.Height <- form.FontHeight * db.Dimscale
            trans.Commit()

    [<Runtime.CommandMethod("StandardTextStyle")>]
    member this.StandardTextStyle() =        
        let doc,ed,db=init()
        use trans = db.TransactionManager.StartTransaction()
        let tsTable = trans.GetObject(db.TextStyleTableId, OpenMode.ForRead):?>TextStyleTable

        for recId  in tsTable do
            let rcd  = trans.GetObject(recId, OpenMode.ForWrite):?>TextStyleTableRecord
            if String.IsNullOrEmpty(rcd.Name) then
                rcd.BigFontFileName <- "gbcbig.shx"
                rcd.FileName <- "txt.shx"
            else
                match rcd.Name.ToUpper() with
                |"ROMANS"->
                    rcd.FileName <- "romans.shx"
                    rcd.BigFontFileName <- "hzd.shx"
                |"MONO"->
                    rcd.FileName <- "monotxt.shx"
                    rcd.BigFontFileName <- "hzd.shx"
                |"楷体"->
                    rcd.FileName <- "楷体_GB2312"
                |"宋体"->
                    rcd.FileName <- "新宋体"
                | _ ->
                    rcd.FileName <- "gbenor.shx"
                    rcd.BigFontFileName <- "gbcbig.shx"
        trans.Commit()
        ed.Regen()


