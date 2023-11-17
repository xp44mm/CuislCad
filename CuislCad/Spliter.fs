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
open Convert
//open Cuisl

type Spliter() =
    ///优化的外半径
    let ropt width radius = 
        let r = radius/width
        radius + width - exp(-r)*width

    let cr width radius number = 
        (radius/(radius+width))**(1.0/(float number + 1.0))
    
    let calculate width radius number = 
        let cr = cr width radius number
        
        let next (r0,w0,ropt0) =
            let r1 = r0 / cr
            let w1 = r1 - r0
            let ropt1 = ropt w1 ropt0
            Some((r0,w0,ropt0),(r1,w1,ropt1))

        let r0 = radius
        let w0 = 0.0
        let ropt0 = radius

        (r0,w0,ropt0)
        |> Seq.unfold next
        |> Seq.take (number+2)

    [<Runtime.CommandMethod("Spliter")>]
    member this.Spliter() =
        let doc,ed,db=init()

        let form = new AutoCADWpf.SpliterWindow()
        let btnCalc = form.FindName("btnCalc") :?> Button
        let dgSpliters = form.FindName("dgSpliters") :?> DataGrid

        btnCalc.Click.Add(fun _ ->
            let w = form.InputWidth
            let r = form.Radius
            let n = form.Number
            form.Spliters <-
                calculate w r n
                |> Seq.map(fun (r,w,ropt) -> new AutoCADWpf.Spliter(Radius=r,OptRadius=ropt))
                |> Seq.toArray

            dgSpliters.ItemsSource <- form.Spliters)            
                    
        if form.ShowDialog().Value then
            let angle = form.Angle * Math.PI / 180.
            
            Input.getOnePoint "请选择插入点：\n" (fun p ->
                let p = pto2d p
                let t1 = - Vector2d.XAxis
                let t2 = Vector2d.XAxis.RotateBy(-angle)

                let entities =
                    form.Spliters
                    |> Array.collect(fun s ->
                        let p1 = p + Vector2d.YAxis*s.Radius
                        let l1 = new Line2d(p1,t1)

                        let p2 = p+Vector2d.YAxis.RotateBy(-angle)*(form.Radius + (s.Radius - form.Radius)/form.InputWidth*form.OutputWidth)
                        let l2 = new Line2d(p2,t2)

                        let p0 = l1.IntersectWith(l2).[0]
                        [|
                            Creator.arcFrom p0 t1 t2 s.OptRadius :> Entity;
                            new Line(new Point3d(XOY,p0),new Point3d(XOY,p1)) :> Entity;
                            new Line(new Point3d(XOY,p0),new Point3d(XOY,p2)) :> Entity;
                        |])

                use trans = db.TransactionManager.StartTransaction()
                let space = 
                    trans.GetObject(db.CurrentSpaceId, DatabaseServices.OpenMode.ForWrite)
                    :?>BlockTableRecord

                entities
                |> Array.iter(fun ent -> 
                    space.AppendEntity(ent)|> ignore
                    trans.AddNewlyCreatedDBObject(ent, true))

                trans.Commit()
            )

