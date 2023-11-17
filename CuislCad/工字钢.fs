namespace FsharpCad

open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Lake.ShapeSteel

module 工字钢Module =
    let getKey (data:工字钢) = sprintf "%s" data.Spec

    let section (data:工字钢) =
        let hAxis = new Line2d(new Point2d(0.,0.5*data.H),Vector2d.XAxis)
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)
        
        let p1 = new Point2d(data.B / 2., 0.)
        let p2 = new Point2d(data.B / 2., data.T - (data.B - data.D) / 24.)
        let p3 = new Point2d(data.D / 2., data.T + (data.B - data.D) / 24.)

        let p4 = p3.Mirror(hAxis)
        let p5 = p2.Mirror(hAxis)
        let p6 = p1.Mirror(hAxis)

        let p7 = p6.Mirror(vAxis)
        let p8 = p5.Mirror(vAxis)
        let p9 = p4.Mirror(vAxis)
        let p10 = p3.Mirror(vAxis)
        let p11 = p2.Mirror(vAxis)
        let p12 = p1.Mirror(vAxis)

        Creator.polygon 
            [|
                p1,0.
                p2,data.R1
                p3,data.R
                p4,data.R
                p5,data.R1
                p6,0.
                p7,0.
                p8,data.R1
                p9,data.R
                p10,data.R
                p11,data.R1
                p12,0.
            |]

type 工字钢Drawer() =
    [<CommandMethod("工字钢")>]
    member this.Drawer() =
        let objects = Lake.ShapeSteel.工字钢.DataRecords
        let views = ["截面", 工字钢Module.section]
        Part.draw objects views 工字钢Module.getKey
