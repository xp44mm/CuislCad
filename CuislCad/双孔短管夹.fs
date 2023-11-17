namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System
open Convert
open Creator
//open Cuisl

open Lake.Clasps

type 双孔短管夹Drawer() =
    // 实例的唯一标识
    let getKey (data:双孔短管夹) = sprintf "%f" data.Dw



    let front (data:双孔短管夹) =
        let theta = 2. * asin((data.E + 2. * data.T) / (data.Dw + 2. * data.T))
        let fh = (data.Dw + 2. * data.T) / 2. * cos(theta / 2.)

        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(data.T + data.E / 2., data.H + data.G)
        let p2 = new Point2d(data.T + data.E / 2., fh)
        let p3 = new Point2d(data.T + data.E / 2., -fh)
        let p4 = new Point2d(data.T + data.E / 2., -data.H1 - data.G)
            
        let leftEnt =
            [
                line(p1,p2):>Entity
                line(p3,p4):>Entity
                arc(Point2d.Origin,data.Dw / 2. + data.T, theta / 2. - Math.PI / 2., Math.PI / 2. - theta / 2.) :> Entity
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
            
    let side (data:双孔短管夹) =
        [
            yield! rectangle (new Point2d(data.B / 2., data.H + data.G))(-new Vector2d(data.B,data.H+data.H1+2.*data.G))
            yield circle(new Point2d(0., data.H),data.M / 2. + 1.)  :>Entity
            yield circle(new Point2d(0., -data.H1),data.M / 2. + 1.):>Entity
        ]

    [<CommandMethod("双孔短管夹")>]
    member this.Drawer() =
        let objects = Lake.Clasps.双孔短管夹.DataRecords
        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey



