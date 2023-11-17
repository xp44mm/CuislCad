namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime
//open Cuisl
open System
open Lake.Screw

module 六角螺母Module =
    let section (s) =
        let a = s / sqrt 3.
        
        let p1 = new Point2d(a, 0.)
        let p2 = p1.RotateBy(Math.PI /3.,Point2d.Origin)
        let p3 = p2.RotateBy(Math.PI /3.,Point2d.Origin)
        let p4 = p3.RotateBy(Math.PI /3.,Point2d.Origin)
        let p5 = p4.RotateBy(Math.PI /3.,Point2d.Origin)
        let p6 = p5.RotateBy(Math.PI /3.,Point2d.Origin)

        Creator.polygon
            [|
                p1, 0.
                p2, 0.
                p3, 0.
                p4, 0.
                p5, 0.
                p6, 0.           
            |]

    let side (s,k) =
        let p1 = new Point2d(0., -0.5 * s)
        let p2 = p1 + Vector2d(k,0.)
        let p3 = p2 + Vector2d(0.,s)
        let p4 = p3 - Vector2d(k,0.)

        [
            yield! Creator.polygon 
                [|
                    p1,0.
                    p2,0.
                    p3,0.
                    p4,0.
                |]
            yield new Line(Point3d(k,0.,0.),Point3d.Origin) :> Entity
        ]

    let getKey (data:六角螺母) = sprintf "%f" data.M

    let nutSection (data:六角螺母) = section data.S

    let nutSide (data:六角螺母) = side(data.S,data.K)

module 六角扁螺母Module =
    let getKey (data:六角扁螺母) = sprintf "%f" data.M

    let flatnutSection(data:六角扁螺母) = 六角螺母Module.section data.S
    let flatnutSide (data:六角扁螺母) = 六角螺母Module.side(data.S,data.K)



type 六角螺母Drawer() =

    [<CommandMethod("六角螺母")>]
    member this.nutDraw() =
        let objects = Lake.Screw.六角螺母.DataRecords
        let views = ["截面", 六角螺母Module.nutSection;"侧面",六角螺母Module.nutSide]
        Part.draw objects views 六角螺母Module.getKey

    [<CommandMethod("六角扁螺母")>]
    member this.flatnutDraw() =
        let objects = Lake.Screw.六角扁螺母.DataRecords
        let views = ["截面", 六角扁螺母Module.flatnutSection;"侧面",六角扁螺母Module.flatnutSide]
        Part.draw objects views 六角扁螺母Module.getKey 
