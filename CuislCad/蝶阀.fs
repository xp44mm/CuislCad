//module 蝶阀
namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
open Lake.Valves 

module mod蝶阀 =

    let getKey (data:蝶阀) = sprintf "PN%f-DN%f" data.PN data.DN

    let side (data:蝶阀) =
        let flange = 
            mod突面平焊法兰.flanges
            |> Array.find(fun fl -> fl.PN=data.PN && fl.DN=data.DN)

        let hAxis = new Line2d(p0,vecx)
        let vAxis = new Line2d(p0,vecy)

        [
            let p1 = new Point2d(-data.L / 2., flange.DS / 2.)
            let v1 = vector(data.L,-flange.DS)

            yield line(p1, p1+v1) :> Entity
            yield! rectangle p1 v1

            yield circle(p0, data.L / 4.) :> Entity
        ]

open Autodesk.AutoCAD.EditorInput

type 蝶阀Drawer() =
    [<CommandMethod("蝶阀")>]
    member this.Drawer() =
        let objects = 蝶阀.DataRecords
        let views = ["侧面",mod蝶阀.side]

        Part.tryGetBlockDef objects views mod蝶阀.getKey
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
