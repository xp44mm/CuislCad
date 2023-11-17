﻿namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
open Lake.Valves 

module mod隔膜阀 =
    let getKey (data:隔膜阀) = sprintf "PN10,%f" data.DN

    let side (data:隔膜阀) =
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

            let p1 = new Point2d(data.L / 2. - flange.C, flange.Di2 / 2.)
            let p2 = p1.Mirror vAxis
            let p3 = p1.Mirror hAxis
            let p4 = p2.Mirror hAxis

            let p5 = (p1 + p2.GetAsVector()) / 2.

            yield line(p1,p2):>Entity
            yield line(p5,p3):>Entity
            yield line(p5,p4):>Entity

        ]

open Autodesk.AutoCAD.EditorInput

type 隔膜阀Drawer() =
    [<CommandMethod("隔膜阀")>]
    member this.Drawer() =
        let objects = 隔膜阀.DataRecords
        let views = ["侧面",mod隔膜阀.side]

        Part.tryGetBlockDef objects views mod隔膜阀.getKey
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










