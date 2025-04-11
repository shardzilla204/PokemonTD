using Godot;
using System;

namespace PokemonTD;

public partial class EvolutionInterface : CanvasLayer
{
	[Export]
	private Timer _evolutionTimer;

	[Export]
	private Label _evolveLabel;

	[Export]
	private CustomButton _exitButton;

	[Export]
	private CustomButton _skipButton;

	public override void _Ready()
	{
		_exitButton.Pressed += () => 
		{
			_evolutionTimer.Stop();
			QueueFree();
		};
		_skipButton.Pressed += EvolvePokemon;
		_evolutionTimer.Timeout += EvolvePokemon;
	}

	public override void _Process(double delta)
	{
		
	}

	private void EvolvePokemon()
	{

	}
}
