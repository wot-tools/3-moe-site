using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGApi;

namespace WGApiDataProvider
{
    public class WGApiDataProvider : DataProvider
    {
        private readonly WGApiClient Client;

        private static readonly object InstanceLock = new object();
        private static WGApiDataProvider _Instance;
        public static WGApiDataProvider Instance
        {
            get
            {
                if (_Instance is null)
                    lock (InstanceLock)
                        if (_Instance is null)
                            _Instance = new WGApiDataProvider();
                return _Instance;
            }
        }

        private Dictionary<int, Player> __Players = new Dictionary<int, Player>();
        public override Dictionary<int, Player> _Players => __Players;
        public override Player[] Players => _Players.Values.ToArray();

        // TODO: after serialization the same marks in tanks and players will not be the same object anymore
        private Dictionary<int, Dictionary<int, Mark>> __TanksMarks = new Dictionary<int, Dictionary<int, Mark>>();
        public override Dictionary<int, Dictionary<int, Mark>> _TanksMarks => __TanksMarks;
        private Dictionary<int, Dictionary<int, Mark>> __PlayersMarks = new Dictionary<int, Dictionary<int, Mark>>();
        public override Dictionary<int, Dictionary<int, Mark>> _PlayersMarks => __PlayersMarks;
        public override Mark[] Marks => _TanksMarks.Values.SelectMany(kvp => kvp.Values).ToArray();

        private Dictionary<int, Clan> __Clans = new Dictionary<int, Clan>();
        public override Dictionary<int, Clan> _Clans => __Clans;
        public override Clan[] Clans => _Clans.Values.ToArray();

        private Dictionary<int, Tank> __Tanks = new Dictionary<int, Tank>();
        public override Dictionary<int, Tank> _Tanks => __Tanks;
        public override Tank[] Tanks => _Tanks.Values.ToArray();

        private Task RunningUpdate;
        private int nextRun;

        private WGApiDataProvider()
        {
            Mark.SetDataProvider(this);
            Player.SetDataProvider(this);
            Tank.SetDataProvider(this);

            Client = new WGApiClient("https://api.worldoftanks", Region.eu, "insert key here, cause I'm lazy", new Logger());

            var setupTasks = new List<Task>()
            {
                LoadIDs("ids_EU.txt"),
                TryLoad(),
                LoadTanks(Client),
            };

            Task.WaitAll(setupTasks.ToArray());

            StartAutoUpdate();

            //__Players = JsonConvert.DeserializeObject<Player[]>(File.ReadAllText("player-test.json")).ToDictionary(p => p.ID);
            //__Clans = JsonConvert.DeserializeObject<Clan[]>(File.ReadAllText("clan-test.json")).ToDictionary(c => c.ID);
        }




        public void StartAutoUpdate()
        {
            DoRun = true;
            Task.Run(() => LoadTanks(Client));
            RunningUpdate = Task.Run(UpdateLoop);
        }

        public async Task StopAutoUpdate()
        {
            DoRun = false;
            await RunningUpdate;
            await Save();
        }

        private const string PLAYERS_DATA_FILE = "players.json";
        private const string MARKS_DATA_FILE = "marks.json";
        private const string CLANS_DATA_FILE = "clans.json";
        private const string LAST_RUN_FILE = "nextRun.txt";

        private string CreateFilePath(string fileName) => Path.Combine("savedata", fileName);

        private async Task<bool> TryLoad()
        {
            try
            {
                await Load();
            }
            catch (DirectoryNotFoundException) { return false; }
            catch (FileNotFoundException) { return false; }
            return true;
        }

        private async Task Load()
        {
            var playersTask = File.ReadAllTextAsync(CreateFilePath(PLAYERS_DATA_FILE));
            var marksTask = File.ReadAllTextAsync(CreateFilePath(MARKS_DATA_FILE));
            var clansTask = File.ReadAllTextAsync(CreateFilePath(CLANS_DATA_FILE));
            var nextRunTask = File.ReadAllTextAsync(CreateFilePath(LAST_RUN_FILE));

            await Task.WhenAll(playersTask, marksTask, clansTask, nextRunTask);

            __Players = JsonConvert.DeserializeObject<Dictionary<int, Player>>(playersTask.Result);
            __PlayersMarks = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, Mark>>>(marksTask.Result);
            __TanksMarks = __PlayersMarks
                .SelectMany(kvp => kvp.Value.Values)
                .GroupBy(m => m.TankID)
                .ToDictionary(g => g.Key, g => g.ToDictionary(m => m.PlayerID));
            __Clans = JsonConvert.DeserializeObject<Dictionary<int, Clan>>(clansTask.Result);
            nextRun = Convert.ToInt32(nextRunTask.Result);
        }

        private async Task Save()
        {
            if (!Directory.Exists(CreateFilePath(String.Empty)))
                Directory.CreateDirectory(CreateFilePath(String.Empty));

            var playersTask = File.WriteAllTextAsync(CreateFilePath(PLAYERS_DATA_FILE), JsonConvert.SerializeObject(__Players));
            var marksTask = File.WriteAllTextAsync(CreateFilePath(MARKS_DATA_FILE), JsonConvert.SerializeObject(__PlayersMarks));
            var clansTask = File.WriteAllTextAsync(CreateFilePath(CLANS_DATA_FILE), JsonConvert.SerializeObject(__Clans));
            var nextRunTask = File.WriteAllTextAsync(CreateFilePath(LAST_RUN_FILE), $"{nextRun}");
            await Task.WhenAll(playersTask, marksTask, clansTask, nextRunTask);
        }

        private Stack<int> PlayerIDsToCheck;
        private List<int> ClanIDsToCheck = new List<int>();
        private bool DoRun = true;

        private async Task UpdateLoop()
        {
            int i = nextRun;
            var totalRunCount = Math.Ceiling(PlayerIDsToCheck.Count / 100.0);
            Console.WriteLine($"Starting up. Next run: {nextRun}");
            while (true)
            {
                if (i >= 5)
                    StopAutoUpdate();

                if (!DoRun)
                {
                    return;
                }

                Console.WriteLine($"{DateTime.Now}: doing run {i:N0}/{totalRunCount:N0} ({i / totalRunCount:P2})");

                var clans = ClanIDsToCheck.ToArray();
                ClanIDsToCheck.Clear();
                Task clanTask = null;
                if (clans.Any())
                    clanTask = GetClans(Client, clans);

                var playerTask = GetPlayers(Client, PlayerIDsToCheck.Skip(i * 100).Take(100));
                await Task.WhenAll(clanTask ?? playerTask, playerTask);
                if (i % 10 == 0)
                {
                    Console.WriteLine($"{DateTime.Now}: saving data");
                    await Save();
                }

                i += 1;
                nextRun = i;
            }
        }

        private async Task LoadIDs(string playerIDsPath)
        {
            var playerIDs = await File.ReadAllTextAsync(playerIDsPath);

            PlayerIDsToCheck = new Stack<int>(playerIDs.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(id => int.Parse(id)));
        }

        public async Task LoadTanks(WGApiClient client)
        {
            __Tanks = new Dictionary<int, Tank>();
            var vehicles = await client.GetVehiclesAsync();
            foreach (var kvp in vehicles.Where(x => x.Value.Tier >= 5))
                __Tanks.Add(kvp.Key, new Tank(kvp.Key, kvp.Value));

            __Tanks.Add(61713, new Tank(61713, new WGApi.Tank()
            {
                Tier = 7,
                VehicleType = VehicleTypes.TD,
                Nation = Nations.Germany,
                Name = "Krupp-Steyr Waffentr�ger",
                IsPremium = true,
                Tag = "G109_Steyr_WT",
                ShortName = "Steyr WT",
                Icons = new Icons()
                {
                    Big = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/germany-G109_Steyr_WT.png",
                    Contour = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/contour/germany-G109_Steyr_WT.png",
                    Small = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/small/germany-G109_Steyr_WT.png",
                }
            }));
            __Tanks.Add(12033, new Tank(12033, new WGApi.Tank()
            {
                Tier = 9,
                VehicleType = VehicleTypes.TD,
                Nation = Nations.USSR,
                Name = "SU-122-54",
                IsPremium = false,
                Tag = "R75_SU122_54",
                ShortName = "SU-122-54",
                Icons = new Icons()
                {
                    Big = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/ussr-R75_SU122_54.png",
                    Contour = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/contour/ussr-R75_SU122_54.png",
                    Small = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/small/ussr-R75_SU122_54.png",
                }
            }));
            __Tanks.Add(17153, new Tank(17153, new WGApi.Tank()
            {
                Tier = 10,
                VehicleType = VehicleTypes.Medium,
                Nation = Nations.USSR,
                Name = "Object 430 B",
                IsPremium = false,
                Tag = "R96_Object_430B",
                ShortName = "Obj. 430B",
                Icons = new Icons()
                {
                    Big = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/ussr-R96_Object_430B.png",
                    Contour = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/contour/ussr-R96_Object_430B.png",
                    Small = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/small/ussr-R96_Object_430B.png",
                }
            }));
            __Tanks.Add(14337, new Tank(14337, new WGApi.Tank()
            {
                Tier = 10,
                VehicleType = VehicleTypes.SPG,
                Nation = Nations.USSR,
                Name = "Object 263",
                IsPremium = false,
                Tag = "R96_Object_430",
                ShortName = "Obj. 263",
                Icons = new Icons()
                {
                    Big = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/ussr-R93_Object263.png",
                    Contour = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/contour/ussr-R93_Object263.png",
                    Small = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/small/ussr-R93_Object263.png",
                }
            }));
            __Tanks.Add(62017, new Tank(62017, new WGApi.Tank()
            {
                Tier = 8,
                VehicleType = VehicleTypes.Heavy,
                Nation = Nations.France,
                Name = "AMX M4 mle. 49 Libert�",
                IsPremium = false,
                Tag = "F74_AMX_M4_1949_Liberte",
                ShortName = "AMX M4 49 L",
                Icons = new Icons()
                {
                    Big = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/france-F74_AMX_M4_1949_Liberte.png",
                    Contour = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/contour/france-F74_AMX_M4_1949_Liberte.png",
                    Small = "http://api.worldoftanks.eu/static/2.64.0/wot/encyclopedia/vehicle/small/france-F74_AMX_M4_1949_Liberte.png",
                }
            }));
        }

        private async Task GetPlayers(WGApiClient client, IEnumerable<int> playerIDs)
        {
            var stats = await client.GetPlayerStatsAsync(playerIDs);

            foreach (var playerKvp in stats)
            {
                var player = playerKvp.Value;
                if (player is null) continue;
                player.AccountID = playerKvp.Key;

                var task = client.GetPlayerMarksAsync(player.AccountID);

                __Players[player.AccountID] = new Player(player);
                if (player.ClanID.HasValue)
                    ClanIDsToCheck.Add(player.ClanID.Value);

                var moes = await task;
                if (null == moes) continue;

                foreach (var moe in moes.Where(m => m.Mark == 3))
                {
                    var newMark = new Mark(player.AccountID, moe.TankID, DateTime.Now);
                    __TanksMarks.TryAdd(moe.TankID, new Dictionary<int, Mark>());
                    __TanksMarks[moe.TankID].TryAdd(player.AccountID, newMark);

                    __PlayersMarks.TryAdd(player.AccountID, new Dictionary<int, Mark>());
                    __PlayersMarks[player.AccountID].TryAdd(moe.TankID, newMark);
                }
            }
        }

        private async Task GetClans(WGApiClient client, IEnumerable<int> clans)
        {
            var info = await client.GetClanInformationAsync(clans);

            foreach (var clan in info.Values)
            {
                var trackingStarted = DateTime.Now;
                if (__Clans.TryGetValue(clan.ID, out Clan old))
                    trackingStarted = old.TrackingStarted;
                __Clans[clan.ID] = new Clan(clan.ID, clan.Name, clan.Tag, -1, -1, -1, clan.UpdatedAt, clan.CreatedAt, trackingStarted, DateTime.Now, clan.Emblems);
            }
        }
    }

    internal class Logger : ILogger
    {
        public void CriticalError(string message)
        {
            Print(message);
        }

        public void CriticalError(string format, params object[] args)
        {
            Print(format, args);
        }

        public void Error(string message)
        {
            Print(message);
        }

        public void Error(string format, params object[] args)
        {
            Print(format, args);
        }

        public void Info(string message)
        {
            Print(message);
        }

        public void Info(string format, params object[] args)
        {
            Print(format, args);
        }

        public void Verbose(string message)
        {
            Print(message);
        }

        public void Verbose(string format, params object[] args)
        {
            Print(format, args);
        }

        public void VVerbose(string message)
        {
            Print(message);
        }

        public void VVerbose(string format, params object[] args)
        {
            Print(format, args);
        }

        public void VVVerbose(string message)
        {
            Print(message);
        }

        public void VVVerbose(string format, params object[] args)
        {
            Print(format, args);
        }

        private void Print(string message)
        {
            Console.WriteLine(message);
        }

        private void Print(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
