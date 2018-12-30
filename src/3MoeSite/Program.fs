module _3MoeSite.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open _3MoeSite.Model
open _3MoeSite.Views
open System.Globalization
open WGApiDataProvider
open Microsoft.EntityFrameworkCore.Diagnostics

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open GiraffeViewEngine
    open Newtonsoft.Json
    open Giraffe.HttpStatusCodeHandlers

    //let data = MockupDataProvider.Instance
    let data = WGApiDataProvider.Instance

    let layout customTitlePrefix (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText (sprintf "%s | 3 MoE" customTitlePrefix) ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] (navigation () :: content)
            footer [] [
                br []
                hr []
                p  [ _class "footerText" ] [
                    a  [_href "http://forum.worldoftanks.eu/index.php?/topic/524379-marks-of-excellence-data-a-new-metric-has-been-born-190117-dank-marks-can-melt-steel-beams/" ] [ encodedText "WoT forum thread" ]
                    encodedText " | "
                    a [ _href "http://forum.wotlabs.net/index.php?/topic/22352-marks-of-excellence-data-a-new-metric-has-been-born-251216-last-christmas-i-gave-you-my-marks/" ] [ encodedText "Wotlabs forum thread" ]
                    encodedText " | Created by Aim_Drol and pun_xD 2k16 - 2k17 \\//\\/"
                ]
            ]
        ]

    let partial () =
        h1 [] [ encodedText "_3MoeSite" ]
      
    let playersTable params =
        let tableTemplate = customTable ([
            createCustomColumn "Name"       "name"      (fun p -> S p.Name)
                (fun p -> a [ _href (sprintf "/player/%i" p.ID) ] [ encodedText p.Name ])
            createCustomColumn "Clan"       "clan"      (fun p -> S (string p.Clan))
                (fun p -> a [ _href (sprintf "/clan/%i" p.Clan.ID) ] [ encodedText p.Clan.Name ])
            createColumn "3 MoE"            "moe"       (fun p -> I p.ThreeMoeCount)
            createColumn "Battles"          "battles"   (fun p -> I p.BattleCount)
            createColumn "Win ratio"        "wr"        (fun p -> M p.WinRatio)
            createColumn "WN8"              "wn8"       (fun p -> D p.Wn8)
            createColumn "MoE Rating"       "moer"      (fun p -> D p.MoeRating)
            createColumn "WG Rating"        "wgr"       (fun p -> I p.WgRating)
            createColumn "Client language"  "lang"      (fun p -> S p.ClientLanguage)
            createColumn "Last battle"      "lbatt"     (fun p -> T p.LastBattle)
            createColumn "Last logout"      "llog"      (fun p -> T p.LastLogout)
            createColumn "Account created"  "created"   (fun p -> T p.AccountCreated)
            createColumn "Last update (WG)" "lupd"      (fun p -> T p.LastWgUpdate)
            createColumn "Last checked"     "lch"       (fun p -> T p.LastChecked)
        ] : Player Column list) params

        [
            h1 [] [ encodedText "Players" ]
            data.Players |> tableTemplate
        ] |> layout "Players"

    let marksTable params =
        let tableTemplate = customTable ([
            createCustomColumn "Player"     "player"(fun m -> S m.Player.Name)
                (fun m -> a [ _href (sprintf "/player/%i" m.Player.ID) ] [ encodedText m.Player.Name ])
            createCustomColumn "Clan"       "clan"  (fun m -> S m.Clan.Name)
                (fun m -> a [ _href (sprintf "/clan/%i" m.Clan.ID) ] [ encodedText m.Clan.Name ])
            createCustomColumn "Tank"       "tank"  (fun m -> S m.Tank.Name)
                (fun m -> a [ _href (sprintf "/tank/%i" m.Tank.ID) ] [ encodedText m.Tank.Name ])
            createColumn "First Detected At""det"   (fun m -> T m.FirstDetected) 
        ] : Mark Column List) params

        [
            h1 [] [ encodedText "Marky Marks" ]
            data.Marks |> tableTemplate
        ] |> layout "Marky Marks"

    let clansTable params =
        let tableTemplate = customTable ([
            createCustomColumn "Name"       "name"  (fun c -> S c.Name)
                (fun c -> a [ _href (sprintf "/clan/%i" c.ID) ] [ encodedText c.Name ])
            createColumn "Tag"              "tag"   (fun c -> S c.Tag)
            createColumn "3 MoE"            "moe"   (fun c -> I c.ThreeMoe)
            createColumn "MoE Rating"       "moer"  (fun c -> D c.MoeRating)
            createColumn "Last update (WG)" "lupd"  (fun c -> T c.UpdatedWG)
            createColumn "Created at"       "created"(fun c -> T c.CreatedAt)
            createColumn "Tracking started" "trsta" (fun c -> T c.TrackingStarted)
            createColumn "Last checked at"  "lch"   (fun c -> T c.LastChecked)
        ] : Clan Column List) params

        [
            h1 [] [ encodedText "Clans" ]
            data.Clans |> tableTemplate
        ] |> layout "Clans"

    let tanksTable params =
        let tableTemplate = customTable ([
            createColumn "Name"         "name"  (fun t -> S t.Name)
            createColumn "Short Name"   "sn"    (fun t -> S t.ShortName)
            createColumn "3 MoE"        "moe"   (fun t -> I t.ThreeMoeCount)
            createColumn "MoE Value"    "moev"  (fun t -> D t.MoeValue)
            createColumn "Tier"         "tier"  (fun t -> I t.Tier)
            createColumn "Nation"       "nat"   (fun t -> E t.Nation)
            createColumn "Type"         "type"  (fun t -> E t.Type)
        ] : Tank Column List) params

        [
            h1 [] [encodedText "Tanks" ]
            data.Tanks |> tableTemplate
        ] |> layout "Tanks"

    let foo () =
        div [ _class "foo"] [
            table [] [
                [1 .. 5] |> List.map (fun i -> [i |> string |> encodedText] |> td []) |> tr []
            ]
            a [ _href "https://www.google.com/"
                _target "blank"
            ] [encodedText "click for google"]
        ]

    let rootPage () =
        [
            div [] [
                    h1 [] [ encodedText "3 Marks of Excellence Data" ]
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

    let getWn8CssClass (wn8 : float) =
        match wn8 with
        | i when i < 300.0 -> "cVeryBad"
        | i when i < 450.0 -> "cBad"
        | i when i < 650.0 -> "cBelowAverage"
        | i when i < 900.0 -> "cAverage"
        | i when i < 1200.0 -> "cAboveAverage"
        | i when i < 1600.0 -> "cGood"
        | i when i < 2000.0 -> "cVeryGood"
        | i when i < 2450.0 -> "cGreat"
        | i when i < 2900.0 -> "cUnicum"
        | i -> "cSuperUnicum"

    let getWinratioCssClass (winratio : decimal) =
        match winratio with
        | i when i < 0.46m -> "cVeryBad"
        | i when i < 0.47m -> "cBad"
        | i when i < 0.48m -> "cBelowAverage"
        | i when i < 0.50m -> "cAverage"
        | i when i < 0.52m -> "cAboveAverage"
        | i when i < 0.54m -> "cGood"
        | i when i < 0.56m -> "cVeryGood"
        | i when i < 0.60m -> "cGreat"
        | i when i < 0.65m -> "cUnicum"
        | i -> "cSuperUnicum" 

    let errorBlock errorText = 
        [
            div [ _class "errorBox"] [
                span [] [ encodedText "Error" ]
                br []
                encodedText errorText
            ]
        ] |> layout "Error!"

    let linkWithImage (link : string) (title: string) (imgSrc : string) = 
        a [ _href link
            _target "blank" 
            _class "linkWithImage"] [
                img [ _src imgSrc ]
                encodedText title
          ]

    type StatValueObject =
        | I of int
        | D of decimal
        | F of float

    let printStatValueObject (o : StatValueObject) =
        match o with
        | I i -> System.String.Format("{0:N0}", i)
        | D d -> System.String.Format("{0:P2}", d)
        | F f -> System.String.Format("{0:N0}", f)

    let playerPage id =
        let (success, player) = data._Players.TryGetValue id

        let dateBlock (displayValue : DateTime) text = 
            div [ _class "dateValueBox" ] [
                    div [ _class "dateValueBoxDiv" ] [ encodedText (System.String.Format("{0}", displayValue)) ]
                    encodedText text
                ]

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
                                encodedText player.ClientLanguage
                            ]
                            div [ _class "moeCount" ] [
                                encodedText (System.String.Format("{0:N0}", player.ThreeMoeCount))
                            ]
                            div [ _class "playerLinkDiv"] [
                                linkWithImage (sprintf "https://worldoftanks.eu/en/community/accounts/%i" player.ID) "WG Profile " "http://eu.wargaming.net/favicon.ico"
                                linkWithImage (sprintf "http://wotlabs.net/eu/player/%s" player.Name) "Wotlabs" "http://wotlabs.net/images/favicon.png"
                                    ]
                            ]
                        ]
                        div [ _class "clanInfoDiv" ] [
                            img [ _src (System.String.Format("https://eu.wargaming.net/clans/media/clans/emblems/cl_335/{0}/emblem_32x32.png", player.Clan.ID)) 
                                  _class "clanIcon" ]
                            span [ _class "clanTag" ] [ 
                                    a [ _href (sprintf "/clan/%i" player.Clan.ID)
                                        _target "blank" ] [
                                            encodedText player.Clan.Tag
                                    ]
                            ]
                            div [ _class "clanLinkDiv" ] [
                                linkWithImage (sprintf "https://eu.wargaming.net/clans/wot/%i" player.Clan.ID) "WG Profile" "http://eu.wargaming.net/favicon.ico"
                                linkWithImage (sprintf "https://wotlabs.net/eu/clan/%s" player.Clan.Tag) "Wotlabs" "http://wotlabs.net/images/favicon.png"
                            ]
                        ]
                    ]
                    div [ _class "valueBoxes clearfix"] [
                        statBlockColoredBackground (F player.Wn8) "WN8" (getWn8CssClass player.Wn8)
                        statBlockColoredBackground (D player.WinRatio) "Winratio" (getWinratioCssClass player.WinRatio)
                        statBlock (I player.BattleCount) "Battles"
                        statBlock (F player.MoeRating) "MoE Rating"
                        statBlock (I player.WgRating) "WG Rating"
                    ]
                    div [ _class "valueBoxes clearfix"] [
                        dateBlock player.LastBattle "Last Battle"
                        dateBlock player.LastLogout "Last Logout"
                        dateBlock player.LastChecked "Last checked"
                        dateBlock player.AccountCreated "Account created"
                        dateBlock player.LastWgUpdate "Last update (WG)"
                    ]
                ]
            ] |> layout player.Name

    let clanPage id =
        let clan = data._Clans.[id]

        [
            h1 [] [ encodedText clan.Name ]
            h2 [] [ encodedText clan.Tag ]
            h2 [] [ encodedText (string clan.CreatedAt) ]
            h2 [] [ encodedText "players" ]
        ] |> layout clan.Tag

    let tankPage id =
        let tank = data._Tanks.[id]

        [
            h1 [] [ encodedText tank.Name ]
            h2 [] [ encodedText (string tank.Nation) ]
            h2 [] [ encodedText (string tank.Type) ]
            h2 [] [ encodedText "players with marks" ]
        ] |> layout tank.Name

    let index (model : Message) =
        [
            partial()
            p [] [ encodedText model.Text ]
            foo()
        ] |> layout "Index"

// ---------------------------------
// Web app
// ---------------------------------

let indexHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let model     = { Text = greetings }
    let view      = Views.index model
    htmlView view

let rootHandler =
    htmlView (Views.rootPage())

let playerHandler (id : int) =
    htmlView (Views.playerPage id)

let clanHandler (id : int) =
    htmlView (Views.clanPage id)

let tankHandler (id : int) =
    htmlView (Views.tankPage id)

let tableBinding (viewFunc : TableParams -> GiraffeViewEngine.XmlNode) defaultParams =
    tryBindQuery<TableParams>
        (fun err -> (viewFunc >> htmlView) defaultParams) // abusing error func to show default when empty args, but faulty args will trigger that too
        (Some CultureInfo.InvariantCulture)
        (viewFunc >> htmlView)

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> rootHandler
                routef "/player/%i" playerHandler
                routef "/clan/%i" clanHandler
                routef "/tank/%i" tankHandler
                route "/players" >=> tableBinding Views.playersTable { sort = "moer"; direction = "desc"; page = 1 }
                route "/marks"   >=> tableBinding Views.marksTable { sort = "det"; direction = "desc"; page = 1 }
                route "/clans"   >=> tableBinding Views.clansTable { sort = "moe"; direction = "desc"; page = 1 }
                route "/tanks"   >=> tableBinding Views.tanksTable { sort = "moe"; direction = "asc"; page = 1 }
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
           .AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0