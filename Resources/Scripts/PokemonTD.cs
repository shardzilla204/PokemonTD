using Godot;

namespace PokemonTD;

/* 
   TODO: Highlight Pokemon's stage slot when trying to drag Pokemon's team slot, if already out
   TODO: Add status effect functionality
   TODO: Add move effect functionality
   TODO: Add save and load functionality
   TODO: Add settings button while in pokemon stage
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
    public static bool IsGamePaused = true;

    public static bool IsTeamRandom = false;
    public static bool AreStagesEnabled = true;
    public static bool IsCaptureModeEnabled = false;
    public static bool AreLevelsRandomized = false;
    public static bool AreMovesRandomized = false;

    public static int StarterPokemonLevel = 5;

    public static int PokeCenterCount;
    public static bool IsPokeCenterRandomized = false;
    public static bool HasSelectedStarter = false;

    public static PokemonManager PokemonManager;
    public static PokemonTypes PokemonTypes;
    public static PokemonMoves PokemonMoves;
    public static PokemonMoveset PokemonMoveset;
    public static PokemonTeam PokemonTeam;
    public static PokemonEvolution PokemonEvolution;
    public static PokemonStages PokemonStages;
    public static PokeCenter PokeCenter;
    public static AudioManager AudioManager;
    
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

    public static StageConsole StageConsole;

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

        Signals.EmitSignal(Signals.SignalName.GameStarted);
    }

    public static void AddPokeDollars(Pokemon pokemon)
    {  
        int minPokeDollars = pokemon.Level * 10;
        int maxPokeDollars = pokemon.Level * 20;
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

    public static void AddStageConsoleMessage(TextColor textColor, string text)
    {
        if (StageConsole == null) return;

        StageConsoleLabel stageConsoleLabel = PackedScenes.GetStageConsoleLabel();
        stageConsoleLabel.Text = text;
        stageConsoleLabel.SelfModulate = Color.FromHtml(PrintRich.GetColorHex(textColor));

        StageConsole.AddMessage(stageConsoleLabel);
    }

    public static Texture2D GetGenderSprite(Pokemon pokemon)
	{
		string filePath = $"res://Assets/Images/Gender/{pokemon.Gender}Icon.png";
		return ResourceLoader.Load<Texture2D>(filePath);
	}
}