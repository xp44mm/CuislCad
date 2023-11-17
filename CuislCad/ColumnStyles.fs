namespace ColumnStyles

type Align =
    |Left
    |Center
    |Right

    static member Parse(s:string) = 
        match s.ToLower() with
        |"left" -> Left
        |"center" -> Center
        |"right" -> Right
        | _ -> failwith ""
    override this.ToString() =
        match this with
        | Left -> "Left"
        | Center -> "Center"
        | Right -> "Right"

type ColumnStyle(align:Align,width:float)=
    member this.Align = align
    member this.Width = width
    override this.ToString() =
        sprintf "%O %f" align width
    static member Default = new ColumnStyle(Left,50.)

type Style(name:string,columnStyles:ColumnStyle list) =
    member this.Name = name
    member this.ColumnStyles = columnStyles
    override this.ToString() =
        sprintf "%s:\n %A" name columnStyles 


