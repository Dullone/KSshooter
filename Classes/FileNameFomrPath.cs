using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSshooter.Classes
{
    static class FileNameFromPath
    {
        public static string GetFileNameFromPath (string filename)
        {
            int nameStart = 0;
            int nameEnd;

            nameStart = filename.LastIndexOf('\\') + 1;
            nameEnd = filename.LastIndexOf('.');
            return filename.Substring(nameStart, nameEnd - nameStart);
        }

    }
}
