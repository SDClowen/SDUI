using System;
using System.Collections.Generic;

namespace SDUI.Designer.Commands;

/// <summary>
/// Manages undo/redo operations using Command pattern
/// </summary>
public class UndoRedoManager
{
    private readonly Stack<DesignCommand> _undoStack = new();
    private readonly Stack<DesignCommand> _redoStack = new();
    private const int MaxHistorySize = 100;

    public event EventHandler? StateChanged;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public string? UndoDescription => CanUndo ? _undoStack.Peek().Description : null;
    public string? RedoDescription => CanRedo ? _redoStack.Peek().Description : null;

    public void ExecuteCommand(DesignCommand command)
    {
        command.Execute();
        
        _undoStack.Push(command);
        _redoStack.Clear();
        
        // Limit history size
        if (_undoStack.Count > MaxHistorySize)
        {
            var temp = new Stack<DesignCommand>();
            for (int i = 0; i < MaxHistorySize; i++)
            {
                temp.Push(_undoStack.Pop());
            }
            _undoStack.Clear();
            while (temp.Count > 0)
            {
                _undoStack.Push(temp.Pop());
            }
        }
        
        OnStateChanged();
    }

    public void Undo()
    {
        if (!CanUndo) return;

        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        
        OnStateChanged();
    }

    public void Redo()
    {
        if (!CanRedo) return;

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        
        OnStateChanged();
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        OnStateChanged();
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
