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
	private CustomButton _cancelButton;

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

	private Pokemon _pokemon;
	private Pokemon _pokemonEvolution;

	private Tween _tween;

	private bool _hasEvolved = false;

	public override void _Ready()
	{
		_pokemonEvolutionSprite.Visible = false;
		_pokemonEvolutionSilhouette.Visible = false;

		_evolveLabel.Text = $"{_pokemon.Name}\n is evolving!";

		_cancelButton.Pressed += () => 
		{
			_evolutionTimer.Stop();
			_tween.Kill();

			PokemonEvolution.Instance.RemoveFromQueue(this);
			PokemonEvolution.Instance.IsQueueEmpty();

			_pokemon.HasCanceledEvolution = true;

			EmitSignal(SignalName.Finished, _pokemon);
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

		TweenEvolution(_pokemonSprite, _pokemonSilhouette, _pokemonEvolutionSilhouette);
		
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PressedPause);
	}

	public async void TweenEvolution(TextureRect pokemonSprite, TextureRect pokemonSilhouette, TextureRect pokemonEvolutionSilhouette)
	{
		Color transparent = Colors.White;
		transparent.A = 0f;

		_tween = GetTree().CreateTween();
		_tween.TweenProperty(pokemonSprite, "modulate", transparent, 2f);

		await ToSignal(_tween, Tween.SignalName.Finished);

		float tweenDuration = 0.25f;
		_tween = GetTree().CreateTween().SetLoops().SetParallel(true);
		_tween.TweenProperty(pokemonEvolutionSilhouette, "visible", true, tweenDuration);
		_tween.TweenProperty(pokemonSilhouette, "visible", false, tweenDuration);
		_tween.Chain().TweenProperty(pokemonEvolutionSilhouette, "visible", false, tweenDuration);
		_tween.TweenProperty(pokemonSilhouette, "visible", true, tweenDuration);
	}

	public void SetPokemon(Pokemon pokemon, Pokemon pokemonEvolution)
	{
		_pokemon = pokemon;
		_pokemonEvolution = pokemonEvolution;

		_pokemonSprite.Texture = pokemon.Sprite;
		_pokemonSilhouette.Texture = pokemon.Sprite;

		_pokemonEvolutionSprite.Texture = pokemonEvolution.Sprite;
		_pokemonEvolutionSilhouette.Texture = pokemonEvolution.Sprite;
	}

	private void EvolvePokemon()
	{
		_tween.Kill();

		Color transparent = Colors.White;
		transparent.A = 0f;

		_cancelButton.Modulate = transparent;

		_skipButton.Visible = false;
		_continueButton.Visible = true;

		_pokemonSprite.Visible = false;
		_pokemonSilhouette.Visible = false;

		_pokemonEvolutionSprite.Visible = true;
		_pokemonEvolutionSilhouette.Visible = false;

		_hasEvolved = true;
		_evolveLabel.Text = $"{_pokemon.Name} has evolved into \n{_pokemonEvolution.Name}!";
	}
}
