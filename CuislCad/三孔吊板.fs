namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System
open Convert
open Creator
open Lake.Hangers

type 三孔吊板Drawer() =
    // 实例的唯一标识
    let getKey (data:三孔吊板) = sprintf "%fx%f" data.Phi1 data.Phi2

    let front (data:三孔吊板) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p0 = new Point2d(0., sqrt(2.) * data.R)
        let p1 = new Point2d(data.A / 2., sqrt(2.) * data.R - data.A / 2.)
        let p2 = new Point2d(data.A / 2., -data.H)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)
        
        [
            yield! Creator.polygon 
                [|
                    p0,data.R 
                    p1,data.R0 
                    p2,data.R0 
                    p3,data.R0 
                    p4,data.R0 
                |]

            yield circle(Point2d.Origin,0.5*data.Phi1):>Entity
            let c = circle(new Point2d(0.5*data.B,-data.H1),0.5*data.Phi2) :> Entity
            yield c
            yield c |> mirror vAxis
        ]
    let side (data:三孔吊板) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(0.5*data.T,data.R)
        let p2 = new Point2d(0.5*data.T,-data.H) 
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)
        
        let p5 = new Point2d(0.,-data.H1)

        [
            yield! Creator.polygon 
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]
            yield point(Point2d.Origin):>Entity
            yield point(p5):>Entity
        ]
    [<CommandMethod("三孔吊板")>]
    member this.Drawer() =
        let objects = Lake.Hangers.三孔吊板.DataRecords
        let views = ["正面", front;"侧面",side]
        Part.draw objects views getKey
