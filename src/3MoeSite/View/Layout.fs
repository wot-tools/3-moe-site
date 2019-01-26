[<AutoOpen>]
module _3MoeSite.Views.Layout
open Giraffe
open _3MoeSite.Views
open Giraffe.GiraffeViewEngine


let layout customTitlePrefix (content: XmlNode list) =
    html [] [
        head [] [
            title []  [ encodedText (sprintf "%s | 3 MoE" customTitlePrefix) ]
            link [ _rel  "stylesheet"
                   _type "text/css"
                   _href "/main.css" ]
            link [ _rel "icon" 
                   _type "image/png" 
                   _href "/img/favicon.png" ]
        ]
        header [] [ navigation () ]
        main [] [
            div [ _class "contentBox" ] content
        ]
        footer [] [
            div [ _class "footerBox" ] [
                p  [ _class "footerText" ] [
                    a  [_href "http://forum.worldoftanks.eu/index.php?/topic/524379-marks-of-excellence-data-a-new-metric-has-been-born-190117-dank-marks-can-melt-steel-beams/" ] [ encodedText "WoT forum thread" ]
                    encodedText " | "
                    a [ _href "http://forum.wotlabs.net/index.php?/topic/22352-marks-of-excellence-data-a-new-metric-has-been-born-251216-last-christmas-i-gave-you-my-marks/" ] [ encodedText "Wotlabs forum thread" ]
                    encodedText " | Created by Aim_Drol and pun_xD 2016 - 2019"
                ]
            ]
        ]
    ]