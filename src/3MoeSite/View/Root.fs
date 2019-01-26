[<AutoOpen>]
module _3MoeSite.Views.Root
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views
open _3MoeSite


let rootPage =
    [
        div [] [
                headlineBlock "3 Marks of Excellence Data"
                p  [] [ encodedText "This website displays detailed information about players and clans with 3 Marks of Excellence on the EU server." ]
                h2 [] [
                        a [ _href Links.markIndex ]
                            [ encodedText (System.String.Format("{0:N0} marks", data.Marks.Length)) ]
                        encodedText " | "
                        a [ _href Links.tankIndex ]
                            [ encodedText (System.String.Format("{0:N0} tanks", data.Tanks.Length)) ]
                        encodedText " | "
                        a [ _href Links.playerIndex ]
                            [ encodedText (System.String.Format("{0:N0} players", data.Players.Length)) ]
                        encodedText " | "
                        a [ _href Links.clanIndex ]
                            [ encodedText (System.String.Format("{0:N0} clans", data.Clans.Length)) ]
                        ]
                ]
    ]|> layout "Home"