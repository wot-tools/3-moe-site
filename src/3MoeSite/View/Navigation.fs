[<AutoOpen>]
module _3MoeSite.Views.Navigation
open Giraffe.GiraffeViewEngine

let navigation () =
    [
        ([
            ("/", "3 Moe Data")
            ("/stats/distribution", "Mark distribution")
            ("/marks", "Marks")
            ("/tanks", "Tanks")
            ("/players", "players")
            ("/clans", "Clans")
            ("/nations", "Nations")
            ("/vehicle_tyles", "Vehicle types")
            ("/tiers", "Tiers")
        ] 
        |> List.map (fun (url, text) -> li [ _class "headerButton" ] [ a [ _href url ] [ encodedText text ] ] )
        |> ul [ _class "headerNavigationBar" ])
    ] |> div [ _class "headerBox" ]