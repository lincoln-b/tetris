//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;

namespace DigitalRubyShared
{
    /// <summary>
    /// A scale gesture detects two fingers moving towards or away from each other to scale something
    /// </summary>
    public class ScaleGestureRecognizer : GestureRecognizer
    {
        private const float minimumScaleResolutionSquared = 1.005f;
        private const float stationaryScaleResolutionSquared = 1.05f;
        private const float stationaryTimeSeconds = 0.1f; // if stationary for this long, use stationaryScaleResolutionSquared else minimumScaleResolutionSquared
        private const float hysteresisScaleResolutionSquared = 1.1f; // higher values resist scaling in the opposite direction more

        private float previousDistance;
        private float previousDistanceDirection;
        private float previousDistanceX;
        private float previousDistanceY;
        private float initialDistance;
        private float initialDistanceX;
        private float initialDistanceY;

        private readonly System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        public ScaleGestureRecognizer()
        {
            ScaleMultiplier = ScaleMultiplierX = ScaleMultiplierY = 1.0f;
            ZoomSpeed = 3.0f;
            ThresholdUnits = 0.15f;
            MinimumNumberOfTouchesToTrack = MaximumNumberOfTouchesToTrack = 2;
            timer.Start();
        }

        private void UpdateCenter(float distance, float distanceX, float distanceY)
        {
            previousDistance = distance;
            previousDistanceX = distanceX;
            previousDistanceY = distanceY;
        }

        private void ProcessTouches()
        {
            CalculateFocus(CurrentTrackedTouches);

            if (!TrackedTouchCountIsWithinRange)
            {
                return;
            }

            float distance = DistanceBetweenPoints(CurrentTrackedTouches[0].X, CurrentTrackedTouches[0].Y, CurrentTrackedTouches[1].X, CurrentTrackedTouches[1].Y);
            float distanceX = Distance(CurrentTrackedTouches[0].X - CurrentTrackedTouches[1].X);
            float distanceY = Distance(CurrentTrackedTouches[0].Y - CurrentTrackedTouches[1].Y);

            if (State == GestureRecognizerState.Possible)
            {
                if (previousDistance == 0.0f)
                {
                    previousDistance = distance;
                    previousDistanceX = distanceX;
                    previousDistanceY = distanceY;
                }
                else
                {
                    float diff = Math.Abs(previousDistance - distance);
                    if (diff >= ThresholdUnits)
                    {
                        initialDistance = distance;
                        initialDistanceX = distanceX;
                        initialDistanceY = distanceY;
                        UpdateCenter(distance, distanceX, distanceY);
                        SetState(GestureRecognizerState.Began);
                    }
                }
            }
            else if (State == GestureRecognizerState.Executing)
            {
                if (distance != previousDistance)
                {
                    float jitterThreshold = (float)timer.Elapsed.TotalSeconds <= stationaryTimeSeconds ? minimumScaleResolutionSquared : stationaryScaleResolutionSquared;
                    float currentDistanceSquared = distance * distance;
                    float previousDistanceSquared = previousDistance * previousDistance;
                    if ((currentDistanceSquared - previousDistanceSquared) * previousDistanceDirection < 0.0f)
                    {
                        jitterThreshold = Math.Max(jitterThreshold, hysteresisScaleResolutionSquared);
                    }
                    bool aboveJitterThreshold = ((previousDistanceSquared > jitterThreshold * currentDistanceSquared) ||
                        (currentDistanceSquared > jitterThreshold * previousDistanceSquared));
                    if (aboveJitterThreshold)
                    {
                        timer.Reset();
                        timer.Start();
                        ScaleMultiplier = Math.Max(0.1f, 1.0f + (ZoomSpeed * ((distance - previousDistance) / initialDistance)));
                        ScaleMultiplierX = Math.Max(0.1f, 1.0f + (ZoomSpeed * ((distanceX - previousDistanceX) / initialDistanceX)));
                        ScaleMultiplierY = Math.Max(0.1f, 1.0f + (ZoomSpeed * ((distanceY - previousDistanceY) / initialDistanceY)));
                        previousDistance = distance;
                        previousDistanceX = distanceX;
                        previousDistanceY = distanceY;
                        previousDistanceDirection = Math.Sign(currentDistanceSquared - previousDistanceSquared);
                        SetState(GestureRecognizerState.Executing);
                    }
                }
            }
            else if (State == GestureRecognizerState.Began)
            {
                SetState(GestureRecognizerState.Executing);
            }
            else
            {
                SetState(GestureRecognizerState.Possible);
            }
        }

        protected override void TouchesBegan(System.Collections.Generic.IEnumerable<GestureTouch> touches)
        {
            previousDistance = 0.0f;
        }

        protected override void TouchesMoved()
        {
            ProcessTouches();
        }

        protected override void TouchesEnded()
        {
            if (State == GestureRecognizerState.Executing)
            {
                CalculateFocus(CurrentTrackedTouches);
                SetState(GestureRecognizerState.Ended);
            }
            else
            {
                // didn't get to the executing state, fail the gesture
                SetState(GestureRecognizerState.Failed);
            }
        }

        /// <summary>
        /// The current scale multiplier. Multiply your current scale value by this to scale.
        /// </summary>
        /// <value>The scale multiplier.</value>
        public float ScaleMultiplier { get; private set; }

        /// <summary>
        /// The current scale multiplier for x axis. Multiply your current scale x value by this to scale.
        /// </summary>
        /// <value>The scale multiplier.</value>
        public float ScaleMultiplierX { get; private set; }

        /// <summary>
        /// The current scale multiplier for y axis. Multiply your current scale y value by this to scale.
        /// </summary>
        /// <value>The scale multiplier.</value>
        public float ScaleMultiplierY { get; private set; }

        /// <summary>
        /// Additional multiplier for ScaleMultipliers. This will making scaling happen slower or faster. Default is 3.0.
        /// </summary>
        /// <value>The zoom speed.</value>
        public float ZoomSpeed { get; set; }

        /// <summary>
        /// How many units the distance between the fingers must increase or decrease from the start distance to begin executing. Default is 0.15.
        /// </summary>
        /// <value>The threshold in units</value>
        public float ThresholdUnits { get; set; }
    }
}
