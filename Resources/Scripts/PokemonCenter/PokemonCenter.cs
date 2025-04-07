using Godot;

using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

// TODO: Add order functionality

public partial class PokemonCenter : Node
{
	public List<Pokemon> Pokemon = new List<Pokemon>();

	public int PokemonPerPage = 30;

    public override void _EnterTree()
    {
        PokemonTD.PokemonCenter = this;
    }

    public override void _Ready()
    {
		if (PokemonTD.IsPokemonCenterRandomized) AddRandomPokemon();

        PokemonTD.Signals.PokemonEnemyCaptured += AddCapturedPokemon;
    }

	private void AddCapturedPokemon(PokemonEnemy pokemonEnemy)
	{
		if (PokemonTD.PokemonTeam.Pokemon.Count < PokemonTD.MaxTeamSize) return;
		
		Pokemon pokemon = pokemonEnemy.Pokemon;
		pokemon.SetLevel(10);
		pokemon.Moves = pokemon.GetMoveset();
		Pokemon.Add(pokemon);

		string transferredMessage = $"{pokemon.Name} Was Transferred To The Pokemon Center";
		PrintRich.PrintLine(TextColor.Yellow, transferredMessage);
	}

    public List<Pokemon> OrderByLevel()
	{
		return Pokemon.OrderBy(pokemon => pokemon.Level).ToList();
	}

	public List<Pokemon> OrderByNationalNumber()
	{
		return Pokemon.OrderBy(pokemon => pokemon.NationalNumber).ToList();
	}

	public List<Pokemon> OrderByName()
	{
		return Pokemon.OrderBy(pokemon => pokemon.Name).ToList();
	}

	public List<Pokemon> OrderByType()
	{
		return Pokemon.OrderBy(pokemon => pokemon.Types[0]).ToList();
	}

	private void AddRandomPokemon()
	{
		for (int i = 0; i < PokemonTD.PokemonCenterCount; i++)
		{
			Pokemon pokemon = PokemonTD.GetRandomPokemon();
			pokemon.Level = PokemonTD.GetRandomLevel();
			pokemon.Moves = pokemon.GetMoveset();
			Pokemon.Add(pokemon);
		}
	}
}
