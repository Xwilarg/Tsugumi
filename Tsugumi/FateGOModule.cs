using Discord.Commands;
using System;
using System.Collections.Generic;
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
            List<string> characters = new List<string>();
            string androidVersion, iosVersion;
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
                html = await hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/Template:Game_Version?action=raw");
                androidVersion = Regex.Match(html, "\\[https:\\/\\/news\\.fate-go\\.jp\\/2019\\/0919qwer\\/ ([0-9]+\\.[0-9]+\\.[0-9]+)\\]").Groups[1].Value;
                iosVersion = Regex.Match(html, "\\[https:\\/\\/news\\.fate-go\\.jp\\/2019\\/0919jwnr\\/ ([0-9]+\\.[0-9]+\\.[0-9]+)\\]").Groups[1].Value;
            }
            await ReplyAsync(characters.Count + " characters found, Android version: " + androidVersion + ", IOS version: " + iosVersion);
        }
    }
}