using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;

namespace PokemonTD;

public enum MoveCategory
{
	Physical,
	Special,
	Status
}

public partial class PokemonMove : Node
{
	public new string Name;
	public PokemonType Type;
	public MoveCategory Category;
	public int Power;
	public int Accuracy;
	public string Effect;
	public List<int> HitCount = new List<int>();
	public GC.Dictionary<string, float> StatusCondition = new GC.Dictionary<string, float>();
}
