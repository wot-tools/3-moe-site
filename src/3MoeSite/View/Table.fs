[<AutoOpen>]
module _3MoeSite.Views.Table
open Giraffe.GiraffeViewEngine

type 'T Column = { name : string; selector : 'T -> string }

let customTable ( columns : Column<'a> list ) ( objects : 'a list ) : XmlNode =
    table []
        ((columns |> List.map (fun c -> th [] [ a [ _href (sprintf "?name=%s&direction=%s" c.name "asc") ] [ encodedText c.name ] ]) |> tr [])
        :: (objects |> (List.map (fun o -> columns |> (List.map (fun c -> td [] [ o |> c.selector |> encodedText ])) |> tr [] ) ) ) )
