//Contributors:
//BeksOmega

namespace UnityEngine.UI.ScrollSnaps
{
    [HelpURL("https://bitbucket.org/beksomega/unityuiscrollsnaps/wiki/HelperClasses/Scroller")]
    /// <summary>
    /// This class encapsulates scrolling. You can use scrollers to collect
    /// the data you need to create a scrolling animation, for example, in
    /// responce to a fling gesture. Scrollers track
    /// scroll offsets for you over time, but they don't automatically apply those
    /// positions to your view. It's your responsibility to get and apply new
    /// coordinates at a rate that will make the scrolling animation look smooth.
    /// </summary>
    public class Scroller
    {
        #region Variables
        public enum Mode
        {
            Scrolling,
            Flinging
        }

        public bool isScrolling
        {
            get
            {
                return m_Mode == Mode.Scrolling && !m_Finished;
            }
        }

        public bool isFlinging
        {
            get
            {
                return m_Mode == Mode.Flinging && !m_Finished;
            }
        }

        public bool isFinished
        {
            get
            {
                return m_Finished;
            }
        }

        public Interpolator interpolator
        {
            get
            {
                return m_Interpolator;
            }
        }

        public Vector2 startPosition
        {
            get
            {
                return m_StartPosition;
            }
        }

        public Vector2 finalPosition
        {
            get
            {
                return m_FinalPosition;
            }
        }

        public Vector2 currentPosition
        {
            get
            {
                return m_CurrentPosition;
            }
        }

        public Vector2 currentVelocity
        {
            get
            {
                return m_CurrentVelocity;
            }
        }

        public float durationOfLatestAnimation
        {
            get
            {
                return m_Duration;
            }
        }

        public float timePassedSinceStartOfAnimation
        {
            get
            {
                return Time.time - m_StartTime;
            }
        }

        private float timeSinceLastCompute
        {
            get
            {
                return Time.time - m_LastComputeTime;
            }
        }


        private Mode m_Mode;
        private bool m_Finished = true;

        private Interpolator m_Interpolator = new ViscousFluidInterpolator();

        private Vector2 m_StartPosition;
        private Vector2 m_FinalPosition;
        private Vector2 m_CurrentPosition;
        private Vector2 m_MinPosition;
        private Vector2 m_MaxPosition;

        private Vector2 m_MovementDelta;
        private Vector2 m_CurrentVelocity;

        private float m_Duration;
        private float m_DurationReciprocal;
        private float m_DecelerationRate;

        private float m_StartTime;
        private float m_LastComputeTime;

        private Vector2 MIN_VECTOR = new Vector2(float.MinValue, float.MinValue);
        private Vector2 MAX_VECTOR = new Vector2(float.MaxValue, float.MaxValue);
        public float DEFAULT_DECELERATION = 0.135f;
        public float DEFAULT_DURATION = .25f;

        #endregion

        #region Animation Logic

        public Scroller() { }

        /// <summary>
        /// Calculates how the movement delta based on the velocity at the start of the animation and the deceleration rate.
        /// </summary>
        /// <param name="startVelocity">The velocity at the start of the animation.</param>
        /// <param name="decelerationRate">The deceleration rate of the velocity.</param>
        /// <returns></returns>
        public float CalculateMovementDelta(float startVelocity, float decelerationRate)
        {
            return (1 - startVelocity) / Mathf.Log(decelerationRate);
        }

        /// <summary>
        /// Calculates the Deceleration rate based on the velocity at the start of the animation and how far you want the animation to move.
        /// </summary>
        /// <param name="startVelocity">The velocity at the start of the animation.</param>
        /// <param name="movementDelta"></param>
        /// <returns></returns>
        public float CalculateDecelerationRate(float startVelocity, float movementDelta)
        {
            return Mathf.Exp((1 - startVelocity) / movementDelta);
        }

        /// <summary>
        /// Calculates the duration based on the velocity at the start of the animation and the deceration rate.
        /// </summary>
        /// <param name="startVelocity">The velocity at the start of the animation.</param>
        /// <param name="decelerationRate">The deceleration rate of the velocity.</param>
        /// <returns></returns>
        public float CalculateDuration(float startVelocity, float decelerationRate)
        {
            return Mathf.Log(1 / startVelocity) / Mathf.Log(decelerationRate);
        }

        /// <summary>
        /// Starts a fling animation based on the Start Position and Velocity. If doFlyWheel is true the velocity will be added to the current velocity of any animations. The deceleration rate will be set to default (0.135).
        /// </summary>
        /// <param name="startPosition">The start position of the animation.</param>
        /// <param name="velocity">The start velocity of the animation.</param>
        /// <param name="doFlyWheel">If true the velocity will be added to the current velocity of any animation.</param>
        public void StartFling(Vector2 startPosition, Vector2 velocity, bool doFlyWheel)
        {
            StartFling(startPosition, velocity, doFlyWheel, DEFAULT_DECELERATION);
        }

        /// <summary>
        /// Starts a fling animation based on the Start Position, Velocity, and Deceleration Rate. If doFlyWheel is true the velocity will be added to the current velocity of any animations. 
        /// </summary>
        /// <param name="startPosition">The start position of the animation.</param>
        /// <param name="velocity">The start velocity of the animation.</param>
        /// <param name="doFlyWheel">If true the velocity will be added to the current velocity of any animations.</param>
        /// <param name="decelerationRate">The deceleration rate of the velocity.</param>
        public void StartFling(Vector2 startPosition, Vector2 velocity, bool doFlyWheel, float decelerationRate)
        {
            StartFling(startPosition, velocity, doFlyWheel, decelerationRate, MIN_VECTOR, MAX_VECTOR);
        }

        /// <summary>
        /// Starts a fling animation based on the Start Position, Velocity, and Deceleration Rate. The animation will not go beyond the Min Pos or Max Pos. If doFlyWheel is true the velocity will be added to the current velocity of any animations.
        /// </summary>
        /// <param name="startPosition">The start position of the animation.</param>
        /// <param name="velocity">The start velocity of the animation.</param>
        /// <param name="doFlyWheel">If true the velocity will be added to the current velocity of any animations.</param>
        /// <param name="decelerationRate">The deceleration rate of the velocity.</param>
        /// <param name="minPos">The minimum position the animation will not go beyond.</param>
        /// <param name="maxPos">The maximum position th animation will not go beyond.</param>
        public void StartFling(Vector2 startPosition, Vector2 velocity, bool doFlyWheel, float decelerationRate, Vector2 minPos, Vector2 maxPos)
        {
            if (doFlyWheel && Mathf.Sign(m_CurrentVelocity.x) == Mathf.Sign(velocity.x) && Mathf.Sign(m_CurrentVelocity.y) == Mathf.Sign(velocity.y))
            {
                velocity += m_CurrentVelocity;
            }

            decelerationRate = Mathf.Clamp(decelerationRate, 0, 1 - Mathf.Epsilon);

            m_Mode = Mode.Flinging;
            m_Finished = false;

            m_Duration = CalculateDuration(velocity.magnitude, decelerationRate);
            m_DurationReciprocal = 1 / m_Duration;
            m_StartTime = Time.time;
            m_LastComputeTime = Time.time;

            m_MinPosition = minPos;
            m_MaxPosition = maxPos;

            m_StartPosition = startPosition;
            m_CurrentPosition = m_StartPosition;
            m_FinalPosition = new Vector2(m_StartPosition.x + CalculateMovementDelta(velocity.x, decelerationRate), m_StartPosition.y + CalculateMovementDelta(velocity.y, decelerationRate));
            m_FinalPosition.x = Mathf.Clamp(m_FinalPosition.x, m_MinPosition.x, m_MaxPosition.x);
            m_FinalPosition.y = Mathf.Clamp(m_FinalPosition.y, m_MinPosition.y, m_MaxPosition.y);
            m_MovementDelta = m_FinalPosition - m_StartPosition;

            m_CurrentVelocity = velocity;
            m_DecelerationRate = decelerationRate;
        }

        /// <summary>
        /// Starts a scroll animation based on the Start Position and Final Position. The duration will be set to default (.25). The interpolator will be set to default (Viscous Fluid).
        /// </summary>
        /// <param name="startPosition">The start position of the animation.</param>
        /// <param name="finalPosition">The final position of the animation.</param>
        public void StartScroll(Vector2 startPosition, Vector2 finalPosition)
        {
            StartScroll(startPosition, finalPosition, DEFAULT_DURATION);
        }

        /// <summary>
        /// Starts a scroll animation based on the Start Position, Final Position, and Duration. The interpolator will be set to default (Viscous Fluid).
        /// </summary>
        /// <param name="startPosition">The start position of the animation.</param>
        /// <param name="finalPosition">The final position of the animation.</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        public void StartScroll(Vector2 startPosition, Vector2 finalPosition, float duration)
        {
            StartScroll(startPosition, finalPosition, duration, new ViscousFluidInterpolator());
        }

        /// <summary>
        /// Starts a scroll animation based on the Start Positoin, Final Position, Duration, and Interpolator.
        /// </summary>
        /// <param name="startPosition">The start position of the animation.</param>
        /// <param name="finalPosition">The final position of the animation.</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        /// <param name="interpolator">An interpolator that modifies the animation.</param>
        public void StartScroll(Vector2 startPosition, Vector2 finalPosition, float duration, Interpolator interpolator)
        {
            if (interpolator == null)
            {
                interpolator = new ViscousFluidInterpolator();
            }

            m_Mode = Mode.Scrolling;
            m_Finished = false;
            m_Interpolator = interpolator;

            m_Duration = duration;
            m_DurationReciprocal = 1 / m_Duration;
            m_StartTime = Time.time;
            m_LastComputeTime = Time.time;

            m_StartPosition = startPosition;
            m_CurrentPosition = m_StartPosition;
            m_FinalPosition = finalPosition;
            m_MovementDelta = m_FinalPosition - m_StartPosition;

            m_CurrentVelocity = m_MovementDelta / m_Duration; //average velocity
        }

        /// <summary>
        /// Shifts the start and end points of scroll animations by the offset.
        /// </summary>
        /// <param name="offset">The amount to shift the scroll animation by.</param>
        public void ShiftAnimation(Vector2 offset)
        {
            m_StartPosition += offset;
            m_CurrentPosition += offset;
            m_FinalPosition += offset;
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void ForceFinish()
        {
            m_Finished = true;
        }

        /// <summary>
        /// Stops the animation and sets the current position to the end position of the animation.
        /// </summary>
        public void AbortAnimation()
        {
            m_CurrentPosition = m_FinalPosition;
            m_Finished = true;
        }

        /// <summary>
        /// Updates the current position of the animation.
        /// </summary>
        /// <returns>Returns true if the animation is animating.</returns>
        public bool ComputeScrollOffset()
        {
            if (m_Finished)
            {
                return false;
            }

            if (timePassedSinceStartOfAnimation < m_Duration)
            {
                switch (m_Mode)
                {
                    case Mode.Scrolling:
                        float x = m_Interpolator.GetInterpolation(timePassedSinceStartOfAnimation * m_DurationReciprocal);
                        m_CurrentPosition = m_StartPosition + (x * m_MovementDelta);
                        break;
                    case Mode.Flinging:
                        Vector2 prevVelocity = m_CurrentVelocity;
                        m_CurrentVelocity *= Mathf.Pow(m_DecelerationRate, timeSinceLastCompute);
                        Vector2 avgVelocity = m_CurrentVelocity + (prevVelocity - m_CurrentVelocity) / 2;

                        if (m_CurrentVelocity.x < 1)
                        {
                            m_CurrentVelocity.x = 0;
                        }
                        if (m_CurrentVelocity.y < 1)
                        {
                            m_CurrentVelocity.y = 0;
                        }

                        m_CurrentPosition += avgVelocity * timeSinceLastCompute;
                        m_CurrentPosition.x = Mathf.Clamp(m_CurrentPosition.x, m_MinPosition.x, m_MaxPosition.x);
                        m_CurrentPosition.y = Mathf.Clamp(m_CurrentPosition.y, m_MinPosition.y, m_MaxPosition.y);
                        if (m_CurrentPosition == m_FinalPosition)
                        {
                            m_Finished = true;
                        }
                        break;
                }
            }
            else
            {
                m_CurrentPosition = m_FinalPosition;
                m_Finished = true;
            }

            m_LastComputeTime = Time.time;
            return true;
        }

        #endregion

        #region Interpolators

        public class ViscousFluidInterpolator : Interpolator
        {
            /** Controls the viscous fluid effect (how much of it). */
            private readonly float VISCOUS_FLUID_SCALE = 8.0f;
            private readonly float VISCOUS_FLUID_NORMALIZE;
            private readonly float VISCOUS_FLUID_OFFSET;

            public ViscousFluidInterpolator()
            {
                // must be set to 1.0 (used in viscousFluid())
                VISCOUS_FLUID_NORMALIZE = 1.0f / viscousFluid(1.0f);
                // account for very small floating-point error
                VISCOUS_FLUID_OFFSET = 1.0f - VISCOUS_FLUID_NORMALIZE * viscousFluid(1.0f);
            }

            private float viscousFluid(float x)
            {
                x *= VISCOUS_FLUID_SCALE;
                if (x < 1.0f)
                {
                    x -= (1.0f - Mathf.Exp(-x));
                }
                else
                {
                    float start = 0.36787944117f;   // 1/e == exp(-1)
                    x = 1.0f - Mathf.Exp(1.0f - x);
                    x = start + x * (1.0f - start);
                }
                return x;
            }

            public float GetInterpolation(float input)
            {
                float interpolated = VISCOUS_FLUID_NORMALIZE * viscousFluid(input);
                if (interpolated > 0)
                {
                    return interpolated + VISCOUS_FLUID_OFFSET;
                }
                return interpolated;
            }
        }

        public class AccelerateDecelerateInterpolator : Interpolator
        {
            public float GetInterpolation(float input)
            {
                return (Mathf.Cos((input + 1) * Mathf.PI) / 2.0f) + 0.5f;
            }
        }

        public class AccelerateInterpolator : Interpolator
        {
            public float GetInterpolation(float input)
            {
                return input * input;
            }
        }

        public class AnticipateInterpolator : Interpolator
        {
            private float mTension = 2f;

            public AnticipateInterpolator() { }

            public AnticipateInterpolator(float tension)
            {
                mTension = tension;
            }

            public float GetInterpolation(float input)
            {
                // a(t) = t * t * ((tension + 1) * t - tension)
                return input * input * ((mTension + 1) * input - mTension);
            }
        }

        public class AnticipateOvershootInterpolator : Interpolator
        {
            private float mTension = 2f * 1.5f;

            public AnticipateOvershootInterpolator() { }

            public AnticipateOvershootInterpolator(float tension)
            {
                mTension = tension * 1.5f;
            }

            private static float a(float t, float s)
            {
                return t * t * ((s + 1) * t - s);
            }
            private static float o(float t, float s)
            {
                return t * t * ((s + 1) * t + s);
            }

            public float GetInterpolation(float input)
            {
                // a(t, s) = t * t * ((s + 1) * t - s)
                // o(t, s) = t * t * ((s + 1) * t + s)
                // f(t) = 0.5 * a(t * 2, tension * extraTension), when t < 0.5
                // f(t) = 0.5 * (o(t * 2 - 2, tension * extraTension) + 2), when t <= 1.0
                if (input < 0.5f) return 0.5f * a(input * 2.0f, mTension);
                else return 0.5f * (o(input * 2.0f - 2.0f, mTension) + 2.0f);
            }
        }

        public class BounceInterpolator : Interpolator
        {
            private static float mBouncyness = 8f;

            public BounceInterpolator() { }

            public BounceInterpolator(float _bouncyness)
            {
                mBouncyness = _bouncyness;
            }

            private static float bounce(float t)
            {
                return t * t * mBouncyness;
            }

            public float GetInterpolation(float input)
            {
                // _b(t) = t * t * 8
                // bs(t) = _b(t) for t < 0.3535
                // bs(t) = _b(t - 0.54719) + 0.7 for t < 0.7408
                // bs(t) = _b(t - 0.8526) + 0.9 for t < 0.9644
                // bs(t) = _b(t - 1.0435) + 0.95 for t <= 1.0
                // b(t) = bs(t * 1.1226)
                input *= 1.1226f;
                if (input < 0.3535f) return bounce(input);
                else if (input < 0.7408f) return bounce(input - 0.54719f) + 0.7f;
                else if (input < 0.9644f) return bounce(input - 0.8526f) + 0.9f;
                else return bounce(input - 1.0435f) + 0.95f;
            }
        }

        public class DecelerateInterpolator : Interpolator
        {
            public float GetInterpolation(float input)
            {
                return (1.0f - (1.0f - input) * (1.0f - input));
            }
        }

        public class DecelerateAccelerateInterpolator : Interpolator
        {
            public float GetInterpolation(float input)
            {
                return (Mathf.Tan(((.5f * input) + .75f) * Mathf.PI) / 2) + .5f;
            }
        }

        public class LinearInterpolator : Interpolator
        {
            public float GetInterpolation(float input)
            {
                return input;
            }
        }

        public class OvershootInterpolator : Interpolator
        {
            private float mTension = 2f;

            public OvershootInterpolator() { }

            public OvershootInterpolator(float tension)
            {
                mTension = tension;
            }

            public float GetInterpolation(float input)
            {
                // _o(t) = t * t * ((tension + 1) * t + tension)
                // o(t) = _o(t - 1) + 1
                input -= 1.0f;
                return input * input * ((mTension + 1) * input + mTension) + 1.0f;
            }
        }


        #endregion
    }

    public interface Interpolator
    {
        float GetInterpolation(float input);
    }
}
