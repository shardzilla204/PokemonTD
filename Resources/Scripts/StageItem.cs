using Godot;

namespace PokemonTD;

public partial class StageItem : Control
{
    [Export]
    private TextureRect _itemSprite;

    [Export]
    private Label _itemQuantity;

    private PokeMartItem _pokeMartItem;
    private bool _isDragging = false;

    public override void _Notification(int what)
    {
        if (what != NotificationDragEnd || !_isDragging) return;

        _isDragging = false;
        SetItem(_pokeMartItem); // Refresh quantity
    }


    public override Variant _GetDragData(Vector2 atPosition)
    {
        if (_pokeMartItem.Quantity == 0) return new Control();

        _isDragging = true;

        Control dragPreview = GetDragPreview();
        SetDragPreview(dragPreview);

        return _pokeMartItem;
    }

    private Control GetDragPreview()
    {
        int minValue = 65;
        Vector2 minSize = new Vector2(minValue, minValue);
        TextureRect textureRect = new TextureRect()
        {
            CustomMinimumSize = minSize,
            Texture = _pokeMartItem.Sprite,
            TextureFilter = TextureFilterEnum.Nearest,
            Position = -new Vector2(minSize.X / 2, minSize.Y / 2),
            PivotOffset = new Vector2(minSize.X / 2, minSize.Y / 2)
        };

        Control dragPreview = new Control();
        dragPreview.AddChild(textureRect);

        return dragPreview;
    }

    public void SetItem(PokeMartItem pokeMartItem)
    {
        _pokeMartItem = pokeMartItem;

        _itemSprite.Texture = pokeMartItem == null ? null : pokeMartItem.Sprite;
        _itemQuantity.Text = pokeMartItem == null ? "0x" : $"{pokeMartItem.Quantity}x";

        if (pokeMartItem == null) return;

        Color color = Colors.White;
        if (pokeMartItem.Quantity == 0)
        {
            Modulate = color.Darkened(0.25f);
        }
        else
        {
            Modulate = color;
        }
    }
}
