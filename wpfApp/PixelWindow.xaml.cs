using System.Windows;
using ImageEditing;

namespace wpfApp
{
	/// <summary>
	/// Interaction logic for PixelWindow.xaml
	/// </summary>
	public partial class PixelWindow : Window
	{
		private Image image = null!;

		public PixelWindow(Image image)
		{
			InitializeComponent();

			this.image = image;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int x = int.Parse(xText.Text);
				int y = int.Parse(yText.Text);
				byte r = byte.Parse(redText.Text);
				byte g = byte.Parse(greenText.Text);
				byte b = byte.Parse(blueText.Text);

				image.SetPixel(x, y, r, g, b);

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
