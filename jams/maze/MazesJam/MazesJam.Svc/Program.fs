namespace MazesJam.Svc

open ServiceStack.Common.Web
open ServiceStack.ServiceHost
open ServiceStack.ServiceInterface
open ServiceStack.WebHost.Endpoints
open System

type AppHost() =
  inherit AppHostHttpListenerBase("MazesJam",typeof<Render>.Assembly)
  override self.Configure(container) =
    self.Routes
        .Add<Render>("/mazes/render",ApplyTo.Post) 
        .Add<GetFile>("/{name}",ApplyTo.Get) 
        |> ignore

module Program =
  
  let [<Literal>] OKAY = 0

  let iscanfn = System.Console.ReadLine >> ignore

  let runServer host port =
    let baseURI = sprintf "http://%s:%i/" host port
    use appHost = new AppHost()
    appHost.Init()
    appHost.Start(baseURI)
    printfn "Serving requests on %s... Press <RETURN> to exit." baseURI
    iscanfn()
  
  let (|Int|_|) value =
    match Int32.TryParse(value) with
    | true ,number -> Some(number)
    | false,_      -> None

  [<EntryPoint>]
  let Main args =
    match args with
    | [|                  |] -> runServer "localhost" 1979
    | [|       Int(port); |] -> runServer "localhost" port
    | [| host;            |] -> runServer host 1979
    | [| host; Int(port); |] -> runServer host port
    | _ -> printfn "usage: MazsesJam.Svc.exe [host] [port]"
    OKAY
