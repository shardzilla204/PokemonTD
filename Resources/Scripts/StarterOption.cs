using Godot;

namespace PokémonTD;

public partial class StarterOption : VBoxContainer
{
	[Export]
	private Label _name;

	[Export]
	private CustomButton _button;

	[Export]
	private TextureRect _sprite;

	public Pokémon Pokémon;

	public override void _Ready()
	{
		Pokémon.Level = 5;
		_name.Text = $"{Pokémon.Name}";
		_sprite.Texture = PokémonTD.GetPokémonSprite(Pokémon.Name);
		_button.Pressed += () => PokémonTD.Signals.EmitSignal(Signals.SignalName.PokémonStarterSelected, Pokémon);
	}
}
