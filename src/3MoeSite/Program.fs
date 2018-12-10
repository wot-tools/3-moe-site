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

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open GiraffeViewEngine

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
        let tableTemplate = customTable [
                { name = "Name"; sortName = "name"; selector = fun ( p : Player ) -> S p.Name }
                { name = "Clan"; sortName = "clan"; selector = fun p -> S (string p.Clan) }
                { name = "3 MoE"; sortName = "moe"; selector = fun p -> I p.ThreeMoeCount }
                { name = "Battles"; sortName = "battles"; selector = fun p -> I p.BattleCount }
                { name = "Win ratio"; sortName = "wr"; selector = fun p -> M p.WinRatio }
                { name = "WN8"; sortName = "wn8"; selector = fun p -> D p.Wn8 }
                { name = "MoE Rating"; sortName = "moer"; selector = fun p -> D p.MoeRating }
                { name = "WG Rating"; sortName = "wgr"; selector = fun p -> I p.WgRating }
                { name = "Client language"; sortName = "lang"; selector = fun p -> S p.ClientLanguage }
                { name = "Last battle"; sortName = "lbatt"; selector = fun p -> T p.LastBattle }
                { name = "Last logout"; sortName = "llog"; selector = fun p -> T p.LastLogout }
                { name = "Account created"; sortName = "created"; selector = fun p -> T p.AccountCreated }
                { name = "Last update (WG)"; sortName = "lupd"; selector = fun p -> T p.LastWgUpdate }
                { name = "Last checked"; sortName = "lch"; selector = fun p -> T p.LastChecked } ] params

        [
            h1 [] [ encodedText "Players" ]
            data.Players |> tableTemplate
        ] |> layout

    let marksTable params =
        let tableTemplate = customTable [
            { name = "Player"; sortName = "player"; selector = fun ( m : Mark ) -> S m.Player.Name }
            { name = "Clan"; sortName = "clan"; selector = fun m -> S m.Clan.Name }
            { name = "Tank"; sortName = "tank"; selector = fun m -> S m.Tank.Name }
            { name = "First Detected At"; sortName = "det"; selector = fun m -> T m.FirstDetected } ] params

        [
            h1 [] [ encodedText "Marky Marks" ]
            data.Marks |> tableTemplate
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

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler "world"
                routef "/hello/%s" indexHandler
                route "/players"  >=> tryBindQuery<TableParams> (fun err -> RequestErrors.BAD_REQUEST err) (Some CultureInfo.InvariantCulture) (Views.playersTable >> htmlView)
                route "/marks"    >=> tryBindQuery<TableParams> (fun err -> RequestErrors.BAD_REQUEST err) (Some CultureInfo.InvariantCulture) (Views.marksTable   >> htmlView)
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