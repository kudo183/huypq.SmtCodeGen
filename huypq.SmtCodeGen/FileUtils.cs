using System.Linq;
using System.Text;

namespace huypq.SmtCodeGen
{
    public static class FileUtils
    {
        public static void WriteAllTextInUTF8(string path, string content)
        {
            System.IO.File.WriteAllText(path, content, Encoding.UTF8);
        }

        public static void DeleteAllFileEndWith(string path, string end)
        {
            if (System.IO.Directory.Exists(path) == false)
            {
                return;
            }

            foreach (var item in System.IO.Directory.EnumerateFiles(path))
            {
                if (item.EndsWith(end) == true)
                {
                    System.IO.File.Delete(item);
                }
            }
        }

        public static void DeleteAllFileEndWith(string path, string end, string exclude)
        {
            if (System.IO.Directory.Exists(path) == false)
            {
                return;
            }

            foreach (var item in System.IO.Directory.EnumerateFiles(path, end))
            {
                if (item.EndsWith(exclude) == true)
                {
                    continue;
                }
                if (item.EndsWith(end) == true)
                {
                    System.IO.File.Delete(item);
                }
            }
        }
    }
}
