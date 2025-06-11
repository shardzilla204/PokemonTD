using Godot;
using System;

namespace PokemonTD;

public partial class BuyButton : CustomButton
{
    [Signal]
    public delegate void AmountChangedEventHandler(int amount);

    private int _minAmount = 1;
    private int _maxAmount = 10;
    public int Amount = 1;
    

    private bool _isControlPressed = false;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventKey eventKey && eventKey.Keycode == Key.Ctrl)
        {
            _isControlPressed = eventKey.Pressed;
        }

        if (!_isControlPressed) return;

        if (!IsHovering || @event is not InputEventMouseButton eventMouseButton) return;
		if (!eventMouseButton.Pressed) return;

        if (eventMouseButton.ButtonIndex == MouseButton.WheelUp)
        {
            Amount++;
            Amount = Math.Min(Amount, _maxAmount);
            EmitSignal(SignalName.AmountChanged, Amount);
            SetText();
        }
        else if (eventMouseButton.ButtonIndex == MouseButton.WheelDown)
        {
            Amount--;
            Amount = Math.Max(Amount, _minAmount);

            EmitSignal(SignalName.AmountChanged, Amount);
            SetText();
        }
    }

    private void SetText()
    {
        Text = Amount > 1 ? $"+{Amount}" : "+";
    }
}
