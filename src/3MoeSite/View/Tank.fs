[<AutoOpen>]
module _3MoeSite.Views.Tank
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views
open _3MoeSite

let data = WGApiDataProvider.Instance

let tankPage (id : int) (params : TableParams) =
    let (success, tank) = data._Tanks.TryGetValue id

    match success with
    | false -> errorBlock (sprintf "Could not find the tank with ID '%i'" id)        
    | true ->
        [
            div [ _class "tankInfoBlock" ] [
                div [ _class "tankImageDiv" ] [
                    img [ _src tank.Icons.Big ]
                ]
                div [ _class "tankDataDiv" ] [
                    div [ _class "tankName" ] [
                        encodedText tank.Name
                    ]
                    div [ _class "moeCount" ] [
                        img [ _src "/img/3stars.png" 
                              _class "image3moe" ]
                        encodedText (System.String.Format("{0:N0}", tank.ThreeMoeCount))
                    ]
                    div [] [
                        img [ _class "imageNation" 
                              _src (Links.nationFlag ((string tank.Nation).ToLower())) ]
                        a [ _href (Links.nationPage (int tank.Nation)) ] [ encodedText (string tank.Nation) ]
                        encodedText ", "
                        a [ _href (Links.tierPage tank.Tier) ] [ encodedText (sprintf "%i" tank.Tier) ]
                        encodedText ", "
                        a [ _href (Links.vehicleTypePage (int tank.VehicleType)) ] [ encodedText (string tank.VehicleType) ]
                        img [ _class "imageTankType" 
                              _src "" ]
                    ]
                ]                    
            ]
            (match data._TanksMarks.TryGetValue id with
            | false, _ -> encodedText "no marks"
            | true, marks ->
                marks.Values |> Seq.toArray |> customTable ([
                    createCustomColumn "Name"       "name"           (fun m -> S m.Player.Name)
                        (fun m -> a [ _href (Links.playerPage m.Player.ID) ] [ encodedText m.Player.Name ])
                    createCustomColumn "Clan"       "clan"           (fun m -> S (string m.Player.Clan.Tag))
                        (fun m -> a [ _href (Links.clanPage m.Player.Clan.ID) ] [ encodedText m.Player.Clan.Tag ])
                    createColumn "Frist detected"   "firstDetection" (fun m -> T m.FirstDetected)
                    createColumn "3 MoE"            "moe"            (fun m -> TableCellObject.I m.Player.ThreeMoeCount)
                    createColumn "Battles"          "battles"        (fun m -> TableCellObject.I m.Player.BattleCount)
                    createColumn "Win ratio"        "wr"             (fun m -> Perc m.Player.WinRatio)
                    createColumn "WN8"              "wn8"            (fun m -> TableCellObject.D m.Player.Wn8)
                    createColumn "MoE Rating"       "moer"           (fun m -> TableCellObject.D m.Player.MoeRating)
                    createColumn "WG Rating"        "wgr"            (fun m -> TableCellObject.I m.Player.WgRating)
                    createColumn "Client language"  "lang"           (fun m -> Flag m.Player.ClientLanguage)
                    createColumn "Last battle"      "lbatt"          (fun m -> T m.Player.LastBattle)
                    createColumn "Last logout"      "llog"           (fun m -> T m.Player.LastLogout)
                    createColumn "Account created"  "created"        (fun m -> T m.Player.AccountCreated)
                    createColumn "Last update (WG)" "lupd"           (fun m -> T m.Player.LastWgUpdate)
                    createColumn "Last checked"     "lch"            (fun m -> T m.Player.LastChecked)
                    ] : Mark Column list) params)
        ] |> layout tank.Name

        
let tanksTable params =
    [
        headlineBlock "Tank Overview"
        data.Tanks |> tankDisplayTable params
    ] |> layout "Tanks"
        

let tankHandler (id : int) (params : TableParams) =
    htmlView (tankPage id params)