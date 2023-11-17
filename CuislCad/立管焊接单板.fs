namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator

open Lake.WeldingPipes 

type 立管焊接单板Drawer() =
    // 实例的唯一标识
    let getKey (data:立管焊接单板) = sprintf "%f" data.Dw

    let front (data:立管焊接单板) =
        let vAxis = new Line2d(p0,vecy)
        let right = 
            [
                //轮廓
                let p1 = new Point2d(data.Dw / 2., -data.T / 2.)
                let v1 = vector((data.L-data.Dw)/2.,data.T)
                yield! rectangle p1 v1

                //螺栓孔
                yield point(new Point2d(data.C / 2., 0.)):>Entity

                //投影线
                let p4 = new Point2d(data.Dw / 2. + data.B2 - 10., data.T / 2.)
                let p5 = p4 - data.T*vecy

                yield line(p4,p5):>Entity   
            ]
        //汇总
        [
            yield! right
            yield! right |> List.map(mirror vAxis)        
        ]

    let side (data:立管焊接单板) =
        let vAxis = new Line2d(p0,vecy)

        let p1 = new Point2d(data.Dw / 2., 0.)
        let p2 = new Point2d(data.L / 2., 0.)
        let p3 = new Point2d(data.L / 2., -data.B2)
        let p4 = new Point2d(data.Dw / 2. + data.B2 - 10., -data.B)
        let p5 = new Point2d(data.Dw / 2., -data.B)
        let p6 = new Point2d(data.C / 2., -data.B1)
        
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

                yield circle(p6,0.5*data.D) :> Entity        
            ]

        //汇总
        [
            yield! right
            yield! right |> List.map(mirror vAxis)
        ]


    [<CommandMethod("立管焊接单板")>]
    member this.Drawer() =
        let objects = Lake.WeldingPipes.立管焊接单板.DataRecords
        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey

