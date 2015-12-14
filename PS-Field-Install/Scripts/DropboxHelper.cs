using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace PS_Field_Install.Scripts {
	public static class DropboxHelper {

		/// <summary>
		/// Client used for Dropbox API
		/// </summary>
		private static DropboxClient client = new DropboxClient(Token);

		/// <summary>
		/// Authorization Token for Power Sentry Field Installation dropbox app
		/// </summary>
		public const string Token = "Pxp-iufae2kAAAAAAAACftTrIqvxrZ2cVWRZdI3r043UmwkmGwYQPvNIdFnjIuJB";

		/// <summary>
		/// Uploaded a file to dropbox
		/// </summary>
		/// <param name="folder">The folder where the file will be uploaded to</param>
		/// <param name="file">The name of the file that will be uploaded</param>
		/// <param name="content">An array of chars that will be written to the file</param>
		/// <returns>Revision confirmation number</returns>
		public static async Task<string> SendCharsToDropbox(string folder, string file, char[] content) {
			using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content))) {
				var updated = await client.Files.UploadAsync(folder + "/" + file, WriteMode.Overwrite.Instance, body: mem);

				return updated.Rev;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFolder"></param>
		/// <param name="sourceFile"></param>
		/// <param name="destFolder"></param>
		/// <param name="destFile"></param>
		/// <returns></returns>
		public static async Task<string> SendFileToDropbox(string sourceFolder, string sourceFile, string destFolder, string destFile) {
			StreamReader sr = new StreamReader(TextTools.MyRelativePath(sourceFolder + @"\" + sourceFile));

			var updated = await client.Files.UploadAsync(destFolder + "/" + destFile, WriteMode.Overwrite.Instance, body: sr.BaseStream);
			return updated.Rev;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="destFolder"></param>
		/// <param name="destFile"></param>
		/// <returns></returns>
		public static async Task<string> SendFileToDropbox(string sourcePath, string destFolder, string destFile) {
			StreamReader sr = new StreamReader(sourcePath);

			var updated = await client.Files.UploadAsync(destFolder + "/" + destFile, WriteMode.Overwrite.Instance, body: sr.BaseStream);
			sr.Close();
			return updated.Rev;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFolder"></param>
		/// <param name="sourceFile"></param>
		/// <returns></returns>
		public static async Task<byte[]> GetBytesFromDropbox(string sourceFolder, string sourceFile) {
			using (var response = await client.Files.DownloadAsync(sourceFolder + "/" + sourceFile)) {
				byte[] fileData = await response.GetContentAsByteArrayAsync();
				MessageBox.Show("File downloaded successfully.  Size of file: " + fileData.Length.ToString());

				return fileData;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFolder"></param>
		/// <param name="sourceFile"></param>
		/// <param name="destFolder"></param>
		/// <param name="destFile"></param>
		public static async Task GetFileFromDropbox(string sourceFolder, string sourceFile, string destFolder, string destFile) {
			using (var response = await client.Files.DownloadAsync(sourceFolder + "/" + sourceFile)) {
				var fileData = await response.GetContentAsStreamAsync();

				if (!Directory.Exists(TextTools.MyRelativePath(destFolder))) {
					Directory.CreateDirectory(TextTools.MyRelativePath(destFolder));
				}

				FileStream fileStream;

				if (File.Exists(TextTools.MyRelativePath(destFolder + @"\" + destFile))) {
					fileStream = File.OpenWrite(TextTools.MyRelativePath(destFolder + @"\" + destFile));
				} else {
					fileStream = File.Create(TextTools.MyRelativePath(destFolder + @"\" + destFile));
				}

                fileData.CopyTo(fileStream);
				fileStream.Close();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="destFolder"></param>
		/// <param name="destFile"></param>
		public static void CheckFileExists(string destFolder, string destFile) {
			if (!File.Exists(TextTools.MyRelativePath(destFolder + @"\" + destFile))) {
				File.Create(TextTools.MyRelativePath(destFolder + @"\" + destFile));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static async Task<ListFolderResult> GetFolderContents(string path) {
			return await client.Files.ListFolderAsync(path);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="name"></param>
		public static async void AddFolder(string path, string name) {
			await client.Files.CreateFolderAsync(path + "/" + name);
		}

	}
}
