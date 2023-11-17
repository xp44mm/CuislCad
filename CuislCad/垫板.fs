namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System

open Convert
open Creator
open Lake.Screw

type 垫板Drawer() =
    // 实例的唯一标识
    let getKey (data:垫板) = sprintf "%f" data.M

    let front (data:垫板) =
        let rect =
            rectangle (new Point2d(-0.5*data.A, -0.5*data.A)) (new Vector2d(data.A,data.A))
        [
            yield! rect
            yield  circle(Point2d.Origin,0.5*data.D):>Entity
        ]
    let side (data:垫板) =
        rectangle (new Point2d(-0.5*data.A, 0.)) (new Vector2d(data.A,data.T))
        |> Array.toList

    [<CommandMethod("垫板")>]
    member this.Drawer() =
        let objects = Lake.Screw.垫板.DataRecords
        let views = ["正面", front;"侧面",side]
        Part.draw objects views getKey
