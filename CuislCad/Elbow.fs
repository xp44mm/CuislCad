namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
//open Cuisl

open Lake.GbPipes

module modElbow =
    let pipes = PipeModule.Dws

    let getKey (data:Elbow) = sprintf "%f" data.DN


    let top (data:Elbow) =
        let dw =
            PipeModule.Dws
            |> Array.find(fun p -> p.DN=data.DN)
            |> fun p -> p.Dw
        let hAxis = new Line2d(p0,vecx)
            
        let p1 = new Point2d(0., dw / 2.)
        let p2 = new Point2d(data.R, dw / 2.)
        let p3 = p2.Mirror hAxis
        let p4 = p1.Mirror hAxis

        [
            yield! sequential
                [|
                    p1, 0.
                    p2, 0.
                    p3, 0.
                    p4, 0.
                |]
            yield arc(p0, 0.5*dw,0.5*Math.PI,1.5*Math.PI) :> Entity
        ]

    let side angle (data:Elbow) =
        let dw =
            pipes
            |> Array.find(fun p -> p.DN=data.DN)
            |> fun p -> p.Dw

        let v1 = Vector2d.XAxis
        let v2 = Vector2d.XAxis.RotateBy(Math.PI - angle)

        

        let arcc = arcFrom p0 v1 v2  data.R
        let arcw = arc(pto2d arcc.Center,(data.R + 0.5*dw),-angle-0.5*Math.PI,-0.5*Math.PI)
        let arcn = arc(pto2d arcc.Center,(data.R - 0.5*dw),-angle-0.5*Math.PI,-0.5*Math.PI)
        
        [
            yield arcc :> Entity
            yield arcw :> Entity
            yield arcn :> Entity

            yield line(pto2d arcw.StartPoint,pto2d arcn.StartPoint) :> Entity
            yield line(pto2d arcw.EndPoint  ,pto2d arcn.EndPoint  ) :> Entity

        ]

    let singleLine angle (data:Elbow) =
        let dw =
            pipes
            |> Array.find(fun p -> p.DN=data.DN)
            |> fun p -> p.Dw

        //由交叉点指向切点
        let v1 = Vector2d.XAxis
        let v2 = Vector2d.XAxis.RotateBy(Math.PI - angle)

        let arc = arcFrom p0 v1 v2 data.R

        [
            yield arc :> Entity
            yield circle(pto2d arc.StartPoint,0.05*dw) :> Entity
            yield circle(pto2d arc.EndPoint  ,0.05*dw) :> Entity
        ]

open Autodesk.AutoCAD.EditorInput
open System.Windows
open System.Windows.Controls
open AutoCADWpf

type ElbowDrawer() =
    
    [<CommandMethod("弯头")>]
    member this.Drawer() =
        let w = new PartWindow(Title = "弯头")
        let dgObjects = w.FindName("dgObjects"):?>DataGrid
        let cbViews = w.FindName("cbViews"):?>ComboBox
        
        let objects = Lake.GbPipes.Elbow.DataRecords

        dgObjects.ItemsSource <- objects
        cbViews.ItemsSource <- ["顶面";"侧面";"单线图";]

        if w.ShowDialog().Value then
            let data = w.Object :?> Elbow

            match w.View with
            |"顶面" ->
                let nm = sprintf "弯头-顶面-%s" (modElbow.getKey data)
                let entities = modElbow.top data
                let blockId = EntityOps.defineBlock nm entities

                //*在屏幕上获取两点
                let db,ed = 
                    let doc = Input.getDocument()
                    doc.Database,doc.Editor

                let ps1 = ed.GetPoint("请选择插入点")
                if ps1.Status <> PromptStatus.OK then () else
                let p1 = ps1.Value 

                let ps2 = ed.GetPoint(new PromptPointOptions("选取方向点",UseBasePoint = true, UseDashedLine = true, BasePoint = p1))
                if ps2.Status <> PromptStatus.OK then () else
                let p2 = ps2.Value 

                Part.getBr2 p1 p2 blockId |> EntityOps.drawEntity db.CurrentSpaceId

            |"侧面"|"单线图" ->
                //*在屏幕上获取3点
                let db,ed = 
                    let doc = Input.getDocument()
                    doc.Database, doc.Editor


                let ps1 = ed.GetPoint("请选择插入点")
                if ps1.Status <> PromptStatus.OK then () else
                let p1 = ps1.Value 

                let ps2 = ed.GetPoint(new PromptPointOptions("选取方向点1",UseBasePoint = true, UseDashedLine = true, BasePoint = p1))
                if ps2.Status <> PromptStatus.OK then () else
                let p2 = ps2.Value 

                let ps3 = ed.GetPoint(new PromptPointOptions("选取方向点2",UseBasePoint = true, UseDashedLine = true, BasePoint = p1))
                if ps3.Status <> PromptStatus.OK then () else
                let p3 = ps3.Value 

                let ucs = ed.CurrentUserCoordinateSystem

                let xoy =
                    let plane = new Plane(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis)
                    plane.TransformBy(ucs)
                    plane

                let zaxis = Vector3d.ZAxis.TransformBy(ucs)

                let v1 = (p2 - p1).TransformBy(ucs)
                let v2 = (p3 - p1).TransformBy(ucs)

                let angle = Math.PI - v1.GetAngleTo(v2)
                let nm = sprintf "弯头-%s-%s-%f" w.View (modElbow.getKey data) (angle/Math.PI * 180.)
                let entities = (if w.View="侧面" then modElbow.side else modElbow.singleLine) angle data
                let blockId = EntityOps.defineBlock nm entities
                    
                let br = new BlockReference(p1, blockId)

                br.Rotation <- 
                    if v1.CrossProduct(v2).IsCodirectionalTo(zaxis) then
                        v1.Convert2d(xoy).Angle
                    else
                        v2.Convert2d(xoy).Angle

                br.TransformBy(ucs)

                br |> EntityOps.drawEntity db.CurrentSpaceId
            | _ -> ()
