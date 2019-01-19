[<AutoOpen>]
module _3MoeSite.Views.Tier
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views

let data = WGApiDataProvider.Instance

let tierPage (id : int) (params : TableParams) = 
    let success = id > 0 && id < 11

    match success with
    | false -> errorBlock(sprintf "Tier %i is not valid. It has to be between 1 and 10" id)
    | true ->
        [
            headlineBlock (sprintf "Tier %i tanks" id)
            data.Tanks |> Array.where(fun t -> t.Tier = id) |> tankDisplayTable params
        ] |> layout (sprintf "Tier %i" id)

        
let tiersTable params =
    let tableTemplate = customTable ([
        createCustomColumn "Tier"       "tier"      (fun p -> TableCellObject.I p)
            (fun p -> a [ _href (sprintf "/tier/%i" p) ] [ encodedText (sprintf "%i" p)])
        ] : int Column list) params
        
    [
        headlineBlock "Tiers"
        [| 1 .. 10|] |> tableTemplate
    ] |> layout "Tiers"

        
let tierHandler (id : int) (params :  TableParams) =
    htmlView (tierPage id params)