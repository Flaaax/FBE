using System.Diagnostics;
using FBE.Scripts.Cards;
using FBE.Scripts.Relics;
using FBE.Scripts.Utils;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.ValueProps;

namespace FBE.Scripts.Events;

public sealed class QuantumTower : FBEEventModel
{
    private bool _gameBgmSuppressed;

    // 背景图位置
    public override string CustomInitialPortraitPath => NoTowerPortrait;
    private const string TowerPortrait = "res://FBE/images/events/QuantumTower1.png";
    private const string NoTowerPortrait = "res://FBE/images/events/QuantumTower2.png";
    private const string MusicPath = "res://FBE/audio/music/The Nomai.mp3";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Count", 6),
        new StringVar("Enchantment", "占位符")
    ];

    public override void OnRoomEnter()
    {
        StartMusic();
    }

    protected override void OnEventFinished()
    {
        StopMusic();
    }

    // public override void CalculateVars()
    // {
    //     DynamicVars.Gold.BaseValue = Rng.NextInt(235, 265);
    // }

    public override bool IsAllowed(IRunState runState) => runState.CurrentActIndex <= 1;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Blink)
    ];

    private Task Blink()
    {
        SetLocalPortrait(TowerPortrait);
        SetEventState(PageDescription("BLINK"), [
            //todo Add hover tip
            Option(EnterTower, [], "BLINK"),
            Option(BlinkAgain, [], "BLINK")
        ]);
        return Task.CompletedTask;
    }

    private async Task EnterTower()
    {
        //todo do some logic...

        SetLocalPortrait(NoTowerPortrait);
        SetEventFinished(PageDescription("ENTER_TOWER_CHOSEN"));
    }

    private Task BlinkAgain()
    {
        SetLocalPortrait(NoTowerPortrait);
        SetEventFinished(PageDescription("BLINK_AGAIN_CHOSEN"));
        return Task.CompletedTask;
    }

    private void StartMusic()
    {
        if (!IsLocalOwner())
        {
            return;
        }

        var audioManager = NAudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.SetBgmVol(0f);
            _gameBgmSuppressed = true;
        }

        AudioHelper.PlayLoop(MusicPath);
    }

    private void StopMusic()
    {
        if (!IsLocalOwner())
        {
            return;
        }

        AudioHelper.StopLoop(MusicPath);

        if (!_gameBgmSuppressed)
        {
            return;
        }

        NAudioManager.Instance?.SetBgmVol(SaveManager.Instance.SettingsSave.VolumeBgm);
        _gameBgmSuppressed = false;
    }
}