namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime
open System
open Convert
open Creator
open Lake.WeldingPipes 

type 热压弯头托座Drawer() =
    // 实例的唯一标识
    let getKey (data:热压弯头托座) = sprintf "%f" data.Dw

    let front (data : 热压弯头托座) =
        [
            //底板
            let p1 = new Point2d(-data.B / 2., -data.H)
            let v1 = vector(data.B,data.T)
            yield! rectangle p1 v1

            //支撑
            let p5 = new Point2d(data.Dw1 / 2., data.T - data.H)
            let p6 = new Point2d(data.Dw1 / 2., data.L1 + data.T - data.H)
            yield line(p5,p6):>Entity

            let p7 = new Point2d(-data.Dw1 / 2., data.T - data.H)
            let p8 = new Point2d(-data.Dw1 / 2., data.L + data.T - data.H)
            yield line(p7,p8):>Entity
        ]



    [<CommandMethod("热压弯头托座")>]
    member this.Drawer() =
        let objects = Lake.WeldingPipes.热压弯头托座.DataRecords
        let views = [ "前面", front ]
        Part.draw objects views getKey

