using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;
using System.Linq;
using System;

namespace PokemonTD;

public partial class PokemonMoveset : Node
{
    private static PokemonMoveset _instance;

    public static PokemonMoveset Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

	private GC.Dictionary<string, Variant> _pokemonLearnsetDictionaries = new GC.Dictionary<string, Variant>();

    public override void _EnterTree()
    {
        Instance = this;
        LoadLearnsetFile();
    }

	private void LoadLearnsetFile()
    {
        string filePath = "res://JSON/PokemonLearnset.json";

        Json json = new Json();

		using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		if (json.Parse(jsonString) != Error.Ok) return;

		_pokemonLearnsetDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);

        // Print Message To Console
        string loadSuccessMessage = "Pokemon Learnset File Successfully Loaded";
        PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
    }

    // Gets pokemon moves from the pokemons learnset
	public List<PokemonMove> GetPokemonMoves(Pokemon pokemon, int levels)
    {
        List<PokemonMove> pokemonMoves = new List<PokemonMove>();

        GC.Dictionary<string, Variant> pokemonLearnsetDictionary = GetPokemonLearnsetDictionary(pokemon.Name);
        List<string> pokemonMoveNames = GetPokemonMoveNames(pokemon.Name);

        foreach (string pokemonMoveName in pokemonMoveNames)
        {
            List<int> levelRequirements = pokemonLearnsetDictionary[pokemonMoveName].As<GC.Array<int>>().ToList();
            bool hasPassedLevelRequirement = HasPassedLevelRequirement(levelRequirements, pokemon, levels);
            if (!hasPassedLevelRequirement) continue;

            PokemonMove pokemonMove = PokemonMoves.Instance.FindPokemonMove(pokemonMoveName);
            pokemonMoves.Add(pokemonMove);
        }

        return pokemonMoves;
    }

    // Get at most the first 4 moves
    public List<PokemonMove> GetPokemonMoveset(Pokemon pokemon)
    {
        List<PokemonMove> learnablePokemonMoves = GetLearnablePokemonMoves(pokemon);

        // Get a random set of moves that the pokemon can learn if the count is above 4
        List<PokemonMove> pokemonMoveset = new List<PokemonMove>();
        if (learnablePokemonMoves.Count > PokemonTD.MaxMoveCount)
        {
            pokemonMoveset.AddRange(GetRandomPokemonMoveset(learnablePokemonMoves));
        }
        else
        {
            pokemonMoveset.AddRange(learnablePokemonMoves);
        }

        return pokemonMoveset;
    }
    
    // Gets all the moves that the pokemon can learn
    public List<PokemonMove> GetLearnablePokemonMoves(Pokemon pokemon)
    {
        List<PokemonMove> pokemonMoves = new List<PokemonMove>();
        List<string> pokemonLearnsetNames = GetPokemonLearnsetNames(pokemon);

        foreach (string pokemonLearnsetName in pokemonLearnsetNames)
        {
            PokemonMove pokemonMove = PokemonMoves.Instance.FindPokemonMove(pokemonLearnsetName);
            pokemonMoves.Add(pokemonMove);
        }

        return pokemonMoves;
    }

	public void AddPokemonMove(Pokemon pokemon, PokemonMove pokemonMove)
	{
		if (pokemon.Moves.Count < PokemonTD.MaxMoveCount) 
		{
			pokemon.Moves.Add(pokemonMove);

            // Print Message To Console
            string learnedMoveMessage = $"{pokemon.Name} Learned {pokemonMove.Name}";
            PrintRich.PrintLine(TextColor.Purple, learnedMoveMessage);
		}
		else
		{
			PokemonTD.Signals.EmitSignal(Signals.SignalName.ForgetMove, pokemon, pokemonMove);
		}
	}

    
    public List<PokemonMove> GetRandomPokemonMoveset(List<PokemonMove> pokemonMoves)
    {
        List<PokemonMove> randomPokemonMoves = new List<PokemonMove>();
        for (int i = 0; i < PokemonTD.MaxMoveCount; i++)
        {
            while (true)
            {
                PokemonMove randomPokemonMove = GetRandomPokemonMove(pokemonMoves);
                if (randomPokemonMoves.Contains(randomPokemonMove)) continue;
                
                randomPokemonMoves.Add(randomPokemonMove);
                break;
            }
        }
        return randomPokemonMoves;
    }
    
    private PokemonMove GetRandomPokemonMove(List<PokemonMove> pokemonMoves)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomMoveIndex = RNG.RandiRange(0, pokemonMoves.Count - 1);
		return pokemonMoves[randomMoveIndex];
    }

	private List<string> GetPokemonLearnsetNames(Pokemon pokemon)
    {
        List<string> pokemonLearnsetNames = new List<string>();

        GC.Dictionary<string, Variant> pokemonLearnsetDictionary = GetPokemonLearnsetDictionary(pokemon.Name);
        List<string> pokemonMoveNames = GetPokemonMoveNames(pokemon.Name);

        // Filter out moves that have a higher level required to learn
        foreach (string pokemonMoveName in pokemonMoveNames)
        {
            List<int> levelRequirements = pokemonLearnsetDictionary[pokemonMoveName].As<GC.Array<int>>().ToList();

            // Get the name of the move when it passes the level requirement
            if (HasPassedLevelRequirement(levelRequirements, pokemon)) pokemonLearnsetNames.Add(pokemonMoveName);
        }
        return pokemonLearnsetNames;
    }

	private GC.Dictionary<string, Variant> GetPokemonLearnsetDictionary(string pokemonName)
    {
        GC.Dictionary<string, Variant> learnsetDictionary = new GC.Dictionary<string, Variant>();
        try 
        {
            learnsetDictionary = _pokemonLearnsetDictionaries[pokemonName].As<GC.Dictionary<string, Variant>>();
        }
        catch (KeyNotFoundException)
        {
            GD.PrintErr(pokemonName);
        }
        return learnsetDictionary;
    }

	// Checks the level requirement
    private bool HasPassedLevelRequirement(List<int> levelRequirements, Pokemon pokemon)
    {
        foreach (int levelRequirement in levelRequirements)
        {
            if (pokemon.Level >= levelRequirement) return true;
        }

        return false;
    }

	// Checks the level requirement
    private bool HasPassedLevelRequirement(List<int> levelRequirements, Pokemon pokemon, int levels)
    {
        for (int i = 0; i < levels; i++)
        {
            int pokemonLevel = pokemon.Level + i;
            foreach (int levelRequirement in levelRequirements)
            {
                if (pokemonLevel == levelRequirement) 
                {
                    return true;
                }
            }
        }

        return false;
    }

    private List<string> GetPokemonMoveNames(string pokemonName)
    {
        GC.Dictionary<string, Variant> pokemonLearnsetDictionary = GetPokemonLearnsetDictionary(pokemonName);
        List<string> moveNames = pokemonLearnsetDictionary.Keys.ToList();

        return moveNames;
    }

     // ! Doesn't get moves from the Pokemon's learnset, completely random
    public List<PokemonMove> GetRandomMoveset()
	{
        List<PokemonMove> pokemonMoves = new List<PokemonMove>();
		for (int i = 0; i < PokemonTD.MaxMoveCount; i++)
		{
			pokemonMoves.Add(PokemonMoves.Instance.GetRandomPokemonMove());
		}
        return pokemonMoves;
    }
}
