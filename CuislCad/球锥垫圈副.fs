namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System
open Convert
open Lake.Screw

type 球锥垫圈副Drawer() =
    // 实例的唯一标识
    let getKey (data:球锥垫圈副) = sprintf "%f" data.M

    let front (data:球锥垫圈副) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(0.5*data.D,0.)
        let p2 = new Point2d(0.5*data.D,data.H)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)
        
        let p5 = new Point2d(0.5*data.D,data.H1)
        let p6 = p5.Mirror(vAxis)
        
        [
            yield! Creator.polygon
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]

            yield new Line(pto3d p5,pto3d p6) :> Entity

        ]
    [<CommandMethod("球锥垫圈副")>]
    member this.Drawer() =
        let objects = Lake.Screw.球锥垫圈副.DataRecords
        let views = ["正面", front]
        Part.draw objects views getKey

        