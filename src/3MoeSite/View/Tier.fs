[<AutoOpen>]
module _3MoeSite.Views.Tier
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views
open _3MoeSite

let data = WGApiDataProvider.Instance

type Tier (tier, tankCount, threeMoECount) =
    member this.Tier = tier
    member this.TankCount = tankCount
    member this.ThreeMoECount = threeMoECount

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
        createCustomColumn "Tier"       "tier"      (fun p -> TableCellObject.I p.Tier)
            (fun p -> a [ _href (Links.tierPage p.Tier) ] [ encodedText (sprintf "%i" p.Tier)])
        createColumn       "Tanks"      "tanks"     (fun p -> TableCellObject.I p.TankCount)
        createColumn       "3 MoE"      "3moe"      (fun p -> TableCellObject.I p.ThreeMoECount)
        ] : Tier Column list) params
        
    [
        headlineBlock "Tiers"
        [| 5 .. 10|] |> Seq.map(fun t -> Tier(t, data.Tanks.Count(fun s -> s.Tier = t), data.Tanks.Where(fun m -> m.Tier = t).Sum(fun r -> r.ThreeMoeCount))) |> Seq.toArray |> tableTemplate
    ] |> layout "Tiers"

        
let tierHandler (id : int) (params :  TableParams) =
    htmlView (tierPage id params)