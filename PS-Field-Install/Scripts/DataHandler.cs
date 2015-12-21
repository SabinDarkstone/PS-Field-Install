using System.Collections.Generic;
using System.Data;
using System.IO;

namespace PS_Field_Install.Scripts {
	public static class DataHandler {

		public static DataSet productData;
		public static List<string> resultsOrder = new List<string>();

		public static string CICodeColumn = "";
		public static string DescriptionColumn = "";
		public static string PowerSentryColumn = "";

		public static void LoadDatabaseFromLocal() {
			// Read xml file into dataset
			productData = new DataSet();
			ReadDatabase();

			if (!productData.Tables.Contains(Settings.DataTableName)) {
				productData.Tables.Add(Settings.DataTableName);
				SaveDatabase();
			}

			ReorderColumns();
		}

		public static void ReorderColumns() {
			resultsOrder.Clear();
			resultsOrder = TextTools.ReadFileLines(Settings.ResultsDisplayOrder, true);
		}

		public static void AddSavedColumns() {
			ClearDatabase();

			StreamReader sr = new StreamReader(Settings.ResultsDisplayOrder);
			string line;
			while ((line = sr.ReadLine()) != null) {
				line = line.Trim().Replace(" ", "_");
				productData.Tables[Settings.DataTableName].Columns.Add(line, typeof(string));
				resultsOrder.Add(line);
			}
			sr.Close();

			productData.Tables[Settings.DataTableName].AcceptChanges();
			productData.AcceptChanges();
		}

		public static void ClearDatabase() {
			productData.Tables[Settings.DataTableName].Columns.Clear();
			productData.Tables[Settings.DataTableName].Clear();
			productData.Tables[Settings.DataTableName].AcceptChanges();
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

		public static void GetSearchColumns() {
			var list = TextTools.ReadFileLines(Settings.ResultsDisplayOrder, true);
			foreach (var item in list) {
				if (item.Contains(Settings.CICodeTest)) {
					CICodeColumn = item;
				}

				if (item.Contains(Settings.DescriptionTest)) {
					DescriptionColumn = item;
				}
			}
		}

	}
}