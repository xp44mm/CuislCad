namespace FsharpCad

open System
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
open Lake.Valves 

module mod旋启式止回阀 =

    let getKey (data:旋启式止回阀) = sprintf "PN10,%f" data.DN

    let side (data:旋启式止回阀) =
        let flange = 
            mod突面平焊法兰.flanges
            |> Array.find(fun fl -> fl.PN=1. && fl.DN=data.DN)

        let hAxis = new Line2d(p0,vecx)
        let vAxis = new Line2d(p0,vecy)

        [
            //flange
            let fl1 = mod突面平焊法兰.side flange |> List.map(move (0.5*data.L*vecx))
            yield! fl1
            yield! fl1 |> List.map(mirror vAxis)

            let p1 = new Point2d(flange.C - data.L / 2., -flange.DS / 2.)
            let p2 = new Point2d(0., -flange.Dw / 2.)
            let p3 = p1.Mirror vAxis

            let arc =
                let a = new CircularArc2d(p1,p2,p3)
                let sa = -(p1 - a.Center).GetAngleTo(vecx)
                let ea = -(p3 - a.Center).GetAngleTo(vecx)
                arc(a.Center,a.Radius, sa, ea):>Entity

            yield arc

            let p4 = p1.Mirror hAxis
            let p5 = p3.Mirror hAxis
            yield line(p4,p5):>Entity

            let s1 = new Point2d(1.5 * flange.C, 0.)
            let s2 = new Point2d(-1.5 * flange.C, 1.5 * flange.C)
            let s3 = s2.Mirror hAxis

            yield! polygon 
                [|
                    s1,0.
                    s2,0.
                    s3,0.
                |]

        ]

open Autodesk.AutoCAD.EditorInput

type 旋启式止回阀Drawer() =
    [<CommandMethod("旋启式止回阀")>]
    member this.Drawer() =
        let objects = Lake.Valves.旋启式止回阀.DataRecords
        let views = ["侧面",mod旋启式止回阀.side]

        Part.tryGetBlockDef objects views mod旋启式止回阀.getKey
        |>  function
            |Some(blockId,view) ->
                let db,ed = 
                    let doc = Input.getDocument()
                    doc.Database, doc.Editor

                //*在屏幕上获取两点
                let ps1 = ed.GetPoint("请选择插入点")
                if ps1.Status <> PromptStatus.OK then ()
                let p1 = ps1.Value 

                let ps2 = ed.GetPoint(new PromptPointOptions("选取方向点",UseBasePoint = true, UseDashedLine = true, BasePoint = p1))
                if ps2.Status <> PromptStatus.OK then ()
                let p2 = ps2.Value 

                Part.getBr2 p1 p2 blockId |> EntityOps.drawEntity db.CurrentSpaceId

            |None ->()
