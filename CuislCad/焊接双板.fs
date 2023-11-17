namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
//open Cuisl

open Lake.WeldingPipes

type 焊接双板Drawer() =
    // 实例的唯一标识
    let getKey (data:焊接双板) = sprintf "%f" data.Dw

    let precalc (data:焊接双板) =
        let alpha = 2. * asin(data.B / data.Dw)
        let fh1 = 
            data.Dw / 2. * cos(alpha / 2.)
        let fh2 =
            let beta = 2. * asin((data.B - 2. * data.T2) / data.Dw)
            data.Dw / 2. * cos(beta / 2.)
        alpha,fh1,fh2

    let front (data:焊接双板) =
        let alpha,fh1,fh2 = precalc data
        let vAxis = new Line2d(p0,vecy)

        let p1 = new Point2d(data.B / 2., fh1)
        let p2 = new Point2d(data.B / 2., data.H + data.H1)

        let p3 = new Point2d(data.B / 2. - data.T2, fh2)
        let p4 = new Point2d(data.B / 2. - data.T2, data.H + data.H1)

        let p5 = new Point2d(0., data.H)

        let l1 = line(p1,p2):>Entity
        let l2 = line(p3,p4):>Entity

        [

            line(p2,p2-data.B*vecx):>Entity
            l1
            l2
            circle(p5,data.D/2.):>Entity
            l1|> mirror vAxis
            l2|> mirror vAxis
            arc(p0, 0.5*data.Dw, Math.PI/2.-alpha/2., Math.PI/2.+alpha/2.):>Entity

        ]

    let side (data:焊接双板) =
        let alpha,fh1,fh2 = precalc data

        let vAxis = new Line2d(p0,vecy)
        let p1 = new Point2d(data.A1 / 2., fh1)
        let p2 = new Point2d(data.A2 / 2., data.H + data.H1)
        let p3 = p2.Mirror(vAxis)
        let p4 = p1.Mirror(vAxis)

        let p5 = new Point2d(0., data.H)

        let p6 = new Point2d(data.T1 / 2., data.H + data.H1)
        let p7 = new Point2d(data.T1 / 2., data.Dw / 2.)
        let p8 = p7.Mirror(vAxis)
        let p9 = p6.Mirror(vAxis)

        [
            yield! polygon 
                [
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                ]
            yield point(p5) :> Entity            
            yield! sequential 
                [
                    p6,0.
                    p7,0.
                    p8,0.
                    p9,0.
                ]          
        ]

    [<CommandMethod("焊接双板")>]
    member this.Drawer() =
        let objects = Lake.WeldingPipes.焊接双板.DataRecords
        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey

