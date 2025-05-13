using System;
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
		try 
		{
			_pokemonMoveName.Text = pokemonMove.Name;
			_pokemonMoveColor.SelfModulate = PokemonTypes.Instance.GetTypeColor(pokemonMove.Type);
		}
		catch (NullReferenceException)
		{
			GD.PrintErr(pokemonMove.Name);
		}
	}
}
