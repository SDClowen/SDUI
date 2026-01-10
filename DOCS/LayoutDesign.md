# Layout Design — WinForms-like semantics for SDUI

## Goals
- Implement a Measure/Arrange layout pipeline similar to WinForms so controls size & position deterministically.
- Preserve current optimizations (deferred redraw, throttled animations) while matching WinForms behaviors for Dock/Anchor/AutoSize.
- Ensure DPI changes trigger re-measure/arrange so fonts and sizes match immediately.

## WinForms semantics (summary)
- Measure phase: control calculates desired size given an available size (does not commit). Controls should return their preferred size; containers query children.
- Arrange phase: parent assigns final bounds to each child based on measure results, Dock/Anchor settings, margins and padding.
- PerformLayout: kicks off an invalidation that causes Measure and Arrange to run when required. AutoSize controls call Invalidate/PerformLayout after content changes.
- Dock: anchors control to an edge of parent and allocates space accordingly.
- Anchor: keeps control distance to anchored edges on parent resize.

## Acceptance criteria
- When parent size changes, child measure/arrange runs and child bounds update according to Dock/Anchor rules.
- AutoSize controls recompute size on content change and trigger parent layout.
- Moving window between monitors (DPI change) triggers font cache invalidation, re-measure, and re-arrange so sizes update.
- No large regressions in benchmarked redraw/CPU usage (small layout changes should be cheap).

## Implementation plan (high level, files)
1. LayoutEngine.cs
   - Add MeasureChildren(availableSize) and ArrangeChildren(finalRect) helpers.
   - Make measure results cacheable per layout pass to avoid repeated measurements.
2. UIElementBase.cs
   - Add virtual Measure(Size available) and Arrange(Rect finalRect) methods (defaults: measure = current Size, arrange = set bounds).
   - Make PerformLayout call into LayoutEngine.Measure/Arrange and propagate to children.
   - Ensure AutoSize and AdjustSize call Invalidate/PerformLayout.
3. Controls
   - Update container controls (FlowLayoutPanel, Panel, MenuStrip, ContextMenuStrip) to override Measure/Arrange where they need custom behavior.
4. Tests & Demos
   - Add unit tests for Measure/Arrange results and a demo page to show Dock/Anchor/AutoSize behavior.

## Tests to add
- Measure/Arrange correctness for a simple nested container.
- AutoSize: changing text/font triggers size change and parent layout.
- Dock: left/right/top/bottom/Fill cases.
- Anchor on parent resize.
- DPI change triggers re-measure and fonts change size.

---

Next: implement basic Measure/Arrange API in `UIElementBase` and wiring in `LayoutEngine` as a first PR-sized change.
