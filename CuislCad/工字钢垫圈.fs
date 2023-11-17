namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System

open Convert
open Creator
open Lake.Screw 

type 工字钢垫圈Drawer() =
    // 实例的唯一标识
    let getKey (data:工字钢垫圈) = sprintf "%f" data.M

    let front (data:工字钢垫圈) =
        let p1 = new Point2d( 0.5* data.B, 0.)
        let p2 = new Point2d(-0.5* data.B, 0.)
        let p3 = new Point2d(-0.5* data.B, data.H)
        let p4 = new Point2d( 0.5* data.B, data.H + data.B / 6.)

        Creator.polygon
            [|
                p1, 0.
                p2, 0.
                p3, 0.
                p4, 0.
            |]
    [<CommandMethod("工字钢垫圈")>]
    member this.Drawer() =
        let objects = Lake.Screw.工字钢垫圈.DataRecords
        let views = ["正面", front]
        Part.draw objects views getKey

