namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System
open Convert
open Creator
//open Cuisl
open Lake.Clasps 

module U形螺栓管卡Module =
    let getKey (data:U形螺栓管卡) = sprintf "%f" data.Dw

    let front (data:U形螺栓管卡) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(0.5*data.C, 0.)
        let p2 = new Point2d(0.5*data.C, -data.H)

        let ln1 = line(p1,p2):>Entity
        let ln2 = ln1 |> mirror vAxis
        let arc = arc(Point2d.Origin, 0.5*data.C, 0., Math.PI) :> Entity

        //汇总
        [
            ln1;ln2;arc
        ]
            
    let side (data:U形螺栓管卡) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(0.5*data.D,0.5*data.C)
        let p2 = new Point2d(0.5*data.D,-data.H)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        //汇总
        [
            yield! Creator.sequential
                [|
                    p1, 0.
                    p2, 0.
                    p3, 0.
                    p4, 0.
                |]

            yield arc(new Point2d(0.,0.5*data.C), 0.5*data.D, 0., Math.PI) :> Entity
        ]

type U形螺栓管卡Drawer() =
    [<CommandMethod("U形螺栓管卡")>]
    member this.Drawer() =
        let objects = Lake.Clasps.U形螺栓管卡.DataRecords
        let views = ["前面",U形螺栓管卡Module.front;"侧面",U形螺栓管卡Module.side]
        Part.draw objects views U形螺栓管卡Module.getKey

