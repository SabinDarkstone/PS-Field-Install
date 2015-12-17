using System.Data;
using System.IO;

namespace PS_Field_Install.Scripts {
	public static class DataHandler {

		public static DataSet productData;

		public static void LoadDatabaseFromLocal() {
			// Read xml file into dataset
			productData = new DataSet();
			ReadDatabase();

			if (!productData.Tables.Contains("Products")) {
				productData.Tables.Add("Products");
				SaveDatabase();

				StreamReader sr = new StreamReader(Settings.SavedCategories);
				string line;
				while ((line = sr.ReadLine()) != null) {
					productData.Tables["Products"].Columns.Add(line);
				}
				sr.Close();
			}

		}

		public static void ReadDatabase() {
			productData.ReadXml(Settings.DatabaseFile);
		}

		public static void SaveDatabase() {
			for (int i = 0; i < productData.Tables.Count; i++) {
				productData.Tables[i].AcceptChanges();
			}

			productData.AcceptChanges();
			productData.WriteXml(Settings.DatabaseFile);
		}

	}
}