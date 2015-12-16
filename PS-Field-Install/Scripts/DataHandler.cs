using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;

namespace PS_Field_Install.Scripts {
	public static class DataHandler {

		public static DataSet productData;

		// public static StreamResourceInfo databaseFile = Application.GetResourceStream(new Uri(@"pack://siteoforigin:,,,/Data/PowerSearch.xml"));
		// private static string databaseFile = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\Data\PowerSearch.xml";
		// private static string databaseFile = new Uri("pack://siteoforigin:,,,/Data/PowerSearch.xml", UriKind.Absolute).AbsolutePath;
		private static string databaseFile = @"\\cdcsrvr1\Depts\PMD\COMMON\Emergency\Apps\Field Install App\Data\PowerSearch.xml";

		/* UNDONE Removed for testing
		public static async Task<bool> LoadDatabaseFromWeb() {
			// Download file for dropbox to local machine
			try {
				await DropboxHelper.GetFileFromDropbox(" / Data", "PowerSearch.xml", TextTools.MyRelativePath("Temp").ToString(), "PowerSearch.xml");
			} catch (Exception ex) {
				MessageBox.Show("An error occured while checking for the latest database file from remote server.\n" + ex.ToString());
				return false;
			}

			// Read xml file into dataset
			productData = new DataSet();
			ReadDatabase();

			if (!productData.Tables.Contains("Products")) {
				productData.Tables.Add("Products");
				// productData.Tables["Products"].AcceptChanges();
				// productData.AcceptChanges();
				SaveDatabase();

				productData.Tables["Products"].Columns.Add("CICodes", typeof(string));
				productData.Tables["Products"].Columns.Add("Descriptions", typeof(string));
				productData.Tables["Products"].Columns.Add("Power_Sentry_Solutions", typeof(string));
				productData.Tables["Products"].Columns.Add("Mounting_Options", typeof(string));
				productData.Tables["Products"].Columns.Add("Wiring_Diagrams", typeof(string));
				productData.Tables["Products"].Columns.Add("Comments", typeof(string));
			}

			// MessageBox.Show("Database successfully loaded.\n\nNumber of tables found: " + productData.Tables.Count + "\nNumber of records found: " + productData.Tables[0].Rows.Count);
			return true;
		}
		*/

		public static void LoadDatabaseFromLocal() {
			// Read xml file into dataset
			productData = new DataSet();
			ReadDatabase();

			if (!productData.Tables.Contains("Products")) {
				productData.Tables.Add("Products");
				// productData.Tables["Products"].AcceptChanges();
				// productData.AcceptChanges();
				SaveDatabase();

				productData.Tables["Products"].Columns.Add("CICodes", typeof(string));
				productData.Tables["Products"].Columns.Add("Descriptions", typeof(string));
				productData.Tables["Products"].Columns.Add("Power_Sentry_Solutions", typeof(string));
				productData.Tables["Products"].Columns.Add("Mounting_Options", typeof(string));
				productData.Tables["Products"].Columns.Add("Wiring_Diagrams", typeof(string));
				productData.Tables["Products"].Columns.Add("Comments", typeof(string));
			}

			// MessageBox.Show("Database successfully loaded.\n\nNumber of tables found: " + productData.Tables.Count + "\nNumber of records found: " + productData.Tables[0].Rows.Count);
		}

		public static void ReadDatabase() {
			// productData.ReadXml(TextTools.MyRelativePath(@"Temp\PowerSearch.xml"));
			productData.ReadXml(databaseFile);
		}

		public static void SaveDatabase() {
			for (int i = 0; i < productData.Tables.Count; i++) {
				productData.Tables[i].AcceptChanges();
			}

			productData.AcceptChanges();
			// productData.WriteXml(TextTools.MyRelativePath(@"Temp\PowerSearch.xml"));
			productData.WriteXml(databaseFile);
		}

		public static bool CheckFileExists() {
			// return System.IO.File.Exists(TextTools.MyRelativePath(@"Temp\PowerSearch.xml"));
			return true;
		}

		/* UNDONE Removed for testing
		public static async Task DownloadImages() {
			var filesPowerSentry = await DropboxHelper.GetFolderContents("/Images/Power Sentry");
			var filesLithonia = await DropboxHelper.GetFolderContents("/Images/Lithonia");

			foreach (var item in filesPowerSentry.Entries.Where(i => i.IsFile)) {
				if (File.Exists(TextTools.MyRelativePath(@"Temp\Power Sentry\" + item.Name))) {
					continue;
				}
				await DropboxHelper.GetFileFromDropbox("/Images/Power Sentry", item.Name, TextTools.MyRelativePath(@"Temp\Power Sentry"), item.Name);
			}

			foreach (var item2 in filesLithonia.Entries.Where(j => j.IsFile)) {
				if (File.Exists(TextTools.MyRelativePath(@"Temp\Lithonia\" + item2.Name))) {
					continue;
				}
				await DropboxHelper.GetFileFromDropbox("/Images/Lithonia", item2.Name, TextTools.MyRelativePath(@"Temp\Lithonia"), item2.Name);
			}

			// MessageBox.Show("Finished updating image database!");
		}
		*/

		/* UNDONE
		private static bool CheckTableForErrors() {
			if (productData.Tables.Count == 0) {
				// Add a new table
				productData.Tables.Add("Products");
				CheckTableForErrors();
			}

			if (!productData.Tables[0].Equals(productData.Tables["Products"])) {
				// Remove all tables and try again
				for (int i = 0; i < productData.Tables.Count; i++) {
					productData.Tables.Remove(productData.Tables[0]);
				}
				CheckTableForErrors();
			}

			if (!productData.Tables["Products"].Columns.Contains("pCICode") &&
						productData.Tables["Products"].Columns.Contains("pDescription") &&
						productData.Tables["Products"].Columns.Contains("pPowerSentrySolution") &&
						productData.Tables["Products"].Columns.Contains("pMounting") &&
						productData.Tables["Products"].Columns.Contains("pWiring") &&
						productData.Tables["Products"].Columns.Contains("pComment")) {
				// Add in the correct columns
				productData.Tables["Products"].Columns.Clear();
				productData.Tables["Products"].Columns.Add("pCICode", typeof(string));
				productData.Tables["Products"].Columns.Add("pDescription", typeof(string));
				productData.Tables["Products"].Columns.Add("pPowerSentrySolution", typeof(string));
				productData.Tables["Products"].Columns.Add("pMounting", typeof(string));
				productData.Tables["Products"].Columns.Add("pWiring", typeof(string));
				productData.Tables["Products"].Columns.Add("pComment", typeof(string));
				
			}
			productData.Tables["Products"].AcceptChanges();
			productData.AcceptChanges();
			return true;
		}
		*/

	}
}