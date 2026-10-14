# Procedural Animation for 3D Characters

> Two techniques for procedurally animating 3D characters in Unity: a state-machine-driven arm reaching toward walls, and real-time foot placement on uneven terrain.

![Unity](https://img.shields.io/badge/Unity-Animation%20Rigging-000000?logo=unity&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-0078D6)
![License](https://img.shields.io/badge/License-MIT-green)

A Unity project exploring two complementary approaches to **procedural character animation**, comparing what can be achieved by driving bones purely from code against hybrid systems that combine traditional keyframed animation with runtime adaptation. The result of a final-degree project (*Trabajo de Fin de Grado*) on the viability of procedural animation as a replacement or complement to traditional pipelines.

---

## Table of Contents

- [Overview](#overview)
- [The Two Techniques](#the-two-techniques)
  - [1. Wall Interaction via State Machine](#1-wall-interaction-via-state-machine)
  - [2. Foot Placement on Uneven Terrain](#2-foot-placement-on-uneven-terrain)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [Usage](#usage)
- [Results and Trade-offs](#results-and-trade-offs)
- [Credits](#credits)
- [License](#license)

---

## Overview

Character animation in video games has traditionally been **pre-recorded** in a DCC tool and exported as static clips. While effective, this approach cannot react to a changing environment: a character cannot plant their feet on a slope they were not authored on, or raise a hand to a wall at the exact height of the wall they encounter.

Procedural animation inverts this. Instead of playing back a fixed clip, the engine **computes** the motion at runtime, giving the character the ability to adapt to its surroundings without an animator having to author a variant for every case.

This project implements and evaluates two different procedural techniques, chosen because they sit at opposite ends of the procedural/traditional spectrum:

1. A **fully procedural** approach where the motion is generated entirely by a state machine and inverse kinematics, with no keyframed animation involved for the interacting limb.
2. A **hybrid** approach that keeps the naturalness of traditional animation and layers a runtime IK pass on top of it to adapt the feet to the terrain.

Both implementations share the same base character with an idle / walk Animator Controller, and can be evaluated side by side.

---

## The Two Techniques

### 1. Wall Interaction via State Machine

The character detects an interactable wall in front of it, selects an arm, and reaches toward the wall with a natural motion. Once the hand contacts the wall, the pose is held while the character is in range, and released gracefully when they move away.

**How it works**

The limb motion is entirely procedural. The arm is controlled through two Unity Animation Rigging components:

- **`TwoBoneIKConstraint`** — drives the arm through inverse kinematics. It takes a `target` (the position the hand should reach) and a `hint` (the direction the elbow should bend toward), and a `weight` that blends the IK effect between 0 (follow the base animation) and 1 (fully IK-driven).
- **`MultiRotationConstraint`** — rotates the hand to match the wall's surface orientation plus an offset, so the palm lies flat against the wall.

Both constraints are animated from code by a finite state machine. The state machine follows a **`StateManager` / Context / State** pattern, which separates shared state (references to the constraints, the collider, the rigidbody) from per-state behaviour:

| State | Responsibility |
|---|---|
| `Search` | Look for an interactable wall in the detection radius |
| `Approach` | Choose an arm, move the target to a mid-height position in front of the character, blend the constraint weights up |
| `Rise` | Raycast against the wall at shoulder height, interpolate the target upward |
| `Touch` | Hold the pose while the character is in range and within the interaction timer |
| `Reset` | Return the targets to their rest positions and blend the weights back to 0 |

The state machine is triggered by a `Collider` on the character that detects GameObjects tagged as interactable. Transitions are driven by distance to the wall and by the constraint's progress, not by timers alone, so the character always finishes the current phase before changing state.

**Why this matters**

This demonstrates the ceiling of what pure procedural animation can do for a single limb: complete control over the motion, no dependency on a pre-authored clip, and the ability to adapt to any wall the character encounters. The cost is that extending the same approach to the whole body (walking, torso rotation, head tracking) becomes exponentially more complex.

### 2. Foot Placement on Uneven Terrain

The character walks over uneven ground — slopes, steps, small obstacles — and the feet plant naturally on the surface beneath them, while the torso rises and falls to compensate for the terrain.

**How it works**

This technique keeps the traditional keyframed animation for the character's overall walk cycle and layers a runtime IK pass on top of it:

1. The Animator Controller's base layer has **IK Pass** enabled, which causes Unity to call the `OnAnimatorIK` callback on the character's MonoBehaviour every frame after the animation is evaluated.
2. A custom `IK Foot Placement` script performs a **raycast** downward from each foot's current position. If the ray hits terrain, the script overrides the foot's `AnimatorIK` position and rotation so it matches the surface.
3. The torso's vertical offset is driven by the altitude of the **lower** foot, interpolated smoothly so that stepping up onto a curb looks natural rather than instantaneous.

The critical design decision is that the code does not try to *replace* the animation — it only *corrects* the parts that would look wrong on uneven ground. The character's walk cycle, weight shift, and arm swing all still come from the traditional clip, which is what gives the motion its naturalness.

**Why this matters**

This demonstrates the sweet spot for most production use cases: procedural animation is used not to author motion from scratch, but to **adapt an existing motion** to conditions the animator could not have anticipated. The combined system achieves a level of realism that neither approach could reach on its own.

---

## Project Structure

```
Assets/
├── Scenes/
│   ├── Wall_Adaptation_Scene            # State machine + arm IK demo
│   └── Leg_IKAnim_Adaptation_Scene      # Traditional walk + foot IK demo
├── Scripts/
│   ├─── Arm_EnvironmentInteractions
│   │   ├── EnvironmentInteractionArmState.cs
│   │   ├── EnvironmentInteractionArmStateMachine.cs
│   │   └── Arm_States/
│   │       └── ApproachArmState.cs
│   │       ├── ResetArmState.cs
│   │       ├── RiseArmState.cs
│   │       ├── SearchArmState.cs
│   │       └── TouchArmState.cs
│   ├── CharacterMovement/
│   │   └── CharacterMovement.cs
│   ├── IK_Foot_Placement/
│   │   ├── IKFootPlacement.cs
│   │   └── IKMovementV2.cs
│   ├── Leg_EnvironmentInteractions/
│   │   ├── EnvironmentInteractionLegState.cs
│   │   ├── EnvironmentInteractionLegStateMachine.cs
│   │   └── Leg_States/
│   │       ├── ResetLegState.cs
│   │       └── SearchLegState.cs
│   └── StateMachine/
│       ├─── BaseState.cs
│       ├─── EnvironmentInteractionContext.cs
│       └─── StateManager.cs
├── Prefabs/
│   ├── Ninja               # Prefab with Ninja model and with state machine
│   └── Ninja_IKAnim        # Prefab with Ninja model and with state machine + foot IK
└── Models_Animations/
    └── Character                  # Humanoid rig (user-supplied)
```

---

## Requirements

| | |
|---|---|
| **Unity** | 2021.3 LTS or later (uses the Animation Rigging package) |
| **Packages** | `com.unity.animation.rigging` (installed via the Unity Package Manager) |
| **OS** | Windows / macOS / Linux |

No external SDK or hardware is required. The project runs entirely inside the Unity Editor.

---

## Usage

### Wall Interaction Scene

1. Open `WallInteractionScene`.
2. Press **Play**.
3. Walk the character toward the wall with the movement keys.
4. When the character enters the detection radius, the state machine will trigger automatically and the character will reach a hand toward the wall.
5. Move away from the wall to see the release animation.

Red cubes appear in the Scene view during play, indicating the IK **targets** and **hints**. These are visible in the Editor for debugging and can be toggled off via the Gizmos menu.

### Foot Placement Scene

1. Open `FootPlacementScene`.
2. Press **Play**.
3. Move the character across the test terrain (slopes, steps, small obstacles).
4. The feet will plant on the surface beneath them, and the torso will adjust its height to compensate.

The test terrain is intentionally exaggerated so the effect is easy to see. On flat ground the IK pass has no effect, and the character behaves exactly as with the base animation.

---

## Results and Trade-offs

The project produced two working systems and a set of conclusions about when each is appropriate.

### Procedural vs. Procedural + Traditional

| | 100% Procedural (wall interaction) | Procedural + Traditional (foot placement) |
|---|---|---|
| **Advantages** | Full control over the motion. Serves as a reusable template for procedural limb animation. | Naturalness of a keyframed animation combined with runtime adaptation that is impossible to author by hand. |
| **Drawbacks** | Motion can feel "direct" — realism decreases as more of the body is driven procedurally. Time cost grows exponentially with the number of bones involved. | Requires an existing traditional animation that shares the character's bone structure. Less code-level control over the result. |
| **Recommended for** | Limbs whose motion does not depend on an existing clip, and where high precision is required. | Adapting an existing animation to runtime events (uneven terrain, dynamic obstacles, procedural targets). |

### Key findings

- **Pure procedural animation scales poorly with body coverage.** A single reaching arm is manageable; a full walking cycle driven from code becomes a research project on its own.
- **Hybrid approaches give the best realism for the cost.** Using Unity's `OnAnimatorIK` to layer corrections over a traditional clip achieves a level of fidelity that neither approach reaches alone.
- **The torso must be part of the IK solution.** Adapting only the feet without lifting the hips produces the "sunken character" artefact. The `IK Foot Placement` script computes the torso offset from the lower foot to avoid this.
- **State machines are the right abstraction for limb-level procedural motion.** Attempting to blend procedural and traditional motion with ad-hoc timers was fragile; the state-machine pattern (each state responsible for a single phase of the motion, transitions based on measured progress rather than time) was robust.

### Possible improvements

- Modulate the vertical torso speed based on terrain steepness, so descending a slope feels different from descending a step.
- Add joint-angle limits to reject physically impossible configurations (e.g. elbows or knees bending the wrong way).
- Extend the state machine to a second interacting limb (both hands on the wall).
- Blend between multiple base animations (idle, walk, run) based on the same state machine that drives the IK targets.

---

## Credits

This project is built on top of work by several people and external references, and their contribution is gratefully acknowledged.

### iHeartGameDev — state machine architecture and procedural animation tutorials

The finite state machine pattern used for the wall interaction, and the approach to blending `TwoBoneIKConstraint` and `MultiRotationConstraint` weights from code, are based on the tutorials from the **iHeartGameDev** YouTube channel, created by Dan.

- Channel: https://www.youtube.com/@iHeartGameDev

All credit for the underlying state machine architecture and the pedagogical approach belongs to iHeartGameDev. The adaptation to the specific wall-interaction use case in this project is original work. See [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for details.

### Unity Animation Rigging package

The runtime IK and constraint components (`TwoBoneIKConstraint`, `MultiRotationConstraint`, `BoneRenderer`) are part of Unity's official **Animation Rigging** package, developed by Unity Technologies. They are installed through the Unity Package Manager and are not redistributed with this repository.

### Unity engine

The runtime environment and standard APIs (Animator, Rigidbody, Collider, Raycast, `OnAnimatorIK`, MonoBehaviour lifecycle) are provided by the Unity engine, subject to Unity's Terms of Service.

### Character model

The character used during development is not bundled with this repository. Users should provide their own Humanoid rig.

---

## License

This project's original code is released under the **MIT License**. See [`LICENSE`](LICENSE) for the full text.

Third-party components and referenced materials are subject to their own licenses and terms; see [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) for details.
