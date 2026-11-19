# Project bootstrap verification

Date: 2026-11-19
Status: Verified empty-project setup

## Environment and scope

- Repository: https://github.com/AndreiDevelop/TestEducationalProject (existing public repository; visibility preserved).
- Final Unity root: repository root, with Assets, Packages and ProjectSettings directly beneath it.
- Editor: Unity 6000.6.0f1 arm64, the only installed Editor. Used the installed stable release; no LTS Editor was installed and no Editor installation or upgrade was performed.
- Unity CLI: 1.0.1; the online self-update check reported it current.
- Template: Universal 3D, com.unity.template.urp-blank, local template 17.2.1. Template URP package: 17.6.0.
- Added com.unity.pipeline 1.0.0 for CLI control and the bundled Editor-only AssetProcessing tools. Other packages are the template defaults. No Zenject, gameplay scaffolding, monetization, CI or platform modules were added.
- Project registered at its final path in Unity Hub and added to GitHub Desktop. Desktop selection shows TestEducationalProject on main.

## Checks performed

- Created the template with Unity CLI in a temporary parent, then transferred only Assets, Packages and ProjectSettings into the cloned repository without overwriting files.
- Imported the final project in the live Editor. The first launch exited during initial refresh; a second CLI launch completed import successfully. Final Pipeline state is ready, with compilation and updating both false.
- Unity Console ground truth: compilationFailed false, zero errors and zero warnings after import and tutorial cleanup.
- Set product name TestEducationalProject, ForceText serialization and Visible Meta Files through Unity APIs; saved settings.
- Opened Assets/Scenes/SampleScene.unity through Unity APIs. Saved roots: Main Camera, Directional Light, Global Volume. Scene is clean and has zero missing MonoBehaviour scripts.
- Active rendering pipeline is Assets/Settings/PC_RPAsset.asset, selected by the template quality settings. A null global fallback pipeline is the template state; the active pipeline is URP.
- Removed unused template Readme.asset and TutorialInfo through AssetDatabase after checking the retained scene dependency graph.
- Verified Tools > Compress Assets opens the three-tab window. Captured and visually inspected the window and the Uncompressed Assets scanner using CLI Editor-window captures.
- Refreshed the scanner and invoked its read-only scans for textures, models and audio: zero uncompressed entries in each category in this empty project. No Optimize or Apply operation ran.
- AssetProcessing files and nested metadata exactly match the bundled copy. Every Assets file and folder has metadata; no duplicate asset GUIDs were found.
- Markdown local file links resolve outside illustrative fenced templates. No project-name placeholders or source-game system names remain in maintained documents.
- Preserved the complete shared architecture and art guidance, including mandatory Intro/Main/HUD, MCV/Zenject, typed SO balance folders, prefab reuse, authored scene/UI composition, responsive UI and build authorization rules.
- Unity caches, user settings, local IDE files, builds and asset-processing reports are excluded by .gitignore.

## Tool defaults and pending work

The compression tool defaults to a WebGL texture override with max size 2048, Automatic format and Compressed HQ; audio initially targets WebGL AAC. These are tool defaults, not a chosen project platform or applied asset changes. Review settings before use.

Intro/Main/HUD separation, loading/readiness, shared and scene-local lifetimes and cleanup remain mandatory pending implementation. Zenject and gameplay systems are absent. No gameplay balance assets, placeholder prefabs, approved GDD or art direction were invented.

Unity CLI reported that the Editor opened without a signed-in Unity account; the existing local license allowed import and verification. No account credentials were changed.

No player build, CI run, gameplay test or Play Mode validation was performed or required for this empty-project setup. Platform validation remains pending an explicit build request. CrazyGames documentation was omitted because no CrazyGames target was specified.
