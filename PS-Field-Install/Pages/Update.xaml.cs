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

		private bool manualChangeFlag = true;
		private Hashtable images;
		private int rows;

		private enum Direction {
			UP,
			DOWN,
			TOP,
			BOTTOM
		};

		private Hashtable databaseTransform = new Hashtable();

		private List<string> headings;
		private List<string> categories;

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
			Uri uri = new Uri("Pages/Search.xaml", UriKind.Relative);
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
				var filename = openFileDialog.FileName;
				txtFilename.Text = filename;
				GetColumnHeadings(filename);

				foreach (var item in headings) {
					listHeadings.Items.Add(item);
				}

				UpdateCategories();
			}
		}

		/// <summary>
		/// Reads the excel file given by filename for the headings of each column
		/// </summary>
		/// <param name="filename">The fully qualified filepath of the excel file</param>
		/// <returns>A list of strings that represents each column heading in the excel file</returns>
		private void GetColumnHeadings(string filename) {
			if (headings == null) {
				headings = new List<string>();
			}

			Excel.Application excelApp;
			Excel.Workbook excelWorkbook;
			Excel.Worksheet excelWorksheet;
			Excel.Range range;

			string str;
			int rCnt, cCnt;
			rCnt = cCnt = 0;

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
				var selectedHeading = listHeadings.SelectedItem.ToString();
				if (databaseTransform.ContainsKey(selectedHeading)) {
					manualChangeFlag = false;
					comboHeadings.SelectedItem = databaseTransform[selectedHeading];
					manualChangeFlag = true;
				} else {
					manualChangeFlag = false;
					comboHeadings.SelectedIndex = -1;
					manualChangeFlag = true;
				}
			}
		}

		private void btnConfirm_Click(object sender, RoutedEventArgs e) {
			var verifyMe = "";

			foreach (var item in categories) {
				verifyMe += item.ToString() + ":\n";
				if (databaseTransform == null) {
					System.Windows.MessageBox.Show("An error occured while transforming the database");
					return;
				} else {
					foreach (DictionaryEntry de in databaseTransform) {
						if (de.Value.ToString() == item.ToString()) {
							verifyMe += "     " + de.Key.ToString() + "\n";
						}
					}
				}
				verifyMe += "\n";
			}

			MessageBoxResult result = System.Windows.MessageBox.Show("Please verify that the settings you chose are correct:\n\n" + verifyMe, "Verify Information", MessageBoxButton.YesNo);

			if (result == MessageBoxResult.Yes) {
				ProgressBar bar = new ProgressBar(databaseTransform, txtFilename.Text, rows);
				bar.Show();
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e) {
			linkSearch_MouseLeftButtonDown(null, null);
		}

		/// <summary>
		/// Resets the information displayed in the database update tab
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnReset_Click(object sender, RoutedEventArgs e) {
			txtNewCategory.Text = "";
			txtFilename.Text = "";
			listHeadings.Items.Clear();
		}

		private void comboHeadings_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (manualChangeFlag) {
				var selectedHeading = listHeadings.SelectedItem.ToString();
				if (databaseTransform.ContainsKey(selectedHeading)) {
					databaseTransform.Remove(selectedHeading);
				}
				databaseTransform.Add(selectedHeading, comboHeadings.SelectedItem.ToString());
			}
		}

		/// <summary>
		/// Handles the reading of the category file to check for existing and previously used categories
		/// </summary>
		private void UpdateCategories() {
			if (categories == null) {
				categories = new List<string>();
			} else {
				categories.Clear();
			}

			categories = TextTools.ReadFileLines(Settings.SavedCategories, false);

			comboHeadings.Items.Clear();
			foreach (var item in categories) {
				comboHeadings.Items.Add(item);
			}

			DataHandler.GetSearchColumns();

		}

		/// <summary>
		/// Adds category name in txtNew Category to list of available categories for use
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnAddCategory_Click(object sender, RoutedEventArgs e) {
			if (categories == null) {
				categories = new List<string>();
			}

			StreamReader sr = new StreamReader(Settings.SavedCategories);
			string line;
			while ((line = sr.ReadLine()) != null) {
				if (line == txtNewCategory.Text) {
					System.Windows.MessageBox.Show("Category already exists.");
					sr.Close();
					return;
				}
			}

			sr.Close();
			StreamWriter sw = File.AppendText(Settings.SavedCategories);
			sw.WriteLine(txtNewCategory.Text);
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
			IEnumerable<string> imagesLithonia = Directory.EnumerateFiles(Settings.ImagesFolder_Lithonia);
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

		#region Display Settings
		private void listboxAvailCats_Loaded(object sender, RoutedEventArgs e) {
			InitializeDisplaySettings();
		}

		private void InitializeDisplaySettings() {
			ClearDisplaySettings();

			StreamReader sr = new StreamReader(Settings.SavedCategories);
			List<string> fileCats = new List<string>();
			string line;
			while ((line = sr.ReadLine()) != null) {
				fileCats.Add(line);
			}
			sr.Close();

			foreach (var item in categories) {
				if (!fileCats.Contains(line)) {
					// Nothing to see here for the moment
				}
				listboxAvailCats.Items.Add(item);
			}

			StreamReader sr2 = new StreamReader(Settings.ResultsDisplayOrder);
			string line2;
			while ((line2 = sr2.ReadLine()) != null) {
				listboxShownCats.Items.Add(line2);
			}
			sr2.Close();

			for (int i = 0; i < listboxShownCats.Items.Count; i++) {
				if (listboxAvailCats.Items.Contains(listboxShownCats.Items[i])) {
					listboxAvailCats.Items.Remove(listboxShownCats.Items[i]);
				}
			}
		}

		private void ClearDisplaySettings() {
			listboxAvailCats.Items.Clear();
			listboxShownCats.Items.Clear();
		}

		private void btnDisplayAdd_Click(object sender, RoutedEventArgs e) {
			if (listboxAvailCats.SelectedIndex != -1) {
				listboxShownCats.Items.Add(listboxAvailCats.SelectedItem);
				listboxAvailCats.Items.Remove(listboxAvailCats.SelectedItem);
			}
		}

		private void btnDisplayRemove_Click(object sender, RoutedEventArgs e) {
			if (listboxShownCats.SelectedIndex != -1) {
				listboxAvailCats.Items.Add(listboxShownCats.SelectedItem);
				listboxShownCats.Items.Remove(listboxShownCats.SelectedItem);
			}
		}

		private void btnDisplayCancel_Click(object sender, RoutedEventArgs e) {
			linkSearch_MouseLeftButtonDown(null, null);
		}

		private void btnDisplayApply_Click(object sender, RoutedEventArgs e) {
			StreamWriter sw = new StreamWriter(Settings.ResultsDisplayOrder);
			for (int i = 0; i < listboxShownCats.Items.Count; i++) {
				sw.WriteLine(listboxShownCats.Items[i].ToString());
			}
			sw.Close();

			System.Windows.MessageBox.Show("Categories list saved.");

			DataHandler.ReorderColumns();
		}

		#region Listbox Moving
		/// <summary>
		/// Reorders the listbox that represents what is being shown in the results view
		/// </summary>
		/// <param name="selectedItem">The selected item in the listbox</param>
		/// <param name="direction">The direction of which  to move the item</param>
		private void MoveListItem(object selectedItem, Direction direction) {
			List<object> list = new List<object>();
			for (int i = 0; i < listboxShownCats.Items.Count; i++) {
				list.Add(listboxShownCats.Items[i]);
			}
			var index = listboxShownCats.Items.IndexOf(selectedItem);
			switch (direction) {
				case Direction.DOWN:
					if (index != list.Count - 1) {
						list.Insert(index + 2, selectedItem);
						list.RemoveAt(index);
					}
					break;
				case Direction.UP:
					if (index != 0) {
						list.Insert(index - 1, selectedItem);
						list.RemoveAt(index + 1);
					}
					break;
				case Direction.TOP:
					if (index != 0) {
						list.Insert(0, selectedItem);
						list.RemoveAt(index + 1);
					}
					break;
				case Direction.BOTTOM:
					if (index != list.Count - 1) {
						list.Insert(list.Count, selectedItem);
						list.RemoveAt(index);
					}
					break;
				default:
					break;
			}

			listboxShownCats.Items.Clear();
			foreach (var item in list) {
				listboxShownCats.Items.Add(item);
			}

			listboxShownCats.SelectedItem = selectedItem;

		}

		private void btnDisplayMoveUp_Click(object sender, RoutedEventArgs e) {
			MoveListItem(listboxShownCats.SelectedItem, Direction.UP);
		}

		private void btnDisplayMoveDown_Click(object sender, RoutedEventArgs e) {
			MoveListItem(listboxShownCats.SelectedItem, Direction.DOWN);

		}

		private void btnDisplayMoveTop_Click(object sender, RoutedEventArgs e) {
			MoveListItem(listboxShownCats.SelectedItem, Direction.TOP);

		}

		private void btnDisplayMoveBottom_Click(object sender, RoutedEventArgs e) {
			MoveListItem(listboxShownCats.SelectedItem, Direction.BOTTOM);

		}
		#endregion
		#endregion

	}
}
