namespace FsharpCad

open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System
open Creator
open Convert
//open Cuisl

open Lake.ShapeSteel
open Lake.WeldingPipes

module 槽钢支座Module =
    let getKey (data:槽钢支座) = sprintf "%f" data.Dw

    let channel(data:槽钢支座) =
        Lake.ShapeSteel.槽钢.DataRecords
        |> Seq.find(fun c -> c.Spec = data.ChannelSpec)

    let front (data:槽钢支座) =
        let ch = channel data
        let ents = 槽钢module.section ch
        ents 
        |> List.map(
            move (new Vector2d(-data.HH, -ch.H / 2.))
            >> rotateBy (0.5*Math.PI) Point2d.Origin)

    let side (data:槽钢支座) =
        let ch = channel data
        let p = new Point2d(-data.A / 2., -data.HH)
        let v = new Vector2d(data.A,ch.B)
        rectangle p v
        |> Array.toList

type 槽钢支座Drawer() =    

    [<CommandMethod("槽钢支座")>]
    member this.Drawer() =
        let objects = Lake.WeldingPipes.槽钢支座.DataRecords
        let views = ["前面",槽钢支座Module.front;"侧面",槽钢支座Module.side]
        Part.draw objects views 槽钢支座Module.getKey
