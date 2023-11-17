namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System

open Convert
open Creator
//open Cuisl
open Lake.Clasps 

type 管夹滑动支座Drawer() =
    // 实例的唯一标识
    let getKey (data:管夹滑动支座) = sprintf "%f" data.Dw

    let precalc (data:管夹滑动支座) =
        let dianpianThick = 3.
        let dibanThick = data.T2
        let sectChordLength,sectHeight =
            let r = data.Dw / 2. + data.T1
            if data.Dw < 160. then
                let a = 2. * Math.PI / 3.
                sector.chordLength r a,sector.height r a
            else
                data.B,sector.height r (sector.getAngleFromChordLength r data.B)
        dianpianThick,dibanThick,sectChordLength,sectHeight

    let front (data:管夹滑动支座) =
        let dianpianThick,dibanThick,sectChordLength,sectHeight = precalc data
        let vAxis = new Line2d(Point2d.Origin, Vector2d.YAxis)

        let p1 = new Point2d(sectChordLength / 2., -sectHeight)
        let p2 = new Point2d(data.B / 2., -data.H + dibanThick + 2. * dianpianThick)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        let p5 = new Point2d(data.B / 2., -data.H + dibanThick + dianpianThick)
        let p6 = p5.Mirror(vAxis)

        let p7 = new Point2d(-data.B1 / 2., -data.H)

        [
            yield circle(Point2d.Origin,data.Dw / 2. + data.T1):>Entity
            yield! Creator.sequential 
                [|
                    p1, 0.
                    p2, 2. * data.T1
                    p3, 2. * data.T1
                    p4, 0.
                |]
            yield line(p5,p6):>Entity
            yield! rectangle p7 (new Vector2d(data.B1,dibanThick))
        ]

    let side (data:管夹滑动支座) =
        let dianpianThick,dibanThick,sectChordLength,sectHeight = precalc data
        let clasp = 
            Lake.Clasps.双孔短管夹.DataRecords 
            |> Array.find(fun row -> row.Dw = data.Dw )

        let outradius = data.Dw / 2. + data.T1
        let vAxis = new Line2d(Point2d.Origin, Vector2d.YAxis)
        //管夹
        let c1 = new Point2d(data.A / 2., -sectHeight)
        let c2 = new Point2d(data.A / 2., outradius)
        let c3 = new Point2d(data.A / 2. - clasp.B, data.Dw / 2. + data.T1)
        let c4 = new Point2d(data.A / 2. - clasp.B, -sectHeight)

        //支撑
        let z1 = new Point2d(-data.A / 2., -sectHeight)
        
        //滑动垫片的中缝
        let s1 = new Point2d(-data.A / 2., -data.H + dibanThick + dianpianThick)
        let s2 = s1.Mirror(vAxis)
        
        //底板
        let d1 = new Point2d(-data.A1 / 2., -data.H)
        let d2 = new Point2d(data.A1 / 2., -data.H + dibanThick)

        [
            let gj = Creator.sequential 
                        [|
                            c1,0.
                            c2,0.
                            c3,0.
                            c4,0.         
                        |]
            yield! gj
            yield! gj |> List.map(mirror vAxis)
            yield! rectangle z1 (new Vector2d(data.A, dibanThick + 2. * dianpianThick + sectHeight - data.H))

            yield line(s1,s2):>Entity

            yield! rectangle d1 (new Vector2d(data.A1, dibanThick))

        ]
    [<CommandMethod("管夹滑动支座")>]
    member this.Drawer() =
        let objects = Lake.Clasps.管夹滑动支座.DataRecords
        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey


