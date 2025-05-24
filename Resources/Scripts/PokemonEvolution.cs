using Godot;
using GC = Godot.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

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
        if (PrintRich.AreFileMessagesEnabled) PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
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

	public Pokemon GetPokemonEvolution(Pokemon pokemon, string pokemonEvolutionName)
	{		
		Pokemon pokemonEvolution = PokemonManager.Instance.GetPokemon(pokemonEvolutionName, pokemon.Level);
		pokemonEvolution.Moves.Clear();
		pokemonEvolution.Moves.AddRange(pokemon.Moves);
		pokemonEvolution.Move = pokemon.Move;
		return pokemonEvolution;
	}

	public Pokemon EvolvePokemon(Pokemon pokemon)
	{
		string pokemonEvolutionName = GetPokemonEvolutionName(pokemon, EvolutionStone.None);
		if (pokemonEvolutionName == "") return null;

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);
		Pokemon pokemonEvolution = GetPokemonEvolution(pokemon, pokemonEvolutionName);
		return pokemonEvolution;
	}

	public Pokemon EvolvePokemonWithStone(Pokemon pokemon, EvolutionStone evolutionStone)
	{
		string pokemonEvolutionName = GetPokemonEvolutionName(pokemon, evolutionStone);
		if (pokemonEvolutionName == "") return null;

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);
		Pokemon pokemonEvolution = GetPokemonEvolution(pokemon, pokemonEvolutionName);
		return pokemonEvolution;
	}

	public string GetPokemonEvolutionName(Pokemon pokemon, EvolutionStone evolutionStone)
	{
		GC.Dictionary<string, Variant> pokemonEvolutionDictionary = _pokemonEvolutionDictionaries[pokemon.Name].As<GC.Dictionary<string, Variant>>();
		if (evolutionStone == EvolutionStone.None) return pokemonEvolutionDictionary.Keys.ToList()[0];

		List<string> pokemonEvolutionNames = pokemonEvolutionDictionary.Keys.ToList();
		foreach (string pokemonEvolutionName in pokemonEvolutionNames)
		{
			string evolutionStoneName = pokemonEvolutionDictionary[pokemonEvolutionName].As<string>();
			EvolutionStone pokemonEvolutionStone = Enum.Parse<EvolutionStone>(evolutionStoneName);
			if (evolutionStone == pokemonEvolutionStone) return pokemonEvolutionName;
		}
		return "";
	}

	public bool CanEvolveWithStone(Pokemon pokemon)
	{
		GC.Dictionary<string, Variant> pokemonEvolutionDictionary = _pokemonEvolutionDictionaries[pokemon.Name].As<GC.Dictionary<string, Variant>>();
		List<EvolutionStone> evolutionStones = GetEvolutionStones(pokemonEvolutionDictionary);
		foreach (EvolutionStone evolutionStone in evolutionStones)
		{
			// Print Message To Console
			string canEvolveWithStone = $"{pokemon.Name} Can Evolve With {evolutionStone}";
			if (PrintRich.AreFileMessagesEnabled) PrintRich.PrintLine(TextColor.Green, canEvolveWithStone);
		}
		return evolutionStones.Count != 0;
	}

	private EvolutionStone GetEvolutionStone(string evolutionStoneName)
	{
		EvolutionStone evolutionStone;
		bool isEvolutionStone = Enum.TryParse(evolutionStoneName, out evolutionStone);
		return isEvolutionStone ? evolutionStone : EvolutionStone.None;
	}

	private List<EvolutionStone> GetEvolutionStones(GC.Dictionary<string, Variant> pokemonEvolutionDictionary)
	{
		List<EvolutionStone> evolutionStones = new List<EvolutionStone>();
		foreach (string pokemonEvolutionName in pokemonEvolutionDictionary.Keys)
		{
			string evolutionStoneName = pokemonEvolutionDictionary[pokemonEvolutionName].As<string>();
			EvolutionStone evolutionStone = GetEvolutionStone(evolutionStoneName);

			if (evolutionStone == EvolutionStone.None) continue;

			evolutionStones.Add(evolutionStone);
		}
		return evolutionStones;
	}

	public void AddToQueue(EvolutionInterface evolutionInterface, Node sibling)
	{
		if (_evolutionQueue.Count == 0) sibling.AddSibling(evolutionInterface);
		_evolutionQueue.Add(evolutionInterface);
	}

	public void RemoveFromQueue(EvolutionInterface evolutionInterface)
	{
		_evolutionQueue.Remove(evolutionInterface);
		EmitSignal(SignalName.QueueUpdated, evolutionInterface);
	}

	public void ShowNext(Node sibling)
    {
		EvolutionInterface nextEvolutionInterface = _evolutionQueue[0];
        sibling.AddSibling(nextEvolutionInterface);
    }

	public bool IsQueueEmpty()
	{
		if (_evolutionQueue.Count == 0) EmitSignal(SignalName.QueueCleared);
		return _evolutionQueue.Count == 0;
	}
}
