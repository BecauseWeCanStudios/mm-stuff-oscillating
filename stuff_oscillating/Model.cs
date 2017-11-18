using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Diagnostics;

namespace stuff_oscillating
{
    public static class Model
    {

        public class ModelParameters
        {
            public double ObjectMass = 1;
            public double RestrictionCoeffitient = 1;
            public double FrictionCoeffitient = 0.1;
#pragma warning disable IDE1006 // Стили именования
            public double ω => RestrictionCoeffitient / ObjectMass;
#pragma warning restore IDE1006 // Стили именования
            public double ForceAmplitude = 0;
            public double ForcePeriod = 1;
            public double InitialVelocity = 0;
            public double InitialX  = 1;
            public double Dt = 0.1;

            private static ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();
            private bool useForce = false;
            public bool UseForce
            {
                get
                {
                    try
                    {
                        rwlock.EnterReadLock();
                        return useForce;
                    }
                    finally
                    {
                        rwlock.ExitReadLock();
                    }
                }
                set
                {
                    try
                    {
                        rwlock.EnterWriteLock();
                        useForce = value;
                    }
                    finally
                    {
                        rwlock.ExitWriteLock();
                    }
                }
            }
        }

        public class ModelStatus : EventArgs
        {
            public double X;
            public double Velocity;
            public double Energy;
            public double Time;
        }

        private static ReaderWriterLockSlim imp_rwlock = new ReaderWriterLockSlim();
        private static ReaderWriterLockSlim tick_rwlock = new ReaderWriterLockSlim();

        private static Stopwatch stopwatch = new Stopwatch();
        private static Timer timer = null;

        private static ModelStatus modelStatus = new ModelStatus();

        private static double impulse = 0;
        public  static double Impulse
        {
            get
            {
                try
                {
                    imp_rwlock.EnterReadLock();
                    return impulse;
                }
                finally
                {
                    imp_rwlock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    imp_rwlock.EnterWriteLock();
                    impulse = value;
                }
                finally
                {
                    imp_rwlock.ExitWriteLock();
                }
            }
        }

        private static ModelParameters parameters = new ModelParameters();
        public static EventHandler<ModelStatus> ModelTick;

        private static void CalculateState(object state)
        {
            double time = stopwatch.Elapsed.TotalMilliseconds;
            double a = -parameters.ω * modelStatus.X - parameters.FrictionCoeffitient * modelStatus.Velocity;
            a += Impulse;
            Impulse = 0;
            if (parameters.UseForce)
                a += parameters.ForceAmplitude * (1 - Math.Cos(2 * Math.PI * time / parameters.ForcePeriod)) / 2;
            modelStatus.Time = time / 1000;
            modelStatus.Velocity = modelStatus.Velocity + a * parameters.Dt;
            modelStatus.X += modelStatus.Velocity * parameters.Dt;
            modelStatus.Energy = 0.5 * (parameters.ObjectMass * modelStatus.Velocity * modelStatus.Velocity +
                 modelStatus.X * modelStatus.X * parameters.RestrictionCoeffitient);
            ModelTick(null, modelStatus);
        }

        public static void Start(ModelParameters new_parameters)
        {
            stopwatch.Start();
            timer = new Timer(CalculateState, null, 0, 50);
            new_parameters = parameters;
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
