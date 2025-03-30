using Godot;

namespace PokémonTD;

public partial class PersonalComputerInterface : CanvasLayer
{
	[Export]
	private CustomButton _exitButton;

	public override void _Ready()
	{
		_exitButton.Pressed += () => 
		{
			StageSelectInterface stageSelectInterface = PokémonTD.PackedScenes.GetStageSelectInterface();
			AddSibling(stageSelectInterface);
			QueueFree();
		};
	}
}
