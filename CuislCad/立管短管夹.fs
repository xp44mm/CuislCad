namespace FsharpCad

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Runtime

open System
open Convert
open Creator
//open Cuisl
open Lake.Clasps

type 立管短管夹Drawer() =
    // 实例的唯一标识
    let getKey (data:立管短管夹) = sprintf "%f" data.Dw

    let precalc (data:立管短管夹) =
        let od = data.Dw + 2. * data.T1
        let oe = data.E + 2. * data.T1
        let theta = 2. * asin(oe / od)
        let fh = od / 2. * cos(theta / 2.)
        od,oe,theta,fh

    let front (data:立管短管夹) =
        let od,oe,theta,fh = precalc data

        let hAxis = new Line2d(Point2d.Origin,Vector2d.XAxis)
        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        let p1 = new Point2d(fh, 0.5*oe)
        let p2 = new Point2d(data.L / 2., 0.5*oe)

        let p3 = new Point2d(data.C / 2., 0.7*oe)//中心线伸出边界0.2x
        let p4 = p3.Mirror(hAxis)

        let p5 = new Point2d(data.D / 2., 0.7*oe)//中心线伸出边界0.2x
        let p6 = p5.Mirror(hAxis)

        //管夹
        let ln1 = line(p1,p2):>Entity
        let ar1 = arc(Point2d.Origin, 0.5*od, 0.5*theta + Math.PI, -0.5*theta) :> Entity
        let ln2 = ln1 |> mirror vAxis

        let ln3 = ln1 |> mirror hAxis
        let ar2 = ar1 |> mirror hAxis
        let ln4 = ln2 |> mirror hAxis

        //四个紧固件中心线
        let ln5 = line(p3,p4):>Entity
        let ln6 = line(p5,p6):>Entity
        let ln7 = ln6 |> mirror vAxis
        let ln8 = ln5 |> mirror vAxis

        //挡块中心线
        let ln = line(new Point2d(0.,0.7*od),new Point2d(0.,-0.7*od)):>Entity
        //汇总
        [
            yield! [ln1; ln2; ln3; ln4; ln5; ln6; ln7; ln8; ar1; ar2 ]
            match data.N with
            | 4 ->
                yield ln |> rotateBy ( Math.PI/4.) Point2d.Origin 
                yield ln |> rotateBy (-Math.PI/4.) Point2d.Origin 

            | 6 ->
                yield ln
                yield ln |> rotateBy ( Math.PI/3.) Point2d.Origin 
                yield ln |> rotateBy (-Math.PI/3.) Point2d.Origin 
            | _ ->()

        ]
        
            
    let side (data:立管短管夹) =
        let od,oe,theta,fh = precalc data

        let vAxis = new Line2d(Point2d.Origin,Vector2d.YAxis)

        //管夹轮廓
        let p1 = new Point2d(-0.5*data.L,0.)
        let v1 = new Vector2d(data.L,data.B)
        let rect1 = rectangle p1 v1

        //螺栓孔中心
        let c1 = circle(new Point2d(0.5*data.C, 0.5*data.B), 0.5*data.M1 + 1.):>Entity
        let c2 = circle(new Point2d(0.5*data.D, 0.5*data.B), 0.5*data.M2 + 1.):>Entity
        let c3 = c2 |> mirror vAxis
        let c4 = c1 |> mirror vAxis

        //管部挡块
        let k1 = rectangle (new Point2d(-0.5*data.T2, data.B)) (new Vector2d(data.T2,data.H)) 

        //汇总
        [
            yield! rect1
            yield! [c1;c2;c3;c4]
            match data.N with
            | 4 ->
                yield! k1 |> Array.map(move ( 0.5*data.Dw * sin(Math.PI/4.)*Vector2d.XAxis))
                yield! k1 |> Array.map(move (-0.5*data.Dw * sin(Math.PI/4.)*Vector2d.XAxis))
            | 6 ->           
                yield! k1    
                yield! k1 |> Array.map(move ( 0.5*data.Dw * sin(Math.PI/3.)*Vector2d.XAxis))
                yield! k1 |> Array.map(move (-0.5*data.Dw * sin(Math.PI/3.)*Vector2d.XAxis))
            | _ ->()
        ]

    [<CommandMethod("立管短管夹")>]
    member this.Drawer() =
        let objects = Lake.Clasps.立管短管夹.DataRecords

        let views = ["前面",front;"侧面",side]
        Part.draw objects views getKey

