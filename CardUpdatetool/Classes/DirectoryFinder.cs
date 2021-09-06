using System.Collections.Generic;
using System.IO;

namespace CardUpdateTool
{
    static class DirectoryFinder
    {
        public static List<string> Grab_All_Directories(string OriginalPath)
        {
            var FoldersPath = new List<string>
            {
                OriginalPath
            };
            FoldersPath.AddRange(Directory.GetDirectories(OriginalPath, "*", SearchOption.AllDirectories)); //grab child folders
            for (var i = 0; i < FoldersPath.Count; i++)
            {
                if (FoldersPath[i].EndsWith(@"\Sets"))
                {
                    FoldersPath.RemoveAt(i--);
                    continue;
                }
                if (Directory.GetFiles(FoldersPath[i], "*.png").Length == 0)
                {
                    FoldersPath.RemoveAt(i--);
                }
            }
            if (FoldersPath.Count == 0)
            {
                FoldersPath.Add(OriginalPath);
            }

            return FoldersPath;
        }

        public static List<string> Get_Cards_From_Path(string OriginalPath)
        {
            var Choosen = new List<string>();
            var Paths = new List<string>();
            if (Directory.Exists(OriginalPath))
            {
                Paths.Add(OriginalPath);
                Paths.AddRange(Directory.GetDirectories(OriginalPath, "*", SearchOption.AllDirectories)); //grab child folders
            }
            //step through each folder and grab files
            foreach (var path in Paths)
            {
                if (path.Contains("BadCardData") || path.Contains("MissingMods") || path.Contains("OutdatedMods"))
                {
                    continue;
                }
                var files = Directory.GetFiles(path, "*.png");
                Choosen.AddRange(files);
            }
            return Choosen;
        }
    }
}


