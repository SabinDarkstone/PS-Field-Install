using System.Windows.Controls;

namespace PS_Field_Install.Controls {
	/// <summary>
	/// Interaction logic for ColumnResult.xaml
	/// </summary>
	public partial class ColumnResult : UserControl {
		public ColumnResult() {
			InitializeComponent();
		}

		public ColumnResult(string label, string result) {
			InitializeComponent();

			DisplayResult(label, result);
			this.Name = label.Replace(" ", "");
		}

		public void Clear() {
			txtLabel.Text = "";
			txtResult.Text = "";
		}

		public void DisplayResult(string label, string result) {
			txtLabel.Text = label;
			txtResult.Text = result;
		}

		private void txtLabel_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			TextBlock block = (TextBlock)sender;
			System.Windows.MessageBox.Show(this.Name);
		}

		public string GetData() {
			return txtLabel.Text + "\t" + txtResult.Text + "\n";
		}
	}
}
