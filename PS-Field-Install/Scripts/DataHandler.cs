using System.Collections.Generic;
using System.Data;
using System.IO;

namespace PS_Field_Install.Scripts {
	public static class DataHandler {

		public static DataSet productData;
		public static List<string> resultsOrder = new List<string>();
		public static string CICodeColumn = "";
		public static string DescriptionColumn = "";

		public static void LoadDatabaseFromLocal() {
			// Read xml file into dataset
			productData = new DataSet();
			ReadDatabase();

			if (!productData.Tables.Contains("Products")) {
				productData.Tables.Add("Products");
				SaveDatabase();
			}
		}

		public static void ReorderColumns() {
			resultsOrder.Clear();
			StreamReader sr = new StreamReader(Settings.ResultsDisplayOrder);
			string line;
			while ((line = sr.ReadLine()) != null) {
				line = line.Trim().Replace(" ", "_");
				resultsOrder.Add(line);
			}
			sr.Close();
		}

		public static void AddSavedColumns() {
			ClearDatabase();

			StreamReader sr = new StreamReader(Settings.ResultsDisplayOrder);
			string line;
			while ((line = sr.ReadLine()) != null) {
				line = line.Trim().Replace(" ", "_");
				productData.Tables["Products"].Columns.Add(line, typeof(string));
				resultsOrder.Add(line);
			}
			sr.Close();

			productData.Tables["Products"].AcceptChanges();
			productData.AcceptChanges();
		}

		public static void ClearDatabase() {
			productData.Tables["Products"].Columns.Clear();
			productData.Tables["Products"].Clear();
			productData.Tables["Products"].AcceptChanges();
			productData.AcceptChanges();

			resultsOrder.Clear();
		}

		public static void ReadDatabase() {
			try {
				productData.ReadXml(Settings.DatabaseFile);
			} catch { }
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