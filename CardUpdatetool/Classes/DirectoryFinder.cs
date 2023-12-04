using System.Collections.Generic;
using System.IO;

namespace CardUpdateTool
{
    static class DirectoryFinder
    {
        public static List<string> Grab_All_Directories(string originalPath)
        {
            var foldersPath = new List<string>
            {
                originalPath
            };
            foldersPath.AddRange(Directory.GetDirectories(originalPath, "*", SearchOption.AllDirectories)); //grab child folders
            for (var i = 0; i < foldersPath.Count; i++)
            {
                if (foldersPath[i].EndsWith(@"\Sets"))
                {
                    foldersPath.RemoveAt(i--);
                    continue;
                }
                if (Directory.GetFiles(foldersPath[i], "*.png").Length == 0)
                {
                    foldersPath.RemoveAt(i--);
                }
            }
            if (foldersPath.Count == 0)
            {
                foldersPath.Add(originalPath);
            }

            return foldersPath;
        }

        public static List<string> Get_Cards_From_Path(string originalPath)
        {
            var choosen = new List<string>();
            var paths = new List<string>();
            if (Directory.Exists(originalPath))
            {
                paths.Add(originalPath);
                paths.AddRange(Directory.GetDirectories(originalPath, "*", SearchOption.AllDirectories)); //grab child folders
            }
            //step through each folder and grab files
            foreach (var path in paths)
            {
                if (path.Contains("BadCardData") || path.Contains("MissingMods") || path.Contains("OutdatedMods"))
                {
                    continue;
                }
                var files = Directory.GetFiles(path, "*.png");
                choosen.AddRange(files);
            }
            return choosen;
        }
    }
}


