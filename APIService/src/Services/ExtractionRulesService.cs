using EmailExtractor.Configuration;

namespace EmailExtractor.Services;

/// <summary>
/// Loads the extraction rules from the configuration, e.g. tax rate and default values.
/// </summary>
/// <param name="configuration"></param>
public class ExtractionRulesService(IConfiguration configuration) : IExtractionRulesService
{
    public ExtractionRules Rules { get; } = Load(configuration);

    private static ExtractionRules Load(IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection(nameof(ExtractionRules));
        var taxRate = section.GetValue<decimal?>(nameof(ExtractionRules.TaxRate));
        if (taxRate is null or < 0)
        {
            throw new InvalidOperationException("ExtractionRules:TaxRate must be a nonnegative decimal fraction.");
        }

        var rules = new ExtractionRules { TaxRate = taxRate.Value };
        foreach (var entry in section.GetSection("Defaults").GetChildren())
        {
            var parent = entry["Parent"] ?? "";
            var field = entry["Field"];
            var value = entry["Value"];
            if (string.IsNullOrWhiteSpace(field) || value is null)
            {
                throw new InvalidOperationException("Each extraction default requires a Field and a Value.");
            }

            if (!rules.DefaultsByParent.ContainsKey(parent))
            {
                rules.DefaultsByParent.Add(parent, new Dictionary<string, string>(StringComparer.Ordinal));
            }

            if (!rules.DefaultsByParent[parent].TryAdd(field, value.Trim()))
            {
                throw new InvalidOperationException($"Multiple defaults map to field '{field}' under parent '{parent}'.");
            }
        }

        return rules;
    }
}
