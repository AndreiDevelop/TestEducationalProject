# Art Direction Guidelines

This is a portable production baseline. Preserve the destination project's approved art direction and references; do not copy another game's palette, characters or art assets as universal defaults. Keep actual palette values and reference measurements in the target's authoritative art guide.

## Art documentation and reference storage

Save new art documentation and its actual reference images under the target project's `Documentation/Art/`. Group images by category and subject, with related views in one folder, such as `Characters/<Skin>/turnaround.png` and `preview-3q.png`. Keep documentation concepts separate from imported Unity assets. Follow [Documentation Guidelines](../DOCUMENTATION_GUIDELINES.md#location-and-authority) and the [GDD concept maintenance rules](../GDD/GDD_BEST_PRACTICES.md#concept-art-storage-and-freshness); update documents, galleries and image links together.

## Authoring visual styles

Follow [authored visual styling and Editor tools](../Engineering/CODE_STYLE_AND_ARCHITECTURE.md#authored-visual-styling-and-editor-tools). Store production appearance in shared prefabs, materials, font presets, theme/style assets or UXML/USS, with scene overrides only where needed. Editor scripts must not be the source of palettes, typography, spacing or layout, or overwrite authored styling on load, import or setup. Reuse authored assets and preserve their references when making visual changes. Runtime scripts must not rebuild or restyle UI or overwrite its authored geometry/layout unless explicitly requested by the user; keep ordinary data and state presentation separate.

## Responsive UI and text

Follow the mandatory [UI anchors, text sizing and raycast targets](../Engineering/CODE_STYLE_AND_ARCHITECTURE.md#ui-anchors-text-sizing-and-raycast-targets): author stretched panels/content roots with parent-relative anchors and padding, attach individual elements/groups to their intended corner/edge/center with local offsets, use explicit TMP font sizes with Auto Size disabled, and raycast targets only for required interaction or intentional blockers. Group related elements in meaningful authored containers, with correct anchors on both the containers and their children; avoid a flat collection of unrelated siblings under Canvas/Safe Area. Combine responsive anchors with the sliced backgrounds below; sprite slicing alone does not make layout responsive.

## Rounded UI geometry

- New UI buttons, panels, cards, dialogs and similar containers use genuinely rounded corners by default. Use the destination project's established rounded component family and record its radius and outline at the intended display scale. An explicitly requested different silhouette takes precedence; an existing angled or square placeholder is not the default for new UI. Apply changes to existing screens only within the requested scope.
- Preserve the corner shape as a component resizes. In uGUI, use a correctly bordered 9-sliced sprite or suitable native geometry; set Image type, sprite PPU, pixels-per-unit multiplier and minimum RectTransform dimensions together so borders never overlap or collapse. Keep corner caps outside the stretchable center and allow content padding inside the outline. Do not stretch an entire decorated sprite into another aspect ratio.
- In UI Toolkit, author consistent corner radii and clip child content where needed; rounded decoration alone must not leave square child backgrounds protruding. Other frameworks should use their equivalent rounded geometry and clipping controls.
- Keep interaction states readable in the project's palette. Disabled controls must remain visibly distinct. Do not unintentionally make their background translucent and let a differently colored parent change their apparent color. Preserve the intended rounded silhouette in every state.
- Verify representative controls at their smallest and largest supported sizes and Canvas scales, including enabled, highlighted, pressed and disabled states over light and dark parents. Inspect corners, outlines, alpha fringes, atlas sampling and text padding in the actual UI. Repair the slicing or replace unsuitable artwork when it produces pinched corners, seams, streaks or neighboring atlas pixels. This default does not prescribe one universal radius, palette or texture size for every project.

### Unity uGUI: correct 9-slice setup

Follow [UI layout, spacing and resize verification](../Engineering/CODE_STYLE_AND_ARCHITECTURE.md#ui-layout-spacing-and-resize-verification). Repair Borders and Sliced settings on existing sprites/Images; do not create asset or runtime sprite copies solely to fix stretching. Verify other consumers when changing a shared sprite.

- Resizable sprite-backed button, panel and card backgrounds use `Image.Type.Sliced` with an authored nonzero `Sprite.border`. Keep each rounded corner, outline and bevel entirely inside its fixed corner regions; stretch only the intended edge/center regions. Icons and non-resizable illustrations do not acquire borders merely because they are UI.
- Set sprite PPU, Canvas reference PPU and `Image.pixelsPerUnitMultiplier` deliberately. Choose the smallest RectTransform dimensions so opposing borders still fit and a center region remains; do not rely on Unity collapsing overlapping borders to disguise unsuitable artwork. Radius and stroke must stay visually consistent across the intended sizes.
- Use a full rectangular sprite mesh for the sliced background. Let the RectTransform control its aspect ratio (`preserveAspect = false`); keep the center filled unless the design is intentionally hollow. Padding belongs to the content/label layout and must keep text and its outline inside the rounded frame.
- Keep unrelated atlas pixels and transparent edge fringes out of the sliced sprite. Check source bounds, wrap/filter behavior, atlas padding/extrusion and compression at actual scale. Preserve unrelated import settings; repair the offending sprite or its import/packing configuration when an artifact is demonstrated.
- Verify both the smallest ordinary control and a larger/alternate-aspect control after import/packing, including disabled and pressed states. Reject flattened or pinched corners, doubled borders, seams, long edge streaks and unintended parent-color bleed-through. Native/vector/shader UI geometry uses its equivalent resize-safe rounded rendering and clipping instead of artificial sprite slicing.

## Compact spacing and visual grouping

- Use the project's authored TMP sprite glyph for every player-facing currency or resource amount. The glyph comes first, before the number and any related label or action text; do not spell out the currency name in its place or put the glyph after the number. Keep the glyph and copy in one compact group.
- Keep related values together: a currency icon and its amount form one compact group with a small consistent gap. Anchor that group as a unit; do not position the icon and number at separate percentage columns that drift apart on wider cards.
- Size popups to their actual content. Avoid oversized empty headers, large unused margins and excessive gaps between rows. Keep deliberate breathing room for readability and touch targets, with smaller gaps within a group than between independent groups.
- Choose padding and spacing from the destination project's UI scale and shared component family. Reserve error/status space only as needed; do not introduce empty placeholder areas merely to fill a large panel.
- Check both the largest and smallest supported card widths: icon/amount pairs must still read as one value, text must not clip and popup padding must remain proportionate. Preserve rounded corners, shared prefabs and clear interaction bounds while tightening layout.

## Icon families and outlines

- Group comparable icons by their visual role and intended displayed size: equipment tiers, resources, HUD symbols and portraits may have distinct line hierarchies. Within one family, match contour color, visible thickness, cap/join character, highlight treatment and the relationship between outer contour and internal detail.
- Compare at equal intended screen size in Unity, after import and atlas packing. Source-pixel widths and PNG dimensions alone do not establish equal line weight. Record the reference display size and chosen thickness/tolerance per family; inspect representative small/mobile sizes as well.
- Keep tier/variant silhouettes recognizable while standardizing outlines. Do not add a second halo, change framing, grow transparent silhouettes or change sprite pivots/slicing merely to match line weight. Preserve intentional smaller internal lines. Use a shared master/reference and preserve its ink when adding color.
- Color-only reskins retain silhouettes, transparent alpha, composition, component dimensions, text, navigation, raycasts, data bindings and behavior. Match palette roles and state colors to approved references. Simple glyphs and text stay crisp; texture must not obscure them.
- Inspect icons together on light and dark backgrounds, in selected, locked and disabled states and at actual UI scale. Check alpha fringes, clipping, apparent size and outline consistency after packing. Record actual visual checks rather than claiming uniformity from a generation prompt.

## Import invariants

Preserve existing source dimensions, GUIDs, sprite IDs/rectangles, PPU, pivots, borders, filtering, mipmaps, compression format/quality/Crunch and platform overrides during a color-only reskin unless the user explicitly authorizes changing them. Preserve atlas packing and compression settings as well. New assets match the closest existing role. A palette change does not authorize a compression or resolution optimization.

## Scope and evidence

Use original or authorized artwork. Distinguish concept previews from verified runtime captures. Document project-specific style decisions in the existing art/UI guide and link the relevant evidence. Installing these rules does not generate art, rebuild UI or implement a rendering effect.

## Editable scene composition

Follow [authored scene content](../Engineering/CODE_STYLE_AND_ARCHITECTURE.md#authored-scene-content). Permanent layout must be visible and editable as prefab instances outside Play Mode; preserve authored placement and prefab connections. Reread current project and visual rules before implementing visual changes.
