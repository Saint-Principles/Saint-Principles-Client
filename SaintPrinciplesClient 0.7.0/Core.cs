using Il2Cpp;
using Il2CppCodeStage.AntiCheat.Storage;
using MelonLoader;
using System.Collections;
using UnityEngine;
using static Il2Cpp.UpdaterV2;

[assembly: MelonInfo(typeof(Saint_Principles_Client.Core), "Saint Principles Client", "1.0.0", "Master", null)]
[assembly: MelonColor(0, 255, 0, 0)]

namespace Saint_Principles_Client
{
    public class Core : MelonMod
    {
        private const string SERVERS_FILE_PATH = "UserData/SPO_Servers.txt";
        private const string DEFAULT_SERVER_NAME = "SAINT PRINCIPLES";
        private const string DEFAULT_SERVER_IP = "e1d06195-fe86-4bd2-9738-2d02cc763b3f";
        private const float SERVER_ADD_DELAY = 0.1f;

        private static Il2CppSystem.Collections.Generic.List<serverInfo> ServerList
        {
            get
            {
                var updater = UnityEngine.Object.FindObjectOfType<UpdaterV2>();
                return updater?.OJKJNHPHFFO;
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != "MainMenu" && sceneName != "Updater") return;

            UpdatePlayerNickname();
            InitializeServerConfiguration();
        }

        private static void UpdatePlayerNickname()
        {
            string nickname = $"{ObscuredPrefs.GetString("ZWName0001")}|{ObscuredPrefs.GetString("PlayerType0001")}";

            if (PhotonNetwork.networkingPeer == null) return;

            PhotonNetwork.networkingPeer.PlayerName = nickname;
            PhotonNetwork.networkingPeer.playername = nickname;
        }

        private static void InitializeServerConfiguration() => MelonCoroutines.Start(ConfigureServers());

        private static IEnumerator ConfigureServers()
        {
            if (ServerList == null)
            {
                MelonLogger.Error("Server list is not available!");
                yield break;
            }

            ServerList.Clear();

            var serverConfigs = LoadServerConfigurations();

            yield return AddServersFromConfig(serverConfigs);

            LogServerConfigurationSummary(serverConfigs.Count);
        }

        private static IEnumerator AddServer(string serverName, string serverIP)
        {
            var serverInfo = CreateServerInfo(serverName, serverIP);

            if (!ServerList.Contains(serverInfo))
            {
                ServerList.Add(serverInfo);
                MelonLogger.Msg($"Added server: {serverName}");
            }
            else MelonLogger.Warning($"Server already exists: {serverName}");

            yield return null;
        }

        private static serverInfo CreateServerInfo(string name, string ip) => new()
        {
            cloudRegion = 0,
            serverType = "APP",
            serverName = name,
            serverIP = ip
        };

        private static List<ServerConfig> LoadServerConfigurations()
        {
            var servers = new List<ServerConfig>();

            if (!File.Exists(SERVERS_FILE_PATH))
            {
                CreateDefaultServersFile();
                return servers;
            }

            var lines = File.ReadAllLines(SERVERS_FILE_PATH);

            foreach (var line in lines)
            {
                var config = ParseServerConfigLine(line);
                if (config != null) servers.Add(config);
            }

            return servers;
        }

        private static ServerConfig ParseServerConfigLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line) ||
                line.StartsWith("#") ||
                line.StartsWith("//"))
                return null;

            var parts = line.Split('|');
            if (parts.Length < 2)
            {
                MelonLogger.Warning($"Invalid line format in servers file: {line}");
                return null;
            }

            return new ServerConfig
            {
                ServerName = parts[0].Trim(),
                ServerIP = parts[1].Trim()
            };
        }

        private static void CreateDefaultServersFile()
        {
            var directory = Path.GetDirectoryName(SERVERS_FILE_PATH);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var defaultContent =
                "# [SPO Client Config]\n" +
                "# Format: ServerName|ServerIP\n" +
                "# Comments start with # or //\n" +
                "# Examples:\n" +
                $"{DEFAULT_SERVER_NAME}|{DEFAULT_SERVER_IP}";

            File.WriteAllText(SERVERS_FILE_PATH, defaultContent);
            MelonLogger.Msg($"Created default servers file at: {SERVERS_FILE_PATH}");
        }

        private static IEnumerator AddServersFromConfig(List<ServerConfig> serverConfigs)
        {
            if (serverConfigs.Count == 0)
            {
                MelonCoroutines.Start(AddServer(DEFAULT_SERVER_NAME, DEFAULT_SERVER_IP));
                yield break;
            }

            foreach (var config in serverConfigs)
            {
                MelonCoroutines.Start(AddServer(config.ServerName, config.ServerIP));
                yield return new WaitForSeconds(SERVER_ADD_DELAY);
            }
        }

        private static void LogServerConfigurationSummary(int serverCount)
        {
            MelonLogger.Msg($"Server configuration complete. Total servers loaded: {serverCount}");
        }

        private class ServerConfig
        {
            public string ServerName { get; set; }
            public string ServerIP { get; set; }
        }
    }
}