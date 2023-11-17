namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
//open Cuisl
open Lake.WeldingPipes 

type 立管支撑板Drawer() =
    // 实例的唯一标识
    let getKey (data:立管支撑板) = sprintf "%f" data.Dw

    let front (data:立管支撑板) =
        let vAxis = new Line2d(p0,vecy)
        let hAxis = new Line2d(p0,vecx)

        let p1 = new Point2d(data.Dw / 2., -data.T / 2.)
        let v1 = vector(data.B,data.T)

        let p3 = new Point2d(data.Dw / 2. + data.B0, data.T / 2.)
        let p4 = p3.Mirror(hAxis)
        
        let first =
            [
                yield! rectangle p1 v1 
                yield line(p3,p4) :> Entity        
            ]

        [
            yield! first
            yield! first |> List.map(mirror vAxis)
            yield! first |> List.map(rotateBy ( Math.PI/2.) p0)
            yield! first |> List.map(rotateBy (-Math.PI/2.) p0)

        ]

    let side (data:立管支撑板) =
        let vAxis = new Line2d(p0,vecy)

        let p1 = new Point2d(data.Dw / 2., data.H)
        let p2 = new Point2d(data.Dw / 2., 0.)
        let p3 = new Point2d(data.Dw / 2. + data.B, 0.)
        let p4 = new Point2d(data.Dw / 2. + data.B, data.H0)
        let p5 = new Point2d(data.Dw / 2. + data.B0, data.H)
        
        let p6 = new Point2d(-data.T / 2., 0.)
        let p7 = new Point2d(-data.T / 2., data.H0)
        let p8 = p7.Mirror(vAxis)
        let right = polygon 
                        [|
                            p1,0.
                            p2,0.
                            p3,0.
                            p4,0.
                            p5,0.                        
                        |]
     

        //汇总
        let vAxis = new Line2d(p0,vecy)
        [
            yield! right
            yield! right |> List.map(mirror vAxis)
            
            yield! rectangle p6 (vector(data.T,data.H))
            yield line(p7,p8):>Entity

        ]

    [<CommandMethod("立管支撑板")>]
    member this.Drawer() =
        let objects = Lake.WeldingPipes.立管支撑板.DataRecords
        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey

