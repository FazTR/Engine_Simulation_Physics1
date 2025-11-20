using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MotorSimEngine
{
    public class MotorForm : Form
    {
        private readonly MotorSimulator _sim;
        private readonly System.Windows.Forms.Timer _timer;   // <— Timer belirsizliğini çözdük
        private readonly double _dt = 0.1; // saniye

        // Grafikler
        private Chart chartSpeed;
        private Chart chartAccel;
        private Chart chartPosition;

        // Kontroller
        private TrackBar trackThrottle;
        private Button btnEngineOn;
        private Button btnEngineOff;
        private Button btnReset;

        private Label lblThrottle;
        private Label lblEngineState;
        private Label lblSpeed;
        private Label lblAccel;
        private Label lblPosition;
        private Label lblRpm;
        private Label lblOdo;
        private Label lblFriction;

        public MotorForm(MotorSimulator simulator)
        {
            _sim = simulator ?? throw new ArgumentNullException(nameof(simulator));

            Text = "Motor Simülator - PhysicsI";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 650);

            InitializeUI();

            _timer = new System.Windows.Forms.Timer   
            {
                Interval = (int)(_dt * 1000)
            };
            _timer.Tick += TimerOnTick;
            _timer.Start();
        }

        // UI Kurulumu 
        private void InitializeUI()
        {
            int chartW = 370;
            int chartH = 200;

            // Hız - Zaman
            chartSpeed = CreateChart("Zaman (s)", "Hız (km/h)", "Hız",
                new Point(10, 10), chartW, chartH);
            Controls.Add(chartSpeed);

            // İvme - Zaman
            chartAccel = CreateChart("Zaman (s)", "İvme (m/s²)", "İvme",
                new Point(10 + chartW + 10, 10), chartW, chartH);
            Controls.Add(chartAccel);

            // Konum - Zaman
            chartPosition = CreateChart("Zaman (s)", "Konum (km)", "Konum",
                new Point(10 + 2 * (chartW + 10), 10), chartW, chartH);
            Controls.Add(chartPosition);

            // Gaz TrackBar
            trackThrottle = new TrackBar
            {
                Minimum        = 0,
                Maximum        = 100,
                TickFrequency  = 10,
                Value          = 0,
                Width          = 350,
                Location       = new Point(10, 230)
            };
            trackThrottle.Scroll += (s, e) =>
            {
                double tVal = trackThrottle.Value / 100.0;
                _sim.SetThrottle(tVal);
                lblThrottle.Text = $"Gaz: {trackThrottle.Value}%";
            };
            Controls.Add(trackThrottle);

            lblThrottle = new Label
            {
                AutoSize  = true,
                Location  = new Point(10, 280),
                Text      = "Gaz: 0%"
            };
            Controls.Add(lblThrottle);

            // Butonlar
            btnEngineOn = new Button
            {
                Text      = "Motoru Çalıştır",
                Width     = 150,
                Height    = 25,
                Location  = new Point(400, 230)
            };
            btnEngineOn.Click += (s, e) =>
            {
                _sim.SetEngine(true);
                lblEngineState.Text = "Motor: ÇALIŞIYOR";
            };
            Controls.Add(btnEngineOn);

            btnEngineOff = new Button
            {
                Text      = "Motoru Durdur",
                 Width     = 150,
                Height    = 25,
                Location  = new Point(580, 230)
            };
            btnEngineOff.Click += (s, e) =>
            {
                _sim.SetEngine(false);
                lblEngineState.Text = "Motor: DURDU";
            };
            Controls.Add(btnEngineOff);

            btnReset = new Button
            {
                Text      = "Sıfırla",
                Location  = new Point(640, 300)
            };
            btnReset.Click += (s, e) => ResetSimulation();
            Controls.Add(btnReset);

            // Bilgi label
            int infoX = 10;
            int infoY = 320;
            int dy    = 25;

            
            lblFriction = new Label
            {
                AutoSize=true,
                Location=new Point(10,450),
                Text="Sürtünme Katsayısı:0.2"
            };
            Controls.Add(lblFriction);
            lblEngineState = new Label
            {
                AutoSize  = true,
                Location  = new Point(infoX, infoY),
                Text      = "Motor: DURDU"
            };
            Controls.Add(lblEngineState);

            lblSpeed = new Label
            {
                AutoSize  = true,
                Location  = new Point(infoX, infoY + dy),
                Text      = "Hız: 0.0 km/h"
            };
            Controls.Add(lblSpeed);

            lblAccel = new Label
            {
                AutoSize  = true,
                Location  = new Point(infoX, infoY + 2 * dy),
                Text      = "İvme: 0.00 m/s²"
            };
            Controls.Add(lblAccel);

            lblPosition = new Label
            {
                AutoSize  = true,
                Location  = new Point(infoX, infoY + 3 * dy),
                Text      = "Konum: 0.000 km"
            };
            Controls.Add(lblPosition);

            lblRpm = new Label
            {
                AutoSize  = true,
                Location  = new Point(400, infoY),
                Text      = "RPM: 0"
            };
            Controls.Add(lblRpm);

            lblOdo = new Label
            {
                AutoSize  = true,
                Location  = new Point(400, infoY + dy),
                Text      = "Kilometre Sayacı: 0.000 km"
            };
            Controls.Add(lblOdo);
        }

        private Chart CreateChart(string xTitle, string yTitle, string seriesName,
                                  Point location, int width, int height)
        {
            var chart = new Chart
            {
                Location = location,
                Size     = new Size(width, height)
            };

            var area = new ChartArea(seriesName + "Area");
            area.AxisX.Title = xTitle;
            area.AxisY.Title = yTitle;
            chart.ChartAreas.Add(area);

            var series = new Series(seriesName)
            {
                ChartType = SeriesChartType.Line,
                ChartArea = area.Name
            };
            chart.Series.Add(series);

            return chart;
        }

        // Timer Tick
        private void TimerOnTick(object? sender, EventArgs e)
        {
            _sim.Step(_dt);
            UpdateUI();
        }

        private void UpdateUI()
        {
            var s = _sim.State;

            chartSpeed.Series["Hız"].Points.AddXY(s.Time,      s.SpeedKmh);
            chartAccel.Series["İvme"].Points.AddXY(s.Time,     s.Accel);
            chartPosition.Series["Konum"].Points.AddXY(s.Time, s.PositionKm);

            LimitPoints(chartSpeed.Series["Hız"],      30000);
            LimitPoints(chartAccel.Series["İvme"],     30000);
            LimitPoints(chartPosition.Series["Konum"], 30000);

            lblSpeed.Text   = $"Hız: {s.SpeedKmh:0.0} km/h";
            lblAccel.Text   = $"İvme: {s.Accel:0.00} m/s²";
            lblPosition.Text= $"Konum: {s.PositionKm:0.000} km";
            lblRpm.Text     = $"RPM: {s.Rpm:0}";
            lblOdo.Text     = $"Kilometre Sayacı: {s.OdometerKm:0.000} km";
            lblEngineState.Text = s.EngineOn ? "Motor: ÇALIŞIYOR" : "Motor: DURDU";
        }

        private void LimitPoints(Series series, int maxPoints)
        {
            while (series.Points.Count > maxPoints)
            {
                series.Points.RemoveAt(0);
            }
        }

        private void ResetSimulation()
        {
            _sim.Reset();

            trackThrottle.Value = 0;
            lblThrottle.Text    = "Gaz: 0%";

            chartSpeed.Series["Hız"].Points.Clear();
            chartAccel.Series["İvme"].Points.Clear();
            chartPosition.Series["Konum"].Points.Clear();

            var s = _sim.State;
            lblEngineState.Text = "Motor: DURDU";
            lblSpeed.Text       = $"Hız: {s.SpeedKmh:0.0} km/h";
            lblAccel.Text       = $"İvme: {s.Accel:0.00} m/s²";
            lblPosition.Text    = $"Konum: {s.PositionKm:0.000} km";
            lblRpm.Text         = $"RPM: {s.Rpm:0}";
            lblOdo.Text         = $"Kilometre Sayacı: {s.OdometerKm:0.000} km";
        }
    }
}
