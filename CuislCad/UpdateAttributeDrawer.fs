namespace Cads

open System
open System.Collections.ObjectModel
open System.Windows
open System.Windows.Controls

open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Colors
open Input
open EntityOps

type UpdateAttributeDrawer()=
    [<Runtime.CommandMethod("UpdateAttribute")>]
    member this.UpdateAttribute() =
        let doc= getDocument()
        let db = doc.Database 
        let ed = doc.Editor

        let blocks =
            seq {
                use trans = db.TransactionManager.StartTransaction()
                let bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead):?> BlockTable
                for btrId in bt do
                    let btr = trans.GetObject(btrId, OpenMode.ForRead):?>BlockTableRecord
                    if btr.IsLayout then
                        for id in btr do
                            match trans.GetObject(id, OpenMode.ForRead) with
                            | :? BlockReference as br ->
                                for attId in br.AttributeCollection do
                                    let att = trans.GetObject(attId, OpenMode.ForRead):?>AttributeReference
                                    yield br.Name, att.Tag, att.TextString
                            | _ -> ()
            }//将扁平数据格式化为结构数据
            |> Seq.groupBy(fun(nm,tag,str)->nm)
            |> Seq.map(fun(nm,atts) ->
                let atts = 
                    atts 
                    |> Seq.map(fun(nm,tag,str)-> tag,str) 
                    |> Seq.groupBy(fun(tag,str)-> tag)
                    |> Seq.map(fun(tag,strings)->
                        let strings =
                            strings
                            |> Seq.map(fun(tag,str) -> str)
                            |> Set.ofSeq |> Set.toSeq
                        new AutoCADWpf.Att(Tag=tag,TextStrings = new ObservableCollection<string>(strings)))
                    |> Seq.toArray
                new AutoCADWpf.Block(Name=nm,Attributes=atts))
            |> Seq.toArray

        let w = new AutoCADWpf.AttributesWindow(blocks)
        if w.ShowDialog().Value then
            let blockName = w.BlockName
            let tag = w.AttTag
            let modifies = w.Modifies;

            use trans = db.TransactionManager.StartTransaction()
            let bt = trans.GetObject(db.BlockTableId, DatabaseServices.OpenMode.ForRead):?> BlockTable

            for btrId in bt do
                let btr = trans.GetObject(btrId, DatabaseServices.OpenMode.ForRead):?>BlockTableRecord
                if btr.IsLayout then
                    for id in btr do
                        match trans.GetObject(id, DatabaseServices.OpenMode.ForRead) with
                        | :? BlockReference as br ->
                            if br.Name = blockName then
                                for attId in br.AttributeCollection do
                                    let att = trans.GetObject(attId, DatabaseServices.OpenMode.ForWrite):?>AttributeReference
                                    if att.Tag = tag then
                                        modifies
                                        |> Array.tryFind(fun m -> m.TextString = att.TextString)
                                        |> function 
                                           | Some m -> att.TextString <- m.NewTextString
                                           | None -> ()
                        | _ -> ()
            trans.Commit()

