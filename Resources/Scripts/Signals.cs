using Godot;

namespace PokemonTD;

public partial class Signals : Node
{
    [Signal]
    public delegate void SortButtonPressedEventHandler(int sortCategoryID);

    [Signal]
    public delegate void StageStartedEventHandler();

    [Signal]
    public delegate void PokeDollarsUpdatedEventHandler();

    [Signal]
    public delegate void RareCandyUpdatedEventHandler();

    [Signal]
    public delegate void PokemonForgettingMoveEventHandler(Pokemon pokemon, PokemonMove pokemonMove);

    [Signal]
    public delegate void EvolutionFinishedEventHandler(Pokemon pokemonEvolution, int teamSlotIndex);
    
    [Signal]
    public delegate void PokemonEnemyPassedEventHandler(PokemonEnemy pokemonEnemy);

    [Signal]
    public delegate void PokemonEnemyCapturedEventHandler(PokemonEnemy pokemonEnemy);

    // Game
    [Signal]
    public delegate void GameStartedEventHandler();

    [Signal]
    public delegate void GameSavedEventHandler();

    [Signal]
    public delegate void GameLoadedEventHandler();

    [Signal]
    public delegate void GameResetEventHandler();

    // Pokemon
    [Signal]
    public delegate void PokemonStarterSelectedEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokemonFaintedEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokemonTeamUpdatedEventHandler();

    [Signal]
    public delegate void PokemonUpdatedEventHandler(Pokemon pokemon, int teamSlotIndex);

    [Signal]
    public delegate void PokemonHealedEventHandler(int health, int teamSlotIndex);

    [Signal]
    public delegate void PokemonDamagedEventHandler(int damage, int teamSlotIndex);

    [Signal]
    public delegate void PokemonLeveledUpEventHandler(int levels, int teamSlotIndex);

    [Signal]
    public delegate void PokemonEvolvedEventHandler(Pokemon pokemonEvolution, int teamSlotIndex);

    [Signal]
    public delegate void PokemonGainedExperienceEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokemonLearnedMoveEventHandler(Pokemon pokemon, PokemonMove pokemonMove);

    [Signal]
    public delegate void ItemReceivedEventHandler();

    // Dragging 
    [Signal]
    public delegate void DraggingPokemonTeamSlotEventHandler(PokemonTeamSlot pokemonTeamSlot, bool isDragging);

    [Signal]
    public delegate void DraggingPokemonStageSlotEventHandler(PokemonStageSlot pokemonStageSlot, bool isDragging);

    [Signal]
    public delegate void DraggingPokeBallEventHandler(bool isDragging);

    [Signal]
    public delegate void DraggingFinishedEventHandler();

    // Stage
    [Signal]
    public delegate void PokemonUsedEventHandler(bool isInUse, int teamSlotIndex);

    [Signal]
    public delegate void HasWonStageEventHandler();

    [Signal]
    public delegate void HasLostStageEventHandler();

    [Signal]
    public delegate void ChangeMovesetPressedEventHandler();

    [Signal]
    public delegate void HasLeftStageEventHandler();

    // Stage Controls
    [Signal]
    public delegate void PressedPlayEventHandler();

    [Signal]
    public delegate void PressedPauseEventHandler();

    [Signal]
    public delegate void SpeedToggledEventHandler(float speed);

    // Audio
    [Signal]
    public delegate void AudioValueChangedEventHandler(int busIndex, int volume);

    [Signal]
    public delegate void AudioMutedEventHandler(int busIndex, bool isMuted);

    [Signal]
    public delegate void PokemonTeamSlotMutedEventHandler(int teamSlotIndex, bool isMuted);

    // Stage Selection
    [Signal]
    public delegate void StageSelectedEventHandler();

    [Signal]
    public delegate void StageSelectButtonHoveredEventHandler(PokemonStage pokemonStage);

    [Signal]
    public delegate void StageSelectButtonPressedEventHandler(PokemonStage pokemonStage);

    [Signal]
    public delegate void PokemonAnalyzedEventHandler(Pokemon pokemon);
}
