namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
open Lake.WeldingPipes 

type 立管焊接双板Drawer() =
    // 实例的唯一标识
    let getKey (data:立管焊接双板) = sprintf "%f" data.Dw

    let front (data:立管焊接双板) =
        let rw = 0.5*data.Dw
        let ang1 = sector.getAngleFromChordLength rw data.H
        let ang2 = sector.getAngleFromChordLength rw (data.H - 2. * data.T)
        let secHeight1 = sector.height rw ang1
        let secHeight2 = sector.height rw ang2

        let hAxis = new Line2d(p0,vecx)
        let right = 
            [
                //立板线
                let p1 = new Point2d(secHeight1, data.H / 2.)
                let p2 = new Point2d(data.L / 2., data.H / 2.)
                let l1 = line(p1,p2):>Entity
                yield l1
                yield l1 |> mirror hAxis

                let p3 = new Point2d(secHeight2, data.H / 2. - data.T)
                let p4 = new Point2d(data.L / 2., data.H / 2. - data.T)
                let l2 = line(p3,p4):>Entity
                yield l2
                yield l2 |> mirror hAxis

                //底板轮廓与螺栓孔
                yield line(p2,p2-data.H*vecy):>Entity
                yield arc(p0,rw,-ang1/2.,ang1/2.):> Entity
                yield circle(new Point2d(data.C / 2., 0.),0.5*data.D):>Entity
            ]

        //汇总
        let vAxis = new Line2d(p0,vecy)
        [
            yield! right
            yield! right |> List.map(mirror vAxis)        
        ]

    let side (data:立管焊接双板) =
        let rw = 0.5*data.Dw
        let ang1 = sector.getAngleFromChordLength rw data.H
        let secHeight1 = sector.height rw ang1
        

        let p1 = new Point2d(secHeight1, data.B)
        let p2 = new Point2d(secHeight1, 0.)
        let p3 = new Point2d(data.L / 2., 0.)
        let p4 = new Point2d(data.L / 2., data.B1)
        let p5 = new Point2d(data.Dw / 2. + data.A, data.B)

        let p6 = new Point2d(data.C / 2., data.T)
        
        let right =
            [
                yield! polygon 
                    [
                        p1,0.
                        p2,0.
                        p3,0.
                        p4,0.
                        p5,0.
                    ]

                yield point(p6) :> Entity        
            ]

        //汇总
        let vAxis = new Line2d(p0,vecy)
        [
            yield! right
            yield! right |> List.map(mirror vAxis)        
        ]


    [<CommandMethod("立管焊接双板")>]
    member this.Drawer() =
        let objects = Lake.WeldingPipes.立管焊接双板.DataRecords

        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey

