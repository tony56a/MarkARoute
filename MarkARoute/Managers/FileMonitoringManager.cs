using ColossalFramework;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MarkARoute.Managers
{
    class FileMonitoringManager
    {
        FileSystemWatcher watcher;

        private static FileMonitoringManager instance;

        public static FileMonitoringManager Instance()
        {
            if( instance == null)
            {
                instance = new FileMonitoringManager();
            }

            return instance;
        }

        public void startWatcher()
        {
            watcher = new FileSystemWatcher();
            /*watcher.Path = FileUtils.GetAltPath(FileUtils.TEXTURES);
            watcher.Created += Watcher_Created;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;*/
        }

        public void stopWatcher()
        {
            //watcher.EnableRaisingEvents = false;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {

            string file = e.FullPath;
            string[] splitValues = file[0] == Path.DirectorySeparatorChar ? file.Substring(1).Split(Path.DirectorySeparatorChar) : file.Split(Path.DirectorySeparatorChar);
            string fileName = splitValues[splitValues.Length - 1];
            string directoryName = splitValues[splitValues.Length - 2];

            string[] fileVal = fileName.Split('.');
            string fileKey = fileVal[0];

            if (fileVal.Length > 1)
            {
                SimulationManager.instance.AddAction("addTexture", () => {
                    LoggerUtils.Log(fileVal[1]);
                    LoggerUtils.Log(string.Format("New texture addition success? {0}", SpriteUtils.AddTexture(file, directoryName, fileKey)));
                });
                
            }

        }
    }
}
