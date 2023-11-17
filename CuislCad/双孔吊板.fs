namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System
open Convert
open Lake.Hangers

type 双孔吊板Drawer() =
    // 实例的唯一标识
    let getKey (data:双孔吊板) = sprintf "%fx%f" data.Phi1 data.Phi2


    let front (data:双孔吊板) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(0.5*data.A,data.C1)
        let p2 = new Point2d(0.5*data.A,-data.H + data.C1) 
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        let c = new Point2d(0.,-data.H+data.C1+data.C2)

        [
            yield! Creator.polygon 
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]
            yield new Circle(Point3d.Origin,Vector3d.ZAxis,0.5*data.Phi1):>Entity
            yield new Circle(pto3d c,Vector3d.ZAxis,0.5*data.Phi2):>Entity
        ]
    let side (data:双孔吊板) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(0.5*data.T,data.C1)
        let p2 = new Point2d(0.5*data.T,-data.H + data.C1) 
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)
        
        let c = new Point2d(0.,-data.H+data.C1+data.C2)

        [
            yield! Creator.polygon 
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]
            yield new DBPoint(Point3d.Origin):>Entity
            yield new DBPoint(pto3d c):>Entity
        ]
    [<CommandMethod("双孔吊板")>]
    member this.Drawer() =
        let objects = Lake.Hangers.双孔吊板.DataRecords
        let views = ["正面", front;"侧面",side]
        Part.draw objects views getKey
