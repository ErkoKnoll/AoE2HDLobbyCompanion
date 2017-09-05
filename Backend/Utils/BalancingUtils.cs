using Backend.Global;
using Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Backend.Utils {
    public class BalancingUtils {
        public static string CalculateBalancedTeamsBasedOnRank() {
            try {
                var hasEvenNumberOfPlayers = CheckIfLobbyHasEvenNumberOfPlayers();
                if (hasEvenNumberOfPlayers != null) {
                    return hasEvenNumberOfPlayers;
                }
                Variables.Teams = new Dictionary<ulong, int>();
                var balanceCalculations = Variables.Lobby.Players.Where(p => p.SteamId > 0).Select(p => new BalanceCalculation {
                    Player = p,
                    Value = p.Rank
                });
                var bestMatch = FindBestMatch(balanceCalculations).ToList();
                var teams = "";
                var players = bestMatch.Select(bc => bc.Player).ToList();
                foreach (var balanceCalculation in balanceCalculations) {
                    var team = (players.IndexOf(balanceCalculation.Player) < balanceCalculations.Count() / 2) ? 1 : 2;
                    Variables.Teams[balanceCalculation.Player.SteamId] = team;
                    teams += team.ToString();
                }
                var team1 = bestMatch.Take(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team2 = bestMatch.Skip(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team1RankSum = team1.Sum(bc => bc.Value);
                var team2RankSum = team2.Sum(bc => bc.Value);
                var output = "Balanced Teams (Rank): " + teams + " | Team 1: " + (team1RankSum / (balanceCalculations.Count() / 2)) + " - Team 2: " + (team2RankSum / (balanceCalculations.Count() / 2)) + " | Calculated using Lobby Companion [aoe2lc.net]";
                LogUtils.Info(output);
                CopyToClipboard(output);
                return null;
            } catch (Exception e) {
                LogUtils.Error("Error while calculating balanced teams", e);
                return "Error while calculating balanced teams, check the logs.";
            }
        }

        public static string CalculateBalancedTeamsBasedOnTotalGames() {
            try {
                var hasEvenNumberOfPlayers = CheckIfLobbyHasEvenNumberOfPlayers();
                if (hasEvenNumberOfPlayers != null) {
                    return hasEvenNumberOfPlayers;
                }
                var memberHasPrivateProfile = CheckIfAnyLobbyMemberHasPrivateProfile();
                if (memberHasPrivateProfile != null) {
                    return memberHasPrivateProfile;
                }
                Variables.Teams = new Dictionary<ulong, int>();
                var balanceCalculations = Variables.Lobby.Players.Where(p => p.SteamId > 0).Select(p => new BalanceCalculation {
                    Player = p,
                    Value = Variables.Lobby.Ranked != 2 ? p.GameStats.TotalGamesRM : p.GameStats.TotalGamesDM
                });
                var bestMatch = FindBestMatch(balanceCalculations).ToList();
                var teams = "";
                var players = bestMatch.Select(bc => bc.Player).ToList();
                foreach (var balanceCalculation in balanceCalculations) {
                    var team = (players.IndexOf(balanceCalculation.Player) < balanceCalculations.Count() / 2) ? 1 : 2;
                    Variables.Teams[balanceCalculation.Player.SteamId] = team;
                    teams += team.ToString();
                }
                var team1 = bestMatch.Take(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team2 = bestMatch.Skip(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team1RankSum = team1.Sum(bc => bc.Value);
                var team2RankSum = team2.Sum(bc => bc.Value);
                var output = "Balanced Teams (Total Games): " + teams + " | Team 1: " + (team1RankSum / (balanceCalculations.Count() / 2)) + " - Team 2: " + (team2RankSum / (balanceCalculations.Count() / 2)) + " | Calculated using Lobby Companion [aoe2lc.net]";
                LogUtils.Info(output);
                CopyToClipboard(output);
                return null;
            } catch (Exception e) {
                LogUtils.Error("Error while calculating balanced teams", e);
                return "Error while calculating balanced teams, check the logs.";
            }
        }

        public static string CalculateBalancedTeamsBasedOnWinRatio() {
            try {
                var hasEvenNumberOfPlayers = CheckIfLobbyHasEvenNumberOfPlayers();
                if (hasEvenNumberOfPlayers != null) {
                    return hasEvenNumberOfPlayers;
                }
                var memberHasPrivateProfile = CheckIfAnyLobbyMemberHasPrivateProfile();
                if (memberHasPrivateProfile != null) {
                    return memberHasPrivateProfile;
                }
                Variables.Teams = new Dictionary<ulong, int>();
                var balanceCalculations = Variables.Lobby.Players.Where(p => p.SteamId > 0).Select(p => new BalanceCalculation {
                    Player = p,
                    Value = Variables.Lobby.Ranked != 2 ? p.GameStats.WinRatioRM : p.GameStats.WinRatioDM
                });
                var bestMatch = FindBestMatch(balanceCalculations).ToList();
                var teams = "";
                var players = bestMatch.Select(bc => bc.Player).ToList();
                foreach (var balanceCalculation in balanceCalculations) {
                    var team = (players.IndexOf(balanceCalculation.Player) < balanceCalculations.Count() / 2) ? 1 : 2;
                    Variables.Teams[balanceCalculation.Player.SteamId] = team;
                    teams += team.ToString();
                }
                var team1 = bestMatch.Take(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team2 = bestMatch.Skip(balanceCalculations.Count() / 2).OrderByDescending(p => p.Value);
                var team1RankSum = team1.Sum(bc => bc.Value);
                var team2RankSum = team2.Sum(bc => bc.Value);
                var output = "Balanced Teams (Win Ratio): " + teams + " | Team 1: " + (team1RankSum / (balanceCalculations.Count() / 2)) + "% - Team 2: " + (team2RankSum / (balanceCalculations.Count() / 2)) + "% | Calculated using Lobby Companion [aoe2lc.net]";
                LogUtils.Info(output);
                CopyToClipboard(output);
                return null;
            } catch (Exception e) {
                LogUtils.Error("Error while calculating balanced teams", e);
                return "Error while calculating balanced teams, check the logs.";
            }
        }

        public static string CopyPlayerStats() {
            try {
                var players = Variables.Lobby.Players.Where(p => p.Profile != null && p.GameStats != null && !(p.Profile.ProfilePrivate && !p.Profile.ProfileDataFetched.HasValue));
                var output = "[aoe2lc.net] Stats (Total Games/Win Ratio/Drop Ratio): " + string.Join(", ", players.Select(p => p.Name + "(" + (Variables.Lobby.Ranked != 2 ? p.GameStats.TotalGamesRM + "/" + p.GameStats.WinRatioRM + "%/" + p.GameStats.DropRatioRM + "%" : p.GameStats.TotalGamesDM + "/" + p.GameStats.WinRatioDM + "%/" + p.GameStats.DropRatioDM + "%") + ")"));
                LogUtils.Info(output);
                CopyToClipboard(output);
                return null;
            } catch (Exception e) {
                LogUtils.Error("Error while copying player stats", e);
                return "Error while copying player stats, check the logs.";
            }
        }

        private static void CopyToClipboard(string text) {
            Thread thread = new Thread(() => Clipboard.SetText(text));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private static string CheckIfLobbyHasEvenNumberOfPlayers() {
            if (Variables.Lobby.Players.Count(p => p.Rank > 0 && p.SteamId > 0) < 2) {
                return "Lobby must have at least 2 players for balance calculation.";
            } else {
                if (Variables.Lobby.Players.Count(p => p.Rank > 0 && p.SteamId > 0) % 2 == 0) {
                    return null;
                } else {
                    return "Lobby must have even number of players for balance calculation.";
                }
            }
        }

        private static string CheckIfAnyLobbyMemberHasPrivateProfile() {
            if (Variables.Lobby.Players.Any(p => p.Rank > 0 && (p.Profile == null || p.GameStats == null || (p.Profile.ProfilePrivate && !p.Profile.ProfileDataFetched.HasValue)))) {
                return "Either some player stats are not fetched yet or some player's profile is private.";
            } else {
                return null;
            }
        }

        private static IEnumerable<BalanceCalculation> FindBestMatch(IEnumerable<BalanceCalculation> players) {
            IEnumerable<BalanceCalculation> bestMatch = null;
            var bestLowestDifference = 9999;
            var totalPlayers = players.Count();
            var playersDictionary = players.ToDictionary(p => p.Player.SteamId);
            var permutations = GetPermutations(players.Select(p => p.Player.SteamId), totalPlayers).ToList();
            foreach (var permutation in permutations) {
                var team1Score = 0;
                var team2Score = 0;
                var index = 0;
                var permutationList = new List<BalanceCalculation>();
                foreach (var permutationValue in permutation) {
                    var player = playersDictionary[permutationValue];
                    permutationList.Add(player);
                    if (index < totalPlayers / 2) {
                        team1Score += player.Value;
                    } else {
                        team2Score += player.Value;
                    }
                    index++;
                }
                var difference = Math.Abs(team1Score - team2Score);
                if (difference < bestLowestDifference) {
                    bestLowestDifference = difference;
                    bestMatch = permutationList;
                }
            }
            return bestMatch;
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length) {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }
    }
}
