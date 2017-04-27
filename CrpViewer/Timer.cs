using System;
using System.Diagnostics;

namespace CrpViewer {
    class Timer {
        private Stopwatch watch;

        private long frequency;

        private TimeSpan lastFrameTime;

        public Timer() {
            frequency = Stopwatch.Frequency;
            watch = new Stopwatch();
        }

        public void Start() {
            watch.Reset();
            watch.Start();
            lastFrameTime = watch.Elapsed;
        }
        public float Stop() {
            var timeSpan = watch.Elapsed - lastFrameTime;
            lastFrameTime = watch.Elapsed;
            watch.Stop();
            return (float)((double)timeSpan.Ticks / (double)frequency);
        }

        public float Restart() {
            var timeSpan = watch.Elapsed - lastFrameTime;
            lastFrameTime = watch.Elapsed;
            return (float)((double)timeSpan.Ticks / (double)frequency);
        }
    }
}
