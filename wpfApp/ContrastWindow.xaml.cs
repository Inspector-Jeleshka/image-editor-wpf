using System.Windows;
using ImageEditing;

namespace wpfApp
{
	/// <summary>
	/// Interaction logic for ContrastWindow.xaml
	/// </summary>
	public partial class ContrastWindow : Window
	{
		private Image image = null!;

		public ContrastWindow(Image image)
		{
			InitializeComponent();

			this.image = image;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int value = int.Parse(contrastText.Text);

				image.EditContrast(value);

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
