namespace FsharpCad

open System
open System.Windows

open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices

open Input

type ImportBlocks() =

    let importBlocks dwgName=
        let _,ed,db = init()

        let sourceDb = new DatabaseServices.Database(buildDefaultDrawing=false, noDocument=true)
        sourceDb.ReadDwgFile(dwgName, System.IO.FileShare.Read, true, "")
        try
            let blockIds = new DatabaseServices.ObjectIdCollection()
            use trans = sourceDb.TransactionManager.StartTransaction()
            let bt = trans.GetObject(sourceDb.BlockTableId, DatabaseServices.OpenMode.ForRead):?>BlockTable

            for btrId in bt do
                let btr = trans.GetObject(btrId, DatabaseServices.OpenMode.ForRead):?>BlockTableRecord

                if not btr.IsAnonymous && not btr.IsLayout then
                    blockIds.Add(btrId)|>ignore
                    ed.WriteMessage("\n" + btr.Name)
            trans.Dispose()

            //用WblockCloneObjects把所有的块从源库拷贝块到目的库的块表中
            let mapping = new DatabaseServices.IdMapping()

            sourceDb.WblockCloneObjects(blockIds, db.BlockTableId, mapping, DatabaseServices.DuplicateRecordCloning.Replace, false)
            ed.WriteMessage("\n从文件： {0} 复制了 {1} 个块定义到目前图形。",dwgName,blockIds.Count.ToString())
        with :? Runtime.Exception as ex ->
            ed.WriteMessage("\n" + ex.Message)

        sourceDb.Dispose()

// todo:修改为WPF对话框
//    [<Runtime.CommandMethod("导入图块定义")>]
//    member this.导入图块定义()=
//        let dlg = new OpenFileDialog()
//        if dlg.ShowDialog() = DialogResult.OK then importBlocks dlg.FileName
        
