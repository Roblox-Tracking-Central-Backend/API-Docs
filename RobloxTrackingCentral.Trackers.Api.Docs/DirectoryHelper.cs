using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Api.Docs
{
    internal static class DirectoryHelper
    {
        /// <summary>
        /// https://stackoverflow.com/a/8714329
        /// </summary>
        /// <param name="path"></param>
        public static void ForceDelete(string path)
        {
            var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
                info.Attributes = FileAttributes.Normal;

            directory.Delete(true);
        }
    }
}
