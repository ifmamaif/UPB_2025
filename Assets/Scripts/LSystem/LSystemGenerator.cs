using System.Collections.Generic;
using System.Text;

/// <summary>
/// Responsible for generating a string based on L-System rules
/// through a specified number of iterations.
/// </summary>
public class LSystemGenerator
{
    // Internal dictionary of rules: maps each character to its replacement string
    private readonly Dictionary<char, string> rules;

    /// <summary>
    /// Constructs the generator from a list of Rule objects.
    /// Only the first rule per character is kept if duplicates exist.
    /// </summary>
    public LSystemGenerator(List<Rule> ruleList)
    {
        rules = new Dictionary<char, string>();

        foreach (var rule in ruleList)
        {
            // Avoid adding duplicate keys; first occurrence is kept
            if (!rules.ContainsKey(rule.key))
                rules.Add(rule.key, rule.value);
        }
    }

    /// <summary>
    /// Generates the final L-System string by applying rules to the axiom
    /// for a given number of iterations.
    /// </summary>
    /// <param name="axiom">The starting string of the system</param>
    /// <param name="iterations">How many times to apply the production rules</param>
    /// <returns>The final expanded string</returns>
    public string Generate(string axiom, int iterations)
    {
        string current = axiom;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < iterations; i++)
        {
            sb.Clear();

            foreach (char c in current)
            {
                // Replace character using rules, or keep it as-is if no rule exists
                sb.Append(rules.TryGetValue(c, out string replacement) ? replacement : c.ToString());
            }

            current = sb.ToString();
        }

        return current;
    }
}
