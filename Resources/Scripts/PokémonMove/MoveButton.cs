using Godot;

namespace PokémonTD;

public partial class MoveButton : CustomButton
{
	[Export]
	private Label _stats;

	[Export]
	private TextureRect _typeIcon;

	public PokémonMove PokémonMove;

	public override void _Ready()
	{
		if (PokémonMove is null) return;

		_stats.Text = $"{PokémonMove.Name}";
		_typeIcon.Texture = PokémonTD.PokémonTypes.GetTypeIcon(PokémonMove.Type);
	}
}
