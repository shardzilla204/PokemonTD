using Godot;

namespace PokémonTD;

public enum MoveCategory
{
	Physical,
	Special,
	Status
}

public partial class PokémonMove : Node
{
	public new string Name;
	public PokémonType Type;
	public MoveCategory Category;
	public int Power;
	public int Accuracy;
	public int PP;
	public string Effect;
}
