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
		if (PokemonTD.IsTeamRandom) GetRandomTeam(PokemonTD.TeamCount);

		PokemonTD.Signals.PokemonStarterSelected += AddStarterPokemon;
		PokemonTD.Signals.PokemonEnemyCaptured += AddCapturedPokemon;
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

	private void GetRandomTeam(int teamCount)
	{
		for (int i = 0; i < teamCount; i++)
		{
			Pokemon pokemon = PokemonTD.PokemonManager.GetRandomPokemon();
			pokemon.Level = PokemonTD.PokemonManager.GetRandomLevel();
            pokemon.Moves = PokemonTD.AreMovesRandomized ? PokemonTD.PokemonMoveset.GetRandomMoveset() : PokemonTD.PokemonMoveset.GetPokemonMoveset(pokemon);
			Pokemon.Add(pokemon);
		}
	}

	public bool IsFull()
	{
		return PokemonTD.PokemonTeam.Pokemon.Count >= PokemonTD.MaxTeamSize;
	}
}
