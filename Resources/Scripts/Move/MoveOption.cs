using Godot;

namespace PokemonTD;

public partial class MoveOption : CustomButton
{
	[Export]
	private NinePatchRect _background;
	
	[Export]
	private Label _moveName;

	[Export]
	private TextureRect _moveType;

	public PokemonMove PokemonMove;

	public override void _Ready()
	{
		if (PokemonMove is null) return;

		_moveName.Text = $"{PokemonMove.Name}";
		_moveType.Texture = PokemonTypes.Instance.GetTypeIcon(PokemonMove.Type);
	}

	public void UpdateOption(PokemonMove pokemonMove)
	{
		PokemonMove = pokemonMove;
		
		_moveName.Text = pokemonMove == null ? "" : $"{pokemonMove.Name}";
		_moveType.Texture = pokemonMove == null ? PokemonTypes.Instance.GetTypeIcon(PokemonType.Normal) : PokemonTypes.Instance.GetTypeIcon(pokemonMove.Type);
	}

	public void SetFontSize(int fontSize)
	{
		_moveName.AddThemeFontSizeOverride("font_size", fontSize);
	}

	public void SetBackgroundColor(Color color)
	{
		_background.SelfModulate = color;
	}
}
