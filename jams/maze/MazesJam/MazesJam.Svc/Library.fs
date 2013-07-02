namespace MazesJam.Svc

module internal Image =
  
  open System.IO
  open System.Drawing
  open System.Drawing.Imaging
  
  (* directions around cell -- passages or walls *)
  let [<Literal>] N = 1;
  let [<Literal>] S = 2;
  let [<Literal>] E = 4;
  let [<Literal>] W = 8;

  let hasWall direction cell = 
    //NOTE: direction indicates a passage _out_ of the cell.
    //      thus, not having a passage equals having a wall.
    cell &&& direction = 0
  
  let render cellSize maze =
    let mazeHeight  = Array.length maze 
    let mazeWidth   = ((Array.map Array.length) >> Array.max) maze
    let visWidth
       ,visHeight   = cellSize * mazeWidth
                     ,cellSize * mazeHeight
    let mazeImage   = new Bitmap(visWidth,visHeight)
    use borderPen   = new Pen(Brushes.Black,2.0f)
    let borderRect  = Rectangle(1,1,visWidth - 2,visHeight - 2)
    use graphics    = Graphics.FromImage mazeImage
    
    let drawCell (x,y) cell =
      let x1,y1 = cellSize * x,cellSize * y
      let x2,y2 = x1 + cellSize,y1 + cellSize
      
      if cell |> hasWall N then graphics.DrawLine(Pens.Black,x1,y1,x2,y1)
      if cell |> hasWall S then graphics.DrawLine(Pens.Black,x1,y2,x2,y2)
      if cell |> hasWall E then graphics.DrawLine(Pens.Black,x2,y1,x2,y2)
      if cell |> hasWall W then graphics.DrawLine(Pens.Black,x1,y1,x1,y2)

    // draw outer "bounds" of maze (with offset to prevent clipping)
    graphics.DrawRectangle(borderPen,borderRect) 
    
    // loop through rows and cells, drawing the walls of each cell
    maze |> Array.iteri (fun y row  ->
     row |> Array.iteri (fun x cell -> drawCell (x,y) cell))

    mazeImage
