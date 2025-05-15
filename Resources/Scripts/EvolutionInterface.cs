using Godot;

namespace PokemonTD;

public partial class EvolutionInterface : CanvasLayer
{
	[Signal]
	public delegate void FinishedEventHandler(Pokemon pokemonEvolution);

	[Export]
	private Timer _evolutionTimer;

	[Export]
	private Label _evolveLabel;

	[Export]
	private CustomButton _exitButton;

	[Export]
	private CustomButton _continueButton;

	[Export]
	private CustomButton _skipButton;

	[Export]
	private TextureRect _pokemonSprite;

	[Export]
	private TextureRect _pokemonSilhouette;

	[Export]
	private TextureRect _pokemonEvolutionSprite;

	[Export]
	private TextureRect _pokemonEvolutionSilhouette;

	public Pokemon Pokemon;

	private Pokemon _pokemonEvolution;

	private Tween _tween;

	private bool _hasEvolved = false;

	public override void _Ready()
	{
		_pokemonEvolution = PokemonEvolution.Instance.GetPokemonEvolution(Pokemon);

		_pokemonSprite.Texture = Pokemon.Sprite;
		_pokemonSilhouette.Texture = Pokemon.Sprite;

		_pokemonEvolutionSprite.Texture = _pokemonEvolution.Sprite;
		_pokemonEvolutionSilhouette.Texture = _pokemonEvolution.Sprite;

		_pokemonEvolutionSprite.Visible = false;
		_pokemonEvolutionSilhouette.Visible = false;

		_evolveLabel.Text = $"{Pokemon.Name}\n is evolving!";

		_exitButton.Pressed += () => 
		{
			_evolutionTimer.Stop();
			_tween.Kill();

			QueueFree();
		};
		_skipButton.Pressed += EvolvePokemon;
		_evolutionTimer.Timeout += EvolvePokemon;
		_continueButton.Pressed += () => 
		{
			PokemonEvolution.Instance.RemoveFromQueue(this);
			PokemonEvolution.Instance.IsQueueEmpty();

			EmitSignal(SignalName.Finished, _pokemonEvolution);
			QueueFree();
		};

		TweenEvolution();

		PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPause);
	}

	private async void TweenEvolution()
	{
		Color transparent = Colors.White;
		transparent.A = 0f;

		_tween = GetTree().CreateTween();
		_tween.TweenProperty(_pokemonSprite, "modulate", transparent, 2f);

		await ToSignal(_tween, Tween.SignalName.Finished);

		float tweenDuration = 0.25f;
		_tween = GetTree().CreateTween().SetLoops().SetParallel(true);
		_tween.TweenProperty(_pokemonEvolutionSilhouette, "visible", true, tweenDuration);
		_tween.TweenProperty(_pokemonSilhouette, "visible", false, tweenDuration);
		_tween.Chain().TweenProperty(_pokemonEvolutionSilhouette, "visible", false, tweenDuration);
		_tween.TweenProperty(_pokemonSilhouette, "visible", true, tweenDuration);
	}

	private void EvolvePokemon()
	{
		_tween.Kill();

		Color transparent = Colors.White;
		transparent.A = 0f;

		_exitButton.Modulate = transparent;
		
		_skipButton.Visible = false;
		_continueButton.Visible = true;

		_pokemonSprite.Visible = false;
		_pokemonSilhouette.Visible = false;

		_pokemonEvolutionSprite.Visible = true;
		_pokemonEvolutionSilhouette.Visible = false;
		
		_hasEvolved = true;
		_evolveLabel.Text = $"{Pokemon.Name} has evolved into \n{_pokemonEvolution.Name}!";
	}
}
