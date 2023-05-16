using System.Diagnostics.CodeAnalysis;
using System.IO;
using SkiaSharp;

namespace WoWsShipBuilder.Desktop.Utilities
{
    /// <summary>
    /// Utilities to store and extract build information in the rendered images.
    /// Concept from https://www.codeproject.com/Tips/635715/Steganography-Simple-Implementation-in-Csharp with style modifications.
    /// </summary>
    public static class BuildImageProcessor
    {
        private enum State
        {
            EncodingText,
            FinishingSequence,
        }

        /// <summary>
        /// Embeds the provided string into an image without visibly changing the image.
        /// </summary>
        /// <param name="bitmapData">The bitmap data as stream.</param>
        /// <param name="content">The content to embed in the image file.</param>
        /// <param name="outputPath">The path of the output image file.</param>
        [SuppressMessage("System.IO.Abstractions", "IO0002", Justification = "This class needs to be tested with real files.")]
        public static void AddTextToBitmap(Stream bitmapData, string content, string outputPath)
        {
            var encodedBitmap = EmbedTextInBitmap(content, SKBitmap.Decode(bitmapData));
            var image = SKImage.FromBitmap(encodedBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(outputPath);
            data.SaveTo(stream);
        }

        /// <summary>
        /// Extracts build data from an image file.
        /// </summary>
        /// <param name="imagePath">The path of the image file.</param>
        /// <returns>The serialized build config that was embedded into the image.</returns>
        public static string ExtractBuildData(string imagePath)
        {
            var bitmap = SKBitmap.Decode(imagePath);
            return ExtractTextFromBitmap(bitmap);
        }

        private static SKBitmap EmbedTextInBitmap(string text, SKBitmap bmp)
        {
            // initially, we'll be hiding characters in the image
            var state = State.EncodingText;

            // holds the index of the character that is being hidden
            int charIndex = 0;

            // holds the value of the character converted to integer
            int charValue = 0;

            // holds the index of the color element (R or G or B) that is currently being processed
            long pixelElementIndex = 0;

            // holds the number of trailing zeros that have been added when finishing the process
            int zeros = 0;

            for (var pixelRow = 0; pixelRow < bmp.Height; pixelRow++)
            {
                for (var pixelColumn = 0; pixelColumn < bmp.Width; pixelColumn++)
                {
                    // holds the pixel that is currently being processed
                    var pixel = bmp.GetPixel(pixelColumn, pixelRow);

                    // now, clear the least significant bit (LSB) from each pixel element
                    int red = pixel.Red - (pixel.Red % 2);
                    int green = pixel.Green - (pixel.Green % 2);
                    int blue = pixel.Blue - (pixel.Blue % 2);

                    // for each pixel, pass through its elements (RGB)
                    for (var n = 0; n < 3; n++)
                    {
                        // check if new 8 bits has been processed
                        if (pixelElementIndex % 8 == 0)
                        {
                            // check if the whole process has finished
                            // we can say that it's finished when 8 zeros are added
                            if (state == State.FinishingSequence && zeros == 8)
                            {
                                // apply the last pixel on the image
                                // even if only a part of its elements have been affected
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(pixelColumn, pixelRow, new((byte)red, (byte)green, (byte)blue));
                                }

                                // return the bitmap with the text hidden in
                                return bmp;
                            }

                            // check if all characters has been hidden
                            if (charIndex >= text.Length)
                            {
                                // start adding zeros to mark the end of the text
                                state = State.FinishingSequence;
                            }
                            else
                            {
                                // move to the next character and process again
                                charValue = text[charIndex++];
                            }
                        }

                        // check which pixel element has the turn to hide a bit in its LSB
                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                if (state == State.EncodingText)
                                {
                                    // the rightmost bit in the character will be (charValue % 2)
                                    // to put this value instead of the LSB of the pixel element
                                    // just add it to it
                                    // recall that the LSB of the pixel element had been cleared
                                    // before this operation
                                    red += charValue % 2;

                                    // removes the added rightmost bit of the character
                                    // such that next time we can reach the next one
                                    charValue /= 2;
                                }

                                break;
                            case 1:
                                if (state == State.EncodingText)
                                {
                                    green += charValue % 2;
                                    charValue /= 2;
                                }

                                break;
                            case 2:
                                if (state == State.EncodingText)
                                {
                                    blue += charValue % 2;
                                    charValue /= 2;
                                }

                                bmp.SetPixel(pixelColumn, pixelRow, new((byte)red, (byte)green, (byte)blue));
                                break;
                        }

                        pixelElementIndex++;

                        if (state == State.FinishingSequence)
                        {
                            // increment the value of zeros until it is 8
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        private static string ExtractTextFromBitmap(SKBitmap bmp)
        {
            var colorUnitIndex = 0;
            var charValue = 0;

            // holds the text that will be extracted from the image
            var extractedText = string.Empty;

            // pass through the rows
            for (var pixelRow = 0; pixelRow < bmp.Height; pixelRow++)
            {
                // pass through each row
                for (var pixelColumn = 0; pixelColumn < bmp.Width; pixelColumn++)
                {
                    var pixel = bmp.GetPixel(pixelColumn, pixelRow);

                    // for each pixel, pass through its elements (RGB)
                    for (var n = 0; n < 3; n++)
                    {
                        // get the LSB from the pixel element (will be pixel.R % 2)
                        // then add one bit to the right of the current character
                        // this can be done by (charValue = charValue * 2)
                        // replace the added bit (which value is by default 0) with
                        // the LSB of the pixel element, simply by addition
                        charValue = (colorUnitIndex % 3) switch
                        {
                            0 => (charValue * 2) + (pixel.Red % 2),
                            1 => (charValue * 2) + (pixel.Green % 2),
                            2 => (charValue * 2) + (pixel.Blue % 2),
                            _ => charValue, // Cannot happen but is required by compiler
                        };

                        colorUnitIndex++;

                        // if 8 bits has been added,
                        // then add the current character to the result text
                        if (colorUnitIndex % 8 == 0)
                        {
                            // reverse? of course, since each time the process occurs
                            // on the right (for simplicity)
                            charValue = ReverseBits(charValue);

                            // can only be 0 if it is the stop character (the 8 zeros)
                            if (charValue == 0)
                            {
                                return extractedText;
                            }

                            extractedText += (char)charValue;
                        }
                    }
                }
            }

            return extractedText;
        }

        private static int ReverseBits(int n)
        {
            var result = 0;
            for (var i = 0; i < 8; i++)
            {
                result = (result * 2) + (n % 2);
                n /= 2;
            }

            return result;
        }
    }
}
