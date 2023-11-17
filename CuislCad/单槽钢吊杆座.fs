namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert

open Lake.Hangers

type 单槽钢吊杆座Drawer() =
    // 实例的唯一标识
    let getKey (data:单槽钢吊杆座) = sprintf "%d" data.POS

    let front (data:单槽钢吊杆座) =
        let p0 = Point2d.Origin
        let p1 = new Point2d(data.A, 0.)
        let p2 = new Point2d(data.A, -data.A1)
        let p3 = new Point2d(data.A1, -data.A)
        let p4 = new Point2d(0., -data.A)

        let p5  = new Point3d(0., -data.T                   ,0.)
        let p6  = new Point3d(data.A, -data.T               ,0.)
        let p7  = new Point3d(data.E + data.D / 2., -data.T ,0.)
        let p8  = new Point3d(data.E + data.D / 2., 0.      ,0.)
        let p9  = new Point3d(data.E - data.D / 2., -data.T ,0.)
        let p10 = new Point3d(data.E - data.D / 2., 0.      ,0.)
        let p11 = new Point3d(data.E, -data.T / 2.          ,0.)

        [
            yield! Creator.polygon 
                [|
                    p0,0.
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]
            yield new Line(p5,p6)    :>Entity
            yield new Line(p7,p8)    :>Entity
            yield new Line(p9,p10)   :>Entity

            yield new DBPoint(Position=p11):>Entity
        ]

    let top (data:单槽钢吊杆座) =
        let p1 = new Point2d(0.,     data.C / 2.)
        let p2 = new Point2d(data.A1,data.C / 2.)
        let p3 = new Point2d(data.A, data.C / 2.)

        let p4 = new Point2d(0.,     data.C / 2. - data.T)
        let p5 = new Point2d(data.A1,data.C / 2. - data.T)
        let p6 = new Point2d(data.A, data.C / 2. - data.T)

        let p7 = new Point2d(data.E, 0.)

        let hAxis = new Line2d(Point2d.Origin,Vector2d.XAxis)

        let p8  = p6.Mirror(hAxis)
        let p9  = p5.Mirror(hAxis)
        let p10 = p4.Mirror(hAxis)
        let p11 = p3.Mirror(hAxis)
        let p12 = p2.Mirror(hAxis)
        let p13 = p1.Mirror(hAxis)

        [
            yield! Creator.polygon 
                [|
                    p1,0.
                    p3,0.
                    p11,0.
                    p13,0.
                |]
            yield new Line(pto3d p2, pto3d p5) :>Entity
            yield new Line(pto3d p4, pto3d p6) :>Entity
            yield new Line(pto3d p12,pto3d p9):>Entity
            yield new Line(pto3d p10,pto3d p8):>Entity

            yield new Circle(pto3d p7,Vector3d.ZAxis,0.5*data.D):>Entity
        ]
    let side (data:单槽钢吊杆座) =

        let p1 = new Point2d(data.C / 2., 0.)
        let p2 = new Point2d(data.C / 2.,  -data.A)
        let p3 = new Point2d(data.C / 2. - data.T, -data.A)
        let p4 = new Point2d(data.C / 2. - data.T, -data.T)


        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p5 = p4.Mirror(vAxis)
        let p6 = p3.Mirror(vAxis)
        let p7 = p2.Mirror(vAxis)
        let p8 = p1.Mirror(vAxis)

        let p9  = new Point2d(data.D/2.,0.)
        let p10 = new Point2d(data.D/2.,-data.T)

        [
            yield! Creator.polygon 
                [|
                    p1,1.5*data.T
                    p2,0.
                    p3,0.
                    p4,0.5*data.T
                    p5,0.5*data.T
                    p6,0.
                    p7,0.
                    p8,1.5*data.T
                |]
            let l1 = new Line(pto3d p9,pto3d p10) :> Entity
            yield l1
            yield l1 |> mirror vAxis
        ]

    [<CommandMethod("单槽钢吊杆座")>]
    member this.Drawer() =
        let objects = Lake.Hangers.单槽钢吊杆座.DataRecords
        let views = ["正面", front;"顶面",top;"侧面",side]
        Part.draw objects views getKey









