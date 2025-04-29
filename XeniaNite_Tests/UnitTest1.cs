using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Runtime;
using Xenia;

namespace XeniaNite_Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var serializer = JsonSerializer.CreateDefault();
            string path = "C:/Users/minec/Desktop/Xenia/XeniaManager.DesktopApp.exe";
            int lastIndex = path.LastIndexOf('/');
            
            string config = "Config/games.json";
            string xeniaDirectory = lastIndex >= 0 ? path.Substring(0, lastIndex) : path;

            string configPath = Path.GetFullPath(Path.Combine(xeniaDirectory, config));
            var xeniaGames = serializer.Deserialize<List<GameInfo>>(new JsonTextReader(new StringReader(File.ReadAllText(configPath))));
            Assert.NotNull(xeniaGames);

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
                            Arguments = game.GameId
                        }
                    },
                    IsInstalled = true,
                    Icon = new MetadataFile(iconPath),
                    CoverImage = new MetadataFile(coverPath),
                    BackgroundImage = new MetadataFile(backgroundPath),
                    InstallDirectory = game.FileLocation.Game
                });
            }
        }
    }
}