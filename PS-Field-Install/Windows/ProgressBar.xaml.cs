using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Data;
using Excel = Microsoft.Office.Interop.Excel;
using PS_Field_Install.Scripts;
using System.IO;
using System.Collections.Generic;

namespace PS_Field_Install {
	/// <summary>
	/// Interaction logic for ProgressBar.xaml
	/// </summary>
	public partial class ProgressBar : Window {

		#region Objects
		private Hashtable columns;
		private string filename;
		#endregion

		#region Progress Bar Handling
		public ProgressBar(Hashtable columnAllocation, string filename, int rowCount) {
			InitializeComponent();

			columns = columnAllocation;
			this.filename = filename;
			pbStatus.Maximum = rowCount;
		}

		private void Window_ContentRendered(object sender, EventArgs e) {
			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += worker_DoWork;
			worker.ProgressChanged += worker_ProgressChanged;

			worker.RunWorkerAsync();
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			pbStatus.Value = e.ProgressPercentage;
		}
		#endregion

		public void worker_DoWork(object sender, DoWorkEventArgs e) {
			DataTable table = DataHandler.productData.Tables["Products"];

			#region Excel Prep
			Excel.Application excelApp;
			Excel.Workbook excelWorkbook;
			Excel.Worksheet excelWorksheet;
			Excel.Range range;

			int rCnt = 0;
			int cCnt = 0;

			excelApp = new Excel.Application();
			excelWorkbook = excelApp.Workbooks.Open(filename);
			excelWorksheet = (Excel.Worksheet)excelWorkbook.Worksheets.get_Item(1);
			range = excelWorksheet.UsedRange;
			#endregion

			// Assign columns to table
			DataHandler.AddSavedColumns();

			var savedCats = new List<string>();
			var columnsNeeded = new List<string>();
			foreach (DictionaryEntry de in columns) {
				if (table.Columns.Contains(de.Value.ToString().Replace(" ", "_"))) {
					savedCats.Add(de.Value.ToString());
					columnsNeeded.Add(de.Key.ToString());

					// MessageBox.Show("Category: " + de.Value.ToString() + "\nColumn: " + de.Key.ToString());

					if (de.Value.ToString().Contains("CI")) {
						DataHandler.CICodeColumn = de.Value.ToString();
					}

					if (de.Value.ToString().Contains("Desc")) {
						DataHandler.DescriptionColumn = de.Value.ToString();
					}
				}
			}

			for (rCnt = 2; rCnt < range.Rows.Count + 1; rCnt++) {
				DataRow dr = table.NewRow();
				for (cCnt = 1; cCnt < range.Columns.Count + 1; cCnt++) {
					var thisHeading = (range.Cells[1, cCnt] as Excel.Range).Value;
					var thisCategory = columns[thisHeading].ToString().Replace(" ", "_");
					var thisCell = (range.Cells[rCnt, cCnt] as Excel.Range).Value;

					if (thisCell == null) {
						continue;
					} else if (thisHeading == null) {
						continue;
					}

					// MessageBox.Show("Heading: " + thisHeading + "\nCategory: " + thisCategory + "\nCell: " + thisCell);

					thisHeading = thisHeading.ToString();
					thisCategory = thisCategory.ToString();
					thisCell = thisCell.ToString();

					if (thisCategory == "Not Used") {
						continue;
					} else if (columnsNeeded.Contains(thisHeading)) {
						if (thisCell.Contains("(CI-") && thisCell.Length > 7) {
							// Cell contains a CI code with other information.  CI code needs to be removed for use in the database
							thisCell = thisCell.Substring(0, thisCell.IndexOf("(CI") - 1);
							dr[thisCategory] = thisCell;
						} else if (thisCell.Contains("//*")) {
							// Cell contains a CI code only, remove the asterisk and keep the rest
							thisCell = thisCell.Substring(1, thisCell.Length);
							dr[thisCategory] = thisCell;
						} else if (thisCell == "1") {
							// Cell contains only the number one, use the column heading as the value for database
							dr[thisCategory] += thisHeading + ", ";
						} else {
							// If all else fails, the cell is likely a comment cell or another type of cell that contains
							// information that will be in the database, not the heading
							dr[thisCategory] = thisCell;
						}
					} else {
						// MessageBox.Show("Cell not added with column of: " + thisHeading + " and a category of: " + thisCategory);
					}
				}

				table.Rows.Add(dr);

				(sender as BackgroundWorker).ReportProgress(rCnt);
				this.Dispatcher.Invoke((Action)(() => {
					txtCurrRecordCICode.Text = dr[DataHandler.CICodeColumn.ToString().Replace(" ", "_")].ToString();
				}));
			} // End reading excel file

			this.Dispatcher.Invoke(() => {
				Close();
			});

			table.AcceptChanges();  // Save the table to dataset

			MessageBox.Show("Data import complete.\nPlease wait while the data is verified and formatted");

			#region Remove Excess commas
			string checkCell;
			for (int row = 0; row < table.Rows.Count; row++) {
				foreach (DataColumn col in table.Columns) {
					checkCell = table.Rows[row][col].ToString();
					if (checkCell.EndsWith(", ")) {
						table.Rows[row][col] = checkCell.Substring(0, checkCell.Length - 2);
					}
				}
			}
			table.AcceptChanges();
			#endregion

			#region Do grammar
			for (int row = 0; row < table.Rows.Count; row++) {
				foreach (DataColumn col in table.Columns) {
					checkCell = table.Rows[row][col].ToString();
					if (checkCell.Contains(",")) {
						table.Rows[row][col] = TextTools.GrammerifyList(checkCell, "and");
					}
				}
			}
			#endregion

			table.AcceptChanges();
			DataHandler.SaveDatabase();

			MessageBoxResult result = MessageBox.Show("Database updating complete!");

			#region Close excel
			excelWorkbook.Close(true, null, null);
			excelApp.Quit();

			ReleaseObject(excelWorksheet);
			ReleaseObject(excelWorkbook);
			ReleaseObject(excelApp);
			#endregion
		}

		private void ReleaseObject(object obj) {
			// LogHelper.Log.Debug("ProgressBar.ReleaseObject(obj)");
			try {
				System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
				obj = null;
			} catch (Exception ex) {
				obj = null;
				MessageBox.Show("Unable to release the Object " + ex.ToString());
			} finally {
				GC.Collect();
			}
		}
	}
}
