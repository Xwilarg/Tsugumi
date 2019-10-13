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
            string name;
            using (HttpClient hc = new HttpClient())
            {
                string html = await hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/Special:Search?search=" + WebUtility.UrlEncode(string.Join("", args)) + "&limit=1");
                Match match = Regex.Match(html, "<a href=\"https?:\\/\\/fategrandorder.fandom.com\\/wiki\\/([^\"]+)\" class=\"result-link");
                if (!match.Success)
                {
                    await ReplyAsync("There is nobody with this name");
                    return;
                }
                name = match.Groups[1].Value;
            }
            var answer = await Program.P.BotDb.GetRelations(name);
            if (answer == null)
            {
                await ReplyAsync("There is nobody with this name");
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = Color.Blue,
                    Title = WebUtility.UrlDecode(answer.Item1)
                };
                embed.AddField("Have dialogues with", "`" + WebUtility.UrlDecode(string.Join(Environment.NewLine, answer.Item2) + "`"));
                embed.AddField("Characters that have dialogues with", "`" + string.Join(Environment.NewLine, await Program.P.BotDb.HaveRelationsWith(name)) + "`");
                await ReplyAsync("", false, embed.Build());
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
                foreach (string servantClass in new[] { "Shielder", "Saber", "Archer", "Lancer", "Rider", "Caster", "Assassin", "Berserker", "Ruler", "Avenger", "Moon_Cancer", "Alter_Ego", "Foreigner" })
                {
                    string html = hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/" + servantClass).GetAwaiter().GetResult();
                    html = html.Split(new[] { "navbox mw-collapsible" }, StringSplitOptions.None)[0];
                    html = string.Join("", html.Split(new[] { "article-thumb tnone show-info-icon" }, StringSplitOptions.None).Skip(1));
                    foreach (string s in html.Split(new[] { "<td>" }, StringSplitOptions.None))
                    {
                        Match match = Regex.Match(s, "<a href=\"\\/wiki\\/([^\"]+)\"( |\t)*title=\"[^\"]+\">");
                        if (match.Success && !s.Contains("Unplayable"))
                        {
                            string name = match.Groups[1].Value;
                            if (!characters.Contains(name))
                            {
                                characters.Add(name);
                            }
                        }
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
            ulong i = 1;
            foreach (char c in CleanWord(word))
            {
                id += c * i;
                i *= 10;
            }
            return id;
        }
    }
}