namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System

open Convert
open Creator

open Lake.Pedestals

type 耳式支座Drawer() =
    // 实例的唯一标识
    let getKey (data:耳式支座) = sprintf "%s" data.Spec
    
    //肋板切角长度剩余直段
    let lr (data:耳式支座) =
        match data.Spec.[0] with
        | 'A' ->30.
        | 'B' ->100.
        | _ -> failwith ""

    //肋板切角高度剩余直段
    let hr = 30. 
    
    let top (data:耳式支座) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(-0.5*data.L1,0.)
        let p2 = new Point2d(-0.5*data.B2,0.)

        [
            //底板
            yield! rectangle p1 (new Vector2d(data.L1,data.B1))
            //筋板
            let jb = rectangle p2 (new Vector2d(data.T2,data.L2))

            yield! jb
            yield! jb |> Array.map(mirror vAxis)        
            //地脚螺栓孔
            yield circle (new Point2d(0.,data.S1),0.5*data.D):>Entity        
        ]

    let front (data:耳式支座) =
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)
        //底板
        let db =
            rectangle (new Point2d(-0.5*data.L1, 0.)) (new Vector2d(data.L1,data.T1))
        //筋板
        let jb =
            rectangle (new Point2d(-0.5*data.B2, data.T1)) (new Vector2d(data.T2,data.H - data.T1))
        [
            yield! db
            yield! jb
            yield! jb |> Array.map(mirror vAxis)
        ]
    let side (data:耳式支座) =            
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(0., data.T1 + hr)
        let p2 = new Point2d(0., data.T1)
        let p3 = new Point2d(-data.L2, data.T1)
        let p4 = new Point2d(-data.L2, data.H)
        let p5 = new Point2d(-data.L2 + lr data, data.H)

        //底板
        let db =
            rectangle Point2d.Origin (new Vector2d(-data.B1,data.T1))
        //筋板
        let jb =
            Creator.polygon
                [|
                    p1, 0.
                    p2, 0.
                    p3, 0.
                    p4, 0.
                    p5, 0.
                |]

        //地脚螺栓孔定位
        let dj = point (new Point2d(-data.S1, 0.5*data.T1)):>Entity
        [
            yield!db
            yield!jb
            yield dj
        ]
    [<CommandMethod("耳式支座")>]
    member this.Drawer() =
        let objects = Lake.Pedestals.耳式支座.DataRecords
        let views = ["正面", front;"侧面",side;"顶面",top]
        Part.draw objects views getKey
