using Godot;

namespace PokemonTD;

public partial class PokeBall : TextureRect
{
	private bool _isDragging = false;

    public override Variant _GetDragData(Vector2 atPosition)
    {
		_isDragging = true;

		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.Dragging, true);
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PressedPause);

		SetDragPreview(GetDragPreview());
        return this;
    }

    public override void _Notification(int what)
    {
        if (what != NotificationDragEnd || !_isDragging ) return;

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
