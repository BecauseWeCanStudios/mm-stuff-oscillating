using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Diagnostics;

namespace coffee_cooling
{
    public static class Model
    {

        public class ModelParameters
        {
            public double ObjectMass = 10;
            public double RestrictionCoeffitient = 1;
            public double FrictionCoeffitient = 1;
#pragma warning disable IDE1006 // Стили именования
            public double ω => FrictionCoeffitient / ObjectMass;
#pragma warning restore IDE1006 // Стили именования
            public double ForceAmplitude = 0;
            public double ForcePeriod = 1;
            public bool UseForce = false;
            public double InitialVelocity = 0;
            public double InitialX = 0;
        }

        public class ModelStatus : EventArgs
        {
            public double X;
            public double Velocity;
            public double Energy;
            public double Time;
        }

        private static Stopwatch stopwatch = new Stopwatch();
        private static Timer timer = null;
        private static ModelStatus modelStatus = new ModelStatus();
        private static List<double> impulses = new List<double>();

        private static ModelParameters parameters = new ModelParameters();

        public static void SetParameters(ModelParameters param)
        {
            lock(parameters)
            {
                parameters = param;
            }
        }

        public static void AddImpulse(double impulse)
        {
            lock(impulses)
            {
                impulses.Add(impulse);
            }
        }

        public static EventHandler<ModelStatus> ModelTick;

        private static void CalculateState(object state)
        {
            double time = stopwatch.Elapsed.TotalMilliseconds;
            ModelStatus current;
            ModelParameters p;
            lock (modelStatus)
            {
                current = modelStatus;
            }
            lock(parameters)
            {
                p = parameters;
            }
            double a = -p.ω * current.X - p.FrictionCoeffitient * current.Velocity;
            if (p.UseForce)
                a += p.ForceAmplitude * (1 - Math.Cos(2 * Math.PI * time / p.ForcePeriod)) / 2;
            lock (impulses)
            {
                foreach (var it in impulses)
                    a += it;
                impulses.Clear();
            }
            lock (modelStatus)
            {
                double dt = time - current.Time;
                modelStatus = new ModelStatus()
                {
                    Time = time,
                    Velocity = current.Velocity + a * dt
                };
                modelStatus.X = current.X + modelStatus.Velocity * dt;
                modelStatus.Energy = 0.5 * (p.ObjectMass * modelStatus.Velocity * modelStatus.Velocity +
                    modelStatus.X * modelStatus.X * p.RestrictionCoeffitient);
            }
            ModelTick(null, modelStatus);
        }

        public static void Start(ModelParameters parameters)
        {
            stopwatch.Start();
            timer = new Timer(CalculateState, null, 0, 1);
            modelStatus = new ModelStatus()
            {
                Time = 0,
                X = parameters.InitialX,
                Velocity = parameters.InitialVelocity,
                Energy = 0.5 * (parameters.ObjectMass * parameters.InitialVelocity * parameters.InitialVelocity +
                    parameters.InitialX * parameters.InitialX * parameters.RestrictionCoeffitient)
            };
        }

        public static void Stop()
        {
            timer.Dispose();
        }

    }
}
