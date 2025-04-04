using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDUI.Animation
{
    public static class AnimationEngineProvider
    {
        private static readonly List<WeakReference<AnimationEngine>> _weakAnimationEngines = new();
        private static readonly System.Windows.Forms.Timer timer;
        private static readonly object _lock = new object();
        private const int TARGET_FPS = 60;

        static AnimationEngineProvider() 
        {
            timer = new System.Windows.Forms.Timer
            {
                Interval = 1000 / TARGET_FPS
            };

            timer.Tick += OnTimerTick;
            timer.Start();
        }

        public static void Handle(AnimationEngine animationEngine)
        {
            lock (_lock)
            {
                _weakAnimationEngines.Add(new WeakReference<AnimationEngine>(animationEngine));
                CleanupDeadReferences();
            }
        }

        private static void OnTimerTick(object sender, EventArgs e)
        {
            lock (_lock)
            {
                for (int i = _weakAnimationEngines.Count - 1; i >= 0; i--)
                {
                    if (_weakAnimationEngines[i].TryGetTarget(out var engine))
                    {
                        if (engine.Running)
                            engine.AnimationTimerOnTick(sender, e);
                    }
                    else
                    {
                        _weakAnimationEngines.RemoveAt(i);
                    }
                }
            }
        }

        private static void CleanupDeadReferences()
        {
            for (int i = _weakAnimationEngines.Count - 1; i >= 0; i--)
            {
                if (!_weakAnimationEngines[i].TryGetTarget(out _))
                    _weakAnimationEngines.RemoveAt(i);
            }
        }
    }
}
