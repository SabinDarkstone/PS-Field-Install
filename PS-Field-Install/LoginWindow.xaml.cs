using System.Windows;
using PS_Field_Install.Scripts;

namespace PS_Field_Install {
	/// <summary>
	/// Interaction logic for Login.xaml
	/// </summary>
	public partial class Login : Window {
		public Login() {
			InitializeComponent();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e) {
			LogHelper.Log.Debug("Login.btnCancel_Click(sender, e)");
			DialogResult = false;
			this.Close();
		}

		private void btnLogin_Click(object sender, RoutedEventArgs e) {
			LogHelper.Log.Debug("Login.btnLogin_Click(sender, e)");
			if (txtPassword.Password == "Lithonia" && txtUsername.Text == "Admin") {
				DialogResult = true;
				this.Close();
			} else {
				MessageBox.Show("Incorrect login credentials");
			}
		}

		private async void Window_Unloaded(object sender, RoutedEventArgs e) {
			await LogHelper.UploadLog();
		}
	}
}
