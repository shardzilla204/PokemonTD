using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GC = Godot.Collections;

namespace PokémonTD;

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

public partial class PokémonMoveset : Node
{
    private GC.Dictionary<string, Variant> _typeMovesetsDictionary = new GC.Dictionary<string, Variant>();
    public List<PokémonMove> PokémonMoves = new List<PokémonMove>();

    public override void _EnterTree()
	{
		PokémonTD.PokémonMoveset = this;
	}

	public override void _Ready()
	{
        LoadTypeMovesetDictionary();
        SetTypeMoves();
	}

    private void LoadTypeMovesetDictionary()
	{
        PokémonType[] typeArray = Enum.GetValues<PokémonType>();

        int totalMoveCount = 0;
        foreach (PokémonType type in typeArray)
        {
            totalMoveCount += AddTypeMoveset(type);
        }
        GD.Print();

        string typeCountMessage = $"Total Pokémon Types: {typeArray.Length}";
        PrintRich.PrintLine(TextColor.Blue, typeCountMessage);

        string totalCountMessage = $"Total Move Count: {totalMoveCount}";
        PrintRich.PrintLine(TextColor.Blue, totalCountMessage);
	}

    private int AddTypeMoveset(PokémonType type)
    {
		string filePath = $"res://JSON/Moveset/{type}Moveset.json";

		using FileAccess typeMovesetFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = typeMovesetFile.GetAsText();

        Json json = new Json();

		if (json.Parse(jsonString) != Error.Ok) return 0;

        GC.Dictionary<string, Variant> typeMovesetDictionary = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);

        string typeCountAddedMessage = $"Total {type} Count: {typeMovesetDictionary.Count}";
		PrintRich.Print(TextColor.Blue, typeCountAddedMessage);

		_typeMovesetsDictionary.Add(type.ToString(), typeMovesetDictionary);

        return typeMovesetDictionary.Count;
    }

    private void SetTypeMoves()
    {
        List<string> typeNames = _typeMovesetsDictionary.Keys.ToList();
        foreach (string typeName in typeNames)
        {
            GC.Dictionary<string, Variant> typeMovesetDictionary = (GC.Dictionary<string, Variant>) _typeMovesetsDictionary[typeName];
            SetTypeMoveset(typeMovesetDictionary);
        }
    }

    private void SetTypeMoveset(GC.Dictionary<string, Variant> typeMovesetDictionary)
    {
        List<string> moveNames = typeMovesetDictionary.Keys.ToList();
        foreach (string moveName in moveNames)
        {
            GC.Dictionary<string, Variant> movesetDictionary = (GC.Dictionary<string, Variant>) typeMovesetDictionary[moveName];
            AddPokémonMove(movesetDictionary, moveName);
        }
    }

    private void AddPokémonMove(GC.Dictionary<string, Variant> typeMovesetDictionary, string moveName)
    {
        PokémonMove pokémonMove = new PokémonMove()
        {
            Name = moveName,
            Type = Enum.Parse<PokémonType>(typeMovesetDictionary["Type"].As<string>()),
            Category = Enum.Parse<MoveCategory>(typeMovesetDictionary["Category"].As<string>()),
            Power = typeMovesetDictionary["Power"].As<int>(),
            Accuracy = typeMovesetDictionary["Accuracy"].As<int>(),
            PP = typeMovesetDictionary["PP"].As<int>(),
            Effect = typeMovesetDictionary["Effect"].As<string>()
        };
        PokémonMoves.Add(pokémonMove);
    }

    public PokémonMove GetRandomPokémonMove()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomValue = RNG.RandiRange(0, PokémonMoves.Count - 1);
        return PokémonMoves[randomValue];
    }

    public PokémonMove GetPokémonMove(string moveName)
    {
        return PokémonMoves.Find(pokémonMove => pokémonMove.Name == moveName);
    }
}