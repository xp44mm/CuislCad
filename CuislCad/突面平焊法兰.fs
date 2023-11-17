namespace FsharpCad

open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open Convert
open Creator
//open Cuisl

open Lake.Valves

module mod突面平焊法兰 =
    let flanges = Lake.Valves.突面平焊法兰.DataRecords
    let holes = Lake.Valves.法兰螺纹孔.DataRecords
    
    let getKey (data:突面平焊法兰) = sprintf "%f_%f" data.PN data.DN

    let front (data: 突面平焊法兰) =
        let phi = 
            holes
            |> Array.find(fun h -> h.M = data.M)
            |> fun h -> h.Phi

        [
            yield circle(p0,0.5*data.Dw):>Entity
            yield circle(p0,0.5*data.DK):>Entity
            yield circle(p0,0.5*data.DS):>Entity
            yield circle(p0,0.5*data.Di2):>Entity

            let h = circle(new Point2d(0.5*data.DK,0.),0.5*phi):> Entity
            let n = float data.N
            for i in 0.0..(n-1.0) do
                let angle = Math.PI/2. + (2. * i - 1.)/ n * Math.PI
                yield h |> rotateBy angle p0         
        ]

    let side (data:突面平焊法兰) =
        let hAxis = new Line2d(p0,vecx)
        [
            //小突台
            let p1 = new Point2d(-data.HS, -data.DS / 2.)
            let p2 = new Point2d(      0., -data.DS / 2.)
            let p3 = p2.Mirror hAxis
            let p4 = p1.Mirror hAxis
            yield! sequential 
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]
        
            //基体
            let p5 = new Point2d(-data.C , -data.Dw / 2.)
            let v5 = vector(data.C-data.HS,data.Dw)
            yield! rectangle p5 v5

            //节圆
            let jy = point(new Point2d(-data.C / 2., data.DK / 2.)):>Entity
            yield jy
            yield jy|> mirror hAxis
            //管子外径
            let gz = point(new Point2d(-data.H, data.Di2 / 2.)):>Entity
            yield gz
            yield gz|> mirror hAxis
        ]


open Autodesk.AutoCAD.EditorInput

type 突面平焊法兰Drawer() =
    [<CommandMethod("突面平焊法兰")>]
    member this.Drawer() =
        let views = ["前面",mod突面平焊法兰.front;"侧面",mod突面平焊法兰.side]

        Part.tryGetBlockDef mod突面平焊法兰.flanges views mod突面平焊法兰.getKey
        |>  function
            |Some(blockId,view) ->
                let db,ed = 
                    let doc = Input.getDocument()
                    doc.Database, doc.Editor

                if view="侧面" then
                    //*在屏幕上获取两点
                    let ps1 = ed.GetPoint("请选择插入点")
                    if ps1.Status <> PromptStatus.OK then () else
                    let p1 = ps1.Value 

                    let ps2 = ed.GetPoint(new PromptPointOptions("选取方向点",UseBasePoint = true, UseDashedLine = true, BasePoint = p1))
                    if ps2.Status <> PromptStatus.OK then () else
                    let p2 = ps2.Value 

                    Part.getBr2 p1 p2 blockId |> EntityOps.drawEntity db.CurrentSpaceId

                else
                    //*在屏幕上获取一点
                    let ps = ed.GetPoint("请选择插入点")
                    if ps.Status = PromptStatus.OK then
                        Part.getBr ps.Value blockId |> EntityOps.drawEntity db.CurrentSpaceId

            |None ->()









