namespace FsharpCad

open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime
open Creator
open Lake.ShapeSteel

module 扁钢Module =
    let getKey (data:扁钢) = sprintf "%s" data.Spec

    let section (data:扁钢) =     
        rectangle Point2d.Origin (new Vector2d(data.Width,data.Thick)) 
        |> Array.toList 
        
type 扁钢Drawer() =
    [<CommandMethod("扁钢")>]
    member this.Drawer() =        
        let objects = Lake.ShapeSteel.扁钢.DataRecords

        let views = ["截面", 扁钢Module.section]
        Part.draw objects views 扁钢Module.getKey
