namespace MazesJam.Svc

open ServiceStack.Common.Web
open ServiceStack.Service
open ServiceStack.ServiceHost
open System
open System.Net
open System.IO
open System.Drawing
open System.Drawing.Imaging

type ImageResult(image:Image,?imageFormat) =
  let mutable image = image
  let imageFormat   = defaultArg imageFormat ImageFormat.Png
  interface IHasOptions with
    member __.Options = 
      let contentType = sprintf "image/%s" ((string imageFormat).ToLower())
      dict [(HttpHeaders.ContentType,contentType)]
  interface IStreamWriter with
    member __.WriteTo(responseStream) =
      image.Save(responseStream,imageFormat)
  interface IDisposable with
    member __.Dispose() = 
      (image :> IDisposable).Dispose()
      image <- null

type [<CLIMutable>] Render  = { Maze      : int[][] 
                                CellSize  : int }

type RenderService() =
  member __.GET({Maze=maze;CellSize=cellSize;}) = 
    new ImageResult(maze |> Image.render cellSize)
  interface IService (* marker *)


type [<CLIMutable>] GetFile = { Name : string }

type FileService() =
  let baseDir = Environment.CurrentDirectory
  member __.GET({Name=name}) =
    let file = FileInfo(Path.Combine(baseDir,name))
    HttpResult(file,false) 
  interface IService (* marker *)
