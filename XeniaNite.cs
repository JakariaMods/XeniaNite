using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace Xenia
{
    public class XeniaNite : LibraryPlugin
    {
        private XeniaNiteSettingsViewModel _settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("114e4d91-7fd5-4f0c-b88c-d7584e332f8a");

        public override string Name => "Xenia Manager";

        public XeniaNite(IPlayniteAPI api) : base(api)
        {
            _settings = new XeniaNiteSettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var serializer = JsonSerializer.CreateDefault();
            string path = _settings.Settings.ExeLocation;
            int lastIndex = path.LastIndexOf('/');
            if(lastIndex == -1)
                lastIndex = path.LastIndexOf('\\');

            string config = "Config/games.json";
            string xeniaDirectory = lastIndex >= 0 ? path.Substring(0, lastIndex) : path;

            string configPath = Path.GetFullPath(Path.Combine(xeniaDirectory, config));
            var xeniaGames = serializer.Deserialize<List<GameInfo>>(new JsonTextReader(new StringReader(File.ReadAllText(configPath))));

            List<GameMetadata> games = new List<GameMetadata>();
            foreach (var game in xeniaGames)
            {
                string iconPath = Path.GetFullPath(Path.Combine(xeniaDirectory, game.ArtWork.Icon));
                string coverPath = Path.GetFullPath(Path.Combine(xeniaDirectory, game.ArtWork.BoxArt));
                string backgroundPath = Path.GetFullPath(Path.Combine(xeniaDirectory, game.ArtWork.Background));

                games.Add(new GameMetadata
                {
                    Name = game.Title,
                    GameId = game.GameId,
                    GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.File,
                            Path = path,
                            IsPlayAction = true,
                            Arguments = $@"""{game.Title}""" //Surround title with quotes so that argument parsing does not split titles containing spaces
                        }
                    },
                    IsInstalled = true,
                    Icon = new MetadataFile(iconPath),
                    CoverImage = new MetadataFile(coverPath),
                    BackgroundImage = new MetadataFile(backgroundPath),
                    InstallDirectory = game.FileLocation.Game
                });
            }

            return games;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new XeniaNiteSettingsView();
        }
    }

    //Would be better to have Xenia Manager as a dependency, but this is easier
    public class GameInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("game_id")]
        public string GameId { get; set; }

        [JsonProperty("playtime")]
        public float? Playtime { get; set; }

        [JsonProperty("artwork")] 
        public Icons ArtWork { get; set; }

        [JsonProperty("file_locations")]
        public FileLocations FileLocation { get; set; }

        public class Icons
        {
            [JsonProperty("background")]
            public string Background;

            [JsonProperty("boxart")]
            public string BoxArt;

            [JsonProperty("icon")]
            public string Icon;
        }

        public class FileLocations
        {
            [JsonProperty("game_location")]
            public string Game { get; set; }
        }
    }
}