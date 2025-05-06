using Godot;

namespace PokemonTD;

public partial class PokeBall : TextureRect
{
	private bool _isDragging = false;

    public override Variant _GetDragData(Vector2 atPosition)
    {
		if (PokemonTD.IsGamePaused) return GetDragPreview();
		
		_isDragging = true;
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingPokeBall, true);
		SetDragPreview(GetDragPreview());
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPause);
        return this;
    }

    public override void _Notification(int what)
    {
        if (what != NotificationDragEnd) return;
		
		if (!_isDragging) return;

		_isDragging = false;
		
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingPokeBall, false);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
    }

	private Control GetDragPreview()
	{
		int minValue = 64;
		Vector2 minSize = new Vector2(minValue, minValue);

		Color color = Colors.White;
		color.A = 0.65f;
		
		TextureRect textureRect = new TextureRect()
		{
			CustomMinimumSize = minSize,
			Texture = Texture,
			TextureFilter = TextureFilterEnum.Nearest,
			SelfModulate = color,
			Position =  -minSize / 2
		};

		Control control = new Control();
		control.AddChild(textureRect);

		return control;
	}
}
