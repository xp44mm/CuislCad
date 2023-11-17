namespace Cads

open System
open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput
open Autodesk.AutoCAD.Geometry
//open Autodesk.AutoCAD.Colors
open Input

type Elevation(pos:float, value:float, dir:Vector3d)=
    let Delta = value - pos
    let Dir = dir
    
    member e.GetValue pos = Delta + pos
    member e.GetPos value= value - Delta

    static member SelectBaseElevation()=
        let _,Ed,Db=init()


        let result = 
            let psopts = new PromptSelectionOptions()
            psopts.SingleOnly <- true
            psopts.MessageForAdding <- "请选择一个基准标高"
            Ed.GetSelection(psopts, new SelectionFilter [|new TypedValue(int DxfCode.Start, "INSERT")|])

        if result.Status = PromptStatus.OK then
            use trans = Db.TransactionManager.StartTransaction()
            let br = trans.GetObject(result.Value.GetObjectIds().[0], OpenMode.ForRead, true):?>BlockReference

            if br.Name = "Elevation" then
                let dir,pos=
                    if br.Normal.IsParallelTo(Vector3d.ZAxis) then
                        (Vector3d.YAxis, br.Position.Y)
                    elif br.Normal.IsPerpendicularTo(Vector3d.ZAxis) then
                        (Vector3d.ZAxis,br.Position.Z)
                    else
                        failwith "方向有误"

                let Att = trans.GetObject(br.AttributeCollection.[0], OpenMode.ForRead, true):?>AttributeReference
                let value = 
                    if Att.Tag = "EL" then
                        Double.Parse(Att.TextString) * 1000.0
                    else
                        failwith "Att.Tag<>EL"

                new Elevation(pos, value, dir)
            else
                failwith "请选择Elevation"
        else
            failwith "意外中断"
type ElevationDrawer() =
    [<Runtime.CommandMethod("修改标高块")>]
    member this.修改标高块()=
        let _,Ed,Db=init()

        let baseElev = Elevation.SelectBaseElevation()

        let Opts = new PromptSelectionOptions()
        Opts.MessageForAdding <- "请选择要修改的标高"
        let values = [|new TypedValue(int DxfCode.Start, "INSERT")|]
        let res = Ed.GetSelection(Opts, new SelectionFilter(values))

        if res.Status = PromptStatus.OK then
            let refIds = res.Value.GetObjectIds()

            use trans = Db.TransactionManager.StartTransaction()
            for brId in refIds do
                let br = trans.GetObject(brId, DatabaseServices.OpenMode.ForWrite):?>BlockReference
                if br.Name = "Elevation" then
                    let brPos =
                        if br.Normal.IsParallelTo(Vector3d.ZAxis) then
                            br.Position.Y
                        elif br.Normal.IsPerpendicularTo(Vector3d.ZAxis) then
                            br.Position.Z
                        else
                            failwith ""

                    for attId in br.AttributeCollection do
                        let Att = trans.GetObject(attId, DatabaseServices.OpenMode.ForWrite, true):?>AttributeReference
                        if Att.Tag = "EL" then
                            Att.TextString <- (baseElev.GetValue(brPos) / 1000.0).ToString("0.000")
            trans.Commit()

