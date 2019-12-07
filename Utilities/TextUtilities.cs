using System;

namespace GroupHStegafy.Utilities
{
    public static class TextUtilities
    {
        public static string ExpandKey(int length, string key)
        {
            var expandedKey = "";

            for (int strPosition = 0, keyPosition = 0; strPosition < length; strPosition++, keyPosition++)
            {
                if (keyPosition == key.Length)
                {
                    keyPosition = 0;
                }

                expandedKey += key.ToUpper()[keyPosition];
            }

            return expandedKey;
        }

        public static string EncryptText(string text, string expandedKey)
        {
            if (expandedKey.Length != text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(expandedKey), "expanded key must be the same length as text to encrypt");
            }

            var encryptedText = "";

            for (var i = 0; i < text.Length; i++)
            {
                var isLowercase = char.IsLower(text[i]);
                var numericEquivalent = (text.ToUpper()[i] + expandedKey.ToUpper()[i]) % 26;
                var letterToAdd = (char) (numericEquivalent + 'A');
                if (isLowercase)
                {
                    encryptedText += char.ToLower(letterToAdd);
                }
                else
                {
                    encryptedText += letterToAdd;
                }
            }

            return encryptedText;
        }

        public static string DecryptText(string encryptedText, string expandedKey)
        {
            if (expandedKey.Length != encryptedText.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(expandedKey), "expanded key must be the same length as text to encrypt");
            }

            var decryptedText = "";

            for (var i = 0; i < encryptedText.Length; i++)
            {
                var isLowercase = char.IsLower(encryptedText[i]);
                var numericEquivalent = (encryptedText.ToUpper()[i] - expandedKey.ToUpper()[i] + 26) % 26;
                var letterToAdd = (char) (numericEquivalent + 'A');
                if (isLowercase)
                {
                    decryptedText += char.ToLower(letterToAdd);
                }
                else
                {
                    decryptedText += letterToAdd;
                }
            }

            return decryptedText;
        }
    }
}