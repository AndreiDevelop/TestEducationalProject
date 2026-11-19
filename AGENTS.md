# Project instructions

## Mandatory rule reread before implementation

Before each feature implementation or code change, reread the current applicable AGENTS.md files, the full authoritative code style/architecture guide, and the task-relevant documentation and invoked skills from disk. Do this again when resuming implementation after new user instructions or context compaction; earlier reading, memory and a conversation summary are not substitutes. Inspect current code, callers and serialized references before choosing an approach. Resolve any conflict in favor of explicit user instructions, and apply the rules before writing code rather than checking them only afterward.


Read and follow [Code Style and Architecture](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md) before changing project-owned code.
Read and follow [Documentation Guidelines](Documentation/DOCUMENTATION_GUIDELINES.md) when creating or updating documentation.
Read and follow [GDD Best Practices](Documentation/GDD/GDD_BEST_PRACTICES.md) when creating or updating game design specifications.
For generated GDD concepts, follow [concept-art storage and freshness](Documentation/GDD/GDD_BEST_PRACTICES.md#concept-art-storage-and-freshness): save images locally under `Documentation/Art/` by subject, link them from the relevant design specification, and keep images, galleries and links current together.


## Documentation and art storage

- Resolve the actual target project root before saving deliverables. A chat workspace, temporary folder, generator cache or `output/` directory is not the project root merely because it is the current working directory.
- Keep maintained project documents under that project's `Documentation/`, using its `Engineering/`, `GDD/` and `Art/` responsibilities. Root entry points such as `AGENTS.md` and `README.md` remain at the root.
- Keep new art direction documents, visual references, concept art and documentation previews under `Documentation/Art/`. Group artwork by subject, for example `Documentation/Art/Characters/<Skin>/`, with turnarounds and preview views together. Use relative links from the art guide and relevant GDD specification.
- Reuse the existing authority and update image files, captions, manifests, galleries and links together. Do not leave maintained art documentation or its only images in a separate root-level `Art/`, chat folder or generator cache. Production assets imported into Unity still belong under `Assets/`.

## Shared architecture baseline

Follow the mandatory [shared architecture requirements](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#shared-architecture-requirements): MCV with model injection into Views and Controllers through Zenject, separate Intro/Main/HUD scene responsibilities, SO configuration, type-first logical asset ownership and typed SO folders, and recursive subsystem/role grouping only for groups of at least two files. Preserve these rules across project integrations; record legacy implementation gaps separately. Only an explicit user instruction changes a shared requirement.

Follow [SOLID, KISS, DRY and YAGNI](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#solid-kiss-dry-and-yagni) and [cohesive UI components](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#cohesive-ui-components): one cohesive responsibility per class, focused child Views for independent screen blocks, and no speculative abstractions or duplicated ownership.

Follow [UI hierarchy and panel ownership](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#ui-hierarchy-and-panel-ownership) for platform branches, panel `Body` ownership, Views, Controllers and Models.

Follow [authored visual styling and Editor tools](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#authored-visual-styling-and-editor-tools): keep game appearance in authored assets; avoid Editor scripts that hard-code or regenerate styling. Runtime UI layout/style or hierarchy changes require an explicit user request; ordinary data/state updates remain UI behavior.

Follow [UI anchors, text sizing and raycast targets](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#ui-anchors-text-sizing-and-raycast-targets): stretch panel/content anchors instead of fixed-resolution center-anchored wrappers, group related elements in meaningful containers with correct parent-relative anchors and attach individual elements/groups to the corresponding corner/edge/center, prohibit TMP Auto Size, and enable Raycast Target only where input or deliberate blocking requires it.

Read [Art Direction Guidelines](Documentation/Art/ART_DIRECTION_GUIDELINES.md) for visual work, rounded UI defaults, compact spacing and icon-family outline consistency. Preserve the project's approved style and import/compression settings. Follow the [currency and resource glyph rule](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#presentation-and-runtime-cost): use authored TMP glyphs before player-facing amounts and related UI text instead of currency-name words.

## UI layout and verification

Follow [UI layout, spacing and resize verification](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#ui-layout-spacing-and-resize-verification): preserve consistent padding and gaps, use correct parent-relative anchors/layout, and verify text fit, overlaps and rounded corners at representative supported screen sizes. Repair Borders and Sliced settings on existing sprites and Images; do not create sprite copies to fix stretching. Check other consumers of shared sprites and preserve unrelated import settings.

## Existing project guidance

- Preserve existing project-specific instructions and approved visual references when applying this template. Add links to the project's actual art guide when available; do not import another game's art direction.

## Builds

Create a player/platform build only when the user explicitly requests a build; ordinary implementation or verification does not authorize local or CI-triggered builds.

## Unity workflow preferences

- Prefer Unity CLI for every Unity task it can perform, including inspecting the running Editor, importing and configuring assets, editing scenes and Animator controllers, executing editor C#, and validating changes.
- Follow the unity:unity-cli skill when available. Target the intended project explicitly with --project-path and include the caller/skill flags required by the skill.
- Use Unity MCP when a required capability is unavailable through CLI or CLI remains unusable after a focused diagnosis. Use UI automation for necessary visual checks or operations unavailable through CLI/MCP.
- A timeout does not prove that an operation was cancelled. Inspect the resulting state before retrying a mutation to avoid duplicate assets or changes.
- Minimize token consumption: use reusable scripts, batch related work where appropriate, and return short summaries and relevant diagnostics. Save full reports locally instead of printing large scene hierarchies, asset lists, or logs.
- Ordinary file reading, source editing, and version control may use standard filesystem and shell tools when these are more direct. Prefer Unity APIs through CLI for changes to Unity-serialized assets.
- Verify the actual result before reporting completion. Explain briefly when falling back from CLI.

## Permanent content in scenes

Follow [authored scene content](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#authored-scene-content): permanent level objects are editable prefab instances in the owning scene, not runtime-generated layout. Initialize existing references and restore visibility/progression without overwriting placement or creating duplicates.
