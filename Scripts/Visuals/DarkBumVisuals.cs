using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace FBE.Scripts.Visuals;

public partial class DarkBumVisuals : NCreatureVisuals
{
	public AnimatedSprite2D Sprite { get; } = new();

	private const string IdleAnimationName = "idle";

	// 静态贴图路径
	private const string IdleTexturePath = "res://FBE/animations/DarkBum/Idle.png";

	// 贴图缩放。像素风建议用整数：1、2、3...
	private const float SpriteScale = 4.0f;

	// 贴图基础偏移。x 正数向右，y 正数向下。
	private static readonly Vector2 BaseSpritePosition = new(50.0f, -50.0f);

	// idle 浮动幅度，单位：像素
	private const float BobAmplitude = 5.0f;

	// idle 浮动速度
	private const float BobSpeed = 3.0f;

	private double _time;

	public override void _Ready()
	{
		EnsureCreatureVisualNodes();

		// 必须在节点结构创建之后再调用 base._Ready()
		base._Ready();

		Sprite.Play(IdleAnimationName);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		_time += delta;

		if (_time >= 2 * double.Pi)
		{
			_time -= 2 * double.Pi;
		}

		// 只在 idle 状态上下浮动。
		// 后面你加 attack / hit / die 等动画时，它们不会被这个浮动影响。
		if (Sprite.Animation == IdleAnimationName)
		{
			var bobOffset = new Vector2(
				0f,
				(float)Math.Sin(_time * BobSpeed) * BobAmplitude
			);

			Sprite.Position = BaseSpritePosition + bobOffset;
		}
		else
		{
			Sprite.Position = BaseSpritePosition;
		}
	}

	private void EnsureCreatureVisualNodes()
	{
		var visuals = new Node2D
		{
			Name = "Visuals",
			UniqueNameInOwner = true,
			TextureFilter = TextureFilterEnum.Nearest
		};
		AddOwnedChild(visuals);

		Sprite.Name = "AnimatedSprite2D";
		Sprite.Centered = true;
		Sprite.Position = BaseSpritePosition;
		Sprite.Scale = new Vector2(SpriteScale, SpriteScale);
		Sprite.TextureFilter = TextureFilterEnum.Nearest;
		Sprite.SpriteFrames = BuildSpriteFrames();
		Sprite.Animation = IdleAnimationName;

		visuals.AddChild(Sprite);
		Sprite.Owner = this;

		AddOwnedChild(new Control
		{
			Name = "Bounds",
			UniqueNameInOwner = true,
			Position = Sprite.Position,
			Size = new Vector2(128f, 128f)
		});

		AddOwnedChild(new Marker2D
		{
			Name = "IntentPos",
			UniqueNameInOwner = true,
			Position = new Vector2(0f, -88f)
		});
	}

	private void AddOwnedChild(Node node)
	{
		AddChild(node);
		node.Owner = this;
	}

	private static SpriteFrames BuildSpriteFrames()
	{
		var frames = new SpriteFrames();

		frames.ClearAll();

		// idle 是单帧动画：静态图 + 代码控制上下浮动
		frames.AddAnimation(IdleAnimationName);
		frames.SetAnimationLoop(IdleAnimationName, true);
		frames.SetAnimationSpeed(IdleAnimationName, 1.0);

		var idleTexture = GD.Load<Texture2D>(IdleTexturePath);
		if (idleTexture == null)
		{
			GD.PushError($"DarkBum idle texture not found: {IdleTexturePath}");
		}
		else
		{
			frames.AddFrame(IdleAnimationName, idleTexture);
		}

		return frames;
	}
}