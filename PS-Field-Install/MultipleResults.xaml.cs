using System.Windows;
using System.Data;
using PS_Field_Install.Scripts;

namespace PS_Field_Install {
	/// <summary>
	/// Interaction logic for MultipleResults.xaml
	/// </summary>
	public partial class MultipleResults : Window {
		public MultipleResults(ref DataRow[] results) {
			InitializeComponent();

			LogHelper.Log.Debug("MultipleResults.Constructor");
			foreach (DataRow row in results) {
				listResults.Items.Add(row["Descriptions"]);
			}
		}

		private void btnConfirm_Click(object sender, RoutedEventArgs e) {
			LogHelper.Log.Debug("MultipleResults.btnConfirm.Click(sender, e)");
			DialogResult = true;
		}

		public int GetResult() {
			LogHelper.Log.Debug("MultipleResults.GetResults()");
			return listResults.SelectedIndex;
		}

		private async void Window_Unloaded(object sender, RoutedEventArgs e) {
			await LogHelper.UploadLog();
		}
	}
}
