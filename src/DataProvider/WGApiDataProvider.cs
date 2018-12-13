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

        private static object InstanceLock = new object();
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

        private WGApiDataProvider()
        {
            Mark.SetDataProvider(this);
            Player.SetDataProvider(this);

            Client = new WGApiClient("https://api.worldoftanks", Region.eu, "insert key here, cause I'm lazy", new Logger());

            LoadIDs("ids_EU.txt").Wait();
            Task.Run(() => LoadTanks(Client));
            Task.Run(UpdateLoop);

            //__Players = JsonConvert.DeserializeObject<Player[]>(File.ReadAllText("player-test.json")).ToDictionary(p => p.ID);
            //__Clans = JsonConvert.DeserializeObject<Clan[]>(File.ReadAllText("clan-test.json")).ToDictionary(c => c.ID);
        }




        public void StartAutoUpdate()
        {

        }

        public void StopAutoUpdate()
        {

        }

        private Stack<int> PlayerIDsToCheck;
        private List<int> ClanIDsToCheck = new List<int>();
        private bool DoRun = true;

        public async Task UpdateLoop()
        {
            int i = 0;
            while (true)
            {
                if (!DoRun)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    continue;
                }

                Console.WriteLine($"doing run {i}");

                var clans = ClanIDsToCheck.ToArray();
                ClanIDsToCheck.Clear();
                Task clanTask = null;
                if (clans.Any())
                    clanTask = GetClans(Client, clans);

                var playerTask = GetPlayers(Client, PlayerIDsToCheck.Skip(i * 100).Take(100));
                await Task.WhenAll(clanTask ?? playerTask, playerTask);
                i += 1;
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
            foreach (var kvp in vehicles)
                __Tanks.Add(kvp.Key, new Tank(kvp.Key, kvp.Value));
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

                foreach (var moe in moes.Where(m => m.Mark == 3))
                {
                    var newMark = new Mark(player.AccountID, player.ClanID ?? 0, moe.TankID, DateTime.Now);
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
                __Clans[clan.ID] = new Clan(clan.ID, clan.Name, clan.Tag, -1, -1, -1, clan.UpdatedAt, clan.CreatedAt, trackingStarted, DateTime.Now);
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
