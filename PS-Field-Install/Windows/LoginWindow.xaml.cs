using System.Windows;

namespace PS_Field_Install {
	/// <summary>
	/// Interaction logic for Login.xaml
	/// </summary>
	public partial class Login : Window {
		public Login() {
			InitializeComponent();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e) {
			DialogResult = false;
			this.Close();
		}

		private void btnLogin_Click(object sender, RoutedEventArgs e) {
			if (txtPassword.Password == "Lithonia" && txtUsername.Text == "Admin") {
				DialogResult = true;
				this.Close();
			} else {
				MessageBox.Show("Incorrect login credentials");
			}
		}
	}
}
