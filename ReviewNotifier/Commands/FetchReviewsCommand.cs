using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewNotifier.Commands
{
    public class FetchReviewsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public string Name => "FetchReviews";

        public string Help => "Displays Not Reviewed UnturnedStore Plugins";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            ReviewNotifier.Instance.DisplayReviews(0);
        }
    }
}
