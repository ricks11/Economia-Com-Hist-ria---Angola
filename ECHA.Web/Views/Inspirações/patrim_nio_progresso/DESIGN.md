```markdown
# Design System Document: Educational Heritage & Economic Narrative

## 1. Overview & Creative North Star: "The Modern Archivist"
This design system departs from the sterile, "template-driven" look of standard educational platforms. Our Creative North Star is **"The Modern Archivist."** 

We are not just presenting data; we are curating the economic history of Angola. The aesthetic combines the weight of historical significance with the clarity of a high-end editorial journal. To achieve this, the design system utilizes **intentional asymmetry**, **tonal layering**, and **expansive whitespace**. We move away from boxes and lines toward a fluid, paper-on-paper experience that feels prestigious yet deeply accessible to the academic community.

---

## 2. Colors: Tonal Depth & Soul
The palette is rooted in the rich, authoritative tones of Bordeaux and Gold, balanced by a sophisticated grayscale that provides "breathing room."

### Core Palette
*   **Primary (`#6B1E1E`):** The "Heritage Red." Used for high-impact headers and primary actions. Use a subtle gradient transition to `primary_container` (`#4D070B`) in large sections to add "soul" and avoid a flat, digital-only feel.
*   **Tertiary/Accent (`#D4AF37`):** The "Soft Gold." Reserved for secondary buttons and curated highlights. It represents the value and wealth of history.
*   **Neutral Surfaces (`#F9F9F9` to `#EEEEEE`):** These are the foundation of our UI.

### The "No-Line" Rule
**Explicit Instruction:** Designers are prohibited from using 1px solid borders for sectioning or containment. Boundaries must be defined solely through background color shifts. 
*   *Example:* A `surface_container_low` (`#F3F3F3`) sidebar sitting against a `surface` (`#F9F9F9`) background.

### Surface Hierarchy & Nesting
Treat the UI as a series of stacked, fine paper sheets. 
*   **Level 0:** `surface` – The base floor of the application.
*   **Level 1:** `surface_container_low` – Inset areas or secondary navigation.
*   **Level 2:** `surface_container` – Standard card backgrounds.
*   **Level 3:** `surface_container_high` – Elements that require immediate focus or interaction.

---

## 3. Typography: Editorial Authority
We utilize a dual-typeface approach to distinguish between "The Narrative" (History) and "The Data" (Economy).

*   **Display & Headlines (Manrope):** A sophisticated, wide-set sans-serif that commands attention. 
    *   *Usage:* Use `display-lg` (3.5rem) for hero titles. Ensure tight letter-spacing (-0.02em) to maintain an authoritative, editorial look.
*   **Body & Titles (Public Sans):** Chosen for its exceptional legibility in long-form educational content.
    *   *Usage:* `body-lg` (1rem) is the standard for lesson content. Always ensure a line-height of 1.6 to prevent eye fatigue for students.
*   **Scale Hierarchy:** The massive contrast between `display-lg` and `body-sm` creates a rhythmic visual interest that breaks the "standard web" feel.

---

## 4. Elevation & Depth: Tonal Layering
Traditional shadows are often too "heavy" for an academic context. We achieve depth through subtle light physics.

*   **The Layering Principle:** Instead of a shadow, place a `surface_container_lowest` (#FFFFFF) card on a `surface_container` (#EEEEEE) background. This creates a soft, natural lift.
*   **Ambient Shadows:** For floating modals or primary CTAs, use an "Extra-Diffused" shadow:
    *   *Values:* `Y: 8px, Blur: 24px, Spread: -4px`.
    *   *Color:* Use `on_surface` at 6% opacity. Never use pure black.
*   **Glassmorphism:** To elevate the experience, use `surface_container_lowest` at 80% opacity with a `20px` backdrop blur for navigation headers. This allows the bordeaux and gold accents to "bleed" through as the user scrolls, creating a sense of continuity.
*   **The "Ghost Border" Fallback:** If a border is required for accessibility, use `outline_variant` at 15% opacity. It should be felt, not seen.

---

## 5. Components: Intentional Interaction

### Cards & Lists
*   **The 20px Rule:** All cards must use a `1.5rem` (xl) or `1.25rem` corner radius.
*   **No Dividers:** Forbid the use of horizontal lines. Separate list items using `0.75rem` of vertical whitespace and a subtle background shift on hover (`surface_container_high`).

### Buttons
*   **Primary:** `primary` background with `on_primary` (White) text. Heavy rounded corners (full).
*   **Secondary (Soft Gold):** Use the `tertiary_container` (`#CCA830`) background. This makes the button feel like a curated choice rather than a secondary thought.
*   **States:** On hover, do not change color brightness; instead, apply a "Lift" effect (a soft ambient shadow and a -2px Y-axis shift).

### Educational Chips
*   **Contextual Tags:** Use `secondary_container` with `on_secondary_container` text. These should be "Flat" (no shadow) to remain secondary to the main narrative.

### Input Fields
*   **Style:** Minimalist. No bottom line or box. Use a `surface_container_low` background with a `20px` border-radius.
*   **Focus:** Transition the background to `surface_container_lowest` and apply a "Ghost Border" of `primary` at 20%.

---

## 6. Do’s and Don’ts

### Do
*   **Do** use asymmetrical layouts. A large headline on the left with body text offset to the right creates a "designed" editorial feel.
*   **Do** use the Bordeaux (`primary`) sparingly for "Signposts"—guiding the student's eye to the most important historical fact or economic figure.
*   **Do** prioritize whitespace. If a layout feels "crowded," remove a container background before removing text.

### Don’t
*   **Don't** use standard "Drop Shadows." They make the interface look like a generic app from 2015.
*   **Don't** use dividers or lines to separate content. Use the Spacing Scale (16px, 24px, 32px) to create "invisible" sections.
*   **Don't** use pure black for text. Use `on_surface` (`#1A1C1C`) to maintain a softer, more premium contrast against the light gray backgrounds.
*   **Don't** use sharp corners. Everything must feel approachable and organic, echoing the "Human" side of history.

---

*This design system is a living framework. It is intended to evolve, but the core principles of Tonal Layering and Editorial Authority must remain the foundation of every screen.*```