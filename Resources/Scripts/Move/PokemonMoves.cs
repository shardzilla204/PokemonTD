using Godot;
using GC = Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

/* Moves that can't be used by all pokemon
- Struggle: All moves can be used infinitely.
- 
*/

/* Moves that can't be used by pokemon enemies
! - All attacking moves [Barrage, Bind, Body Slam,]
! - All moves that reduce stats [Swift, ]
- Conversion: Can't attack back. No point in changing type.
- Metronome: Move could be an attack.
*/

/* Moves that can't be used by the player's pokemon
! - All moves that inflict damage on itself [Bide, Double-Edge, Explosion]
- Disable: Enemies can't attack back.
? - Hyper Beam: Recharge mechanic needed.
*/

public partial class PokemonMoves : Node
{
    private static PokemonMoves _instance;
    public static PokemonMoves Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

    [Signal]
    public delegate void QueueClearedEventHandler();

    private GC.Dictionary<string, Variant> _typeMovesetsDictionary = new GC.Dictionary<string, Variant>();
    private List<ForgetMoveInterface> _forgetMoveQueue = new List<ForgetMoveInterface>();

    public List<PokemonMove> Moves = new List<PokemonMove>();

    public override void _EnterTree()
    {
        Instance = this;

        LoadTypeMovesetDictionary();
        SetTypeMoves();
    }

    private void LoadTypeMovesetDictionary()
    {
        PokemonType[] typeArray = Enum.GetValues<PokemonType>();

        int totalMoveCount = 0;
        foreach (PokemonType type in typeArray)
        {
            totalMoveCount += AddTypeMoveset(type);
        }
        GD.Print(); // Print Spacing

        // Print Messages To Console
        string typeCountMessage = $"Total Pokemon Types: {typeArray.Length}";
        PrintRich.PrintLine(TextColor.Blue, typeCountMessage);

        string totalCountMessage = $"Total Move Count: {totalMoveCount}";
        PrintRich.PrintLine(TextColor.Blue, totalCountMessage);
    }

    private int AddTypeMoveset(PokemonType type)
    {
        string filePath = $"res://JSON/Moveset/{type}Moveset.json";

        using FileAccess typeMovesetFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        string jsonString = typeMovesetFile.GetAsText();

        Json json = new Json();

        if (json.Parse(jsonString) != Error.Ok) return 0;

        GC.Dictionary<string, Variant> typeMovesetDictionary = new GC.Dictionary<string, Variant>((GC.Dictionary)json.Data);

        _typeMovesetsDictionary.Add(type.ToString(), typeMovesetDictionary);

        // Print Messages To Console
        string typeCountAddedMessage = $"Total {type} Count: {typeMovesetDictionary.Count}";
        PrintRich.Print(TextColor.Blue, typeCountAddedMessage);

        return typeMovesetDictionary.Count;
    }

    private void SetTypeMoves()
    {
        List<string> typeNames = _typeMovesetsDictionary.Keys.ToList();
        foreach (string typeName in typeNames)
        {
            GC.Dictionary<string, Variant> typeMovesetDictionary = (GC.Dictionary<string, Variant>)_typeMovesetsDictionary[typeName];
            SetTypeMoveset(typeMovesetDictionary);
        }
    }

    private void SetTypeMoveset(GC.Dictionary<string, Variant> typeMovesetDictionary)
    {
        List<string> moveNames = typeMovesetDictionary.Keys.ToList();
        foreach (string moveName in moveNames)
        {
            GC.Dictionary<string, Variant> movesetDictionary = (GC.Dictionary<string, Variant>)typeMovesetDictionary[moveName];
            AddPokemonMove(movesetDictionary, moveName);
        }
    }

    private void AddPokemonMove(GC.Dictionary<string, Variant> typeMovesetDictionary, string moveName)
    {
        PokemonMove pokemonMove = new PokemonMove()
        {
            Name = moveName,
            Type = Enum.Parse<PokemonType>(typeMovesetDictionary["Type"].As<string>()),
            Category = Enum.Parse<MoveCategory>(typeMovesetDictionary["Category"].As<string>()),
            Power = typeMovesetDictionary["Power"].As<int>(),
            Accuracy = typeMovesetDictionary["Accuracy"].As<int>(),
            Effect = typeMovesetDictionary["Effect"].As<string>(),
            HitCount = typeMovesetDictionary["Hit Count"].As<GC.Array<int>>().ToList(),
            StatusCondition = typeMovesetDictionary["Status Condition"].As<GC.Dictionary<string, float>>(),
        };
        Moves.Add(pokemonMove);
    }

    public PokemonMove GetRandomPokemonMove()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomValue = RNG.RandiRange(0, Moves.Count - 1);
        return Moves[randomValue];
    }

    public PokemonMove FindPokemonMove(string pokemonMoveName)
    {
        return Moves.Find(pokemonMove => pokemonMove.Name == pokemonMoveName);
    }

    public void AddToQueue(ForgetMoveInterface forgetMoveInterface)
    {
        if (_forgetMoveQueue.Count == 0)
        {
            Node parentNode = GetParentOrNull<Node>();
            parentNode.AddChild(forgetMoveInterface);
            parentNode.MoveChild(forgetMoveInterface, GetChildCount() - 1);
        }

        _forgetMoveQueue.Add(forgetMoveInterface);
    }

    public void RemoveFromQueue(ForgetMoveInterface forgetMoveInterface)
    {
        _forgetMoveQueue.Remove(forgetMoveInterface);
    }

    public void ShowNext()
    {
        ForgetMoveInterface nextForgetMoveInterface = _forgetMoveQueue[0];

        Node parentNode = GetParentOrNull<Node>();
        parentNode.AddChild(nextForgetMoveInterface);
        parentNode.MoveChild(nextForgetMoveInterface, GetChildCount() - 1);
    }

    public bool IsQueueEmpty()
    {
        if (_forgetMoveQueue.Count != 0) return false;

        EmitSignal(SignalName.QueueCleared);
        return true;
    }

    public bool IsAutomaticMove(PokemonMove pokemonMove)
    {
        bool hasIncreasingStatChanges = PokemonStatMoves.Instance.HasIncreasingStatChanges(pokemonMove) &&
        pokemonMove.Name != "Skull Bash" &&
        pokemonMove.Name != "Rage";
        bool isAutomaticMove = hasIncreasingStatChanges ||
            pokemonMove.Name == "Focus Energy" ||
            pokemonMove.Name == "Light Screen" ||
            pokemonMove.Name == "Reflect" ||
            pokemonMove.Name == "Conversion";
        return isAutomaticMove;
    }
}