using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace TestPj1
{
    internal class BangumiInfo
    {
        public const string SUBJECTURL = "https://bgm.tv/subject/";

        private string id;
        private string url;

        private string? summary;
        private string? originalName;
        private string? score;

        private Dictionary<string, List<string>>? infoBox;

        private Dictionary<string, List<Dictionary<string, string>>>? relatedWorks;

        private HtmlWeb bangumiWeb;
        private HtmlDocument bangumiDoc;

        public BangumiInfo(string bgmId)
        {
            id = bgmId;
            url = SUBJECTURL + id;
            
            bangumiWeb = new HtmlWeb();
            bangumiWeb.OverrideEncoding = Encoding.UTF8;
            bangumiDoc = bangumiWeb.Load(url);

            summary = null;
            originalName = null;
            score = null;

            infoBox = null;
            relatedWorks = null;

        }

        public void Clean()
        {
            summary = null;
            originalName = null;
            score = null;

            infoBox = null;
            relatedWorks = null;
        }

        public string GetRawPage()
        {
            return bangumiDoc.Text;
        }

        public string GetOriginalName()
        {
            if (originalName != null)
                return originalName;

            originalName = bangumiDoc.DocumentNode
                .SelectSingleNode("//descendant::h1[@class='nameSingle']")
                .InnerText
                .Trim();

            return originalName;
        }

        public string GetScore()
        {
            if (score != null)
                return score;

            score = bangumiDoc.DocumentNode
                .SelectSingleNode("//span[@class='number']")
                .InnerText
                .Trim();

            return score;
        }

        public string GetSummary()
        {
            if (summary != null)
                return summary;

            summary = bangumiDoc.DocumentNode
                .SelectSingleNode("//descendant-or-self::div[@id='subject_summary']")
                .InnerText
                .Trim();

            return summary;
        }

        public Dictionary<string, List<string>> GetInfoBox()
        {
            if (infoBox != null)
                return infoBox;

            infoBox = new Dictionary<string, List<string>>();
            var infoList = bangumiDoc.DocumentNode
                .SelectSingleNode("//descendant::ul[@id='infobox']");

            foreach (var i in infoList.ChildNodes)
            {
                if (i.InnerText.Trim() == "") continue;
                var innerTxt = i.InnerText.Trim();

                string subType = innerTxt.Split(": ")[0];
                string[] subContent = innerTxt.Split(": ")[1].Split("、");

                if (!infoBox.ContainsKey(subType))
                {
                    infoBox.Add(subType, subContent.ToList());
                    continue;
                }
                infoBox[subType].AddRange(subContent.ToList());
            }
            return infoBox;
        }

        public Dictionary<string, List<Dictionary<string, string>>> GetRelated()
        {
            if (relatedWorks != null)
                return relatedWorks;
            relatedWorks = new Dictionary<string, List<Dictionary<string, string>>>();

            var nearNode = bangumiDoc.DocumentNode
                .SelectSingleNode("//descendant::div[@class='content_inner']");
            var lastSubType = "";
            foreach (var desNode in nearNode.Descendants())
            {
                if (desNode.HasClass("sub") && desNode.InnerText != "")
                {
                    var subType = desNode.InnerText;
                    lastSubType = subType;

                    if (!relatedWorks.ContainsKey(subType))
                        relatedWorks.Add(subType, new List<Dictionary<string, string>>());
                    continue;
                }
                if (!desNode.HasClass("title")) continue;

                var subId = desNode.Attributes["href"].Value
                    .Split('/')[2];
                var subTitle = desNode.InnerText;

                relatedWorks[lastSubType]
                    .Add(
                    new Dictionary<string, string>
                    {
                        { "id", subId }, { "title", subTitle }
                    }
                    );
            }

            return relatedWorks;
            
        }
    }
}
