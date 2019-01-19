[<AutoOpen>]
module _3MoeSite.Views.Mark
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views

let data = WGApiDataProvider.Instance


let marksTable params =
    let tableTemplate = customTable ([
        createCustomColumn "Player"     "player"(fun m -> S m.Player.Name)
            (fun m -> a [ _href (sprintf "/player/%i" m.Player.ID) ] [ encodedText m.Player.Name ])
        createCustomColumn "Clan"       "clan"  (fun m -> S m.Clan.Name)
            (fun m -> a [ _href (sprintf "/clan/%i" m.Clan.ID) ] [ encodedText m.Clan.Name ])
        createCustomColumn "Tank"       "tank"  (fun m -> S m.Tank.Name)
            (fun m -> a [ _href (sprintf "/tank/%i" m.Tank.ID) ] [ encodedText m.Tank.Name ])
        createColumn "First Detected At""det"   (fun m -> T m.FirstDetected) 
    ] : Mark Column List) params

    [
        headlineBlock "Marky Marks"
        data.Marks |> tableTemplate
    ] |> layout "Marky Marks"