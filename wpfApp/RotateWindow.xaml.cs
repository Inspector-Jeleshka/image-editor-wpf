using System.Windows;
using ImageEditing;

namespace wpfApp
{
	/// <summary>
	/// Interaction logic for RotateWindow.xaml
	/// </summary>
	public partial class RotateWindow : Window
	{
		private Image image = null!;

		public RotateWindow(Image image)
		{
			InitializeComponent();

			this.image = image;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				double angle = double.Parse(angleText.Text);
				int x = int.Parse(xText.Text);
				int y = int.Parse(yText.Text);

				image.Rotate(angle, x, y);

				DialogResult = true;
				Close();
			}
			catch (Exception ex)
			{
				ValidationErrorText.Text = ex.Message;
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}
	}
}
