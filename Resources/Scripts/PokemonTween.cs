using Godot;

namespace PokemonTD;

public partial class PokemonTween : Node
{
    public void TweenSlotDragRotation(Control dragPreview, Vector2 initialPosition, Vector2 finalPosition, bool isDragging)
    {
		if (!isDragging || dragPreview is null) return;

		Vector2 direction = finalPosition.DirectionTo(initialPosition);
		float distance = initialPosition.DistanceTo(finalPosition) * 100;
		float rotationValue = 0;
		int distanceSensitivity = 25;
		float rotationIncrease = 20f;
		while (distance > 0)
		{
			distance -= distanceSensitivity;
			rotationValue += rotationIncrease;
		}

		TextureRect pokemonTexture = dragPreview.GetChildOrNull<TextureRect>(0);

		float degrees = direction.X > 0 ? -rotationValue : rotationValue;
		degrees = direction.X == 0 ? 0 : degrees;
		
		if (direction.X != 0) pokemonTexture.FlipH = direction.X < 0;

		float rotation = Mathf.DegToRad(degrees);

		float duration = 0.5f;
		Tween tween = CreateTween();
		tween.TweenProperty(dragPreview, "rotation", rotation, duration);
    }
}