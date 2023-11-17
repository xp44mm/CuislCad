module Convert

open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.Geometry

//常数
let XOY = new Plane(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis)
let p0 = Point2d.Origin
let vecx = Vector2d.XAxis
let vecy = Vector2d.YAxis

//点
let pto2d (p:Point3d) = p.Convert2d(XOY)
let pto3d (p:Point2d) = new Point3d(XOY,p)

//向量
let vto2d (v:Vector3d) = v.Convert2d(XOY)
let vto3d (v:Vector2d) =new Vector3d(XOY, v)

let vector(x,y) = new Vector2d(x,y)

//实体变换
let move(disp: Vector2d)(ent : #Entity) =
    let mat = Matrix3d.Displacement(vto3d disp)
    ent.GetTransformedCopy(mat)

let mirror(axis : Line2d)(ent : #Entity) =
    let center = axis.PointOnLine
    let mat = Matrix3d.Mirroring(new Plane(pto3d center, vto3d axis.Direction, Vector3d.ZAxis))
    ent.GetTransformedCopy(mat)

let rotateBy(angle : float)(center : Point2d)(ent : #Entity) =
    let mat = Matrix3d.Rotation(angle, Vector3d.ZAxis, pto3d center)
    ent.GetTransformedCopy(mat)
    
let scaleBy(scaleAll : float) (center : Point2d) (ent:#Entity) =
    let mat = Matrix3d.Scaling(scaleAll, pto3d center)
    ent.GetTransformedCopy(mat)



