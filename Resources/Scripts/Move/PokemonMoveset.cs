using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;
using System.Linq;

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

    public override void _Ready()
    {
        PokemonTD.Signals.PokemonLeveledUp += OnPokemonLeveledUp;
    }

	private void LoadLearnsetFile()
    {
        string filePath = "res://JSON/PokemonLearnset.json";

        Json json = new Json();

		using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		if (json.Parse(jsonString) != Error.Ok) return;

        string loadSuccessMessage = "Pokemon Learnset File Successfully Loaded";
        PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);

		_pokemonLearnsetDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
    }

	private void OnPokemonLeveledUp(Pokemon pokemon, int teamSlotIndex)
	{
		PokemonMove pokemonMove = GetPokemonMove(pokemon);

		if (pokemonMove is null) return;

		if (pokemon.Moves.Contains(pokemonMove)) return;

		AddPokemonMove(pokemon, pokemonMove);
		pokemon.OldMoves.Add(pokemonMove);
	}

    // Get a pokemon move from the pokemons learnset that equals it's current level
	private PokemonMove GetPokemonMove(Pokemon pokemon)
    {
        PokemonMove pokemonMove = null;

        GC.Dictionary<string, Variant> pokemonLearnsetDictionary = GetPokemonLearnsetDictionary(pokemon.BaseName);
        List<string> pokemonMoveNames = GetPokemonMoveNames(pokemon.BaseName);

        // Remove moves that have already been and has currently learned
        foreach (PokemonMove oldPokemonMove in pokemon.OldMoves)
        {
            pokemonMoveNames.Remove(oldPokemonMove.Name);
        }

        foreach (string pokemonMoveName in pokemonMoveNames)
        {
            List<int> levelRequirements = pokemonLearnsetDictionary[pokemonMoveName].As<GC.Array<int>>().ToList();
            bool hasPassed = HasPassed(levelRequirements, pokemon);

            if (!hasPassed) continue;

            pokemonMove = PokemonMoves.Instance.GetPokemonMove(pokemonMoveName);
            break;
        }

        return pokemonMove;
    }

    // Get at most the first 4 moves
	public List<PokemonMove> GetPokemonMoveset(Pokemon pokemon)
	{
		List<PokemonMove> learnablePokemonMoves = GetLearnablePokemonMoves(pokemon);
        pokemon.OldMoves.AddRange(learnablePokemonMoves); 

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
            PokemonMove pokemonMove = PokemonMoves.Instance.GetPokemonMove(pokemonLearnsetName);
            pokemonMoves.Add(pokemonMove);
        }

        return pokemonMoves;
    }

	private void AddPokemonMove(Pokemon pokemon, PokemonMove pokemonMove)
	{
		if (pokemon.Moves.Count < PokemonTD.MaxMoveCount) 
		{
			pokemon.Moves.Add(pokemonMove);

            string learnedMoveMessage = $"{pokemon.Name} Learned {pokemonMove.Name}";
            PrintRich.PrintLine(TextColor.Purple, learnedMoveMessage);
		}
		else
		{
			PokemonTD.Signals.EmitSignal(Signals.SignalName.ForgetMove, pokemon, pokemonMove);
		}
	}

	private List<PokemonMove> GetRandomPokemonMoveset(List<PokemonMove> pokemonMoves)
	{
		RandomNumberGenerator RNG = new RandomNumberGenerator();
		List<PokemonMove> randomPokemonMoves = new List<PokemonMove>();
        while (randomPokemonMoves.Count < PokemonTD.MaxMoveCount)
        {
            int randomMoveIndex = RNG.RandiRange(0, pokemonMoves.Count - 1);
            PokemonMove randomPokemonMove = pokemonMoves[randomMoveIndex];

            if (!randomPokemonMoves.Contains(randomPokemonMove)) 
            {
                randomPokemonMoves.Add(randomPokemonMove);
            }
        }
		return randomPokemonMoves;
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
            if (HasPassed(levelRequirements, pokemon)) pokemonLearnsetNames.Add(pokemonMoveName);
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
    private bool HasPassed(List<int> levelRequirements, Pokemon pokemon)
    {
        foreach (int levelRequirement in levelRequirements)
        {
            if (pokemon.Level >= levelRequirement) return true;
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
