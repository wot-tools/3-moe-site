[<AutoOpen>]
module _3MoeSite.Views.Table
open Giraffe.GiraffeViewEngine
open _3MoeSite.Model
open System
open System.Linq
open WGApiDataProvider

[<StructuralEquality>]
[<StructuralComparison>]
type TableCellObject =
    | S of string
    | I of int
    | M of decimal
    | D of double
    | T of DateTime
    | E of Enum
    | Img of string
    | Perc of decimal
    | Flag of string

let printCellObject (o : TableCellObject) =
    match o with
    | S s -> encodedText s
    | I i -> encodedText (System.String.Format("{0:N0}", i))
    | M m -> encodedText (string m)
    | D d -> encodedText (System.String.Format("{0:N0}", d))
    | T t -> encodedText (string t)
    | E e -> encodedText (string e)
    | Img i -> img [ _src i]
    | Perc p -> encodedText (System.String.Format("{0:P2}", p))
    | Flag f -> img [ _src (sprintf "/img/flags/%s.png" f)
                      _class "flagIcon" ]

type 'T Column =
    {
        name : string
        sortName : string
        selector : 'T -> TableCellObject
        customContent : Option<'T -> XmlNode>
    }

let createColumn name sortName selector =
    { name = name; sortName = sortName; selector = selector; customContent = None }

let createCustomColumn name sortName selector customContent =
    { name = name; sortName = sortName; selector = selector; customContent = Some customContent }

let pageSize = 25

let _compare prop objs =
    let o1, o2 = objs 
    compare (prop o1) (prop o2)

let __compare columns params (o1 : 'T) o2 = 
    let objs = match params.direction with
                | "asc" -> (o1, o2)
                | "desc" -> (o2, o1)
                | _ -> (o1, o2)

    let colComparer () =
        let col = columns |> List.find (fun (l : 'T Column) -> l.sortName = params.sort)
        col.selector

    _compare (fun o -> colComparer() o ) objs

let sortAndFilter columns params (objs : 'a[]) =
    let skip = min ((params.page - 1) * pageSize) objs.Length
    let take = min (objs.Length - skip) pageSize
    objs
    |> Array.sortWith (__compare columns params)
    |> Array.skip skip
    |> Array.take take
    |> Array.toList

let customTable ( columns : Column<'a> list ) ( params : TableParams ) ( objects : 'a[] ) : XmlNode =
    let linkParamTemplate = sprintf "?sort=%s&direction=%s&page=%i"

    let ascOrDesc (colName, currentSortCol, currentOrder) =
        if colName = currentSortCol
        then match currentOrder with
             | "asc" -> ("desc", "▲")
             | "desc" -> ("asc", "▼")
        else ("asc", "")

    

    let createHeaderCol c =
        let (dir, s) = ascOrDesc (c.sortName, params.sort, params.direction)
        th [] [ a [ _href (linkParamTemplate c.sortName dir params.page) ] [ encodedText (c.name + s) ] ]

    let header() =
        thead [] [
            tr []
                (columns |> List.map createHeaderCol)
        ]

    let pagination() =
        let maxPage = double objects.Length / double pageSize |> Math.Ceiling |> int
        let pages current = [(max 1 (current - 5)) .. min (current + 5) maxPage]
        let selectablePages() = List.append (1 :: pages params.page) [maxPage] |> List.distinct

        match (maxPage) with
        | 1 -> div [] []
        | _ -> div [ _class "paginationContainer" ] [
            (if params.page > 1 then
                a [ _class "paginationFirst"
                    _href (linkParamTemplate params.sort params.direction 1) ] [
                    encodedText "Frist" ]
            else
                span [ _class "paginationFirst paginationDisabled" ] [ encodedText "First" ])

            (if params.page > 1 then
                a [ _class "paginationPreviousPage"
                    _href (linkParamTemplate params.sort params.direction (params.page - 1)) ] [
                encodedText "←" ]
            else
                span [ _class "paginationPreviousPage paginationDisabled" ] [ encodedText "←" ])
            

            div [ _class "paginationPages" ] ((pages params.page) |> List.map (fun i -> a [ _href (linkParamTemplate params.sort params.direction i)
                                                                                            _class (sprintf "pagionationPageSelect %s" (if i = params.page then "paginationCurrent" else "")) ] [ 
                                                                                                i |> sprintf "%i" |> encodedText ]))

            (if params.page < maxPage then
                a [ _class "paginationNextPage"
                    _href (linkParamTemplate params.sort params.direction (params.page + 1)) ] [
                encodedText "→" ]
            else
                span [ _class "paginationNextPage paginationDisabled" ] [ encodedText "→" ])

            (if params.page < maxPage then
                a [ _class "paginationLast"
                    _href (linkParamTemplate params.sort params.direction maxPage) ] [
                encodedText "Last" ]
            else
                span [ _class "paginationLast paginationDisabled" ] [ encodedText "Last" ])
            
            
        ]
        
    let body() =
        tbody []
            (sortAndFilter columns params objects |> List.map 
                (fun o -> columns |> List.map (fun c ->
                    [(match c.customContent with
                     | None -> o |> c.selector |> printCellObject
                     | Some f -> f o)] |> td []) |> tr []))

    div [] [
        pagination()
        table [] [
            header()
            body()
        ]
        pagination()
    ]
