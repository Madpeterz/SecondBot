using System;
using System.Collections.Generic;
using System.Linq;
using OpenMetaverse;
using BetterSecondBotShared.Static;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.bottypes;
using Microsoft.VisualBasic.CompilerServices;
using System.IO;
using System.IO.Compression;

namespace BSB.bottypes
{
    public abstract class Log2FileBot : ActionsAutoAcceptBot
	{
		// stolen from: https://gist.github.com/Kashkovsky/eba3c91cc9d64e705f18ba9a272f7d92
		protected string LogFolderName = "logs";
        protected string LogFileName = "secondbot.log";
		private readonly int _logChunkSize = 1000000;
		private readonly int _logChunkMaxCount = 5;
		private readonly int _logArchiveMaxCount = 10;
		private readonly int _logCleanupPeriod = 7;

		public override void Log2File(string message, ConsoleLogLogLevel Level)
        {
            if(myconfig.Log2File_Enable == true)
            {
                if(myconfig.Log2File_Level >= (int)Level)
                {
                    CreateLog(message);
                }
            }
			base.Log2File(message, Level);
        }
        protected void CreateLog(string message)
        {
			var logFolderPath = LogFolderName;

			if (!Directory.Exists(logFolderPath))
			{
				Directory.CreateDirectory(logFolderPath);
			}

            var logFilePath = Path.Combine(logFolderPath, LogFileName);

            Rotate(logFilePath);

			try
			{
				using var sw = File.AppendText(logFilePath);
				sw.WriteLine(message);
			}
			catch
			{

			}
        }

		private void Rotate(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return;
			}

			var fileInfo = new FileInfo(filePath);
			if (fileInfo.Length < _logChunkSize)
			{
				return;
			}

			var fileTime = DateTime.Now.ToString("dd_MM_yy_h_m_s");
			var rotatedPath = filePath.Replace(".log", $".{fileTime}");
			File.Move(filePath, rotatedPath);

			var folderPath = Path.GetDirectoryName(rotatedPath);
			var logFolderContent = new DirectoryInfo(folderPath).GetFileSystemInfos();

			var chunks = logFolderContent.Where(x => !x.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase));

			if (chunks.Count() <= _logChunkMaxCount)
			{
				return;
			}

			var archiveFolderInfo = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(rotatedPath), $"{LogFolderName}_{fileTime}"));

			foreach (var chunk in chunks)
			{
				Directory.Move(chunk.FullName, Path.Combine(archiveFolderInfo.FullName, chunk.Name));
			}

			ZipFile.CreateFromDirectory(archiveFolderInfo.FullName, Path.Combine(folderPath, $"{LogFolderName}_{fileTime}.zip"));
			Directory.Delete(archiveFolderInfo.FullName, true);

			var archives = logFolderContent.Where(x => x.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)).ToArray();

			if (archives.Count() <= _logArchiveMaxCount)
			{
				return;
			}

			var oldestArchive = archives.OrderBy(x => x.CreationTime).First();
			var cleanupDate = oldestArchive.CreationTime.AddDays(_logCleanupPeriod);
			if (DateTime.Compare(cleanupDate, DateTime.Now) <= 0)
			{
				foreach (var file in logFolderContent)
				{
					file.Delete();
				}
			}
			else
			{
				File.Delete(oldestArchive.FullName);
			}

		}
	}
}
