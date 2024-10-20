namespace Willow.LiveData.TelemetryDataQuality.Filters;

using System;

/// <summary>
/// The state for a simple Kalman filter for a single value.
/// </summary>
internal class Kalman : IEquatable<Kalman>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Kalman"/> class.
    /// Initial Kalman filter.
    /// </summary>
    public Kalman()
    {
        CurrentEstimate = -999; // marker value used by time interval check
        LastEstimate = -999; // marker value, so first real value overrides it
        ErrMeasure = 10.0;
        ErrEstimate = 10.0;
        Q = 0.25;
        KalmanGain = 0.25;
    }

    public double ErrMeasure { get; set; }

    public double ErrEstimate { get; set; }

    public double Q { get; set; }

    public double CurrentEstimate { get; set; }

    public double LastEstimate { get; set; }

    public double KalmanGain { get; set; }

    /// <summary>
    /// Equals method for the Kalman class.
    /// Determines whether the current instance of Kalman is equal to another Kalman object.
    /// </summary>
    /// <param name="other">The Kalman object to compare with the current instance. </param>
    /// <returns>true if the current instance is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(Kalman? other)
    {
        return other is { } k && (CurrentEstimate, LastEstimate, ErrMeasure, ErrEstimate, Q, KalmanGain)
            .Equals((k.CurrentEstimate, k.LastEstimate, k.ErrMeasure, k.ErrEstimate, k.Q, k.KalmanGain));
    }
}

/// <summary>
/// Implementation for a Kalman filter.
/// </summary>
/// <remarks>
/// See original implementation on GitHub https://github.com/IanMercer/pi-sniffer/blob/master/src/core/kalman.c .
/// </remarks>
internal static class KalmanFilter
{
    public static void Initialize(Kalman k)
    {
        k.CurrentEstimate = -999; // marker value used by time interval check
        k.LastEstimate = -999; // marker value, so first real value overrides it
        k.ErrMeasure = 10.0;
        k.ErrEstimate = 10.0;
        k.Q = 0.25;
    }

    public static double Update(Kalman k, double mea)
    {
        // First time through, use the measured value as the actual value
        if (k.LastEstimate == -999)
        {
            k.LastEstimate = mea;
            return mea;
        }

        k.KalmanGain = k.ErrEstimate / (k.ErrEstimate + k.ErrMeasure);
        k.CurrentEstimate = k.LastEstimate + (k.KalmanGain * (mea - k.LastEstimate));
        k.ErrEstimate = ((1.0 - k.KalmanGain) * k.ErrEstimate) + (Math.Abs(k.LastEstimate - k.CurrentEstimate) * k.Q);
        k.LastEstimate = k.CurrentEstimate;

        return k.CurrentEstimate;
    }
}

/// <summary>
/// A Binary Kalman Filter is a filtering algorithm that estimates the continuous probability of a binary variable based on noisy observations.
/// It provides a way to infer the underlying continuous probability of the binary variable by incorporating both prior knowledge and current measurements.
/// </summary>
internal class BinaryKalmanFilter : IEquatable<BinaryKalmanFilter>
{
    public BinaryKalmanFilter(double initialProbability,
        double initialCovariance,
        double processNoise = 0.01,
        double measurementNoise = 0.1,
        double stateTransition = 1.0)
    {
        State = initialProbability;
        Covariance = initialCovariance;
        ProcessNoise = processNoise;
        MeasurementNoise = measurementNoise;
        StateTransition = stateTransition;
        BenchMark = initialProbability;
    }

    public BinaryKalmanFilter()
    {
        State = 0.2;
        Covariance = 0.0;
        ProcessNoise = 0.01;
        MeasurementNoise = 0.1;
        StateTransition = 1.0;
        BenchMark = 0.2;
    }

    /// <summary>
    /// Gets or sets state variable representing the continuous value of the binary variable.
    /// </summary>
    public double State { get; set; }

    /// <summary>
    /// Gets or sets error covariance.
    /// </summary>
    public double Covariance { get; set; }

    /// <summary>
    /// Gets or sets process noise covariance.
    /// </summary>
    public double ProcessNoise { get; set; }

    /// <summary>
    /// Gets or sets measurement noise covariance.
    /// </summary>
    public double MeasurementNoise { get; set; }

    /// <summary>
    /// Gets or sets state transition parameter.
    /// </summary>
    public double StateTransition { get; set; }

    /// <summary>
    /// Gets or sets benchmark to validate result against.
    /// </summary>
    public double BenchMark { get; set; }

    public bool Equals(BinaryKalmanFilter? other)
    {
        return other != null && (State, Covariance, ProcessNoise, MeasurementNoise, StateTransition)
            .Equals((other.State, other.Covariance, other.ProcessNoise, other.MeasurementNoise, other.StateTransition));
    }
}

internal static class BinaryKalmanFilterFunctions
{
    public static void Update(BinaryKalmanFilter bK, bool measurement)
    {
        // Map the boolean measurement to a continuous value
        var continuousMeasurement = measurement ? 1.0 : 0.0;

        // Predict the next state estimate and covariance
        var predictedState = bK.StateTransition * bK.State;
        var predictedCovariance = (bK.StateTransition * bK.Covariance * bK.StateTransition) + bK.ProcessNoise;

        // Calculate the Kalman gain
        var kalmanGain = predictedCovariance / (predictedCovariance + bK.MeasurementNoise);

        // Update the state estimate and covariance based on the measurement
        bK.State = predictedState + (kalmanGain * (continuousMeasurement - predictedState));
        bK.Covariance = (1 - kalmanGain) * predictedCovariance;
    }
}
