module Creator
open System

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry

open Convert

let point(p) = new DBPoint(pto3d p)
let line(p1,p2) = new Line(pto3d p1,pto3d p2)
let circle(c,r) = new Circle(pto3d c,Vector3d.ZAxis,r)
let arc(c,r,start,final) = new Arc(pto3d c,r,start,final)

let ellipseArc center majorAxis radiusRatio start final =
    new Ellipse(pto3d center, Vector3d.ZAxis, vto3d majorAxis, radiusRatio, start, final)

let ellipse center majorAxis radiusRatio =
    ellipseArc center majorAxis radiusRatio 0. (2. * Math.PI)

let arcFrom (insect: Point2d) (tangent1: Vector2d) (tangent2: Vector2d) (radius: float) =
    let totalAngle = Math.PI - tangent1.GetAngleTo(tangent2)
    let center = insect + (tangent1 * tangent2.Length + tangent2 * tangent1.Length).GetNormal() * radius / cos(0.5*totalAngle)
    let center = pto3d center
    let startAngle,endAngle =
        let t1 = vto3d tangent1
        let t2 = vto3d tangent2

        if t1.CrossProduct(t2).IsCodirectionalTo(Vector3d.ZAxis) then
            let sa = tangent2.GetPerpendicularVector().Angle
            let ea = tangent1.GetPerpendicularVector().Negate().Angle
            sa,ea
        else
            let sa = tangent1.GetPerpendicularVector().Angle
            let ea = tangent2.GetPerpendicularVector().Negate().Angle
            sa,ea

    new Arc(center,radius,startAngle,endAngle)

let private treePoint p1 p2 p3 r =
    if r>0.0 then
        let t1 = p1-p2
        let t2 = p3-p2

        let arc = arcFrom p2 t1 t2 r
        let sp,ep =
            let arcStartPoint = arc.StartPoint |> pto2d
            let arcEndPoint = arc.EndPoint |> pto2d
                
            if t1.IsCodirectionalTo(arcStartPoint-p2) then
                arcStartPoint,arcEndPoint
            else
                arcEndPoint,arcStartPoint
        Some(arc),new Point3d(XOY,sp),new Point3d(XOY,ep)
    else
        let p2 = new Point3d(XOY,p2)
        None,p2,p2

let private round (inputs:#seq<Point2d*float>) =
    inputs
    |> Seq.pairwise |> Seq.pairwise |> Seq.map(fun((a,b),(_,c))-> a,b,c)
    |> Seq.map(fun((p1,_),(p2,r),(p3,_))-> treePoint p1 p2 p3 r)
    |> Seq.pairwise
    |> Seq.collect(function((a1,_,p1),(_,p2,_))->
                            [
                                if a1.IsSome then yield a1.Value :> Entity
                                yield new Line(p1,p2) :> Entity
                            ])
    |> Seq.toList


//起点和终点半径值为零
//表示一系列连续的直线，连接顶点
let sequential (inputs:#seq<Point2d*float>) =
    [
        yield Point2d.Origin,0.
        yield! inputs
        yield Point2d.Origin,0.
    ]
    |> round

let polygon (inputs:#seq<Point2d*float>) =
    [
        yield! inputs
        yield inputs |> Seq.item 0
        yield inputs |> Seq.item 1
        yield inputs |> Seq.item 2

    ]
    |> round

let rectangle (p0:Point2d) (vec:Vector2d) =
    let p1 = p0 + new Vector2d(vec.X,0.0)
    let p2 = p0 + vec
    let p3 = p0 + new Vector2d(0.0,vec.Y)
    [|
        line(p0,p1) :> Entity //输入点对应的横边
        line(p0,p3) :> Entity //输入点对应的竖边
        line(p2,p1) :> Entity //对角点对应的横边
        line(p2,p3) :> Entity //对角点对应的竖边
    |]
       
