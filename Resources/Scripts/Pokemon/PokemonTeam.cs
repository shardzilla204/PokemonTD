using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonTeam : Node
{
	public List<Pokemon> Pokemon = new List<Pokemon>();

	public override void _EnterTree()
	{
		PokemonTD.PokemonTeam = this;
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonEnemyCaptured += AddCapturedPokemon;
		PokemonTD.Signals.PokemonStarterSelected += AddStarterPokemon;
	}

	private void AddStarterPokemon(Pokemon pokemon)
	{
		Pokemon.Add(pokemon);

		string addedMessage = $"{pokemon.Name} Was Selected As Your Starter";
		PrintRich.PrintLine(TextColor.Yellow, addedMessage);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);
	}

	private void AddCapturedPokemon(PokemonEnemy pokemonEnemy)
	{
		if (Pokemon.Count >= PokemonTD.MaxTeamSize) return;

		Pokemon pokemon = pokemonEnemy.Pokemon;
		Pokemon.Add(pokemon);

		string addedMessage = $"{pokemon.Name} Was Added To The Team";
		PrintRich.PrintLine(TextColor.Yellow, addedMessage);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);
	}
}
