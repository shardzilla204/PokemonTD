using Godot;
using GC = Godot.Collections;
using System;

namespace PokemonTD;

/* 
    * Tasks:
        TODO: Create tutorials for poke mart
        TODO: Reconfigure the rest of the stages

    * Ideas (Low):
        ? Shiny pokemon through color palettes
        ? Pitch variation on hovering button
        ? Add keybinds

        ? Mute all stage team slot options in settings
        ? Make master audio slider a master ball

    * Ideas (High):
        ? Show tutorial for the section (Pokemon Stage, Poke Center, and Poke Mart) and once the player sees all of the them, display the exit button
        ? Add a button to access inventory

        ? Distribute stat decrease among targets like status conditions

        ?? Figure out a way to compensate paying for fainting pokemon

    * Bugs:
        !! Pokeball will eventually not pause the game when picked up
        !? Pokemon permanently dies/freezes (!? = Possibly Fixed)

        ! Status condition is not removed from Pokemon
        ! Potential Causes: Drag and drop while the Pokemon has the Status Condition

        ! Tutorial is broken

        ! Part (1/2) When a Pokemon is hit with a stat debuff specically speed, and is hit with a status condition that works with speed e.g (Confuse or Sleep). 
        ! Part (2/2) When the timer of the stat debuff is done, it'll reset the stat without taking consideration of the status conditions it currently has.

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
    private int _startingPokeDollars = 727;

    [Export]
    private bool _isTeamRandom;

    [Export(PropertyHint.Range, "0,5,1")]
    private int _teamCount = 5;

    [Export]
    private bool _areStagesEnabled = true;

    [Export]
    private bool _isCaptureModeEnabled = false;

    [Export]
    private bool _areLevelsRandomized = false;

    [Export]
    private bool _areMovesRandomized = false;

    [Export(PropertyHint.Range, "1,100,1")]
    private int _starterPokemonLevel = 5;

    [Export]
    private bool _isScreenshotModeOn = false;


    [ExportCategory("Poke Center")]
    [Export]
    private bool _isPokeCenterRandomized = false;

    [Export(PropertyHint.Range, "0,999,1")]
    private int _pokeCenterCount;

    [ExportCategory("Pokemon Level")]
    [Export(PropertyHint.Range, "1,100,1")]
    private int _minPokemonLevel = 1;

    [Export(PropertyHint.Range, "1,100,1")]
    private int _maxPokemonLevel = 100;

    [Export(PropertyHint.Range, "1,100,1")]
    private int _minPokemonEnemyLevel = 1;

    [Export(PropertyHint.Range, "1,100,1")]
    private int _maxPokemonEnemyLevel = 100;

    public static PackedScenes PackedScenes;

    public static float GameSpeed = 1;
    public static bool IsGamePaused;
    public static PokemonTween Tween;

    public static bool IsTeamRandom = false;
    public static bool AreStagesEnabled = true;
    public static bool IsCaptureModeEnabled = false;
    public static bool AreLevelsRandomized = false;
    public static bool AreMovesRandomized = false;
    public static bool IsScreenshotModeOn = false;

    public static int StarterPokemonLevel = 5;

    public static int PokeCenterCount;
    public static bool IsPokeCenterRandomized = false;
    public static bool HasSelectedStarter = false;

    public static AudioManager AudioManager;

    public static PokemonSignals Signals = new PokemonSignals();
    public static PokemonKeybinds Keybinds;

    public static int PokeDollars = 0;

    public const int MinPokemonLevel = 1;
    public const int MaxPokemonLevel = 100;

    public static int MinRandomPokemonLevel = 1;
    public static int MaxRandomPokemonLevel = 100;

    public static int MinPokemonEnemyLevel = 1;
    public static int MaxPokemonEnemyLevel = 100;
    public static int TeamCount = 1;

    public const int MaxTeamSize = 6;
    public const int MaxMoveCount = 4;

    public override void _EnterTree()
    {
        PackedScenes = _packedScenes;
        Tween = _pokemonTween;
        Keybinds = _pokemonKeybinds;

        IsTeamRandom = _isTeamRandom;
        AreStagesEnabled = _areStagesEnabled;
        IsCaptureModeEnabled = _isCaptureModeEnabled;
        AreLevelsRandomized = _areLevelsRandomized;
        AreMovesRandomized = _areMovesRandomized;
        IsScreenshotModeOn = _isScreenshotModeOn;

        StarterPokemonLevel = _starterPokemonLevel;

        PokeCenterCount = _pokeCenterCount;
        IsPokeCenterRandomized = _isPokeCenterRandomized;

        MinRandomPokemonLevel = _minPokemonLevel;
        MaxPokemonEnemyLevel = _maxPokemonLevel;

        MinPokemonEnemyLevel = _minPokemonEnemyLevel;
        MaxPokemonEnemyLevel = _maxPokemonEnemyLevel;

        TeamCount = _teamCount;
    }

    public override void _Ready()
    {
        Signals.GameReset += () =>
        {
            HasSelectedStarter = false;
            PokeDollars = _startingPokeDollars;
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
        return RNG.RandiRange(MinRandomPokemonLevel, MaxRandomPokemonLevel);
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