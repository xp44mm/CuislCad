namespace Cads

open System
open System.Collections.Generic
open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput
open Autodesk.AutoCAD.Geometry
//open Autodesk.AutoCAD.Colors

type Coordination(pos : Point2d, aValue : float, bValue : float, aDir : Vector2d, bDir : Vector2d) =
    let Matrix =
        Matrix2d.AlignCoordinateSystem
            (new Point2d(aValue, bValue), aDir, bDir, pos, Vector2d.XAxis,
             Vector2d.YAxis)

    member this.GetPosition(aValue : float, bValue : float) =
        let AB = new Point2d(aValue, bValue)
        AB.TransformBy(Matrix)

    member this.GetAttributeCollection(pos : Point2d) =
        let GetDirAtt(dir : Vector2d) =
            if dir.IsCodirectionalTo(Vector2d.YAxis) then "上"
            elif dir.IsCodirectionalTo(Vector2d.YAxis.Negate()) then "下"
            elif dir.IsCodirectionalTo(Vector2d.XAxis.Negate()) then "左"
            elif dir.IsCodirectionalTo(Vector2d.XAxis) then "右"
            else failwith ""

        let Atts = new Dictionary<String, AttributeReference>()
        let AB = pos.TransformBy(Matrix.Inverse())

        let aValueAtt = new AttributeReference()
        aValueAtt.Tag <- "AValue"
        aValueAtt.TextString <- String.Format("A={0:0.000}", AB.X / 1000.0)
        Atts.Add(aValueAtt.Tag, aValueAtt)

        let bValueAtt = new AttributeReference()
        bValueAtt.Tag <- "BValue"
        bValueAtt.TextString <- String.Format("B={0:0.000}", AB.Y / 1000.0)
        Atts.Add(bValueAtt.Tag, bValueAtt)

        let aDirAtt = new AttributeReference()
        aDirAtt.Tag <- "ADIRECTION"
        aDirAtt.TextString <- GetDirAtt(Matrix.CoordinateSystem2d.Xaxis)
        Atts.Add(aDirAtt.Tag, aDirAtt)

        let bDirAtt = new AttributeReference()
        bDirAtt.Tag <- "BDIRECTION"
        bDirAtt.TextString <- GetDirAtt(Matrix.CoordinateSystem2d.Yaxis)
        Atts.Add(bDirAtt.Tag, bDirAtt)

        Atts

    static member GetDir(dirString : String) =
        match dirString with
        | "上" -> Vector2d.YAxis
        | "下" -> Vector2d.YAxis.Negate()
        | "左" -> Vector2d.XAxis.Negate()
        | "右" -> Vector2d.XAxis
        | _ -> failwith ""

    static member SelectBaseCoordination() =
        let _, ed, db = Input.init()
        use trans = db.TransactionManager.StartTransaction()

        let baseRes =
            let opts = new PromptSelectionOptions()
            opts.SingleOnly <- true
            opts.MessageForAdding <- "请选择一个基准坐标"
            let filter =
                new SelectionFilter [| new TypedValue(int DxfCode.Start,
                                                      "INSERT") |]
            ed.GetSelection(opts, filter)

        let mutable aValue = 0.
        let mutable bValue = 0.
        let mutable aDir = new Vector2d()
        let mutable bDir = new Vector2d()
        if baseRes.Status = PromptStatus.OK then
            let br =
                trans.GetObject
                    (baseRes.Value.GetObjectIds().[0],
                     DatabaseServices.OpenMode.ForRead) :?> BlockReference
            let btr =
                trans.GetObject
                    (br.DynamicBlockTableRecord,
                     DatabaseServices.OpenMode.ForRead) :?> BlockTableRecord
            if btr.Name = "Coordination" then
                for attId in br.AttributeCollection do
                    let att =
                        trans.GetObject
                            (attId, DatabaseServices.OpenMode.ForRead, true) :?> AttributeReference
                    match att.Tag.ToUpper() with
                    | "AVALUE" ->
                        aValue <- Double.Parse(att.TextString.Substring(2))
                                  * 1000.
                    | "BVALUE" ->
                        bValue <- Double.Parse(att.TextString.Substring(2))
                                  * 1000.
                    | "ADIRECTION" ->
                        aDir <- Coordination.GetDir(att.TextString)
                    | "BDIRECTION" ->
                        bDir <- Coordination.GetDir(att.TextString)
                    | _ -> ()
                let bs =
                    new Coordination(br.Position.Convert2d
                                         (new Plane(Point3d.Origin,
                                                    Vector3d.XAxis,
                                                    Vector3d.YAxis)), aValue,
                                     bValue, aDir, bDir)
                bs
            else failwith ""
        else failwith ""

//open System.Windows
//open System.Windows.Controls
open Input
open EntityOps

type CoordinationDrawer() =

    [<Runtime.CommandMethod("修改坐标块")>]
    member this.修改坐标块() =
        let _, ed, db = init()
        let xoy = new Plane(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis)
        let bs = Coordination.SelectBaseCoordination()

        let res =
            let Opts = new PromptSelectionOptions()
            Opts.MessageForAdding <- "请选择要修改的坐标"
            let filter =
                new SelectionFilter [| new TypedValue(int DxfCode.Start,
                                                      "INSERT") |]
            ed.GetSelection(Opts, filter)
        if res.Status <> PromptStatus.OK then () else

        use trans = db.TransactionManager.StartTransaction()
        for id in res.Value.GetObjectIds() do
            let br =
                trans.GetObject(id, OpenMode.ForRead) 
                :?> BlockReference
            let btr =
                trans.GetObject
                    (br.DynamicBlockTableRecord, OpenMode.ForRead) 
                    :?> BlockTableRecord
            if btr.Name = "Coordination" then
                let buffer =
                    bs.GetAttributeCollection(br.Position.Convert2d(xoy))
                for attId in br.AttributeCollection do
                    let att =
                        trans.GetObject(attId, OpenMode.ForWrite, true) :?> AttributeReference
                    att.TextString <- buffer.[att.Tag].TextString
        trans.Commit()

    [<Runtime.CommandMethod("标记坐标点")>]
    member this.标记坐标点() =
        let bs = Coordination.SelectBaseCoordination()
        let abf = new AutoCADWpf.ABWindow()
        if abf.ShowDialog().Value then
            let pos = bs.GetPosition(abf.A * 1000., abf.B * 1000.)
            let xoy = new Plane(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis)
            let ents = new ResizeArray<Entity>()
            let pos = new Point3d(xoy, pos)
            ents.Add
                (new Xline(BasePoint = pos, UnitDir = Vector3d.XAxis))
            ents.Add
                (new Xline(BasePoint = pos, UnitDir = Vector3d.YAxis))
            let values =
                Map.ofList [
                    "AVALUE"     , string abf.A
                    "BVALUE"     , string abf.B
                    "ADIRECTION" , ""
                    "BDIRECTION" , ""                               
                ]
            let blockname = "Coordination"

            let _, _, db = init()
            Attributes.addAttributeBlockReference blockname values db.CurrentSpaceId pos
            ents |> drawEntities db.CurrentSpaceId

