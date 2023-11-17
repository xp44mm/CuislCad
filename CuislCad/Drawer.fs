namespace FsharpCad

open System

open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Colors
open Input
open EntityOps

type Drawer() =
        
    [<Runtime.CommandMethod("TotalLength")>]
    member this.TotalLength()=
        let _,Ed,Db=init()
        let options = new EditorInput.PromptSelectionOptions()
        options.AllowDuplicates <- true
        options.MessageForAdding <- "请选择要计算的Line, Pline, Circle, Arc -->\n"

        let res = Ed.GetSelection(options)
        if not (res.Status = EditorInput.PromptStatus.OK) then ()
        let SS = res.Value
        let objIDs=SS.GetObjectIds()

        use trans=Db.TransactionManager.StartTransaction()

        let lst =new ResizeArray<string*float>()
        for id in objIDs do
            let ent =trans.GetObject(id, DatabaseServices.OpenMode.ForRead, true):?>Entity

            match ent with
            | :? Line as e->lst.Add ("Line",e.Length)
            | :? Polyline as e->lst.Add ("Polyline",e.Length)
            | :? Arc as e->lst.Add ("Arc",e.Length)
            | :? DatabaseServices.Circle as e->lst.Add ("Circle",e.Circumference)
            | _ ->()

        lst|>Seq.iter (fun (ty,len)-> 
            Ed.WriteMessage("本实体的类型为{0}，长度为{1} mm({1:#,##0} mm)\n", ty, len))

        let total=lst|>Seq.map (fun (_,len)->len)|>Seq.sum
        Ed.WriteMessage("实体的总长度为{0} mm({0:#,##0} mm)\n", total)

    [<Runtime.CommandMethod("角平分线")>]
    member this.角平分线()=
        let getLine msg=
            let _,ed,db=init()
            let ops = new EditorInput.PromptEntityOptions(msg)

            ops.SetRejectMessage("\n请选择直线:")
            ops.AddAllowedClass(typeof<DatabaseServices.Line>, true)

            let entRes = ed.GetEntity(ops)

            if entRes.Status <> EditorInput.PromptStatus.Error then
                use trans = db.TransactionManager.StartTransaction()
                match trans.GetObject(entRes.ObjectId, DatabaseServices.OpenMode.ForRead) with
                | :? Line as ln->
                    //实体中的点坐标为世界坐标
                    (new Geometry.LineSegment3d(ln.StartPoint, ln.EndPoint),ln.GetClosestPointTo(entRes.PickedPoint.TransformBy(ed.CurrentUserCoordinateSystem), false))                    
                | _ -> failwith ""
            else
                failwith ""

        let ln1,pp1=getLine "\n请选择第一条直线"
        let ln2,pp2=getLine "\n请选择第二条直线"

        let xl=new Xline()
        if ln1.Direction.IsParallelTo(ln2.Direction) then
            xl.BasePoint<-(ln1.PointOnLine+ln2.PointOnLine.GetAsVector())/2.0
            xl.UnitDir<-ln1.Direction
        else
            let InsPoint=ln1.IntersectWith(ln2).[0]
            xl.BasePoint<-InsPoint
            xl.UnitDir<-((pp1 - InsPoint).GetNormal() + (pp2 - InsPoint).GetNormal())

        let _,_,db=init()
        use trans = db.TransactionManager.StartTransaction()
        let spc  = trans.GetObject(db.CurrentSpaceId, DatabaseServices.OpenMode.ForWrite):?>BlockTableRecord
        spc.AppendEntity(xl)|>ignore
        trans.AddNewlyCreatedDBObject(xl, true)
        trans.Commit()
        
    [<Runtime.CommandMethod("垂直平分线")>]
    member this.垂直平分线()=
        let _,ed,db=init()
        
        let ops = new EditorInput.PromptEntityOptions("\n请选择一条直线")
        ops.SetRejectMessage( "\n请选择直线/圆/圆弧")
        ops.AddAllowedClass(typeof<Line>, true)
        ops.AddAllowedClass(typeof<Arc>, true)
        ops.AddAllowedClass(typeof<DatabaseServices.Circle>, true)

        let entRes  = ed.GetEntity(ops)
        if entRes.Status <> EditorInput.PromptStatus.Error then
            use trans = db.TransactionManager.StartTransaction()
            let spc  = trans.GetObject(db.CurrentSpaceId, DatabaseServices.OpenMode.ForWrite):?>BlockTableRecord

            let AppendEntity ent =
                spc.AppendEntity(ent)|>ignore
                trans.AddNewlyCreatedDBObject(ent, true)

            let ent = trans.GetObject(entRes.ObjectId, DatabaseServices.OpenMode.ForRead):?>DatabaseServices.Entity

            match ent with
            | :? Line as ln->                
                let xl = new Xline()
                xl.UnitDir <- ln.Delta.GetPerpendicularVector()
                xl.BasePoint <- ln.StartPoint + ln.Delta / 2.0
                AppendEntity xl
                trans.Commit()
            | :? Arc as arc->                
                let xl = new Xline()
                xl.UnitDir <- arc.Center - (arc.StartPoint + arc.EndPoint.GetAsVector()) / 2.0
                xl.BasePoint <- arc.Center
                AppendEntity xl
                trans.Commit()
            | :? DatabaseServices.Circle as cir->                
                let vx = Vector3d.XAxis.TransformBy(ed.CurrentUserCoordinateSystem) * cir.Radius * 1.2
                let vy = Vector3d.YAxis.TransformBy(ed.CurrentUserCoordinateSystem) * cir.Radius * 1.2
                
                AppendEntity (new Line(cir.Center + vx, cir.Center - vx))
                AppendEntity (new Line(cir.Center + vy, cir.Center - vy))
                trans.Commit()
            | _ ->()
    
    [<Runtime.CommandMethod("创建属性表代码")>]
    member this.创建属性表代码() =
        let _,ed,db=init()
        
        let ops = new PromptEntityOptions("\n请选则一个包含属性的块")
        ops.SetRejectMessage("\n请选则一个包含属性的块")
        ops.AddAllowedClass(typeof<BlockReference>, true)

        let entRes = ed.GetEntity(ops)

        if entRes.Status <> EditorInput.PromptStatus.Error then
            use trans = db.TransactionManager.StartTransaction()
            let br = trans.GetObject(entRes.ObjectId, DatabaseServices.OpenMode.ForRead)
                     :?> BlockReference
                            
            if br.IsDynamicBlock then
                let btr = trans.GetObject(br.DynamicBlockTableRecord, DatabaseServices.OpenMode.ForRead):?>BlockTableRecord
                ed.WriteMessage("\nCREATE TABLE [Cad].{0}(", btr.Name)
            else
                ed.WriteMessage("\nCREATE TABLE [Cad].{0}(", br.Name)
                
            for id in br.AttributeCollection do
                let Att = trans.GetObject(id, DatabaseServices.OpenMode.ForRead, true):?>AttributeReference
                ed.WriteMessage("\n\t{0} nvarchar(50) NULL,", Att.Tag)

            ed.WriteMessage("\b\n)")

    [<Runtime.CommandMethod("LineTypeScaleCmd")>]
    member this.LineTypeScaleCmd()=
        ModifyAllEntities(fun ent-> ent.LinetypeScale <- 1.0)
        let _,ed,_=init()
        ed.Regen()

    [<Runtime.CommandMethod("ColorByLayer")>]
    member this.ColorByLayer()=
        ModifyAllEntities(fun ent-> ent.Color <- Color.FromColorIndex(ColorMethod.ByLayer, int16 256))
        let _,ed,_=init()
        ed.Regen()

    [<Runtime.CommandMethod("ColorByBlock")>]
    member this.ColorByBlock()=
        ModifyAllEntities(fun ent-> ent.Color <- Color.FromColorIndex(ColorMethod.ByBlock, int16 0))
        let _,ed,_=init()
        ed.Regen()

    [<Runtime.CommandMethod("LinetypeByLayer")>]
    member this.LinetypeByLayer() =
        ModifyAllEntities(fun ent-> ent.Linetype <- "ByLayer")
        let _,ed,_=init()
        ed.Regen()

    [<Runtime.CommandMethod("LineweightByLayer")>]
    member this.LineweightByLayer()=
        ModifyAllEntities(fun ent-> ent.LineWeight<- LineWeight.ByLayer)
        let _,ed,_=init()
        ed.Regen()

    [<Runtime.CommandMethod("ListObject")>]
    member this.ListObject()=
        let _,ed,db=init()
        
        let Opts = new PromptSelectionOptions()
        Opts.MessageForAdding <- "请选择实体"
        Opts.SingleOnly <- true

        let res = ed.GetSelection(Opts)

        if res.Status = PromptStatus.OK then
            use trans = db.TransactionManager.StartTransaction()
            let dbobj = trans.GetObject(res.Value.GetObjectIds().[0], OpenMode.ForRead)

            let t = dbobj.GetType()
            ed.WriteMessage("type name: {0}\n", t.Name)
            for prop in t.GetProperties() do
                let name = prop.Name
                let retp = prop.PropertyType
                let value= try prop.GetValue(dbobj, null) with _ -> null

                match value with
                | null ->ed.WriteMessage("  {0} : {1} is null\n", name, retp.Name)
                | _ ->ed.WriteMessage("  {0} : {1} = {2}\n", name, retp.Name, value.ToString())
            trans.Commit()

