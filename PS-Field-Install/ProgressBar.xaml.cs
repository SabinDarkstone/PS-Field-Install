using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Data;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading.Tasks;
using PS_Field_Install.Scripts;
using System.IO;
using System.Text;
using Dropbox.Api.Files;

namespace PS_Field_Install {
	/// <summary>
	/// Interaction logic for ProgressBar.xaml
	/// </summary>
	public partial class ProgressBar : Window {

		#region Objects
		Hashtable columns;
		string filename;
		private enum ColTypes { DoNotUse, pCICode, pDescription, pPowerSentrySolution, pMounting, pWiring, pComment };
		#endregion

		#region Progress Bar Handling
		public ProgressBar(ref Hashtable columnAllocation, string filename, int rowCount) {
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

		async void worker_DoWork(object sender, DoWorkEventArgs e) {
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

			table.Rows.Clear();
			table.AcceptChanges();

			for (rCnt = 2; rCnt < range.Rows.Count + 1; rCnt++) {
				DataRow dr = table.NewRow();
				for (cCnt = 1; cCnt < range.Columns.Count + 1; cCnt++) {
					object currColType = columns[(string)(range.Cells[1, cCnt] as Excel.Range).Value];
					if (currColType.ToString() != ColTypes.DoNotUse.ToString()) {
						object currCell = (range.Cells[rCnt, cCnt] as Excel.Range).Value2;
						if (currCell != null) {
							#region Switch Stuff
							switch (currColType.ToString()) {
								case "pCICode":
									dr[ColTypes.pCICode.ToString()] = currCell.ToString();
									break;
								case "pDescription":
									var desc = currCell.ToString();
									desc = desc.Substring(0, currCell.ToString().IndexOf("(CI-") - 1);
									dr[ColTypes.pDescription.ToString()] = desc;
									break;
								case "pPowerSentrySolution":
									dr[ColTypes.pPowerSentrySolution.ToString()] += ((range.Cells[1, cCnt] as Excel.Range).Value2).ToString() + ", ";
									break;
								case "pMounting":
									dr[ColTypes.pMounting.ToString()] += ((range.Cells[1, cCnt] as Excel.Range).Value2).ToString() + ", ";
									break;
								case "pWiringDiagram":
									dr[ColTypes.pWiring.ToString()] += ((range.Cells[1, cCnt] as Excel.Range).Value2).ToString() + ", ";
									break;
								case "pComment":
									dr[ColTypes.pComment.ToString()] += currCell.ToString();
									break;
								default:
									break;
							}
							#endregion
						}
					}
				}
				table.Rows.Add(dr);

				(sender as BackgroundWorker).ReportProgress(rCnt);
				this.Dispatcher.Invoke((Action)(() => {
					txtCurrRecordCICode.Text = dr[ColTypes.pCICode.ToString()].ToString();
					// txtCurrRecordDescription.Text = dr[ColTypes.pDescription.ToString()].ToString().Substring(0, dr[ColTypes.pDescription.ToString()].ToString().IndexOf("(CI-") - 1);
				}));
			} // End reading excel file

			table.AcceptChanges();  // Save the table to dataset

			MessageBox.Show("Data import complete.\nPlease wait while the data is verified and formatted");

			#region Remove Excess commas
			string checkCell;
			for (int row = 0; row < table.Rows.Count; row++) {
				foreach (DataColumn col in table.Columns) {
					checkCell = table.Rows[row][col].ToString();
					// MessageBox.Show(checkCell);
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

			if (!Directory.Exists(TextTools.MyRelativePath(@"Temp\"))) {
				Directory.CreateDirectory(TextTools.MyRelativePath(@"Temp\"));
			}

			DataHandler.SaveDatabase();

			string rev = await DropboxHelper.SendFileToDropbox(TextTools.MyRelativePath(@"Temp\PowerSearch.xml"), "/Data", "PowerSearch.xml");

			MessageBoxResult result = MessageBox.Show("Database updating complete!\n" + "Revision Code: " + rev);

			#region Close excel
			excelWorkbook.Close(true, null, null);
			excelApp.Quit();

			ReleaseObject(excelWorksheet);
			ReleaseObject(excelWorkbook);
			ReleaseObject(excelApp);
			#endregion

			try {
				this.Dispatcher.Invoke(() => {
					Close();
				});
			} catch (Exception exc) {
				MessageBox.Show(exc.ToString());
			}
		}

		private void ReleaseObject(object obj) {
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
