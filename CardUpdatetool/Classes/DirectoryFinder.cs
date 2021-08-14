using System.Collections.Generic;
using System.IO;

namespace CardUpdateTool
{
    static class DirectoryFinder
    {
        public static List<string> Grab_All_Directories(string OriginalPath)
        {
            List<string> FoldersPath = new List<string>
            {
                OriginalPath
            };
            FoldersPath.AddRange(Directory.GetDirectories(OriginalPath, "*", SearchOption.AllDirectories)); //grab child folders
            for (int i = 0; i < FoldersPath.Count; i++)
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
            List<string> Choosen = new List<string>();
            List<string> Paths = new List<string>();
            if (Directory.Exists(OriginalPath))
            {
                Paths.Add(OriginalPath);
                Paths.AddRange(Directory.GetDirectories(OriginalPath, "*", SearchOption.AllDirectories)); //grab child folders
            }
            //step through each folder and grab files
            foreach (string path in Paths)
            {
                if (path.Contains("BadCardData") || path.Contains("MissingMods") || path.Contains("OutdatedMods"))
                {
                    continue;
                }
                string[] files = Directory.GetFiles(path, "*.png");
                Choosen.AddRange(files);
            }
            return Choosen;
        }
    }
}


