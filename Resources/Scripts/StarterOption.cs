using Godot;

namespace PokemonTD;

public partial class StarterOption : VBoxContainer
{
	[Export]
	private Label _name;

	[Export]
	private CustomButton _button;

	[Export]
	private TextureRect _sprite;

	public Pokemon Pokemon;

	public override void _Ready()
	{
		_name.Text = $"{Pokemon.Name}";
		_sprite.Texture = Pokemon.Sprite;
		_button.Pressed += () => PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonStarterSelected, Pokemon);
	}
}
