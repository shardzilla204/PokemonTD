using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        PokemonTD.Signals.PokemonEvolved += PokemonEvolved;
        PokemonTD.Signals.PokemonForgettingMove += PokemonForgettingMove;
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
        if (PrintRich.AreFileMessagesEnabled) PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
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
        pokemon.Experience.Max = GetExperienceRequired(pokemon);

        SetPokemonStats(pokemon);
        pokemon.HP = GetPokemonHP(pokemon);
        pokemon.MaxHP = GetPokemonHP(pokemon);
        pokemon.Accuracy = 1;
        pokemon.Evasion = 0;
        return pokemon;
    }
    
    public async Task<Pokemon> PokemonEvolving(Pokemon pokemon, EvolutionStone evolutionStone, int teamSlotIndex)
    {
        string pokemonEvolutionName = PokemonEvolution.Instance.GetPokemonEvolutionName(pokemon, evolutionStone);
        Pokemon pokemonEvolution = PokemonEvolution.Instance.GetPokemonEvolution(pokemon, pokemonEvolutionName);
        EvolutionInterface evolutionInterface = PokemonTD.PackedScenes.GetEvolutionInterface(pokemon, pokemonEvolution, teamSlotIndex);

        Pokemon evolution = new Pokemon();
        evolutionInterface.Finished += (evolutionResult) =>
        {
            evolution = evolutionResult;
            if (!PokemonEvolution.Instance.IsQueueEmpty()) PokemonEvolution.Instance.ShowNext();
        };
        PokemonEvolution.Instance.AddToQueue(evolutionInterface);

        await ToSignal(evolutionInterface, EvolutionInterface.SignalName.Finished);

        return evolution;
    }

    private async void PokemonForgettingMove(Pokemon pokemon, PokemonMove pokemonMove)
    {
        if (!PokemonEvolution.Instance.IsQueueEmpty()) await ToSignal(PokemonEvolution.Instance, PokemonEvolution.SignalName.QueueCleared);

        ForgetMoveInterface forgetMoveInterface = PokemonTD.PackedScenes.GetForgetMoveInterface(pokemon, pokemonMove);
        forgetMoveInterface.Finished += () =>
        {
            if (!PokemonMoves.Instance.IsQueueEmpty()) PokemonMoves.Instance.ShowNext();
        };

        PokemonMoves.Instance.AddToQueue(forgetMoveInterface);
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

    private async void PokemonLeveledUp(int levels, int teamSlotIndex)
    {
        Pokemon pokemon = PokemonTeam.Instance.Pokemon[teamSlotIndex];
        bool canEvolve = PokemonEvolution.Instance.CanEvolve(pokemon, levels);
        if (canEvolve && !pokemon.HasCanceledEvolution)
        {
            Pokemon pokemonResult = await PokemonEvolving(pokemon, EvolutionStone.None, teamSlotIndex);
            if (pokemonResult != pokemon) pokemon = PokemonEvolution.Instance.EvolvePokemon(pokemon);
        }

        List<PokemonMove> pokemonMoves = PokemonMoveset.Instance.GetPokemonMoves(pokemon, levels);
        foreach (PokemonMove pokemonMove in pokemonMoves)
        {
            if (pokemonMove == null || pokemon.Moves.Contains(pokemonMove)) continue;
            PokemonMoveset.Instance.AddPokemonMove(pokemon, pokemonMove);
        }

        // Set level once potential moves and potential evolution have been added
        pokemon.IncreaseLevel(levels);

        if (canEvolve) PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEvolved, pokemon, teamSlotIndex);
    }

    private void PokemonEvolved(Pokemon pokemonEvolution, int teamSlotIndex)
    {
        // Update the pokemon that evolved
        PokemonTeam.Instance.Pokemon.RemoveAt(teamSlotIndex);
        PokemonTeam.Instance.Pokemon.Insert(teamSlotIndex, pokemonEvolution);
    }

    public Texture2D GetPokemonSprite(string pokemonName)
    {
        string filePath = $"res://Assets/Images/Pokemon/{pokemonName}.png";
        return ResourceLoader.Load<Texture2D>(filePath);
    }

    public Gender GetRandomGender()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomValue = RNG.RandiRange((int) Gender.Male, (int) Gender.Female);

        return (Gender) randomValue;
    }

    public void SetPokemonStats(Pokemon pokemon)
    {
        pokemon.Attack = GetOtherPokemonStat(pokemon, PokemonStat.Attack);
        pokemon.Defense = GetOtherPokemonStat(pokemon, PokemonStat.Defense);
        pokemon.SpecialAttack = GetOtherPokemonStat(pokemon, PokemonStat.SpecialAttack);
        pokemon.SpecialDefense = GetOtherPokemonStat(pokemon, PokemonStat.SpecialDefense);
        pokemon.Speed = GetOtherPokemonStat(pokemon, PokemonStat.Speed);
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
        return Mathf.Max(0, hpStatValue);
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
        return Mathf.Max(0, pokemonStatValue);
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
        string attackingTypesString = PrintRich.GetTypesString(attackingPokemon);
        string defendingTypesString = PrintRich.GetTypesString(defendingPokemon);
        string typesChangedMessage = $"{attackingPokemon.Name} Has Changed It's Types {attackingTypesString} Into {defendingTypesString}";
        PrintRich.PrintLine(TextColor.Orange, typesChangedMessage);

        attackingPokemon.Types.Clear();
        attackingPokemon.Types.AddRange(defendingPokemon.Types);
    }

    public void ChangePokemon(Pokemon attackingPokemon, Pokemon defendingPokemon)
    {
        attackingPokemon.Sprite = defendingPokemon.Sprite;
        attackingPokemon.Species = defendingPokemon.Species;

        attackingPokemon.Moves.Clear();
        attackingPokemon.Moves.AddRange(defendingPokemon.Moves);
        attackingPokemon.Move = defendingPokemon.Move;

        attackingPokemon.Attack = defendingPokemon.Attack;
        attackingPokemon.SpecialAttack = defendingPokemon.SpecialAttack;
        attackingPokemon.Defense = defendingPokemon.Defense;
        attackingPokemon.SpecialDefense = defendingPokemon.SpecialDefense;
        attackingPokemon.Speed = defendingPokemon.Speed;

        attackingPokemon.Height = defendingPokemon.Height;
        attackingPokemon.Weight = defendingPokemon.Weight;

        ChangeTypes(attackingPokemon, defendingPokemon);

        string transformedMessage = $"{attackingPokemon.Name} Has Transformed Into {defendingPokemon.Name}";
        PrintRich.PrintLine(TextColor.Orange, transformedMessage);
    }

    public Pokemon GetPokemonCopy(Pokemon pokemon)
    {
        Pokemon pokemonCopy = new Pokemon();

        pokemonCopy.Sprite = pokemon.Sprite;
        pokemonCopy.Species = pokemon.Species;

        pokemonCopy.Moves.Clear();
        pokemonCopy.Moves.AddRange(pokemon.Moves);
        pokemonCopy.Move = pokemon.Move;

        pokemonCopy.Attack = pokemon.Attack;
        pokemonCopy.SpecialAttack = pokemon.SpecialAttack;
        pokemonCopy.Defense = pokemon.Defense;
        pokemonCopy.SpecialDefense = pokemon.SpecialDefense;
        pokemonCopy.Speed = pokemon.Speed;

        pokemonCopy.Height = pokemon.Height;
        pokemonCopy.Weight = pokemon.Weight;
        
        return pokemonCopy;
    }
}