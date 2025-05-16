using Godot;

namespace PokemonTD;

public partial class StatMove : Node
{
    public StatMove(string pokemonMoveName, PokemonStat pokemonStat, bool isIncreasing)
    {
        PokemonMoveName = pokemonMoveName;
        PokemonStat = pokemonStat;
        IsIncreasing = isIncreasing;
        IsSharp = false;
    }

    public StatMove(string pokemonMoveName, PokemonStat pokemonStat, bool isIncreasing, bool isSharp)
    {
        PokemonMoveName = pokemonMoveName;
        PokemonStat = pokemonStat;
        IsIncreasing = isIncreasing;
        IsSharp = isSharp;
    }

    public StatMove(string pokemonMoveName, PokemonStat pokemonStat, bool isIncreasing, int probability)
    {
        PokemonMoveName = pokemonMoveName;
        PokemonStat = pokemonStat;
        IsIncreasing = isIncreasing;
        Probability = probability;
    }

    public string PokemonMoveName;
    public PokemonStat PokemonStat;
    public bool IsIncreasing;
    public bool IsSharp = false;
    public int Probability = 255;
}