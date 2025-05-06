using Godot;

namespace PokemonTD;

public partial class MoveOption : CustomButton
{
	[Export]
	private NinePatchRect _background;
	
	[Export]
	private Label _moveStats;

	[Export]
	private TextureRect _moveType;

	public PokemonMove PokemonMove;

	public override void _Ready()
	{
		if (PokemonMove is null) return;

		_moveStats.Text = $"{PokemonMove.Name}";
		_moveType.Texture = PokemonTypes.Instance.GetTypeIcon(PokemonMove.Type);
	}

	public void UpdateOption(PokemonMove pokemonMove)
	{
		PokemonMove = pokemonMove;
		
		_moveStats.Text = $"{pokemonMove.Name}";
		_moveType.Texture = PokemonTypes.Instance.GetTypeIcon(pokemonMove.Type);
	}

	public void SetFontSize(int fontSize)
	{
		_moveStats.AddThemeFontSizeOverride("font_size", fontSize);
	}

	public void SetBackgroundColor(Color color)
	{
		_background.SelfModulate = color;
	}
}
