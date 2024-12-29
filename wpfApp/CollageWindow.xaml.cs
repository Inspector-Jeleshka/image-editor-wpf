using ImageEditing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wpfApp
{
	/// <summary>
	/// Interaction logic for CollageWindow.xaml
	/// </summary>
	public partial class CollageWindow : Window
	{
		private Image image = null!;
		private Image image1 = null!;

		public CollageWindow(Image image)
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

				image.MakeCollage(image1, x, y);

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

		private void addImageButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new()
			{
				FileName = "Изображение",
				DefaultExt = ".bmp",
				Filter = "BMP изображение|*.bmp"
			};

			if (fileDialog.ShowDialog() != true)
				return;

			try
			{
				image1 = new(fileDialog.FileName);

				chosenImagePath.Text = $"Выбранное изображение: {image1.Path}";
			}
			catch (Exception ex)
			{
				ValidationErrorText.Text = ex.Message;
			}
		}
	}
}
