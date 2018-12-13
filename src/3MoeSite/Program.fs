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

    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "_3MoeSite" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] (navigation () :: content)
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
        ] |> layout

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
        ] |> layout

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
        ] |> layout

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
        ] |> layout

    let foo () =
        div [ _class "foo"] [
            table [] [
                [1 .. 5] |> List.map (fun i -> [i |> string |> encodedText] |> td []) |> tr []
            ]
            a [ _href "https://www.google.com/"
                _target "blank"
            ] [encodedText "click for google"]
        ]

    let playerPage id =
        let player = data._Players.[id]

        [
            h1 [] [ encodedText player.Name ]
            h2 [] [ encodedText player.Clan.Name ]
        ] |> layout

    let clanPage id =
        let clan = data._Clans.[id]

        [
            h1 [] [ encodedText clan.Name ]
            h2 [] [ encodedText clan.Tag ]
            h2 [] [ encodedText (string clan.CreatedAt) ]
            h2 [] [ encodedText "players" ]
        ] |> layout

    let tankPage id =
        let tank = data._Tanks.[id]

        [
            h1 [] [ encodedText tank.Name ]
            h2 [] [ encodedText (string tank.Nation) ]
            h2 [] [ encodedText (string tank.Type) ]
            h2 [] [ encodedText "players with marks" ]
        ] |> layout

    let index (model : Message) =
        [
            partial()
            p [] [ encodedText model.Text ]
            foo()
        ] |> layout

// ---------------------------------
// Web app
// ---------------------------------

let indexHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let model     = { Text = greetings }
    let view      = Views.index model
    htmlView view

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
                route "/" >=> indexHandler "world"
                routef "/hello/%s" indexHandler
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