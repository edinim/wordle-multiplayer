namespace Services
{
    public class SolutionWordService
    {

        private static Dictionary<string, List<string>> Words;
        public SolutionWordService()
        {
            Words = new Dictionary<string, List<string>>();
            var path = Environment.CurrentDirectory;

            var enWordsPath = System.IO.Path.Join(path, "wwwroot/words/en-words.txt");

            Words.Add("en", ReadFileLineByLine(enWordsPath));
        }

        public bool IsWord(string lang, string word) {
            return Words[lang].Any(x => x.ToLower().Equals(word.ToLower()));
        }

        public string GetRandomWord(string lang){
            return Words[lang][new Random().Next(Words[lang].Count)];
        }


        private List<string> ReadFileLineByLine(string path)
        {
            List<string> lines = [];
            using (StreamReader sr = File.OpenText(path))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    lines.Add(s);
                }
            }

            return lines;
        }
    }
}