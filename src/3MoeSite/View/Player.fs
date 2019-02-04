[<AutoOpen>]
module _3MoeSite.Views.Player
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views
open _3MoeSite

let data = WGApiDataProvider.Instance

let playerPage (id : int) (params : TableParams) =
    let (success, player) = data._Players.TryGetValue id

    let statBlockColoredBackground (value : StatValueObject) (title : string) (colorClass : string)= 
        div [ _class (sprintf "statValueBox darkBorder %s" colorClass) ] [
                div [ _class "statValueBoxDiv" ] [ encodedText (printStatValueObject value) ]
                encodedText title
            ]

    let statBlock (value : StatValueObject) (title : string) = 
        div [ _class "statValueBox" ] [
                div [ _class "statValueBoxDiv" ] [ encodedText (printStatValueObject value) ]
                encodedText title
            ]        

    match success with
    | false -> errorBlock (sprintf "Could not find the player with Wargaming ID '%i'" id)
    | true ->
        [
            div [ _class "playerInfoBlock"] [
                div [ _class "clearfix" ] [
                    div [ _class "playerInfoDiv"] [
                        div [ _class "playerName" ] [
                            encodedText player.Name
                            img [ _src (Links.clientLanguageFlag player.ClientLanguage) ]
                        ]
                        div [ _class "moeCount"] [
                        img [ _src "/img/3stars.png" 
                              _class "image3moe" ]
                        encodedText (System.String.Format("{0:N0}", player.ThreeMoeCount))
                        ]
                        div [ _class "playerLinkDiv"] [
                            linkWithImage (Links.playerProfileWG player.ID) "WG Profile " "http://eu.wargaming.net/favicon.ico"
                            linkWithImage (Links.playerProfileWotlabs player.Name) "Wotlabs" "http://wotlabs.net/images/favicon.png"
                        ]
                    ]
                    div [ _class (sprintf "clanInfoDiv %s" (if player.Clan.ID = 0 then "hidden" else "" )) ] [
                        img [ _src player.Clan.Emblems.x32.Portal  
                              _class "clanIcon" ]
                        span [ _class "clanTag" ] [ 
                                a [ _href (Links.clanPage player.Clan.ID)
                                    _target "blank" ] [
                                        encodedText player.Clan.Tag
                                ]
                        ]
                        div [ _class "clanLinkDiv" ] [
                            linkWithImage (Links.clanProfileWG player.Clan.ID) "WG Profile" "http://eu.wargaming.net/favicon.ico"
                            linkWithImage (Links.clanProfileWotlabs player.Clan.Tag) "Wotlabs" "http://wotlabs.net/images/favicon.png"
                        ]
                    ]
                ]
                div [ _class "valueBoxes clearfix"] [
                    statBlockColoredBackground (StatValueObject.F player.Wn8) "WN8" (getWn8CssClass player.Wn8)
                    statBlockColoredBackground (StatValueObject.D player.WinRatio) "Winratio" (getWinratioCssClass player.WinRatio)
                    statBlock (StatValueObject.I player.BattleCount) "Battles"
                    statBlock (StatValueObject.F player.MoeRating) "MoE Rating"
                    statBlock (StatValueObject.I player.WgRating) "WG Rating"
                ]
                div [ _class "valueBoxes clearfix"] [
                    dateBlock player.LastBattle "Last Battle"
                    dateBlock player.LastLogout "Last Logout"
                    dateBlock player.LastChecked "Last checked"
                    dateBlock player.AccountCreated "Account created"
                    dateBlock player.LastWgUpdate "Last update (WG)"
                ]
            ]

            (match data._PlayersMarks.TryGetValue player.ID with
            | false, _ -> div [ _class "noMarksDiv" ][ 
                            encodedText "This player has no 3 Marks of Excellence on any tank."                
                          ]
            | true, marks ->
                marks.Values |> Seq.toArray |> customTable ([
                    createColumn "Contour" "contour" (fun m -> Img m.Tank.Icons.Contour)
                    createCustomColumn "Tank"         "tank"  (fun m -> S m.Tank.Name)
                        (fun m -> (a [ _href (Links.tankPage m.Tank.ID) ] [
                                        encodedText m.Tank.Name ]))
                    createColumn "First detected"   "firstDetection"    (fun m -> T m.FirstDetected)
                    createColumn "3 MoE"        "moe"   (fun m -> TableCellObject.I m.Tank.ThreeMoeCount)
                    createColumn "MoE Value"    "moev"  (fun m -> TableCellObject.D m.Tank.MoeValue)
                    createCustomColumn "Tier"         "tier"  (fun m -> TableCellObject.I m.Tank.Tier)
                        (fun m -> (linkWithImage (Links.tierPage m.Tank.Tier) (string m.Tank.Tier) ""))
                    createCustomColumn "Nation"       "nat"   (fun m -> E m.Tank.Nation)
                        (fun m -> (linkWithImage (Links.nationPage ((int)m.Tank.Nation)) (string m.Tank.Nation) (Links.nationFlag ((string m.Tank.Nation).ToLower()))))
                    createCustomColumn "Type"         "type"  (fun m -> E m.Tank.VehicleType)
                        (fun m -> (linkWithImage (Links.vehicleTypePage (int m.Tank.VehicleType)) (string m.Tank.VehicleType) ""))
                    ] : Mark Column List) params)
        ] |> layout player.Name


let playersTable params =
    let tableTemplate = customTable ([
        createCustomColumn "Name"       "name"      (fun p -> S p.Name)
            (fun p -> a [ _href (Links.playerPage p.ID) ] [ encodedText p.Name ])
        createCustomColumn "Clan"       "clan"      (fun p -> S (string p.Clan.Tag))
            (fun p -> clanLinkWithIcon p.Clan)
        createColumn "3 MoE"            "moe"       (fun p -> TableCellObject.I p.ThreeMoeCount)
        createColumn "Battles"          "battles"   (fun p -> TableCellObject.I p.BattleCount)
        createColumn "Win ratio"        "wr"        (fun p -> Perc p.WinRatio)
        createColumn "WN8"              "wn8"       (fun p -> TableCellObject.D p.Wn8)
        createColumn "MoE Rating"       "moer"      (fun p -> TableCellObject.D p.MoeRating)
        createColumn "WG Rating"        "wgr"       (fun p -> TableCellObject.I p.WgRating)
        createColumn "Client language"  "lang"      (fun p -> Flag p.ClientLanguage)
        createColumn "Last battle"      "lbatt"     (fun p -> T p.LastBattle)
        createColumn "Last logout"      "llog"      (fun p -> T p.LastLogout)
        createColumn "Account created"  "created"   (fun p -> T p.AccountCreated)
        createColumn "Last update (WG)" "lupd"      (fun p -> T p.LastWgUpdate)
        createColumn "Last checked"     "lch"       (fun p -> T p.LastChecked)
    ] : Player Column list) params

    [
        headlineBlock (System.String.Format("{0:N0} Players", data.Players.Length))
        data.Players |> tableTemplate
    ] |> layout "Players"


let playerHandler (id : int) (params : TableParams) =
    htmlView (playerPage id params)