using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ImageEditing;

namespace wpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Image image = null!;
        private BitmapImage bmp = null!;
        private readonly string tempImagePath = @"temp.bmp";


		public MainWindow()
        {
            InitializeComponent();
        }

		private void updateImage()
		{
			image.SaveToFile(tempImagePath);

			string tempImagePathAbsolute = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempImagePath);

			using var fileStream = File.OpenRead(tempImagePathAbsolute);

			bmp = new BitmapImage();
			bmp.BeginInit();
			bmp.StreamSource = fileStream;
			bmp.CacheOption = BitmapCacheOption.OnLoad;
			bmp.EndInit();

			imageViewer.Source = bmp;
			pathText.Text = image.Path;
			imageSizeText.Text = $"Длина: {image.Header.bmpWidth}, Ширина: {image.Header.bmpHeight}";
		}

        private void MainWindow_Closed(object sender, EventArgs e)
        {
			if (File.Exists(tempImagePath))
				File.Delete(tempImagePath);
		}

		private void openImageButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new()
			{
				FileName = "Изображение",
				DefaultExt = ".bmp",
				Filter = "BMP изображение|*.bmp"
			};

            if (fileDialog.ShowDialog() != true)
                return;
            
            string filename = fileDialog.FileName;

			try
            {
                image = new(filename);

				updateImage();

                imagePlaceholder.Visibility = Visibility.Collapsed;
				saveImageButton.IsEnabled = true;
				actionPanel.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
		}

		private void saveImageButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				image.SaveToFile();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void cropImageButton_Click(object sender, RoutedEventArgs e)
		{
			var crop = new CropWindow(image);

			if (crop.ShowDialog() == true)
				updateImage();
		}

		private void rotateImageButton_Click(object sender, RoutedEventArgs e)
		{
			var rotate = new RotateWindow(image);

			if (rotate.ShowDialog() == true)
				updateImage();
		}

		private void makeCollageButton_Click(object sender, RoutedEventArgs e)
		{
			var collage = new CollageWindow(image);

			if (collage.ShowDialog() == true)
				updateImage();
		}

		private void addTitleButton_Click(object sender, RoutedEventArgs e)
		{
			var title = new TitleWindow(image);

			if (title.ShowDialog() == true)
				updateImage();
		}

		private void setPixelButton_Click(object sender, RoutedEventArgs e)
		{
			var pixel = new PixelWindow(image);

			if (pixel.ShowDialog() == true)
				updateImage();
		}

		private void editBrightnessButton_Click(object sender, RoutedEventArgs e)
		{
			var brightness = new BrightnessWindow(image);

			if (brightness.ShowDialog() == true)
				updateImage();
		}

		private void editContrastButton_Click(object sender, RoutedEventArgs e)
		{
			var contrast = new ContrastWindow(image);

			if (contrast.ShowDialog() == true)
				updateImage();
		}
	}
}