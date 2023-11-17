namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System
open Convert
open Creator
//open Cuisl
open Lake.WeldingPipes

type 焊接滑动支座Drawer() =
    // 实例的唯一标识
    let getKey (data:焊接滑动支座) = sprintf "%f" data.Dw

    let precalc (data: 焊接滑动支座) =
        let dianpianThick = 3.
        let dibanThick = data.T

        let sectChordLength, sectHeight =
            let r = data.Dw / 2. + data.T
            if data.Dw < 160. then
                let a = 2. * Math.PI / 3.
                sector.chordLength r a, sector.height r a
            else
                data.B,
                sector.height r (sector.getAngleFromChordLength r data.B)
        dianpianThick, dibanThick, sectChordLength, sectHeight

    let front (data: 焊接滑动支座) =
        let dianpianThick, dibanThick, sectChordLength, sectHeight = precalc data
        let vAxis = new Line2d(Point2d.Origin, Vector2d.YAxis)

        //支撑
        let p1 = new Point2d(sectChordLength / 2., -sectHeight)
        let p2 = new Point2d(data.B / 2., dibanThick + 2. * dianpianThick - data.H)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        //滑动垫片的中缝
        let p5 = new Point2d(data.B / 2.,  dibanThick + dianpianThick-data.H)
        let p6 = p5.Mirror(vAxis)

        //底板
        let p7 = new Point2d(-data.B1 / 2., -data.H)

        [
            yield! Creator.sequential [| p1, 0.
                                         p2, 2. * data.T
                                         p3, 2. * data.T
                                         p4, 0. |]
            yield line (p5, p6) :> Entity
            yield! rectangle p7 (vector(data.B1, dibanThick)) ]

    let side (data: 焊接滑动支座) =
        let dianpianThick, dibanThick, sectChordLength, sectHeight =
            precalc data
        let vAxis = new Line2d(Point2d.Origin, Vector2d.YAxis)

        //支撑
        let p1 = new Point2d(0.5 * data.A, dibanThick + 2. * dianpianThick - data.H)
        let p2 = new Point2d(0.5 * data.A, -sectHeight)
        let p3 = new Point2d(0.2 * data.A, -sectHeight)
        let p4 = new Point2d(0.2 * data.A, -data.Dw / 2. - data.T)
        let p5 = p4.Mirror(vAxis)
        let p6 = p3.Mirror(vAxis)
        let p7 = p2.Mirror(vAxis)
        let p8 = p1.Mirror(vAxis)

        //滑动垫片的中缝
        let s1 = new Point2d(-data.A / 2., -data.H + dibanThick + dianpianThick)
        let s2 = s1.Mirror(vAxis)

        //底板
        let d1 = new Point2d(-data.A1 / 2., -data.H)
        [ 
            yield! polygon 
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                    p5,0.
                    p6,0.
                    p7,0.
                    p8,0.                
                |]
            yield  line (s1, s2) :> Entity
            yield! rectangle d1 (vector(data.A1, dibanThick)) ]

    [<CommandMethod("焊接滑动支座")>]
    member this.Drawer() =
        let objects = Lake.WeldingPipes.焊接滑动支座.DataRecords

        let views =
            [ "前面", front
              "侧面", side ]
        Part.draw objects views getKey