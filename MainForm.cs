using System;

using System.Collections.Generic;

using System.ComponentModel;

using System.Data;

using System.Drawing;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

using System.Windows.Forms;

using System.IO;

using System.Text.RegularExpressions;
using Syn.WordNet;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;

namespace SentimentAnalyzer

{
    public partial class MainForm : Form

    {

        private Dictionary<string, int> myDict;

        public class MessagePiece

        {

            public int count;

            public Dictionary<string, int> innerData;

        }

        private string strpronouns =
@"all
another
any
anybody
anyone
anything
as
aught
both
each
each other
either
enough
everybody
everyone
everything
few
he
her
hers
herself
him
himself
his
I
idem
it
its
itself
many
me
mine
most
my
myself
naught
neither
no one
nobody
none
nothing
nought
one
one another
other
others
ought
our
ours
ourself
ourselves
several
she
some
somebody
someone
something
somewhat
such
suchlike
that
thee
their
theirs
theirself
theirselves
them
themself
themselves
there
these
they
thine
this
those
thou
thy
thyself
us
we
what
whatever
whatnot
whatsoever
whence
where
whereby
wherefrom
wherein
whereinto
whereof
whereon
wherever
wheresoever
whereto
whereunto
wherewith
wherewithal
whether
which
whichever
whichsoever
who
whoever
whom
whomever
whomso
whomsoever
whose
whosever
whosesoever
whoso
whosoever
ye
yon
yonder
you
your
yours
yourself
yourselves";

        private string[] skipWords = { "all", "another" , "any", "anybody", "anyone", "anything", "as", "aught", "both",
            "each", "each other", "either", "enough", "everybody", "everyone", "everything", "few", "he", "her",
            "hers", "herself", "him", "himself", "his", "i", "idem", "it", "its", "itself", "many", "me", "mine",
            "most", "my", "myself", "naught", "neither", "no one", "nobody", "none", "nothing", "nought", "one",
            "one another", "other", "others", "ought", "our", "ours", "ourself", "ourselves", "several", "she",
            "some", "somebody", "someone", "something", "somewhat", "such", "suchlike", "that", "thee", "their",
            "theirs", "theirself", "theirselves", "them", "themself", "themselves", "there", "these", "they", "thine",
            "this", "those", "thou", "thy", "thyself", "us", "we", "what", "whatever", "whatnot", "whatsoever",
            "whence", "where", "whereby", "wherefrom", "wherein", "whereinto", "whereof", "whereon", "wherever",
            "wheresoever", "whereto", "whereunto", "wherewith", "wherewithal", "whether", "which", "whichever",
            "whichsoever", "who", "whoever", "whom", "whomever", "whomso", "whomsoever", "whose", "whosever",
            "whosesoever", "whoso", "whosoever", "ye", "yon", "yonder", "you", "your", "yours", "yourself",
            "yourselves","a","the"};
        public Dictionary<string, MessagePiece> msgDict = new Dictionary<string, MessagePiece>();
        WordNetEngine wordNet = null;

        public MainForm()

        {

            InitializeComponent();

        }

        private void tokenizeStrings(string str) //for trdting snd tokeinizing
        {
            string[] sep = new string[] { "\r\n" };

            string[] rows = str.Split(sep, StringSplitOptions.None); //split full file text into rows 
            textBoxMain.Clear();
            foreach (string row in rows)
            {
                textBoxMain.AppendText("\"" + row + "\",");
            }


        }

        private void TestWordNet()
        {
            var directory = @"C:\code\Sentiment\SentimentAnalyzer\db\dict";
            textBoxDictOut.Clear();
            if (wordNet == null)
            {
                wordNet = new WordNetEngine();
                textBoxDictOut.AppendText("Loading WordNet Start..");
                wordNet.LoadFromDirectory(directory);
                textBoxDictOut.AppendText("\r\nLoading WordNet Complete");
            }

            var word = this.textBoxWord.Text;
            textBoxDictOut.AppendText($"\r\nAnalyzing '{word}'............");
            string singWorld=GetSingular(word);
            if(!singWorld.Equals(word))
            {
                textBoxDictOut.AppendText($"\r\nSingular form:'{singWorld}'............");
            }
            var synSetList = wordNet.GetSynSets(singWorld);
            

            if (synSetList.Count == 0) textBoxDictOut.AppendText ($"\r\nNo SynSet found for '{word}'");

            foreach (var synSet in synSetList)
            {
                var words = string.Join(", ", synSet.Words);

                textBoxDictOut.AppendText($"\r\nWords: {words}");
                textBoxDictOut.AppendText($"\r\nPOS: {synSet.PartOfSpeech}");
                textBoxDictOut.AppendText($"\r\nGloss: {synSet.Gloss}");
            }
        }

        public static string GetSingular(string word)
        {
            PluralizationService ps = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"));
            if (ps.IsPlural(word))
                return ps.Singularize(word);
            return word;           

        }

        private List<SynSet> GetSynSet(string word)
        {
            var directory = @"C:\code\Sentiment\SentimentAnalyzer\db\dict";
            //textBoxMain.Clear();
            if (wordNet == null)
            {
                wordNet = new WordNetEngine();
                textBoxMain.AppendText("Loading WordNet Start..");
                wordNet.LoadFromDirectory(directory);
                textBoxMain.AppendText("\r\nLoading WordNet Complete");
            }
            string singWorld = GetSingular(word);
            var synSetList = wordNet.GetSynSets(singWorld);
            return synSetList;


            //if (synSetList.Count == 0) textBoxMain.AppendText($"\r\nNo SynSet found for '{word}'");

            //foreach (var synSet in synSetList)
            //{
            //    var words = string.Join(", ", synSet.Words);

            //    textBoxMain.AppendText($"\r\nWords: {words}");
            //    textBoxMain.AppendText($"\r\nPOS: {synSet.PartOfSpeech}");
            //    textBoxMain.AppendText($"\r\nGloss: {synSet.Gloss}");
            //}
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)

        {

            //tokenizeStrings(strpronouns);


            //TestWordNet();
            Analyze2();

        }





        private void Analyze1()

        {



            textBoxAnalyze.Clear();

            myDict = TokenizeFile(textBoxFile.Text);

            List<KeyValuePair<string, int>> myList = myDict.ToList();

            myList.Sort(delegate (KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2)

            {

                return pair2.Value.CompareTo(pair1.Value);

                //return pair1.Value.CompareTo(pair2.Value);

                //return pair1.Key.CompareTo(pair2.Key);

            });



            foreach (KeyValuePair<string, int> pair in myList)

            {

                string sb = string.Format("{0} ----> {1}\r\n", pair.Key, pair.Value);

                textBoxAnalyze.AppendText(sb);

            }

        }



        private void Analyze2()

        {

            DataTable dtTbl = ReadCsvFile(textBoxFile.Text);

            textBoxMain.Clear();





            foreach (DataRow dr in dtTbl.Rows)

            {

                Tokenize(dr[0] + "||" + dr[1] + "||" + dr[2]);

            }
            textBoxMain.Clear();
            textBoxDictOut.Clear();

            using (StreamWriter sw = new StreamWriter(textBoxFile.Text + ".out"))

            {

                foreach (string key in msgDict.Keys)

                {

                    MessagePiece msgpiece = msgDict[key];

                    List<KeyValuePair<string, int>> myList = msgpiece.innerData.ToList();

                    myList.Sort(delegate (KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2)

                    {

                        return pair2.Value.CompareTo(pair1.Value);

                    //return pair1.Value.CompareTo(pair2.Value);

                    //return pair1.Key.CompareTo(pair2.Key);
                    });



                    string sb = string.Format("**************MRNID [{0}] ---->MSGCOUNT [{1}]**************\r\n", key, msgpiece.count);

                    textBoxAnalyze.AppendText(sb);
                    sw.WriteLine(sb);

                    foreach (KeyValuePair<string, int> pair in myList)

                    {

                        if (IsAlphabetic(pair.Key) && !skipWords.Contains(pair.Key))

                        {
                            var synSetList=GetSynSet(pair.Key);
                            //foreach (var synSet in synSetList)
                            //{
                            //    var words = string.Join(", ", synSet.Words);

                            //    textBoxDictOut.AppendText($"\r\nWords: {words}");
                            //    textBoxDictOut.AppendText($"\r\nPOS: {synSet.PartOfSpeech}");
                            //    textBoxDictOut.AppendText($"\r\nGloss: {synSet.Gloss}");
                            //}
                            if (synSetList.Count > 0)
                            {
                                sb = string.Format("{0} ----> {1}\r\n", pair.Key, pair.Value);

                                textBoxAnalyze.AppendText(sb);

                                sw.WriteLine(string.Format("{0},{1}", pair.Key, pair.Value));
                            }
                            else
                            {
                                sb = string.Format("{0} ----> {1}\r\n", pair.Key, pair.Value);
                                textBoxMain.AppendText(sb);
                            }

                        }
                        else
                        {
                            sb = string.Format("{0} ----> {1}\r\n", pair.Key, pair.Value);
                            textBoxMain.AppendText(sb);
                        }

                    }

                }
            }
        }



        public static bool IsAlphabetic(string str)

        {

            //string strRegex = @"^(?=.*[a-zA-Z])(?=.*[0-9])[A-Za-z0-9]+$";

            string strRegex = @"^[a-zA-Z]+$";

            Regex re = new Regex(strRegex);

            if (re.IsMatch(str))

                return (true);

            else

                return (false);

        }





        public DataTable ReadCsvFile(string filepath)

        {

            DataTable dtCsv = new DataTable();

            dtCsv.Columns.Add();

            dtCsv.Columns.Add();

            dtCsv.Columns.Add();

            string Fulltext;

            //string sep = "||";

            string[] sep = new string[] { "||" };

            string[] sep1 = new string[] { "|||" };

            using (StreamReader sr = new StreamReader(filepath))

            {

                while (!sr.EndOfStream)

                {

                    Fulltext = sr.ReadToEnd().ToString(); //read full file text  

                    string[] rows = Fulltext.Split(sep1, StringSplitOptions.None); //split full file text into rows  

                    for (int i = 0; i < rows.Count(); i++)

                    {

                        string[] rowValues = rows[i].Split(sep, StringSplitOptions.None); //split each row with comma to get individual values  

                        {

                            //if (i == 0)

                            //{

                            //    for (int j = 0; j < rowValues.Count(); j++)

                            //    {

                            //        dtCsv.Columns.Add(rowValues[j]); //add headers  

                            //    }

                            //}

                            //else

                            {

                                DataRow dr = dtCsv.NewRow();

                                for (int k = 0; k < rowValues.Count(); k++)

                                {

                                    dr[k] = rowValues[k].ToString();

                                    if (k == 0)

                                    {

                                        dr[k] = rowValues[k].ToString().Replace("\r\n", string.Empty);

                                    }

                                }

                                dtCsv.Rows.Add(dr); //add other rows  

                            }

                        }

                    }

                }

            }

            return dtCsv;

        }

        public Dictionary<string, int> TokenizeFile(string filepath)

        {

            //DataTable dtCsv = new DataTable();

            Dictionary<string, int> dataDict = new Dictionary<string, int>();

            string Fulltext = "";

            textBoxMain.Clear();

            using (StreamReader sr = new StreamReader(filepath))

            {

                while (!sr.EndOfStream)

                {

                    Fulltext = sr.ReadToEnd().ToString().Trim(); //read full file text      

                    Fulltext = Fulltext.Replace(".", String.Empty);

                    Fulltext = Fulltext.Replace(",", String.Empty);

                    string[] rows = Fulltext.Split('\n'); //split full file text into rows  

                    for (int i = 0; i < rows.Count(); i++)

                    {

                        string[] rowValues = rows[i].Trim().Split(' '); //split each row with space 

                        for (int j = 0; j < rowValues.Count(); j++)

                        {

                            if (dataDict.ContainsKey(rowValues[j]))

                            {

                                dataDict[rowValues[j]] = dataDict[rowValues[j]] + 1;

                            }

                            else

                            {

                                dataDict[rowValues[j]] = 1;

                            }

                        }

                    }

                }

            }

            textBoxMain.AppendText(Fulltext);

            return dataDict;

        }





        public Dictionary<string, MessagePiece> Tokenize(string text)

        {



            Dictionary<string, int> innerData = null;

            string Fulltext = "";



            Fulltext = text.Replace(".", String.Empty);
            Fulltext = Fulltext.Replace(",", String.Empty);
            Fulltext = Fulltext.Replace("'", String.Empty);
            Fulltext = Fulltext.Replace("?", String.Empty);
            Fulltext = Fulltext.Replace("\"", String.Empty);
            Fulltext = Fulltext.Replace("!", String.Empty);

            string[] sep = new string[] { "||" };
            string[] sepComment = new string[] { " ","\r\n","\r","\n"};

            string[] rows = Fulltext.Split(sep, StringSplitOptions.None); //split full file text into rows 

            if (rows.Count() == 3)

            {

                if (msgDict.ContainsKey(rows[0]))

                {

                    MessagePiece msg = msgDict[rows[0]];

                    msg.innerData = msgDict[rows[0]].innerData; //fetch innerdata already associated

                    msg.count = msgDict[rows[0]].count + 1;//increase msg count                    

                }

                else

                {



                    MessagePiece msg = new MessagePiece();

                    msg.innerData = new Dictionary<string, int>(); ;

                    msg.count = 1;

                    msgDict[rows[0]] = msg;

                }



                //string[] rowValues = rows[2].Trim().Split(' '); //split the comment section
                string[] rowValues = rows[2].Trim().Split(sepComment, StringSplitOptions.None);

                for (int j = 0; j < rowValues.Count(); j++)

                {
                    string key = rowValues[j].Trim().ToLower();
                    innerData = msgDict[rows[0]].innerData;

                    if (innerData.ContainsKey(key))

                    {

                        innerData[key] = innerData[key] + 1;

                    }

                    else

                    {

                        innerData[key] = 1;

                    }

                }

            }



            textBoxMain.AppendText(Fulltext);

            return msgDict;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            TestWordNet();
        }
    }

}







