using Newtonsoft.Json.Linq;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Auto Teams", "Leon", "1.0.0")]
    [Description("Automatically adds people to teams")]
    class AutoTeams : CovalencePlugin
    {
        #region Definitions

        string[] defaultTeamNames = new string[] { "alpha", "bravo", "charlie", "delta", "echo", "foxtrot", "golf", "hotel" };

        #endregion Definitions

        void SaveConfig(Configuration config) => Config.WriteObject(config, true);

        private class Configuration {
            public Dictionary<string, ulong> teams = new Dictionary<string, ulong>();
        }
        
        protected override void LoadDefaultConfig() {
            var config = new Configuration();
            LogWarning("Creating a new configuration file for AutoTeams");

            foreach(var name in defaultTeamNames) {
                var team = RelationshipManager.Instance.CreateTeam();
                config.teams.Add(name, team.teamID);
            }

            SaveConfig(config);
        }     

        RelationshipManager.PlayerTeam getTeamOrCreate(string name) {
            var data = Config.ReadObject<Configuration>();

            if (!data.teams.ContainsKey(name)) {
                var _team = RelationshipManager.Instance.CreateTeam();
                data.teams.Add(name, _team.teamID);
                SaveConfig(data);
                return _team;
            } else {
                RelationshipManager.PlayerTeam team = RelationshipManager.Instance.FindTeam(data.teams[name]);
                
                if (team != null) {
                    return team;
                }

                var _team = RelationshipManager.Instance.CreateTeam();
                data.teams[name] = _team.teamID;
                SaveConfig(data);
                return _team;
            }
        }

        Dictionary<string, RelationshipManager.PlayerTeam> getTeams() {
            var output = new Dictionary<string, RelationshipManager.PlayerTeam>();

            foreach(var name in defaultTeamNames) {
                var team = getTeamOrCreate(name);
                output.Add(name, team);
            }

            return output;
        }

        [Command("myteam")]
        private void myclanCmd(IPlayer player, string command, string[] args)
        {
            var name = _getTeamNameFor(player);
            player.Reply($"Your team name is: {name}");
        }
        
        [HookMethod("getTeamNameFor")]
        public string getTeamNameFor(string Id) {
            var player = players.FindPlayer(Id);
            var name = _getTeamNameFor(player);
            Puts($"Got player: {player.Id} team name: {name}");
            return name;
        }


        [HookMethod("getAllTeamsCount")]
        public Dictionary<string, string> getAllTeamsCount() {
            var output = new Dictionary<string, string>();

            foreach(var name in defaultTeamNames) {
                var count = getTeamOrCreate(name).members.Count;
                output.Add(name, $"{count}/6");
            }

            return output;
        }

        [HookMethod("isTeamMate")]
        public bool isTeamMate(string player1, string player2) {
            var _player1 = players.FindPlayer(player1).Object as BasePlayer;
            var _player2 = players.FindPlayer(player2).Object as BasePlayer;

            return _player1.currentTeam == _player2.currentTeam;
        }

        [HookMethod("getTeamCount")]
        public int getTeamCount(string name) {
            var count = getTeamOrCreate(name).members.Count;
            Puts($"{name} team has count of {count}");
            return count;
        }

        public string _getTeamNameFor(IPlayer player) {
            var basePlayer = player.Object as BasePlayer;
            RelationshipManager.PlayerTeam currentTeam = RelationshipManager.Instance.FindTeam(basePlayer.currentTeam);

            if (currentTeam == null) {
                return "";
            }

            return getTeamName(currentTeam);
        }

        string getTeamName(RelationshipManager.PlayerTeam team) {
            var name = getTeams().FirstOrDefault(x => x.Value.teamID == team.teamID).Key;
            Puts($"Got team name: {name} param: {team.teamID}");
            return name;
        }

        void LeaveTeam(IPlayer player) {
            var basePlayer = player.Object as BasePlayer;
            RelationshipManager.PlayerTeam currentTeam = RelationshipManager.Instance.FindTeam(basePlayer.currentTeam);

            if (currentTeam != null) {
                currentTeam.RemovePlayer(basePlayer.userID);
            }
        }

        [Command("join")]
        private void joinCmd(IPlayer player, string command, string[] args)
        {
            var basePlayer = player.Object as BasePlayer;
            string name = args[0];
            JObject clanInfo;

            if (defaultTeamNames.Contains(name)) {
                LeaveTeam(player);

                timer.In(0.5f, () => {
                    var team = getTeamOrCreate(name);

                    if (team.members.Count < 6) {
                        Puts($"Player {player.Id} - joining clan: {name} ");
                        player.Reply($"You've joined team {name} total members: {team.members.Count}");
                        team.AddPlayer(basePlayer);
                        player.Kill();
                    } else {
                        Puts("Cannot join, team is full");
                        player.Reply("Team is already full");
                    }            
                });
            }
        }

        private void OnUserDisconnected(IPlayer player) {
            var basePlayer = (player.Object as BasePlayer);
            RelationshipManager.PlayerTeam team = RelationshipManager.Instance.FindTeam(basePlayer.currentTeam);

            if (team == null) {
                return;
            }

            team.RemovePlayer(basePlayer.userID);
            Puts($"Player: {player.Id} - Disconnecting.");
        }

        object OnPlayerRespawn(BasePlayer player)
        {
            Puts($"OnPlayerRespawn works: {player.userID}");

            return null;
        }

        [Command("assign")]
        void assignTeam(IPlayer player, string command, string[] args) {
            OnUserConnected(player);
        }
        
        private void OnUserConnected(IPlayer player) {
            PrintWarning($"User connected: {player.Name}");
            var basePlayer = (player.Object as BasePlayer);

            if (basePlayer.currentTeam != null) {
                Puts($"Player: {player.Id} - Connected.");
                RelationshipManager.PlayerTeam team = RelationshipManager.Instance.FindTeam(basePlayer.currentTeam);

                if (team != null) {
                    team.RemovePlayer(basePlayer.userID);
                }
            }            
            
            // Add new player to smallest team
            var teamsOrdered = getTeams().OrderBy(o=>o.Value.members.Count).ToList();
            teamsOrdered.First().Value.AddPlayer(basePlayer);
        }
    }
}
