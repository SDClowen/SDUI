# SDUI
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FSDClowen%2FSDUI.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FSDClowen%2FSDUI?ref=badge_shield)

A fluently windows forms theme library.

## New: TreeView Control
A simple, stylish `TreeView` control has been added under `SDUI/Controls/TreeView.cs` with a supporting `TreeNode` model. It supports:

- Expand/collapse with a clean chevron triangle UI ✅
- Selection with keyboard support (arrow keys) and mouse clicks ✅
- Options: `FullRowSelect` (selects full row) and `HideSelection` (hide highlight when unfocused) ✅
- `NodeMouseClick`, `NodeMouseDoubleClick`, and `NodeMouseRightClick` events are available for node click handling ✅
- Icons: nodes support an optional `IconColor` for a small color icon in the tree ✅
- Multi-selection: select multiple nodes with Ctrl/Shift; use `SelectedNodes` and `SelectedNodesChanged` ✅
- Simple chevron rotation animation for expand/collapse is added ✅
- Demo page with sample data: `SDUI.Test/TreeViewPage.cs` (accessible from the main demo window) ✅

Usage example:
```csharp
var tv = new SDUI.Controls.TreeView();
var root = new SDUI.Controls.TreeNode("Root");
root.Nodes.Add(new SDUI.Controls.TreeNode("Child 1"));
root.Nodes.Add(new SDUI.Controls.TreeNode("Child 2"));
tv.Nodes.Add(root);
tv.SetSelectedNode(root);
```

## Layout compatibility (WinForms) ⚖️
SDUI's layout engine tries to mimic WinForms Dock/AutoSize/Anchor behavior to make porting easier. Key points:

- **Child controls are laid out in the same order they appear in the container's `Controls` collection (the Controls.Add / z-order), matching WinForms behavior**. This ensures controls that rely on add-order docking reproduce WinForms layouts.
- AutoSize controls docked to Top/Bottom/Left/Right use `GetPreferredSize` and respect `AutoSizeMode`, `MinimumSize` and `MaximumSize` when sizing.
- Anchored controls keep their distances to the anchored edges and resize when both opposite anchors are set (e.g., Left+Right or Top+Bottom).

Demo scenarios added (open Demo app and choose "Layout Compatibility"):

- **Min/Max enforcement**: AutoSize controls with `MinimumSize`/`MaximumSize` to verify enforced bounds.
- **Left/Right stacked AutoSize**: Multiple Left/Right docked AutoSize controls to verify stacking and width behavior.
- **Nested Dock + Anchor combos**: Panels nested with mixed docking and an anchored control to verify complex resize semantics.

If you rely on unit tests, please note: we run layout verification through the demo app and interactive manual tests. If you see a mismatch with WinForms, open an issue with repro steps.


## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FSDClowen%2FSDUI.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FSDClowen%2FSDUI?ref=badge_large)