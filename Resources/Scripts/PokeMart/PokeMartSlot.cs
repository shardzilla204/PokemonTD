using Godot;

namespace PokemonTD;

public partial class PokeMartSlot : NinePatchRect
{
    [Signal]
    public delegate void UsedEventHandler();

	[Export]
	private TextureRect _itemSprite;

	[Export]
	private Label _itemQuantity;

	public PokeMartItem PokeMartItem;
    private bool _isDragging;

    public override void _Notification(int what)
    {
        if (what != NotificationDragEnd || !_isDragging) return;

        _isDragging = false;
        EmitSignal(SignalName.Used);
    }


	public void SetPokeMartItem(PokeMartItem pokeMartItem)
    {
        _itemSprite.Texture = pokeMartItem.Sprite;
        _itemQuantity.Text = $"{pokeMartItem.Quantity}x";
    }

	public override Variant _GetDragData(Vector2 atPosition)
	{
        _isDragging = true;

        Control dragPreview = GetDragPreview();
        SetDragPreview(dragPreview);
        
        PokeMartItem pokeMartItem = PokeMart.Instance.Items.Find(item => item.Name == PokeMartItem.Name);
        return pokeMartItem;  
	}

	public Control GetDragPreview()
	{
        Control control = new Control();

        if (PokeMartItem is null) return control;

        int minimumSize = 45;
        Vector2 size = new Vector2(minimumSize, minimumSize);
        TextureRect textureRect = new TextureRect()
        {
            Texture = PokeMartItem.Sprite,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Position = -size / 2,
            Size = size
        };
        control.AddChild(textureRect);

        return control;
	}
}
