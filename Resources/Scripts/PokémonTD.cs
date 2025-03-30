using Godot;

using System.Collections.Generic;

namespace PokémonTD;

/*
    TODO: Create capture mechanic
    TODO: Create level up mechanic
    TODO: Create moveset mechanic
*/

/*
* Moveset
TODO: Choose between up to 4 moves
TODO: Load move from the JSON file
TODO: Check if move has advantage/disadvantage
*/

public partial class PokémonTD : Node
{
    [Export]
    private PackedScenes _packedScenes;

    [Export]
    private bool _areConsoleMessagesEnabled = false;

	[Export]
	private bool _createRandomTeam;

	[Export(PropertyHint.Range, "0,6,1")]
	private int _randomTeamCount = MaxTeamSize;

    [Export]
	private bool _areStagesEnabled = true;


    [Export]
    private bool _isCaptureModeEnabled = false;

    [Export]
    private bool _areLevelsRandomized = false;

    [Export]
    private bool _areMovesRandomized = false;

    [ExportCategory("Personal Computer")]
    [Export]
    private bool _isPersonalComputerRandomized = false;

    [Export(PropertyHint.Range, "0,30,1")]
    private int _personalComputerCount;

    [ExportCategory("Pokémon Level")]
    [Export(PropertyHint.Range, "1,100,1")]
    private int _minPokémonLevel = 1;

    [Export(PropertyHint.Range, "1,100,1")]
    private int _maxPokémonLevel = 100;

    [ExportCategory("Pokémon Enemy Level")]
    [Export(PropertyHint.Range, "1,100,1")]
    private int _minPokémonEnemyLevel = 1;

    [Export(PropertyHint.Range, "1,100,1")]
    private int _maxPokémonEnemyLevel = 100;

    public static PackedScenes PackedScenes;

    public static bool AreConsoleMessagesEnabled = false;

    public static float GameSpeed = 1;
    public static bool IsGamePaused = false;

    public static bool AreStagesEnabled = true;
    public static bool IsCaptureModeEnabled = false;
    public static bool AreLevelsRandomized = false;
    public static bool AreMovesRandomized = false;

    public static int PersonalComputerCount;
    public static bool IsPersonalComputerRandomized = false;

    public static PokémonManager PokémonManager;
    public static PokémonTypes PokémonTypes;
    public static PokémonMoveset PokémonMoveset;
    public static PokémonTeam PokémonTeam;
    public static PokémonStages PokémonStages;
    public static PersonalComputer PersonalComputer;
    
    public static Signals Signals = new Signals();

    public static int PokéDollars = 0;

    public const int MinPokémonLevel = 1;
    public const int MaxPokémonLevel = 100;

    public static int MinRandomPokémonLevel = 1;
    public static int MaxRandomPokémonLevel = 1;

    public static int MinPokémonEnemyLevel = 1;
    public static int MaxPokémonEnemyLevel = 100;

    public const int MaxTeamSize = 6;

    public const int MaxMoveCount = 4;

    public override void _EnterTree()
    {
        PackedScenes = _packedScenes;
        
        AreStagesEnabled = _areStagesEnabled;
        AreConsoleMessagesEnabled = _areConsoleMessagesEnabled;
        IsCaptureModeEnabled = _isCaptureModeEnabled;
        AreLevelsRandomized = _areLevelsRandomized;
        AreMovesRandomized = _areMovesRandomized;

        PersonalComputerCount = _personalComputerCount;
        IsPersonalComputerRandomized = _isPersonalComputerRandomized;

        MinRandomPokémonLevel = _minPokémonLevel;
        MaxPokémonEnemyLevel = _maxPokémonLevel;

        MinPokémonEnemyLevel = _minPokémonEnemyLevel;
        MaxPokémonEnemyLevel = _maxPokémonEnemyLevel;
    }

    public override void _Ready()
    {
        if (_createRandomTeam) PokémonTeam.GetRandomTeam(_randomTeamCount);

        Signals.PressedPlay += () => IsGamePaused = false;
        Signals.PressedPause += () => IsGamePaused = true;
        Signals.SpeedToggled += (speed) => GameSpeed = speed;
    }

    public static Texture2D GetPokémonSprite(string pokémonName)
    {
		string filePath = $"res://Assets/Images/Pokémon/{pokémonName}.png";
        return ResourceLoader.Load<Texture2D>(filePath);
    }

    public static Pokémon GetRandomPokémon()
    {
        return PokémonManager.GetRandomPokémon();
    }

    public static List<PokémonStage> GetPokémonStages()
    {
        return PokémonStages.GetPokémonStages();
    }

    public static PokémonStage GetPokémonStage(int pokémonStageID)
    {
        return PokémonStages.GetPokémonStage(pokémonStageID);
    }

    public static void AddPokéDollars(Pokémon pokémon)
    {  
        int minPokéDollars = pokémon.Level * 10;
        int maxPokéDollars = pokémon.Level * 15;
        RandomNumberGenerator RNG = new RandomNumberGenerator();

        PokéDollars += RNG.RandiRange(minPokéDollars, maxPokéDollars);
    }

    public static int GetRandomLevel()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        return RNG.RandiRange(MinRandomPokémonLevel, MaxRandomPokémonLevel);
    }

    public static int GetRandomLevel(int minLevel, int maxLevel)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        return RNG.RandiRange(minLevel, maxLevel);
    }

    public static List<PokémonMove> GetRandomMoveset()
	{
        List<PokémonMove> pokémonMoves = new List<PokémonMove>();
		for (int i = 0; i < MaxMoveCount; i++)
		{
			pokémonMoves.Add(PokémonMoveset.GetRandomPokémonMove());
		}
        return pokémonMoves;
	}
}