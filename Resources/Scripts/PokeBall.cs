using Godot;

namespace PokemonTD;

public partial class PokeBall : TextureRect
{
	private bool _isDragging = false;

    public override void _ExitTree()
    {
        PokemonTD.Keybinds.PokeBall -= ShowPokeBall;
    }

    public override void _Ready()
	{
		PokemonTD.Keybinds.PokeBall += ShowPokeBall;
	}

	private void ShowPokeBall(bool isPressed)
	{
		_isDragging = isPressed;
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.Dragging, _isDragging);
		
		StringName gameStateSignalName = isPressed ? PokemonSignals.SignalName.PressedPause : PokemonSignals.SignalName.PressedPlay;
		PokemonTD.Signals.EmitSignal(gameStateSignalName);

		if (isPressed)
		{
			Control dragPreview = GetDragPreview();
			ForceDrag(this, dragPreview);
		}
		else
		{
			GetViewport().GuiCancelDrag();
		}
	}

    public override Variant _GetDragData(Vector2 atPosition)
	{
		_isDragging = true;

		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.Dragging, true);
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PressedPause);

		Control dragPreview = GetDragPreview();
		SetDragPreview(dragPreview);
		return this;
	}

	public override void _Notification(int what)
	{
		if (what != NotificationDragEnd || !_isDragging) return;

		_isDragging = false;

		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.Dragging, false);
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PressedPlay);
    }

	private Control GetDragPreview()
	{
		int minimumValue = 64;
		Vector2 minimumSize = new Vector2(minimumValue, minimumValue);

		Color color = Colors.White;
		color.A = 0.65f;
		
		TextureRect textureRect = new TextureRect()
		{
			CustomMinimumSize = minimumSize,
			Texture = Texture,
			TextureFilter = TextureFilterEnum.Nearest,
			SelfModulate = color,
			Position =  -minimumSize / 2
		};

		Control dragPreview = new Control();
		dragPreview.AddChild(textureRect);

		return dragPreview;
	}
}
