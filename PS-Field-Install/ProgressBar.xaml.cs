using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Data;
using Excel = Microsoft.Office.Interop.Excel;
using PS_Field_Install.Scripts;
using System.IO;

namespace PS_Field_Install {
	/// <summary>
	/// Interaction logic for ProgressBar.xaml
	/// </summary>
	public partial class ProgressBar : Window {

		#region Objects
		Hashtable columns;
		string filename;
		#endregion

		#region Progress Bar Handling
		public ProgressBar(ref Hashtable columnAllocation, string filename, int rowCount) {
			// LogHelper.Log.Debug("ProgressBar.Constructor");
			InitializeComponent();

			columns = columnAllocation;
			this.filename = filename;
			pbStatus.Maximum = rowCount;
		}

		private void Window_ContentRendered(object sender, EventArgs e) {
			// LogHelper.Log.Debug("ProgressBar.Window_ContentRendered(sender, e)");
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

		public async void worker_DoWork(object sender, DoWorkEventArgs e) {
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

			table.Columns.Clear();
			table.Rows.Clear();
			table.Clear();
			table.AcceptChanges();

			// Assign columns to table
			foreach (DictionaryEntry de in columns) {
				table.Columns.Add(de.Key.ToString());
				MessageBox.Show(de.Key.ToString());
			}
			table.AcceptChanges();

			for (rCnt = 2; rCnt < range.Rows.Count + 1; rCnt++) {
				DataRow dr = table.NewRow();
				for (cCnt = 1; cCnt < range.Columns.Count + 1; cCnt++) {
					var myCat = columns[(string)(range.Cells[1, cCnt] as Excel.Range).Value];
				}

				table.Rows.Add(dr);

				(sender as BackgroundWorker).ReportProgress(rCnt);
				this.Dispatcher.Invoke((Action)(() => {
					txtCurrRecordCICode.Text = dr["CI Code"].ToString();
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

			/* UNDONE Removed for testing
			if (!Directory.Exists(TextTools.MyRelativePath(@"Temp\"))) {
				Directory.CreateDirectory(TextTools.MyRelativePath(@"Temp\"));
			}
			*/

			DataHandler.SaveDatabase();

			// string rev = await DropboxHelper.SendFileToDropbox(TextTools.MyRelativePath(@"Temp\PowerSearch.xml"), "/Data", "PowerSearch.xml");

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

		private async void Window_Unloaded(object sender, RoutedEventArgs e) {
			// await LogHelper.UploadLog();
		}
	}
}
