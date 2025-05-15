using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class PokemonTeam : Node
{
	private static PokemonTeam _instance;

    public static PokemonTeam Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

	public List<Pokemon> Pokemon = new List<Pokemon>();
	public List<int> StageTeamSlotsMuted = new List<int>();

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonStarterSelected += AddStarterPokemon;
		PokemonTD.Signals.PokemonEnemyCaptured += AddCapturedPokemon;
		PokemonTD.Signals.StageTeamSlotMuted += StageTeamSlotMuted;
		PokemonTD.Signals.GameReset += () => 
		{
			Pokemon.Clear();
			if (PokemonTD.IsTeamRandom) GetRandomTeam(PokemonTD.TeamCount);
		};

		if (PokemonTD.IsTeamRandom) GetRandomTeam(PokemonTD.TeamCount);
	}

	private void StageTeamSlotMuted(int teamSlotIndex, bool isMuted)
	{
		if (isMuted)
		{
			if (StageTeamSlotsMuted.Contains(teamSlotIndex)) return;
			
			StageTeamSlotsMuted.Add(teamSlotIndex);
		}
		else
		{
			StageTeamSlotsMuted.Remove(teamSlotIndex);
		}
	}

	private void AddStarterPokemon(Pokemon pokemon)
	{
		string pokemonName = pokemon.Name;
		int pokemonLevel = pokemon.Level;

		Pokemon starterPokemon = PokemonManager.Instance.GetPokemon(pokemonName, pokemonLevel); // Prevent changes to reference
		Pokemon.Add(pokemon);

		string addedMessage = $"{starterPokemon.Name} Was Selected As Your Starter";
		PrintRich.PrintLine(TextColor.Yellow, addedMessage);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);
	}

	private void AddCapturedPokemon(PokemonEnemy pokemonEnemy)
	{
		if (Pokemon.Count >= PokemonTD.MaxTeamSize) return;

		string pokemonName = pokemonEnemy.Pokemon.Name;
		int pokemonLevel = pokemonEnemy.Pokemon.Level;

		Pokemon capturedPokemon = PokemonManager.Instance.GetPokemon(pokemonName, pokemonLevel); // Prevent changes to reference
		Pokemon.Add(capturedPokemon);

		string addedMessage = $"{capturedPokemon.Name} Was Added To The Team";
		PrintRich.PrintLine(TextColor.Yellow, addedMessage);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);
	}

	private void GetRandomTeam(int teamCount)
	{
		for (int i = 0; i < teamCount; i++)
		{
			Pokemon pokemon = PokemonManager.Instance.GetRandomPokemon();
			pokemon.Level = PokemonManager.Instance.GetRandomLevel();
            pokemon.Moves = PokemonTD.AreMovesRandomized ? PokemonMoveset.Instance.GetRandomMoveset() : PokemonMoveset.Instance.GetPokemonMoveset(pokemon);
			Pokemon.Add(pokemon);
		}
	}

	public void AddPokemon(Pokemon pokemon)
	{
		Pokemon.Add(pokemon);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);

		string addToTeamMessage = $"Adding {pokemon.Name} To Team";
		PrintRich.PrintLine(TextColor.Purple, addToTeamMessage);
	}

	public void RemovePokemon(Pokemon pokemon)
	{
		Pokemon.Remove(pokemon);
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);

		string removeFromTeamMessage = $"Removing {pokemon.Name} From Team";
		PrintRich.PrintLine(TextColor.Purple, removeFromTeamMessage);
	}

	public bool IsFull()
	{
		return Pokemon.Count >= PokemonTD.MaxTeamSize;
	}

	public Pokemon FindPokemon(Pokemon pokemonToFind)
	{
		return Pokemon.Find(pokemon => pokemon == pokemonToFind);
	}

	public GC.Dictionary<string, Variant> GetData()
	{
		GC.Dictionary<string, Variant> pokemonTeamData = new GC.Dictionary<string, Variant>();
		for (int i = 0; i < Pokemon.Count; i++)
		{
			Pokemon pokemon = Pokemon[i];
			GC.Dictionary<string, Variant> pokemonData = PokemonTD.GetPokemonData(pokemon);
			pokemonTeamData.Add($"{i}", pokemonData);
		}
		return pokemonTeamData;
	}

	public void SetData(GC.Dictionary<string, Variant> pokemonTeamData)
	{
		List<string> pokemonKeys = pokemonTeamData.Keys.ToList();
		for (int i = 0; i < pokemonTeamData.Keys.Count; i++)
		{
			string pokemonKey = pokemonKeys[i];
			GC.Dictionary<string, Variant> pokemonData = pokemonTeamData[pokemonKey].As<GC.Dictionary<string, Variant>>();
			string pokemonName = pokemonData["Name"].As<string>();
			Pokemon pokemon = PokemonTD.SetPokemonData(pokemonName, pokemonData);
			Pokemon.Add(pokemon);
		}
	}
}
