using Godot;
using GC = Godot.Collections;
using System;

namespace PokemonTD;

/* 
    * Priority:
    TODO: Add move effect functionality

    TODO: Add Poke mart to utilize poke dollars
    TODO: Add status condition icons to show multiple conditions
    TODO: Add evolution stones & it's functionality
    TODO: Automatically apply stat increasing moves

    * Ideas:
    ? Add keybinds
    ? Shiny pokemon
    ? Mute all stage team slot options in settings

    * Bugs:

    * Notes:
    - Add HP Drain moves
    - Print Out Unique Move Usage
    - Check Focus Energy
*/

public partial class PokemonTD : Control
{
    [Export]
    private PackedScenes _packedScenes;

    [Export]
    private PokemonTween _pokemonTween;

    [Export]
    private bool _areConsoleMessagesEnabled = false;

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

    [Export]
    private bool _areFilePathsVisible = false;

    [Export]
    private bool _isExportingForMobile = false;

    [ExportCategory("Poke Center")]
    [Export]
    private bool _isPokeCenterRandomized = false;

    [Export(PropertyHint.Range, "0,120,1")]
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

    public static bool AreConsoleMessagesEnabled = false;

    public static float GameSpeed = 1;
    public static bool IsGamePaused;
    public static PokemonTween Tween;

    public static bool IsTeamRandom = false;
    public static bool AreStagesEnabled = true;
    public static bool IsCaptureModeEnabled = false;
    public static bool AreLevelsRandomized = false;
    public static bool AreMovesRandomized = false;
    public static bool IsScreenshotModeOn = false;
    public static bool AreFilePathsVisible = false;
    public static bool IsExportingForMobile = false;

    public static int StarterPokemonLevel = 5;

    public static int PokeCenterCount;
    public static bool IsPokeCenterRandomized = false;
    public static bool HasSelectedStarter = false;

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
        Tween = _pokemonTween;

        AreStagesEnabled = _areStagesEnabled;
        IsTeamRandom = _isTeamRandom;
        AreConsoleMessagesEnabled = _areConsoleMessagesEnabled;
        IsCaptureModeEnabled = _isCaptureModeEnabled;
        AreLevelsRandomized = _areLevelsRandomized;
        AreMovesRandomized = _areMovesRandomized;
        IsScreenshotModeOn = _isScreenshotModeOn;
        AreFilePathsVisible = _areFilePathsVisible;
        IsExportingForMobile = _isExportingForMobile;

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
            PokeDollars = 0;
        };

        Signals.PressedPlay += () => IsGamePaused = false;
        Signals.PressedPause += () => IsGamePaused = true;
        Signals.SpeedToggled += (speed) => GameSpeed = speed;

        Signals.EmitSignal(Signals.SignalName.GameStarted);
    }

    public static void AddPokeDollars(Pokemon pokemon)
    {
        int minimumPokeDollars = pokemon.Level * 5;
        int maximumPokeDollars = pokemon.Level * 10;
        RandomNumberGenerator RNG = new RandomNumberGenerator();

        PokeDollars += RNG.RandiRange(minimumPokeDollars, maximumPokeDollars);
    }

    public static int GetRandomLevel()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        return RNG.RandiRange(MinRandomPokemonLevel, MaxRandomPokemonLevel);
    }

    public static int GetRandomLevel(int minimumLevel, int maximumLevel)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        return RNG.RandiRange(minimumLevel, maximumLevel);
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

    public static Texture2D GetStatusIcon(StatusCondition statusCondition)
    {
        if (statusCondition == StatusCondition.None) return null;

        string statusConditionFileName = statusCondition switch
        {
            StatusCondition.BadlyPoisoned => "Poison",
            _ => statusCondition.ToString()
        };
        string filePath = $"res://Assets/Images/StatusConditionIcon/{statusConditionFileName}Icon.png";
        return ResourceLoader.Load<Texture2D>(filePath);
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
                { "Gender", (int) pokemon.Gender },
                { "Level", pokemon.Level },
                { "Experience", GetExperienceData(pokemon) },
                { "Move", pokemonMovesData[pokemonMoveIndex] },
                { "Moves", pokemonMovesData },
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
        pokemon.Gender = (Gender) pokemonData["Gender"].As<int>();

        SetExperienceData(pokemon, pokemonData);
        SetPokemonMovesData(pokemon, pokemonData);

        return pokemon;
    }

    private static GC.Dictionary<string, Variant> GetExperienceData(Pokemon pokemon)
    {
        return new GC.Dictionary<string, Variant>()
        {
            { "Minimum", pokemon.Experience.Minimum },
            { "Maximum", pokemon.Experience.Maximum },
        };
    }

    private static void SetExperienceData(Pokemon pokemon, GC.Dictionary<string, Variant> pokemonData)
    {
        GC.Dictionary<string, Variant> experienceData = pokemonData["Experience"].As<GC.Dictionary<string, Variant>>();
        pokemon.Experience.Minimum = experienceData["Minimum"].As<int>();
        pokemon.Experience.Maximum = experienceData["Maximum"].As<int>();
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
        int minimumValue = 125;
        Vector2 minimumSize = new Vector2(minimumValue, minimumValue);
        TextureRect textureRect = new TextureRect()
        {
            CustomMinimumSize = minimumSize,
            Texture = pokemon.Sprite,
            TextureFilter = TextureFilterEnum.Nearest,
            Position = -new Vector2(minimumSize.X / 2, minimumSize.Y / 4),
            PivotOffset = new Vector2(minimumSize.X / 2, 0)
        };

        Control stageDragPreview = new Control();
        stageDragPreview.AddChild(textureRect);

        return stageDragPreview;
    }

    public static GC.Dictionary<string, Variant> GetStageDragData(Pokemon pokemon, int teamSlotIndex, bool fromTeamSlot, bool isMuted)
    {
        return new GC.Dictionary<string, Variant>()
		{
			{ "TeamSlotIndex", teamSlotIndex },
			{ "FromTeamSlot", fromTeamSlot },
			{ "IsMuted", isMuted },
			{ "Pokemon", pokemon }
		};
    }
}