module Attributes
open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry

open Input

let getAttributes (blockReferenceId: ObjectId) =
    let db = getDb()
    use trans=db.TransactionManager.StartTransaction()
    let br = trans.GetObject(blockReferenceId, OpenMode.ForRead):?> BlockReference
    [
        for attId in br.AttributeCollection do
            let att = trans.GetObject(attId, OpenMode.ForRead):?>AttributeReference
            yield att.Tag, att.TextString 
    ]
    |> Map.ofList

let setAttributes (blockReferenceId: ObjectId) (values:Map<string,string>)=
    let db = getDb()
    use trans = db.TransactionManager.StartTransaction()
    let br = trans.GetObject(blockReferenceId, OpenMode.ForWrite)
             :?> BlockReference

    for id: ObjectId in br.AttributeCollection do
        let att  = trans.GetObject(id, OpenMode.ForWrite):?> AttributeReference
        att.TextString <- values.[att.Tag]
    trans.Commit()

let addAttributeBlockReference (blockname:string) (values:Map<string,string>) (sid:ObjectId)(pos:Point3d) =
    let db = getDb()
    use trans=db.TransactionManager.StartTransaction()

    let btr = 
        let bt  = 
            trans.GetObject(db.BlockTableId, OpenMode.ForRead)
            :?>BlockTable

        if bt.Has(blockname) then
            trans.GetObject(bt.[blockname], OpenMode.ForRead):?>BlockTableRecord
        else
            failwith ("未定义的块：" + blockname)

    let br = new BlockReference(pos, btr.ObjectId)
    let space = trans.GetObject(sid, OpenMode.ForWrite):?>BlockTableRecord
    space.AppendEntity(br)|> ignore
    trans.AddNewlyCreatedDBObject(br, true)

    for id in btr do
        match trans.GetObject(id, OpenMode.ForRead) with
        | :? AttributeDefinition as attDef ->
            // 根据属性定义生成属性引用
            let ar = new AttributeReference()
            ar.SetAttributeFromBlock(attDef,br.BlockTransform)
            ar.TextString <- values.[ar.Tag.ToUpper()]

            //将属性引用添加到块引用中
            br.AttributeCollection.AppendAttribute(ar)|> ignore

        | _ ->()
    trans.Commit()


    


