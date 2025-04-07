using Godot;

namespace PokemonTD;

public partial class InteractComponent : Control
{
	[Signal]
	public delegate void InteractedEventHandler(bool isLeftClick, bool isPressed, bool isDoubleClick);

	private bool _isHovering;

  public override void _Ready()
  {
    MouseEntered += () => _isHovering = true;
    MouseExited += () => _isHovering = false;
  }

  public override void _Input(InputEvent @event)
  {
    if (!_isHovering) return;

    if (@event is not InputEventMouseButton eventMouseButton) return;

    bool isLeftClick = eventMouseButton.ButtonIndex == MouseButton.Left ? true : false;
    bool isPressed = eventMouseButton.IsPressed();
    bool isDoubleClick = eventMouseButton.DoubleClick;

    EmitSignal(SignalName.Interacted, isLeftClick, isPressed, isDoubleClick);
  }
}
