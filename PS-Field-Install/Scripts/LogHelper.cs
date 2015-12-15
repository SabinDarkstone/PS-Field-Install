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
			TRACE
		};

		public static string dropboxFolder = "/Logs";
		private static string localFolder = @"Temp\Logs";
		private static string filename = "";

		private static string date = "";

		private static FileStream logFile;

		/// <summary>
		/// Gets the current date and time to be used by the logger
		/// </summary>
		public static void GetDateAndTime() {
			date = DateTime.Now.Date.ToShortDateString();
			ConvertDate();
		}


		/// <summary>
		/// Converts the date to a usable format for file paths
		/// </summary>
		private static void ConvertDate() {
			var oldDate = date.Split('/');
			var newDate = "";

			newDate += oldDate[0] + "-" + oldDate[1] + "-" + oldDate[2];

			date = newDate;
		}

		/// <summary>
		/// Checks for a folder with the current date and creates one if not found
		/// </summary>
		/// <returns></returns>
		private static async Task CheckForFolder() {
			var contents = await DropboxHelper.GetFolderContents(dropboxFolder);
			bool isFound = false;

			foreach (var item in contents.Entries.Where(i => i.IsFolder)) {
				if (item.Name == date.ToString()) {
					// Matching folder found
					isFound = true;
				}
			}

			if (isFound) {
				dropboxFolder += "/" + date;
			} else {
				DropboxHelper.AddFolder(dropboxFolder, date);
				dropboxFolder = dropboxFolder + "/" + date;
			}
		}

		/// <summary>
		/// Checks to see what the last log file was and increments the number by one
		/// </summary>
		/// <returns></returns>
		private static async Task CheckLogFile() {
			var contents = await DropboxHelper.GetFolderContents(dropboxFolder);

			if (contents.Entries.Count == 0) {
				await CreateLogFile(1);
			} else {
				int logNum = 0;
				if (contents.Entries.Count != 0) {
					foreach (var item in contents.Entries.Where(i => i.IsFile)) {
						var currLogNum = item.Name.ToString().Substring(item.Name.ToString().IndexOf('g') + 1);
						currLogNum = currLogNum.Substring(0, currLogNum.IndexOf('.'));
						if (int.Parse(currLogNum) >= logNum) {
							logNum = int.Parse(currLogNum);
						}
					}
				}
				await CreateLogFile(++logNum);
			}
		}

		/// <summary>
		/// Saves a blank log file with the correct filename
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		private static async Task CreateLogFile(int number) {
			filename = "PSFIT_Log" + number + ".txt";
			Directory.CreateDirectory(TextTools.MyRelativePath(localFolder));
			logFile = File.Create(TextTools.MyRelativePath(localFolder + @"\" + filename));
			logFile.Close();
			await DropboxHelper.SendFileToDropbox(TextTools.MyRelativePath(localFolder + @"\" + filename), dropboxFolder, filename);
		}

		/// <summary>
		/// Prepares the log file, to be called before anything in done with the log file
		/// </summary>
		/// <returns>Task</returns>
		public static async Task PrepSessionLog() {
			await CheckForFolder();
			await CheckLogFile();

			await WriteLineToLog(LogLevels.INFO, "----- Beginning of log file -----");
		}

		/// <summary>
		/// Write a line to the log file from given objects and log level
		/// </summary>
		/// <param name="level">The log level to be displayed given by the enum</param>
		/// <param name="message">Object to convert to string for log file</param>
		/// <returns></returns>
		private static async Task WriteLineToLog(LogLevels level, object message) {
			var time = DateTime.Now.TimeOfDay.ToString().Substring(0, DateTime.Now.TimeOfDay.ToString().IndexOf('.'));
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

		/// <summary>
		/// Uploads the log file to dropbox
		/// </summary>
		/// <returns></returns>
		public static async Task UploadLog() {
			await DropboxHelper.SendFileToDropbox(TextTools.MyRelativePath(localFolder + @"\" + filename), dropboxFolder, filename);
		}

		public static class Log {

			/// <summary>
			/// INFO level logging
			/// </summary>
			/// <param name="message"></param>
			public static async void Info(object message) {
				await WriteLineToLog(LogLevels.INFO, message);
			}

			/// <summary>
			/// DEBUG level logging
			/// </summary>
			/// <param name="message"></param>
			public static async void Debug(object message) {
				await WriteLineToLog(LogLevels.DEBUG, message);
			}

			/// <summary>
			/// WARNING level logging
			/// </summary>
			/// <param name="message"></param>
			public static async void Warning(object message) {
				await WriteLineToLog(LogLevels.WARNING, message);
			}

			/// <summary>
			/// ERROR level logging
			/// </summary>
			/// <param name="message"></param>
			public static async void Error(object message) {
				await WriteLineToLog(LogLevels.ERROR, message);
			}

			/// <summary>
			/// FATAL level logging
			/// </summary>
			/// <param name="message"></param>
			public static async void Fatal(object message) {
				await WriteLineToLog(LogLevels.FATAL, message);
			}

			public static async void Trace(object message) {
				await WriteLineToLog(LogLevels.TRACE, message);
			}

		}

	}
}
