[<AutoOpen>]
module _3MoeSite.Views.Css

let getWn8CssClass (wn8 : float) =
    match wn8 with
    | _ when wn8 < 300.0 -> "cVeryBad"
    | _ when wn8 < 450.0 -> "cBad"
    | _ when wn8 < 650.0 -> "cBelowAverage"
    | _ when wn8 < 900.0 -> "cAverage"
    | _ when wn8 < 1200.0 -> "cAboveAverage"
    | _ when wn8 < 1600.0 -> "cGood"
    | _ when wn8 < 2000.0 -> "cVeryGood"
    | _ when wn8 < 2450.0 -> "cGreat"
    | _ when wn8 < 2900.0 -> "cUnicum"
    | _ -> "cSuperUnicum"

let getWinratioCssClass (winratio : decimal) =
    match winratio with
    | _ when winratio < 0.46m -> "cVeryBad"
    | _ when winratio < 0.47m -> "cBad"
    | _ when winratio < 0.48m -> "cBelowAverage"
    | _ when winratio < 0.50m -> "cAverage"
    | _ when winratio < 0.52m -> "cAboveAverage"
    | _ when winratio < 0.54m -> "cGood"
    | _ when winratio < 0.56m -> "cVeryGood"
    | _ when winratio < 0.60m -> "cGreat"
    | _ when winratio < 0.65m -> "cUnicum"
    | _ -> "cSuperUnicum"  