namespace cuisl.cad

open System
open System.Text 
open System.IO 

open Autodesk.AutoCAD
open Autodesk.AutoCAD.DatabaseServices
open Autodesk.AutoCAD.EditorInput
open Autodesk.AutoCAD.Geometry
open Autodesk.AutoCAD.Colors
open Table

open Convert
open Creator

type Plateform()=
    let pnt x y = new Point2d(x,y)
    let vector x y = new Vector2d(x,y)
    let line x y = line(x,y)
    let p0 = Point2d.Origin
    let guard1050 =(1050.0,740.0,430.0)
    let guard1200 =(1200.0,840.0,480.0)

    //平台立面图
    let plateform guard (len:float) =
        //栏杆的高度//横杆1高度//横杆2高度
        let height,y1,y2= guard

        let frameHeight = 100.0 //平台框架高度
        let maxspace = 1000.0 //立柱最大间距

        //平台槽钢框架轮廓
        let frame = rectangle p0 (new Vector2d(len, -frameHeight)) |> Array.toList

        let xline y = Creator.line(new Point2d(0.0, y), new Point2d(len, y))
        let yline x = Creator.line(new Point2d(x, 0.0), new Point2d(x, height))

        //绘制水平线
        let xlines = [height;y1;y2] |> List.map(fun y -> xline y :> Entity)
            
        //立柱的x坐标
        let poles =
            let num = ceil (len/maxspace) //立柱空隙个数

            if num = 1.0 then 
                [ 0.0; len]
            else
                let num = (num - 2.0)//中间的立柱空隙为maxspace的数量
                let dist = (len - num * maxspace)/2.0 //边距
                
                [ 
                    yield 0.0
                    for i in 0.0 .. num do
                        yield dist + i * maxspace
                    yield len
                ]

        let ylines = poles |> List.map(fun x -> yline x :> Entity)

        xlines @ ylines @ frame
        
    //45d斜梯侧面视图
    let xieti (height:float) =
        //height 为100高度的奇数倍
        let tiliangHeight = 160.0 //梯梁为ch16,高度为160.0

        let l = tiliangHeight*2.0**0.5-100.0//梯梁水平切口长度
        let tilianproj =height-100.0//梯梁斜边投影长度

        let p1 = new Point2d(              l,            0.0)
        let p2 = new Point2d( (l+tilianproj), (height-100.0))
        let p3 = new Point2d( (l+tilianproj),         height)
        let p4 = new Point2d(   (tilianproj),         height)
        let p5 = new Point2d(            0.0,          100.0)

        let tiliang = polygon 
                        [
                            p0, 0.
                            p1, 0.
                            p2, 0.
                            p3, 0.
                            p4, 0.
                            p5, 0.
                        ]

        //下部踏步
        let xiabutabu =
            //间隔数
            let num = ceil (height / 200.0)

            let vect = new Vector2d(-200.0,-200.0)

            [
                for i in [1.0..num - 1.0] do
                    let p = p4 + vect * i
                    yield Creator.line(p, (p+new Vector2d(200.0, 0.0))) :> Entity
            ]

        //栏杆立柱
        let poles =
            //栏杆立柱的个数
            let maxspace = 1000.0//栏杆最大间距
            let guardHeight = 1030.0
            let hengganHeight = 503.0

            //水平长度比高度小100，下面边距200，上面边距300
            let polex = (height-100.0-200.0-300.0)//第一和最后立柱的总水平距离
            //间隙的数量
            let num = ceil ( polex / maxspace)
            
            let dist = polex / num
            let bp = p4 - vector 300.0 300.0//最高一根立柱的下端点
            let vect = vector dist dist
            let org = line p4 p5:>Entity

            [
                for i in [ 0.0.. num] do
                    let p = bp - i * vect
                    yield line p (p + vector 0.0 guardHeight) :> Entity
                yield move (new Vector2d(0.0,guardHeight)) org
                yield move (new Vector2d(0.0,hengganHeight)) org
            ]

        tiliang @ xiabutabu @ poles

    let xietiData=(63.0,200.0,900.0)
    let zipatiData=(50.0,300.0,500.0)

    //斜梯，直爬梯的正面视图
    let ladder data (height:float) =
        //梯梁宽
        let chwidth,space,width = data

        let yline x = line (pnt x 0.0) (pnt x (-height))
        let xline y = line (pnt chwidth y) (pnt (chwidth+width) y)

        let x1 = chwidth
        let x2 = chwidth+width
        let x3 = chwidth+width+chwidth
        let ys = [-space .. -space .. -height]
        [
            for x in [0.0;x1;x2;x3] -> yline x
            yield line               p0  (pnt x3     0.0)
            yield line (pnt 0.0 -height) (pnt x3 -height)
            for y in ys -> xline y
            ]
    
    //直爬梯护笼侧面图
    let hulong (height:float) =

        let x1 = 350.0 //垂直条1
        let x2 = 597.5 //垂直条2
        let width = 700.0 //最外垂直条距离
        let maxspace = 750.0//护笼水平圈间距
        //以上变量为常数

        let xline y = line (pnt 0.0 y) (pnt width y)
        let yline x = line (pnt x 0.0) (pnt x height)

        let xs = 
            let num = ceil (height / maxspace)
            let space = height / num
            [0.0..num] |> List.map(fun y -> xline(y*space))

        let ys = [0.0;x1;x2;width] |> List.map(yline)
        
        xs @ ys
        
    [<Runtime.CommandMethod("xieti")>]
    member this.drawXieti() =

        let frm = AutoCADWpf.InputWindow()
        frm.Title <- "请输入斜梯的高度(mm)："

        if frm.ShowDialog().Value then
            let height = float frm.InputText //斜梯的高度

            let blockname =sprintf "inclinedladder%d" (int height)
            let blockId = EntityOps.defineBlock blockname (xieti height)

            let doc,ed,db = Input.init()
            let ps = Input.getDocument().Editor.GetPoint("请选择左下点：\n")
            if ps.Status = PromptStatus.OK then 
                let p = ps.Value
                let br = new BlockReference(p, blockId)

                br.TransformBy(ed.CurrentUserCoordinateSystem)
                EntityOps.drawEntity db.CurrentSpaceId br

    [<Runtime.CommandMethod("ladder")>]
    member this.drawLadder() =
        let w = new AutoCADWpf.ladderWindow()
        if w.ShowDialog().Value then
            let height = w.LadderHeight            

            let data = 
                match w.LadderType with
                |"直爬梯" ->zipatiData
                |"斜梯" ->xietiData
                |_ -> failwith ""

            let blockname = sprintf "ladder%s%d" w.LadderType (int height)
            let blockId = EntityOps.defineBlock blockname (ladder data height)

            let doc,ed,db = Input.init()
            let ps = Input.getDocument().Editor.GetPoint("请选择左上点：\n")
            if ps.Status = PromptStatus.OK then 
                let p = ps.Value

                let br = new BlockReference(p, blockId)
                br.TransformBy(ed.CurrentUserCoordinateSystem)
                EntityOps.drawEntity db.CurrentSpaceId br
                
    [<Runtime.CommandMethod("hulong")>]
    member this.drawHulong() =
        let frm = AutoCADWpf.InputWindow()
        frm.Title <- "请输入护笼的高度(mm)："

        if frm.ShowDialog().Value then
            let height = float frm.InputText //护笼的高度

            let blockname = sprintf "hulong%d" (int height)
            let blockId = EntityOps.defineBlock blockname (hulong height)

            let doc,ed,db = Input.init()
            let ps = Input.getDocument().Editor.GetPoint("请选择右下点：\n")
            if ps.Status = PromptStatus.OK then 
                let p = ps.Value

                let br = new BlockReference(p, blockId)

                br.TransformBy(ed.CurrentUserCoordinateSystem)
                EntityOps.drawEntity db.CurrentSpaceId br

    [<Runtime.CommandMethod("plateform")>]
    member this.drawPlateform() =
        let w = new AutoCADWpf.plateformWindow()
        if w.ShowDialog().Value then
            let len = w.PlateformLength     

            let guard = 
                match w.PlateformType with
                |"1050" ->guard1050
                |"1200" ->guard1200
                |_ -> failwith ""

            let blockname = sprintf "plateform%s%d" w.PlateformType (int len)
            let blockId = EntityOps.defineBlock blockname (plateform guard len)

            let doc,ed,db = Input.init()
            let ps = Input.getDocument().Editor.GetPoint("请选择左下点：\n")
            if ps.Status = PromptStatus.OK then 
                let p = ps.Value
                let br = new BlockReference(p, blockId)
                br.TransformBy(ed.CurrentUserCoordinateSystem)
                br |> EntityOps.drawEntity db.CurrentSpaceId