using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WordFrequency.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public static string RemoveHtmlTags(string html)
        {
            string htmlRemoved = Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>|<[^>]+>| ", " ").Trim();
            string normalised = Regex.Replace(htmlRemoved, @"\s{2,}", " ");
            return normalised;
        }

        public JObject DownloadPageAsync(string url)
        {
            using (WebClient client = new WebClient())
            {
                //get the page source
                string html = client.DownloadString(url).ToLower();

                //remove html elements
                html = RemoveHtmlTags(html);

                //split list into keywords by space characters
                List<string> list = html.Split(' ').ToList();

                //remove any non alphabet characters
                var onlyAlphabetRegEx = new Regex(@"^[A-z]+$");
                list = list.Where(f => onlyAlphabetRegEx.IsMatch(f)).ToList();

                //further blacklist words (greater than 2 characters, not important, etc..)
                string[] blacklist = { "a", "an", "on", "of", "or", "as", "i", "in", "is", "to", "the", "and", "for", "with", "not", "by" }; //add your own
                list = list.Where(x => x.Length > 2).Where(x => !blacklist.Contains(x)).ToList();

                //distict keywords by key and count, and then order by count.
                var keywords = list.GroupBy(x => x).OrderByDescending(x => x.Count());

                Dictionary<string, int> wordsDictionary = new Dictionary<string, int>();

                //print each keyword to console.
                foreach (var word in keywords)
                {
                    wordsDictionary.Add(word.Key, word.Count());
                }

                string json = JsonConvert.SerializeObject(wordsDictionary, Formatting.Indented);

                return JObject.Parse(json);
            }
        }
    }
}