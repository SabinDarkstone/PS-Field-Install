using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PS_Field_Install.Scripts {
	public static class LogHelper {

		public enum LogLevels {
			DEBUG,
			INFO,
			WARNING,
			ERROR,
			FATAL,
			EXCEPTION
		};

		private static string dropboxFolder = "/Logs";
		private static string localFolder = @"Temp\Logs";
		private static string filename = "";

		private static string date = "";
		private static string startTime = "";

		private static FileStream logFile;

		public static void GetDateAndTime() {
			date = DateTime.Now.Date.ToShortDateString();
			ConvertDate();

			startTime = DateTime.Now.TimeOfDay.ToString();
		}

		private static void ConvertDate() {
			var oldDate = date.Split('/');
			var newDate = "";

			newDate += oldDate[0] + "-" + oldDate[1] + "-" + oldDate[2];

			date = newDate;
		}

		private static async Task CheckForFolder() {
			var contents = await DropboxHelper.GetFolderContents(dropboxFolder);

			if (contents.Entries.Count == 0) {
				DropboxHelper.AddFolder(dropboxFolder, date);
				dropboxFolder = dropboxFolder + "/" + date;
				return;
			}

			foreach (var item in contents.Entries.Where(i => i.IsFolder)) {
				if (item.Name == date.ToString()) {
					// Matching folder found
					dropboxFolder = dropboxFolder + "/" + date;
					return;
				} else {
					DropboxHelper.AddFolder(dropboxFolder, date);
				}
			}

			dropboxFolder = dropboxFolder + "/" + date;
		}

		private static async Task CheckLogFile() {
			var contents = await DropboxHelper.GetFolderContents(dropboxFolder);

			if (contents.Entries.Count == 0) {
				await CreateLogFile(1);
			} else {
				int logNum = 0;
				foreach (var item in contents.Entries.Where(i => i.IsFile)) {
					var currLogNum = item.Name.ToString().Substring(item.Name.ToString().IndexOf('g') + 1);
					currLogNum = currLogNum.Substring(0, currLogNum.IndexOf('.'));
					if (int.Parse(currLogNum) >= logNum) {
						logNum = int.Parse(currLogNum);
					}
				}
				await CreateLogFile(++logNum);
			}
		}

		private static async Task CreateLogFile(int number) {
			filename = "PSFIT_Log" + number + ".txt";
			Directory.CreateDirectory(TextTools.MyRelativePath(localFolder));
			logFile = File.Create(TextTools.MyRelativePath(localFolder + @"\" + filename));
			logFile.Close();
			await DropboxHelper.SendFileToDropbox(TextTools.MyRelativePath(localFolder + @"\" + filename), dropboxFolder, filename);
		}

		public static async Task PrepSessionLog() {
			await CheckForFolder();
			await CheckLogFile();

			await WriteLineToLog(LogLevels.INFO, "----- Beginning of log file -----");
		}

		private static async Task WriteLineToLog(LogLevels level, object message) {
			var time = startTime.Substring(0, startTime.IndexOf('.'));
			var data = ("[" + time + "] " +"[" + level.ToString() + "]: " + message.ToString());

			try {
				// logFile = File.OpenWrite(TextTools.MyRelativePath(localFolder + @"\" + filename));
				// await logFile.WriteAsync(data, 0, data.Length);
				StreamWriter stream = File.AppendText(TextTools.MyRelativePath(localFolder + @"\" + filename));
				await stream.WriteLineAsync(data);
				stream.Close();
				// await DropboxHelper.SendFileToDropbox(TextTools.MyRelativePath(localFolder + @"\" + filename), dropboxFolder, filename);
			} catch (Exception ex) {
				System.Windows.MessageBox.Show(ex.Message);
			}
		}

		public static async Task UploadLog() {
			await DropboxHelper.SendFileToDropbox(TextTools.MyRelativePath(localFolder + @"\" + filename), dropboxFolder, filename);
		}

		public static class Log {

			public static async void Info(object message) {
				await WriteLineToLog(LogLevels.INFO, message);
			}

			public static async void Debug(object message) {
				await WriteLineToLog(LogLevels.DEBUG, message);
			}

			public static async void Warning(object message) {
				await WriteLineToLog(LogLevels.WARNING, message);
			}

			public static async void Error(object message) {
				await WriteLineToLog(LogLevels.ERROR, message);
			}

			public static async void Fatal(object message) {
				await WriteLineToLog(LogLevels.FATAL, message);
			}

		}

	}
}
