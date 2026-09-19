# Portal Reef — Agent Handoff

## Project

- Unity project: `/Users/ryanpey/Projects/aug-reality/PortalReef`
- GitHub: https://github.com/shilohgreen/Weird-Fishes
- Repository is private.
- Latest commit at handoff: `ab32d68 Add fish boid movement and swim boundary`
- Current untracked path: `Assets/Blender/` — this is the user's Blender work; do not delete or overwrite it.
- Notion page: **Portal Reef**, page ID `3d85d400-2912-81e7-b96c-d51e0ad1c4d8`

## User preferences

- Never modify files unless the user explicitly says **execute**.
- Answer questions first and explain concisely.
- Debug logs must use this format: `[ <emoji> <parent-function-name> ]`.
- Avoid editing Unity scene YAML while Unity is open. Previous external edits reset unsaved Inspector settings.
- Do not delete or unnecessarily rewrite Vivian Menard's reference code/assets.

## Current roadmap

1. **v1 — Single fish:** Fish1 has a completed compatible rig and smooth, stable swimming animation.
2. **v2 — School and scene:** A small school swims together with lighting and supporting fixtures.
3. **v3 — Quest integration:** Run on Quest and add other boids/species.

Current priority is v1.

## Completed work

- Added a standard Unity `.gitignore`, initialized Git, and pushed the project.
- Added Vivian Menard's boid implementation under `Assets/References/VivianMenard/`.
- Copied VM's original `fish.fbx` so its nested `Boid.prefab` resolves.
- Added VM-required Unity tag/layers:
  - Tag: `EntitiesManager`
  - Layers: `Boids`, `Predators`, `Obstacles`, `BoidsObstacles`, `PredatorsObstacles`
- Wired `EntitiesManager`, spawn area, Fish1 boid prefab, and movement in `Assets/Scenes/SampleScene.unity`.
- Hidden unrelated scene animals while working on Fish1.
- Added null guards in:
  - `Assets/References/VivianMenard/Scripts/Parameters.cs`
  - `Assets/References/VivianMenard/Scripts/EntitiesManagerScript.cs`
- Added a visible swim boundary:
  - `Assets/PortalReef/SwimBoundary.cs`
  - Six persistent `BoxCollider` walls on `BoidsObstacles`
  - Visible wireframe edges
- Boundary avoidance now works after VM raycast parameters/layers were configured.
- Added `Assets/PortalReef/Shaders/FishSwim.shader`, an experimental URP vertex-wave shader. The active VM approach uses skeletal/procedural animation instead.
- Downloaded three boid references outside this project for comparison. VM is the selected implementation.
- Added `com.unity.ugui` to the separate NVJOB reference project's package manifest to fix its UI compile errors.
- Updated the Portal Reef Notion page with the v1–v3 roadmap and a high-level Blender checklist.

## Current blocker: Fish1 rig

Fish1 moves and avoids the boundary, but VM's procedural body animation stutters and repeatedly throws:

```text
KeyNotFoundException
EntityScript.ComputeBonesPositionsAndRotations()
Assets/References/VivianMenard/Scripts/EntityScript.cs:185
```

Relevant VM behavior:

- `EntityScript` obtains all bones from the child `SkinnedMeshRenderer`.
- It records past transforms by frame.
- It positions successive bones from that movement history.
- Its assumptions match VM's simple ordered, linear head-to-tail chain.

Fish1's armature is structurally different:

- It has a main body chain plus several branches around the head/body.
- Bone distances/order are not compatible with VM's history calculation.
- Merely renaming or reordering bones does not turn a branched armature into the required linear chain.

The intended fix is to prepare a new Fish1 rig in Blender rather than further modifying VM's animation code.

## Blender progress

- User installed Blender `5.2.2`.
- Imported both Fish1 and VM's `fish.fbx` into one Blender scene.
- Rotated and positioned them side by side.
- Hid both meshes to compare armatures.
- Visual comparison confirmed:
  - VM fish: one simple, uninterrupted head-to-tail chain.
  - Fish1: main chain with multiple branches near the front.
- No bones should have been edited yet.

Likely Blender file/work is under the currently untracked `Assets/Blender/`.

## Recommended next Blender steps

Proceed interactively and one small step at a time:

1. Preserve the imported/original Fish1 and save a separate working `.blend`.
2. Inspect Fish1's armature in Edit Mode and identify the actual tail-to-head deform chain.
3. Create or simplify to a VM-compatible linear deform chain.
4. Reassign/normalize mesh weights so the body deforms cleanly along that chain.
5. Ensure armature/mesh transforms are applied and orientation/scale are Unity-safe.
6. Export as a **new FBX** without overwriting the original.
7. Import into Unity and create a new Fish1 prefab.
8. Test movement, turns, boundary avoidance, deformation, and confirm there are no animation exceptions.

Do not rush into deleting branch bones until their vertex weights and purpose have been inspected.

## Useful Unity details

- Fish movement is in VM's `EntityScript.FixedUpdate`.
- Obstacle detection uses `Physics.Raycast` with `parameters.obstaclesLayerMask`.
- VM builds that mask from `Obstacles` and `BoidsObstacles`.
- Settings previously needed for avoidance included nonzero `Raycast Base Distance` (tested around `2`) and `Obstacle Base Margin` (tested around `0.5`).
- Make Inspector changes outside Play Mode and save the scene/prefab, otherwise Unity reverts them.
- Fish1 and the other supplied fish FBXs already contain armatures; the issue is compatibility with VM's specific procedural-animation assumptions, not absence of a rig.

## v1 completion checklist

- [ ] Repair/export Fish1 with a compatible head-to-tail deform chain.
- [ ] Create a clean new Fish1 prefab without overwriting the original.
- [ ] Verify smooth swimming and turning.
- [ ] Verify boundary avoidance.
- [ ] Verify no `KeyNotFoundException`, stuttering, or mesh corruption.

