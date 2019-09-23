using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tsugumi
{
    public class FateGOModule : ModuleBase
    {
        [Command("Relation", RunMode = RunMode.Async)]
        public async Task Info()
        {
        }

        public static async Task<List<string>> GetServantRelations(string servant)
        {
            List<string> dialogues = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string html = await hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/Sub:" + servant + "/Dialogue");
                foreach (Match m in Regex.Matches(html, "Dialogue [0-9]+").Cast<Match>())
                {

                }
            }
            return dialogues;
        }

        public static async Task<List<string>> GetServantList()
        {
            List<string> characters = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string html = hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/Servant_List_by_ID").GetAwaiter().GetResult();
                foreach (string s in html.Split(new[] { "<tr>" }, StringSplitOptions.None))
                {
                    Match match = Regex.Match(s, "<a href=\"\\/wiki\\/[^\"]+\"[ \\t]+class=\"[^\"]+\"[ \\t]+title=\"([^\"]+)\"");
                    if (match.Success)
                    {
                        string name = match.Groups[1].Value;
                        if (!s.Contains("Unplayable Servants") && !characters.Contains(name))
                            characters.Add(name);
                    }
                }
            }
            return characters;
        }

        // Array of version: [android Version, IOS Version]
        public static async Task<string[]> GetVersions()
        {
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string html = await hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/Template:Game_Version?action=raw");
                string androidVersion = Regex.Match(html, "\\[https:\\/\\/news\\.fate-go\\.jp\\/2019\\/0919qwer\\/ ([0-9]+\\.[0-9]+\\.[0-9]+)\\]").Groups[1].Value;
                string iosVersion = Regex.Match(html, "\\[https:\\/\\/news\\.fate-go\\.jp\\/2019\\/0919jwnr\\/ ([0-9]+\\.[0-9]+\\.[0-9]+)\\]").Groups[1].Value;
                return new[] { androidVersion, iosVersion };
            }
        }
    }
}