namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System

open Convert
open Creator
//open Cuisl
open Lake.WeldingPipes 

type 焊接管座Drawer() =
    // 实例的唯一标识
    let getKey (data:焊接管座) = sprintf "%f" data.Dw

    let precalc (data:焊接管座) =
        let fh =
            let halfAngle =
                match data.Dw with
                | dw when dw<160.-> Math.PI/3.
                | _ -> asin(data.B / data.Dw)

            0.5*data.Dw*cos(halfAngle)

        let fs =
            match data.Dw with
            | dw when dw<160.-> data.Dw * sin(Math.PI/3.)
            | _ -> data.B
        fh,fs

    let front (data:焊接管座) =        
        let fh,fs = precalc data

        let vAxis = new Line2d(p0,vecy)

        let p1 = new Point2d(fs / 2., -fh)
        let p2 = new Point2d(data.B / 2., -data.H)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)
        [
            yield! Creator.sequential 
                [|
                    p1, 0.
                    p2, 2. * data.T
                    p3, 2. * data.T
                    p4, 0.
                |]
        ]

    let side (data:焊接管座) =
        let fh,fs = precalc data
        let vAxis = new Line2d(p0,vecy)

        let p1 = new Point2d(data.A / 2., -fh)
        let p2 = new Point2d(data.A / 2., -data.H)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        [
            yield! Creator.polygon 
                [|
                    p1, 0.
                    p2, 0.
                    p3, 0.
                    p4, 0.
                |]
        ]
    [<CommandMethod("焊接管座")>]
    member this.Drawer() =
        let objects = Lake.WeldingPipes.焊接管座.DataRecords
        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey
    