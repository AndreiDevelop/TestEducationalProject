# TestEducationalProject

Empty Unity Universal 3D (URP) project. Gameplay and target platforms are not specified.

- Editor: Unity **6000.6.0f1**, the installed stable Editor used for bootstrap.
- Template: `com.unity.template.urp-blank` (Universal 3D).
- Start with [project instructions](AGENTS.md) and [Code Style and Architecture](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md).
- Design: [GDD writing rules](Documentation/GDD/GDD_BEST_PRACTICES.md); no game design is approved yet.
- Art: [Art Direction Guidelines](Documentation/Art/ART_DIRECTION_GUIDELINES.md).
- Editor asset tools: **Tools > Compress Assets**, including a read-only uncompressed-assets scanner. Review settings before applying changes; installation does not perform compression.

## Implementation status

The template camera/light scene is retained. The mandatory separate **Intro**, **Main** and additively loaded **HUD** scenes are pending implementation, including readiness, shared/scene-local lifetimes and unload cleanup. MCV with Zenject is the required implementation target; Zenject and gameplay systems are not installed.

Gameplay configuration must use typed `Assets/SO/<Type>/` folders and SO-suffixed definitions, created when corresponding features are implemented. Repeated content must use prefab composition. No gameplay configuration or placeholder prefabs are scaffolded.

Player builds and CI setup require a separate explicit request.
