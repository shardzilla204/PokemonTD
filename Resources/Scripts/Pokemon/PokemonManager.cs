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

    public List<string> NidoranFemaleStrings = new List<string>() { "Nidoran♀", "Nidorina", "Nidoqueen" };
    public List<string> NidoranMaleStrings = new List<string>() { "Nidoran♂", "Nidorino", "Nidoking" };

    public override void _EnterTree()
    {
        Instance = this;
        LoadPokemonFile();
    }

    public override void _Ready()
    {
        PokemonTD.Signals.PokemonLeveledUp += PokemonLeveledUp;
    }

    private void LoadPokemonFile()
    {
        string filePath = "res://JSON/Pokemon.json";

        using FileAccess pokemonFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        string jsonString = pokemonFile.GetAsText();

        Json json = new Json();

        if (json.Parse(jsonString) != Error.Ok) return;

        _pokemonDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary)json.Data);

        // Print Message To Console
        string loadSuccessMessage = "Pokemon File Successfully Loaded";
        PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
    }

    private Pokemon GetPokemon(string pokemonName)
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

    private async void PokemonLeveledUp(Pokemon pokemon, int teamSlotIndex, int levels)
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
        GC.Array<string> pokemonDictionaryKeys = (GC.Array<string>)_pokemonDictionaries.Keys;
        return pokemonDictionaryKeys[randomValue];
    }

    // ? HP Stat Formula
    // (Base * 2 + Level / 100) + Level + 10
    // Base = Stat 
    // Level = Pokemon Level
    public int GetPokemonHP(Pokemon pokemon)
    {
        Pokemon pokemonData = GetPokemon(pokemon.Name);
        int hpStatValue = Mathf.RoundToInt(pokemonData.HP * 1.35f + pokemon.Level / 100 + pokemon.Level);
        return Mathf.Clamp(hpStatValue, 0, 255);
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

        int pokemonStatValue = Mathf.RoundToInt(baseStatValue + pokemon.Level / 100);
        return Mathf.Clamp(pokemonStatValue, 0, 255);
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

    public void ChangeTypes(Pokemon attackingPokemon, Pokemon defendingPokemon)
    {
        attackingPokemon.Types.Clear();
        attackingPokemon.Types.AddRange(defendingPokemon.Types);
    }
}