# Political Galaxy 🪐

*A 3D explorable explanation translating raw tweet volume and engagement metrics into physical mass and gravitational pull.*
**GAM 509E Midterm Project**

## Overview
**Political Galaxy** is an interactive 3D application that uses gravitational physics as a metaphor for social media echo chambers. By visualizing polarizing viewpoints as planetary bodies, it provides users with an "Explorable Explanation" of algorithmic bias, engagement metrics, and echo chamber mechanics surrounding US Election 2020 topics. 

As users navigate the void, they encounter massive "polarizing planets" whose sizes and gravitational forces are procedurally generated based on real-world tweet counts. 

## Key Features & Mechanics

### 🪐 Planetary Gravity & Data Visualization
- **Data-Driven Mass**: Planet scale, particle effects, and orbits are directly calculated from raw tweet data (using `DataLoader.cs`). The higher the engagement, the stronger the planet's gravitational pull (`PlanetGravity.cs`).
- **Orbiting Keywords**: Words representing related tweets orbit their respective planets at varying speeds, visually demonstrating the clustering of topics within echo chambers (`OrbitingKeyword.cs`).

### 🚀 Scrollytelling & Exploration
- **Immersive Navigation**: A custom FPS controller (`SpaceFPSController.cs`) allows users to seamlessly navigate the 3D space, being physically pulled by the gravity of the polarizing topics.
- **Narrative Progression**: Interactions are scaffolded. Users must visit different thematic planets, updating their UI checklist.
- **The Bridge**: Upon exploring all polarizing viewpoints, a cosmic portal opens. Passing through the portal teleports the user to "The Bridge" planet (`PortalGate.cs`, `BridgePlanetInfo.cs`)—a spatial representation of dialogue, empathy, and common ground, where the keywords shift from divisive to unifying.

### 🎧 Tactile & Ambient Audio
- **Audio Engine**: A centralized audio management system (`AudioManager.cs`) handles background ambiance, UI button clicks, and orbit-entry sound effects. Entering a planet's orbit provides immersive tactile audio feedback to enhance the visceral feeling of gravitational pull.

## Project Structure
All core logic is contained within the `Assets/Scripts` directory:
- `PlanetManager.cs`: Handles procedural generation of planets, portals, UI checklist, and instantiating the orbiting keywords.
- `PlanetGravity.cs`: Manages audio-visual interactions and UI pop-ups upon entering a planetary orbit.
- `DataLoader.cs`: Reads the parsed JSON election dataset.
- `PortalGate.cs` / `BridgePlanetInfo.cs`: Controls the climax of the experience, managing the transition towards the unified "Bridge" state.
- `SpaceFPSController.cs`: The core player movement script adapted for zero-gravity void navigation.
- `IntroManager.cs`: Controls the onboarding flow for new users coming into the experience.

## Getting Started

1. **Prerequisites**: Ensure you have a compatible version of Unity installed (3D Core).
2. **Open the Project**: Clone this repository and open the `Midterm Polar` folder inside the Unity Hub.
3. **Play**: Open the main scene. You will be greeted by the Introduction UI. Click through to start exploring the galaxy. Use WASD / Mouse to fly around the different celestial bodies.

## Context
This project was developed to evaluate how spatial and physical metaphors can improve mental models of complex algorithmic systems. It started as a transition from a WebGL/React-driven prototype into a fully embodied Unity 3D experience.
