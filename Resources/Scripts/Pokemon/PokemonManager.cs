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
        LoadPokemonFile();
    }

    public override void _Ready()
    {
        PokemonTD.Signals.PokemonLeveledUp += OnPokemonLeveledUp;
    }

    private void LoadPokemonFile()
	{
        string filePath = "res://JSON/Pokemon.json";
        
		using FileAccess pokemonFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = pokemonFile.GetAsText();

		Json json = new Json();

		if (json.Parse(jsonString) != Error.Ok) return;

		_pokemonDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);

        // Print Message To Console
        string loadSuccessMessage = "Pokemon File Successfully Loaded";
        PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
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
        
        // ? Comment Out To Level Up Instantly
        pokemon.Experience.Maximum = GetExperienceRequired(pokemon);

        SetPokemonStats(pokemon);

        return pokemon;
    }

    public Pokemon GetRandomPokemon()
    {
        string randomPokemonName = GetRandomPokemonName();
        int randomLevel = GetRandomLevel();
        Pokemon pokemon = GetPokemon(randomPokemonName, randomLevel);

        return pokemon;
    }

    public int GetRandomLevel()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        return RNG.RandiRange(PokemonTD.MinRandomPokemonLevel, PokemonTD.MaxRandomPokemonLevel);
    }

    private async void OnPokemonLeveledUp(Pokemon pokemon, int teamSlotIndex, int levels)
    {
        bool canEvolve = PokemonEvolution.Instance.CanEvolve(pokemon, levels);
        if (canEvolve)
        {
            PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEvolving, pokemon, teamSlotIndex);

            await ToSignal(PokemonTD.Signals, Signals.SignalName.EvolutionFinished);
            
            pokemon = PokemonEvolution.Instance.EvolvePokemon(pokemon);

            // Update the pokemon that evolved
            PokemonTeam.Instance.Pokemon.RemoveAt(teamSlotIndex);
            PokemonTeam.Instance.Pokemon.Insert(teamSlotIndex, pokemon);

            PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEvolved, pokemon, teamSlotIndex);
            
        }

        SetPokemonStats(pokemon);

        List<PokemonMove> pokemonMoves = PokemonMoveset.Instance.GetPokemonMoves(pokemon, levels);
        foreach (PokemonMove pokemonMove in pokemonMoves)
        {
		    if (pokemonMove == null || pokemon.Moves.Contains(pokemonMove)) continue;

            PokemonMoveset.Instance.AddPokemonMove(pokemon, pokemonMove);
        }
        
        // Set level once potential moves have been added
        pokemon.Level = Mathf.Clamp(pokemon.Level + levels, 1, PokemonTD.MaxPokemonLevel);
    }

    public void SetPokemonStats(Pokemon pokemon)
    {
		pokemon.HP = GetPokemonHP(pokemon);
		pokemon.Attack = GetOtherPokemonStat(pokemon, PokemonStat.Attack);
		pokemon.Defense = GetOtherPokemonStat(pokemon, PokemonStat.Defense);
		pokemon.SpecialAttack = GetOtherPokemonStat(pokemon, PokemonStat.SpecialAttack);
		pokemon.SpecialDefense = GetOtherPokemonStat(pokemon, PokemonStat.SpecialDefense);
		pokemon.Speed = GetOtherPokemonStat(pokemon, PokemonStat.Speed);
        pokemon.Accuracy = 1;
        pokemon.Evasion = 0;
        
		// PrintRich.PrintStats(TextColor.Purple, pokemon);
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

    public bool HasPokemonMoveHit(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        try
        {
            int accuracyValue = Mathf.RoundToInt((attackingPokemon.Accuracy - defendingPokemon.Evasion) * pokemonMove.Accuracy);

            RandomNumberGenerator RNG = new RandomNumberGenerator();
            int randomThreshold = Mathf.RoundToInt(RNG.RandfRange(0, 100));
            randomThreshold -= accuracyValue;

            return randomThreshold <= 0;
        }
        catch (NullReferenceException)
        {
            return false;
        }
    }

    public int GetDamage<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        int damage = 0;
        if (attackingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = attackingPokemon as StageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            damage = GetStageSlotDamage(pokemonStageSlot, pokemonMove, pokemonEnemy);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;

            damage = GetPokemonEnemyDamage(pokemonEnemy, pokemonMove, pokemonStageSlot);
        }
        
        return damage;
    }

    // Stage Slot = Attacking
    // Pokemon Enemy = Defending
    private int GetStageSlotDamage(StageSlot pokemonStageSlot, PokemonMove pokemonMove, PokemonEnemy pokemonEnemy)
    {
        float criticalDamageMultiplier = GetCriticalDamageMultiplier(pokemonStageSlot.Pokemon, pokemonMove);
        float attackDefenseRatio = GetAttackDefenseRatio(pokemonStageSlot.Pokemon, pokemonMove, pokemonEnemy.Pokemon);
        int power = GetPower(pokemonEnemy, pokemonMove);

        float damage = (((5 * pokemonStageSlot.Pokemon.Level * criticalDamageMultiplier) + 4) * power * attackDefenseRatio) + 4;
        damage = ApplyTypeMultiplers(pokemonEnemy.Pokemon, pokemonMove, damage);
        damage = HalveDamage(pokemonStageSlot, pokemonMove, damage);

        return Mathf.RoundToInt(damage);
    }

    // Pokemon Enemy = Attacking
    // Stage Slot = Defending
    private int GetPokemonEnemyDamage(PokemonEnemy pokemonEnemy, PokemonMove pokemonMove, StageSlot pokemonStageSlot)
    {
        float criticalDamageMultiplier = GetCriticalDamageMultiplier(pokemonEnemy.Pokemon, pokemonMove) / 5;
        float attackDefenseRatio = GetAttackDefenseRatio(pokemonEnemy.Pokemon, pokemonMove, pokemonStageSlot.Pokemon);
        int power = GetPower(pokemonStageSlot, pokemonMove);

        float damage = (((5 * pokemonEnemy.Pokemon.Level * criticalDamageMultiplier) + 2) * power * attackDefenseRatio) + 2;
        damage = ApplyTypeMultiplers(pokemonStageSlot.Pokemon, pokemonMove, damage);
        damage = HalveDamage(pokemonStageSlot, pokemonMove, damage);

        return Mathf.RoundToInt(damage);
    }

    // Earthquake Doubles It's Power If Opposing Pokemon Used Dig
    private int GetPower<Defending>(Defending defendingPokemon, PokemonMove pokemonMove)
    {
        int power = pokemonMove.Power;
        if (pokemonMove.Name != "Earthquake") return power;

        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.UsedDig) power *= 2;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.UsedDig) power *= 2;
        }
        return power;
    }

    private float ApplyTypeMultiplers(Pokemon defendingPokemon, PokemonMove pokemonMove, float damage)
    {
        List<float> typeMultipliers = PokemonTypes.Instance.GetTypeMultipliers(pokemonMove.Type, defendingPokemon.Types);
        foreach (float typeMultiplier in typeMultipliers)
        {
            damage *= typeMultiplier;
        }
        return damage;
    }

    // Light Screen & Reflect Pokemon Moves Halve Damage 
    private float HalveDamage<Defending>(Defending defendingPokemon, PokemonMove pokemonMove, float damage)
    {
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.LightScreenCount > 0 && pokemonMove.Category == MoveCategory.Special)
            {
                damage /= 2;
            }
            else if (pokemonStageSlot.ReflectCount > 0 && pokemonMove.Category == MoveCategory.Physical)
            {
                damage /= 2;
            }
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.LightScreenCount > 0 && pokemonMove.Category == MoveCategory.Special)
            {
                damage /= 2;
            }
            else if (pokemonEnemy.ReflectCount > 0 && pokemonMove.Category == MoveCategory.Physical)
            {
                damage /= 2;
            }
        }
        return damage;
    }

    private float GetAttackDefenseRatio(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        float specialRatio = (float)attackingPokemon.SpecialAttack / defendingPokemon.SpecialDefense;
        float normalRatio = (float)attackingPokemon.Attack / defendingPokemon.Defense;

        float attackDefenseRatio = pokemonMove.Category == MoveCategory.Special ? specialRatio : normalRatio;

        return (float) Math.Round(attackDefenseRatio, 2) / 50;
    }
    
    private float GetCriticalDamageMultiplier(Pokemon pokemon, PokemonMove pokemonMove)
    {
        return IsCriticalHit(pokemon, pokemonMove) ? 2 : 1;
    }

    private bool IsCriticalHit(Pokemon pokemon, PokemonMove pokemonMove)
    {
        float criticalHitRatio = PokemonMoveEffect.Instance.GetCriticalHitRatio(pokemon, pokemonMove);
        int maxThreshold = 255;

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int criticalValue = Mathf.RoundToInt(RNG.RandfRange(criticalHitRatio, maxThreshold));
        int randomThreshold = Mathf.RoundToInt(RNG.RandfRange(0, maxThreshold));
        randomThreshold -= criticalValue;

        bool isCriticalHit = randomThreshold <= 0;

		if (isCriticalHit) 
        {
            // Print Message To Console
            string criticalHitMessage = $"{pokemon.Name} Has Landed A Critical Hit";
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
        Pokemon pokemonData = GetPokemon(pokemon.Name);
        return pokemonData.HP * 2 + pokemon.Level / 100 + pokemon.Level + 10;
    }

    // ? Other Stat Formula
    // (Base * 2 + Level / 100) + 5
    // Base = Stat 
    // Level = Pokemon Level
    public int GetOtherPokemonStat(Pokemon pokemon, PokemonStat pokemonStat)
    {
        Pokemon pokemonData = GetPokemon(pokemon.Name);

        int baseStatValue = pokemonStat switch 
        {
            PokemonStat.Attack => pokemonData.Attack,
            PokemonStat.Defense => pokemonData.Defense,
            PokemonStat.SpecialAttack => pokemonData.SpecialAttack,
            PokemonStat.SpecialDefense => pokemonData.SpecialDefense,
            PokemonStat.Speed => pokemonData.Speed,
            PokemonStat.Accuracy => 1,
            PokemonStat.Evasion => 0,
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