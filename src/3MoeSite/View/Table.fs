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

let printCellObject (o : TableCellObject) =
    match o with
    | S s -> encodedText s
    | I i -> encodedText (string i)
    | M m -> encodedText (string m)
    | D d -> encodedText (string d)
    | T t -> encodedText (string t)
    | E e -> encodedText (string e)
    | Img i -> img [ _src i]
    | Perc p -> encodedText (System.String.Format("{0:P2}", p))

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

    let footer() =
        let maxPage = double objects.Length / double pageSize |> Math.Ceiling |> int
        let pages current = [(max 1 (current - 5)) .. min (current + 5) maxPage]
        let selectablePages() = List.append (1 :: pages params.page) [maxPage] |> List.distinct

        tfoot [] [
            tr [] [
                td [ _colspan "100%" ] // crossbrowser way of spanning all columns
                    (selectablePages() |> List.map (fun i -> a [ _href (linkParamTemplate params.sort params.direction i)
                                                                 _class "pageSelect"] [ i |> sprintf "%i" |> encodedText ]) )
            ]
        ]
        
    let body() =
        tbody []
            (sortAndFilter columns params objects |> List.map 
                (fun o -> columns |> List.map (fun c ->
                    [(match c.customContent with
                     | None -> o |> c.selector |> printCellObject
                     | Some f -> f o)] |> td []) |> tr []))

    table [] [
        header()
        footer()
        body()
    ]
