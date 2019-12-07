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
            var encryptedText = "";

            for (int i = 0; i < text.Length; i++)
            {
                var isLowercase = char.IsLower(text[i]);
                var numericEquivalent = (text.ToUpper()[i] + expandedKey[i]) % 26;
                var letterToAdd = (char) (numericEquivalent += 'A');
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
    }
}