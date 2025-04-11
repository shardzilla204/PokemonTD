using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GC = Godot.Collections;

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
    private GC.Dictionary<string, Variant> _typeMovesetsDictionary = new GC.Dictionary<string, Variant>();
    public List<PokemonMove> Moves = new List<PokemonMove>();

    public override void _EnterTree()
	{
		PokemonTD.PokemonMoves = this;
	}

	public override void _Ready()
	{
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
        GD.Print();

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
            AddPokemonMove(movesetDictionary, moveName);
        }
    }

    private void AddPokemonMove(GC.Dictionary<string, Variant> typeMovesetDictionary, string moveName)
    {
        PokemonMove PokemonMove = new PokemonMove()
        {
            Name = moveName,
            Type = Enum.Parse<PokemonType>(typeMovesetDictionary["Type"].As<string>()),
            Category = Enum.Parse<MoveCategory>(typeMovesetDictionary["Category"].As<string>()),
            Power = typeMovesetDictionary["Power"].As<int>(),
            Accuracy = typeMovesetDictionary["Accuracy"].As<int>(),
            PP = typeMovesetDictionary["PP"].As<int>(),
            Effect = typeMovesetDictionary["Effect"].As<string>()
        };
        Moves.Add(PokemonMove);
    }

    public PokemonMove GetRandomPokemonMove()
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomValue = RNG.RandiRange(0, Moves.Count - 1);
        return Moves[randomValue];
    }

    public PokemonMove GetPokemonMove(string pokemonMoveName)
    {
        return Moves.Find(pokemonMove => pokemonMove.Name == pokemonMoveName);
    }
}