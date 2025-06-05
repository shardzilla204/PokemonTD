using Godot;

namespace PokemonTD;

public partial class GameSettings : Container
{
    [Export]
    private CustomButton _saveButton;

    [Export]
    private CustomButton _loadButton;

    [Export]
    private CustomButton _deleteButton;

    public override void _Ready()
    {
        _saveButton.Pressed += () => PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.GameSaved);
		_loadButton.Pressed += () => PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.GameLoaded);
		_deleteButton.Pressed += () => PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.GameReset);
    }
}
