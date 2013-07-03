namespace MazesJam.Svc

open ServiceStack.Common.Web
open ServiceStack.Service
open ServiceStack.ServiceHost
open ServiceStack.ServiceInterface
open System
open System.Net
open System.IO
open System.Drawing
open System.Drawing.Imaging

(* maze rendering *)
type [<CLIMutable>] Render = { Maze : int[][]; CellSize : int; }

type RenderService() =
  inherit Service()

  let getRawImage { Maze = maze; CellSize = cellSize; } =
    use image   = maze |> Image.render cellSize
    use buffer  = new MemoryStream()
    image.Save(buffer,ImageFormat.Png)
    buffer.ToArray();

  let encode = Convert.ToBase64String

  let (|Text|Image|) (mime:string) =
    if mime.StartsWith("text")  then Text ("text/plain")
                                else Image("image/png" ) 

  let normalize acceptTypes =
    if (acceptTypes                 = null) 
    || (acceptTypes |> Array.length = 0   ) then [| "*/*" |]
                                            else acceptTypes

  let negotiate acceptTypes =
    acceptTypes
    |> Array.map (fun mime -> match mime with
                              | "image/*"     -> (mime,3)
                              | "image/png"   -> (mime,5)
                              | "text/*"      -> (mime,2)
                              | "text/plain"  -> (mime,4)
                              | _             -> (mime,1))
    |> Array.maxBy (snd)
    |> (fun (mime,_) -> Some mime)

  let handle renderInfo mime = //function
    match mime with
    | Some (Text  mime) ->  let image = (getRawImage >> encode) renderInfo
                            HttpResult(image,mime)
    | Some (Image mime) ->  let image = getRawImage renderInfo
                            HttpResult(image,mime)
    | _                 ->  let code,msg = HttpStatusCode.NotAcceptable
                                         ,"Not Acceptable"
                            HttpResult(code,msg)
  
  member self.POST(renderInfo) =
    self.Request.AcceptTypes
    |> normalize
    |> negotiate 
    |> handle renderInfo

(* static file serving (for GUI) *)
type [<CLIMutable>] GetFile = { Name : string }

type FileService() =
  inherit Service()

  let baseDir = Environment.CurrentDirectory
  
  member __.GET({Name=name}) =
    let file = FileInfo(Path.Combine(baseDir,name))
    HttpResult(file,false) 
  