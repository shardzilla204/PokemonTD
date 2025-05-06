using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;
using System;

namespace PokemonTD;

public partial class PokemonManager : Node
{
    private static PokemonManager _instance;

    public static PokemonManager Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

    private GC.Dictionary<string, Variant> _pokemonDictionaries = new GC.Dictionary<string, Variant>();

    public List<string> NidoranFemaleStrings = new List<string>(){ "Nidoran♀", "Nidorina", "Nidoqueen" };
    public List<string> NidoranMaleStrings = new List<string>(){ "Nidoran♂", "Nidorino", "Nidoking" };

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        LoadPokemonFile();

        PokemonTD.Signals.PokemonLeveledUp += OnPokemonLeveledUp;
    }

    private void LoadPokemonFile()
	{
        string filePath = "res://JSON/Pokemon.json";
        
		using FileAccess pokemonFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = pokemonFile.GetAsText();

		Json json = new Json();

		if (json.Parse(jsonString) != Error.Ok) return;

        string loadSuccessMessage = "Pokemon File Successfully Loaded";
        PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);

		_pokemonDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
	}

    public Pokemon GetPokemon(string pokemonName)
    {
        GC.Dictionary<string, Variant> pokemonDictionary = _pokemonDictionaries[pokemonName].As<GC.Dictionary<string, Variant>>();
        GC.Array<string> pokemonTypes = pokemonDictionary["Type"].As<GC.Array<string>>();
        GC.Dictionary<string, Variant> pokemonStats = pokemonDictionary["Base Stats"].As<GC.Dictionary<string, Variant>>();

        Pokemon pokemon = new Pokemon(pokemonName, pokemonDictionary, pokemonTypes, pokemonStats);
        return pokemon;
    }

    public Pokemon GetPokemon(string pokemonName, int pokemonLevel)
    {
        Pokemon pokemon = GetPokemon(pokemonName);
        pokemon.SetLevel(pokemonLevel);
        
        List<PokemonMove> pokemonMoves = PokemonMoveset.Instance.GetPokemonMoveset(pokemon);
        pokemon.SetMoves(pokemonMoves);
        
        // ? Comment out to level instantly
        pokemon.MaxExperience = GetExperienceRequired(pokemon);

        SetPokemonStats(pokemon);

        return pokemon;
    }

    public Pokemon GetRandomPokemon()
    {
        string randomPokemonName = GetRandomPokemonName();
        int randomLevel = GetRandomLevel();
        
        Pokemon randomPokemon = GetPokemon(randomPokemonName, randomLevel);
        randomPokemon.Moves = PokemonTD.AreMovesRandomized ? PokemonMoveset.Instance.GetRandomMoveset() : PokemonMoveset.Instance.GetPokemonMoveset(randomPokemon);
        
        return randomPokemon;
    }

    public int GetRandomLevel()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        return RNG.RandiRange(PokemonTD.MinRandomPokemonLevel, PokemonTD.MaxRandomPokemonLevel);
    }

    private async void OnPokemonLeveledUp(Pokemon pokemon, int teamSlotID)
    {
        if (PokemonEvolution.Instance.CanEvolve(pokemon))
        {
            PokemonTD.Signals.EmitSignal(Signals.SignalName.EvolutionStarted, pokemon);

            await ToSignal(PokemonTD.Signals, Signals.SignalName.PokemonEvolved);
            
            Pokemon pokemonEvolution = PokemonEvolution.Instance.EvolvePokemon(pokemon, teamSlotID);
            
		    PokemonTD.Signals.EmitSignal(Signals.SignalName.EvolutionFinished, pokemonEvolution, teamSlotID);
        }

        SetPokemonStats(pokemon);
    }

    private void SetPokemonStats(Pokemon pokemon)
    {
		pokemon.HP = GetPokemonHP(pokemon);
		pokemon.Attack = GetOtherPokemonStat(pokemon, PokemonStat.Attack);
		pokemon.Defense = GetOtherPokemonStat(pokemon, PokemonStat.Defense);
		pokemon.SpecialAttack = GetOtherPokemonStat(pokemon, PokemonStat.SpecialAttack);
		pokemon.SpecialDefense = GetOtherPokemonStat(pokemon, PokemonStat.SpecialDefense);
		pokemon.Speed = GetOtherPokemonStat(pokemon, PokemonStat.Speed);

		PrintRich.PrintStats(TextColor.Purple, pokemon);
    }

    public string GetRandomPokemonName()
    {
        // Get a random value from 0 to the total count of Pokemon
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomValue = RNG.RandiRange(0, _pokemonDictionaries.Count - 1);

        // Get a random name from a list of keys
        GC.Array<string> pokemonDictionaryKeys = (GC.Array<string>) _pokemonDictionaries.Keys;
        return pokemonDictionaryKeys[randomValue];
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

    public bool IsPokemonMoveLanding(PokemonMove pokemonMove)
    {
        float thresholdValue = 1 - ((float) pokemonMove.Accuracy / 100);
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float randomValue = RNG.RandfRange(0, 1);

        return randomValue >= thresholdValue ? true : false;
    }

    public int GetDamage(Pokemon pokemon, PokemonMove pokemonMove, PokemonEnemy pokemonEnemy)
    {
        float criticalDamageMultiplier = GetCriticalDamageMultiplier(pokemon);
        float attackDefenseRatio = GetAttackDefenseRatio(pokemon, pokemonMove, pokemonEnemy);
        float damage = (((5 * pokemon.Level * criticalDamageMultiplier / 5) + 2) * pokemonMove.Power * attackDefenseRatio / 50) + 2;
        
        List<float> typeMultipliers = PokemonTypes.Instance.GetTypeMultipliers(pokemonMove.Type, pokemonEnemy.Pokemon.Types);
        foreach (float typeMultiplier in typeMultipliers)
        {
            damage *= typeMultiplier;
        }
        
        return Mathf.RoundToInt(damage);
    }

    private float GetAttackDefenseRatio(Pokemon pokemon, PokemonMove pokemonMove, PokemonEnemy pokemonEnemy)
    {
        float specialRatio = (float) pokemon.SpecialAttack / pokemonEnemy.Pokemon.SpecialDefense;
        float normalRatio = (float) pokemon.Attack / pokemonEnemy.Pokemon.Defense;

        float attackDefenseRatio = pokemonMove.Category == MoveCategory.Special ? specialRatio : normalRatio;

        return (float) Math.Round(attackDefenseRatio, 2);
    }
    
    private float GetCriticalDamageMultiplier(Pokemon pokemon)
    {
        return IsCriticalHit(pokemon) ? ((2 * pokemon.Level) + 5) / pokemon.Level + 5 : 1;
    }

    private bool IsCriticalHit(Pokemon pokemon)
    {
        float minThreshold = pokemon.Speed / 2;
        float maxThreshold = 255;

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float thresholdValue = RNG.RandfRange(minThreshold, maxThreshold);
        float randomValue = RNG.RandfRange(0, maxThreshold);

        bool isCriticalHit = randomValue > thresholdValue;

		if (isCriticalHit) 
        {
            string criticalHitMessage = $"{pokemon.Name} Has Landed A Critical Hit";
            PrintRich.PrintLine(TextColor.Purple, criticalHitMessage);

            PokemonTD.AddStageConsoleMessage(TextColor.Purple, criticalHitMessage);
        }

        return isCriticalHit;
    }

    // ? HP Stat Formula
    // (Base * 2 + Level / 100) + Level + 10
    // Base = Stat 
    // Level = Pokemon Level
    private int GetPokemonHP(Pokemon pokemon)
    {
        Pokemon pokemonData = GetPokemon(pokemon.Name);
        return pokemonData.HP * 2 + pokemon.Level / 100 + pokemon.Level + 10;
    }

    // ? Other Stat Formula
    // (Base * 2 + Level / 100) + 5
    // Base = Stat 
    // Level = Pokemon Level
    private int GetOtherPokemonStat(Pokemon pokemon, PokemonStat pokemonStat)
    {
        Pokemon pokemonData = GetPokemon(pokemon.Name);

        int baseStatValue = pokemonStat switch 
        {
            PokemonStat.Attack => pokemonData.Attack,
            PokemonStat.Defense => pokemonData.Defense,
            PokemonStat.SpecialAttack => pokemonData.SpecialAttack,
            PokemonStat.SpecialDefense => pokemonData.SpecialDefense,
            PokemonStat.Speed => pokemonData.Speed,
            _ => pokemonData.Attack,
        };

        return baseStatValue * 2 + pokemon.Level / 100 + 5;
    }

    // ? EXP Formula
	// EXP = 6/5n^3 - 15n^2 + 100n - 140
	// n = Next Pokemon Level
	public int GetExperienceRequired(Pokemon pokemon)
	{
		int nextLevel = pokemon.Level + 1;
		int experience = Mathf.RoundToInt(6 / 5 * Mathf.Pow(nextLevel, 3) - 15 * Mathf.Pow(nextLevel, 2) + (100 * nextLevel) - 140);
		return experience;
	}
}