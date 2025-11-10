using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SDUI.Animation
{
    public static class AnimationEngineProvider
    {
        private static readonly List<AnimationEngine> animationEngines = new();
        private static readonly Timer timer;
        private static int _targetFps = 120;
        
        static AnimationEngineProvider()
        {
            timer = new Timer { Interval = 1000 / _targetFps };
            timer.Tick += OnTimerTick;
        }

        public static void Handle(AnimationEngine animationEngine)
        {
            animationEngines.Add(animationEngine);
        }

        internal static void Wake()
        {
            if (!timer.Enabled)
            {
                timer.Interval = 1000 / _targetFps;
                timer.Start();
            }
        }

        private static void OnTimerTick(object sender, EventArgs e)
        {
            bool anyRunning = false;
            for (int i = 0; i < animationEngines.Count; i++)
            {
                var engine = animationEngines[i];
                if (engine.Running)
                {
                    anyRunning = true;
                    engine.AnimationTimerOnTick(sender, e);
                }
            }

            if (!anyRunning && timer.Enabled)
            {
                timer.Stop();
            }
        }
    }
}
