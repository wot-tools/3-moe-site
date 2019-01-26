[<AutoOpen>]
module _3MoeSite.Views.Layout
open Giraffe
open _3MoeSite
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
                    a  [_href Links.threadWoTForum ] [ encodedText "WoT forum thread" ]
                    encodedText " | "
                    a [ _href Links.threadWotlabsForum ] [ encodedText "Wotlabs forum thread" ]
                    encodedText " | Created by Aim_Drol and pun_xD 2016 - 2019"
                ]
            ]
        ]
    ]