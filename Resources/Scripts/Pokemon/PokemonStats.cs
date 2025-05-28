using Godot;
using Godot.Collections;

public partial class PokemonStats : Node
{
    public PokemonStats(Dictionary<string, Variant> pokemonStats)
    { 
		HP = pokemonStats["HP"].As<int>();
		MaxHP = pokemonStats["HP"].As<int>();
		Attack = pokemonStats["Attack"].As<int>();
		Defense = pokemonStats["Defense"].As<int>();
		SpecialAttack = pokemonStats["Special Attack"].As<int>();
		SpecialDefense = pokemonStats["Special Defense"].As<int>();
		Speed = pokemonStats["Speed"].As<int>();
    }

    public int HP;
	public int MaxHP;
	public int Attack;
	public int Defense;
	public int SpecialAttack;
	public int SpecialDefense;
	public int Speed;
	public float Accuracy = 1;
	public float Evasion = 0;

}