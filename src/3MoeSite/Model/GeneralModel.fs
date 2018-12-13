module _3MoeSite.Model

type Message =
    {
        Text : string
    }

type TestObject =
    {
        name : string
        age : int
        favoriteColor : string
    }

[<CLIMutable>]
type TableParams = 
    {
        sort : string
        direction : string
        page : int
    }