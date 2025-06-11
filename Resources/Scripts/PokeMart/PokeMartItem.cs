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
    private BuyButton _buyButton;

    public new string Name;
    public string Description;
    public int Price;
    public PokeMartItemCategory Category;
    public Texture2D Sprite;
    public int Quantity;

    public int TargetPrice;

    public override void _Ready()
    {
        TargetPrice = Price;

        _buyButton.Pressed += Purchase;
        _buyButton.AmountChanged += AmountChanged;

        SetItem();
    }

    private void Purchase()
    {
        if (TargetPrice > PokemonTD.PokeDollars)
        {
            Label control = GetInsufficientFundsControl();
            GetParent<Node>().GetOwnerOrNull<PokeMartInterface>().AddChild(control);
            TweenControl(control);
            return;
        }

        PokeMartItem pokeMartItem = PokeMart.Instance.Items.Find(item => item.Name == Name);
        pokeMartItem.Quantity += _buyButton.Amount;

        PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.ItemReceived);
        PokemonTD.SubtractPokeDollars(TargetPrice);

        HBoxContainer spriteControl = GetItemSpriteControl();
        GetParent<Node>().GetOwnerOrNull<PokeMartInterface>().AddChild(spriteControl);
        TweenControl(spriteControl);
    }

    private void AmountChanged(int amount)
    {
        TargetPrice = Price * amount;
        SetItem();
    }

    private void SetItem()
    {
        _itemSprite.Texture = Sprite;
        _itemName.Text = Name;
        _itemDescription.Text = Description;
        _itemPrice.Text = $"₽ {TargetPrice}";
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

        Label itemAmount = new Label()
        {
            Text = _buyButton.Amount > 1 ? $"x{_buyButton.Amount}" : "",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            CustomMinimumSize = size,
            SizeFlagsVertical = SizeFlags.Fill
        };
        
        itemSprite.AddChild(itemAmount);

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
