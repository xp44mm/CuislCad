//module Reducer
namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator

open Lake.GbPipes

module modReducer =
    let getKey (data:Reducer) = sprintf "%fx%f" data.DN1 data.DN2

    let 同心大小头 (data:Reducer) =
        let dw1 =
            PipeModule.Dws
            |> Array.find(fun p -> p.DN=data.DN1)
            |> fun p -> p.Dw

        let dw2 =
            PipeModule.Dws
            |> Array.find(fun p -> p.DN=data.DN2)
            |> fun p -> p.Dw

        let hAxis = new Line2d(p0,vecx)
        let vAxis = new Line2d(p0,vecy)
            
        let p1 = new Point2d(0., 0.5 * dw1)
        let p2 = new Point2d(data.H, 0.5 * dw2)
        let p3 = p2.Mirror hAxis
        let p4 = p1.Mirror hAxis

        [
            yield! polygon
                [|
                    p1, 0.
                    p2, 0.
                    p3, 0.
                    p4, 0.
                |]
        ]

    let 偏心大小头 (data:Reducer) =
        let dw1 =
            PipeModule.Dws
            |> Array.find(fun p -> p.DN=data.DN1)
            |> fun p -> p.Dw

        let dw2 =
            PipeModule.Dws
            |> Array.find(fun p -> p.DN=data.DN2)
            |> fun p -> p.Dw

        let p1 = new Point2d(0., 0.)
        let p2 = new Point2d(0., dw1)
        let p3 = new Point2d(data.H, dw2)
        let p4 = new Point2d(data.H, 0.)

        [
            yield! polygon
                [|
                    p1, 0.
                    p2, 0.
                    p3, 0.
                    p4, 0.
                |]
        ]

open Autodesk.AutoCAD.EditorInput    
type ReducerDrawer() =
    
    [<CommandMethod("大小头")>]
    member this.Drawer() =
        let objects = Lake.GbPipes.Reducer.DataRecords
        
        let views = ["同心大小头",modReducer.同心大小头;"偏心大小头",modReducer.偏心大小头;]

        Part.tryGetBlockDef objects views modReducer.getKey
        |>  function
            |Some(blockId,view) ->
                let db,ed = 
                    let doc = Input.getDocument()
                    doc.Database, doc.Editor

                //*在屏幕上获取两点
                let ps1 = ed.GetPoint("请选择插入点")
                if ps1.Status <> PromptStatus.OK then () else
                let p1 = ps1.Value 

                let ps2 = ed.GetPoint(new PromptPointOptions("选取方向点",UseBasePoint = true, UseDashedLine = true, BasePoint = p1))
                if ps2.Status <> PromptStatus.OK then () else
                let p2 = ps2.Value 

                Part.getBr2 p1 p2 blockId |> EntityOps.drawEntity db.CurrentSpaceId

            |None ->()
