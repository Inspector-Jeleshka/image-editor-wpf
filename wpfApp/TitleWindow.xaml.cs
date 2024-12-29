using System.Windows;
using ImageEditing;

namespace wpfApp
{
	/// <summary>
	/// Interaction logic for TitleWindow.xaml
	/// </summary>
	public partial class TitleWindow : Window
	{
		private Image image = null!;

		public TitleWindow(Image image)
		{
			InitializeComponent();

			this.image = image;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string title = !string.IsNullOrEmpty(titleText.Text) ? titleText.Text
					: throw new ArgumentException("Пустое значение текста");
				int x = int.Parse(xText.Text);
				int y = int.Parse(yText.Text);

				image.AddTitle(title, x, y);

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
