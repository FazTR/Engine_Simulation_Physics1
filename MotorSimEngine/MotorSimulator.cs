using System;

namespace MotorSimEngine
{
    public class MotorSimulator
    {
        public MotorState State { get; private set; }

        private const double EngineAccelMax = 4.0; // m/s²
        private const double DragCoeff      = 0.2; // sürtünme katsayısı

        public MotorSimulator()
        {
            State = new MotorState();
            Reset();
        }

        public void Reset()
        {
            State.Time      = 0;
            State.Speed     = 0;
            State.Accel     = 0;
            State.Position  = 0;
            State.Rpm       = 0;
            State.Throttle  = 0;
            State.EngineOn  = false;
        }

        public void SetEngine(bool on)
        {
            State.EngineOn = on;
            if (!on)
            {
                State.Rpm = 0;
            }
        }

        public void SetThrottle(double value)
        {
            State.Throttle = Math.Clamp(value, 0.0, 1.0);
        }
        public void ChangeThrottle(double delta)
        {
            SetThrottle(State.Throttle + delta);
        }

        public void Step(double dt)
        {
            if (dt <= 0) return;

            if (State.EngineOn)
            {
                double engineAccel = EngineAccelMax * State.Throttle;
                double drag        = DragCoeff * State.Speed;
                State.Accel        = engineAccel - drag;

                State.Rpm = 800 + State.Throttle * 4000 + State.Speed * 30;
            }
            else
            {
                double drag = DragCoeff * State.Speed;
                State.Accel = -drag;

                if (State.Speed < 0.1)
                {
                    State.Speed = 0;
                    State.Accel = 0;
                }

                State.Rpm = 0;
            }

            State.Speed    += State.Accel * dt;
            if (State.Speed < 0) State.Speed = 0;

            State.Position += State.Speed * dt;
            State.Time     += dt;
        }
    }
}
