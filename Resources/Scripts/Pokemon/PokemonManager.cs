using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class PokemonManager : Node
{
    private GC.Dictionary<string, Variant> _pokemonDictionaries = new GC.Dictionary<string, Variant>();
    private GC.Dictionary<string, Variant> _pokemonLearnsetDictionaries = new GC.Dictionary<string, Variant>();

    public override void _EnterTree()
    {
        PokemonTD.PokemonManager = this;
    }

    public override void _Ready()
    {
        LoadPokemonFile();
        LoadLearnsetFile();

        PokemonTD.Signals.ForgetMove += OnForgetMove;
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

    public Pokemon GetPokemonData(string pokemonName)
    {
        GC.Dictionary<string, Variant> pokemonDictionary = _pokemonDictionaries[pokemonName].As<GC.Dictionary<string, Variant>>();
        GC.Array<string> pokemonTypes = pokemonDictionary["Type"].As<GC.Array<string>>();
        GC.Dictionary<string, Variant> pokemonStats = pokemonDictionary["Base Stats"].As<GC.Dictionary<string, Variant>>();

        Pokemon pokemon = new Pokemon(pokemonName, pokemonDictionary, pokemonTypes, pokemonStats);
        return pokemon;
    }

    public Pokemon GetPokemon(string pokemonName, int pokemonLevel)
    {
        Pokemon pokemon = new Pokemon(pokemonName, pokemonLevel);
        Pokemon pokemonData = GetPokemonData(pokemonName);

        pokemon.Name = pokemonName;
        pokemon.Species = pokemonData.Species;
        pokemon.Height = pokemonData.Height;
        pokemon.Weight = pokemonData.Weight;
        pokemon.Description = pokemonData.Description;
        pokemon.Sprite = pokemonData.Sprite;
        pokemon.ExperienceYield = pokemonData.ExperienceYield;
        pokemon.MaxExperience = pokemonData.MaxExperience;

        pokemon.Types = pokemonData.Types;

        pokemon.SetStats();

        return pokemon;
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

    public List<PokemonMove> GetPokemonMoves(Pokemon pokemon)
    {
        List<PokemonMove> pokemonMoves = new List<PokemonMove>();

        List<string> pokemonLearnsetNames = GetPokemonLearnsetNames(pokemon);

        foreach (string pokemonLearnsetName in pokemonLearnsetNames)
        {
            PokemonMove pokemonMove = PokemonTD.PokemonMoveset.GetPokemonMove(pokemonLearnsetName);
            pokemonMoves.Add(pokemonMove);

            if (pokemonMoves.Count > PokemonTD.MaxMoveCount) break;
        }

        return pokemonMoves;
    }

    private void OnForgetMove(Pokemon pokemon, PokemonMove pokemonMove)
	{
		ForgetMoveInterface forgetMoveInterface = PokemonTD.PackedScenes.GetForgetMoveInterface();
		forgetMoveInterface.Pokemon = pokemon;
		forgetMoveInterface.MoveToLearn = pokemonMove;
		AddSibling(forgetMoveInterface);
        
        Node parent = GetParent<Node>();
        parent.MoveChild(forgetMoveInterface, parent.GetChildCount());
	}

    public PokemonMove GetPokemonMoveFromLevelUp(Pokemon pokemon, List<PokemonMove> oldMoves)
    {
        GC.Dictionary<string, Variant> pokemonLearnsetDictionary = GetPokemonLearnsetDictionary(pokemon.Name);
        List<string> pokemonMoveNames = GetPokemonMoveNames(pokemon.Name);
        foreach (PokemonMove oldPokemonMove in oldMoves)
        {
            pokemonMoveNames.Remove(oldPokemonMove.Name);
        }

        PokemonMove pokemonMove = null;
        foreach (string pokemonMoveName in pokemonMoveNames)
        {
            List<int> levelRequirements = pokemonLearnsetDictionary[pokemonMoveName].As<GC.Array<int>>().ToList();
            bool hasPassed = HasPassed(levelRequirements, pokemon);

            if (!hasPassed) continue;

            pokemonMove = PokemonTD.PokemonMoveset.GetPokemonMove(pokemonMoveName);
            break;
        }

        return pokemonMove;
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

    // Check level requirement
    private bool HasPassed(List<int> levelRequirements, Pokemon pokemon)
    {
        foreach (int levelRequirement in levelRequirements)
        {
            if (pokemon.Level >= levelRequirement) return true;
        }

        return false;
    }

    private GC.Dictionary<string, Variant> GetPokemonLearnsetDictionary(string pokemonName)
    {
        GD.Print($"Pokemon Name: {pokemonName}");
        return _pokemonLearnsetDictionaries[pokemonName].As<GC.Dictionary<string, Variant>>();
    }

    // Get all the moves the pokemon can learn
    private List<string> GetPokemonMoveNames(string pokemonName)
    {
        GC.Dictionary<string, Variant> pokemonLearnsetDictionary = GetPokemonLearnsetDictionary(pokemonName);
        List<string> moveNames = pokemonLearnsetDictionary.Keys.ToList();

        return moveNames;
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
        float damage = (((2 * pokemon.Level * criticalDamageMultiplier / 5) + 2) * pokemonMove.Power * attackDefenseRatio / 50) + 2;
        
        List<float> typeMultipliers = PokemonTD.PokemonTypes.GetTypeMultipliers(pokemonMove.Type, pokemonEnemy.Pokemon.Types);
        foreach (float typeMultiplier in typeMultipliers)
        {
            damage *= typeMultiplier;
        }
        
        return Mathf.RoundToInt(damage);
    }

    public float GetAttackDefenseRatio(Pokemon pokemon, PokemonMove pokemonMove, PokemonEnemy pokemonEnemy)
    {
        if (pokemonMove.Category == MoveCategory.Special)
        {
            return pokemon.SpecialAttack / pokemonEnemy.Pokemon.SpecialDefense;
        }
        else
        {
            return pokemon.Attack / pokemonEnemy.Pokemon.Defense;
        }
    }
    
    public float GetCriticalDamageMultiplier(Pokemon pokemon)
    {
        return IsCriticalHit(pokemon) ? ((2 * pokemon.Level) + 5) / pokemon.Level + 5 : 1;
    }

    public bool IsCriticalHit(Pokemon pokemon)
    {
        float minThreshold = pokemon.Speed / 2;
        float maxThreshold = 255;

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float thresholdValue = RNG.RandfRange(minThreshold, maxThreshold);
        float randomValue = RNG.RandfRange(0, maxThreshold);

        bool isCriticalHit = randomValue > thresholdValue;

		if (isCriticalHit) 
        {
            string criticalHitMessage = $"{pokemon.Name} Has Landed A Critical Hit!";
            PrintRich.PrintLine(TextColor.Purple, criticalHitMessage);
        }

        return isCriticalHit;
    }

    // ? HP Stat Formula
    // (Base * 2 + Level / 100) + Level + 10
    // Base = Stat 
    // Level = Pokemon Level
    public int GetPokemonHP(Pokemon pokemon)
    {
        Pokemon pokemonData = GetPokemonData(pokemon.Name);
        return pokemonData.HP * 2 + pokemon.Level / 100 + pokemon.Level + 10;
    }

    // ? Other Stat Formula
    // (Base * 2 + Level / 100) + 5
    // Base = Stat 
    // Level = Pokemon Level
    public int GetOtherPokemonStat(Pokemon pokemon, PokemonStat pokemonStat)
    {
        Pokemon pokemonData = GetPokemonData(pokemon.Name);

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
}