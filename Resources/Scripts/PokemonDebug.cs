using Godot;

namespace PokemonTD;

public partial class PokemonDebug : Node
{
    [Export]
    private bool _areStagesEnabled = true;

    [Export]
    private bool _isTeamRandom;

    [Export(PropertyHint.Range, "0,5,1")]
    private int _teamCount = 5;

    [Export]
    private bool _isCaptureModeEnabled = false;

    [Export]
    private bool _areLevelsRandomized = false;

    [Export]
    private bool _areMovesRandomized = false;

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

    public bool IsTeamRandom = false;
    public bool AreStagesEnabled = true;
    public bool IsCaptureModeEnabled = false;
    public bool AreLevelsRandomized = false;
    public bool AreMovesRandomized = false;
    public bool IsScreenshotModeOn = false;

    public int PokeCenterCount;
    public bool IsPokeCenterRandomized = false;

    public int MinPokemonLevel = 1;
    public int MaxPokemonLevel = 100;

    public int MinPokemonEnemyLevel = 1;
    public int MaxPokemonEnemyLevel = 100;

    public int TeamCount = 1;

    public override void _EnterTree()
    {
        IsTeamRandom = _isTeamRandom;
        AreStagesEnabled = _areStagesEnabled;
        IsCaptureModeEnabled = _isCaptureModeEnabled;
        AreLevelsRandomized = _areLevelsRandomized;
        AreMovesRandomized = _areMovesRandomized;

        PokeCenterCount = _pokeCenterCount;
        IsPokeCenterRandomized = _isPokeCenterRandomized;

        MinPokemonLevel = _minPokemonLevel;
        MaxPokemonLevel = _maxPokemonLevel;

        MinPokemonEnemyLevel = _minPokemonEnemyLevel;
        MaxPokemonEnemyLevel = _maxPokemonEnemyLevel;

        TeamCount = _teamCount;
    }
}