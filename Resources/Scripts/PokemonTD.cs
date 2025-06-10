using Godot;
using GC = Godot.Collections;
using System;

namespace PokemonTD;

/* 
    * Tasks:

    * Ideas (Low):
        ? Shiny pokemon through color palettes
        ? Pitch variation on hovering button
        ? Add keybinds

    * Ideas (High):
        ? Show tutorial for the section (Pokemon Stage, Poke Center, and Poke Mart) and once the player sees all of the them, display the exit button
        
        ?? Figure out a way to compensate paying for fainting pokemon

    * Bugs:

    * Notes:
        - Pin Missile SFX Will Be Damage SFX
        - Growl SFX Is The Pokemon's Cry
        - Haunter will evolve to Gengar by LVL 34
        - Kadabra will evolve to Alakazam by LVL 33
        - Machoke will evolve to Machamp by LVL 36
        - Graveler will evolve to Golem by LVL 33
        - Pokemon evolves first then learns potential moves
        - No Pokemon learns surf on leveling up
*/

public partial class PokemonTD : Control
{
    [Export]
    private PackedScenes _packedScenes;

    [Export]
    private PokemonTween _pokemonTween;

    [Export]
    private PokemonKeybinds _pokemonKeybinds;

    [Export]
    private PokemonMasterMode _pokemonMasterMode;

    [Export]
    private PokemonDebug _pokemonDebug;

    [Export]
    private int _startingPokeDollars = 727;

    [Export]
    private GC.Dictionary<string, int> _startingInventory = new GC.Dictionary<string, int>()
    {
        { "Potion", 5 },
        { "Rare Candy", 1 },
        { "Super Potion", 1 },
    };

    [Export(PropertyHint.Range, "1,100,1")]
    private int _starterPokemonLevel = 5;

    [Export]
    private bool _isScreenshotModeOn = false;

    public static PackedScenes PackedScenes;

    public static float GameSpeed = 1;
    public static bool IsGamePaused;
    
    public static bool HasSelectedStarter = false;

    public static AudioManager AudioManager;

    public static PokemonTween Tween;
    public static PokemonKeybinds Keybinds;
    public static PokemonMasterMode MasterMode;
    public static PokemonDebug Debug;
    public static PokemonSignals Signals = new PokemonSignals();

    public static int PokeDollars = 727;
    public static GC.Dictionary<string, int> StartingInventory = new GC.Dictionary<string, int>();
    public static int StarterPokemonLevel = 5;
    public static bool IsScreenshotModeOn = false;

    public const int MinPokemonLevel = 1;
    public const int MaxPokemonLevel = 100;

    public const int MaxTeamSize = 6;
    public const int MaxMoveCount = 4;

    public override void _EnterTree()
    {
        PackedScenes = _packedScenes;
        Tween = _pokemonTween;
        Keybinds = _pokemonKeybinds;
        MasterMode = _pokemonMasterMode;
        Debug = _pokemonDebug;

        PokeDollars = _startingPokeDollars;
        StartingInventory = _startingInventory;
        StarterPokemonLevel = _starterPokemonLevel;
        IsScreenshotModeOn = _isScreenshotModeOn;
    }

    public override void _Ready()
    {
        Signals.GameReset += () =>
        {
            HasSelectedStarter = false;
            PokeDollars = _startingPokeDollars;
            PokeMart.Instance.AddStartingItems();
        };

        Signals.PressedPlay += () => IsGamePaused = false;
        Signals.PressedPause += () => IsGamePaused = true;
        Signals.SpeedToggled += (speed) => GameSpeed = speed;

        Signals.EmitSignal(PokemonSignals.SignalName.GameStarted);
    }

    public static void AddPokeDollars(Pokemon pokemon)
    {
        int minPokeDollars = pokemon.Level * 5;
        int maxPokeDollars = pokemon.Level * 10;
        RandomNumberGenerator RNG = new RandomNumberGenerator();

        int amount = RNG.RandiRange(minPokeDollars, maxPokeDollars);
        PokeDollars += amount;
        Signals.EmitSignal(PokemonSignals.SignalName.PokeDollarsUpdated);
    }

    public static void AddPokeDollars(int amount)
    {
        PokeDollars += amount;
        Signals.EmitSignal(PokemonSignals.SignalName.PokeDollarsUpdated);
    }

    public static void SubtractPokeDollars(Pokemon pokemon)
    {
        int minPokeDollars = pokemon.Level;
        int maxPokeDollars = pokemon.Level * 3;
        RandomNumberGenerator RNG = new RandomNumberGenerator();

        int amount = RNG.RandiRange(minPokeDollars, maxPokeDollars);
        PokeDollars = Mathf.Max(0, PokeDollars - amount);
        Signals.EmitSignal(PokemonSignals.SignalName.PokeDollarsUpdated);
    }

    public static void SubtractPokeDollars(int amount)
    {
        PokeDollars = Mathf.Max(0, PokeDollars - amount);
        Signals.EmitSignal(PokemonSignals.SignalName.PokeDollarsUpdated);
    }

    public static int GetRandomLevel()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        return RNG.RandiRange(Debug.MinPokemonLevel, Debug.MaxPokemonLevel);
    }

    public static int GetRandomLevel(int minimumLevel, int MaxLevel)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        return RNG.RandiRange(minimumLevel, MaxLevel);
    }

    public static Texture2D GetGenderSprite(Pokemon pokemon)
    {
        string filePath = $"res://Assets/Images/Gender/{pokemon.Gender}Icon.png";
        return GetSprite(filePath);
    }

    public static Texture2D GetStatIcon(PokemonStat stat)
    {
        if (stat == PokemonStat.Accuracy || stat == PokemonStat.Evasion) return null;

        string statFileName = stat.ToString();
        string filePath = $"res://Assets/Images/StatIcon/{statFileName}Icon.png";
        return GetSprite(filePath);
    }

    public static Texture2D GetStatusIcon(StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None) return null;

        string statusConditionFileName = statusCondition switch
        {
            StatusCondition.BadlyPoisoned => "Poison",
            _ => statusCondition.ToString()
        };
        string filePath = $"res://Assets/Images/StatusConditionIcon/{statusConditionFileName}Icon.png";
        return GetSprite(filePath);
    }

    public static GC.Dictionary<string, Variant> GetPokemonData(Pokemon pokemon)
    {
        try
        {
            if (pokemon == null) throw new NullReferenceException("Pokemon is null");

            int pokemonMoveIndex = pokemon.Moves.IndexOf(pokemon.Move);
            GC.Dictionary<int, string> pokemonMovesData = GetPokemonMovesData(pokemon);

            GC.Dictionary<string, Variant> pokemonData = new GC.Dictionary<string, Variant>()
            {
                { "Name", pokemon.Name },
                { "HP", pokemon.Stats.HP },
                { "Gender", (int) pokemon.Gender },
                { "Level", pokemon.Level },
                { "Experience", GetExperienceData(pokemon) },
                { "Move", pokemonMovesData[pokemonMoveIndex] },
                { "Moves", pokemonMovesData },
                { "Has Canceled Evolution", pokemon.HasCanceledEvolution}
            };
            return pokemonData;
        }
        catch (NullReferenceException)
        {
            return new GC.Dictionary<string, Variant>();
        }
    }

    public static Pokemon SetPokemonData(string pokemonName, GC.Dictionary<string, Variant> pokemonData)
    {
        int pokemonLevel = pokemonData["Level"].As<int>();
        Pokemon pokemon = PokemonManager.Instance.GetPokemon(pokemonName, pokemonLevel);
        pokemon.Stats.HP = pokemonData["HP"].As<int>();
        pokemon.Gender = (Gender)pokemonData["Gender"].As<int>();
        pokemon.HasCanceledEvolution = pokemonData["Has Canceled Evolution"].As<bool>();

        SetExperienceData(pokemon, pokemonData);
        SetPokemonMovesData(pokemon, pokemonData);

        return pokemon;
    }

    private static GC.Dictionary<string, Variant> GetExperienceData(Pokemon pokemon)
    {
        return new GC.Dictionary<string, Variant>()
        {
            { "Min", pokemon.Experience.Min },
            { "Max", pokemon.Experience.Max },
        };
    }

    private static void SetExperienceData(Pokemon pokemon, GC.Dictionary<string, Variant> pokemonData)
    {
        GC.Dictionary<string, Variant> experienceData = pokemonData["Experience"].As<GC.Dictionary<string, Variant>>();
        pokemon.Experience.Min = experienceData["Min"].As<int>();
        pokemon.Experience.Max = experienceData["Max"].As<int>();
    }

    private static GC.Dictionary<int, string> GetPokemonMovesData(Pokemon pokemon)
    {
        GC.Dictionary<int, string> pokemonMovesData = new GC.Dictionary<int, string>();
        for (int i = 0; i < pokemon.Moves.Count; i++)
        {
            pokemonMovesData.Add(i, pokemon.Moves[i].Name);
        }
        return pokemonMovesData;
    }

    private static void SetPokemonMovesData(Pokemon pokemon, GC.Dictionary<string, Variant> pokemonData)
    {
        pokemon.Moves.Clear();
        GC.Dictionary<int, string> pokemonMovesData = pokemonData["Moves"].As<GC.Dictionary<int, string>>();
        foreach (string pokemonMoveName in pokemonMovesData.Values)
        {
            PokemonMove pokemonMove = PokemonMoves.Instance.FindPokemonMove(pokemonMoveName);
            pokemon.Moves.Add(pokemonMove);
        }
        string selectedPokemonMoveName = pokemonData["Move"].As<string>();
        PokemonMove selectedPokemonMove = pokemon.Moves.Find(pokemonMove => pokemonMove.Name == selectedPokemonMoveName);
        pokemon.Move = selectedPokemonMove;
    }

    public static Control GetStageDragPreview(Pokemon pokemon)
    {
        int minValue = 125;
        Vector2 minSize = new Vector2(minValue, minValue);
        TextureRect textureRect = new TextureRect()
        {
            CustomMinimumSize = minSize,
            Texture = pokemon.Sprite,
            TextureFilter = TextureFilterEnum.Nearest,
            Position = -new Vector2(minSize.X / 2, minSize.Y / 4),
            PivotOffset = new Vector2(minSize.X / 2, 0)
        };

        Control stageDragPreview = new Control();
        stageDragPreview.AddChild(textureRect);

        return stageDragPreview;
    }

    public static Texture2D GetSprite(string filePath)
    {
        return ResourceLoader.Load<Texture2D>(filePath);
    }
}