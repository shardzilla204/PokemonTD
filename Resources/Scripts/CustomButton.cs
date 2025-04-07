using Godot;

namespace PokemonTD;

public partial class CustomButton : Button
{
	public override void _Ready()
	{
		MouseEntered += () => ChangeModulation(true);
		MouseExited += () => ChangeModulation(false);
	}

	private void ChangeModulation(bool hasEntered)
	{
		float darkPercentage = 0.15f;
		Modulate = hasEntered ? Colors.White.Darkened(darkPercentage) : Colors.White;
	}
}
