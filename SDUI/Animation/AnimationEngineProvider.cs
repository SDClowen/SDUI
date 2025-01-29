using System;
using System.Collections.Generic;

namespace SDUI.Animation
{
    public static class AnimationEngineProvider
    {
        private static List<AnimationEngine> animationEngines = new();
        private static System.Windows.Forms.Timer timer;
        static AnimationEngineProvider()
        {
            timer = new System.Windows.Forms.Timer
            {
                Interval = 1000 / 60
            };

            timer.Tick += onTimerTick;
            timer.Start();
        }

        public static void Handle(AnimationEngine animationEngine)
        {
            animationEngines.Add(animationEngine);
        }

        private static void onTimerTick(object sender, EventArgs e)
        {
            foreach (var animationEngine in animationEngines)
                if (animationEngine.Running)
                    animationEngine.AnimationTimerOnTick(sender, e);
        }
    }
}
