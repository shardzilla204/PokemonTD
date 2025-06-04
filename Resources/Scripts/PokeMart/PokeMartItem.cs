using System;
using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class PokeMartItem : Container
{
    public PokeMartItem() { }

    public PokeMartItem(string pokeMartItemName, Dictionary<string, Variant> pokeMartItemDictionary)
    {
        Name = pokeMartItemName;
        Description = pokeMartItemDictionary["Description"].As<string>();
        Price = pokeMartItemDictionary["Price"].As<int>();

        string categoryString = pokeMartItemDictionary["Category"].As<string>();
        Category = Enum.Parse<PokeMartItemCategory>(categoryString);

        string filePath = pokeMartItemDictionary["File Path"].As<string>();
        Sprite = PokemonTD.GetSprite(filePath);
    }

    [Export]
    private TextureRect _itemSprite;

    [Export]
    private Label _itemName;

    [Export]
    private Label _itemDescription;

    [Export]
    private Label _itemPrice;

    [Export]
    private CustomButton _buyButton;

    public new string Name;
    public string Description;
    public int Price;
    public PokeMartItemCategory Category;
    public Texture2D Sprite;
    public int Quantity;

    public override void _Ready()
    {
        _buyButton.Pressed += Purchase;

        _itemSprite.Texture = Sprite;
        _itemName.Text = Name;
        _itemDescription.Text = Description;
        _itemPrice.Text = $"₽ {Price}";
    }

    private void Purchase()
    {
        if (Price > PokemonTD.PokeDollars)
        {
            Label control = GetInsufficientFundsControl();
            GetParent<Node>().GetOwnerOrNull<PokeMartInterface>().AddChild(control);
            TweenControl(control);
            return;
        }

        PokeMartItem pokeMartItem = PokeMart.Instance.Items.Find(item => item.Name == Name);
        pokeMartItem.Quantity++;

        PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.ItemReceived);
        PokemonTD.SubtractPokeDollars(Price);

        HBoxContainer spritecontrol = GetItemSpriteControl();
        GetParent<Node>().GetOwnerOrNull<PokeMartInterface>().AddChild(spritecontrol);
        TweenControl(spritecontrol);
    }

    public HBoxContainer GetItemSpriteControl()
    {
        float sizeValue = 50;
        Vector2 size = new Vector2(sizeValue, sizeValue);
        HBoxContainer hBoxContainer = new HBoxContainer()
        {
            Position = GetGlobalMousePosition() - size / 2,
            Alignment = BoxContainer.AlignmentMode.Center,
            Theme = Theme,
            MouseFilter = MouseFilterEnum.Ignore,
        };

        Label plusLabel = new Label()
        {
            Text = "+",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        plusLabel.AddThemeFontSizeOverride("font_size", 30);

        TextureRect itemSprite = new TextureRect()
        {
            Texture = Sprite,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = size,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        hBoxContainer.AddChild(plusLabel);
        hBoxContainer.AddChild(itemSprite);
        return hBoxContainer;
    }

    private Label GetInsufficientFundsControl()
    {
        Vector2 size = new Vector2(150, 50);
        Label insufficientFunds = new Label()
        {
            Text = "Insufficient Funds",
            Position = GetGlobalMousePosition() - size / 2,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Theme = Theme,
            SelfModulate = Color.FromHtml(PrintRich.GetColorHex(TextColor.Red))
        };
        insufficientFunds.AddThemeFontSizeOverride("font_size", 30);
        return insufficientFunds;
    }

    private void TweenControl(Control control)
    {
        Color transparency = Colors.White;
        transparency.A = 0;
        Vector2 targetPosition = new Vector2(control.Position.X, control.Position.Y - 15);

        float duration = 0.5f;
        Tween tween = CreateTween().SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(control, "position", targetPosition, duration);
        tween.TweenProperty(control, "modulate", transparency, duration);
        tween.Finished += control.QueueFree;
    }
}
