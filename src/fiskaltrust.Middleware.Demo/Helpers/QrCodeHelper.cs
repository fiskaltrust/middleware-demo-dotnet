using System;
using System.Collections.Generic;
using fiskaltrust.ifPOS.v1;

namespace fiskaltrust.Middleware.Demo
{
    public static class QrCodeHelper
    {
        public static void PrintQrCode(SignaturItem signaturItem)
        {
            var width = 128;
            var ft = QRTextChars(signaturItem.Data, width, true);

            var line = 0;
            Console.WriteLine(signaturItem.Data);
            Console.WriteLine("============================================================");
            while (line * width < ft.Length)
            {
                Console.WriteLine(ft, width * line++, width);
            }
            Console.WriteLine("============================================================");
        }

        public static char[] QRTextChars(string data, int width = 64, bool invers = false)
        {
            var bytes = QRTextBytes(data, width, invers);

            var chars = new char[bytes.Length];

            var line = 0;
            while (bytes.Length > line * width)
            {
                try
                {
                    for (var i = 0; i < width; i++)
                    {
                        var pos = (line * width) + i;
                        if (bytes[pos] == 0xDB)
                        {
                            chars[pos] = (char)0x2588;
                        }
                        else if (bytes[pos] == 0xDC)
                        {
                            chars[pos] = (char)0x2584;
                        }
                        else if (bytes[pos] == 0xDF)
                        {
                            chars[pos] = (char)0x2580;
                        }
                        else
                        {
                            chars[pos] = (char)0x20;
                        }
                    }
                }
                catch (Exception)
                {
                }
                line++;
            }
            return chars;
        }

        public static byte[] QRTextBytes(string data, int width = 64, bool invers = false)
        {
            var writer = new ZXing.QrCode.QRCodeWriter();

            var hints = new Dictionary<ZXing.EncodeHintType, object>
            {
                { ZXing.EncodeHintType.CHARACTER_SET, "ISO-8859-1" },
                { ZXing.EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.M }
            };

            var matrix = writer.encode(data, ZXing.BarcodeFormat.QR_CODE, width, width, hints);
            var bytes = new List<byte>();
            var y = 0;
            while (y < width)
            {

                for (var x = 0; x < width; x++)
                {
                    if (matrix[x, y] == true && matrix[x, y + 1] == true)
                    {
                        bytes.Add(219);
                    }
                    else if (matrix[x, y] == true && matrix[x, y + 1] == false)
                    {
                        bytes.Add(223);
                    }
                    else if (matrix[x, y] == false && matrix[x, y + 1] == true)
                    {
                        bytes.Add(220);
                    }
                    else
                    {
                        bytes.Add(32);
                    }
                }
                y += 2;
            }

            var result = bytes.ToArray();

            if (invers)
            {
                for (var i = 0; i < result.Length; i++)
                {
                    if (result[i] == 0xDB)
                    {
                        result[i] = 0x20;
                    }
                    else if (result[i] == 0x20)
                    {
                        result[i] = 0xDB;
                    }
                    else if (result[i] == 0xDC)
                    {
                        result[i] = 0xDF;
                    }
                    else if (result[i] == 0xDF)
                    {
                        result[i] = 0xDC;
                    }
                }
            }

            return result;
        }
    }
}
