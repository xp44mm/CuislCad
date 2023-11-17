namespace FsharpCad
open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
//open Cuisl
open Lake.GbPipes

module modTee =
    let getKey (data:Tee) = sprintf "%fx%f" data.DN1 data.DN2 

    let front (data:Tee) =
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
            
        let p1 = new Point2d(-dw2 / 2., data.M)
        let p2 = new Point2d(-dw2 / 2., dw1 / 2.)
        let p3 = new Point2d(-data.C, dw1 / 2.)
        let p4 = p3.Mirror hAxis
        let p5 = p4.Mirror vAxis
        let p6 = p3.Mirror vAxis
        let p7 = p2.Mirror vAxis
        let p8 = p1.Mirror vAxis

        [
            yield! polygon
                [|
                    p1, 0.
                    p2, 0.
                    p3, 0.
                    p4, 0.
                    p5, 0.
                    p6, 0.
                    p7, 0.
                    p8, 0.
                |]
            //相贯线
            if dw1 = dw2 then
                yield line(p0,p2) :> Entity
                yield line(p0,p7) :> Entity
            else
                let blg = sector.bulge (dw1 / 2.) (sector.getAngleFromChordLength (dw1 / 2.) dw2)
                yield ellipseArc (new Point2d(0., dw1 / 2.)) (dw2 / 2. * vecx) (2. * blg / dw2) Math.PI (2. * Math.PI) 
                      :> Entity
        ]
        |> List.map(rotateBy (-0.5*Math.PI) p0)

    let top (data:Tee) =
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

        let p1 = new Point2d(-data.C, -dw1 / 2.)
        let v1 = vector(2.*data.C,dw1)

        [
            yield! rectangle p1 v1
            yield circle(p0,0.5*dw2) :> Entity
        ]

    let side (data:Tee) =
        let dw1 =
            PipeModule.Dws
            |> Array.find(fun p -> p.DN=data.DN1)
            |> fun p -> p.Dw

        let dw2 =
            PipeModule.Dws
            |> Array.find(fun p -> p.DN=data.DN2)
            |> fun p -> p.Dw

        let vAxis = new Line2d(p0,vecy)
        let height = sector.height (dw1 / 2.) (sector.getAngleFromChordLength (dw1 / 2.) dw2)

        let p1 = new Point2d(dw2/2.,height)
        let p2 = new Point2d(dw2/2.,data.M)
        let p3 = p2.Mirror vAxis
        let p4 = p1.Mirror vAxis

        [
            yield! sequential 
                [|
                    p1, 0.
                    p2, 0.
                    p3, 0.
                    p4, 0.
                |]
            yield circle(p0,0.5*dw1) :> Entity
        ]

    let singleLine (data:Tee) =
        let dw1 =
            PipeModule.Dws
            |> Array.find(fun p -> p.DN=data.DN1)
            |> fun p -> p.Dw

        let dw2 =
            PipeModule.Dws
            |> Array.find(fun p -> p.DN=data.DN2)
            |> fun p -> p.Dw

        let p1 = new Point2d(-data.C, 0.)
        let p2 = new Point2d(data.C, 0.)
        let p3 = new Point2d(0., data.M)

        let p4 = new Point2d(-data.C / 4., 0.)
        let p5 = new Point2d(data.C / 4., 0.)
        let p6 = new Point2d(0., data.M / 4.)
        [
            line(p1,p2):>Entity
            line(p0,p3):>Entity

            line(p4,p6):>Entity
            line(p5,p6):>Entity

            circle(p1,0.05*dw1) :> Entity
            circle(p2,0.05*dw1) :> Entity
            circle(p3,0.05*dw2) :> Entity     
        
        ]
        |> List.map(rotateBy (-0.5*Math.PI) p0)

open Autodesk.AutoCAD.EditorInput

type TeeDrawer() =
    
    [<CommandMethod("Tee")>]
    member this.Drawer() =
        let objects = Lake.GbPipes.Tee.DataRecords

        let views = ["正面", modTee.front;"顶面", modTee.top;"侧面", modTee.side;"单线图", modTee.singleLine;]

        Part.tryGetBlockDef objects views modTee.getKey
        |>  function
            | Some(blockId,view) ->
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

            | None ->()