#define TEST

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using PS_Field_Install.Scripts;

namespace PS_Field_Install {

	public partial class Search : Page {

		private string[] bmpPSimg;

		public Search() {
			InitializeComponent();
		}

		private void ClearText() {

		}

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			ClearText();

			Waiting waiting = new Waiting("Updating Database...");
			waiting.Show();

			DataHandler.LoadDatabaseFromLocal();
			waiting.Close();

			DisplayResult(null);

			foreach (DataColumn dc in DataHandler.productData.Tables["Products"].Columns) {
				if (dc.ToString().Contains("CI")) {
					DataHandler.CICodeColumn = dc.ToString();
					// MessageBox.Show("CI Code column found: " + dc.ToString());
				}

				if (dc.ToString().Contains("Desc")) {
					DataHandler.DescriptionColumn = dc.ToString();
					// MessageBox.Show("Description column found: " + dc.ToString());
				}
			}
		}

		#region Help Link Events
		private void linkHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			Help helpWindow = new Help();
			helpWindow.ShowDialog();
		}

		private void linkHelp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

		}
		#endregion

		private void btnRunSearch_Click(object sender, RoutedEventArgs e) {
			DataRow[] foundRows;
			bmpPSimg = null;

			if (radioCICodeOnly.IsChecked == true) {
				foundRows = RunQuery("CICode", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
				if (foundRows.Length == 0) {
					MessageBox.Show("Could not find specified product.");
					DisplayResult(null);
					return;
				}
			} else if (radioDescriptionOnly.IsChecked == true) {
				foundRows = RunQuery("Description", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
				if (foundRows.Length == 0) {
					MessageBox.Show("Could not find specified product.");
					DisplayResult(null);
					return;
				}
			} else {
				foundRows = RunQuery("CICode", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
				if (foundRows.Length == 0) {
					foundRows = RunQuery("Description", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
					if (foundRows.Length == 0) {
						MessageBox.Show("Could not find specified product.");
						DisplayResult(null);
						return;
					}
				}
			}

			if (chkbxAllowPartial.IsChecked == false) {
				DisplayResult(foundRows[0]);
			} else {
				MultipleResults multi = new MultipleResults(ref foundRows);
				multi.ShowDialog();
				if (multi.DialogResult == true) {
					DisplayResult(foundRows[multi.GetResult()]);
				}
			}
		}

		private DataRow[] RunQuery(string mode, string findText, bool? partials) {
			switch (mode) {
				case "CICode":
					return DataHandler.productData.Tables["Products"].Select(DataHandler.CICodeColumn + "='" + findText + "'");

				case "Description":
					if (partials == true) {
						return DataHandler.productData.Tables["Products"].Select(DataHandler.DescriptionColumn + " LIKE '%" + findText + "%'");
					} else {
						return DataHandler.productData.Tables["Products"].Select(DataHandler.DescriptionColumn + "='" + findText + "'");
					}

				default:
					MessageBox.Show("An unknown error occured while searching");
					return null;
			}
		}

		private void GetImages(DataRow row) {
			if (row == null) {
				return;
			}

			string[] psImageName;
			int batteryCount = 0;
			int i = 0;

			// Start with the Power Sentry product image
			string strPS = row["PS_Solution"].ToString();
			psImageName = TextTools.SplitToArray(strPS, "and");
			batteryCount = psImageName.Length;
			bmpPSimg = new string[batteryCount];

			foreach (string s in psImageName) {
				bmpPSimg[i] = Settings.ImagesFolder_PowerSentry + @"\" + s + ".png";
				i++;
			}

			cycleBatteries.Images = bmpPSimg;

			// Next grab the image for the product
			string strLL = TextTools.GetProductFamily(row[DataHandler.DescriptionColumn].ToString());

			try {
				imageProduct.Source = new BitmapImage(new Uri(Settings.ImagesFolder_Lithonia + @"\" + TextTools.GetProductFamily(row[DataHandler.DescriptionColumn].ToString()) + ".png"));
			} catch (Exception) { }
		}

		private void DisplayResult(DataRow row) {
			gridResults.Children.RemoveRange(0, gridResults.Children.Count);

			imageProduct.Source = null;
			cycleBatteries.Images = null;

			if (row == null) {
				return;
			}

			foreach (var item in DataHandler.resultsOrder) {
				if (row == null) {
					Controls.ColumnResult colResult = new Controls.ColumnResult("", "");
					gridResults.Children.Add(colResult);
				} else {
					Controls.ColumnResult colResult = new Controls.ColumnResult(item, row[item].ToString());
					gridResults.Children.Add(colResult);
				}
			}

			GetImages(row);
			cycleBatteries.Initiate();

		}

		private void linkLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
#if TEST
			this.NavigationService.Navigate(new Uri("Pages/Update.xaml", UriKind.Relative));
#else
			Login login = new Login();
			login.ShowDialog();
			if (login.DialogResult == true) {
				Uri uri = new Uri("Pages/Update.xaml", UriKind.Relative);
				this.NavigationService.Navigate(uri);
			}
#endif
		}

		private void textblock_RightClick(object sender, MouseButtonEventArgs e) {

		}

		private void image_RightClick(object sender, MouseButtonEventArgs e) {

		}

		private void CopyExcel_Click(object sender, RoutedEventArgs e) {

		}

		private void CopySingle_Click(object sender, RoutedEventArgs e) {

		}

	}
}