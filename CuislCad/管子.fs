namespace FsharpCad
open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
//open Cuisl

open Lake.GbPipes

module PipeModule =
    let Dws = Lake.GbPipes.PipeOutsideDiameter.DataRecords
        
    let Thicks = Lake.GbPipes.PipeWallThick.DataRecords

    let getKey(data:PipeOutsideDiameter) = sprintf "%f" data.Dw 

    let front (data:PipeOutsideDiameter) =
        [
            circle(p0,data.Dw/2.):>Entity
//            circle(p0,data.Dw/2.-data.t):>Entity
        ]

    let side (data:PipeOutsideDiameter) =
        let vAxis = new Line2d(p0,vecy)
        let p1 = new Point2d(data.Dw / 4., 0.)
        let p2 = p1.Mirror(vAxis)

        [
            ellipseArc p1 (data.Dw / 4.*vecx) 0.5 0. Math.PI :> Entity
            ellipse    p2 (data.Dw / 4.*vecx) 0.5       :> Entity
        ]

type SteelPipeDrawer() =

    [<CommandMethod("管子")>]
    member this.Drawer() =
        let views = ["前面",PipeModule.front;"侧面",PipeModule.side]
        Part.draw PipeModule.Dws views PipeModule.getKey



