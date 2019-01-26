[<AutoOpen>]
module _3MoeSite.Views.Navigation
open Giraffe.GiraffeViewEngine
open _3MoeSite

let navigation () =
    [
        ([
            ("/", "3 Moe Data")
            (Links.statsIndex, "Mark distribution")
            (Links.markIndex, "Marks")
            (Links.tankIndex, "Tanks")
            (Links.playerIndex, "players")
            (Links.clanIndex, "Clans")
            (Links.nationIndex, "Nations")
            (Links.vehicleTypeIndex, "Vehicle types")
            (Links.tierIndex, "Tiers")
        ] 
        |> List.map (fun (url, text) -> li [ _class "headerButton" ] [ a [ _href url ] [ encodedText text ] ] )
        |> ul [ _class "headerNavigationBar" ])
    ] |> div [ _class "headerBox" ]