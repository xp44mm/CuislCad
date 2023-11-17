namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System

open Convert
open Creator
//open Cuisl

open Lake.Screw

type 地脚螺栓Drawer() =
    // 实例的唯一标识
    let getKey (data:地脚螺栓) = sprintf "%f" data.M

    let side (data:地脚螺栓) =
//        let alpha = 5.*data.m
//        let a = 5.*data.m
//        let l = 100.
        let l0 = 20.*data.M

        let r1 = data.D / 2. + data.M
        let p1 = new Point2d(-0.5*data.M, data.H - r1)

        let alpha = p1.GetAsVector().Angle + acos(r1 / p1.GetDistanceTo(Point2d.Origin))
        
        [
            yield circle(Point2d.Origin, 0.5*data.D):>Entity
            let arc = arc(Point2d.Origin, r1, alpha, Math.PI - alpha)

            yield arc:>Entity
            yield line(p1, pto2d arc.StartPoint):>Entity
            yield line(p1, new Point2d(-0.5*data.M, l0 - r1)):>Entity
        ]

    [<CommandMethod("地脚螺栓")>]
    member this.Drawer() =
        let objects = Lake.Screw.地脚螺栓.DataRecords

        let views = ["侧面", side]
        Part.draw objects views getKey

