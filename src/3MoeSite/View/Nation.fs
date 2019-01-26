[<AutoOpen>]
module _3MoeSite.Views.Nation
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views

let data = WGApiDataProvider.Instance

type Nation (id : int, name: string, tankCount, threeMoECount) =
    member this.ID = id
    member this.Name = name
    member this.TankCount = tankCount
    member this.ThreeMoECount = threeMoECount

let nationPage (id : int) (params : TableParams) = 
    let success = Enum.IsDefined(typeof<WGApi.Nations>, id)

    match success with
    | false -> errorBlock(sprintf "Nation %i is not valid." id)
    | true ->        
        let nation = enum<WGApi.Nations>(id)
        [
            headlineBlock ((string) nation)
            data.Tanks |> Array.where(fun t -> t.Nation = nation) |> tankDisplayTable params
        ] |> layout ((string)nation)

        
let nationTable params =
    let tableTemplate = customTable ([
        createCustomColumn "Nation"     "nation"      (fun p -> TableCellObject.S p.Name)
            (fun p -> (linkWithImage (sprintf "/nation/%i" (p.ID)) (string p.Name) (sprintf "https://eu-wotp.wgcdn.co/static/5.32.1_0d6e8f/wotp_static/img/core/frontend/scss/common/components/icons/img/filter-%s.png" (p.Name.ToLower()))))
        createColumn       "Tanks"      "tanks"     (fun p -> TableCellObject.I p.TankCount)
        createColumn       "3 MoE"      "3moe"      (fun p -> TableCellObject.I p.ThreeMoECount)
        ] : Nation Column list) params
        
    [
        headlineBlock "Nations"
        Enum.GetValues(typeof<WGApi.Nations>) |> Seq.cast<WGApi.Nations> 
            |> Seq.map(fun t -> Nation((int)t, (string)t, data.Tanks.Count(fun s -> s.Nation= t),
                                       data.Tanks.Where(fun m -> m.Nation = t).Sum(fun r -> r.ThreeMoeCount))) |> Seq.toArray |> tableTemplate
    ] |> layout "Nation"

        
let nationHandler (id : int) (params :  TableParams) =
    htmlView (nationPage id params)