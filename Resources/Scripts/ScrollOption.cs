using Godot;

public partial class ScrollOption : NinePatchRect
{
    [Signal]
    public delegate void ValueChangedEventHandler(int value);

    [Export]
    private Label _label;

    [Export]
    private TextureProgressBar _textureProgressBar;

    public int Value;

    private bool _isHovering = false;
    private bool _isPressingShift = false;

    public override void _Ready()
    {
        Value = (int) _textureProgressBar.Value;
        MouseEntered += () => _isHovering = true;
        MouseExited += () => _isHovering = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isHovering) return;

        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Keycode == Key.Shift) _isPressingShift = eventKey.Pressed;
        }

        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
        {
            int value = 0;
            if (eventMouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                value = 1;
            }
            else if (eventMouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                value = -1;
            }
            value *= _isPressingShift ? 5 : 1;
            _textureProgressBar.Value += value;

            EmitSignal(SignalName.ValueChanged, _textureProgressBar.Value);
        }
    }

    public void SetText(string text)
    {
        _label.Text = text;
    }

    public void SetProgress(int progress)
    {
        _textureProgressBar.Value = progress;
        Value = progress;
    }

    public void SetMaxValue(int maxValue)
    {
        _textureProgressBar.MaxValue = maxValue;
    }
}
