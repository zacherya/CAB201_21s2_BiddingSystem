using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EBuy.Helpers
{
    public static class SecurityHelper
    {
        /// <summary>
        /// Hash a given string to pretect it's contents (Scramble text be unrecognisable to the human eye)
        /// </summary>
        /// <param name="plainText">The plain text string to hash</param>
        /// <returns>The plain text string hashed</returns>
        public static string HashString(string plainText)
        {
            try
            {
                byte[] encData_byte = new byte[plainText.Length]; // new byte array with length of text
                encData_byte = System.Text.Encoding.UTF8.GetBytes(plainText); // get bytes of text and store in bytes array
                string encodedData = Convert.ToBase64String(encData_byte); // Convert bytes to base64 string hash
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }

        /// <summary>
        /// UnHash a given hashed string to decode it's content (Unscramble text to be recognised by humans or input)
        /// </summary>
        /// <param name="encodedText">The hashed string to unhash</param>
        /// <returns>The encoded hash in plain text</returns>
        public static string UnHashString(string encodedText)
        {
            try
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding(); // Create a new instance of the UTF8 encoder
                System.Text.Decoder utf8Decode = encoder.GetDecoder(); // Get the decoder object from the encoder
                byte[] todecode_byte = Convert.FromBase64String(encodedText); // Convert the hashed text to array of bytes
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length); // Get the char count for precise decoding
                char[] decoded_char = new char[charCount]; // New char array with length of decoded text
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0); // Get the chars from the decoded chars
                string result = new String(decoded_char); // Turn into string
                return result;
            } catch
            {
                throw new Exception("Failed to unhash");
            }
        }

    }
}
