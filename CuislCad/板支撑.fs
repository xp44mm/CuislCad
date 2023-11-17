namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System

open Convert
open Creator
//open Cuisl

open Lake.Pedestals //支座

type 板支撑Drawer() =
    // 实例的唯一标识
    let getKey (data:板支撑) = sprintf "%d" data.Pos

    let isLarge (data:板支撑) = (data.Pos > 4)
    //斜边的垂直高度
    let xh (data:板支撑) =
        let b1 = data.B1
        let l2 = data.L2
        let t2 = data.T2

        if isLarge data then
            (l2 - b1 + t2) * tan(Math.PI / 3.)
        else
            (l2 - b1) * tan(Math.PI / 3.)

    let front (data:板支撑) =
        let p1 = new Point2d(0., data.T1)
        let p2 = new Point2d(0., data.H + 15.)
        let p3 = new Point2d(data.L2, data.H + 15.)

        let xh = xh data

        if data |> isLarge then
            let p4 = new Point2d(data.L2, xh + data.T1)
            let p5 = new Point2d(data.B1 - data.T1, data.T1)

            [
                yield! sequential 
                    [|
                        p1, 0.
                        p2, 0.
                        p3, 0.
                        p4, 0.
                        p5, 0.
                    |]

                yield! rectangle Point2d.Origin (p5.GetAsVector())//底板

                let tl = line(p4, p5)
                let vec = (vto2d -tl.Delta).GetPerpendicularVector().GetNormal()* data.T2

                yield tl |> move vec
            ]

        else
            let p4 = new Point2d(data.L2, xh + data.T1)
            let p5 = new Point2d(data.B1, data.T1)
            [
                yield! sequential
                    [|
                        p1, 0.
                        p2, 0.
                        p3, 0.
                        p4, 0.
                        p5, 0.
                    |]
                yield! rectangle Point2d.Origin (p5.GetAsVector())//底板
            ]

    let side (data:板支撑) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(-data.L1 / 2., 0.)
        let p2 = new Point2d(-data.B2 / 2., data.T1)
        let p3 = new Point2d(-data.L1 / 2., data.T1)
        [
            yield! rectangle p1 (new Vector2d(data.L1, data.T1))//底板
            let jb = rectangle p2 (new Vector2d(data.T2, data.H - data.T1))//肋板

            yield! jb
            yield! jb |> Array.map(mirror vAxis)
            
            if data |> isLarge then
                yield! rectangle p3 (new Vector2d(data.L1, xh data))
        ]

    [<CommandMethod("板支撑")>]
    member this.Drawer() =
        let objects = Lake.Pedestals.板支撑.DataRecords
        let views = ["正面", front;"侧面",side]
        Part.draw objects views getKey

