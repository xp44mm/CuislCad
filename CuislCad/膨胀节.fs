namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
//open Cuisl
open Lake.Valves 

module mod膨胀节 =
    let getKey (data:膨胀节) = sprintf "PN10-DN%f" data.DN
    let side (data:膨胀节) =
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

            let rr = (data.L - 3. * flange.C) / 2. / sin(2. / 3. * Math.PI)
            let p1 = Point2d(0., flange.Di2 / 2. - rr / 2.)

            let a = arc(p1, rr, Math.PI / 6., 5. / 6. * Math.PI):>Entity
            yield a
            yield a |>mirror hAxis
            
            let p2 = new Point2d(flange.C - data.L / 2., -flange.Di2 / 2.)
            let rect = rectangle p2 (vector(flange.C / 2., flange.Di2))
            yield! rect
            yield! rect |> Array.map(mirror vAxis)
        ]

open Autodesk.AutoCAD.EditorInput

type 膨胀节Drawer() =
    [<CommandMethod("膨胀节")>]
    member this.Drawer() =
        let objects = 膨胀节.DataRecords

        let views = ["侧面",mod膨胀节.side]

        Part.tryGetBlockDef objects views mod膨胀节.getKey
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










