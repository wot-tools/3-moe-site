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
// Web app
// ---------------------------------

let tableBinding (viewFunc : TableParams -> GiraffeViewEngine.XmlNode) defaultParams =
    tryBindQuery<TableParams>
        (fun err -> (viewFunc >> htmlView) defaultParams) // abusing error func to show default when empty args, but faulty args will trigger that too
        (Some CultureInfo.InvariantCulture)
        (viewFunc >> htmlView)

let combinedHandler viewFunc defaultParams (id : int) : HttpHandler =
    fun (next : HttpFunc) (ctx : Microsoft.AspNetCore.Http.HttpContext) ->
        FSharp.Control.Tasks.ContextSensitive.task {
            let result = ctx.TryBindQueryString<TableParams> CultureInfo.InvariantCulture

            let finalParams = match result with
                                | Error err -> defaultParams
                                | Ok params -> params
            
            return! viewFunc id finalParams next ctx
        }

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> htmlView Views.Root.rootPage
                route "/about" >=> htmlView Views.About.aboutPage
                routef "/player/%i" (combinedHandler playerHandler { sort = "firstDetection"; direction = "desc"; page = 1 })
                routef "/clan/%i" clanHandler
                routef "/tank/%i" (combinedHandler tankHandler { sort = "firstDetection"; direction = "asc"; page =1 })
                route "/players" >=> tableBinding Views.Player.playersTable { sort = "moer"; direction = "desc"; page = 1 }
                route "/marks"   >=> tableBinding Views.Mark.marksTable { sort = "det"; direction = "desc"; page = 1 }
                route "/clans"   >=> tableBinding Views.Clan.clansTable { sort = "moe"; direction = "desc"; page = 1 }
                route "/tanks"   >=> tableBinding Views.Tank.tanksTable { sort = "moe"; direction = "asc"; page = 1 }
                routef "/tier/%i" (combinedHandler Views.Tier.tierHandler { sort = "moe"; direction = "asc"; page = 1 })
                route "/tiers"  >=> tableBinding Views.Tier.tiersTable { sort = "tier"; direction = "desc"; page = 1 }
                routef "/type/%i" (combinedHandler Views.VehicleType.vehicleTypeHandler { sort = "moe"; direction = "asc"; page = 1 })
                route "/types"  >=> tableBinding Views.VehicleType.vehicleTypeTable { sort = "type"; direction = "asc"; page = 1 }
                routef "/nation/%i" (combinedHandler Views.Nation.nationHandler { sort = "moe"; direction = "asc"; page = 1 })
                route "/nations"  >=> tableBinding Views.Nation.nationTable { sort = "nation"; direction = "asc"; page = 1 }
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