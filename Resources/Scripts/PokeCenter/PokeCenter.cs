using Godot;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class PokeCenter : Node
{
	private static PokeCenter _instance;

    public static PokeCenter Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

	public List<Pokemon> Pokemon = new List<Pokemon>();

	public int PokemonPerPage = 30;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
		if (PokemonTD.IsPokeCenterRandomized) AddRandomPokemon();

        PokemonTD.Signals.PokemonEnemyCaptured += AddCapturedPokemon;
    }

	private void AddCapturedPokemon(PokemonEnemy pokemonEnemy)
	{
		if (PokemonTeam.Instance.Pokemon.Count < PokemonTD.MaxTeamSize) return;

		string pokemonName = pokemonEnemy.Pokemon.Name;
		int pokemonLevel = pokemonEnemy.Pokemon.Level;
		
		Pokemon capturedPokemon = PokemonManager.Instance.GetPokemon(pokemonName, pokemonLevel);
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
			Pokemon randomPokemon = PokemonManager.Instance.GetRandomPokemon();
			Pokemon.Add(randomPokemon);
		}
	}

	public void AddPokemon(Pokemon pokemon)
	{
		Pokemon.Insert(0, pokemon);
		PokemonTeam.Instance.RemovePokemon(pokemon);
	}

	public void RemovePokemon(Pokemon pokemon)
	{
		Pokemon.Remove(pokemon);
		PokemonTeam.Instance.AddPokemon(pokemon);
	}
}
