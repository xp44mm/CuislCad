namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System

open Convert
open Creator

open Lake.Pedestals

type 钢管支撑Drawer() =
    // 实例的唯一标识
    let getKey (data:钢管支撑) = sprintf "%d" data.Pos

    let front (data:钢管支撑) =
        //底板
        let db =
            rectangle (new Point2d(-0.5*data.B, 0.)) (new Vector2d(data.B,data.T1))
        //管子
        let gz =
            rectangle (new Point2d(-0.5*data.D2, data.T1)) (new Vector2d(data.D2,data.Hmax))
        [
            yield! db
            yield! gz
        ]
    let top (data:钢管支撑) =
        //底板
        let db =
            rectangle (new Point2d(-0.5*data.B, -0.5*data.B)) (new Vector2d(data.B,data.B))
        [
            yield! db
            //'管子外圆和内圆
            yield circle(Point2d.Origin,0.5*data.D2):> Entity
            yield circle(Point2d.Origin,0.5*data.D2-data.T2):> Entity
        ]

    [<CommandMethod("钢管支撑")>]
    member this.Drawer() =
        let objects = Lake.Pedestals.钢管支撑.DataRecords
        let views = ["正面", front; "顶面",top]
        Part.draw objects views getKey
