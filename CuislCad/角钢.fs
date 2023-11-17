namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Lake.ShapeSteel

module 不等边角钢Module =
    let getKey (data:不等边角钢) = sprintf "%s" data.Spec

    let section (b1,b2,d,r,r1) =
        let p0 = Point2d.Origin
        let p1 = new Point2d(b2, 0.)
        let p2 = new Point2d(b2, d)
        let p3 = new Point2d(d, d)
        let p4 = new Point2d(d, b1)
        let p5 = new Point2d(0., b1)
        Creator.polygon 
            [|
                p0,0.
                p1,0.
                p2,r1
                p3,r
                p4,r1
                p5,0.
            |]

    let uasection(data:不等边角钢) =
        section(data.B1,data.B2,data.D,data.R,data.R1)

module 等边角钢Module =
    let getKey (data:等边角钢) = sprintf "%s" data.Spec

    let easection(data:等边角钢) =
        不等边角钢Module.section(data.B,data.B,data.D,data.R,data.R1)
        
type 角钢Drawer() =
    [<CommandMethod("不等边角钢")>]
    member this.uadrawer() =
        let objects = Lake.ShapeSteel.不等边角钢.DataRecords
        
        let views = ["截面", 不等边角钢Module.uasection]
        Part.draw objects views 不等边角钢Module.getKey

    [<CommandMethod("等边角钢")>]
    member this.eadrawer() =
        let objects = Lake.ShapeSteel.等边角钢.DataRecords
        let views = ["截面", 等边角钢Module.easection]
        Part.draw objects views 等边角钢Module.getKey 

        


