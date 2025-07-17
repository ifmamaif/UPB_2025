using System.Collections.Generic;

public class LSystemParser
{
    public struct SymbolToken
    {
        public char symbol;
        public float? parameter;
    }

    public static List<SymbolToken> Parse(string input)
    {
        List<SymbolToken> tokens = new();
        int i = 0;

        while (i < input.Length)
        {
            char c = input[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsLetterOrDigit(c) || "+-&^/\\[]!\"?_".Contains(c))
            {
                float? param = null;

                if (i + 1 < input.Length && input[i + 1] == '(')
                {
                    int start = i + 2;
                    int end = input.IndexOf(')', start);
                    if (end > start)
                    {
                        string paramStr = input.Substring(start, end - start);
                        if (float.TryParse(paramStr, out float parsed))
                            param = parsed;
                        i = end + 1;
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    i++;
                }

                tokens.Add(new SymbolToken { symbol = c, parameter = param });
            }
            else
            {
                i++;
            }
        }

        return tokens;
    }
}
