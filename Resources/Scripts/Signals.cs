using Godot;

namespace PokemonTD;

public partial class Signals : Node
{
    [Signal]
    public delegate void GameStartedEventHandler();

    [Signal]
    public delegate void GameSavedEventHandler();

    [Signal]
    public delegate void GameLoadedEventHandler();

    [Signal]
    public delegate void GameResetEventHandler();

    [Signal]
    public delegate void PokemonStarterSelectedEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokemonFaintedEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokemonTeamUpdatedEventHandler();

    [Signal]
    public delegate void PokemonDamagedEventHandler(int damage, int teamSlotIndex);

    [Signal]
    public delegate void SortButtonPressedEventHandler();

    [Signal]
    public delegate void StageStartedEventHandler();

    // Evolution
    [Signal]
    public delegate void EvolutionStartedEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void EvolutionFinishedEventHandler(int teamSlotIndex);

    [Signal]
    public delegate void EvolutionQueueClearedEventHandler();

    // Pokemon
    [Signal]
    public delegate void PokemonLeveledUpEventHandler(Pokemon pokemon, int teamSlotIndex);

    [Signal]
    public delegate void PokemonEvolvedEventHandler(Pokemon pokemon, Pokemon pokemonEvolution);

    [Signal]
    public delegate void PokemonGainedExperienceEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokemonLearnedMoveEventHandler(Pokemon pokemon, PokemonMove pokemonMove);

    // Enemy Pokemon
    [Signal]
    public delegate void PokemonEnemyFaintedEventHandler(PokemonEnemy pokemonEnemy);

    [Signal]
    public delegate void PokemonEnemyPassedEventHandler(PokemonEnemy pokemonEnemy);
    
    [Signal]
    public delegate void PokemonEnemyCapturedEventHandler(PokemonEnemy pokemonEnemy);

    // Dragging 
    [Signal]
    public delegate void DraggingStageTeamSlotEventHandler(bool isDragging);

    [Signal]
    public delegate void DraggingStageSlotEventHandler(bool isDragging);

    [Signal]
    public delegate void DraggingPokeBallEventHandler(bool isDragging);

    // Stage
    [Signal]
    public delegate void PokemonOnStageEventHandler(int teamSlotIndex);

    [Signal]
    public delegate void PokemonOffStageEventHandler(int teamSlotIndex);

    [Signal]
    public delegate void HasWonEventHandler();

    [Signal]
    public delegate void HasLostEventHandler();

    [Signal]
    public delegate void ChangeMovesetPressedEventHandler();

    // Stage Controls
    [Signal]
    public delegate void PressedPlayEventHandler();

    [Signal]
    public delegate void PressedPauseEventHandler();

    [Signal]
    public delegate void SpeedToggledEventHandler(float speed);

    // Forget Move
    [Signal]
    public delegate void ForgetMoveEventHandler(Pokemon pokemon, PokemonMove pokemonMove);

    [Signal]
    public delegate void ForgetMoveQueueClearedEventHandler();

    // Audio
    [Signal]
    public delegate void AudioMutedEventHandler(int busIndex, bool isMuted);

    [Signal]
    public delegate void AudioValueChangedEventHandler(int busIndex, int volume);

    [Signal]
    public delegate void StageTeamSlotMutedEventHandler(int teamSlotIndex, bool isMuted);

    // Stage Selection
    [Signal]
    public delegate void OnStageSelectionEventHandler();

    [Signal]
    public delegate void StageSelectButtonHoveredEventHandler(PokemonStage pokemonStage);

    [Signal]
    public delegate void StageSelectButtonPressedEventHandler(PokemonStage pokemonStage);

    [Signal]
    public delegate void PokemonAnalyzedEventHandler(Pokemon pokemon);
}
