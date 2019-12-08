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

        public static string EncryptText(string text, string key)
        {
            var expandedKey = ExpandKey(text.Length, key);

            var encryptedText = key + "#KEY#";

            for (var i = 0; i < text.Length; i++)
            {
                var isLowercase = char.IsLower(text[i]);
                var numericEquivalent = (text.ToUpper()[i] + expandedKey.ToUpper()[i]) % 26;
                var letterToAdd = (char)(numericEquivalent + 'A');
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

        public static string DecryptText(string encryptedTextWithKey)
        {
            var decryptedText = "";

            char[] separator = { '#', 'K', 'E', 'Y', '#' };
            var stringList = encryptedTextWithKey.Split(separator, 2, StringSplitOptions.None);
            var encryptedText = stringList[1];
            var expandedKey = ExpandKey(encryptedText.Length, stringList[0]);

            for (var i = 0; i < encryptedText.Length; i++)
            {
                var isLowercase = char.IsLower(encryptedText[i]);
                var numericEquivalent = (encryptedText.ToUpper()[i] - expandedKey.ToUpper()[i] + 26) % 26;
                var letterToAdd = (char)(numericEquivalent + 'A');
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

        public static string GetKey(string encryptedTextWithKey)
        {
            string[] separator = { "#", "KEY", "#" };
            var stringList = encryptedTextWithKey.Split(separator, 2, StringSplitOptions.None);
            var encryptedText = stringList[1];
            return stringList[0];
        }
    }
}