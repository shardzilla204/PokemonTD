using Godot;

namespace PokemonTD;

public partial class PokeCenterInterface : CanvasLayer
{
	[Export]
	private CustomButton _exitButton;

	public override void _Ready()
	{
		_exitButton.Pressed += () => 
		{
			if (PokemonTD.PokemonTeam.Pokemon.Count == 0) return;

			StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
			AddSibling(stageSelectInterface);
			QueueFree();
		};
	}
}
