[<AutoOpen>]
module _3MoeSite.Views.About
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views
open _3MoeSite


let aboutPage = 
    [
        headlineBlock "MoE Rating"
        div [ _class "aboutMoERating" ] [
            p [] [
                    encodedText "Each 3 MoE on a tank is worth a certain amount of points depending on the tier and tank type of the tank and the amount of players with 3 MoE on it (indicator for the difficulty). The rating of a player is the sum of points for each tank he 3-marked, rounded down to the nearest integer. The rating of a clan is the sum of the rating of its players."
            ]
            p [] [
                encodedText "Keep in mind that this (currently) just me playing around with some numbers because it seemed like a fun thing to do."
            ]
            img [ _src "/img/moeRatingFormula.png" ]
        ]
        headlineBlock "Credits"
        p [] [ 
            encodedText "This website uses the "
            a [ _href Links.flagIconsSource ] [ encodedText "flag icons" ]
            encodedText " created by Mark James."
        ]
    ] |> layout "About"