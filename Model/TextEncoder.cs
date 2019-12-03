using System;

namespace GroupHStegafy.Model
{
    /// <summary>
    ///     Handles encoding and decoding of text into an image.
    /// </summary>
    public class TextEncoder
    {
        /// <summary>
        ///     Encodes the message.
        /// </summary>
        /// <param name="originalImageBytes">The original image bytes.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public byte[] EncodeMessage(byte[] originalImageBytes, string message)
        {
            throw new ArgumentException("Not Implemented");
        }

        /// <summary>
        ///     Decodes the message.
        /// </summary>
        /// <param name="modifiedImageBytes">The modified image bytes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Not Implemented</exception>
        public string DecodeMessage(byte[] modifiedImageBytes)
        {
            throw new ArgumentException("Not Implemented");
        }
    }
}
