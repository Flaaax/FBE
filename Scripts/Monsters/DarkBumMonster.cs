using FBE.Scripts.Visuals;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine.Backends;

namespace FBE.Scripts.Monsters;

[RegisterMonster]
public class DarkBumMonster : ModMonsterTemplate
{
	public override int MinInitialHp => 9999;

	public override int MaxInitialHp => 9999;

	public override bool IsHealthBarVisible => false;
	
	protected override NCreatureVisuals TryCreateCreatureVisuals()
	{
		return new DarkBumVisuals();
	}

	protected override ModAnimStateMachine? SetupCustomCombatAnimationStateMachine(Node visualsRoot, MonsterModel monster)
	{
		if (visualsRoot is not DarkBumVisuals visuals)
			return null;

		var backend = new AnimatedSprite2DBackend(visuals.Sprite);

		return ModAnimStateMachineBuilder.Create()
			.AddState("idle", loop: true).AsInitial().Done()
			.AddAnyState("Idle", "idle")
			.AddAnyState("Hit", "idle")
			.AddAnyState("Dead", "idle")
			.AddAnyState("Attack", "idle")
			.AddAnyState("Cast", "idle")
			.Build(backend);
	}

	// public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
	// {
	// 	var skinName = ((!IsMutable)
	// 		? MegaCrit.Sts2.Core.Models.Relics.Byrdpip.SkinOptions[0]
	// 		: Creature.PetOwner!.GetRelic<MegaCrit.Sts2.Core.Models.Relics.Byrdpip>()!.Skin);
	// 	MegaSkeletonDataResource data = skeleton.GetData();
	// 	skeleton.SetSkin(data.FindSkin(skinName));
	// 	skeleton.SetSlotsToSetupPose();
	// }

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		var moveState = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
		moveState.FollowUpState = moveState;
		return new MonsterMoveStateMachine([moveState], moveState);
	}
}