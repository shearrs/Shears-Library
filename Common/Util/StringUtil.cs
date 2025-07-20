using System.Text;
using UnityEngine;

namespace Shears
{
    public static class StringUtil
    {
        // sourced from Binary Worrier @ https://stackoverflow.com/questions/272633/add-spaces-before-capital-letters
        public static string PascalSpace(string pascalText)
        {
            if (string.IsNullOrWhiteSpace(pascalText))
                return "";

            var builder = new StringBuilder(pascalText.Length * 2);
            builder.Append(pascalText[0]);

            for (int i = 1; i < pascalText.Length; i++)
            {
                if (char.IsUpper(pascalText[i]) && pascalText[i - 1] != ' ')
                    builder.Append(' ');

                builder.Append(pascalText[i]);
            }

            return builder.ToString();
        }
    }
}
