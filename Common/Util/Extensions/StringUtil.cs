using System.Text;
using UnityEngine;

namespace Shears
{
    public static class StringUtil
    {
        private static readonly StringBuilder builder = new();

        // sourced from Binary Worrier @ https://stackoverflow.com/questions/272633/add-spaces-before-capital-letters
        public static string PascalSpace(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            builder.Clear();
            builder.Append(char.ToUpper(text[0]));

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    builder.Append(' ');

                builder.Append(text[i]);
            }

            return builder.ToString();
        }
    }
}
