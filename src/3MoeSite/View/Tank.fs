[<AutoOpen>]
module _3MoeSite.Views.Tank
open Giraffe
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider
open _3MoeSite.Views

let data = WGApiDataProvider.Instance

let tankPage id =
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
                              _src "" ]
                        a [ _href (sprintf "/nation/%s" (string tank.Nation)) ] [ encodedText (string tank.Nation) ]
                        encodedText ", "
                        a [ _href (sprintf "/tiers/%i" tank.Tier) ] [ encodedText (sprintf "%i" tank.Tier) ]
                        encodedText ", "
                        a [ _href (sprintf "/type/%s" (string tank.VehicleType)) ] [ encodedText (string tank.VehicleType) ]
                        img [ _class "imageTankType" 
                              _src "" ]
                    ]
                ]                    
            ]
        ] |> layout tank.Name

        
let tanksTable params =
    [
        headlineBlock "Tank Overview"
        data.Tanks |> tankDisplayTable params
    ] |> layout "Tanks"
        

let tankHandler (id : int) =
    htmlView (tankPage id)