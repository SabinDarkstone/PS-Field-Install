using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dropbox.Api.Files;

namespace PS_Field_Install.Scripts {
	public static class ImageHandler {

		/* UNDONE
		private static string imageDatabasePath = TextTools.MyRelativePath(@"StoredData\Images.xml");

		public static BitmapImage XMLToBitmap(string product) {
			DataSet data = new DataSet();
			data.ReadXml(imageDatabasePath);

			DataRow[] records = data.Tables[0].Select("Name='" + product + "'");

			if (records != null) {
				var imageData = records[0]["Image"];
				BitmapImage img = null;
				byte[] bitmapBytes = Convert.FromBase64String(imageData.ToString());
				using (MemoryStream ms = new MemoryStream(bitmapBytes)) {
					img.StreamSource = ms;
				}
				return img;
			}

			return null;
		}

		public static async void BitmapToXML(string localPath, string productName) {
			Bitmap bmp = new Bitmap(localPath);
			string image = null;
			using (MemoryStream ms = new MemoryStream()) {
				bmp.Save(ms, ImageFormat.Png);
				byte[] bitmapBytes = ms.GetBuffer();
				image = Convert.ToBase64String(bitmapBytes, Base64FormattingOptions.InsertLineBreaks);
			}

			DataSet data = new DataSet();

			try {
				data.ReadXml(imageDatabasePath);
			} catch (Exception) {
				if (data == null || data.Tables[0] == null) {
					data.Tables.Add();
				}

				data.Tables.Add();

				data.Tables[0].Columns.Add("Image", typeof(string));
				data.Tables[0].Columns.Add("Name", typeof(string));
				data.Tables[0].AcceptChanges();
			}

			DataRow row = data.Tables[0].NewRow();

			if (data.Tables[0].Select("Name='" + productName + "'").Length != 1) {
				row["Image"] = image;
				row["Name"] = productName;
			} else {
				MessageBox.Show("Please check that the product image you are trying to add does not already exist.");
				return;
			}

			data.Tables[0].Rows.Add(row);

			data.Tables[0].AcceptChanges();
			data.AcceptChanges();

			UploadToDropBox(data);
		}

		private static async Task<byte[]> CheckDatabaseFile(string folder, string file) {
			using (var response = await DropboxHelper.client.Files.DownloadAsync(folder + "/" + file)) {
				byte[] fileData = await response.GetContentAsByteArrayAsync();

				MessageBox.Show("File Size: " + fileData.Length.ToString());

				return fileData;
			}
		}

		public static async Task<DataSet> LoadFileFromDropbox() {
			File.WriteAllBytes(imageDatabasePath, await CheckDatabaseFile("/Images", "ImageDatabase.xml"));

			try {
				DataSet data = new DataSet();
				data.ReadXml(imageDatabasePath);
				return data;
			} catch {
				return null;
			}
		}

		public static async void UploadToDropBox(DataSet dataset) {
			dataset.WriteXml(imageDatabasePath);

			byte[] data = File.ReadAllBytes(imageDatabasePath);

			string rev = await SendToDropbox("/Images", "ImageDatabase.xml", Encoding.UTF8.GetChars(data));

			MessageBoxResult result = MessageBox.Show("Database updating complete!\n" + "Revision Code: " + rev);
		}

		private static async Task<string> SendToDropbox(string folder, string file, char[] content) {
			using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content))) {
				var updated = await DropboxHelper.client.Files.UploadAsync(folder + "/" + file, WriteMode.Overwrite.Instance, body: mem);

				return updated.Rev;
			}
		}
		*/



	}
}
