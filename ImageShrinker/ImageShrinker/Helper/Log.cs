
using System;
using System.IO;

namespace ImageShrinker.Helper
{
    class Log
    {
        public static void WriteLine(string line)
        {
            using (TextWriter writer = new StreamWriter("limageshrinker.log"))
            {
                writer.WriteLine(string.Format("{0}: {1}", DateTime.Now, line));
                writer.Close();
            }
        }

        public static void WriteException(Exception exception)
        {
            WriteLine(string.Format("Exception caught:\n\tMessage: {0}\n\tStacktrace: {1}", exception.Message, exception.StackTrace));
        }
    }
}
