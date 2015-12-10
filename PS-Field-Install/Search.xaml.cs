using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using PS_Field_Install.Scripts;
using System.Windows.Media.Imaging;

namespace PS_Field_Install {

	public partial class Search : Page {

		private BitmapImage[] bmpPSimg;

		private string databaseFilepath = TextTools.MyRelativePath(@"Temp\PowerSearch.xml");

		public Search() {
			InitializeComponent();

			txtResultCICode.Text = "";
			txtResultComments.Text = "";
			txtResultDescription.Text = "";
			txtResultMountingOption.Text = "";
			txtResultPowerSentry.Text = "";
			txtResultWiringDiagram.Text = "";
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e) {
			Waiting waiting = new Waiting();

			waiting.Show();
			if (await DataHandler.LoadDatabaseFromWeb() == false) {
				return;
			}
			await DataHandler.DownloadImages();
			waiting.Close();
		}

		#region Help Link Events
		private void linkHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			linkHelp.Foreground = Brushes.Red;
			Help helpWindow = new Help();
			helpWindow.ShowDialog();
		}

		private void linkHelp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			linkHelp.Foreground = Brushes.White;
		}

		private void linkHelp_MouseEnter(object sender, MouseEventArgs e) {
			linkHelp.Foreground = Brushes.LightBlue;
		}

		private void linkHelp_MouseLeave(object sender, MouseEventArgs e) {
			linkHelp.Foreground = Brushes.White;
		}
		#endregion

		#region Admin Login Link Events
		private void linkLogin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			linkLogin.Foreground = Brushes.Red;
			Login loginWindow = new Login();
			loginWindow.ShowDialog();

			if (loginWindow.DialogResult == true) {
				Uri uri = new Uri("Update.xaml", UriKind.Relative);
				this.NavigationService.Navigate(uri);
			}
		}

		private void linkLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			linkLogin.Foreground = Brushes.White;
		}

		private void linkLogin_MouseEnter(object sender, MouseEventArgs e) {
			linkLogin.Foreground = Brushes.LightBlue;
		}

		private void linkLogin_MouseLeave(object sender, MouseEventArgs e) {
			linkLogin.Foreground = Brushes.White;
		}
		#endregion

		private void btnRunSearch_Click(object sender, RoutedEventArgs e) {
			DataRow[] foundRows;
			bmpPSimg = null;

			if (radioCICodeOnly.IsChecked == true) {
				foundRows = RunQuery("CICode", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
				if (foundRows.Length == 0) {
					MessageBox.Show("Could not find specified product.");
					DisplayResults(null);
					return;
				}
			} else if (radioDescriptionOnly.IsChecked == true) {
				foundRows = RunQuery("Description", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
				if (foundRows.Length == 0) {
					MessageBox.Show("Could not find specified product.");
					DisplayResults(null);
					return;
				}
			} else {
				foundRows = RunQuery("CICode", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
				if (foundRows.Length == 0) {
					foundRows = RunQuery("Description", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
					if (foundRows.Length == 0) {
						MessageBox.Show("Could not find specified product.");
						DisplayResults(null);
						return;
					}
				}
			}

			if (chkbxAllowPartial.IsChecked == false) {
				DisplayResults(foundRows[0]);
			} else {
				MultipleResults multi = new MultipleResults(ref foundRows);
				multi.ShowDialog();
				if (multi.DialogResult == true) {
					DisplayResults(foundRows[multi.GetResult()]);
				}
			}

		}

		private DataRow[] RunQuery(string mode, string findText, bool? partials) {
			switch (mode) {
				case "CICode":
					return DataHandler.productData.Tables[0].Select("pCICode='" + findText + "'");

				case "Description":
					if (partials == true) {
						return DataHandler.productData.Tables[0].Select("pDescription LIKE '%" + findText + "%'");
					} else {
						return DataHandler.productData.Tables[0].Select("pDescription LIKE '" + findText + " (%'");
					}

				default:
					MessageBox.Show("An unknown error occured while searching");
					return null;
			}
		}

		private void DisplayResults(DataRow row) {
			txtResultCICode.Text = "";
			txtResultDescription.Text = "";
			txtResultMountingOption.Text = "";
			txtResultPowerSentry.Text = "";
			txtResultWiringDiagram.Text = "";
			txtResultComments.Text = "";

			if (row == null) {
				return;
			}

			txtResultCICode.Text = row["pCICode"].ToString();
			txtResultDescription.Text = row["pDescription"].ToString();
			txtResultMountingOption.Text = row["pMounting"].ToString();
			txtResultPowerSentry.Text = row["pPowerSentrySolution"].ToString();
			txtResultWiringDiagram.Text = row["pWiring"].ToString();
			txtResultComments.Text = row["pComment"].ToString();

			GetImages(row);
			cycleBatteries.Initiate();
		}

		private void GetImages(DataRow row) {
			string[] psImageName;
			int batteryCount = 0;
			int i = 0;

			// Start with the Power Sentry product image
			string strPS = row["pPowerSentrySolution"].ToString();
			psImageName = TextTools.SplitToArray(strPS, "and");
			batteryCount = psImageName.Length;
			bmpPSimg = new BitmapImage[batteryCount];

			foreach (string s in psImageName) {
				bmpPSimg[i] = new BitmapImage(new Uri(TextTools.MyRelativePath(@"Temp\Power Sentry\" + s + ".png")));
				i++;
			}

			cycleBatteries.Images = bmpPSimg;
       
			// Next grab the image for the product
			string strLL = TextTools.GetProductFamily(row["pDescription"].ToString());

			imageProduct.Source = new BitmapImage(new Uri(TextTools.MyRelativePath(@"Temp\Lithonia\" + TextTools.GetProductFamily(row["pDescription"].ToString()) + ".png")));
		}

	}
}