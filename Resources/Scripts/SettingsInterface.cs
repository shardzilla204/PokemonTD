using Godot;

namespace PokemonTD;

public partial class SettingsInterface : CanvasLayer
{
	[Export]
	private CustomButton _exitButton;

	[Export]
	private CustomButton _informationButton;

	[Export]
	private Container _gameSettings;

	public bool FromMainMenu;

    public override void _Ready()
    {
		_exitButton.Pressed += () => 
		{
			if (FromMainMenu)
			{
				MenuInterface menuInterface = PokemonTD.PackedScenes.GetMenuInterface();
				AddSibling(menuInterface);
			}
			else
			{
				PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PressedPlay);
			}
			QueueFree();
		};
		
		_informationButton.Pressed += () => 
		{
			InformationInterface informationInterface = PokemonTD.PackedScenes.GetInformationInterface();
			informationInterface.FromMainMenu = FromMainMenu;
			AddSibling(informationInterface);
			QueueFree();
		};
		
		_gameSettings.Visible = FromMainMenu;
    }
}
