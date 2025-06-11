using Godot;
using Godot.Collections;
using System;

namespace PokemonTD;

public partial class PokeMartSlot : NinePatchRect
{
    [Signal]
    public delegate void UsedEventHandler();

	[Export]
	private TextureRect _itemSprite;

	[Export]
	private Label _itemQuantity;

	private PokeMartItem _pokeMartItem;

    private bool _isDragging;
    private Control _dragPreview;
    private Dictionary<string, Variant> _dataDictionary = new Dictionary<string, Variant>();
    private int _amount = 1;

    public override void _Input(InputEvent @event)
    {
        if (_pokeMartItem.Category != PokeMartItemCategory.Candy || _dragPreview == null) return;

        if (@event is not InputEventMouseButton eventMouseButton) return;

        if (!eventMouseButton.Pressed) return;

        if (eventMouseButton.ButtonIndex == MouseButton.WheelUp)
        {
            _amount++;
            _amount = Math.Min(_amount, _pokeMartItem.Quantity);
        }
        else if (eventMouseButton.ButtonIndex == MouseButton.WheelDown)
        {
            _amount--;
            _amount = Math.Max(_amount, 1);
        }

        Label dragPreviewLabel = _dragPreview.GetChildOrNull<Label>(1);
        dragPreviewLabel.Text = _amount > 1 ? $"x{_amount}" : "";
        _dataDictionary["Amount"] = _amount;
    }

    public override void _Notification(int what)
    {
        if (what != NotificationDragEnd || !_isDragging) return;

        _isDragging = false;
        EmitSignal(SignalName.Used);
    }

	public void SetPokeMartItem(PokeMartItem pokeMartItem)
    {
        _pokeMartItem = pokeMartItem;

        _itemSprite.Texture = pokeMartItem.Sprite;
        _itemQuantity.Text = $"{pokeMartItem.Quantity}x";
    }

	public override Variant _GetDragData(Vector2 atPosition)
	{
        _isDragging = true;

        _dragPreview = GetDragPreview();
        SetDragPreview(_dragPreview);

        _dataDictionary = new Dictionary<string, Variant>()
        {
            { "PokeMartItem", _pokeMartItem },
            { "Amount", _amount }
        };
        
        return _dataDictionary;  
	}

	public Control GetDragPreview()
	{
        Control control = new Control();

        if (_pokeMartItem is null) return control;

        int minSize = 45;
        Vector2 size = new Vector2(minSize, minSize);
        TextureRect textureRect = new TextureRect()
        {
            Texture = _pokeMartItem.Sprite,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Position = -size / 2,
            CustomMinimumSize = size
        };

        Label label = new Label()
        {
            Text = _amount > 1 ? $"x{_amount}" : "",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Position = -size / 2,
            CustomMinimumSize = size,
            SizeFlagsVertical = SizeFlags.Fill
        };
        label.AddThemeFontSizeOverride("font_size", 20);

        control.AddChild(textureRect);
        control.AddChild(label);

        return control;
	}
}
