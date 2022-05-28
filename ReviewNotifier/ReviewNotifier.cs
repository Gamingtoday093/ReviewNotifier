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
using Rocket.Core.Utils;
using Rocket.Core;
using Rocket.API;
using Rocket.API.Collections;

namespace ReviewNotifier
{
    public class ReviewNotifier : RocketPlugin<ReviewNotifierConfiguration>
    {
        public static ReviewNotifier Instance { get; private set; }
        public static ReviewNotifierConfiguration Config { get; private set; }
        protected override void Load()
        {
            Instance = this;
            Config = Configuration.Instance;

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
                List<IRocketPlugin> RocketPlugins = R.Plugins.GetPlugins();

                foreach (DisplayProduct displayProduct in Displayproducts)
                {
                    if (displayProduct.seller.name == Config.UsernameOrSteamId || Config.UsernameOrSteamId == displayProduct.seller.steamId) continue;
                    if (Config.IgnoreProductIDs.Any(p => p == displayProduct.id)) continue;
                    if (!Config.ProductIDs.Any(p => p == displayProduct.id) && !RocketPlugins.Any(p => FuzzySearch(p.Name, displayProduct.name))) continue;
                    bool isReviewed = await IsReviewed(displayProduct.id);
                    if (isReviewed) continue;
                    SendNotReviewedMessage(displayProduct);
                }

                LogMessage("ReviewNotifier >> All Products have been Searched!");
            });
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

        public void LogMessage(object message, ConsoleColor consoleColor = ConsoleColor.Gray)
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
