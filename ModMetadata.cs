using SemanticVersioning;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SemVer = SemanticVersioning.Version;
using SemVerRange = SemanticVersioning.Range;

namespace FleaLevelUnlock;

/// <summary>
/// Metadata the SPT mod loader reads to identify and validate this mod.
/// Every mod assembly needs exactly one implementation of <see cref="IModMetadata" />.
/// </summary>
public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.spt.flealevelunlock";

    public string Name { get; init; } = "FleaLevelUnlock";

    public string Author { get; init; } = "y1ngq1ang";

    public List<string>? Contributors { get; init; }

    public SemVer Version { get; init; } = new("1.0.0", false);

    public SemVerRange SptVersion { get; init; } = new("~5.0.0", false);

    public bool HasPrepatcher { get; init; }

    public List<string>? Incompatibilities { get; init; }

    public Dictionary<string, SemVerRange>? ModDependencies { get; init; }

    public string? Url { get; init; }

    public string License { get; init; } = "MIT";
}
