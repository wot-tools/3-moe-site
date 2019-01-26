[<AutoOpen>]
module _3MoeSite.Views.VehicleType
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views
open _3MoeSite

let data = WGApiDataProvider.Instance

type VehicleType (id : int, name: string, tankCount, threeMoECount) =
    member this.ID = id
    member this.Name = name
    member this.TankCount = tankCount
    member this.ThreeMoECount = threeMoECount

let vehicleTypePage (id : int) (params : TableParams) = 
    let success = Enum.IsDefined(typeof<WGApi.VehicleTypes>, id)

    match success with
    | false -> errorBlock(sprintf "Tank Type %i is not valid." id)
    | true ->        
        let tankType = enum<WGApi.VehicleTypes>(id)
        [
            headlineBlock ((string) tankType)
            data.Tanks |> Array.where(fun t -> t.VehicleType = tankType) |> tankDisplayTable params
        ] |> layout ((string)tankType)

        
let vehicleTypeTable params =
    let tableTemplate = customTable ([
        createCustomColumn "Type"       "type"      (fun p -> TableCellObject.S p.Name)
            (fun p -> a [ _href (Links.vehicleTypePage p.ID) ] [ encodedText p.Name])
        createColumn       "Tanks"      "tanks"     (fun p -> TableCellObject.I p.TankCount)
        createColumn       "3 MoE"      "3moe"      (fun p -> TableCellObject.I p.ThreeMoECount)
        ] : VehicleType Column list) params
        
    [
        headlineBlock "Vehicle types"
        Enum.GetValues(typeof<WGApi.VehicleTypes>) |> Seq.cast<WGApi.VehicleTypes> |> Seq.map(fun t -> VehicleType((int)t, (string)t, data.Tanks.Count(fun s -> s.VehicleType = t),
                                                     data.Tanks.Where(fun m -> m.VehicleType = t).Sum(fun r -> r.ThreeMoeCount))) |> Seq.toArray |> tableTemplate
    ] |> layout "Vehicle types"

        
let vehicleTypeHandler (id : int) (params :  TableParams) =
    htmlView (vehicleTypePage id params)