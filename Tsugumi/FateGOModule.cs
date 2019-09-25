using Discord;
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
        public async Task Info(params string[] args)
        {
            if (Program.P.ServantUpdate < 100.5f)
            {
                await ReplyAsync("The database is currently updating, please retry later...");
                return;
            }
            if (!await Program.P.BotDb.AreVersionSame())
            {
                await ReplyAsync("Updating database, please wait... This can take several minutes.");
                await Program.P.BotDb.UpdateDb();
            }
            var answer = await Program.P.BotDb.GetRelations(string.Join("", args));
            if (answer == null)
            {
                await ReplyAsync("There is nobody with this name");
            }
            else
            {
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Color = Color.Blue,
                    Title = answer.Item1,
                    Description = string.Join(Environment.NewLine, answer.Item2)
                }.Build());
            }
        }

        public static async Task<Dictionary<string, List<string>>> GetAllServantRelations()
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
            var allServants = await GetServantList();
            Console.Write("Getting servants... 0%");
            Program.P.ServantUpdate = 0f;
            int i = 0;
            foreach (string s in await GetServantList())
            {
                res.Add(s, await GetServantRelations(s));
                i++;
                Program.P.ServantUpdate = i * 100f / allServants.Count;
                Console.Write("\rGetting servants... " + Program.P.ServantUpdate.ToString("0.00") + "%");
            }
            Console.WriteLine();
            Program.P.ServantUpdate = 101f;
            return res;
        }

        private static async Task<List<string>> GetServantRelations(string servant)
        {
            List<string> dialogues = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string html = await hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/Sub:" + servant + "/Dialogue");
                foreach (Match m in Regex.Matches(html, "Dialogue [0-9]+<br \\/>\\(<a href=\"\\/wiki\\/([^\"]+)\"[^>]+>").Cast<Match>())
                {
                    dialogues.Add(m.Groups[1].Value);
                }
            }
            return dialogues;
        }

        private static async Task<List<string>> GetServantList()
        {
            List<string> characters = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string html = hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/Servant_List_by_ID").GetAwaiter().GetResult();
                foreach (string s in html.Split(new[] { "<tr>" }, StringSplitOptions.None))
                {
                    Match match = Regex.Match(s, "<td> <a href=\"\\/wiki\\/([^\"]+)\"[ \\t]+class=\"[^\"]+\"[ \\t]+title=\"[^\"]+\"");
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

        public static string CleanWord(string word)
            => string.Join("", word.Where(x => char.IsLetterOrDigit(x)));

        public static ulong GetId(string word)
        {
            ulong id = 0;
            foreach (char c in CleanWord(word))
            {
                id += c;
            }
            return id;
        }
    }
}