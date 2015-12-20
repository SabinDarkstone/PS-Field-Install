using System.Windows;

namespace PS_Field_Install {
	/// <summary>
	/// Interaction logic for Waiting.xaml
	/// </summary>
	public partial class Waiting : Window {
		public Waiting() {
			InitializeComponent();
			txtblkLoading.Text = "";
		}

		public Waiting(string text) {
			InitializeComponent();
			txtblkLoading.Text = text;
		}

		public void ChangeText(string text) {
			txtblkLoading.Text = text;
		}
	}
}
