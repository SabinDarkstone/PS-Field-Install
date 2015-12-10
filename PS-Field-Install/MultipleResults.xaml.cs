using System.Windows;
using System.Data;

namespace PS_Field_Install {
	/// <summary>
	/// Interaction logic for MultipleResults.xaml
	/// </summary>
	public partial class MultipleResults : Window {
		public MultipleResults(ref DataRow[] results) {
			InitializeComponent();

			foreach (DataRow row in results) {
				listResults.Items.Add(row["pDescription"]);
			}
		}

		private void btnConfirm_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}

		public int GetResult() {
			return listResults.SelectedIndex;
		}
	}
}
