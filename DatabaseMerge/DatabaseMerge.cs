using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.SDK.Data;
using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Playnite.SDK.Events;

namespace DatabaseMerge
{
    public class DatabaseMerge : LibraryPlugin
    {
        //private imported database object
        private IEnumerable<Game> importedGames;
        private static readonly ILogger logger = LogManager.GetLogger();

        private DatabaseMergeSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("39a68e17-7510-4ad9-971e-5695e6db4c75");

        // Change to something more appropriate
        public override string Name => "Import Playnite Database";

        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new DatabaseMergeClient();

        public DatabaseMerge(IPlayniteAPI api) : base(api)
        {
            settings = new DatabaseMergeSettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
            
            

        }
        
        private List<Guid> ArrayToGuids(BsonArray array)
        {
            //demo value: [{"$guid":"aec1ae80-795f-4c8c-9040-bdfc7cc80ba6"}]
            var list = new List<Guid>();
            foreach (BsonValue guid in array)
            {
                list.Add(Guid.Parse(guid.AsString));
                logger.Info(list.Last().ToString());
            }

            return list;
        }

        public IEnumerable<Game> ImportDatabase(string path) 
        {
            using(var db = new LiteDatabase(@"C:\gm\Playnite_Data\library\games - copy-backup.db"))
            {
                //reade Game collection from db
                //list collections
                var collections = db.GetCollectionNames();
                foreach (var name in collections)
                {
                    logger.Info(name);
                }
                var games = db.GetCollection("Game");
                //convert collection into list of Game
                List<Game> gamesObjects = new List<Game>();
                foreach (var game in games.FindAll())
                {
                    //logger.Info(game["Name"].AsString);
                    gamesObjects.Add(GameFromBson(game));
                    //logger.Info(gamesObjects.Last().Name);
                    
                }
                
                //reference playnite library
                var playniteDb = PlayniteApi.Database.Games;
                //check if each imported game already exists
                foreach (var game in gamesObjects)
                {
                    if(playniteDb.ContainsItem(game.Id))
                    {
                        logger.Info("Game not found in Playnite library: " + game.Name);
                        playniteDb.Add(game);
                    }
                    else
                    {
                        logger.Info("Game found in Playnite library" + game.Name);
                        //compare values and update playnite games if necessary
                        foreach (var key in game.GetDifferences(playniteDb.Get(game.Id)))
                        {
                            logger.Info(key.ToString());
                            //playniteDb.Update(game);
                            
                        }
                        
                    }
                }

                return gamesObjects;
            }

        }
        
        public Game GameFromBson(BsonDocument bson)
        {
            Game game = new Game();
            var thing = bson;
            //check if each parameter exists
            if(thing.TryGetValue("Name", out BsonValue name))
            {
                game.Name = thing["Name"].AsString;
                logger.Info(game.Name);
            }
            if(thing.TryGetValue("Description", out BsonValue description))
            {
                game.Description = thing["Description"].AsString;
                logger.Info(game.Description);
            }
            if(thing.TryGetValue("GenreIds", out BsonValue genres))
            {
                game.GenreIds = new List<Guid>();
                game.GenreIds = ArrayToGuids(thing["GenreIds"].AsArray);   
            }
            if(thing.TryGetValue("Tags", out BsonValue tags))
            {
                game.TagIds = new List<Guid>();
                game.TagIds = ArrayToGuids(thing["Tags"].AsArray);
            }
            if(thing.TryGetValue("PlatformIds", out BsonValue platforms))
            {
                game.PlatformIds = new List<Guid>();
                game.PlatformIds = ArrayToGuids(thing["PlatformIds"].AsArray);
            }
            if(thing.TryGetValue("ReleaseDate", out BsonValue releaseDate))
            {
                game.ReleaseDate = ReleaseDate.Deserialize(thing["ReleaseDate"].AsString);
                logger.Info(game.ReleaseDate.ToString());
            }
                //bson: [PublisherIds, [{"$guid":"aec1ae80-795f-4c8c-9040-bdfc7cc80ba6"}]]
            if (thing.TryGetValue("DeveloperIds", out BsonValue developerIds))
            {
                game.DeveloperIds = new List<Guid>();
                game.DeveloperIds = ArrayToGuids(thing["DeveloperIds"].AsArray);
            }
            if(thing.TryGetValue("PublisherIds", out BsonValue publisherIds))
            {
                game.PublisherIds = new List<Guid>();
                game.PublisherIds = ArrayToGuids(thing["PublisherIds"].AsArray);
            }
            if(thing.TryGetValue("CoverImage", out BsonValue coverImage))
            {
                game.CoverImage = thing["CoverImage"].AsString;
            }
            if(thing.TryGetValue("Icon", out BsonValue icon))
            {
                game.Icon = thing["Icon"].AsString;
            }
            if(thing.TryGetValue("BackgroundImage", out BsonValue backgroundImage)) {
                game.BackgroundImage = thing["BackgroundImage"].AsString;
            }
            
            return game;
        }
        
        //print out games from current database and imported database
        public void PrintCollectionGames(IEnumerable<Game> games)
        {
            logger.Info("Imported games follow\n==================================");
            logger.Info(games.Count().ToString());
            
        }
        
        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                Description = "Name of main menu item",
                MenuSection = "@DBM",
                Action = (mainMenuItem) =>
                {
                    // Do something when menu item is clicked
                    logger.Info("Invoked from Main Menu");
                    //PrintLibrary();
                    importedGames = ImportDatabase(@"C:\gm\Playnite_Data\library\games - copy-backup.db");
                    logger.Info("Imported games:");
                    PrintCollectionGames(importedGames);
                }
            };
        }
        

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new DatabaseMergeSettingsView();
        }
    }
}