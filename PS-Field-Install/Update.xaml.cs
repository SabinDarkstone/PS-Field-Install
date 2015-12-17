using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PS_Field_Install.Scripts;

using Excel = Microsoft.Office.Interop.Excel;



namespace PS_Field_Install {

	public partial class Update : Page {

		// FLAGS
		private bool manualChangeFlag = true;

		private Hashtable columnAllocation;
		private Hashtable images;

		/// <summary>
		/// List of categories that will be in the final datatable of database
		/// </summary>
		private List<string> categories;

		/// <summary>
		/// List of column headings in excel file
		/// </summary>
		private List<string> headings;

		private int rows;

		public Update() {
			InitializeComponent();
			InitializeUploader();
		}

		private void InitializeUploader() {
			UpdateCategories();
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
				headings = GetColumnHeadings(openFileDialog.FileName);

				foreach (string str in headings) {
					listHeadings.Items.Add(str);
				}

				if (columnAllocation != null) {
					columnAllocation.Clear();
				} else {
					columnAllocation = new Hashtable();
				}

				foreach (var col in listHeadings.Items) {
					columnAllocation.Add(col, headings[0]);
				}
			}
		}

		/// <summary>
		/// Reads the excel file given by filename for the headings of each column
		/// </summary>
		/// <param name="filename">The fully qualified filepath of the excel file</param>
		/// <returns>A list of strings that represents each column heading in the excel file</returns>
		private List<string> GetColumnHeadings(string filename) {
			Excel.Application excelApp;
			Excel.Workbook excelWorkbook;
			Excel.Worksheet excelWorksheet;
			Excel.Range range;

			string str;
			int rCnt, cCnt;
			rCnt = cCnt = 0;

			headings = new List<string>();

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

			string headingList = "";
			foreach (var item in headings) {
				headingList += item.ToString() + "\n";
			}
			System.Windows.MessageBox.Show(headingList);

			return headings;
		}

		/// <summary>
		/// Releases resources
		/// </summary>
		/// <param name="obj"></param>
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
			if (listHeadings.SelectedIndex != -1) {
				string currHeading = columnAllocation[listHeadings.SelectedItem].ToString();
				if (headings != null) {
					foreach (var item in categories) {
						if (currHeading == item.ToString()) {
							manualChangeFlag = false;
							comboHeadings.SelectedItem = item.ToString();
							manualChangeFlag = true;
							return;
						}
					}
					manualChangeFlag = false;
					comboHeadings.SelectedIndex = -1;
					manualChangeFlag = true;
				}
			}
		}

		private void btnConfirm_Click(object sender, RoutedEventArgs e) {
			string verifyMe = "";

			foreach (var item in categories) {
				verifyMe += item.ToString() + ":\n";
				if (columnAllocation == null) {
					System.Windows.MessageBox.Show("An error occured while building column allocation list.");
					return;
				}

				foreach (DictionaryEntry de in columnAllocation) {
					if (de.Value.ToString() == item.ToString()) {
						verifyMe += "     " + de.Key.ToString() + "\n";
					}
				}

				verifyMe += "\n";
			}

			MessageBoxResult result = System.Windows.MessageBox.Show("Please verify that the settings you chose are correct:\n\n" + verifyMe, "Verify Information", MessageBoxButton.YesNo);

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
			txtNewCategory.Text = "";
			txtFilename.Text = "";
			listHeadings.Items.Clear();
		}

		private void comboHeadings_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (manualChangeFlag) {
				var currHeading = listHeadings.SelectedItem;
				columnAllocation[currHeading] = e.AddedItems[0].ToString();
			}
		}

		private void UpdateCategories() {
			comboHeadings.Items.Clear();

			if (categories == null) {
				categories = new List<string>();
			}

			StreamReader sr = new StreamReader(Settings.SavedCategories);
			string line;
			while ((line = sr.ReadLine()) != null) {
				if (!categories.Contains(line)) {
					categories.Add(line);
				}
			}
			sr.Close();

			foreach (var item in categories) {
				comboHeadings.Items.Add(item.ToString());
			}
		}

		/// <summary>
		/// Adds category name in txtNew Category to list of available categories for use
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnAddCategory_Click(object sender, RoutedEventArgs e) {
			var newCat = txtNewCategory.Text;
			if (categories == null) {
				categories = new List<string>();
			}

			categories.Add(newCat);

			StreamReader sr = new StreamReader(Settings.SavedCategories);
			string line;
			while ((line = sr.ReadLine()) != null) {
				if (newCat == line) {
					System.Windows.MessageBox.Show("Category already exists!");
					return;
				}
			}
			sr.Close();

			StreamWriter sw = File.AppendText(Settings.SavedCategories);
			sw.WriteLine(newCat);
			sw.Close();
			txtNewCategory.Text = "";
			UpdateCategories();
		}
		#endregion

		#region Image Uploader
		/// <summary>
		/// Opens the file dialogs to allow user to navigate to image location for uploading
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnOpen_Click(object sender, RoutedEventArgs e) {
			// LogHelper.Log.Debug("Update.btnOpen_Click(sender, e)");
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "PNG Files(*.png)|*.png";
			DialogResult dr = openFileDialog.ShowDialog();

			if (dr == DialogResult.OK) {
				txtImageFile.Text = openFileDialog.FileName;
				ImageSource image = new BitmapImage(new Uri(txtImageFile.Text));
				picPreview.Source = image;
			}
		}

		/// <summary>
		/// Uploads the image to directory for use in the database
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnUploadImage_Click(object sender, RoutedEventArgs e) {
			if (txtProduct.Text == null) {
				System.Windows.MessageBox.Show("Please enter a family or product name before uploading the image file.");
				return;
			}

			if (txtImageFile.Text != null) {
				if (radioLuminaire.IsChecked == true) {
					File.Copy(txtImageFile.Text, Settings.ImagesFolder_Lithonia + @"\" + txtProduct.Text + ".png");
				} else if (radioPowerSentry.IsChecked == true) {
					File.Copy(txtImageFile.Text, Settings.ImagesFolder_PowerSentry + @"\" + txtProduct.Text + ".png");
				} else {
					System.Windows.MessageBox.Show("Please select the type of product.");
					return;
				}
			} else {
				System.Windows.MessageBox.Show("Please select a new file to upload by using the 'Browse' button");
				return;
			}

			System.Windows.MessageBox.Show("Image successfully uploaded");

			InitializeCurrentImages();  // Reload image list
		}

		/// <summary>
		/// Initializes the current images stored for the database.
		/// </summary>
		private void InitializeCurrentImages() {
			IEnumerable<string> imagesLithonia = Directory.EnumerateFiles(Settings.ImagesFolder_PowerSentry);
			IEnumerable<string> imagesPowerSentry = Directory.EnumerateFiles(Settings.ImagesFolder_PowerSentry);

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

		/// <summary>
		/// Clears the image currently shown in preview
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnClear_Click(object sender, RoutedEventArgs e) {
			picPreview.Source = null;
		}

		/// <summary>
		/// Shows selected product image (from listbox) in preview
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnPreview_Click(object sender, RoutedEventArgs e) {
			picPreview.Source = new BitmapImage(new Uri(images[listCurrentImageFiles.SelectedIndex].ToString(), UriKind.Absolute));
		}
		#endregion

	}
}
