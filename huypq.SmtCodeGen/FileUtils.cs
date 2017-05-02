using System.Text;

namespace huypq.SmtCodeGen
{
    public static class FileUtils
    {
        public static void WriteAllTextInUTF8(string path, string content)
        {
            System.IO.File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
