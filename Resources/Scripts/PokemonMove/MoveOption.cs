using Godot;

namespace PokemonTD;

public partial class MoveOption : CustomButton
{
	[Export]
	private Label _moveStats;

	[Export]
	private TextureRect _moveType;

	public PokemonMove PokemonMove;

	public override void _Ready()
	{
		if (PokemonMove is null) return;

		_moveStats.Text = $"{PokemonMove.Name}";
		_moveType.Texture = PokemonTD.PokemonTypes.GetTypeIcon(PokemonMove.Type);
	}

	public void UpdateOption(PokemonMove pokemonMove)
	{
		PokemonMove = pokemonMove;
		
		_moveStats.Text = $"{pokemonMove.Name}";
		_moveType.Texture = PokemonTD.PokemonTypes.GetTypeIcon(pokemonMove.Type);
	}

	public void SetFontSize(int fontSize)
	{
		_moveStats.AddThemeFontSizeOverride("font_size", fontSize);
	}
}
