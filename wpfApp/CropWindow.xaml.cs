using System.Windows;
using ImageEditing;

namespace wpfApp
{
	/// <summary>
	/// Interaction logic for CropWindow.xaml
	/// </summary>
	public partial class CropWindow : Window
	{
		private Image image = null!;

		public CropWindow(Image image)
		{
			InitializeComponent();

			this.image = image;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int x1 = int.Parse(x1Text.Text);
				int y1 = int.Parse(y1Text.Text);
				int x2 = int.Parse(x2Text.Text);
				int y2 = int.Parse(y2Text.Text);

				image.Crop(x1, y1, x2, y2);

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
