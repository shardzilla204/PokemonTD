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

	[Signal]
	public delegate void QueueUpdatedEventHandler(EvolutionInterface evolutionInterface);

	[Signal]
	public delegate void QueueClearedEventHandler();

	private GC.Dictionary<string, Variant> _pokemonEvolutionDictionaries = new GC.Dictionary<string, Variant>();
	private List<EvolutionInterface> _evolutionQueue = new List<EvolutionInterface>();

    public override void _EnterTree()
    {
        Instance = this;
        LoadEvolutionFile();
    }

	private void LoadEvolutionFile()
	{
		string filePath = "res://JSON/PokemonEvolution.json";

        Json json = new Json();

		using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		if (json.Parse(jsonString) != Error.Ok) return;

		_pokemonEvolutionDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
		
		// Print Message To Console
		string loadSuccessMessage = "Pokemon Evolution File Successfully Loaded";
        PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
	}

	public bool CanEvolve(Pokemon pokemon, int levels)
	{
		try 
		{
			int pokemonLevel = pokemon.Level + levels;
			GC.Dictionary<string, Variant> pokemonEvolutionDictionary = _pokemonEvolutionDictionaries[pokemon.Name].As<GC.Dictionary<string, Variant>>();
			int levelRequirement = (int) pokemonEvolutionDictionary.Values.ToList()[0];
			return pokemonLevel >= levelRequirement;
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
		
		Pokemon pokemonEvolution = PokemonManager.Instance.GetPokemon(pokemonEvolutionNameString);
		pokemonEvolution.Level = pokemon.Level;
		pokemonEvolution.Moves.AddRange(pokemon.Moves);
		pokemonEvolution.Move = pokemon.Move;
		return pokemonEvolution;
	}

	public Pokemon EvolvePokemon(Pokemon pokemon)
	{
		Pokemon pokemonEvolution = GetPokemonEvolution(pokemon);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);
		return pokemonEvolution;
	}

	public void EvolveWithStone()
	{
		
	}

	public void AddToQueue(EvolutionInterface evolutionInterface, PokemonStage pokemonStage)
	{
		if (_evolutionQueue.Count == 0) pokemonStage.AddSibling(evolutionInterface);
		_evolutionQueue.Add(evolutionInterface);
	}

	public void RemoveFromQueue(EvolutionInterface evolutionInterface)
	{
		_evolutionQueue.Remove(evolutionInterface);
		EmitSignal(SignalName.QueueUpdated, evolutionInterface);
	}

	public void ShowNext(PokemonStage pokemonStage)
    {
		EvolutionInterface nextEvolutionInterface = _evolutionQueue[0];
        pokemonStage.AddSibling(nextEvolutionInterface);
    }

	public bool IsQueueEmpty()
	{
		if (_evolutionQueue.Count == 0) EmitSignal(SignalName.QueueCleared);
		return _evolutionQueue.Count == 0;
	}
}
