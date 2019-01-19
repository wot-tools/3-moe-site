[<AutoOpen>]
module _3MoeSite.Views.Clan
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views

let data = WGApiDataProvider.Instance

let clanPage id =
    let (success, clan) = data._Clans.TryGetValue id

    match success with
    | false -> errorBlock (sprintf "Could not find the clan with Wargaming ID '%i'" id)        
    | true ->
        [
            div [ _class "clanInfoBlock"] [
                div [ _class "clanImageDiv" ] [
                    img [ _class "clanImage" 
                          _src clan.Emblems.x195.Portal ]
                ]
                div [ _class "clanInfoWrapper" ] [
                    div [ _class "clanName" ] [
                        encodedText clan.Name
                    ]
                    div [ _class "clanTag" ] [
                        encodedText clan.Tag
                    ]
                    div [ _class "moeCount"] [
                        img [ _src "/img/3stars.png" 
                              _class "image3moe" ]
                        encodedText (System.String.Format("{0:N0}", clan.ThreeMoe))
                    ]
                    div [] [ encodedText (sprintf "%i Members" clan.Members) ]
                    div [ _class "externalClanLinks" ] [
                        linkWithImage (sprintf "https://eu.wargaming.net/clans/wot/%i/" clan.ID) "WG Profile" "http://eu.wargaming.net/favicon.ico"
                        linkWithImage (sprintf "https://wotlabs.net/eu/clan/%s" clan.Tag) "Wotlabs" "http://wotlabs.net/images/favicon.png"
                    ]
                ]
            ]
            div [ _class "valueBoxes clearfix"] [
                    dateBlock clan.CreatedAt "Created at"
                    dateBlock clan.LastChecked "Last checked"
                    dateBlock clan.UpdatedWG "Last update (WG)"
                    dateBlock clan.TrackingStarted "Tracking started"
                ]
        ] |> layout clan.Tag


let clansTable params =
    let tableTemplate = customTable ([
        createCustomColumn "Name"       "name"  (fun c -> S c.Name)
            (fun c -> a [ _href (sprintf "/clan/%i" c.ID) ] [ encodedText c.Name ])
        createColumn "Tag"              "tag"   (fun c -> S c.Tag)
        createColumn "3 MoE"            "moe"   (fun c -> TableCellObject.I c.ThreeMoe)
        createColumn "MoE Rating"       "moer"  (fun c -> TableCellObject.D c.MoeRating)
        createColumn "Last update (WG)" "lupd"  (fun c -> T c.UpdatedWG)
        createColumn "Created at"       "created"(fun c -> T c.CreatedAt)
        createColumn "Tracking started" "trsta" (fun c -> T c.TrackingStarted)
        createColumn "Last checked at"  "lch"   (fun c -> T c.LastChecked)
    ] : Clan Column List) params

    [
        headlineBlock "Clans"
        data.Clans |> tableTemplate
    ] |> layout "Clans"


let clanHandler (id : int) =
    htmlView (clanPage id)