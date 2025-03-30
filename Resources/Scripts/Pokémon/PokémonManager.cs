using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokémonTD;

public partial class PokémonManager : Node
{
    private GC.Dictionary<string, Variant> _pokémonDictionaries = new GC.Dictionary<string, Variant>();
    private GC.Dictionary<string, Variant> _pokémonLearnsetDictionaries = new GC.Dictionary<string, Variant>();

    public override void _EnterTree()
    {
        PokémonTD.PokémonManager = this;
    }

    public override void _Ready()
    {
        LoadPokémonFile();
        LoadLearnsetFile();
    }

    private void LoadPokémonFile()
	{
        string filePath = "res://JSON/Pokémon.json";
        
		using FileAccess pokémonFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = pokémonFile.GetAsText();

		Json json = new Json();

		if (json.Parse(jsonString) != Error.Ok) return;

        string loadSuccessMessage = "Pokémon File Successfully Loaded";
        PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);

		_pokémonDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
	}

    private void LoadLearnsetFile()
    {
        string filePath = "res://JSON/PokémonLearnset.json";

        Json json = new Json();

		using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		if (json.Parse(jsonString) != Error.Ok) return;

        string loadSuccessMessage = "Pokémon Learnset File Successfully Loaded";
        PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);

		_pokémonLearnsetDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
    }

    public Pokémon GetPokémon(string pokémonName)
    {
        GC.Dictionary<string, Variant> pokémonDictionary = _pokémonDictionaries[pokémonName].As<GC.Dictionary<string, Variant>>();
        GC.Array<string> pokémonTypes = pokémonDictionary["Type"].As<GC.Array<string>>();
        GC.Dictionary<string, Variant> pokémonStats = pokémonDictionary["Base Stats"].As<GC.Dictionary<string, Variant>>();

        Pokémon pokémon = new Pokémon(pokémonName, pokémonDictionary, pokémonTypes, pokémonStats);

        if (PokémonTD.AreLevelsRandomized) pokémon.Level = PokémonTD.GetRandomLevel();
        if (PokémonTD.AreMovesRandomized) 
        {
            pokémon.Moves = PokémonTD.GetRandomMoveset();
        }
        else
        {
            pokémon.SetMoveset();
        }

        return pokémon;
    }

    public Pokémon GetRandomPokémon()
    {
        string pokémonName = GetRandomPokémonName();
        return GetPokémon(pokémonName);
    }

    private string GetRandomPokémonName()
    {
        // Get a random value from 0 to the total count of pokémon
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomValue = RNG.RandiRange(0, _pokémonDictionaries.Count - 1);

        // Get a random name from a list of keys
        GC.Array<string> pokémonDictionaryKeys = (GC.Array<string>) _pokémonDictionaries.Keys;
        return pokémonDictionaryKeys[randomValue];
    }

    public List<PokémonMove> GetPokémonMoves(Pokémon pokémon)
    {
        List<PokémonMove> pokémonMoves = new List<PokémonMove>();

        List<string> pokémonLearnsetNames = GetPokémonLearnsetNames(pokémon);
        foreach (string pokémonMoveName in pokémonLearnsetNames)
        {
            PokémonMove pokémonMove = PokémonTD.PokémonMoveset.GetPokémonMove(pokémonMoveName);

            if (pokémonMove is not null) pokémonMoves.Add(pokémonMove);
        }
        return pokémonMoves;
    }

    private List<string> GetPokémonLearnsetNames(Pokémon pokémon)
    {
        List<string> learnsetNames = new List<string>();

        GC.Dictionary<string, Variant> pokémonLearnsetDictionary = GetPokémonLearnsetDictionary(pokémon.Name);
        List<string> moveNames = GetMoveNames(pokémon.Name);

        foreach (string moveName in moveNames)
        {
            List<int> levelRequirements = pokémonLearnsetDictionary[moveName].As<GC.Array<int>>().ToList();
            learnsetNames.Add(CheckLevelRequirement(levelRequirements, pokémon, moveName));
        }
        return learnsetNames;
    }

    // Get the name of the move when it passes a level requirement
    private string CheckLevelRequirement(List<int> levelRequirements, Pokémon pokémon, string moveName)
    {
        foreach (int levelRequirement in levelRequirements)
        {
            if (pokémon.Level >= levelRequirement) return moveName;
        }

        return "";
    }

    private GC.Dictionary<string, Variant> GetPokémonLearnsetDictionary(string pokémonName)
    {
        return _pokémonLearnsetDictionaries[pokémonName].As<GC.Dictionary<string, Variant>>();
    }

    private List<string> GetMoveNames(string pokémonName)
    {
        GC.Dictionary<string, Variant> pokémonLearnsetDictionary = GetPokémonLearnsetDictionary(pokémonName);
        List<string> moveNames = pokémonLearnsetDictionary.Keys.ToList();

        return moveNames;
    }

    public bool IsMoveLanding(PokémonMove pokémonMove)
    {
        float thresholdValue = 1 - ((float) pokémonMove.Accuracy / 100);
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float randomValue = RNG.RandfRange(0, 1);

        return randomValue >= thresholdValue ? true : false;
    }

    public int GetDamage(Pokémon pokémon, PokémonMove pokémonMove, PokémonEnemy pokémonEnemy)
    {
        float criticalMultiplier = GetCriticalDamageMultiplier(pokémon);

        float attackDefenseRatio = GetAttackDefenseRatio(pokémon, pokémonMove, pokémonEnemy);

        float damage = (((2 * pokémon.Level * criticalMultiplier / 5) + 2) * pokémonMove.Power * attackDefenseRatio / 50) + 2;
        
        List<float> typeMultipliers = PokémonTD.PokémonTypes.GetTypeMultipliers(pokémonMove.Type, pokémonEnemy.Pokémon.Types);
        foreach (float typeMultiplier in typeMultipliers)
        {
            damage *= typeMultiplier;
        }
        
        return Mathf.RoundToInt(damage);
    }

    public float GetAttackDefenseRatio(Pokémon pokémon, PokémonMove pokémonMove, PokémonEnemy pokémonEnemy)
    {
        if (pokémonMove.Category == MoveCategory.Special)
        {
            return pokémon.SpecialAttack / pokémonEnemy.Pokémon.SpecialDefense;
        }
        else
        {
            return pokémon.Attack / pokémonEnemy.Pokémon.Defense;
        }
    }
    
    public float GetCriticalDamageMultiplier(Pokémon pokémon)
    {
        return IsCriticalHit(pokémon) ? ((2 * pokémon.Level) + 5) / pokémon.Level + 5 : 1;
    }

    public bool IsCriticalHit(Pokémon pokémon)
    {
        float minThreshold = pokémon.Speed / 2;

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float thresholdValue = RNG.RandfRange(minThreshold, 255);
        float randomValue = RNG.RandfRange(0, 255);

        bool isCriticalHit = randomValue > thresholdValue;

        string criticalHitMessage = $"{pokémon.Name} Has Landed A Critical Hit!";
		if (isCriticalHit) PrintRich.PrintLine(TextColor.Purple, criticalHitMessage);

        return isCriticalHit;
    }

        // ? HP Stat Formula
    // (Base * 2 + Level / 100) + Level + 10
    // Base = Stat 
    // Level = Pokémon Level
    public int GetPokémonHP(Pokémon pokémon)
    {
        Pokémon pokémonData = GetPokémon(pokémon.Name);

        return pokémonData.HP * 2 + pokémon.Level / 100 + pokémon.Level + 10;
    }

    // ? Other Stat Formula
    // (Base * 2 + Level / 100) + 5
    // Base = Stat 
    // Level = Pokémon Level
    public int GetOtherPokémonStat(Pokémon pokémon, PokémonStat pokémonStat)
    {
        Pokémon pokémonData = GetPokémon(pokémon.Name);

        int baseStatValue = pokémonStat switch 
        {
            PokémonStat.Attack => pokémonData.Attack,
            PokémonStat.Defense => pokémonData.Defense,
            PokémonStat.SpecialAttack => pokémonData.SpecialAttack,
            PokémonStat.SpecialDefense => pokémonData.SpecialDefense,
            PokémonStat.Speed => pokémonData.Speed,
            _ => pokémonData.Attack,
        };

        return baseStatValue * 2 + pokémon.Level / 100 + 5;
    }
}