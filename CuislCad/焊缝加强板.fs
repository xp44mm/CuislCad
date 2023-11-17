namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime
open Creator

open Lake.Fujian

module 焊缝加强板Module =
    let getKey (data:焊缝加强板) = sprintf "%d" data.POS 
    let front (data:焊缝加强板) =
        let p1 = new Point2d(data.A, 0.0    )
        let p2 = new Point2d(data.A, data.A1)
        let p3 = new Point2d(data.A1, data.A)
        let p4 = new Point2d(0.0, data.A    )

        [
            line(Point2d.Origin, p1)  :> Entity
            line(p1, p2)              :> Entity
            line(p2, p3)              :> Entity
            line(p3, p4)              :> Entity
            line(p4, Point2d.Origin)  :> Entity
        ]

    let side (data:焊缝加强板) =
            let p1 = new Point2d( data.T * 0.5, 0.0    )
            let p2 = new Point2d( data.T * 0.5, data.A )
            let p3 = new Point2d(-data.T * 0.5, data.A )
            let p4 = new Point2d(-data.T * 0.5, 0.0    )
            let p5 = new Point2d( data.T * 0.5, data.A1)
            let p6 = new Point2d(-data.T * 0.5, data.A1)
        
            [
                line(p1, p2) :> Entity
                line(p2, p3) :> Entity
                line(p3, p4) :> Entity
                line(p4, p1) :> Entity

                line(p5, p6) :> Entity
            ]


type 焊缝加强板Drawer()=
    [<CommandMethod("焊缝加强板")>]
    member this.Drawer() =
        let objects = Lake.Fujian.焊缝加强板.DataRecords

        let views = ["前面",焊缝加强板Module.front;"侧面",焊缝加强板Module.side]
        Part.draw objects views 焊缝加强板Module.getKey
