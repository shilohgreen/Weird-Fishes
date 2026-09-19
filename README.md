# Portal Reef

An augmented reality reef for Meta Quest. Parts of your room become portals into open ocean, and fish swim in through one portal, cross the room around you, and leave through another.

This repository is the Unity project. It is an early prototype, and the current focus is getting one fish to swim well before building the school, the scene, and the headset integration.

## Status

| Version | Focus | State |
|---|---|---|
| v1 | One fish with a clean rig and smooth, stable swimming | In progress. The fish swims with procedural body animation and no runtime errors. Tuning remains. |
| v2 | A small school swimming together, with lighting and set pieces | Not started |
| v3 | Running on Quest, with more species | Not started |

## How the fish works

Fish movement and body animation come from Vivian Ménard's boids project. See [Credits](#credits).

- **Movement.** Each fish is a boid. It steers using separation, alignment, cohesion, a random walk when alone, and forward raycasts to avoid obstacles.
- **Body animation.** The fish is not keyframed. The code records the path the fish's head has travelled, then places each spine bone along that path, behind the head. The body bends through turns because the tail follows where the head has been.
- **Rig requirement.** That approach needs one linear chain of bones from head to tail, with the first joint on the model's pivot and the last bone farthest from the head. The original `Fish1.fbx` has a branched armature, so it was re-rigged in Blender as `fish1_VMrig.fbx`.
- **Boundary.** `SwimBoundary` is a visible test volume made of six box colliders on the `BoidsObstacles` layer. Fish steer away from the walls before touching them.

## Getting started

1. Install Unity `6000.0.61f1` through Unity Hub. The project uses the Universal Render Pipeline.
2. Clone the repository and add the folder to Unity Hub.
3. Open `Assets/Scenes/SampleScene.unity`.
4. Press Play. One fish spawns inside the cyan boundary and wanders.

Behaviour settings live on the `EntitiesManager` object in the scene. Change them outside Play mode and save the scene, or Unity reverts them.

## Project layout

| Path | Contents |
|---|---|
| `Assets/PortalReef/` | Everything owned by this project: models, prefabs, the swim boundary, shaders |
| `Assets/PortalReef/Models/fish1_VMrig.fbx` | Fish1 with the five-bone head-to-tail rig |
| `Assets/PortalReef/Prefabs/fish1_VMrig_boid.prefab` | The boid prefab the manager spawns: model, Boids layer, sphere collider, boid script |
| `Assets/References/VivianMenard/` | Vivian Ménard's boids code and assets, kept separate from project code |
| `Assets/Blender/` | Blender working files for rigging |

## Rigging a new fish

The same steps should work for the other fish models.

1. In Blender, duplicate the mesh, clear its parent, and remove its Armature modifier and vertex groups.
2. Move the mesh so the nose tip is on the world origin, with the body along the Y axis. Apply all transforms.
3. Add a single bone from the nose to the tail tip, then subdivide it into the number of bones you want. Five matches the reference fish.
4. Parent the mesh to the armature using **With Automatic Weights**.
5. Export as FBX with only the armature and mesh selected, **Apply Scalings** set to **FBX All**, **Add Leaf Bones** off, and animation off.
6. In Unity, set the rig to **Generic** and turn **Strip Bones** off. Build a prefab variant with the `Boids` layer, a sphere collider, and `BoidScript`. Keep the prefab root at scale 1.

## Credits

**Boid simulation and procedural fish animation: Vivian Ménard.**
The code and assets under `Assets/References/VivianMenard/` come from the [boids](https://github.com/VivianMenard/boids) project by Vivian Ménard, used under the MIT License, copyright 2024 Vivian Ménard. The full licence text is in [`Assets/References/VivianMenard/LICENSE`](Assets/References/VivianMenard/LICENSE). This covers the flocking behaviour, obstacle avoidance, predator logic, and the trajectory-based body animation that Portal Reef's fish rely on. There is also a [video demo](https://www.youtube.com/watch?v=RCrR4KQPHqg) and a [playable build](https://vivianmenard.itch.io/boids) of the original.

Changes made to that code in this repository are small: null guards in `Parameters.cs` and `EntitiesManagerScript.cs`. The five-bone Fish1 rig also follows the structure and weighting method of the reference fish model from that project.

**Fish and sea animal models.** Source to be confirmed.
