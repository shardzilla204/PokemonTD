using Godot;

namespace PokemonTD;

public partial class MoveOption : CustomButton
{
	[Export]
	private NinePatchRect _background;

	[Export]
	private Label _moveName;

	[Export]
	private TextureRect _moveCategory;

	[Export]
	private TextureRect _moveType;

	public PokemonMove PokemonMove;
	private bool _isForgettingMove = false;

	public override void _Ready()
	{
		if (PokemonMove != null && PokemonMoves.Instance.IsAutomaticMove(PokemonMove)) Disable();
	}

	public void SetOption(PokemonMove pokemonMove, bool isForgettingMove)
	{
		PokemonMove = pokemonMove;
		_isForgettingMove = isForgettingMove;

		_moveName.Text = pokemonMove == null ? "" : $"{pokemonMove.Name}";
		_moveCategory.Texture = pokemonMove == null ? null : PokemonTD.GetMoveCategoryIcon(pokemonMove.Category);
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

	private void Disable()
	{
		if (_isForgettingMove) return;

		Color disabledColor = _background.SelfModulate.Darkened(0.25f);
		_background.SelfModulate = disabledColor;
		Disabled = true;
	}
}
