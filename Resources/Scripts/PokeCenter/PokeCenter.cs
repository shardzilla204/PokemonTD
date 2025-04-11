using Godot;

using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

// TODO: Add order functionality

public partial class PokeCenter : Node
{
	public List<Pokemon> Pokemon = new List<Pokemon>();

	public int PokemonPerPage = 30;

    public override void _EnterTree()
    {
        PokemonTD.PokeCenter = this;
    }

    public override void _Ready()
    {
		if (PokemonTD.IsPokeCenterRandomized) AddRandomPokemon();

		OrderByLevel();

        PokemonTD.Signals.PokemonEnemyCaptured += AddCapturedPokemon;
    }

	private void AddCapturedPokemon(PokemonEnemy pokemonEnemy)
	{
		if (PokemonTD.PokemonTeam.Pokemon.Count < PokemonTD.MaxTeamSize) return;
		
		Pokemon pokemon = pokemonEnemy.Pokemon;
		pokemon.SetLevel(10);
		pokemon.Moves = PokemonTD.PokemonMoveset.GetPokemonMoveset(pokemon);
		Pokemon.Add(pokemon);

		string transferredMessage = $"{pokemon.Name} Was Transferred To The Pokemon Center";
		PrintRich.PrintLine(TextColor.Yellow, transferredMessage);
	}

    public void OrderByLevel()
	{
		Pokemon = Pokemon.OrderBy(pokemon => pokemon.Level).ToList();
	}

	public void OrderByNationalNumber()
	{
		Pokemon = Pokemon.OrderBy(pokemon => pokemon.NationalNumber).ToList();
	}

	public void OrderByName()
	{
		Pokemon = Pokemon.OrderBy(pokemon => pokemon.Name).ToList();
	}

	public void OrderByType()
	{
		Pokemon = Pokemon.OrderBy(pokemon => pokemon.Types[0]).ToList();
	}

	private void AddRandomPokemon()
	{
		for (int i = 0; i < PokemonTD.PokeCenterCount; i++)
		{
			Pokemon randomPokemon = PokemonTD.PokemonManager.GetRandomPokemon();
			Pokemon.Add(randomPokemon);
		}
	}
}
