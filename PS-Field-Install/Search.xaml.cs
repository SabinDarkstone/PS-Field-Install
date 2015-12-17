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
			txtResultCICode.Text = "";
			txtResultComments.Text = "";
			txtResultDescription.Text = "";
			txtResultMountingOption.Text = "";
			txtResultPowerSentry.Text = "";
			txtResultWiringDiagram.Text = "";

			txtLabelCICode.Opacity = 0;
			txtLabelComments.Opacity = 0;
			txtLabelDescription.Opacity = 0;
			txtLabelMountingOption.Opacity = 0;
			txtLabelPowerSentry.Opacity = 0;
			txtLabelWiringDiagram.Opacity = 0;
		}

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			ClearText();

			Waiting waiting = new Waiting("Updating Database...");
			waiting.Show();

			DataHandler.LoadDatabaseFromLocal();
			waiting.Close();
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
					LogHelper.Log.Info("Could not find specified product");
					MessageBox.Show("Could not find specified product.");
					DisplayResults(null);
					return;
				}
			} else if (radioDescriptionOnly.IsChecked == true) {
				foundRows = RunQuery("Description", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
				if (foundRows.Length == 0) {
					LogHelper.Log.Info("Could not find specified product");
					MessageBox.Show("Could not find specified product.");
					DisplayResults(null);
					return;
				}
			} else {
				foundRows = RunQuery("CICode", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
				if (foundRows.Length == 0) {
					foundRows = RunQuery("Description", txtSearchBox.Text, chkbxAllowPartial.IsChecked);
					if (foundRows.Length == 0) {
						LogHelper.Log.Info("Could not find specified product");
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
					return DataHandler.productData.Tables[0].Select("CICodes='" + findText + "'");

				case "Description":
					if (partials == true) {
						return DataHandler.productData.Tables[0].Select("Descriptions LIKE '%" + findText + "%'");
					} else {
						return DataHandler.productData.Tables[0].Select("Descriptions='" + findText + "'");
					}

				default:
					MessageBox.Show("An unknown error occured while searching");
					return null;
			}
		}

		private void DisplayResults(DataRow row) {
			// LogHelper.Log.Debug("DisplayResults(row)");
			ClearText();

			imageProduct.Source = null;
			cycleBatteries.Images = null;

			if (row == null) {
				return;
			}

			txtLabelCICode.Opacity = 100;
			txtLabelComments.Opacity = 100;
			txtLabelDescription.Opacity = 100;
			txtLabelMountingOption.Opacity = 100;
			txtLabelPowerSentry.Opacity = 100;
			txtLabelWiringDiagram.Opacity = 100;

			txtResultCICode.Text = row["CICodes"].ToString();
			txtResultDescription.Text = row["Descriptions"].ToString();
			txtResultMountingOption.Text = row["Mounting_Options"].ToString();
			txtResultPowerSentry.Text = row["Power_Sentry_Solutions"].ToString();
			txtResultWiringDiagram.Text = row["Wiring_Diagrams"].ToString();
			txtResultComments.Text = row["Comments"].ToString();

			GetImages(row);
			cycleBatteries.Initiate();
		}

		private void GetImages(DataRow row) {
			string[] psImageName;
			int batteryCount = 0;
			int i = 0;

			// Start with the Power Sentry product image
			string strPS = row["Power_Sentry_Solutions"].ToString();
			psImageName = TextTools.SplitToArray(strPS, "and");
			batteryCount = psImageName.Length;
			bmpPSimg = new string[batteryCount];

			foreach (string s in psImageName) {
				bmpPSimg[i] = Settings.ImagesFolder_PowerSentry + @"\" + s + ".png";
				i++;
			}

			cycleBatteries.Images = bmpPSimg;

			// Next grab the image for the product
			string strLL = TextTools.GetProductFamily(row["Descriptions"].ToString());

			try {
				imageProduct.Source = new BitmapImage(new Uri(Settings.ImagesFolder_Lithonia + @"\" + TextTools.GetProductFamily(row["Descriptions"].ToString()) + ".png"));
			} catch (Exception) { }
		}

		private void linkLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			Login login = new Login();
			login.ShowDialog();
			if (login.DialogResult == true) {
				Uri uri = new Uri("Update.xaml", UriKind.Relative);
				this.NavigationService.Navigate(uri);
			}
		}

		private void textblock_RightClick(object sender, MouseButtonEventArgs e) {

		}

		private void image_RightClick(object sender, MouseButtonEventArgs e) {

		}

		private void CopyExcel_Click(object sender, RoutedEventArgs e) {
			string copiedData = "";
			copiedData += "CI Code: " + "\t" + txtResultCICode.Text + "\n";
			copiedData += "Description: " + "\t" + txtResultDescription.Text + "\n";
			copiedData += "Power Sentry Solution: " + "\t" + txtResultPowerSentry.Text + "\n";
			copiedData += "Mounting Options: " + "\t" + txtResultMountingOption.Text + "\n";
			copiedData += "Wiring Diagrams: " + "\t" + txtResultWiringDiagram.Text + "\n";
			if (txtResultComments.Text.Length > 0) {
				copiedData += "Comments: " + "\t" + txtResultComments.Text + "\n";
			}
			Clipboard.SetText(copiedData);
			MessageBox.Show("Results copied to clipboard", "PS Field Install Tool");
		}

		private void CopySingle_Click(object sender, RoutedEventArgs e) {

		}

	}
}