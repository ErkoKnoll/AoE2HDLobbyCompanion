using Commons.Constants;
using Commons.Models;
using Process.NET;
using Process.NET.Memory;
using Process.NET.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPFTest;

namespace Overlay {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const int GwlExstyle = -20;
        private const int WsExTransparent = 0x00000020;
        private DispatcherTimer _visibilityTimer;
        private DispatcherTimer _processTimer;
        private DispatcherTimer _listUpdateTimer;
        private ProcessSharp _processSharp;
        private IWindow _processWindow;
        private Configuration _config;
        private bool _inLobby = false;
        private bool _sessionRunning = false;
        private HotKey _hotKeyCopyPlayerStats;
        private HotKey _hotKeyCalculateTeamsRank;
        private HotKey _hotKeyCalculateTeamsTotalGames;
        private HotKey _hotKeyCalculateTeamsWinRatio;
        private Action _copyPlayerStats;
        private Action _calculateTeamsRank;
        private Action _calculateTeamsTotalGames;
        private Action _calculateTeamsWinRatio;

        public MainWindow() {
            InitializeComponent();
            StartVisibilityTimer();
            StartProcessTimer();
            _hotKeyCopyPlayerStats = new HotKey(Key.D1, KeyModifier.Ctrl, OnHotKeyCopyPlayerStats);
            _hotKeyCalculateTeamsRank = new HotKey(Key.D2, KeyModifier.Ctrl, OnHotKeyCalculateTeamsRank);
            _hotKeyCalculateTeamsTotalGames = new HotKey(Key.D3, KeyModifier.Ctrl, OnHotKeyCalculateTeamsTotalGames);
            _hotKeyCalculateTeamsWinRatio = new HotKey(Key.D4, KeyModifier.Ctrl, OnHotKeyCalculateTeamsWinRatio);
        }

        public void RegisterHotKeyHooks(Action copyPlayerStats, Action calculateTeamsRank, Action calculateTeamsTotalGames, Action calculateTeamsWinRatio) {
            _copyPlayerStats = copyPlayerStats;
            _calculateTeamsRank = calculateTeamsRank;
            _calculateTeamsTotalGames = calculateTeamsTotalGames;
            _calculateTeamsWinRatio = calculateTeamsWinRatio;
        }

        public void UpdateConfiguration(Configuration configuration, bool sessionRunning) {
            try {
                if (configuration != null) {
                    _config = configuration;
                    _sessionRunning = sessionRunning;
                    UpdateVisibility();
                }
            } catch { }
        }

        public void UpdateVisibility() {
            try {
                bool show = true;
                if (_config == null || !_config.ShowOverlay) {
                    show = false;
                } else {
                    if (_config.ShowOverlayWhenInLobby) {
                        show = _inLobby;
                    }
                    if (show == true) {
                        if (_config.ShowOverlayWhenFocused && (_processWindow == null || !_processWindow.IsActivated)) {
                            show = false;
                        }
                    }
                    if (show == true && _sessionRunning == false) {
                        show = false;
                    }
                }
                if (Visibility == Visibility.Visible && show == false) {
                    Application.Current.Dispatcher.Invoke(() => Hide());
                } else if (Visibility == Visibility.Hidden && show == true) {
                    Application.Current.Dispatcher.Invoke(() => Show());
                }
                UpdatePosition();
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                try {
                    if (_config != null && _config.ShowOverlay) {
                        Show();
                    }
                } catch { }
            }
        }

        public void UpdateLobbyPresence(bool inLobby) {
            _inLobby = inLobby;
            UpdateVisibility();
        }

        public void UpdatePlayers(List<Player> players, Dictionary<ulong, int> teams, bool dm) {
            try {
                if (players == null) {
                    players = new List<Player>();
                }
                int index = 1;
                foreach (var player in players) {
                    Application.Current.Dispatcher.Invoke(() => {
                        try {
                            var totalGamesLabel = ((Label)GridContainer.FindName("Label" + index + "PlayerTotalGames"));
                            var dropsLabel = ((Label)GridContainer.FindName("Label" + index + "PlayerDrops"));
                            var winsLabel = ((Label)GridContainer.FindName("Label" + index + "PlayerWins"));
                            var locationLabel = ((Label)GridContainer.FindName("Label" + index + "PlayerLocation"));
                            var gamesLabel = ((Label)GridContainer.FindName("Label" + index + "PlayerGames"));
                            var badRepLabel = ((Label)GridContainer.FindName("Label" + index + "PlayerBadRep"));
                            var goodRepLabel = ((Label)GridContainer.FindName("Label" + index + "PlayerGoodRep"));
                            var teamLabel = ((Label)GridContainer.FindName("Label" + index + "PlayerTeam"));
                            var rank = "";
                            ((Label)GridContainer.FindName("Label" + index + "PlayerName")).Content = player.Name;
                            if (player.Rank > 0) {
                                rank = player.Rank.ToString();
                            }
                            ((Label)GridContainer.FindName("Label" + index + "PlayerRank")).Content = rank;
                            if (player.GameStats != null && player.Profile != null) {
                                int totalGames = !dm ? player.GameStats.TotalGamesRM : player.GameStats.TotalGamesDM;
                                int winRatio = !dm ? player.GameStats.WinRatioRM : player.GameStats.WinRatioDM;
                                int dropRatio = !dm ? player.GameStats.DropRatioRM : player.GameStats.DropRatioDM;
                                var games = "";
                                var wins = "";
                                var drops = "";
                                if (player.Profile.ProfilePrivate) {
                                    if (player.Profile.ProfileDataFetched.HasValue) {
                                        games = "[" + totalGames.ToString() + "]";
                                        wins = "[" + winRatio + "%" + "]";
                                        drops = "[" + dropRatio + "%" + "]";
                                    } else {
                                        games = "PRIVATE ACC";
                                    }
                                } else {
                                    games = totalGames.ToString();
                                    wins = winRatio + "%";
                                    drops = dropRatio + "%";
                                }
                                totalGamesLabel.Content = games;
                                winsLabel.Content = wins;
                                dropsLabel.Content = drops;
                            } else {
                                totalGamesLabel.Content = "";
                                winsLabel.Content = "";
                                dropsLabel.Content = "";
                            }
                            if (player.Profile != null && player.Profile.Location != null) {
                                locationLabel.Content = player.Profile.Location;
                            } else {
                                locationLabel.Content = "";
                            }
                            if (teams.TryGetValue(player.SteamId, out int value)) {
                                if (value > 0) {
                                    teamLabel.Content = value;
                                } else {
                                    teamLabel.Content = "";
                                }
                            } else {
                                teamLabel.Content = "";
                            }
                            if (player.ReputationStats != null) {
                                gamesLabel.Content = player.ReputationStats.Games;
                                badRepLabel.Content = player.ReputationStats.NegativeReputation;
                                goodRepLabel.Content = player.ReputationStats.PositiveReputation;
                            } else {
                                gamesLabel.Content = "";
                                badRepLabel.Content = "";
                                goodRepLabel.Content = "";
                            }
                            UpdateFieldColor(gamesLabel, player, PlayerFields.GAMES);
                            UpdateFieldColor(goodRepLabel, player, PlayerFields.POSITIVE_REPUTATION);
                            UpdateFieldColor(badRepLabel, player, PlayerFields.NEGATIVE_REPUTATION);
                            UpdateFieldColor(totalGamesLabel, player, PlayerFields.TOTAL_GAMES);
                            UpdateFieldColor(winsLabel, player, PlayerFields.WIN_RATIO);
                            UpdateFieldColor(dropsLabel, player, PlayerFields.DROP_RATIO);
                            index++;
                        } catch (Exception e) {
                            Console.WriteLine("Error while updating overlay slot: " + index);
                            Console.WriteLine(e.ToString());
                        }
                    });
                }
            } catch (Exception e) {
                Console.WriteLine("Error while updating overlay list");
                Console.WriteLine(e.ToString());
            }
        }

        public void ShowMessage(string message, bool oneLiner = false) {
            Application.Current.Dispatcher.Invoke(() => {
                OverlayMessage.Text = message;
                Height = oneLiner ? 225 : 245;
            });
        }

        public void HideMessage() {
            Application.Current.Dispatcher.Invoke(() => Height = 200);
        }

        private void UpdatePosition() {
            var top = this._config.OverlayInactiveTopPosition;
            var left = this._config.OverlayInactiveLeftPosition;
            if (_processWindow != null && _processWindow.IsActivated) {
                top = this._config.OverlayActiveTopPosition;
                left = this._config.OverlayActiveLeftPosition;
            }
            if (Top != top || Left != left) {
                Application.Current.Dispatcher.Invoke(() => {
                    Top = top;
                    Left = left;
                });
            }
        }

        private void UpdateFieldColor(Label label, Player player, string fieldName) {
            switch (player.FieldColors[fieldName]) {
                case (int)PlayerFieldColors.NONE:
                    label.Background = null;
                    break;
                case (int)PlayerFieldColors.SAFE:
                    label.Background = SafeColor;
                    break;
                case (int)PlayerFieldColors.DANGER:
                    label.Background = DangerColor;
                    break;
            }
        }

        private SolidColorBrush DangerColor {
            get { return new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)); }
        }

        private SolidColorBrush SafeColor {
            get { return new SolidColorBrush(Color.FromArgb(50, 0, 255, 0)); }
        }

        private void StartListUpdateTimer() {
            _listUpdateTimer = new DispatcherTimer();
            _listUpdateTimer.Tick += (sender, args) => {

            };
            _listUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _listUpdateTimer.Start();
        }

        private void StartVisibilityTimer() {
            _visibilityTimer = new DispatcherTimer();
            _visibilityTimer.Tick += (sender, args) => {
                UpdateVisibility();
            };
            _visibilityTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _visibilityTimer.Start();
        }

        private void StartProcessTimer() {
            _processTimer = new DispatcherTimer();
            _processTimer.Tick += (sender, args) => {
                try {
                    var process = System.Diagnostics.Process.GetProcessesByName("AoK HD").FirstOrDefault();
                    if (process != null) {
                        _processSharp = new ProcessSharp(process, MemoryType.Remote);
                        _processWindow = _processSharp.WindowFactory.MainWindow;
                    } else {
                        _processWindow = null;
                    }
                } catch {
                    _processWindow = null;
                }
            };
            _processTimer.Interval = new TimeSpan(0, 0, 1);
            _processTimer.Start();
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            MakeWindowTransparent();
        }

        private void MakeWindowTransparent() {
            var hwnd = new WindowInteropHelper(this).Handle;
            var extendedStyle = Native.GetWindowLongPtr(hwnd, GwlExstyle);
            Native.SetWindowLongPtr(hwnd, GwlExstyle, new IntPtr(extendedStyle.ToInt32() | WsExTransparent));
        }

        private void OnHotKeyCopyPlayerStats(HotKey hotKey) {
            _copyPlayerStats?.Invoke();
        }

        private void OnHotKeyCalculateTeamsRank(HotKey hotKey) {
            _calculateTeamsRank?.Invoke();
        }

        private void OnHotKeyCalculateTeamsTotalGames(HotKey hotKey) {
            _calculateTeamsTotalGames?.Invoke();
        }

        private void OnHotKeyCalculateTeamsWinRatio(HotKey hotKey) {
            _calculateTeamsWinRatio?.Invoke();
        }
    }
}
