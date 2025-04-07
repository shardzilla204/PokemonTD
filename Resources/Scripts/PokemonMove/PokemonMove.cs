using Godot;

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
	public int PP;
	public string Effect;
}
