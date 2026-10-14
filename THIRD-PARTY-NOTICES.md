# Third-Party Notices

This project is distributed under the MIT License (see `LICENSE`). It depends
on and/or has been informed by the following third-party components and
resources, which are subject to their own terms.

---

## iHeartGameDev — YouTube channel

**Role:** Tutorial reference for the state machine implementation and the
procedural arm animation logic.

- **Author:** Dan (iHeartGameDev)
- **Channel:** https://www.youtube.com/@iHeartGameDev
- **GitHub (community resources):** https://github.com/iHeartGameDev

**Usage in this project:** The finite state machine pattern used for the wall
interaction (see `EnvironmentInteractionStateMachine` and the `StateManager` /
`EnvironmentInteractionContext` classes) follows the architecture presented in
iHeartGameDev's tutorials on animation state machines and procedural character
animation. Some of the constraint setups for `TwoBoneIKConstraint` and
`MultiRotationConstraint` (weight blending, target/hint assignment) are also
based on the approach demonstrated in those tutorials.

**Attribution:** All credit for the underlying state machine architecture and
the pedagogical framing of the procedural animation approach belongs to
iHeartGameDev. The adaptation to this project's specific use case (wall
interaction on a character with an idle / walk base animation) is original work
by the project author.

**Recommendation:** If you plan to reuse the state machine pattern in your own
project, we strongly recommend watching the original tutorials directly, as
they contain important design context that a code comment cannot fully convey.

---

## Unity Animation Rigging package

**Role:** Runtime IK and constraint components used in this project.

- **Author:** Unity Technologies
- **Package:** `com.unity.animation.rigging`
- **Documentation:** https://docs.unity3d.com/Packages/com.unity.animation.rigging@latest
- **License:** Unity Companion License (see the package's own `LICENSE.md`)

**Components used:**

- `TwoBoneIKConstraint` — for the arm reaching toward the wall target
- `MultiRotationConstraint` — for matching the hand orientation to the wall
- `BoneRenderer` — for visualizing the skeleton during development
- Animator **IK Pass** (not a component of this package, but a related Unity
  feature) — enables the `OnAnimatorIK` callback used by the foot placement
  system

**Usage in this project:** These components are part of the standard Unity
Animation Rigging package and are installed through the Unity Package Manager.
They are not redistributed in this repository; users get them automatically
when the package is resolved.

---

## Unity engine and standard assets

**Role:** Runtime and editing environment.

- **Author:** Unity Technologies
- **License:** Unity Terms of Service (see https://unity.com/legal/terms-of-service)
- **Note:** Standard Unity APIs (Animator, Rigidbody, Collider, Raycast,
  MonoBehaviour lifecycle) are used throughout the project. No Unity binaries
  are redistributed here.

---

## Character model

**Role:** Reference Humanoid rig used for both implementations.

- **Author:** Adobe
- **Source:** https://www.mixamo.com/
- **License:** Adobe Mixamo Terms of Use
  (https://www.mixamo.com/legal/mixamo_terms_of_use)

**Notes:**The Ninja character is included as a reference
Humanoid rig for recording and baking demonstrations. It must remain bundled
with the rest of the project and must not be extracted and redistributed as a
standalone asset.
**Files:** `Assets/KinectEnvironment/Characters/`

**Usage in this project:** The Ninja character is included as a reference
Humanoid rig for recording and baking demonstrations. It must remain bundled
with the rest of the project and must not be extracted and redistributed as a
standalone asset.
