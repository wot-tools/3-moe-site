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

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open GiraffeViewEngine

    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "_3MoeSite" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]

    let partial () =
        h1 [] [ encodedText "_3MoeSite" ]

    let tableTest comparer =
        [
            navigation ()
            h1 [] [ encodedText "Table Name" ]
            [
                { name = "Klaus"; age = 12; favoriteColor = "yellow" }
                { name = "Dieter"; age = 35; favoriteColor = "blue" }
                { name = "Karl"; age = 45; favoriteColor = "orange" }
                { name = "Fritz"; age = 23; favoriteColor = "purple" }
                { name = "Bob"; age = 64; favoriteColor = "beige" }
            ] |> List.sortWith comparer
              |> customTable [
                { name = "Name"; selector = fun o -> o.name }
                { name = "Age"; selector = fun o -> string o.age }
                { name = "Favorite Color"; selector = fun o -> o.favoriteColor }
            ]
        ] |> layout

    let tableTestDefault () = tableTest (fun o1 o2 -> o1.name.CompareTo o2.name)

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
            navigation()
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

let tableHandler () = htmlView (Views.tableTestDefault ())

[<CLIMutable>]
type SortParams = { Name : string; Direction : string }

let sortHandler (params : SortParams) =
    let _compare prop objs =
        let o1, o2 = objs 
        compare (prop o1) (prop o2)

    let compareTestObjects o1 o2 = 
        let objs = match params.Direction with
                   | "asc" -> (o1, o2)
                   | "desc" -> (o2, o1)
                   | _ -> (o1, o2)

        _compare (fun (o : TestObject) -> 
            match params.Name with
            | "Name" -> o.name
            | "Age" -> string o.age
            | "Favorite Color" -> o.favoriteColor
            | _ -> o.name
        ) objs

    htmlView (Views.tableTest compareTestObjects)


let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler "world"
                routef "/hello/%s" indexHandler
                route "/table" >=> tryBindQuery<SortParams> (fun err -> RequestErrors.BAD_REQUEST err) (Some CultureInfo.InvariantCulture) sortHandler
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