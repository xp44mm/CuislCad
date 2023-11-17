namespace FsharpCad

open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime
open Creator

open Lake.ShapeSteel

module 槽钢module =

    let getKey (data:槽钢) = sprintf "%s" data.Spec

    let section (data:槽钢) =
        let hAxis = new Line2d(new Point2d(0.,0.5*data.H),Vector2d.XAxis)

        let p1 = Point2d.Origin
        let p2 = new Point2d(data.B, 0.)
        let p3 = new Point2d(data.B, data.T - (data.B - data.D) / 20.)
        let p4 = new Point2d(data.D, data.T + (data.B - data.D) / 20.)
        let p5 = p4.Mirror(hAxis)
        let p6 = p3.Mirror(hAxis)
        let p7 = p2.Mirror(hAxis)
        let p8 = p1.Mirror(hAxis)

        polygon 
            [|
                p1,0.
                p2,0.
                p3,data.R1
                p4,data.R
                p5,data.R
                p6,data.R1
                p7,0.
                p8,0.
            |]

type 槽钢Drawer() =
    [<CommandMethod("槽钢")>]
    member this.Drawer() =
        let channels = Lake.ShapeSteel.槽钢.DataRecords
        let views = ["截面", 槽钢module.section]
        Part.draw channels views 槽钢module.getKey

