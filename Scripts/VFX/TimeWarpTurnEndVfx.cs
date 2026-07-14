using Godot;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace FBE.Scripts.VFX;

public partial class TimeWarpTurnEndVfx : Control
{
	private const string IconPath = "res://FBE/images/vfx/TimeWarp/TimeWarpIcon_86x87.png";
	private const string BorderPath = "res://FBE/images/vfx/TimeWarp/BorderGlow2_1280x720.png";

	private const float Duration = 2f;
	private const float MoveDuration = 1f;
	private const float BorderDuration = 1f;
	private const float BorderFadeInDuration = 0.1f;
	private const float IconScale = 3f;
	private const float TargetHeightFromTop = 0.3f;

	private static readonly Color Gold = new(1f, 0.843f, 0f, 1f);

	private TextureRect _border = null!;
	private TextureRect _icon = null!;
	private Tween? _borderTween;
	private Tween? _iconTween;
	private bool _started;

	public static void Play()
	{
		if (NonInteractiveMode.IsActive)
			return;

		NGlobalUi? globalUi = NRun.Instance?.GlobalUi;
		Control? container = globalUi?.AboveTopBarVfxContainer;
		if (container == null)
			return;

		Texture2D? iconTexture = ResourceLoader.Load<Texture2D>(IconPath);
		Texture2D? borderTexture = ResourceLoader.Load<Texture2D>(BorderPath);
		if (iconTexture == null || borderTexture == null)
		{
			GD.PushWarning($"[TimeWarpTurnEndVfx] Missing texture. Icon: {iconTexture != null}, Border: {borderTexture != null}");
			return;
		}

		TimeWarpTurnEndVfx vfx = new()
		{
			Name = nameof(TimeWarpTurnEndVfx),
			MouseFilter = MouseFilterEnum.Ignore
		};
		vfx.BuildVisuals(iconTexture, borderTexture);
		container.AddChild(vfx);
		vfx.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
	}

	private void BuildVisuals(Texture2D iconTexture, Texture2D borderTexture)
	{
		Vector2 iconSize = iconTexture.GetSize();

		_border = new TextureRect
		{
			Name = "BorderGlow",
			Texture = borderTexture,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.Scale,
			MouseFilter = MouseFilterEnum.Ignore,
			Modulate = new Color(Gold.R, Gold.G, Gold.B, 0f),
			Material = new CanvasItemMaterial
			{
				BlendMode = CanvasItemMaterial.BlendModeEnum.Add
			}
		};
		AddChild(_border);
		_border.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

		_icon = new TextureRect
		{
			Name = "TimeWarpIcon",
			Texture = iconTexture,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			MouseFilter = MouseFilterEnum.Ignore,
			Size = iconSize,
			PivotOffset = iconSize * 0.5f,
			Scale = Vector2.One * IconScale,
			RotationDegrees = 720f
		};
		AddChild(_icon);
	}

	public override void _Process(double delta)
	{
		if (_started)
			return;

		_started = true;
		SetProcess(false);
		StartAnimation();
	}

	private void StartAnimation()
	{
		Vector2 viewportSize = Size;
		if (viewportSize.X <= 0f || viewportSize.Y <= 0f)
			viewportSize = GetViewportRect().Size;

		Vector2 halfIconSize = _icon.Size * 0.5f;
		_icon.Position = new Vector2(
			viewportSize.X * 0.5f - halfIconSize.X,
			viewportSize.Y - halfIconSize.Y);

		Vector2 targetPosition = new(
			viewportSize.X * 0.5f - halfIconSize.X,
			viewportSize.Y * TargetHeightFromTop - halfIconSize.Y);

		_borderTween = CreateTween();
		_borderTween.TweenProperty(_border, "modulate:a", Gold.A, BorderFadeInDuration)
			.SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Sine);
		_borderTween.TweenProperty(_border, "modulate:a", 0f, BorderDuration - BorderFadeInDuration)
			.SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Quad);

		_iconTween = CreateTween().SetParallel();
		_iconTween.TweenProperty(_icon, "position", targetPosition, MoveDuration)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Back);
		_iconTween.TweenProperty(_icon, "rotation_degrees", 0f, Duration)
			.SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Linear);
		_iconTween.TweenProperty(_icon, "modulate:a", 0f, Duration - MoveDuration)
			.SetDelay(MoveDuration)
			.SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Cubic);
		_iconTween.TweenCallback(Callable.From(QueueFree)).SetDelay(Duration);
	}

	public override void _ExitTree()
	{
		_borderTween?.Kill();
		_iconTween?.Kill();
	}
}
