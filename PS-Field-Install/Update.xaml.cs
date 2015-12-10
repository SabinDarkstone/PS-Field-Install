using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using PS_Field_Install.Scripts;

namespace PS_Field_Install {

	public partial class Update : Page {

		// FLAGS
		private bool columnEditMode = false;
		private bool itemSelected = false;

		private Hashtable columnAllocation;
		private Hashtable images;

		private enum ColTypes {
			DoNotUse, pCICode, pDescription, pPowerSentrySolution, pMounting, pWiringDiagram, pComment
		};

		private int rows;

		public Update() {
			InitializeComponent();
			InitializeUploader();
		}

		private void InitializeUploader() {
			radioCICode.Tag = ColTypes.pCICode;
			radioComment.Tag = ColTypes.pComment;
			radioDescription.Tag = ColTypes.pDescription;
			radioMounting.Tag = ColTypes.pMounting;
			radioNone.Tag = ColTypes.DoNotUse;
			radioPowerSentrySolution.Tag = ColTypes.pPowerSentrySolution;
			radioWiringDiagram.Tag = ColTypes.pWiringDiagram;

			InitializeCurrentImages();
		}

		#region Help Link Events
		private void linkHelp_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
			linkHelp.Foreground = System.Windows.Media.Brushes.LightBlue;
		}

		private void linkHelp_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
			linkHelp.Foreground = System.Windows.Media.Brushes.White;
		}

		private void linkHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			linkHelp.Foreground = System.Windows.Media.Brushes.Red;
			Help helpWindow = new Help();
			helpWindow.ShowDialog();
		}

		private void linkHelp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			linkHelp.Foreground = System.Windows.Media.Brushes.White;
		}
		#endregion

		#region Search Link Events
		private void linkSearch_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
			linkSearch.Foreground = System.Windows.Media.Brushes.White;
		}

		private void linkSearch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			linkSearch.Foreground = System.Windows.Media.Brushes.Red;

			Uri uri = new Uri("Search.xaml", UriKind.Relative);
			this.NavigationService.Navigate(uri);
		}

		private void linkSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			linkSearch.Foreground = System.Windows.Media.Brushes.White;
		}

		private void linkSearch_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
			linkSearch.Foreground = System.Windows.Media.Brushes.LightBlue;
		}
		#endregion

		#region Databse Update
		private void btnChooseFile_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Excel Files (*.xls, *.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*";
			openFileDialog.FilterIndex = 1;
			DialogResult dr = openFileDialog.ShowDialog();

			if (dr == DialogResult.OK) {
				txtFilename.Text = openFileDialog.FileName;
				List<string> headings = GetColumnHeadings(openFileDialog.FileName);

				foreach (string str in headings) {
					listHeadings.Items.Add(str);
				}

				columnEditMode = true;
				columnAllocation = new Hashtable();

				foreach (object col in listHeadings.Items) {
					columnAllocation.Add(col, ColTypes.DoNotUse.ToString());
				}
			}
		}

		private List<string> GetColumnHeadings(string filename) {
			Excel.Application excelApp;
			Excel.Workbook excelWorkbook;
			Excel.Worksheet excelWorksheet;
			Excel.Range range;


			string str;
			int rCnt, cCnt;
			rCnt = cCnt = 0;

			List<string> headings = new List<string>();

			excelApp = new Excel.Application();
			excelWorkbook = excelApp.Workbooks.Open(filename);
			excelWorksheet = (Excel.Worksheet)excelWorkbook.Worksheets.get_Item(1);

			range = excelWorksheet.UsedRange;
			rows = range.Rows.Count;

			for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++) {
				str = (string)(range.Cells[1, cCnt] as Excel.Range).Value;
				headings.Add(str);
			}

			excelWorkbook.Close(true, null, null);
			excelApp.Quit();

			ReleaseObject(excelWorksheet);
			ReleaseObject(excelWorkbook);
			ReleaseObject(excelApp);

			return headings;
		}

		private void ReleaseObject(object obj) {
			try {
				System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
				obj = null;
			} catch (Exception ex) {
				obj = null;
				System.Windows.MessageBox.Show("Unable to release the Object " + ex.ToString());
			} finally {
				GC.Collect();
			}
		}

		private void listHeadings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if (columnEditMode) {
				if (listHeadings.SelectedIndex != -1) {
					itemSelected = true;
					string heading = columnAllocation[listHeadings.SelectedItem].ToString();
					if (heading != null) {
						switch (heading) {
							case "DoNotUse":
								radioNone.IsChecked = true;
								break;
							case "pCICode":
								radioCICode.IsChecked = true;
								break;
							case "pDescription":
								radioDescription.IsChecked = true;
								break;
							case "pPowerSentrySolution":
								radioPowerSentrySolution.IsChecked = true;
								break;
							case "pMounting":
								radioMounting.IsChecked = true;
								break;
							case "pWiringDiagram":
								radioWiringDiagram.IsChecked = true;
								break;
							case "pComment":
								radioComment.IsChecked = true;
								break;
							default:
								radioNone.IsChecked = true;
								break;
						}
					} else {
						radioNone.IsChecked = true;
					}
				} else {
					itemSelected = false;
				}
			}

			return;
		}

		private void radioButtons_Checked(object sender, RoutedEventArgs e) {
			System.Windows.Controls.RadioButton rb = sender as System.Windows.Controls.RadioButton;

			if (itemSelected && columnEditMode) {
				object heading = listHeadings.SelectedItem;
				columnAllocation[heading] = rb.Tag;
				// System.Windows.MessageBox.Show(columnAllocation[heading].ToString());
			} else {
				return;
			}

		}

		private void btnConfirm_Click(object sender, RoutedEventArgs e) {
			// Confirm with the user that their settings are correct
			string verifyMe = "";

			foreach (DictionaryEntry de in columnAllocation) {
				verifyMe += string.Format("Column {0} is a {1}\n", de.Key, de.Value);
			}

			MessageBoxResult result = System.Windows.MessageBox.Show("Please verfiy the settings you chose are correct:\n" + verifyMe, "Verify Information", MessageBoxButton.YesNo);

			if (result == MessageBoxResult.Yes) {
				ProgressBar pBar = new ProgressBar(ref columnAllocation, txtFilename.Text, rows);
				pBar.Show();
			} else {
				return;
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e) {
			linkSearch_MouseLeftButtonDown(null, null);
		}

		private void btnReset_Click(object sender, RoutedEventArgs e) {
			radioCICode.IsChecked = false;
			radioComment.IsChecked = false;
			radioDescription.IsChecked = false;
			radioMounting.IsChecked = false;
			radioNone.IsChecked = false;
			radioPowerSentrySolution.IsChecked = false;
			radioWiringDiagram.IsChecked = false;
			txtFilename.Text = "";
			listHeadings.Items.Clear();
		}
		#endregion

		#region Image Uploader
		private void btnOpen_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "PNG Files(*.png)|*.png";
			DialogResult dr = openFileDialog.ShowDialog();

			if (dr == DialogResult.OK) {
				txtImageFile.Text = openFileDialog.FileName;
				ImageSource image = new BitmapImage(new Uri(txtImageFile.Text));
				picPreview.Source = image;
			}
		}

		private async void btnUploadImage_Click(object sender, RoutedEventArgs e) {
			if (txtProduct.Text == null) {
				System.Windows.MessageBox.Show("Please enter a family or product name before uploading the image file.");
				return;
			}

			if (txtImageFile.Text != null) {
				if (radioLuminaire.IsChecked == true) {
					await DropboxHelper.SendFileToDropbox(txtImageFile.Text, "/Images/Lithonia", txtProduct.Text + ".png");
				} else if (radioPowerSentry.IsChecked == true) {
					await DropboxHelper.SendFileToDropbox(txtImageFile.Text, "/Images/Power Sentry", txtProduct.Text + ".png");
				} else {
					System.Windows.MessageBox.Show("Please select the type of product.");
					return;
				}
			} else {
				System.Windows.MessageBox.Show("Please select a new file to upload by using the 'Browse' button");
				return;
			}

			System.Windows.MessageBox.Show("Image successfully uploaded");

			Waiting waiting = new Waiting();
			waiting.Show();
			await DataHandler.DownloadImages();
			waiting.Close();

			InitializeCurrentImages();  // Reload image list
		}

		// TODO: Finish retrieving files from "Lithonia" and "Power Sentry" directories and showing them in the listbox
		private void InitializeCurrentImages() {
			IEnumerable<string> imagesLithonia = System.IO.Directory.EnumerateFiles(TextTools.MyRelativePath(@"Temp\Lithonia"));
			IEnumerable<string> imagesPowerSentry = System.IO.Directory.EnumerateFiles(TextTools.MyRelativePath(@"Temp\Power Sentry"));

			if (images == null) {
				images = new Hashtable();
			} else {
				images.Clear();
			}

			int i = 0;
			foreach (string s in imagesLithonia) {
				images.Add(i, s);
					i++;
			}
			foreach (string s in imagesPowerSentry) {
				images.Add(i, s);
				i++;
			}

			listCurrentImageFiles.Items.Clear();

			foreach (string str in imagesLithonia) {
				var product = str.Substring(str.IndexOf("Lithonia") + 9);
				product = product.Substring(0, product.IndexOf("."));
				listCurrentImageFiles.Items.Add(product);
			}

			foreach (string str in imagesPowerSentry) {
				var product = str.Substring(str.IndexOf("Power Sentry") + 13);
				product = product.Substring(0, product.IndexOf("."));
				listCurrentImageFiles.Items.Add(product);
			}
		}

		private void btnClear_Click(object sender, RoutedEventArgs e) {
			picPreview.Source = null;
		}

		private void btnPreview_Click(object sender, RoutedEventArgs e) {
			picPreview.Source = new BitmapImage(new Uri(images[listCurrentImageFiles.SelectedIndex].ToString(), UriKind.Absolute));
		}
		#endregion

	}
}
