using Godot;

namespace PokemonTD;

public partial class PokemonMoveButton : CustomButton
{
	[Export]
	private Label _pokemonMoveName;

	[Export]
	private NinePatchRect _pokemonMoveColor;

	public void Update(PokemonMove pokemonMove)
	{
		_pokemonMoveName.Text = pokemonMove.Name;
		_pokemonMoveColor.SelfModulate = PokemonTypes.Instance.GetTypeColor(pokemonMove.Type);
	}
}
