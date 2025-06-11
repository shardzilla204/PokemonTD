using Godot;

namespace PokemonTD;

public partial class PokemonTween : Control
{
    public override void _EnterTree()
    {
		PokemonTD.Tween = this;
    }

	public void TweenSlotDragRotation(Control dragPreview, bool isDragging)
	{
		if (!isDragging || dragPreview is null) return;

		Vector2 initialPosition = dragPreview.GlobalPosition;
		Vector2 finalPosition = GetGlobalMousePosition();

		TextureRect pokemonTexture = dragPreview.GetChildOrNull<TextureRect>(0);
		Vector2 direction = finalPosition.DirectionTo(initialPosition);
		FlipPokemonTexture(pokemonTexture, direction);

		float duration = 0.5f;
		float rotation = GetRotation(initialPosition, finalPosition, direction);
		Tween tween = CreateTween();
		tween.TweenProperty(dragPreview, "rotation", rotation, duration);
	}

	private float GetRotation(Vector2 initialPosition, Vector2 finalPosition, Vector2 direction)
	{
		float distance = initialPosition.DistanceTo(finalPosition) * 100;
		int distanceSensitivity = 25;
		float rotationValue = 0;
		float rotationIncrease = 20f;
		while (distance > 0)
		{
			distance -= distanceSensitivity;
			rotationValue += rotationIncrease;
		}

		float degrees = direction.X > 0 ? -rotationValue : rotationValue;
		degrees = direction.X == 0 ? 0 : degrees;
		return Mathf.DegToRad(degrees);
	}

	private void FlipPokemonTexture(TextureRect pokemonTexture, Vector2 direction)
	{
		int directionX = Mathf.RoundToInt(direction.X);
		if (directionX == -1)
		{
			pokemonTexture.FlipH = true;
		}
		else if (directionX == 1)
		{
			pokemonTexture.FlipH = false;
		}
	}

	public void TweenAttack(TextureRect pokemonSprite, PokemonEnemy pokemonEnemy, Timer attackTimer)
	{
		Vector2 direction = pokemonSprite.GlobalPosition.DirectionTo(pokemonEnemy.GlobalPosition);
		pokemonSprite.FlipH = direction.X > 0;

		Vector2 positionOne = pokemonSprite.Position - (direction * 5); // Moves the sprite backwards
		Vector2 positionTwo = pokemonSprite.Position + (direction * 15); // Moves the sprite forwards

		float speedMultiplier = 0.25f;
		Vector2 originalPosition = pokemonSprite.Position;
		Tween tween = CreateTween().SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(pokemonSprite, "position", positionOne, attackTimer.WaitTime * speedMultiplier);
		tween.TweenProperty(pokemonSprite, "position", positionTwo, attackTimer.WaitTime * speedMultiplier);
		tween.TweenProperty(pokemonSprite, "position", originalPosition, attackTimer.WaitTime * speedMultiplier);
	}
}