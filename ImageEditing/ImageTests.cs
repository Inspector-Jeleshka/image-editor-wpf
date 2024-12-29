using NUnit.Framework;

namespace ImageEditing.Tests
{
	[TestFixture]
	public class ImageTests
	{
		private const string TestImagePath = "test_image.bmp";
		private const string OutputImagePath = "output_image.bmp";

		[SetUp]
		public void Setup()
		{
			using FileStream fs = File.Create(TestImagePath);

			byte[] bmpHeader = new byte[54];
			bmpHeader[0] = 0x42;
			bmpHeader[1] = 0x4D;
			int fileSize = 54 + 3 * 2 * 2 + 4 + 2;
			Buffer.BlockCopy(BitConverter.GetBytes(fileSize), 0, bmpHeader, 2, 4);
			bmpHeader[10] = 54;
			bmpHeader[14] = 40;
			Buffer.BlockCopy(BitConverter.GetBytes(2), 0, bmpHeader, 18, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(2), 0, bmpHeader, 22, 4);
			bmpHeader[26] = 1;
			bmpHeader[28] = 24;
			bmpHeader[34] = 16;
			fs.Write(bmpHeader, 0, bmpHeader.Length);

			byte[] pixels = {
					255, 0, 0, 0, 255, 0, 0, 0,
					0, 0, 255, 255, 255, 255, 0, 0
				};
			fs.Write(pixels, 0, pixels.Length);
			fs.Write(new byte[2], 0, 2);
		}

		[Test]
		public void TestImageConstructor()
		{
			Image image = new(TestImagePath);

			Assert.That(image.Header.bmpWidth, Is.EqualTo(2));
			Assert.That(image.Header.bmpHeight, Is.EqualTo(2));
		}

		[Test]
		public void TestCrop()
		{
			Image image = new(TestImagePath);
			image.Crop(0, 0, 1, 1);

			Assert.That(image.Header.bmpWidth, Is.EqualTo(1));
			Assert.That(image.Header.bmpHeight, Is.EqualTo(1));
		}

		[Test]
		public void TestRotate()
		{
			Image image = new(TestImagePath);
			image.Rotate(90, 1, 1);

			Assert.DoesNotThrow(() => image.SaveToFile(OutputImagePath));
		}

		[Test]
		public void TestAddTitle()
		{
			Image image = new(TestImagePath);
			image.AddTitle("A", 0, 0);

			image.SaveToFile(OutputImagePath);
			Assert.That(File.Exists(OutputImagePath), Is.True);
		}

		[Test]
		public void TestMakeCollage()
		{
			Image baseImage = new(TestImagePath);
			Image overlayImage = new(TestImagePath);

			baseImage.MakeCollage(overlayImage, 1, 1);

			Assert.That(baseImage.Header.bmpWidth, Is.EqualTo(3));
			Assert.That(baseImage.Header.bmpHeight, Is.EqualTo(3));
		}

		[Test]
		public void TestSetPixel()
		{
			Image image = new(TestImagePath);

			image.SetPixel(0, 0, 128, 128, 128);

			Assert.DoesNotThrow(() => image.SaveToFile());
		}

		[Test]
		public void TestEditBrightness()
		{
			Image image = new(TestImagePath);

			image.EditBrightness(50);
			image.EditBrightness(-100);

			Assert.DoesNotThrow(() => image.SaveToFile());
		}

		[Test]
		public void TestEditContrast()
		{
			Image image = new(TestImagePath);

			image.EditContrast(50);
			image.EditContrast(-50);

			Assert.DoesNotThrow(() => image.SaveToFile());
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(OutputImagePath))
				File.Delete(OutputImagePath);
		}
	}
}
