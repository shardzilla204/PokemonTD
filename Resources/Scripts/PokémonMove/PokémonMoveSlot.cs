using Godot;

namespace PokémonTD;

public partial class MoveSlot : NinePatchRect
{
	[Export]
	private Label _move;

	public void UpdateSlot(string moveName, PokémonType moveType)
	{
		_move.Text = moveName;
		SelfModulate = PokémonTD.PokémonTypes.GetTypeColor(moveType);
	}

	public PokémonMove GetMove()
	{
		return PokémonTD.PokémonMoveset.GetPokémonMove(_move.Text);
	}
}
