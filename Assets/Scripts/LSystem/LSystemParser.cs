using System.Collections.Generic;

/// <summary>
/// Parses an L-System string into a list of symbol tokens,
/// each optionally containing a parameter (e.g., F(2.5)).
/// </summary>
public class LSystemParser
{
    /// <summary>
    /// Represents a symbol in the L-System along with an optional parameter.
    /// </summary>
    public struct SymbolToken
    {
        public char symbol;
        public float? parameter;
    }

    /// <summary>
    /// Parses a raw L-System string into a list of symbol tokens.
    /// Supports optional float parameters in parentheses, e.g., F(2.0), +(45).
    /// </summary>
    public static List<SymbolToken> Parse(string input)
    {
        var tokens = new List<SymbolToken>();
        int i = 0;

        while (i < input.Length)
        {
            char c = input[i];

            // Skip whitespace
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            // Valid symbols include letters, digits, and common L-System operators
            if (char.IsLetterOrDigit(c) || "+-&^/\\[]!\"?_".Contains(c))
            {
                float? param = null;

                // Check for parameter in parentheses: e.g. F(2.5)
                if (i + 1 < input.Length && input[i + 1] == '(')
                {
                    int paramStart = i + 2;
                    int paramEnd = input.IndexOf(')', paramStart);

                    // Parse float value inside parentheses if valid
                    if (paramEnd > paramStart)
                    {
                        string paramStr = input.Substring(paramStart, paramEnd - paramStart);
                        if (float.TryParse(paramStr, out float parsedValue))
                            param = parsedValue;

                        i = paramEnd + 1; // Move past closing ')'
                    }
                    else
                    {
                        i++; // Malformed parameter, skip just the symbol
                    }
                }
                else
                {
                    i++; // No parameter, move to next character
                }

                tokens.Add(new SymbolToken { symbol = c, parameter = param });
            }
            else
            {
                i++; // Skip unrecognized characters
            }
        }

        return tokens;
    }
}
