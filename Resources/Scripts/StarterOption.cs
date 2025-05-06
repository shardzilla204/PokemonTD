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
		_button.MouseEntered += () => 
		{
			PokemonTD.AudioManager.PlayPokemonCry(Pokemon, false);
			TweenRotate();
		};
	}

	private void TweenRotate()
	{
		float degrees = 7.5f;
		float radians = Mathf.DegToRad(degrees);
		float duration = 0.25f;

		Tween tween = CreateTween();
		tween.TweenProperty(_sprite, "rotation", -radians, duration);
		tween.TweenProperty(_sprite, "rotation", radians, duration);
		tween.TweenProperty(_sprite, "rotation", 0, duration);
	}
}
