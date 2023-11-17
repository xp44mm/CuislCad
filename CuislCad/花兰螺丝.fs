namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System
open Convert

open Lake.Hangers

type 花兰螺丝Drawer() =

    let front (h,a,d,b,f1) =
        let hAxis = new Line2d(Point2d(0.,0.5*h),Vector2d.XAxis)
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1  = new Point2d(a / 2., 0.)
        let p2 = p1.Mirror(vAxis)
        let p3 = p2.Mirror(hAxis)
        let p4 = p3.Mirror(vAxis)
        
        let p5  = new Point2d(d / 2., 0.)
        let p6  = new Point2d(d / 2., b)
        let p7 = p6.Mirror(vAxis)
        let p8 = p5.Mirror(vAxis)
        
        let p9 = new Point2d(a / 2. - f1, b)
        let p10 = p9.Mirror(hAxis)
        
        [
            yield! Creator.polygon
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]

            let ue = Creator.sequential
                        [|
                            p5,0.
                            p6,0.
                            p7,0.
                            p8,0.
                        |]
            yield! ue
            yield! ue |> List.map(mirror hAxis)


            let line = new Line(pto3d p9,pto3d p10) :> Entity
            yield line
            yield line |> mirror vAxis
        ]

    let side (h,f2,b,s) =
        let hAxis = new Line2d(Point2d(0.,0.5*h),Vector2d.XAxis)
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1  = new Point2d(f2 / 2., b)
        let p2  = new Point2d(s / 2., b)
        let p3  = new Point2d(s / 2., 0.)

        let p4 = p3.Mirror(vAxis)
        let p5 = p2.Mirror(vAxis)
        let p6 = p1.Mirror(vAxis)


        let p7 = new Point2d(f2 / 2., 0.)
        let p8 = p7.Mirror(hAxis)

        [
            let ue = Creator.sequential
                        [|
                            p1,0.
                            p2,0.
                            p3,0.
                            p4,0.
                            p5,0.
                            p6,0.
                        |]
            yield! ue
            yield! ue |> List.map(mirror hAxis)
            
            let line = new Line(pto3d p7,pto3d p8) :> Entity
            yield line
            yield line |> mirror vAxis
        ]

    let hlfront (data:花兰螺丝) =
        let h = data.H
        let a = data.A
        let d = data.D
        let b = data.B
        let f1 = data.F1
        front (h,a,d,b,f1)

    let hlside (data:花兰螺丝) =
        let h = data.H
        let f2 = data.F2
        let b = data.B
        let s = data.S
        side (h,f2,b,s)

    let lwfront (data:螺纹接头) =
        let h = data.H
        let a = data.A
        let d = data.D
        let b = data.B
        let f1 = data.F1
        front (h,a,d,b,f1)

    let lwside (data:螺纹接头) =
        let h = data.H
        let f2 = data.F2
        let b = data.B
        let s = data.D
        side (h,f2,b,s)



    [<CommandMethod("花兰螺丝")>]
    member this.hlls() =
        // 实例的唯一标识
        let getKey (data:花兰螺丝) = sprintf "%f" data.D

        let objects = Lake.Hangers.花兰螺丝.DataRecords
        let views = ["正面", hlfront;"侧面",hlside]
        Part.draw objects views getKey

    [<CommandMethod("螺纹接头")>]
    member this.lwjt() =
        // 实例的唯一标识
        let getKey (data:螺纹接头) = sprintf "%f" data.D

        let objects = Lake.Hangers.螺纹接头.DataRecords
        let views = ["正面", lwfront;"侧面",lwside]
        Part.draw objects views getKey


