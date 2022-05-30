using Rocket.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = Rocket.Core.Logging.Logger;
using SDG.Unturned;
using ReviewNotifier.Models;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using Rocket.Core;
using Rocket.API.Collections;
using System.IO;
using Dir = System.IO.Directory;
using SDG.Framework.Modules;

namespace ReviewNotifier
{
    public class ReviewNotifier : RocketPlugin<ReviewNotifierConfiguration>
    {
        public static ReviewNotifier Instance { get; private set; }
        public static ReviewNotifierConfiguration Config { get; private set; }
        public static bool RunningOpenMod => ModuleHook.modules.Any(m => m.config.Name == "OpenMod.Unturned" && m.status == EModuleStatus.Initialized);
        protected override void Load()
        {
            Instance = this;
            Config = Configuration.Instance;

            if (RunningOpenMod) Logger.LogWarning("ReviewNotifier Only has Partial Support for OpenMod!");
            Logger.Log($"{Name} {Assembly.GetName().Version} by Gamingtoday093 has been Loaded");

            if (Level.isLoaded) DisplayReviews(0);
            else Level.onLevelLoaded += DisplayReviews;
        }

        protected override void Unload()
        {
            Logger.Log($"{Name} has been Unloaded");
        }

        public void DisplayReviews(int __)
        {
            LogMessage("ReviewNotifier >> Getting Reviews..");
            ThreadPool.QueueUserWorkItem(async (_) =>
            {
                List<DisplayProduct> Displayproducts = await GetDisplayProducts();
                List<string> Plugins = await GetPluginNames();

                foreach (DisplayProduct displayProduct in Displayproducts)
                {
                    if (displayProduct.seller.name == Config.UsernameOrSteamId || Config.UsernameOrSteamId == displayProduct.seller.steamId) continue;
                    if (Config.IgnoreProductIDs.Any(p => p == displayProduct.id)) continue;
                    if (!Config.ProductIDs.Any(p => p == displayProduct.id) && !Plugins.Any(pluginName => FuzzySearch(pluginName, displayProduct.name))) continue;
                    bool isReviewed = await IsReviewed(displayProduct.id);
                    if (isReviewed) continue;
                    SendNotReviewedMessage(displayProduct);
                }

                LogMessage("ReviewNotifier >> All Products have been Searched!");
            });
        }

        public async Task<List<string>> GetPluginNames()
        {
            // Server Directory
            string ServerDirectory = Path.GetDirectoryName(System.Environment.CurrentDirectory);

            // Rocket Plugins
            List<string> Plugins = R.Plugins.GetPlugins().Select(p => p.Name).ToList();

            // OpenMod
            // Would Ideally use Loaded OpenMod Plugins but that seems to be impossible to get if youre not using a OpenMod Plugin (Hours Wasted: 11h)
            if (RunningOpenMod)
            {
                // Plugins Folder
                string OpenModPluginsDirectory = Path.Combine(ServerDirectory, "OpenMod", "plugins");
                if (Dir.Exists(OpenModPluginsDirectory))
                    foreach (string file in Dir.GetFiles(OpenModPluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly))
                        Plugins.Add(Path.GetFileNameWithoutExtension(file));

                // Packages Folder
                string[] IgnorePackages = new string[]
                {
                    "OpenMod.UnityEngine",
                    "OpenMod.Unturned"
                };

                string OpenModPackagesPath = Path.Combine(ServerDirectory, "OpenMod", "packages", "packages.yaml");
                if (File.Exists(OpenModPackagesPath))
                {
                    string[] dataText;
                    using (StreamReader stream = File.OpenText(OpenModPackagesPath))
                    {
                        dataText = (await stream.ReadToEndAsync()).Split(new[] { System.Environment.NewLine },
                                     StringSplitOptions.RemoveEmptyEntries); ;
                    }

                    foreach (string line in dataText)
                    {
                        if (!line.Contains("- id: ")) continue;
                        string packageId = line.Substring(line.IndexOf("- id: ") + 6);
                        if (IgnorePackages.Contains(packageId)) continue;
                        Plugins.Add(packageId);
                    }
                }
            }

            // UScript 2
            // Using Directories and Not Modules Because Scripts dont have an Assembly
            string UScript2Directory = Path.Combine(ServerDirectory, "uScript", "Scripts");
            if (Dir.Exists(UScript2Directory))
                foreach (string file in Dir.GetFiles(UScript2Directory, "*.uscript", SearchOption.AllDirectories))
                    Plugins.Add(Path.GetFileNameWithoutExtension(file));

            return Plugins;
        }

        public bool FuzzySearch(string input, string target)
        {
            input = input.ToLower().Replace(" ", "").Replace("_", "");
            target = target.ToLower().Replace(" ", "").Replace("_", "");

            if ((input.Contains(target) | target.Contains(input)) && (float)input.Length / (float)target.Length < 1.5f && (float)input.Length / (float)target.Length > 0.75f) return true;

            float FuzzyValue = 100;
            // Compare length
            FuzzyValue -= Math.Abs(input.Length - target.Length) * 2; // Removes 2 Points for Every Length Mismatch
            // Character composition
            for (int i = 0; i < input.Length; i++)
            {
                if (!target.Contains(input[i])) FuzzyValue -= 5; // Removes 5 Points if Target doesnt have an input character
            }
            // Character position / Series
            for (int i = 0; i < target.Length; i++)
            {
                if (!input.Contains(target[i])) continue;
                FuzzyValue -= 2;
                var substring = input.Substring(input.IndexOf(target[i]) + 1, input.Length - input.IndexOf(target[i]) - 1);
                i++;
                for (int c = 0; c < substring.Length; c++)
                {
                    if (target.Length <= c + i)
                    {
                        i = target.Length;
                        break;
                    }
                    else if (target[i + c] == substring[c]) FuzzyValue += 2;
                    else
                    {
                        if (target.Length > (i + c + 1) && target[i + c + 1] != substring[c]) FuzzyValue -= 5;
                        FuzzyValue -= 3;
                        i += c;
                        break;
                    }
                }
            }

            return FuzzyValue >= Config.FuzzySearchSensitivity;
        }

        public void SendNotReviewedMessage(DisplayProduct displayProduct)
        {
            LogMessage(Translate("NotReviewed", Name, displayProduct.name, displayProduct.id, displayProduct.seller.name), ConsoleColor.DarkCyan);
        }

        public void LogMessage<T>(T message, ConsoleColor consoleColor = ConsoleColor.Gray)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public async Task<bool> IsReviewed(int ProductID)
        {
            DisplayProduct displayProduct = await GetDisplayProduct(ProductID);
            foreach (ProductReview review in displayProduct.reviews)
            {
                if (Config.UsernameOrSteamId == review.user.name || Config.UsernameOrSteamId == review.user.steamId) return true;
            }
            return false;
        }

        public async Task<List<DisplayProduct>> GetDisplayProducts()
        {
            using (WebClient client = new WebClient())
            {
                return JsonConvert.DeserializeObject<List<DisplayProduct>>(await client.DownloadStringTaskAsync("https://unturnedstore.com/api/products"));
            }
        }

        public async Task<DisplayProduct> GetDisplayProduct(int ProductID)
        {
            using (WebClient client = new WebClient())
            {
                return JsonConvert.DeserializeObject<DisplayProduct>(await client.DownloadStringTaskAsync($"https://unturnedstore.com/api/products/{ProductID}"));
            }
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "NotReviewed", "[{0}] {1} (ID: {2}) has not been Reviewed! Please show {3} Some Love by Reviewing their Plugin!" }
        };
    }
}
