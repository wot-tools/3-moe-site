[<AutoOpen>]
module _3MoeSite.Views.Root
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views


let rootPage =
    [
        div [] [
                headlineBlock "3 Marks of Excellence Data"
                p  [] [ encodedText "This website displays detailed information about players and clans with 3 Marks of Excellence on the EU server." ]
                h2 [] [
                        a [ _href "/marks" ]
                            [ encodedText (System.String.Format("{0:N0} marks", data.Marks.Length)) ]
                        encodedText " | "
                        a [ _href "/tanks" ]
                            [ encodedText (System.String.Format("{0:N0} tanks", data.Tanks.Length)) ]
                        encodedText " | "
                        a [ _href "/players" ]
                            [ encodedText (System.String.Format("{0:N0} players", data.Players.Length)) ]
                        encodedText " | "
                        a [ _href "/clans"]
                            [ encodedText (System.String.Format("{0:N0} clans", data.Clans.Length)) ]
                        ]
                ]
    ]|> layout "Home"