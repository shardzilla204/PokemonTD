using Godot;

namespace PokemonTD;

public partial class PokemonKeybinds : Node
{
    [Signal]
    public delegate void ChangePokemonMoveEventHandler(int pokemonTeamIndex);

    [Signal]
    public delegate void GameStateEventHandler();

    [Signal]
    public delegate void GameSpeedEventHandler(bool isIncreased);

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey eventKey) return;

        Key keycode = eventKey.Keycode;

        if (eventKey.IsActionPressed("ChangePokemonMove"))
        {
            string keycodeString = keycode.ToString();
            int pokemonTeamIndex = int.Parse(keycodeString.Replace("Key", "")) - 1; // Buttons are 1 - 6. Index starts at 0
            EmitSignal(SignalName.ChangePokemonMove, pokemonTeamIndex);
        }
        else if (eventKey.IsActionPressed("GameState"))
        {
            EmitSignal(SignalName.GameState);
        }
        else if (eventKey.IsActionPressed("GameSpeed"))
        {
            bool isSpeedIncreased = keycode == Key.E;
            EmitSignal(SignalName.GameSpeed, isSpeedIncreased);
        }
    }
}