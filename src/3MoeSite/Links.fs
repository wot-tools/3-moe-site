module _3MoeSite.Links

    let about = "/about"
    let flagIconsSource = "http://www.famfamfam.com/lab/icons/flags/"

    let playerIndex = "/players"
    let playerPage id = (sprintf "/player/%i" id)
    let playerProfileWG id = (sprintf "https://worldoftanks.eu/en/community/accounts/%i" id)
    let playerProfileWotlabs name = (sprintf "http://wotlabs.net/eu/player/%s" name)

    let clanIndex = "/clans"
    let clanPage id = (sprintf "/clan/%i" id)
    let clanProfileWG id = (sprintf "https://eu.wargaming.net/clans/wot/%i" id)
    let clanProfileWotlabs tag = (sprintf "https://wotlabs.net/eu/clan/%s" tag)

    let markIndex = "/marks"

    let tankIndex = "/tanks"
    let tankPage id = (sprintf "/tank/%i" id)

    let nationIndex = "/nations"
    let nationPage id = (sprintf "/nation/%i" id)
    let nationFlag id = (sprintf "https://eu-wotp.wgcdn.co/static/5.32.1_0d6e8f/wotp_static/img/core/frontend/scss/common/components/icons/img/filter-%s.png" id)

    let tierIndex = "/tiers"
    let tierPage id = (sprintf "/tier/%i" id)

    let vehicleTypeIndex = "/types"
    let vehicleTypePage id = (sprintf "/tier/%i" id)

    let statsIndex = "/stats"

    let clientLanguageFlag id = (sprintf "/img/flags/%s.png" id)

    let threadWoTForum = "http://forum.worldoftanks.eu/index.php?/topic/524379-marks-of-excellence-data-a-new-metric-has-been-born-190117-dank-marks-can-melt-steel-beams/"
    let threadWotlabsForum = "http://forum.wotlabs.net/index.php?/topic/22352-marks-of-excellence-data-a-new-metric-has-been-born-251216-last-christmas-i-gave-you-my-marks/"