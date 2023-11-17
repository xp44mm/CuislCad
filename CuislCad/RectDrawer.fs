namespace FsharpCad

open System
open System.Windows
open System.Windows.Controls

open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Colors

open Input
open EntityOps

type RectDrawer() =
    let rect(width:float,height:float,up:bool,down:bool)=
        let entities =
            let vec = new Vector3d(width/2.,height/2.,0.0)
            let p0  = new Point3d(-width/2., height/2.,0.)
            let p1  = new Point3d(-width/2.,-height/2.,0.)
            let p2  = new Point3d( width/2.,-height/2.,0.)
            let p3  = new Point3d( width/2., height/2.,0.)

            seq{
                yield new Line(p0,p1)               :> Entity
                yield new Line(p1,p2)               :> Entity
                yield new Line(p2,p3)               :> Entity
                yield new Line(p3,p0)               :> Entity
                if up   then yield new Line(p1,p3)  :> Entity
                if down then yield new Line(p0,p2)  :> Entity
            }
        let name =
            let s = String.Format("rect{0}x{1}",width,height)
            let s = if up   then s + "up"   else s
            let s = if down then s + "down" else s
            s
        entities,name

    [<Runtime.CommandMethod("RectangleBlock")>]
    member this.RectangleBlockCmd() =
        let frm = new AutoCADWpf.RectangleBlockWindow()
        if frm.ShowDialog().Value then
            let entities,name = 
                let width = frm.Dx 
                let height = frm.Dy
                let up = frm.Up
                let down = frm.Down
                rect(width,height,up,down)

            let blockId = EntityOps.defineBlock name entities

            let doc,ed,db=init()
            let ps = Input.getDocument().Editor.GetPoint("请选择矩形块的中心点：\n")
            if ps.Status = PromptStatus.OK then 
                let center = ps.Value

                let br = new BlockReference(center, blockId)
                br.TransformBy(ed.CurrentUserCoordinateSystem)

                EntityOps.drawEntity db.CurrentSpaceId (br:>Entity)

