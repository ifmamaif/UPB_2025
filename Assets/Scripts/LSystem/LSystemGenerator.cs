using System.Collections.Generic;
using System.Text;

public class LSystemGenerator
{
    private readonly Dictionary<char, string> rules;

    public LSystemGenerator(List<Rule> ruleList)
    {
        rules = new Dictionary<char, string>();
        foreach (var rule in ruleList)
        {
            if (!rules.ContainsKey(rule.key))
                rules.Add(rule.key, rule.value);
        }
    }

    public string Generate(string axiom, int iterations)
    {
        var current = axiom;
        var sb = new StringBuilder();

        for (int i = 0; i < iterations; i++)
        {
            sb.Clear();
            foreach (var c in current)
                sb.Append(rules.TryGetValue(c, out var v) ? v : c.ToString());

            current = sb.ToString();
        }

        return current;
    }
}
