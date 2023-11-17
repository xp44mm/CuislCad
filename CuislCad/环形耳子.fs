namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

//open Cuisl
open System
open Convert

open Lake.Hangers

type 环形耳子Drawer() =
    // 实例的唯一标识
    let getKey (data:环形耳子) = sprintf "%f" data.D

    let front (data:环形耳子) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1  = new Point2d(data.B1/2., 0.)
        let p2  = new Point2d(data.B1/2., data.B2)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        let p5  = new Point2d(data.A /2., 0.)
        let p6  = new Point2d(data.A /2., data.L)

        let p7 = new Point2d(data.D /2., data.B2)
        let p8 = new Point2d(data.D /2., data.L)

        let center = new Point2d(0., data.L)

        [
            yield! Creator.sequential
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]


            let ln1 = new Line(pto3d p5,pto3d p6) :> Entity
            let ln2 = new Line(pto3d p7,pto3d p8) :> Entity

            yield ln1
            yield ln2

            yield ln1 |> mirror vAxis
            yield ln2 |> mirror vAxis

            yield new Arc(pto3d center,0.5*data.A,0.,Math.PI) :> Entity
            yield new Arc(pto3d center,0.5*data.D,0.,Math.PI) :> Entity
            yield new Line(pto3d p5,pto3d (p5.Mirror(vAxis))) :> Entity

        ]

    let side (data:环形耳子) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(data.D1 / 2., data.B2)
        let p2 = new Point2d(data.B1 / 2., data.B2)
        let p3 = new Point2d(data.B1 / 2., 0.)
        let p4 = p3.Mirror(vAxis)
        let p5 = p2.Mirror(vAxis)
        let p6 = p1.Mirror(vAxis)

        let p7 = new Point2d(data.D1 / 2., 0.)
        let p8 = new Point2d(data.D1 / 2., data.L + data.A / 2. - data.D1 / 2.)
        
        let p9 = new Point2d(0., data.L)

        [
            yield! Creator.sequential
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                    p5,0.
                    p6,0.
                |]


            let ln = new Line(pto3d p7,pto3d p8) :> Entity

            yield ln
            yield ln |> mirror vAxis

            yield new Arc(pto3d (p8 - Vector2d(0.5*data.D1,0.)),0.5*data.D1,0.,Math.PI) :> Entity
            yield new DBPoint(pto3d p9) :> Entity

        ]

    [<CommandMethod("环形耳子")>]
    member this.Drawer() =
        let objects = Lake.Hangers.环形耳子.DataRecords
        let views = ["正面", front;"侧面",side]
        Part.draw objects views getKey


