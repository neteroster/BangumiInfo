using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace TestPj1
{
    internal class BangumiSubject
    {
        public const string SUBJECTURL = "https://bgm.tv/subject/";

        private string id;
        private string url;

        private string? summary;
        private string? originalName;
        private string? averageScore;

        private Dictionary<string, List<string>>? infoBox;

        public class SingleRelatedWork
        {
            public string title;
            public string id;

            public SingleRelatedWork(string titleInput, string idInput)
            {
                title = titleInput;
                id = idInput;
            }
        }

        private Dictionary<string, List<SingleRelatedWork>>? relatedWorks;

        public class DetailedScore // Fix Constructor
        {
            /*
             * The Structure of `DetailedScore`:
             * (uint) totalVotes: The total number of people who vote this subject.
             * (uint[]) allScoreVotes: The number of people who vote 1, 2, 3 .. 9, 10.
             */
            public uint totalVotes;
            public uint[] allScoreVotes;

            public DetailedScore()
            {
                totalVotes = 0;
                allScoreVotes = new uint[10];
            }

        }

        private DetailedScore? detailedScore;

        public class SingleSubjectTag
        {
            public string title;
            public uint number;

            public SingleSubjectTag(string titleInput, uint numberInput)
            {
                title = titleInput;
                number = numberInput;
            }
        }

        private List<SingleSubjectTag>? subjectTag;

        private HtmlWeb bangumiWeb;
        protected HtmlDocument bangumiDoc;

        public BangumiSubject(string bgmId)
        {
            id = bgmId;
            url = SUBJECTURL + id;
            
            bangumiWeb = new HtmlWeb();
            bangumiWeb.OverrideEncoding = Encoding.UTF8;
            bangumiDoc = bangumiWeb.Load(url);

            summary = null;
            originalName = null;
            averageScore = null;

            infoBox = null;
            relatedWorks = null;

            detailedScore = null;
            subjectTag = null;

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

        public string GetAverageScore()
        {
            if (averageScore != null)
                return averageScore;

            averageScore = bangumiDoc.DocumentNode
                .SelectSingleNode("//span[@class='number']")
                .InnerText
                .Trim();

            return averageScore;
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

        public Dictionary<string, List<SingleRelatedWork>> GetRelated()
        {
            if (relatedWorks != null)
                return relatedWorks;
            relatedWorks = new Dictionary<string, List<SingleRelatedWork>>();

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
                        relatedWorks.Add(subType, new List<SingleRelatedWork>());
                    continue;
                }
                if (!desNode.HasClass("title")) continue;

                var subId = desNode.Attributes["href"].Value
                    .Split('/')[2];
                var subTitle = desNode.InnerText;

                relatedWorks[lastSubType]
                    .Add(new SingleRelatedWork(subTitle, subId));
            }

            return relatedWorks;
            
        }

        public DetailedScore GetDetailedScore()
        {
            if (detailedScore != null)
                return detailedScore;

            detailedScore = new DetailedScore();

            var scoreNode = bangumiDoc.DocumentNode
                .SelectNodes("//descendant::span[@class='count']");

            var count = 9;
            foreach (var subNode in scoreNode)
            {
                detailedScore.allScoreVotes[count] = Convert.ToUInt32(
                    subNode.InnerText.Trim('(', ')')
                    );
                count--;
            }

            foreach (var score in detailedScore.allScoreVotes)
            {
                detailedScore.totalVotes += score;
            }
            return detailedScore;
        }

        public List<SingleSubjectTag> GetTags()
        {
            if (subjectTag != null)
                return subjectTag;

            subjectTag = new List<SingleSubjectTag>();

            var tagNode = bangumiDoc.DocumentNode
                .SelectNodes("//descendant::div[@class='inner']")[1];

            foreach (var subNode in tagNode.ChildNodes)
            {
                subjectTag.Add(
                    new SingleSubjectTag(
                        subNode.ChildNodes[0].InnerText,
                        Convert.ToUInt32(subNode.ChildNodes[2].InnerText)
                        )
                    );
            }

            return subjectTag;
        }
    }

    internal class BangumiMusicSubject: BangumiSubject
    {
        public class SingleSong
        {
            public string title;
            public string epId;

            public SingleSong(string titleInput, string epIdInput)
            {
                title = titleInput;
                epId = epIdInput;
            }
        }

        public class MusicDisc
        {
            public string title;
            public List<SingleSong> songs;
            public MusicDisc(string titleInput, List<SingleSong> songsInput)
            {
                songs = songsInput;
                title = titleInput;
            }

        }

        public class MusicList
        {
            public List<MusicDisc> musicDiscs;
            public MusicList(List<MusicDisc> musicDiscsInput)
            {
                musicDiscs = musicDiscsInput;
            }

        }
        private MusicList? musicList;
        public BangumiMusicSubject(string bgmMusicId) : base(bgmMusicId)
        {
            musicList = null;
        }

        public MusicList GetMusicList()
        {
            if (musicList != null)
                return musicList;

            var discNodes = bangumiDoc.DocumentNode
                .SelectNodes("//ul[@class='line_list line_list_music']/li");

            musicList = new MusicList(new List<MusicDisc>());
            foreach (var child in discNodes)
            {
                if (child.HasClass("cat"))
                {
                    musicList.musicDiscs.Add(new MusicDisc(child.InnerText, new List<SingleSong>()));
                    continue;
                }
                musicList.musicDiscs.Last().songs.Add(
                    new SingleSong(
                        child.ChildNodes[3].InnerText
                        .Trim().Split(' ', 2)[1],
                        child.ChildNodes[3].ChildNodes[1].Attributes["href"].Value
                        .Split('/')[2]
                        )
                    );
            }

            return null;
        }
    }
}
