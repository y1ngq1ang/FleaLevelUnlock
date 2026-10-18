using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace FleaLevelUnlock;

/// <summary>
/// Removes every level requirement the flea market enforces, by patching the in-memory
/// database after it has been loaded. Nothing on disk is touched, so SPT's checks.dat
/// file validation keeps passing and the change survives database updates.
///
/// Three layers exist in SPT 5.0 and all three are cleared here:
///   1. RagFair.MinUserLevel               - overall flea unlock level (default 15).
///   2. RagFair.RagfairMinUserLevelByCategory - master switch for the per-category system
///      (RagfairLevelService.Enabled reads it directly, so false disables the whole service).
///   3. Per-item / per-handbook-category RagfairLevelToTrade values the client uses to lock
///      flea categories and the server mirrors via RagfairOfferHelper.LockWhenBelowLevel.
/// </summary>
[Injectable(InjectionType.Singleton, int.MaxValue, TypePriority = OnLoadOrder.PostLoad + 1)]
public class FleaLevelUnlockPatch(
    GlobalTable globalTable,
    TemplateTable templateTable,
    ISptLogger<FleaLevelUnlockPatch> logger) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var ragFair = globalTable.Configuration.RagFair;

        var previousMinUserLevel = ragFair.MinUserLevel;
        ragFair.MinUserLevel = 0;
        ragFair.RagfairMinUserLevelByCategory = false;

        var itemCount = ClearItemLevels();
        var categoryCount = ClearCategoryLevels();

        logger.Success(
            $"[FleaLevelUnlock] Flea market level requirements removed: minUserLevel {previousMinUserLevel} -> 0, "
                + $"per-category levels off, {itemCount} item(s) and {categoryCount} handbook category(ies) unlocked.");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clear the level requirement on every item template that still has one.
    /// </summary>
    private int ClearItemLevels()
    {
        var changed = 0;

        foreach (var item in templateTable.Items.Values)
        {
            if (item.Properties?.RagfairLevelToTrade is > 0)
            {
                item.Properties.RagfairLevelToTrade = 0;
                changed++;
            }
        }

        return changed;
    }

    /// <summary>
    /// Clear the level requirement on handbook categories. Negative values are left alone:
    /// in SPT's Locks() check a negative value means "locked at any level" rather than a level
    /// requirement, so it is a hard block and not a level restriction. (The only one shipped is
    /// the "Rounds" category.)
    /// </summary>
    private int ClearCategoryLevels()
    {
        var changed = 0;

        foreach (var category in templateTable.Handbook.Categories)
        {
            if (category.RagfairLevelToTrade is > 0)
            {
                category.RagfairLevelToTrade = 0;
                changed++;
            }
        }

        return changed;
    }
}
