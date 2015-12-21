using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PS_Field_Install.Controls {
	/// <summary>
	/// Interaction logic for CycleImageBox.xaml
	/// </summary>
	public partial class CycleImageBox : UserControl {

		private int index = 0;
		private DispatcherTimer timer;

		public string[] Images {
			get;
			set;
		}

		public CycleImageBox() {
			InitializeComponent();
		}

		public void Initiate() {
			index = 0;

			if (Images.Length > 0) {
				var uri = new Uri(Images[index]);
				image.Source = new BitmapImage(uri);

				this.timer = new DispatcherTimer(DispatcherPriority.Render);
				this.timer.Interval = TimeSpan.FromSeconds(1);
				this.timer.Tick += new EventHandler(this.image_UpdateImage);
				this.timer.Start();
			}
		}

		public void Stop() {
			if (this.timer != null) {
				this.timer.Stop();
				this.timer = null;
			}
		}

		private void image_UpdateImage(object sender, EventArgs e) {
			if (Images == null) {
				image.Source = null;
				return;
			}

			var uri = new Uri(Images[index]);
			image.Source = new BitmapImage(uri);
			index++;

			if (index > Images.Length - 1) {
				index = 0;
			}
		}
	}
}
