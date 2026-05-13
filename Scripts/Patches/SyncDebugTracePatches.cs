using System.Collections;
using System.Reflection;
using FBE.Scripts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;
using MegaCrit.Sts2.Core.Rewards;

// ReSharper disable InconsistentNaming

namespace FBE.Scripts.Patches;

internal static class SyncDebugTrace
{
    private static readonly Logger Logger = new("FBE_SYNC_TRACE", LogType.GameSync);

    internal static bool Enabled => Entry.EnableSyncDebugTracePatches;

    internal static bool PreparePatch() => Enabled;

    internal static void SafeLog(Func<string> messageFactory)
    {
        if (!Enabled)
            return;

        try
        {
            Logger.Info(messageFactory());
        }
        catch (Exception e)
        {
            Logger.Error($"[FBE_SYNC_TRACE] logging failed: {e.GetType().Name}: {e.Message}");
        }
    }

    internal static string DescribeRewardsSet(RewardsSet? set)
    {
        if (set == null)
            return "<null rewards set>";

        return $"setId={set.Id} owner={set.Player.NetId} rewards=[{string.Join(",", set.Rewards.Select(DescribeReward))}]";
    }

    internal static string DescribeReward(Reward? reward)
    {
        if (reward == null)
            return "<null reward>";

        return $"{reward.GetType().Name}(player={reward.Player.NetId}, selected={reward.SuccessfullySelected})";
    }

    internal static string DescribeRewardSyncState(RewardsSetSynchronizer sync)
    {
        object? messageBuffer = Field(sync, "_messageBuffer");
        string location = Prop(messageBuffer, "CurrentLocation")?.ToString() ?? "<unknown location>";
        object? localPlayerId = Field(sync, "_localPlayerId");
        var states = Field(sync, "_rewardStates") as IEnumerable;

        List<string> playerStates = new();
        if (states != null)
        {
            int slot = 0;
            foreach (object? state in states)
            {
                playerStates.Add($"slot{slot}:{DescribePlayerRewardState(state)}");
                slot++;
            }
        }

        return $"location={location} local={localPlayerId} states=[{string.Join(" | ", playerStates)}]";
    }

    internal static string DescribePlayerChoiceState(PlayerChoiceSynchronizer sync)
    {
        string ids = DescribeEnumerable(Field(sync, "_choiceIds"));
        string received = DescribeReceivedChoices(Field(sync, "_receivedChoices") as IEnumerable);
        return $"choiceIds=[{ids}] received=[{received}]";
    }

    private static string DescribePlayerRewardState(object? state)
    {
        if (state == null)
            return "<null state>";

        object? nextId = Field(state, "nextId");
        string stack = DescribeRewardStack(Field(state, "rewardsStack") as IEnumerable);
        string buffered = DescribeBufferedMessages(Field(state, "bufferedMessages") as IEnumerable);
        string completed = DescribeDictionaryKeys(Field(state, "completedRewards") as IDictionary);

        return $"nextId={nextId} stack=[{stack}] buffered=[{buffered}] completed=[{completed}]";
    }

    private static string DescribeRewardStack(IEnumerable? stack)
    {
        if (stack == null)
            return "";

        List<string> items = new();
        foreach (object? setState in stack)
        {
            RewardsSet? set = Field(setState, "set") as RewardsSet;
            items.Add(set == null ? "<null set>" : $"{set.Id}:{set.Player.NetId}:{set.Rewards.Count}");
        }
        return string.Join(",", items);
    }

    private static string DescribeBufferedMessages(IEnumerable? messages)
    {
        if (messages == null)
            return "";

        List<string> items = new();
        foreach (object? message in messages)
        {
            object? selected = Field(message, "selectedMessage");
            object? skipped = Field(message, "skippedMessage");
            object? sender = Field(message, "senderId");
            object? payload = selected ?? skipped;
            object? setId = Field(payload, "setId") ?? Prop(payload, "setId");
            object? rewardIndex = Field(payload, "rewardIndex") ?? Prop(payload, "rewardIndex");
            string kind = selected != null ? "select" : skipped != null ? "skip" : "unknown";
            items.Add($"{kind}:sender={sender}:set={setId}:reward={rewardIndex}");
        }
        return string.Join(",", items);
    }

    private static string DescribeReceivedChoices(IEnumerable? choices)
    {
        if (choices == null)
            return "";

        List<string> items = new();
        foreach (object? choice in choices)
        {
            object? sender = Field(choice, "senderId");
            object? choiceId = Field(choice, "choiceId");
            object? task = Field(choice, "completionSource");
            object? taskObj = Prop(task, "Task");
            object? isCompleted = Prop(taskObj, "IsCompleted");
            items.Add($"sender={sender}:choice={choiceId}:completed={isCompleted}");
        }
        return string.Join(",", items);
    }

    private static string DescribeEnumerable(object? value)
    {
        if (value is not IEnumerable enumerable)
            return "";

        List<string> items = new();
        foreach (object? item in enumerable)
        {
            items.Add(item?.ToString() ?? "<null>");
        }
        return string.Join(",", items);
    }

    private static string DescribeDictionaryKeys(IDictionary? dictionary)
    {
        if (dictionary == null)
            return "";

        List<string> items = new();
        foreach (object? key in dictionary.Keys)
        {
            object? value = key == null ? null : dictionary[key];
            items.Add($"{key}:{value}");
        }
        return string.Join(",", items);
    }

    internal static object? Field(object? instance, string name)
    {
        if (instance == null)
            return null;

        FieldInfo? field = AccessTools.Field(instance.GetType(), name);
        return field?.GetValue(instance);
    }

    private static object? Prop(object? instance, string name)
    {
        if (instance == null)
            return null;

        PropertyInfo? prop = AccessTools.Property(instance.GetType(), name);
        return prop?.GetValue(instance);
    }
}

[HarmonyPatch(typeof(RewardsSetSynchronizer), nameof(RewardsSetSynchronizer.BeginRewardsSet))]
internal static class Patch_RewardsSetSynchronizer_BeginRewardsSet_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(RewardsSetSynchronizer __instance, RewardsSet set)
    {
        SyncDebugTrace.SafeLog(() =>
            $"BeginRewardsSet BEFORE {SyncDebugTrace.DescribeRewardsSet(set)} {SyncDebugTrace.DescribeRewardSyncState(__instance)}");
    }

    private static void Postfix(RewardsSetSynchronizer __instance, RewardsSet set)
    {
        SyncDebugTrace.SafeLog(() =>
            $"BeginRewardsSet AFTER {SyncDebugTrace.DescribeRewardsSet(set)} {SyncDebugTrace.DescribeRewardSyncState(__instance)}");
    }
}

[HarmonyPatch(typeof(RewardsSetSynchronizer), nameof(RewardsSetSynchronizer.HandleRewardSelectedMessage))]
internal static class Patch_RewardsSetSynchronizer_HandleRewardSelectedMessage_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(RewardsSetSynchronizer __instance, RewardSelectedMessage message, ulong senderId)
    {
        SyncDebugTrace.SafeLog(() =>
            $"HandleRewardSelectedMessage BEFORE sender={senderId} setId={message.setId} rewardIndex={message.rewardIndex} {SyncDebugTrace.DescribeRewardSyncState(__instance)}");
    }

    private static void Postfix(RewardsSetSynchronizer __instance, RewardSelectedMessage message, ulong senderId)
    {
        SyncDebugTrace.SafeLog(() =>
            $"HandleRewardSelectedMessage AFTER sender={senderId} setId={message.setId} rewardIndex={message.rewardIndex} {SyncDebugTrace.DescribeRewardSyncState(__instance)}");
    }
}

[HarmonyPatch(typeof(RewardsSetSynchronizer), "SelectRewardForPlayer")]
[HarmonyPatch([typeof(Player), typeof(int)])]
internal static class Patch_RewardsSetSynchronizer_SelectRewardForPlayer_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(RewardsSetSynchronizer __instance, Player player, int rewardIndex)
    {
        SyncDebugTrace.SafeLog(() =>
            $"SelectRewardForPlayer BEFORE player={player.NetId} rewardIndex={rewardIndex} {SyncDebugTrace.DescribeRewardSyncState(__instance)}");
    }
}

[HarmonyPatch(typeof(RewardsSetSynchronizer), "CompleteRewardsSet")]
internal static class Patch_RewardsSetSynchronizer_CompleteRewardsSet_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(RewardsSetSynchronizer __instance, object __0, object __1)
    {
        SyncDebugTrace.SafeLog(() =>
        {
            RewardsSet? set = SyncDebugTrace.Field(__0, "set") as RewardsSet;
            return $"CompleteRewardsSet BEFORE state={__1} {SyncDebugTrace.DescribeRewardsSet(set)} {SyncDebugTrace.DescribeRewardSyncState(__instance)}";
        });
    }

    private static void Postfix(RewardsSetSynchronizer __instance, object __0, object __1)
    {
        SyncDebugTrace.SafeLog(() =>
        {
            RewardsSet? set = SyncDebugTrace.Field(__0, "set") as RewardsSet;
            return $"CompleteRewardsSet AFTER state={__1} {SyncDebugTrace.DescribeRewardsSet(set)} {SyncDebugTrace.DescribeRewardSyncState(__instance)}";
        });
    }
}

[HarmonyPatch(typeof(RewardsSetSynchronizer), nameof(RewardsSetSynchronizer.BeforeLeavingRoom))]
internal static class Patch_RewardsSetSynchronizer_BeforeLeavingRoom_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(RewardsSetSynchronizer __instance)
    {
        SyncDebugTrace.SafeLog(() =>
            $"BeforeLeavingRoom BEFORE {SyncDebugTrace.DescribeRewardSyncState(__instance)}");
    }

    private static void Postfix(RewardsSetSynchronizer __instance)
    {
        SyncDebugTrace.SafeLog(() =>
            $"BeforeLeavingRoom AFTER {SyncDebugTrace.DescribeRewardSyncState(__instance)}");
    }
}

[HarmonyPatch(typeof(PlayerChoiceSynchronizer), nameof(PlayerChoiceSynchronizer.ReserveChoiceId))]
internal static class Patch_PlayerChoiceSynchronizer_ReserveChoiceId_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(PlayerChoiceSynchronizer __instance, Player player)
    {
        SyncDebugTrace.SafeLog(() =>
            $"ReserveChoiceId BEFORE player={player.NetId} {SyncDebugTrace.DescribePlayerChoiceState(__instance)}");
    }

    private static void Postfix(PlayerChoiceSynchronizer __instance, Player player, uint __result)
    {
        SyncDebugTrace.SafeLog(() =>
            $"ReserveChoiceId AFTER player={player.NetId} reserved={__result} {SyncDebugTrace.DescribePlayerChoiceState(__instance)}");
    }
}

[HarmonyPatch(typeof(PlayerChoiceSynchronizer), nameof(PlayerChoiceSynchronizer.SyncLocalChoice))]
internal static class Patch_PlayerChoiceSynchronizer_SyncLocalChoice_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(PlayerChoiceSynchronizer __instance, Player player, uint choiceId, PlayerChoiceResult result)
    {
        SyncDebugTrace.SafeLog(() =>
            $"SyncLocalChoice BEFORE player={player.NetId} choiceId={choiceId} result={result} {SyncDebugTrace.DescribePlayerChoiceState(__instance)}");
    }
}

[HarmonyPatch(typeof(PlayerChoiceSynchronizer), nameof(PlayerChoiceSynchronizer.WaitForRemoteChoice))]
internal static class Patch_PlayerChoiceSynchronizer_WaitForRemoteChoice_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(PlayerChoiceSynchronizer __instance, Player player, uint choiceId)
    {
        SyncDebugTrace.SafeLog(() =>
            $"WaitForRemoteChoice BEFORE player={player.NetId} choiceId={choiceId} {SyncDebugTrace.DescribePlayerChoiceState(__instance)}");
    }
}

[HarmonyPatch(typeof(PlayerChoiceSynchronizer), "OnPlayerChoiceMessageReceived")]
internal static class Patch_PlayerChoiceSynchronizer_OnPlayerChoiceMessageReceived_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(PlayerChoiceSynchronizer __instance, PlayerChoiceMessage message, ulong senderId)
    {
        SyncDebugTrace.SafeLog(() =>
            $"OnPlayerChoiceMessageReceived BEFORE sender={senderId} choiceId={message.choiceId} result={message.result} {SyncDebugTrace.DescribePlayerChoiceState(__instance)}");
    }

    private static void Postfix(PlayerChoiceSynchronizer __instance, PlayerChoiceMessage message, ulong senderId)
    {
        SyncDebugTrace.SafeLog(() =>
            $"OnPlayerChoiceMessageReceived AFTER sender={senderId} choiceId={message.choiceId} result={message.result} {SyncDebugTrace.DescribePlayerChoiceState(__instance)}");
    }
}

[HarmonyPatch(typeof(PlayerChoiceSynchronizer), "OnReceivePlayerChoice")]
internal static class Patch_PlayerChoiceSynchronizer_OnReceivePlayerChoice_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(PlayerChoiceSynchronizer __instance, Player player, uint choiceId)
    {
        SyncDebugTrace.SafeLog(() =>
            $"OnReceivePlayerChoice BEFORE player={player.NetId} choiceId={choiceId} {SyncDebugTrace.DescribePlayerChoiceState(__instance)}");
    }

    private static void Postfix(PlayerChoiceSynchronizer __instance, Player player, uint choiceId)
    {
        SyncDebugTrace.SafeLog(() =>
            $"OnReceivePlayerChoice AFTER player={player.NetId} choiceId={choiceId} {SyncDebugTrace.DescribePlayerChoiceState(__instance)}");
    }
}

[HarmonyPatch(typeof(PlayerChoiceSynchronizer), nameof(PlayerChoiceSynchronizer.FastForwardChoiceIds))]
internal static class Patch_PlayerChoiceSynchronizer_FastForwardChoiceIds_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(PlayerChoiceSynchronizer __instance, List<uint> choiceIds)
    {
        SyncDebugTrace.SafeLog(() =>
            $"FastForwardChoiceIds BEFORE incoming=[{string.Join(",", choiceIds)}] {SyncDebugTrace.DescribePlayerChoiceState(__instance)}");
    }

    private static void Postfix(PlayerChoiceSynchronizer __instance, List<uint> choiceIds)
    {
        SyncDebugTrace.SafeLog(() =>
            $"FastForwardChoiceIds AFTER incoming=[{string.Join(",", choiceIds)}] {SyncDebugTrace.DescribePlayerChoiceState(__instance)}");
    }
}

[HarmonyPatch(typeof(CardReward), "OnSelect")]
internal static class Patch_CardReward_OnSelect_Trace
{
    private static bool Prepare() => SyncDebugTrace.PreparePatch();

    private static void Prefix(CardReward __instance)
    {
        SyncDebugTrace.SafeLog(() =>
        {
            object? cards = SyncDebugTrace.Field(__instance, "_cards");
            return $"CardReward.OnSelect BEFORE {SyncDebugTrace.DescribeReward(__instance)} cards=[{DescribeCards(cards as IEnumerable)}]";
        });
    }

    private static string DescribeCards(IEnumerable? cards)
    {
        if (cards == null)
            return "";

        List<string> items = new();
        foreach (object? cardCreationResult in cards)
        {
            object? card = SyncDebugTrace.Field(cardCreationResult, "Card") ?? AccessTools.Property(cardCreationResult?.GetType(), "Card")?.GetValue(cardCreationResult);
            object? id = AccessTools.Property(card?.GetType(), "Id")?.GetValue(card);
            items.Add(id?.ToString() ?? card?.ToString() ?? "<null card>");
        }
        return string.Join(",", items);
    }
}
