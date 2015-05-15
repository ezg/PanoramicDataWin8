using System.Collections.Generic;

namespace PanoramicDataWin8.utils
{
    public class CSVParser
    {
        public static List<string> CSVLineSplit(string line)
        {
            bool open = false;
            List<string> words = new List<string>();
            string word = "";
            foreach (char t in line)
            {
                if (t == ',' && !open)
                {
                    words.Add(word);
                    word = "";
                }
                else if (t == '"' && !open)
                {
                    open = true;
                    //words.Add(word);
                    //word = "";
                }
                else if (t == '"' && open)
                {
                    open = false;
                }
                else
                {
                    word += t;
                }
            }
            words.Add(word);
            return words;
        }
    }
}
