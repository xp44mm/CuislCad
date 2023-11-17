namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Creator
open Lake.ShapeSteel

module H型钢Module =

    let getKey (data:H型钢) = sprintf "%s" data.Spec

    let section (data:H型钢) =
        let hAxis = new Line2d(new Point2d(0.,0.5*data.H),Vector2d.XAxis)
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(0.5*data.B, 0.)
        let p2 = new Point2d(0.5*data.B, data.T2)
        let p3 = new Point2d(0.5*data.T1, data.T2)

        let p4 = p3.Mirror(hAxis)
        let p5 = p2.Mirror(hAxis)
        let p6 = p1.Mirror(hAxis)

        let p7 = p6.Mirror(vAxis)
        let p8 = p5.Mirror(vAxis)
        let p9 = p4.Mirror(vAxis)
        let p10 = p3.Mirror(vAxis)
        let p11 = p2.Mirror(vAxis)
        let p12 = p1.Mirror(vAxis)

        polygon 
            [|
                p1,0.
                p2,0.
                p3,data.R
                p4,data.R
                p5,0.
                p6,0.
                p7,0.
                p8,0.
                p9,data.R
                p10,data.R
                p11,0.
                p12,0.
            |]

type H型钢Drawer() =
    [<CommandMethod("H型钢")>]
    member this.Drawer() =
        let objects = Lake.ShapeSteel.H型钢.DataRecords

        let views = ["截面", H型钢Module.section]
        Part.draw objects views H型钢Module.getKey


