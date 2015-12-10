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

		public BitmapImage[] Images {
			get;
			set;
		}

		public CycleImageBox() {
			InitializeComponent();
		}

		public void Initiate() {
			index = 0;

			if (Images.Length > 0) {
				image.Source = Images[index];

				this.timer = new DispatcherTimer(DispatcherPriority.Render);
				this.timer.Interval = TimeSpan.FromSeconds(1);
				this.timer.Tick += new EventHandler(this.image_UpdateImage);
				this.timer.Start();
			}
		}

		private void image_UpdateImage(object sender, EventArgs e) {
			image.Source = Images[index];
			index++;

			if (index > Images.Length - 1) {
				index = 0;
			}
		}
	}
}
