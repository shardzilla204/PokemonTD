using Godot;
using GC = Godot.Collections;
using System;

namespace PokemonTD;

public partial class StagePath : Path2D
{
    private GC.Array<PathFollow2D> _pathFollows = new GC.Array<PathFollow2D>();

    public override void _Process(double delta)
	{
        bool hasStageFinished = GetParentOrNull<PokemonStage>().HasFinished;
		if (!PokemonTD.AreStagesEnabled || hasStageFinished || PokemonTD.IsGamePaused) return;

		foreach (PathFollow2D pathFollow in GetChildren())
		{
			MovePokemonEnemy(pathFollow, delta);
		}
	}

    private void MovePokemonEnemy(PathFollow2D pathFollow, double delta)
	{
		PokemonEnemy pokemonEnemy = pathFollow.GetChildOrNull<PokemonEnemy>(0);
		Vector2 previousPosition = pokemonEnemy.GlobalPosition;
		double progressSpeed = (pokemonEnemy.Pokemon.Speed / 2) * PokemonTD.GameSpeed * delta;
		
		pathFollow.Progress += (float) progressSpeed;

		Vector2 direction = previousPosition.DirectionTo(pokemonEnemy.GlobalPosition);
		bool isMovingRight = Math.Round(direction.X, 1) >= 0;
		pokemonEnemy.FlipH = isMovingRight;
	}

    public void AddPathFollow(PokemonEnemy pokemonEnemy)
	{
		PathFollow2D pathFollow = GetPathFollow();
		pathFollow.AddChild(pokemonEnemy);

		_pathFollows.Add(pathFollow);
		AddChild(pathFollow);
	}

    public void RemovePathFollow(PokemonEnemy pokemonEnemy)
	{
		PathFollow2D pathFollow = pokemonEnemy.GetParentOrNull<PathFollow2D>();
		pathFollow.QueueFree();
	}

	private PathFollow2D GetPathFollow()
	{
		PathFollow2D pathFollow = new PathFollow2D()
		{
			Rotates = false,
			Loop = false,
			YSortEnabled = true
		};
		pathFollow.TreeExiting += () => _pathFollows.Remove(pathFollow);

		return pathFollow;
	}
}
