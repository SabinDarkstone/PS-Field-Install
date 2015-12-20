using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PS_Field_Install {

	public enum DialogTypes {
		Error,
		Info,
		Warning
	};

	public enum Buttons {
		Okay,
		OkayCancel,
		Close,
		None
	};

	/// <summary>
	/// Interaction logic for CustomDialog.xaml
	/// </summary>
	public partial class CustomDialog : Window {

		double delay = 0;

		public CustomDialog() {
			InitializeComponent();
		}

		public CustomDialog(string title, string message, object error = null, DialogTypes dialogType = DialogTypes.Info, Buttons button = Buttons.Okay) {
			InitializeComponent();

			this.Title = title;
			this.txtMessage.Text = message;
			this.txtError.Text = error.ToString();

			switch (dialogType) {
				case DialogTypes.Error:
					picImage.Source = new BitmapImage(new Uri("/Error.png", UriKind.Relative));
					break;
				case DialogTypes.Info:

					break;
				case DialogTypes.Warning:

					break;
				default:

					break;
			}

			switch (button) {
				case Buttons.Close:
					this.grid.Children.Remove(btnCancel);
					btnOkay.Content = "Close";
					break;
				case Buttons.None:
					this.grid.Children.Remove(btnOkay);
					this.grid.Children.Remove(btnCancel);
					StartCloseTimer();
					break;
				case Buttons.Okay:
					this.grid.Children.Remove(btnCancel);
					break;
				case Buttons.OkayCancel:

					break;
				default:
					this.grid.Children.Remove(btnCancel);
					break;
			}
		}

		public void SetTimer(double seconds) {
			if (delay == 0) {
				delay = 5;
			}
			delay = seconds;
		}

		private void StartCloseTimer() {
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(delay);
			timer.Tick += timer_Tick;
			timer.Start();
		}

		private void timer_Tick(object sender, EventArgs e) {
			DispatcherTimer timer = (DispatcherTimer)sender;
			timer.Stop();
			timer.Tick -= timer_Tick;
			Close();
		}

	}
}
