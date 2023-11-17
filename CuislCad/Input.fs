module Input

open Autodesk.AutoCAD.ApplicationServices
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput

let getDocument() = Application.DocumentManager.MdiActiveDocument
let getDb() = HostApplicationServices.WorkingDatabase

let init() =
    let doc = getDocument()
    let ed  = doc.Editor
    let db  = doc.Database
    (doc,ed,db)

let getOnePoint (message:string) act =
    let doc = getDocument()
    let ed  = doc.Editor
    let ps = ed.GetPoint(message)
    if ps.Status = PromptStatus.OK then
        let p = ps.Value
        act p
