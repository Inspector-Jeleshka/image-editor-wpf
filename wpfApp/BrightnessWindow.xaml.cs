using System.Windows;
using System.Xml.Linq;
using ImageEditing;

namespace wpfApp
{
	/// <summary>
	/// Interaction logic for BrightnessWindow.xaml
	/// </summary>
	public partial class BrightnessWindow : Window
	{
		private Image image = null!;

		public BrightnessWindow(Image image)
		{
			InitializeComponent();

			this.image = image;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int value = int.Parse(brightnessText.Text);

				image.EditBrightness(value);

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
