namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System

open Convert
open Creator
//open Cuisl
open Lake.Clasps

type 长管夹Drawer() =
    // 实例的唯一标识
    let getKey (data:长管夹) = sprintf "%f" data.Dw

    let front (data:长管夹) =
        let alpha = 2. * asin((data.E + 2. * data.T) / (data.Dw + 2. * data.T))
        let fh1 = (data.Dw + 2. * data.T) / 2. * cos(alpha / 2.)
        let beta =
            let aa = data.E / 2. + data.T
            let bb = data.Dw / 2. + data.T
            let cc = data.H - data.G
            let xx = sqrt(aa ** 2. + cc ** 2.)
            (acos(bb / xx) + atan(aa / cc)) * 2.
        let fh2 = (data.Dw + 2. * data.T) / 2. * cos(beta / 2.)
        let fs2 = (data.Dw + 2. * data.T) * sin(beta / 2.)

        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(data.T + data.E / 2., data.H + data.G)
        let p2 = new Point2d(data.T + data.E / 2., data.H - data.G)
        let p3 = new Point2d(fs2 / 2., fh2)
        let p4 = new Point2d(data.T + data.E / 2., -fh1)
        let p5 = new Point2d(data.T + data.E / 2., -data.H1 - data.G)
            
        let leftEnt =
            [
                line(p1,p2):>Entity
                line(p2,p3):>Entity
                line(p4,p5):>Entity
                arc(Point2d.Origin,data.Dw / 2. + data.T, alpha / 2. - Math.PI / 2., Math.PI / 2. - beta / 2.):>Entity
            ]

        let p6 = new Point2d(data.E / 2. + 2. * data.T, data.H)
        let p7 = p6.Mirror(vAxis)

        let p8 = new Point2d(data.E / 2. + 2. * data.T, -data.H1)
        let p9 = p8.Mirror(vAxis)

        [
            yield! leftEnt
            yield! leftEnt |> List.map(fun e -> e |> mirror vAxis)
            yield line(p6,p7) :>Entity
            yield line(p8,p9) :>Entity
        ]
            
    let side (data:长管夹) =
        [

            yield! rectangle (new Point2d(data.B / 2., data.H + data.G))(-new Vector2d(data.B,data.H+data.H1+2.*data.G))
            yield circle(new Point2d(0., data.H),data.M / 2. + 1.)  :>Entity
            yield circle(new Point2d(0., -data.H1),data.M / 2. + 1.):>Entity
        ]

    [<CommandMethod("长管夹")>]
    member this.Drawer() =
        let objects = Lake.Clasps.长管夹.DataRecords
        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey



