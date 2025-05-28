using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class CopyMoves : Node
{
    private List<string> _copyMoves = new List<string>()
    {
        "Mimic",
        "Mirror Move"
    };

    public bool IsCopyMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _copyMoves.Find(pokemonMoveName => pokemonMoveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public PokemonMove GetCopiedPokemonMove(Pokemon attackingPokemon, Pokemon defendingPokemon)
    {
        PokemonMove copiedPokemonMove = defendingPokemon.Move;

        string copiedMoveMessage = $"{attackingPokemon.Name} Has Copied {defendingPokemon.Name}'s Move And Used {copiedPokemonMove.Name}";
        PrintRich.PrintLine(TextColor.Orange, copiedMoveMessage);

        return copiedPokemonMove;
    }
}
