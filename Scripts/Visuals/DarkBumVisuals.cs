using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace FBE.Scripts.Visuals;

public partial class DarkBumVisuals : NCreatureVisuals
{
	public AnimatedSprite2D Sprite { get; } = new();

	private const int Frames = 16;

	public override void _Ready()
	{
		base._Ready();

		AddChild(Sprite);
		Sprite.Centered = true;
		Sprite.SpriteFrames = BuildSpriteFrames();
		Sprite.Animation = "idle";
		Sprite.Play("idle");
	}

	private static SpriteFrames BuildSpriteFrames()
	{
		var frames = new SpriteFrames();

		frames.ClearAll();
		frames.AddAnimation("idle");
		frames.SetAnimationLoop("idle", true);
		frames.SetAnimationSpeed("idle", 12.0);

		for (var i = 0; i < Frames; i++)
		{
			var path = $"res://FBE/animations/DarkBum/FloatDown/FloatDown_{i:000}.png";
			var texture = GD.Load<Texture2D>(path);
			frames.AddFrame("idle", texture);
		}

		return frames;
	}
}