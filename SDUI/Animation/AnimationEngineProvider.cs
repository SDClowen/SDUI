using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SDUI.Animation
{
    public static class AnimationEngineProvider
    {
        private static readonly List<AnimationEngine> animationEngines = new();
        private static readonly Timer timer;
        static AnimationEngineProvider()
        {
            timer = new Timer { Interval =1000 /60 }; // ~16ms
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        public static void Handle(AnimationEngine animationEngine)
        {
            animationEngines.Add(animationEngine);
        }

        private static void OnTimerTick(object sender, EventArgs e)
        {
            bool anyRunning = false;
            for (int i =0; i < animationEngines.Count; i++)
            {
                var engine = animationEngines[i];
                if (engine.Running)
                {
                    anyRunning = true;
                    engine.AnimationTimerOnTick(sender, e);
                }
            }

            if (!anyRunning)
            {
                // No animations active; throttle timer to reduce CPU.
                timer.Interval =250; // sleep longer
            }
            else if (timer.Interval !=1000 /60)
            {
                // Restore responsive interval when animations start again.
                timer.Interval =1000 /60;
            }
        }
    }
}
