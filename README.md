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


## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FSDClowen%2FSDUI.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FSDClowen%2FSDUI?ref=badge_large)