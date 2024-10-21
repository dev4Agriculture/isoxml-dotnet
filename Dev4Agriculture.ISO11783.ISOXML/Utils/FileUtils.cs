using System.IO;
using System.Linq;

namespace Dev4Agriculture.ISO11783.ISOXML.Utils
{
    public class FileUtils
    {

        public static string GetParentFolder(string absolutePath, string relativePath)
        {
            // Combine the absolute and relative paths to get the full path of the file
            string fullPath = Path.GetFullPath(Path.Combine(absolutePath, relativePath));

            // Get the directory name of the full path, which represents the parent folder
            string parentFolder = Path.GetDirectoryName(fullPath);

            return parentFolder;
        }

        public static bool HasMultipleFilesEndingWithThatName(string root, string fileName)
        {

            fileName = fileName.ToLower();
            if (!Directory.Exists(Path.GetFullPath(root)))
            {
                return false;
            }
            var files = Directory.GetFiles(root);
            var numberOfSameFiles = files.ToList().Where(entry => entry.ToLower().EndsWith(fileName)).Count();
            return numberOfSameFiles > 1;
        }


        public static bool AdjustFileNameToIgnoreCasing(string root, string fileName, out string path)
        {

            fileName = fileName.ToLower();
            if (!Directory.Exists(Path.GetFullPath(root)))
            {
                path = "";
                return false;
            }
            foreach (var file in Directory.GetFiles(root))
            {
                if (Path.GetFileName(file).ToLower().Equals(fileName))
                {
                    path = file;
                    return true;
                }
            }

            foreach (var subdir in Directory.GetDirectories(root))
            {
                foreach (var file in Directory.GetFiles(Path.Combine(root, subdir)))
                {
                    if (file.ToLower().Equals(fileName))
                    {
                        path = file;
                        return true;
                    }
                }
            }

            path = "";
            return false;
        }
    }
}
