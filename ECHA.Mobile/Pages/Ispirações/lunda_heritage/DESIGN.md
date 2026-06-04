```markdown
# Design System Document: The Sovereign Archive

## 1. Overview & Creative North Star
**Creative North Star: "The Digital Curator"**

This design system moves away from the "generic ed-tech" aesthetic of bright bubbles and flat grids. Instead, it adopts the persona of a high-end digital archive—a space where Angolan history and economic data are treated with the reverence of a museum and the precision of a modern fintech interface. 

We break the "template" look through **Intentional Asymmetry**. By utilizing the stylized map of Angola as a recurring, deconstructed graphic element, we create a sense of movement. Layouts should feel editorial, utilizing exaggerated whitespace and high-contrast typography to guide the user through complex narratives. This is not just an app; it is a premium pedagogical journey.

---

## 2. Colors & Surface Philosophy
The palette is rooted in the "Deep Burgundy" of Angolan heritage, contrasted against a sophisticated "Professional Grey" to maintain a techno-impact feel.

### The "No-Line" Rule
**Strict Mandate:** Designers are prohibited from using 1px solid borders to section content. 
Boundaries must be defined solely through:
*   **Background Shifts:** Transitioning from `surface` to `surface-container-low`.
*   **Tonal Transitions:** Using subtle shifts in the grey scale to indicate a change in context.

### Surface Hierarchy & Nesting
Treat the UI as a series of physical layers. Use the `surface-container` tiers to create "nested" depth:
*   **Base:** `surface` (#f9f9f9) for the primary screen background.
*   **Content Areas:** `surface-container-low` (#f3f3f3) for secondary content blocks.
*   **Interactive Cards:** `surface-container-lowest` (#ffffff) to make interactive elements "pop" forward naturally.

### The "Glass & Gradient" Rule
To ensure a premium feel, floating headers or navigation bars should utilize **Glassmorphism**:
*   **Values:** `surface` color at 80% opacity with a `20px` backdrop-blur.
*   **Signature Textures:** Apply a subtle linear gradient to main CTAs using `primary` (#570013) to `primary_container` (#800020) at a 135-degree angle. This adds "soul" and prevents the burgundy from feeling flat or dated.

---

## 3. Typography
We utilize a dual-typeface system to balance historical weight with modern legibility.

*   **Display & Headlines (Manrope):** Chosen for its geometric, modern structure. Use `display-lg` to `headline-sm` for chapter titles and key economic data points. These should be set with tight letter-spacing (-0.02em) to feel authoritative.
*   **Body & Labels (Public Sans):** A neutral, highly accessible sans-serif. Used for all educational content and data labels. It ensures that even long-form historical text remains legible on small mobile screens.
*   **The Narrative Scale:** Use `title-lg` in `on_surface_variant` (#584141) for lead-ins to body text, creating a sophisticated editorial hierarchy that feels like a published book.

---

## 4. Elevation & Depth
Depth is achieved through **Tonal Layering**, mimicking the way light hits fine paper.

*   **The Layering Principle:** Place a `surface-container-lowest` card on top of a `surface-container` background. This creates a soft, natural lift without the "dirtiness" of heavy shadows.
*   **Ambient Shadows:** If an element must float (e.g., a Bottom Sheet), use a shadow with a blur of `32px`, an Y-offset of `8px`, and a color of `on_surface` at only **6% opacity**. This mimics natural ambient light.
*   **The "Ghost Border":** For accessibility in high-density data views, use the `outline_variant` (#e0bfbf) at **15% opacity**. Never use a 100% opaque border.
*   **Stylized Map Integration:** The map of Angola should exist on the `surface_dim` or `surface_variant` layer, appearing as a subtle watermark behind content to reinforce the "Territorial" value without distracting from text.

---

## 5. Components

### Buttons
*   **Primary:** Gradient of `primary` to `primary_container`. Roundedness: `md` (0.375rem). High-contrast `on_primary` text.
*   **Secondary:** `surface_container_highest` background with `on_surface` text. No border.
*   **Tertiary:** Text-only in `primary` weight, used for "Read More" or "View Map" actions.

### Cards & Lists
*   **The Divider Forbiddance:** No horizontal lines. Separate list items using `16px` of vertical white space or by alternating background tones between `surface` and `surface-container-low`.
*   **Educational Cards:** Use `surface-container-lowest` with a `xl` (0.75rem) corner radius for a soft, modern feel.

### Input Fields
*   **Styling:** Filled style using `surface_container_high`. 
*   **Interaction:** On focus, the bottom "indicator" line animates in `primary` color. No full-box stroke.

### Specialized Component: The "Timeline Node"
*   A custom vertical progress indicator for historical modules. Uses `tertiary` (#002c36) for completed states and `outline_variant` for upcoming states, symbolizing the connection between past and future.

---

## 6. Do's and Don'ts

### Do:
*   **Use Asymmetric Margins:** Give the right side of the screen slightly more breathing room (e.g., 24px left, 32px right) to evoke a premium magazine layout.
*   **Leverage White Space:** Allow the `background` (#f9f9f9) to "leak" between content blocks to create a feeling of technological impact and cleanliness.
*   **Animate Transitions:** Use subtle "fade and slide" transitions (200ms) when moving between economic eras.

### Don't:
*   **Don't use pure black:** Use `on_surface` (#1a1c1c) for all text to maintain professional softness.
*   **Don't clip the map:** The stylized map of Angola should feel like it belongs to the background; never box it into a square container. Let its edges be organic.
*   **Don't use standard shadows:** Avoid the default "Material Design" shadows. Always use the Ambient Shadow formula specified in Section 4.

---

## 7. Accessibility
All `primary` text on `surface` backgrounds must maintain a contrast ratio of at least 7:1. Public Sans is the primary vehicle for accessibility; never use Manrope for body text or fine print, as its geometric nature can fatigue the eye in long-form educational reading.```