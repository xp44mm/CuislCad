module Part

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput
open Autodesk.AutoCAD.Geometry

open System.Windows
open System.Windows.Controls

open AutoCADWpf

open Convert

///根据一点生成块参照
let getBr p blockId =
    let ucs = Input.getDocument().Editor.CurrentUserCoordinateSystem
    let br = new BlockReference(p, blockId)
    br.TransformBy(ucs)
    br

///根据二点生成块参照
let getBr2 (p1:Point3d) (p2:Point3d) blockId =
    let ucs = Input.getDocument().Editor.CurrentUserCoordinateSystem

    let xoy =
        let plane = new Plane(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis)
        plane.TransformBy(ucs)
        plane
    
    let br = new BlockReference(p1, blockId)
    br.Rotation <- (p2-p1).TransformBy(ucs).Convert2d(xoy).Angle
    br.TransformBy(ucs)
    br

//let tryGetBlockDef<'t> (objects:seq<'t>) (views:List<string*('t -> Entity list)>) =
//    let window = new PartWindow(Title = (typeof<'t>).Name)
//    let dgObjects = window.FindName("dgObjects"):?>DataGrid
//    let cbViews = window.FindName("cbViews"):?>ComboBox
//    dgObjects.ItemsSource<-objects
//    cbViews.ItemsSource<- [ for nm,fn in views -> nm ]
//
//    if window.ShowDialog().Value then
//        let data = window.Object :?> 't
//        let view = window.View
//        let entities = [
//                        for (nm,fn) in views do
//                            if nm = view then yield! fn data
//                       ]
//
//        let nm = sprintf "%s-%s-%s" (typeof<'t>).Name view (data.ToString())
//        let blockId = EntityOps.defineBlock nm entities
//        Some(blockId,window.View)
//    else
//        None
//
//
//
//
//let draw<'t> (objects:seq<'t>) (views:List<string*('t -> Entity list)>) =
//    tryGetBlockDef objects views
//    |>  function
//        |Some(blockId,_) ->
//            let db,ed = 
//                let doc = Input.getDocument()
//                doc.Database, doc.Editor
//
//            let ps = ed.GetPoint("请选择插入点")
//            if ps.Status = PromptStatus.OK then
//                getBr ps.Value blockId 
//                |> EntityOps.drawEntity db.CurrentSpaceId
//
//        |None ->()
//

let tryGetBlockDef<'t> (objects:seq<'t>) (views:List<string*('t -> Entity list)>) getKey =
    let window = new PartWindow(Title = (typeof<'t>).Name)
    let dgObjects = window.FindName("dgObjects"):?>DataGrid
    let cbViews = window.FindName("cbViews"):?>ComboBox
    dgObjects.ItemsSource<-objects
    cbViews.ItemsSource<- [ for nm,fn in views -> nm ]

    if window.ShowDialog().Value then
        let data = window.Object :?> 't
        let view = window.View
        let entities = [
                        for (nm,fn) in views do
                            if nm = view then yield! fn data
                       ]

        let nm = sprintf "%s-%s-%s" typeof<'t>.Name view (getKey data)
        let blockId = EntityOps.defineBlock nm entities
        Some(blockId,window.View)
    else
        None

let draw<'t> (objects:seq<'t>) (views:List<string*('t -> Entity list)>) getKey =
    tryGetBlockDef objects views getKey
    |>  function
        |Some(blockId,_) ->
            let db,ed = 
                let doc = Input.getDocument()
                doc.Database, doc.Editor

            let ps = ed.GetPoint("请选择插入点")
            if ps.Status = PromptStatus.OK then
                getBr ps.Value blockId 
                |> EntityOps.drawEntity db.CurrentSpaceId

        |None ->()
