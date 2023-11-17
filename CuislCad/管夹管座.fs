namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System

open Convert
open Creator
//open Cuisl
open Lake.Clasps

type 管夹管座Drawer() =
    // 实例的唯一标识
    let getKey (data:管夹管座) = sprintf "%f" data.Dw

    let precalc (data:管夹管座) =
        let clasp = 
            Lake.Clasps.管夹管座.DataRecords 
            |> Array.find(fun row -> row.Dw = data.Dw )

        let od = data.Dw + 2. * clasp.T

        let fh =
            let halfAngle =
                match data.Dw with
                | dw when dw<160.-> Math.PI/3.
                | _ -> asin(data.B / od)

            0.5*od*cos(halfAngle)

        let fs =
            match data.Dw with
            | dw when dw<160.-> od * sin(Math.PI/3.)
            | _ -> data.B
        clasp,od,fh,fs

    let front (data:管夹管座) =
        let clasp,od,fh,fs = precalc data

        let p1 = new Point2d(fs / 2., -fh)
        let p2 = new Point2d(data.B / 2., -data.H)
        let p3 = new Point2d(-data.B / 2., -data.H)
        let p4 = new Point2d(-fs / 2., -fh)
        [
            yield! Creator.sequential 
                [|
                    p1, 0.
                    p2, 2. * data.T
                    p3, 2. * data.T
                    p4, 0.
                |]
            yield circle(Point2d.Origin,0.5*od):>Entity
        ]

    let side (data:管夹管座) =
        let clasp,od,fh,fs = precalc data
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(data.A / 2., -fh)
        let p2 = new Point2d(data.A / 2., -data.H)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        let p5 = new Point2d(clasp.B / 2., -fh)
        let p6 = new Point2d(clasp.B / 2., 0.5*od)
        let p7 = p6.Mirror(vAxis)
        let p8 = p5.Mirror(vAxis)

        [
            yield! Creator.polygon 
                [|
                    p1, 0.
                    p2, 0.
                    p3, 0.
                    p4, 0.
                |]
            yield! Creator.sequential
                [|
                    p5, 0.
                    p6, 0.
                    p7, 0.
                    p8, 0.
                |]
        ]
    [<CommandMethod("管夹管座")>]
    member this.Drawer() =
        let objects = Lake.Clasps.管夹管座.DataRecords
        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey
    