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
                //print litecollection
                logger.Info(games.Count().ToString());
                //print out games
                foreach (var game in games.FindAll())
                {
                    logger.Info(game["Name"].ToString());
                    //convert to Game object
                    
                    
                }
                var gameslist = db.GetCollection<Game>("Game");
                logger.Info(gameslist.Count().ToString());
                IEnumerable<Game> gameslist2 = gameslist.FindAll();
                foreach (var game in gameslist2)
                {
                    logger.Info(game.Name);
                }
                
                return new List<Game>();
            }

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