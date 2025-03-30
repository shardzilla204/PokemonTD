using Godot;

namespace PokémonTD;

public partial class PokéBall : TextureRect
{
    public override Variant _GetDragData(Vector2 atPosition)
    {
		SetDragPreview(GetDragPreview());
        return this;
    }

	private Control GetDragPreview()
	{
		const int MinValue = 64;
		Vector2 minSize = new Vector2(MinValue, MinValue);
		Color color = Colors.White;
		color.A = 0.65f;
		TextureRect textureRect = new TextureRect()
		{
			CustomMinimumSize = minSize,
			Texture = Texture,
			TextureFilter = TextureFilterEnum.Nearest,
			SelfModulate = color,
			Position =  -minSize / 2
		};

		Control control = new Control();
		control.AddChild(textureRect);

		return control;
	}
}
