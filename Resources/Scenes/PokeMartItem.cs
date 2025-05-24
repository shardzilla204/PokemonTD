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
        Price = pokeMartItemDictionary["Price"].As<int>();

        string categoryString = pokeMartItemDictionary["Category"].As<string>();
        Category = Enum.Parse<PokeMartItemCategory>(categoryString);

        string filePath = pokeMartItemDictionary["File Path"].As<string>();
        Sprite = PokemonTD.GetSprite(filePath);
    }

    [Signal]
    public delegate void BoughtEventHandler();

    [Signal]
    public delegate void UsedEventHandler();

    [Export]
    private TextureRect _itemSprite;

    [Export]
    private Label _itemName;

    [Export]
    private Label _itemPrice;

    [Export]
    private CustomButton _buyButton;

    public new string Name;
    public PokeMartItemCategory Category;
    public int Price;
    public Texture2D Sprite;
    public int Quantity;

    public override void _Ready()
    {
        _buyButton.Pressed += Purchase;

        _itemSprite.Texture = Sprite;
        _itemName.Text = $"{Name}";
        _itemPrice.Text = $"₽ {Price}";
    }

    private void Purchase()
    {
        if (Price > PokemonTD.PokeDollars) return;

        PokeMartItem pokeMartItem = PokeMart.Instance.Items.Find(item => item.Name == Name);
        pokeMartItem.Quantity++;

        EmitSignal(SignalName.Bought);
        PokemonTD.SubtractPokeDollars(Price);
    }
}
