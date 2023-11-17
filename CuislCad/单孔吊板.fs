namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System
open Convert
//open Cuisl

open Lake.Hangers

type 单孔吊板Drawer() =
    // 实例的唯一标识
    let getKey (data:单孔吊板) = sprintf "%f" data.D

    let front (data:单孔吊板) =        
        let p0 = new Point2d(0., -data.H)
        let p1 = new Point2d(data.A / 2., -data.H)
        let p2 = new Point2d(data.A / 2., 0.)

        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        [
            yield! Creator.sequential
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]
            yield new Circle(pto3d p0,Vector3d.ZAxis,0.5*data.D) :> Entity
            yield new Arc(pto3d p0,0.5*data.A,Math.PI,0.) :> Entity
        ]

    let side (data:单孔吊板) =        
        let p1 = new Point2d( -0.5 * data.Del, 0.)
        let p2 = new Point2d(  0.5 * data.Del, 0.)
        let p3 = new Point2d(  0.5 * data.Del, -data.B)
        let p4 = new Point2d( -0.5 * data.Del, -data.B)

        let p5 = new Point2d( -0.5 * data.Del, -data.H+0.5*data.D)
        let p6 = new Point2d(  0.5 * data.Del, -data.H+0.5*data.D)
        let p7 = new Point2d( -0.5 * data.Del, -data.H-0.5*data.D)
        let p8 = new Point2d(  0.5 * data.Del, -data.H-0.5*data.D)

        let p9 = new Point2d(0., -data.H)
        
        [
            yield! Creator.polygon 
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]
            yield new Line(pto3d p5,pto3d p6) :> Entity
            yield new Line(pto3d p7,pto3d p8) :> Entity
            yield new DBPoint(pto3d p9) :> Entity
        ]

    [<CommandMethod("单孔吊板")>]
    member this.Drawer() =
        let objects = Lake.Hangers.单孔吊板.DataRecords

        let views = ["正面", front;"侧面",side]
        Part.draw objects views getKey
