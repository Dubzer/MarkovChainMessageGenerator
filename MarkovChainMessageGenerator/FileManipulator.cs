using System;
using System.IO;
using System.Text;

namespace MarkovChainMessageGenerator
{
    public class FileManipulator
    {
        /// <summary>
        /// As stackoverflow says, this is one of the fastest way to read files
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>File</returns>
        public static string Read(string path)
        {
            using StreamReader sr = File.OpenText(path);
            string text = String.Empty;
            while ((text = sr.ReadLine()) != null)
            {
                return text;
            }

            throw new Exception();
        }

        /// <summary>
        /// One of the fastest way to write large strings
        /// </summary>
        public static void Write(string text, string path)
        {
            using StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8, 65536);
            sw.WriteLine(text);
        }
    }
}
