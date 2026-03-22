# Critical Spin - Vertigo Games Assignment

This is a Wheel of Fortune mini-game prototype I developed for the Vertigo Games technical assignment. 
It's built with Unity UI, ScriptableObjects for data management, and DOTween for all animations.

## Core Features
*   **Wheel Spinning:** Physics-based wheel spinning with weighted random probability for selecting rewards.
*   **Bomb Logic:** Hitting the Bomb slice deletes your current session rewards unless you have enough gold to hit "Revive".
*   **Zone System:** 
    *   Every 5th zone is a "Safe Zone" (Silver wheel, no bombs).
    *   Every 30th zone is a "Super Zone" (Gold wheel, no bombs, rarer rewards).
*   **Risk & Reward:** Items collected during the spin are stored in temporary memory. They are only saved permanently (to PlayerPrefs) if the player clicks the Cashout button and successfully finishes the run.
*   **Zone Tracker:** A dynamic UI on the right side of the screen that tracks and scales down the remaining distance to the next Safe and Super zones.

## Architecture & Code
I chose to build the game using the MVP (Model-View-Presenter) pattern to keep the logic completely separated from the UI layer.

*   **Views:** Only handle UI and DOTween animations. There's no Unity Animator or timeline used in this project.
*   **Presenters:** Handle the core game states, maths (like the drop chance weights), and session loops.
*   **Models:** I heavily utilized ScriptableObjects (RewardData, WheelSliceData, ZoneData). This makes it easy for game designers to test different probabilities, icons, and items directly from the Inspector without touching the code.
*   **Event Bus:** UI buttons and Game Logic communicate via a global GameEvents static class instead of being tightly coupled.
*   **Object Pooling:** I wrote a custom generic UI Object Pool for things like the Zone Bar and the Cashout summary list to avoid frame drops from constant Instantiate and Destroy actions.
*   **Safe Area:** Implemented a single-run SafeArea script that perfectly fits the UI within the device notch on dimension change, avoiding heavy Update usages.

## Setup
*   Developed on Unity 2022.3.x (Insert your exact version if needed).
*   DOTween is the only external plugin used for performance reasons.
*   Open the main GameScene to play.

## Testing / Cheats
To help test the mechanics faster in the editor without spinning manually 50 times, I left some Numpad cheats active:
*   **Numpad 1:** Skip 10 zones.
*   **Numpad 2:** Skip 50 zones.
*   **Numpad 3:** Force a Bomb drop on the next spin.
*   **Numpad 9:** Toggle Auto-Spin and fast forward time (TimeScale x3).
