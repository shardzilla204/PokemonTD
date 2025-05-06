using Godot;
using GC = Godot.Collections;
using System.Linq;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonEvolution : Node
{
	private static PokemonEvolution _instance;

    public static PokemonEvolution Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

	private GC.Dictionary<string, Variant> _pokemonEvolutionDictionaries = new GC.Dictionary<string, Variant>();
	private List<EvolutionInterface> _evolutionQueue = new List<EvolutionInterface>();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        LoadEvolutionFile();

		// Pokemon pokemon = PokemonManager.Instance.GetPokemon("Eevee");
		// CanEvolve(pokemon);

		PokemonTD.Signals.EvolutionFinished += (pokemon, teamSlotID) => IsQueueEmpty();
    }

	private void LoadEvolutionFile()
	{
		string filePath = "res://JSON/PokemonEvolution.json";

        Json json = new Json();

		using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		if (json.Parse(jsonString) != Error.Ok) return;

        string loadSuccessMessage = "Pokemon Evolution File Successfully Loaded";
        PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);

		_pokemonEvolutionDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
	}

	public bool CanEvolve(Pokemon pokemon)
	{
		try 
		{
			GC.Dictionary<string, Variant> pokemonEvolutionDictionary = _pokemonEvolutionDictionaries[pokemon.Name].As<GC.Dictionary<string, Variant>>();
			int levelRequirement = (int) pokemonEvolutionDictionary.Values.ToList()[0];
			return pokemon.Level == levelRequirement;
		}
		catch (KeyNotFoundException)
		{
			return false;
		}
	}

	public Pokemon GetPokemonEvolution(Pokemon pokemon)
	{
		GC.Dictionary<string, Variant> pokemonEvolutionDictionary = _pokemonEvolutionDictionaries[pokemon.Name].As<GC.Dictionary<string, Variant>>();
		string pokemonEvolutionNameString = pokemonEvolutionDictionary.Keys.ToList()[0];
		return PokemonManager.Instance.GetPokemon(pokemonEvolutionNameString);
	}

	public Pokemon EvolvePokemon(Pokemon pokemon, int teamSlotID)
	{
		Pokemon pokemonEvolution = GetPokemonEvolution(pokemon);

		pokemonEvolution.BaseName = pokemon.BaseName;
		pokemonEvolution.Level = pokemon.Level;
		pokemonEvolution.Moves.AddRange(pokemon.Moves);
		pokemonEvolution.Move = pokemon.Move;
		pokemonEvolution.OldMoves.AddRange(pokemon.OldMoves);

		PokemonTeam.Instance.Pokemon.RemoveAt(teamSlotID);
		PokemonTeam.Instance.Pokemon.Insert(teamSlotID, pokemonEvolution);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);

		return pokemonEvolution;
	}

	public void EvolveWithStone()
	{
		
	}

	public void AddToQueue(EvolutionInterface evolutionInterface)
	{
		_evolutionQueue.Add(evolutionInterface);
	}

	public void RemoveFromQueue(EvolutionInterface evolutionInterface)
	{
		_evolutionQueue.Remove(evolutionInterface);
	}

	public bool IsQueueEmpty()
	{
		if (_evolutionQueue.Count != 0) return false;

		PokemonTD.Signals.EmitSignal(Signals.SignalName.EvolutionQueueCleared);

		return true;
	}
}
