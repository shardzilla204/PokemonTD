using System;
using Godot;

namespace PokemonTD;

public partial class PokemonMoveButton : CustomButton
{
	[Export]
	private Label _pokemonMoveName;

	[Export]
	private NinePatchRect _pokemonMoveColor;

	public override void _ExitTree()
	{
		PokemonTD.Signals.PokemonCopiedMove -= ChangeMove;
	}

	public override void _Ready()
	{
		base._Ready();
		PokemonTD.Signals.PokemonCopiedMove += ChangeMove;
	}

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

	private void ChangeMove(PokemonMove pokemonMove, int pokemonTeamIndex)
	{
		PokemonTeamSlot pokemonTeamSlot = GetParentOrNull<Node>().GetOwnerOrNull<PokemonTeamSlot>();
		if (pokemonTeamSlot.PokemonTeamIndex != pokemonTeamIndex) return;

		Update(pokemonMove);
	}
}
