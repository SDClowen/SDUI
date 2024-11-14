using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class WindowPageControl : UserControl
    {
        private EventHandler<int> _onSelectedIndexChanged;

        public event EventHandler<int> SelectedIndexChanged
        {
            add => _onSelectedIndexChanged += value;
            remove => _onSelectedIndexChanged -= value;
        }

        private int _selectedIndex = -1;
        public int SelectedIndex 
        { 
            get => _selectedIndex; 
            set 
            {
                var sys = Stopwatch.StartNew();

                if (_selectedIndex == value)
                    return;

                if (Controls.Count > 0)
                {
                    if (value < 0)
                        value = Controls.Count - 1;

                    if (value > Controls.Count - 1)
                        value = 0;
                }
                else
                    value = -1;

                var previousSelectedIndex = _selectedIndex;
                _selectedIndex = value;
                _onSelectedIndexChanged?.Invoke(this, previousSelectedIndex);

                for (int i = 0; i < Controls.Count; i++)
                    Controls[i].Visible = i == _selectedIndex;

                Debug.WriteLine($"Index: {_selectedIndex} Finished: {sys.ElapsedMilliseconds} ms");
            } 
        }

        public int Count => Controls.Count;

        public WindowPageControl() 
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer, true
            );

            //BackColor = Color.Transparent;
            UpdateStyles();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            e.Control.Dock = DockStyle.Fill;
            e.Control.BackColor = Color.Transparent;
            e.Control.Visible = Controls.Count == 1;

            if (Controls.Count == 1)
                _selectedIndex = 0;
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);

            if (Controls.Count == 0)
                _selectedIndex = -1;
        }
    }
}
