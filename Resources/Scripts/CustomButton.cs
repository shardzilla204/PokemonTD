using Godot;

namespace PokemonTD;

public partial class CustomButton : Button
{
	[Signal]
	public delegate void LeftClickEventHandler();

	[Signal]
	public delegate void RightClickEventHandler();

	public bool IsHovering;

	public override void _Ready()
	{
		MouseEntered += () =>
		{
			if (PokemonSettings.Instance.ButtonSFXEnabled) PokemonTD.AudioManager.PlayButtonHovered();

			IsHovering = true;
			if (!ButtonPressed) ChangeModulation(true);
		};
		MouseExited += () =>
		{
			IsHovering = false;
			if (!ButtonPressed) ChangeModulation(false);
		};
		Pressed += () =>
		{
			ReleaseFocus();
			if (PokemonSettings.Instance.ButtonSFXEnabled) PokemonTD.AudioManager.PlayButtonPressed();
		};
		Toggled += ChangeModulation;
	}

    public override void _Input(InputEvent @event)
    {
        if (!IsHovering || @event is not InputEventMouseButton eventMouseButton) return;
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
