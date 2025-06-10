using Godot;
using GC = Godot.Collections;
using System;

namespace PokemonTD;

public partial class StagePath : Path2D
{
	[Export]
	private Texture2D _rightArrowTexture;

	private GC.Array<PathFollow2D> _pathFollows = new GC.Array<PathFollow2D>();
	private PathFollow2D _arrowPathFollow;

	public override void _Ready()
	{
		PokemonStage pokemonStage = GetParentOrNull<PokemonStage>();
		if (!pokemonStage.HasStarted && !PokemonTD.IsScreenshotModeOn)
		{
			TextureRect arrow = GetArrow();
			_arrowPathFollow = GetPathFollow(true, true, false);
			pokemonStage.StartedWave += _arrowPathFollow.QueueFree;
			_arrowPathFollow.TreeExiting += () =>
			{
				pokemonStage.StartedWave -= _arrowPathFollow.QueueFree;
			};
			_arrowPathFollow.AddChild(arrow);
			AddPathFollow(_arrowPathFollow);
		}
	}

	public override void _Process(double delta)
	{
		PokemonStage pokemonStage = GetParentOrNull<PokemonStage>();

		if (!pokemonStage.HasStarted) MoveArrow(delta);

		bool hasStageFinished = pokemonStage.HasFinished;
		if (!PokemonTD.AreStagesEnabled || hasStageFinished || PokemonTD.IsGamePaused) return;

		foreach (PathFollow2D pathFollow in GetChildren())
		{
			MovePokemonEnemy(pathFollow, delta);
		}
	}

	private async void MovePokemonEnemy(PathFollow2D pathFollow, double delta)
	{
		PokemonEnemy pokemonEnemy = pathFollow.GetChildOrNull<PokemonEnemy>(0);

		if (!IsInstanceValid(pokemonEnemy)) return;
		
		float speedMultiplier = 1.35f;
		double progressSpeed = pokemonEnemy.Pokemon.Stats.Speed * PokemonTD.GameSpeed * delta * speedMultiplier;

		pathFollow.Progress += (float) progressSpeed;

		Vector2 previousPosition = pokemonEnemy.GlobalPosition;
		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);

		if (!IsInstanceValid(pokemonEnemy)) return;

		Vector2 direction = previousPosition.DirectionTo(pokemonEnemy.GlobalPosition);
		bool isMovingRight = Math.Round(direction.X, 1) >= -0.05f;
		pokemonEnemy.FlipH = isMovingRight;
	}

	private void MoveArrow(double delta)
	{
		float speed = 1500;
		_arrowPathFollow.Progress += (float) delta * speed;
	}

	public void AddPathFollow(PathFollow2D pathFollow)
	{
		_pathFollows.Add(pathFollow);
		pathFollow.TreeExiting += () => _pathFollows.Remove(pathFollow);
		AddChild(pathFollow);
	}

	public void RemovePathFollow(PokemonEnemy pokemonEnemy)
	{
		PathFollow2D pathFollow = pokemonEnemy.GetParentOrNull<PathFollow2D>();
		pathFollow.QueueFree();
	}

	public PathFollow2D GetPathFollow(bool isRotating, bool isLooping, bool isClone)
	{
		PathFollow2D pathFollow = new PathFollow2D()
		{
			Rotates = isRotating,
			Loop = isLooping,
			YSortEnabled = true
		};
		if (!isClone) pathFollow.TreeExiting += () => _pathFollows.Remove(pathFollow);

		return pathFollow;
	}

	private TextureRect GetArrow()
	{
		float minSize = 30;
		TextureRect arrow = new TextureRect()
		{
			Texture = _rightArrowTexture,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			CustomMinimumSize = new Vector2(minSize, minSize),
			YSortEnabled = true,
			Position = new Vector2(-minSize, -minSize) / 2
		};
		return arrow;
	}
}
