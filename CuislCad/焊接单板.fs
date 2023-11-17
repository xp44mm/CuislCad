namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator

open Lake.WeldingPipes

type 焊接单板Drawer() =
    // 实例的唯一标识
    let getKey (data:焊接单板) = sprintf "%f" data.Dw

    let front (data:焊接单板) =
        let vAxis = new Line2d(p0,vecy)

        let p1 = new Point2d(data.T / 2., data.H - data.H2)
        let p2 = new Point2d(data.T / 2., data.H + data.H1)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        let p5 = new Point2d(0.,data.H)

        [
            yield! polygon 
                [
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                ]
            yield point p5 :> Entity        
        ]

    let side (data:焊接单板) =
        let vAxis = new Line2d(p0,vecy)

        let p1 = new Point2d(data.L1 / 2., data.H - data.H2)
        let p2 = new Point2d(data.L2 / 2., data.H + data.H1)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        let p5 = new Point2d(0.,data.H)

        [
            yield! polygon 
                [
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                ]
            yield circle(p5,data.D) :> Entity        
        ]

    [<CommandMethod("焊接单板")>]
    member this.Drawer() = 
        let objects = Lake.WeldingPipes.焊接单板.DataRecords
        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey

