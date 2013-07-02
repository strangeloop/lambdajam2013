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


type [<CLIMutable>] Render = { Maze      : int[][] 
                               CellSize  : int }

type RenderService() =
  inherit Service()

  let getRawImage { Maze = maze; CellSize = cellSize; } =
    use image   = maze |> Image.render cellSize
    use buffer  = new MemoryStream()
    image.Save(buffer,ImageFormat.Png)
    buffer.ToArray();

  let encode = Convert.ToBase64String

  let (|Text|Image|Other|) = function 
    | "text/plain"  as mime -> Text  mime 
    | "image/png"   as mime -> Image mime
    | otherwise             -> Other otherwise

  member self.POST(renderInfo) =
    let image   = getRawImage renderInfo
    let accept  = self.Request.AcceptTypes
                  |> Array.tryPick (function Text  _ as mime -> Some(mime)
                                           | Image _ as mime -> Some(mime)
                                           | _               -> None)
    match accept with
    | Some(Text (mime)) -> HttpResult(encode image,mime)
    | Some(Image(mime)) -> HttpResult(image       ,mime)
    // unsupported media type
    | _ -> HttpResult(HttpStatusCode.UnsupportedMediaType
                     ,string HttpStatusCode.UnsupportedMediaType)

type [<CLIMutable>] GetFile = { Name : string }

type FileService() =
  let baseDir = Environment.CurrentDirectory
  member __.GET({Name=name}) =
    let file = FileInfo(Path.Combine(baseDir,name))
    HttpResult(file,false) 
  interface IService (* marker *)
