namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Lake.Hangers

module U形耳子Module =
    let getKey (data:U形耳子) = sprintf "%f" data.M

    let front (data:U形耳子) =
        let p1  = new Point3d(-0.5*data.A, 0.0             , 0.0)
        let p2  = new Point3d( 0.5*data.A, 0.0             , 0.0)
        let p3  = new Point3d( 0.5*data.A, data.H          , 0.0)
        let p4  = new Point3d( 0.5*data.D, data.H          , 0.0)
        let p5  = new Point3d( 0.5*data.D, data.H1         , 0.0)
        let p6  = new Point3d(-0.5*data.D, data.H1         , 0.0)
        let p7  = new Point3d(-0.5*data.D, data.H          , 0.0)
        let p8  = new Point3d(-0.5*data.A, data.H          , 0.0)
        let p9  = new Point3d(-0.6*data.A, data.H - data.H2, 0.0)
        let p10 = new Point3d( 0.6*data.A, data.H - data.H2, 0.0)
        [
            new Line(p1, p2)  :>Entity
            new Line(p2, p3)  :>Entity
            new Line(p3, p4)  :>Entity
            new Line(p4, p5)  :>Entity
            new Line(p5, p6)  :>Entity
            new Line(p6, p7)  :>Entity
            new Line(p7, p8)  :>Entity
            new Line(p8, p1)  :>Entity

            new Line(p9, p10) :>Entity
        ]

    let side (data:U形耳子) =
        let p1 = new Point3d(- data.B / 2.0, 0.0    , 0.0)
        let p2 = new Point3d(+ data.B / 2.0, 0.0    , 0.0)
        let p3 = new Point3d(+ data.B / 2.0, data.H , 0.0)
        let p4 = new Point3d(- data.B / 2.0, data.H , 0.0)
        let p5 = new Point3d(- data.B / 2.0, data.H1, 0.0)
        let p6 = new Point3d(+ data.B / 2.0, data.H1, 0.0)
        let p7 = new Point3d(0.0, data.H - data.H2  , 0.0)
        [
            new Line(p1, p2)                             :>Entity
            new Line(p2, p3)                             :>Entity
            new Line(p3, p4)                             :>Entity
            new Line(p4, p1)                             :>Entity

            new Line(p5, p6)                             :>Entity

            new Circle(p7, Vector3d.ZAxis, 0.5*data.D1)  :>Entity
        ]
    

type U形耳子Drawer() =
    [<CommandMethod("U形耳子")>]
    member this.Drawer() =
        let objects =  Lake.Hangers.U形耳子.DataRecords
        let views = ["前面",U形耳子Module.front;"侧面",U形耳子Module.side]
        Part.draw objects views U形耳子Module.getKey
