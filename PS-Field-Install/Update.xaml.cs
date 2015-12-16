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
			Do_Not_Use,
			CICodes,
			Descriptions,
			Power_Sentry_Solutions,
			Mounting_Options,
			Wiring_Diagrams,
			Comments
		};

		private int rows;

		public Update() {
			LogHelper.Log.Debug("Update.Constructor");
			InitializeComponent();
			InitializeUploader();
		}

		private void InitializeUploader() {
			LogHelper.Log.Debug("Update.InitializeUploader()");
			radioCICode.Tag = ColTypes.CICodes;
			radioComment.Tag = ColTypes.Comments;
			radioDescription.Tag = ColTypes.Descriptions;
			radioMounting.Tag = ColTypes.Mounting_Options;
			radioNone.Tag = ColTypes.Do_Not_Use;
			radioPowerSentrySolution.Tag = ColTypes.Power_Sentry_Solutions;
			radioWiringDiagram.Tag = ColTypes.Wiring_Diagrams;

			InitializeCurrentImages();
		}

		#region Help Link Events
		private void linkHelp_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {

		}

		private void linkHelp_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {

		}

		private void linkHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			Help helpWindow = new Help();
			helpWindow.ShowDialog();
		}

		private void linkHelp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

		}
		#endregion

		#region Search Link Events
		private void linkSearch_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {

		}

		private void linkSearch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			Uri uri = new Uri("Search.xaml", UriKind.Relative);
			this.NavigationService.Navigate(uri);
		}

		private void linkSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

		}

		private void linkSearch_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {

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
				Waiting waiting = new Waiting();
				waiting.ChangeText("Reading column headings in Excel file");
				waiting.Show();
				List<string> headings = GetColumnHeadings(openFileDialog.FileName);

				foreach (string str in headings) {
					listHeadings.Items.Add(str);
				}

				columnEditMode = true;
				columnAllocation = new Hashtable();

				foreach (object col in listHeadings.Items) {
					columnAllocation.Add(col, ColTypes.Do_Not_Use.ToString());
				}
				waiting.Close();
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

			excelWorkbook.Close(false, null, null);
			excelApp.Quit();

			ReleaseObject(excelWorksheet);
			ReleaseObject(excelWorkbook);
			ReleaseObject(excelApp);

			return headings;
		}

		private void ReleaseObject(object obj) {
			LogHelper.Log.Debug("Update.ReleaseObject(obj)");

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
			LogHelper.Log.Debug("Update.listHeadings_MouseLeftButtonUp(sender, e)");
			if (columnEditMode) {
				if (listHeadings.SelectedIndex != -1) {
					itemSelected = true;
					string heading = columnAllocation[listHeadings.SelectedItem].ToString();
					if (heading != null) {
						switch (heading) {
							case "Do_Not_Use":
								radioNone.IsChecked = true;
								break;
							case "CICodes":
								radioCICode.IsChecked = true;
								break;
							case "Descriptions":
								radioDescription.IsChecked = true;
								break;
							case "Power_Sentry_Solutions":
								radioPowerSentrySolution.IsChecked = true;
								break;
							case "Mounting_Options":
								radioMounting.IsChecked = true;
								break;
							case "Wiring_Diagrams":
								radioWiringDiagram.IsChecked = true;
								break;
							case "Comments":
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
			LogHelper.Log.Debug("Update.radioButtons_Checked(sender, e)");
			System.Windows.Controls.RadioButton rb = sender as System.Windows.Controls.RadioButton;

			if (itemSelected && columnEditMode) {
				object heading = listHeadings.SelectedItem;
				columnAllocation[heading] = rb.Tag;
			} else {
				return;
			}

		}

		private void btnConfirm_Click(object sender, RoutedEventArgs e) {
			LogHelper.Log.Debug("Update.btnConfirm_Click(sender, e)");
			// Confirm with the user that their settings are correct
			string verifyMe = "";

			/* UNDONE to make this easier for user
			foreach (DictionaryEntry de in columnAllocation) {
				verifyMe += string.Format("Column {0} is a {1}\n", de.Key, de.Value);
			}
			*/

			// Builds verifyMe string
			var coltypes = TextTools.GetValues<ColTypes>();
			foreach (ColTypes type in coltypes) {
				verifyMe += type.ToString() + ":\n";
				if (columnAllocation == null) {
					System.Windows.MessageBox.Show("An error occured while building column allocation list.");
					return;
				}
				foreach (DictionaryEntry de in columnAllocation) {
					if (de.Value.ToString() == type.ToString()) {
						verifyMe += "     " + de.Key + "\n";
					}
				}
				verifyMe += "\n";
			}

			MessageBoxResult result = System.Windows.MessageBox.Show("Please verfiy the settings you chose are correct:\n\n" + verifyMe, "Verify Information", MessageBoxButton.YesNo);

			if (result == MessageBoxResult.Yes) {
				ProgressBar pBar = new ProgressBar(ref columnAllocation, txtFilename.Text, rows);
				pBar.Show();
			} else {
				return;
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e) {
			LogHelper.Log.Debug("Update.btnCancel_Click(sender, e)");
			linkSearch_MouseLeftButtonDown(null, null);
		}

		private void btnReset_Click(object sender, RoutedEventArgs e) {
			LogHelper.Log.Debug("Update.btnReset_Click(sender, e)");
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
			LogHelper.Log.Debug("Update.btnOpen_Click(sender, e)");
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
			LogHelper.Log.Debug("Update.btnUploadImage_Click(sender, e)");
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
			// await DataHandler.DownloadImages();
			waiting.Close();

			InitializeCurrentImages();  // Reload image list
		}

		private void InitializeCurrentImages() {
			LogHelper.Log.Debug("Update.InitializeCurrentImages()");
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
			LogHelper.Log.Debug("Update.btnClear_Click(sender, e)");
			picPreview.Source = null;
		}

		private void btnPreview_Click(object sender, RoutedEventArgs e) {
			LogHelper.Log.Debug("Update.btnPreview_Click(sender, e)");
			picPreview.Source = new BitmapImage(new Uri(images[listCurrentImageFiles.SelectedIndex].ToString(), UriKind.Absolute));
		}
		#endregion

		private async void Page_Unloaded(object sender, RoutedEventArgs e) {
			await LogHelper.UploadLog();
		}

		private async void btnDelete_Click(object sender, RoutedEventArgs e) {
			MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to delete the image for " + listCurrentImageFiles.SelectedItem.ToString() + "?", "Confirm Image Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
			if (result == MessageBoxResult.Yes) {
				if (await DropboxHelper.CheckRemoteFileExists("/Images/Lithonia", listCurrentImageFiles.SelectedItem.ToString())) {
					DropboxHelper.DeleteFile("/Images/Lithonia", listCurrentImageFiles.SelectedItem.ToString());
					System.IO.File.Delete(TextTools.MyRelativePath(@"Temp\Lithonia\" + listCurrentImageFiles.SelectedItem.ToString() + ".png"));
				}

				if (await DropboxHelper.CheckRemoteFileExists("/Images/Power Sentry", listCurrentImageFiles.SelectedItem.ToString())) {
					DropboxHelper.DeleteFile("/Images/Power Sentry", listCurrentImageFiles.SelectedItem.ToString());
					System.IO.File.Delete(TextTools.MyRelativePath(@"Temp\Power Sentry\" + listCurrentImageFiles.SelectedItem.ToString() + ".png"));

				}
			}
		}
	}
}
