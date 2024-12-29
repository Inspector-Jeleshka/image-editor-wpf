namespace ImageEditing
{
	public class Image
	{
		private readonly string path = null!;
		private int fileSize;
		private int pixelArrayOffset;
		private DIBHeader header;
		private Pixel[,] pixelArray = null!;


		public Image(string path)
		{
			this.path = path;
			using FileStream file = File.OpenRead(path);
			byte[] fileData = new byte[file.Length];
			file.Read(fileData, 0, (int)file.Length);

			if (BitConverter.ToInt16(fileData, 0) is not (0x424d or 0x4d42))
				throw new NotSupportedException("Incorrect signature");
			if (BitConverter.ToInt32(fileData, 22) < 0)
				throw new NotSupportedException("Unsupported pixel array layout (negative image height)");
			if (BitConverter.ToInt16(fileData, 28) is not 24)
				throw new NotSupportedException("Unsupported color depth");
			if (BitConverter.ToInt32(fileData, 30) != 0)
				throw new NotSupportedException("Unsupported compression method");
			if (BitConverter.ToInt32(fileData, 46) != 0)
				throw new NotSupportedException("Unsupported color table");

			fileSize = BitConverter.ToInt32(fileData, 2);
			pixelArrayOffset = BitConverter.ToInt32(fileData, 10);
			int imageWidth = BitConverter.ToInt32(fileData, 18);
			int imageHeight = BitConverter.ToInt32(fileData, 22);
			int bitsPerPixel = BitConverter.ToInt16(fileData, 28);
			int imageSize = BitConverter.ToInt32(fileData, 34);
			int horizontalResolution = BitConverter.ToInt32(fileData, 38);
			int verticalResolution = BitConverter.ToInt32(fileData, 42);
			int colorTableSize = BitConverter.ToInt32(fileData, 46);

			header = new(imageWidth, imageHeight, imageSize, horizontalResolution, verticalResolution, colorTableSize);
			pixelArray = new Pixel[imageHeight, imageWidth];

			for (int i = 0; i < imageHeight; i++)
				for (int j = 0; j < imageWidth; j++)
				{
					byte blue = fileData[pixelArrayOffset + (imageHeight - i - 1) * RowSize + j * bitsPerPixel / 8];
					byte green = fileData[pixelArrayOffset + (imageHeight - i - 1) * RowSize + j * bitsPerPixel / 8 + 1];
					byte red = fileData[pixelArrayOffset + (imageHeight - i - 1) * RowSize + j * bitsPerPixel / 8 + 2];

					pixelArray[i, j] = new(red, green, blue);
				}
		}


		public string Path => path;
		public int FileSize => fileSize;
		public DIBHeader Header => header;
		public int RowSize => (int)Math.Ceiling(header.bitsPerPixel * header.bmpWidth / 32d) * 4;


		public void Crop(int x1, int y1, int x2, int y2)
		{
			if (x1 < 0 || x2 < 0 || y1 < 0 || y2 < 0)
				throw new ArgumentOutOfRangeException(nameof(x1), "Less than 0");

			int newImageWidth = Math.Abs(x2 - x1);
			int newImageHeight = Math.Abs(y2 - y1);
			if (newImageWidth == 0 || newImageHeight == 0)
				throw new ArgumentException("Image can't have side with a 0 length");

			Pixel[,] croppedImage = new Pixel[newImageHeight, newImageWidth];
			x1 = x1 < x2 ? x1 : x2;
			y1 = y1 < y2 ? y1 : y2;

			for (int i = 0; i < newImageHeight; i++)
				for (int j = 0; j < newImageWidth; j++)
					croppedImage[i, j] = pixelArray[y1 + i, x1 + j];

			pixelArray = croppedImage;
			header.bmpHeight = newImageHeight;
			header.bmpWidth = newImageWidth;
			header.bmpSize = RowSize * newImageHeight;
			fileSize = pixelArrayOffset + header.bmpSize;
		}

		// Counterclockwise rotation around (x, y) pixel, angle in degrees
		public void Rotate(double angle, int x, int y)
		{
			if (x < 0 || y < 0)
				throw new ArgumentOutOfRangeException(x < 0 ? nameof(x) : nameof(y), "Less than 0");

			int imageHeight = header.bmpHeight, imageWidth = header.bmpWidth;
			Pixel[,] rotatedPixelArray = new Pixel[imageHeight, imageWidth];
			angle = angle * Math.PI / 180;

			for (int i = 0; i < imageHeight; i++)
				for (int j = 0; j < imageWidth; j++)
				{
					int xActual = j - x;
					int yActual = i - y;
					int xRotated = (int)(xActual * Math.Cos(angle) - yActual * Math.Sin(angle));
					int yRotated = (int)(xActual * Math.Sin(angle) + yActual * Math.Cos(angle));
					xRotated += x;
					yRotated += y;

					if (xRotated >= 0 && xRotated < imageWidth && yRotated >= 0 && yRotated < imageHeight)
						rotatedPixelArray[i, j] = pixelArray[yRotated, xRotated];
				}

			pixelArray = rotatedPixelArray;
		}

		public void MakeCollage(Image image, int xPos, int yPos)
		{
			int rightEnd = Math.Max(image.header.bmpWidth + xPos, header.bmpWidth);
			int leftEnd = Math.Min(xPos, 0);
			int lowEnd = Math.Max(image.header.bmpHeight + yPos, header.bmpHeight);
			int highEnd = Math.Min(yPos, 0);
			int width = rightEnd - leftEnd;
			int height = lowEnd - highEnd;

			Pixel[,] collage = new Pixel[height, width];

			for (int y = 0; y < header.bmpHeight; y++)
				for (int x = 0; x < header.bmpWidth; x++)
					collage[y - highEnd, x - leftEnd] = pixelArray[y, x];
			for (int y = 0; y < image.header.bmpHeight; y++)
				for (int x = 0; x < image.header.bmpWidth; x++)
					collage[y + yPos - highEnd, x + xPos - leftEnd] = image.pixelArray[y, x];

			pixelArray = collage;
			header.bmpWidth = width;
			header.bmpHeight = height;
			header.bmpSize = height * RowSize;
			fileSize = pixelArrayOffset + header.bmpSize;
		}

		public void AddTitle(string title, int x, int y)
		{
			for (int i = 0; i < title.Length; i++)
			{
				char c = char.ToUpper(title[i]);
				if (!Font.font.ContainsKey(c))
					continue;

				byte[] charBitmap = Font.font[c];
				for (int row = 0; row < Font.charHeight; row++)
					for (int col = 0; col < Font.charWidth; col++)
					{
						int pixelX = x + col + i * (Font.charWidth + 1);
						int pixelY = y + row;
						if (pixelX >= header.bmpWidth || pixelY >= header.bmpHeight || pixelX < 0 || pixelY < 0)
							continue;

						if ((charBitmap[row] & (1 << (Font.charWidth - 1 - col))) != 0 )
							SetPixel(pixelX, pixelY, 0, 0, 0);
					}
			}
		}

		public void SetPixel(int x, int y, byte red, byte green, byte blue)
		{
			SetPixel(x, y, new(red, green, blue));
		}
		public void SetPixel(int x, int y, Pixel pixel)
		{
			if (x < 0 || x >= header.bmpWidth)
				throw new ArgumentOutOfRangeException(nameof(x), "Outside the pixel array");
			if (y < 0 || y >= header.bmpHeight)
				throw new ArgumentOutOfRangeException(nameof(y), "Outside the pixel array");

			pixelArray[y, x] = pixel;
		}

		public void EditBrightness(int value)
		{
			for (int i = 0; i < header.bmpHeight; i++)
				for (int j = 0; j < header.bmpWidth; j++)
				{
					int red = pixelArray[i, j].Red + value;
					int green = pixelArray[i, j].Green + value;
					int blue = pixelArray[i, j].Blue + value;

					pixelArray[i, j] = new(red, green, blue);
				}
		}

		public void EditContrast(double value)
		{
			if (value < -254)
				throw new ArgumentOutOfRangeException(nameof(value), "Less than -254");
			if (value > 258)
				throw new ArgumentOutOfRangeException(nameof(value), "Greater than 258");
			double k = 259d * (value + 255d) / (255d * (259d - value));

			for (int i = 0; i < header.bmpHeight; i++)
				for (int j = 0; j < header.bmpWidth; j++)
				{
					int red = (int)(k * (pixelArray[i, j].Red - 128) + 128);
					int green = (int)(k * (pixelArray[i, j].Green - 128) + 128);
					int blue = (int)(k * (pixelArray[i, j].Blue - 128) + 128);

					pixelArray[i, j] = new(red, green, blue);
				}
		}

		public void SaveToFile()
		{
			SaveToFile(path);
		}
		public void SaveToFile(string path)
		{
			try
			{
				using FileStream file = File.OpenWrite(path);

				List<byte> data = new();
				data.AddRange(BitConverter.GetBytes((short)0x4d42));
				data.AddRange(BitConverter.GetBytes(fileSize));
				data.AddRange(BitConverter.GetBytes(0));
				data.AddRange(BitConverter.GetBytes(pixelArrayOffset));
				data.AddRange(BitConverter.GetBytes(header.headerSize));
				data.AddRange(BitConverter.GetBytes(header.bmpWidth));
				data.AddRange(BitConverter.GetBytes(header.bmpHeight));
				data.AddRange(BitConverter.GetBytes(header.colorPlanes));
				data.AddRange(BitConverter.GetBytes(header.bitsPerPixel));
				data.AddRange(BitConverter.GetBytes(header.compression));
				data.AddRange(BitConverter.GetBytes(header.bmpSize));
				data.AddRange(BitConverter.GetBytes(header.horizontalResolution));
				data.AddRange(BitConverter.GetBytes(header.verticalResolution));
				data.AddRange(BitConverter.GetBytes(header.colorTableSize));
				data.AddRange(BitConverter.GetBytes(header.importantColors));
				byte[] gap = new byte[pixelArrayOffset - 14 - header.headerSize];
				data.AddRange(gap);

				byte[] padding = new byte[RowSize - header.bmpWidth * header.bitsPerPixel / 8];
				for (int i = header.bmpHeight - 1; i >= 0; i--)
				{
					for (int j = 0; j < header.bmpWidth; j++)
					{
						data.Add(pixelArray[i, j].Blue);
						data.Add(pixelArray[i, j].Green);
						data.Add(pixelArray[i, j].Red);
					}
					data.AddRange(padding);
				}

				if (data.Count % 4 != 0)
					data.AddRange(new byte[4 - data.Count % 4]);

				file.Write(data.ToArray(), 0, fileSize);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}


		public struct Pixel
		{
			public byte Red;
			public byte Green;
			public byte Blue;

			public Pixel(byte red, byte green, byte blue)
			{
				Red = red;
				Green = green;
				Blue = blue;
			}
			public Pixel(int red, int green, int blue)
			{
				Func<int, byte> normalize = (value) =>
				{
					if (value > 255)
						return 255;
					if (value < 0)
						return 0;
					return (byte)value;
				};
				Red = normalize(red);
				Green = normalize(green);
				Blue = normalize(blue);
			}
		}
		public struct DIBHeader
		{
			public readonly int headerSize = 40;
			public int bmpWidth;
			public int bmpHeight;
			public readonly short colorPlanes = 1;
			public readonly short bitsPerPixel = 24;
			public readonly int compression = 0;
			public int bmpSize;
			public int horizontalResolution;
			public int verticalResolution;
			public int colorTableSize;
			public readonly int importantColors = 0;

			public DIBHeader(int bmpWidth, int bmpHeight, int bmpSize, int horizontalResolution, int verticalResolution, int colorTableSize)
			{
				this.bmpWidth = bmpWidth;
				this.bmpHeight = bmpHeight;
				this.bmpSize = bmpSize;
				this.horizontalResolution = horizontalResolution;
				this.verticalResolution = verticalResolution;
				this.colorTableSize = colorTableSize;
			}
		}
		private readonly struct Font
		{
			public static readonly int charWidth = 5;
			public static readonly int charHeight = 7;
			public static readonly Dictionary<char, byte[]> font = new()
			{
				{ 'A', new byte[] { 0b00100, 0b01010, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001 } },
				{ 'B', new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10001, 0b10001, 0b11110 } },
				{ 'C', new byte[] { 0b01110, 0b10001, 0b10000, 0b10000, 0b10000, 0b10001, 0b01110 } },
				{ 'D', new byte[] { 0b11100, 0b10010, 0b10001, 0b10001, 0b10001, 0b10010, 0b11100 } },
				{ 'E', new byte[] { 0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b11111 } },
				{ 'F', new byte[] { 0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b10000 } },
				{ 'G', new byte[] { 0b01110, 0b10001, 0b10000, 0b10000, 0b10011, 0b10001, 0b01110 } },
				{ 'H', new byte[] { 0b10001, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b10001 } },
				{ 'I', new byte[] { 0b01110, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110 } },
				{ 'J', new byte[] { 0b00011, 0b00001, 0b00001, 0b00001, 0b00001, 0b10001, 0b01110 } },
				{ 'K', new byte[] { 0b10001, 0b10010, 0b10100, 0b11000, 0b10100, 0b10010, 0b10001 } },
				{ 'L', new byte[] { 0b10000, 0b10000, 0b10000, 0b10000, 0b10000, 0b10000, 0b11111 } },
				{ 'M', new byte[] { 0b10001, 0b11011, 0b10101, 0b10101, 0b10001, 0b10001, 0b10001 } },
				{ 'N', new byte[] { 0b10001, 0b10001, 0b11001, 0b10101, 0b10011, 0b10001, 0b10001 } },
				{ 'O', new byte[] { 0b01110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110 } },
				{ 'P', new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10000, 0b10000, 0b10000 } },
				{ 'Q', new byte[] { 0b01110, 0b10001, 0b10001, 0b10001, 0b10011, 0b10010, 0b01101 } },
				{ 'R', new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10100, 0b10010, 0b10001 } },
				{ 'S', new byte[] { 0b01110, 0b10001, 0b10000, 0b01110, 0b00001, 0b10001, 0b01110 } },
				{ 'T', new byte[] { 0b11111, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100 } },
				{ 'U', new byte[] { 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110 } },
				{ 'V', new byte[] { 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01010, 0b00100 } },
				{ 'W', new byte[] { 0b10001, 0b10001, 0b10001, 0b10101, 0b10101, 0b11011, 0b10001 } },
				{ 'X', new byte[] { 0b10001, 0b10001, 0b01010, 0b00100, 0b01010, 0b10001, 0b10001 } },
				{ 'Y', new byte[] { 0b10001, 0b10001, 0b10001, 0b01110, 0b00100, 0b00100, 0b00100 } },
				{ 'Z', new byte[] { 0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b10000, 0b11111 } },

				{ '0', new byte[] { 0b01110, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b01110 } },
				{ '1', new byte[] { 0b00100, 0b01100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110 } },
				{ '2', new byte[] { 0b01110, 0b10001, 0b00001, 0b00110, 0b01000, 0b10000, 0b11111 } },
				{ '3', new byte[] { 0b01110, 0b10001, 0b00001, 0b00110, 0b00001, 0b10001, 0b01110 } },
				{ '4', new byte[] { 0b00010, 0b00110, 0b01010, 0b10010, 0b11111, 0b00010, 0b00010 } },
				{ '5', new byte[] { 0b11111, 0b10000, 0b11110, 0b00001, 0b00001, 0b10001, 0b01110 } },
				{ '6', new byte[] { 0b01110, 0b10001, 0b10000, 0b11110, 0b10001, 0b10001, 0b01110 } },
				{ '7', new byte[] { 0b11111, 0b00001, 0b00010, 0b00100, 0b00100, 0b00100, 0b00100 } },
				{ '8', new byte[] { 0b01110, 0b10001, 0b10001, 0b01110, 0b10001, 0b10001, 0b01110 } },
				{ '9', new byte[] { 0b01110, 0b10001, 0b10001, 0b01111, 0b00001, 0b10001, 0b01110 } },

				{ 'А', new byte[] { 0b00100, 0b01010, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001 } },
				{ 'Б', new byte[] { 0b11111, 0b10000, 0b11110, 0b10001, 0b10001, 0b10001, 0b11110 } },
				{ 'В', new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10001, 0b10001, 0b11110 } },
				{ 'Г', new byte[] { 0b11111, 0b10000, 0b10000, 0b10000, 0b10000, 0b10000, 0b10000 } },
				{ 'Д', new byte[] { 0b01110, 0b01010, 0b01010, 0b01010, 0b11111, 0b10001, 0b10001 } },
				{ 'Е', new byte[] { 0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b11111 } },
				{ 'Ж', new byte[] { 0b10001, 0b10101, 0b10101, 0b01110, 0b10101, 0b10101, 0b10001 } },
				{ 'З', new byte[] { 0b01110, 0b10001, 0b00001, 0b00110, 0b00001, 0b10001, 0b01110 } },
				{ 'И', new byte[] { 0b10001, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b10001 } },
				{ 'Й', new byte[] { 0b10101, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b10001 } },
				{ 'К', new byte[] { 0b10001, 0b10010, 0b10100, 0b11000, 0b10100, 0b10010, 0b10001 } },
				{ 'Л', new byte[] { 0b01111, 0b01001, 0b01001, 0b01001, 0b01001, 0b10001, 0b10001 } },
				{ 'М', new byte[] { 0b10001, 0b11011, 0b10101, 0b10101, 0b10001, 0b10001, 0b10001 } },
				{ 'Н', new byte[] { 0b10001, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b10001 } },
				{ 'О', new byte[] { 0b01110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110 } },
				{ 'П', new byte[] { 0b11111, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001 } },
				{ 'Р', new byte[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10000, 0b10000, 0b10000 } },
				{ 'С', new byte[] { 0b01110, 0b10001, 0b10000, 0b10000, 0b10000, 0b10001, 0b01110 } },
				{ 'Т', new byte[] { 0b11111, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100 } },
				{ 'У', new byte[] { 0b10001, 0b10001, 0b10001, 0b01111, 0b00001, 0b10001, 0b01110 } },
				{ 'Ф', new byte[] { 0b00100, 0b01110, 0b10101, 0b10101, 0b10101, 0b01110, 0b00100 } },
				{ 'Х', new byte[] { 0b10001, 0b10001, 0b01010, 0b00100, 0b01010, 0b10001, 0b10001 } },
				{ 'Ц', new byte[] { 0b10010, 0b10010, 0b10010, 0b10010, 0b10010, 0b11111, 0b00001 } },
				{ 'Ч', new byte[] { 0b10001, 0b10001, 0b10001, 0b01111, 0b00001, 0b00001, 0b00001 } },
				{ 'Ш', new byte[] { 0b10101, 0b10101, 0b10101, 0b10101, 0b10101, 0b10101, 0b11111 } },
				{ 'Щ', new byte[] { 0b10101, 0b10101, 0b10101, 0b10101, 0b10101, 0b11111, 0b00001 } },
				{ 'Ъ', new byte[] { 0b11000, 0b01000, 0b01110, 0b01001, 0b01001, 0b01001, 0b01110 } },
				{ 'Ы', new byte[] { 0b10001, 0b10001, 0b10001, 0b11101, 0b10011, 0b10011, 0b11101 } },
				{ 'Ь', new byte[] { 0b10000, 0b10000, 0b10000, 0b11110, 0b10001, 0b10001, 0b11110 } },
				{ 'Э', new byte[] { 0b01110, 0b10001, 0b00001, 0b01111, 0b00001, 0b10001, 0b01110 } },
				{ 'Ю', new byte[] { 0b10110, 0b10101, 0b10101, 0b11101, 0b10101, 0b10101, 0b10110 } },
				{ 'Я', new byte[] { 0b01110, 0b10001, 0b10001, 0b01111, 0b00101, 0b01001, 0b10001 } },
			};

			public Font()
			{ }
		}
	}
}
