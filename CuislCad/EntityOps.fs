module EntityOps

open System
open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Colors
open Input

let drawEntity (spaceId: ObjectId) (ent: #Entity) =
    let db = Input.getDb()
    use trans = db.TransactionManager.StartTransaction()
    let space = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite)
                :?>BlockTableRecord
    space.AppendEntity(ent)|> ignore
    trans.AddNewlyCreatedDBObject(ent, true)
    trans.Commit()
    
let drawEntities (spaceId : ObjectId) (entities : #seq<#Entity>) =
    let _,_,db=init() 
    use trans = db.TransactionManager.StartTransaction()
    let space = trans.GetObject(spaceId, OpenMode.ForWrite):?>BlockTableRecord
    entities
    |> Seq.iter (fun ent ->
        space.AppendEntity(ent)|>ignore
        trans.AddNewlyCreatedDBObject(ent, true))

    trans.Commit()


//操作图形中的所有实体,包括块中的实体
let ModifyAllEntities actor =
    let _,_,db=init()

    use trans=db.TransactionManager.StartTransaction()
    let bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead):?>BlockTable

    for btrId in bt do
        let btr=trans.GetObject(btrId, OpenMode.ForRead):?>BlockTableRecord
        for id in btr do
            let ent = trans.GetObject(id, DatabaseServices.OpenMode.ForWrite) :?> Entity
            actor ent

    trans.Commit()

let defineBlock (blockName:string) (entities:#seq<#Entity>) =
    let db = Input.getDb()
    use trans = db.TransactionManager.StartTransaction()
    let bt  = 
        trans.GetObject(db.BlockTableId, OpenMode.ForWrite)
        :?>DatabaseServices.BlockTable

    let blockId =
        if bt.Has(blockName) then
            bt.[blockName]
        else
            let btr = new BlockTableRecord(Name=blockName)
            let blockId = bt.Add(btr)

            trans.AddNewlyCreatedDBObject(btr,true)

            for ent in entities do
                btr.AppendEntity(ent) |> ignore
                trans.AddNewlyCreatedDBObject(ent, true)
            blockId
    trans.Commit()
    blockId

let setEntity (ent:Entity) =
    ent.Layer <- "0"
    ent.Linetype <- "ByLayer"
    ent.LinetypeScale <- 1.0
    ent.LineWeight <- LineWeight.ByLayer
    ent.Color <- Color.FromColorIndex(Colors.ColorMethod.ByLayer, 256s)

