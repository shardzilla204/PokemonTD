using Godot;

namespace PokémonTD;

public partial class StageStateInterface : CanvasLayer
{
	[Export]
	private CustomButton _retryButton;

	[Export]
	private CustomButton _leaveButton;

	[Export]
	private Label _message;

	public Label Message => _message;

	public override void _Ready()
	{
		_retryButton.Pressed += ShowStageSelection;
		_leaveButton.Pressed += ShowStageSelection;
	}

	private void ShowStageSelection()
	{
		QueueFree();
	}
}
