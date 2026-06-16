using BeastTribeQuester.Model;
using System.Reflection;
using System.Text.Json;

namespace BeastTribeQuester;

/// <summary>
/// Loads all <see cref="BeastTribeDefinition"/> JSON files that are embedded
/// in the assembly under the <c>QuestPaths/</c> logical prefix.
/// </summary>
public sealed class QuestDataLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling         = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Returns all tribe definitions, ordered Dawntrail → ARR (newest first).
    /// </summary>
    public static List<BeastTribeDefinition> LoadAll()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var results  = new List<BeastTribeDefinition>();

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith("QuestPaths/", StringComparison.OrdinalIgnoreCase)
                || !resourceName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                continue;

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            var def = JsonSerializer.Deserialize<BeastTribeDefinition>(stream, JsonOptions);
            if (def != null)
                results.Add(def);
        }

        // Sort newest expansion first
        results.Sort((a, b) => b.Expansion.CompareTo(a.Expansion));
        return results;
    }
}
