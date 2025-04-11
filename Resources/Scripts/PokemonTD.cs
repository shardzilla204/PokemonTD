using Godot;

using System.Collections.Generic;

namespace PokemonTD;

/* General
    TODO: Create capture mechanic | Done
    TODO: Create level up mechanic
    TODO: Create moveset mechanic
    TODO: Fix experience error
    TODO: Set moves not selected when instatiating pokemon to old/forgotten 
*/

/*
/* Moveset
    TODO: Choose between up to 4 moves | Done
    TODO: Load move from the JSON file | Done
    TODO: Check if move has advantage/disadvantage | Done
    TODO: If learning another move above the move count, show swap interface 
*/

public partial class PokemonTD : Node
{
    [Export]
    private PackedScenes _packedScenes;

    [Export]
    private bool _areConsoleMessagesEnabled = false;

	[Export]
	private bool _isTeamRandom;

	[Export(PropertyHint.Range, "0,5,1")]
	private int _teamCount = MaxTeamSize;

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

    [ExportCategory("Personal Computer")]
    [Export]
    private bool _isPokeCenterRandomized = false;

    [Export(PropertyHint.Range, "0,60,1")]
    private int _pokeCenterCount;

    [ExportCategory("Pokemon Level")]
    [Export(PropertyHint.Range, "1,100,1")]
    private int _minPokemonLevel = 1;

    [Export(PropertyHint.Range, "1,100,1")]
    private int _maxPokemonLevel = 100;

    [ExportCategory("Pokemon Enemy Level")]
    [Export(PropertyHint.Range, "1,100,1")]
    private int _minPokemonEnemyLevel = 1;

    [Export(PropertyHint.Range, "1,100,1")]
    private int _maxPokemonEnemyLevel = 100;

    public static PackedScenes PackedScenes;

    public static bool AreConsoleMessagesEnabled = false;

    public static float GameSpeed = 1;
    public static bool IsGamePaused = false;

    public static bool IsTeamRandom = false;
    public static bool AreStagesEnabled = true;
    public static bool IsCaptureModeEnabled = false;
    public static bool AreLevelsRandomized = false;
    public static bool AreMovesRandomized = false;

    public static int StarterPokemonLevel = 5;

    public static int PokeCenterCount;
    public static bool IsPokeCenterRandomized = false;

    public static PokemonManager PokemonManager;
    public static PokemonTypes PokemonTypes;
    public static PokemonMoves PokemonMoves;
    public static PokemonMoveset PokemonMoveset;
    public static PokemonTeam PokemonTeam;
    public static PokemonStages PokemonStages;
    public static PokeCenter PokeCenter;
    
    public static Signals Signals = new Signals();

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
        
        AreStagesEnabled = _areStagesEnabled;
        IsTeamRandom = _isTeamRandom;
        AreConsoleMessagesEnabled = _areConsoleMessagesEnabled;
        IsCaptureModeEnabled = _isCaptureModeEnabled;
        AreLevelsRandomized = _areLevelsRandomized;
        AreMovesRandomized = _areMovesRandomized;

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
        Signals.PressedPlay += () => IsGamePaused = false;
        Signals.PressedPause += () => IsGamePaused = true;
        Signals.SpeedToggled += (speed) => GameSpeed = speed;
        Signals.ForgetMove += (pokemon, pokemonMove) => IsGamePaused = true;
    }

    public static void AddPokeDollars(Pokemon pokemon)
    {  
        int minPokeDollars = pokemon.Level * 10;
        int maxPokeDollars = pokemon.Level * 15;
        RandomNumberGenerator RNG = new RandomNumberGenerator();

        PokeDollars += RNG.RandiRange(minPokeDollars, maxPokeDollars);
    }

    public static int GetRandomLevel()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        return RNG.RandiRange(MinRandomPokemonLevel, MaxRandomPokemonLevel);
    }

    public static int GetRandomLevel(int minLevel, int maxLevel)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        return RNG.RandiRange(minLevel, maxLevel);
    }
}