using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FingerPrintLibrary
{
    public static class ImageGenerator
    {
        public static Bitmap GenerateBitmap(int height, int width, byte[] data)
        {
            //it's a black and white image. One bit per pixel. Black or white.
            //take bits and add four 0's after it. 
            //var bits = new BitArray(data);

            //Bitmap bmp = new Bitmap(width, height, PixelFormat.Format1bppIndexed);
            //BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            //IntPtr p = bitmapData.Scan0;
            //for (int y = 0; y < bmp.Height; y++)
            //{
            //    for (int x = 0; x < bmp.Width; x += 2)
            //    {
            //        //var first = get byte from first four values in bits
            //        System.Runtime.InteropServices.Marshal.WriteByte(p, y * bitmapData.Stride + x, Convert.ToByte(bits[x + y]));
            //        System.Runtime.InteropServices.Marshal.WriteByte(p, y * bitmapData.Stride + x + 1, Convert.ToByte(bits[x + y]));
            //    }
            //}

            //ColorPalette pal = bmp.Palette;

            //for (int i = 0; i < 2; i++)
            //{
            //    var temp = i * 255;
            //    pal.Entries[i] = Color.FromArgb(255, temp, temp, temp);
            //}

            //bmp.Palette = pal;

            //return bmp;







            //return SaveAsBitmap(256, 288, data);

            //BitmapData bmpData = new BitmapData() { Height = 288, Width = 256, PixelFormat = PixelFormat.Format1bppIndexed, Stride = 256 };

            //Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
            //return new Bitmap(256, 288, 256, PixelFormat.Format1bppIndexed, bmpData.Scan0);

            ////Here create the Bitmap to the know height, width and format
            //Bitmap bmp = new Bitmap(256, 288, PixelFormat.Format8bppIndexed);
            ////Bitmap bmp = new Bitmap(Image.FromStream(new MemoryStream(data)), new Size(256, 288));
            ////Create a BitmapData and Lock all pixels to be written 
            //BitmapData bmpData = bmp.LockBits(
            //                     new Rectangle(0, 0, bmp.Width, bmp.Height),
            //                     ImageLockMode.WriteOnly, bmp.PixelFormat);

            ////Copy the data from the byte array into BitmapData.Scan0
            //Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            ////Unlock the pixels
            //bmp.UnlockBits(bmpData);

            ////Return the bitmap 
            //return bmp;

















            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            //map pixels to colors...
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            IntPtr p = bitmapData.Scan0;

            //using a byte pointer, is fastest, requires the unsafe switch
            //unsafe
            //{
            //    byte* pp = (byte*)(void*)p;
            //    for (int y = 0; y < bmp.Height; y++)
            //    {
            //        for (int x = 0; x < bmp.Width; x++)
            //        {
            //            //System.Runtime.InteropServices.Marshal.WriteByte(p, y * bitmapData.Stride + x, (byte)_rnd.Next(256));
            //            *pp = (byte)_rnd.Next(256);
            //            pp++;
            //        }
            //    }
            //}

            //if you dont want to use pointers
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x += 2)
                {
                    //need to decompose each byte into two BitArrays of 4 bits each for 16 degrees of grayscale
                    //Then set map each 4 bit array to a 256 gray degree value. (multiply by 16).
                    byte[] newOne = data.Skip((y * bitmapData.Stride / 2 + x / 2) / 8).Take(1).ToArray();
                    var bits = new BitArray(newOne);
                    var bittt = new BitArray(4, false);
                    bittt[0] = bits[0];
                    bittt[1] = bits[1];
                    bittt[2] = bits[2];
                    bittt[3] = bits[3];
                    int value = GetInteger(bittt) << 4;
                    //var first = get byte from first four values in bits
                    System.Runtime.InteropServices.Marshal.WriteByte(p, y * bitmapData.Stride + x, Convert.ToByte(value));

                    bittt[0] = bits[4];
                    bittt[1] = bits[5];
                    bittt[2] = bits[6];
                    bittt[3] = bits[7];
                    value = GetInteger(bittt) << 4;
                    System.Runtime.InteropServices.Marshal.WriteByte(p, y * bitmapData.Stride + x + 1, Convert.ToByte(value));
                }
            }

            bmp.UnlockBits(bitmapData);

            //get and fillup a grayscale-palette
            ColorPalette pal = bmp.Palette;

            for (int i = 0; i < 256; i++)
            {
                pal.Entries[i] = Color.FromArgb(255, i, i, i);
            }

            bmp.Palette = pal;

            return bmp;

            //Image bOld = this.pictureBox1.Image;
            //this.pictureBox1.Image = bmp;

            //if (bOld != null)
            //    bOld.Dispose();
        }

        private static int GetInteger(BitArray vals)
        {
            if (vals.Length != 4)
            {
                throw new ArgumentOutOfRangeException("vals", "BitArray length must be equal to 4");
            }

            int value = 0;

            for (int i = 0; i < vals.Length; i++)
            {
                int temp = 0;
                if (vals[i])
                {
                    temp = 1;
                }

                value += 2 * i * temp;
            }

            return value;
        }
        
        public static Bitmap SaveAsBitmap(int width, int height, byte[] imageData)
        {
            // Need to copy our 8 bit greyscale image into a 32bit layout.
            // Choosing 32bit rather than 24 bit as its easier to calculate stride etc.
            // This will be slow enough and isn't the most efficient method.
            var data = new byte[imageData.Length * 4];

            int o = 0;

            for (var i = 0; i < imageData.Length; i++)
            {
                var value = imageData[i];

                // Greyscale image so r, g, b, get the same
                // intensity value.
                data[o++] = value;
                data[o++] = value;
                data[o++] = value;
                data[o++] = 0;  // Alpha isn't actually used
            }

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    return new Bitmap(width, height, width * 4, PixelFormat.Format32bppRgb, new IntPtr(ptr));

                    //// Craete a bitmap wit a raw pointer to the data
                    //using (Bitmap image = new Bitmap(width, height, width * 4,
                    //            PixelFormat.Format32bppRgb, new IntPtr(ptr)))
                    //{
                    //    // And save it.
                    //    image.Save(Path.ChangeExtension(fileName, ".bmp"));
                    //}
                }
            }
        }
    }



    //static public class SerialPortExtensions
    //{
    //    public static Task<byte[]> ReadAsync(this SerialPort serialPort)
    //    {
    //        var tcs = new TaskCompletionSource<byte[]>();
    //        SerialDataReceivedEventHandler dataReceived = null;
    //        dataReceived = (s, e) =>
    //        {
    //            serialPort.DataReceived -= dataReceived;
    //            var buf = new byte[serialPort.BytesToRead];
    //            serialPort.Read(buf, 0, buf.Length);
    //            tcs.TrySetResult(buf);
    //        };
    //        serialPort.DataReceived += dataReceived;
    //        return tcs.Task;
    //    }
    //}
}
