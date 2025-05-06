using Godot;

using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

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

		OrderByLevel(false);

        PokemonTD.Signals.PokemonEnemyCaptured += AddCapturedPokemon;
    }

	private void AddCapturedPokemon(PokemonEnemy pokemonEnemy)
	{
		if (PokemonTD.PokemonTeam.Pokemon.Count < PokemonTD.MaxTeamSize) return;

		string pokemonName = pokemonEnemy.Pokemon.Name;
		int pokemonLevel = pokemonEnemy.Pokemon.Level;
		
		Pokemon capturedPokemon = PokemonTD.PokemonManager.GetPokemon(pokemonName, pokemonLevel);
		Pokemon.Add(capturedPokemon);

		string transferredMessage = $"{capturedPokemon.Name} Was Transferred To The Pokemon Center";
		PrintRich.PrintLine(TextColor.Yellow, transferredMessage);
	}

    public void OrderByLevel(bool isDescending)
	{
		Pokemon = isDescending ? Pokemon.OrderByDescending(pokemon => pokemon.Level).ToList() : Pokemon.OrderBy(pokemon => pokemon.Level).ToList();
	}

	public void OrderByNationalNumber(bool isDescending)
	{
		Pokemon = isDescending ? Pokemon.OrderByDescending(pokemon => pokemon.NationalNumber).ToList() : Pokemon.OrderBy(pokemon => pokemon.NationalNumber).ToList();
	}

	public void OrderByName(bool isDescending)
	{
		Pokemon = isDescending ? Pokemon.OrderByDescending(pokemon => pokemon.Name).ToList() : Pokemon.OrderBy(pokemon => pokemon.Name).ToList();
	}

	public void OrderByType(bool isDescending)
	{
		Pokemon = isDescending ? Pokemon.OrderByDescending(pokemon => pokemon.Types[0]).ToList() : Pokemon.OrderBy(pokemon => pokemon.Types[0]).ToList();
	}

	private void AddRandomPokemon()
	{
		for (int i = 0; i < PokemonTD.PokeCenterCount; i++)
		{
			Pokemon randomPokemon = PokemonTD.PokemonManager.GetRandomPokemon();
			Pokemon.Add(randomPokemon);
		}
	}

	public void AddPokemon(Pokemon pokemon)
	{
		PokemonTD.PokeCenter.Pokemon.Insert(0, pokemon);
		PokemonTD.PokemonTeam.RemovePokemon(pokemon);
	}

	public void RemovePokemon(Pokemon pokemon)
	{
		PokemonTD.PokeCenter.Pokemon.Remove(pokemon);
		PokemonTD.PokemonTeam.AddPokemon(pokemon);
	}
}
