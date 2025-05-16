using Godot;

namespace PokemonTD;

public partial class CustomButton : Button
{
	[Signal]
	public delegate void LeftClickEventHandler();

	[Signal]
	public delegate void RightClickEventHandler();

	private bool _isHovering;

	public override void _Ready()
	{
		MouseEntered += () =>
		{
			PokemonTD.AudioManager.PlayButtonHovered();

			_isHovering = true;
			ChangeModulation(true);
		};
		MouseExited += () =>
		{
			_isHovering = false;
			ChangeModulation(false);
		};
		Pressed += PokemonTD.AudioManager.PlayButtonPressed;
	}

    public override void _Input(InputEvent @event)
    {
		if (!_isHovering) return;
        if (@event is not InputEventMouseButton eventMouseButton) return;
		if (!eventMouseButton.Pressed) return;
		if (eventMouseButton.ButtonIndex == MouseButton.Left)
		{
			EmitSignal(SignalName.LeftClick);
		}
		else if (eventMouseButton.ButtonIndex == MouseButton.Right)
		{
			EmitSignal(SignalName.RightClick);
		}
    }


	private void ChangeModulation(bool hasEntered)
	{
		float darkPercentage = 0.15f;
		Modulate = hasEntered ? Colors.White.Darkened(darkPercentage) : Colors.White;
	}
}
