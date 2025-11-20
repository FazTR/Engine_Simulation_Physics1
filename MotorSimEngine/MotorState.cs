namespace MotorSimEngine
{
    public class MotorState
    {
        public double Time { get; set; }        // s
        public double Speed { get; set; }       // m/s
        public double Accel { get; set; }       // m/s²
        public double Position { get; set; }    // m
        public double Rpm { get; set; }         // dev/dk
        public double Throttle { get; set; }    // 0..1
        public bool EngineOn { get; set; }

        public double SpeedKmh    => Speed * 3.6;
        public double PositionKm  => Position / 1000.0;
        public double OdometerKm  => Position / 1000.0;
    }
}
