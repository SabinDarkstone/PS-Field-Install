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

			foreach (DataRow row in results) {
				listResults.Items.Add(row[DataHandler.DescriptionColumn]);
			}
		}

		private void btnConfirm_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}

		public int GetResult() {
			return listResults.SelectedIndex;
		}

		private void Window_Unloaded(object sender, RoutedEventArgs e) {

		}
	}
}
