namespace Willow.Caching.Telemetry.DataQuality.Filters;

/// <summary>
/// The state for a simple Kalman filter for a single value.
/// </summary>
public class Kalman : IEquatable<Kalman>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Kalman"/> class.
    /// </summary>
    public Kalman()
    {
        this.CurrentEstimate = -999; // marker value used by time interval check
        this.LastEstimate = -999; // marker value, so first real value overrides it
        this.ErrMeasure = 10.0;
        this.ErrEstimate = 10.0;
        this.Q = 0.25;
        this.KalmanGain = 0.25;
    }

    /// <summary>
    /// Gets or sets the measurement error for the Kalman filter.
    /// </summary>
    public double ErrMeasure { get; set; }

    /// <summary>
    /// Gets or sets the estimate error for the Kalman filter.
    /// </summary>
    public double ErrEstimate { get; set; }

    /// <summary>
    /// Gets or sets the process noise covariance for the Kalman filter.
    /// </summary>
    public double Q { get; set; }

    /// <summary>
    /// Gets or sets the current estimate in the Kalman filter.
    /// </summary>
    public double CurrentEstimate { get; set; }

    /// <summary>
    /// Gets or sets the last estimated value for the Kalman filter.
    /// </summary>
    public double LastEstimate { get; set; }

    /// <summary>
    /// Gets or sets the gain value for the Kalman filter.
    /// </summary>
    public double KalmanGain { get; set; }

    /// <summary>
    /// Equals method for the Kalman class.
    /// Determines whether the current instance of Kalman is equal to another Kalman object.
    /// </summary>
    /// <param name="other">The Kalman object to compare with the current instance. </param>
    /// <returns>true if the current instance is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(Kalman? other)
    {
        return other is { } k && (this.CurrentEstimate, this.LastEstimate, this.ErrMeasure, this.ErrEstimate, this.Q, this.KalmanGain)
            .Equals((k.CurrentEstimate, k.LastEstimate, k.ErrMeasure, k.ErrEstimate, k.Q, k.KalmanGain));
    }
}

/// <summary>
/// Implementation for a Kalman filter.
/// </summary>
/// <remarks>
/// See original implementation on GitHub https://github.com/IanMercer/pi-sniffer/blob/master/src/core/kalman.c .
/// </remarks>
public static class KalmanFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Kalman"/> class.
    /// </summary>
    /// <param name="k">The <see cref="Kalman"/> instance to initialize.</param>
    /// <remarks>
    /// Initial Kalman filter.
    /// </remarks>
    public static void Initialize(Kalman k)
    {
        k.CurrentEstimate = -999; // marker value used by time interval check
        k.LastEstimate = -999; // marker value, so first real value overrides it
        k.ErrMeasure = 10.0;
        k.ErrEstimate = 10.0;
        k.Q = 0.25;
    }

    /// <summary>
    /// Update method for the Kalman class.
    /// </summary>
    /// <param name="k">The Kalman instance.</param>
    /// <param name="mea">The measured value.</param>
    /// <returns>The updated current estimate value.</returns>
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
public class BinaryKalmanFilter : IEquatable<BinaryKalmanFilter>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryKalmanFilter"/> class.
    /// </summary>
    /// <param name="initialProbability">Initial probability.</param>
    /// <param name="initialCovariance">Initial covariance.</param>
    /// <param name="processNoise">Process noise.</param>
    /// <param name="measurementNoise">Measurement noise.</param>
    /// <param name="stateTransition">State transition.</param>
    public BinaryKalmanFilter(
        double initialProbability,
        double initialCovariance,
        double processNoise = 0.01,
        double measurementNoise = 0.1,
        double stateTransition = 1.0)
    {
        this.State = initialProbability;
        this.Covariance = initialCovariance;
        this.ProcessNoise = processNoise;
        this.MeasurementNoise = measurementNoise;
        this.StateTransition = stateTransition;
        this.BenchMark = initialProbability;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryKalmanFilter"/> class.
    /// </summary>
    public BinaryKalmanFilter()
    {
        this.State = 0.2;
        this.Covariance = 0.0;
        this.ProcessNoise = 0.01;
        this.MeasurementNoise = 0.1;
        this.StateTransition = 1.0;
        this.BenchMark = 0.2;
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

    /// <summary>
    /// Equals method for the Kalman class.
    /// Determines whether the current instance of Kalman is equal to another Kalman object.
    /// </summary>
    /// <param name="other">The Kalman object to compare with the current instance. </param>
    /// <returns>true if the current instance is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(BinaryKalmanFilter? other)
    {
        return other != null && (this.State, this.Covariance, this.ProcessNoise, this.MeasurementNoise, this.StateTransition)
            .Equals((other.State, other.Covariance, other.ProcessNoise, other.MeasurementNoise, other.StateTransition));
    }
}

/// <summary>
/// Contains static methods for performing operations related to the Binary Kalman filter.
/// </summary>
public static class BinaryKalmanFilterFunctions
{
    /// <summary>
    /// The Update method is used to update the state estimate of the BinaryKalmanFilter based on a new measurement.
    /// </summary>
    /// <param name="bK">The instance of the `BinaryKalmanFilter` class.</param>
    /// <param name="measurement">The measured value.</param>
    /// <returns>The updated current estimate value.</returns>
    public static double Update(BinaryKalmanFilter bK, bool measurement)
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

        return bK.State; // Return the updated state estimate
    }
}

/// <summary>
/// One-dimensional Kalman filter.
/// </summary>
public class OneDimensionalKalman(
    double initialEstimate = 0,
    double initialError = 1,
    double processNoise = 0.1,
    double measurementNoise = 1)
    : IEquatable<OneDimensionalKalman>
{
    /// <summary>
    /// Gets or sets the estimated state.
    /// </summary>
    public double State { get; set; } = initialEstimate;

    /// <summary>
    /// Gets or sets process noise covariance.
    /// </summary>
    public double ProcessNoise { get; set; } = processNoise;

    /// <summary>
    /// Gets or sets measurement noise covariance.
    /// </summary>
    public double MeasurementNoise { get; set; } = measurementNoise;

    /// <summary>
    /// Gets or sets estimate error covariance.
    /// </summary>
    public double EstimateError { get; set; } = initialError;

    /// <summary>
    /// Equals method for the Kalman class.
    /// Determines whether the current instance of Kalman is equal to another Kalman object.
    /// </summary>
    /// <param name="other">The Kalman object to compare with the current instance. </param>
    /// <returns>true if the current instance is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(OneDimensionalKalman? other)
    {
        return other != null && (this.State, this.ProcessNoise, this.MeasurementNoise, this.EstimateError)
            .Equals((other.State, other.ProcessNoise, other.MeasurementNoise, other.EstimateError));
    }
}

/// <summary>
/// Contains static methods for performing operations related to the one-dimensional Kalman filter.
/// </summary>
public static class OneDimensionalKalmanFunctions
{
    /// <summary>
    /// Updates the state of the one-dimensional Kalman filter based on the given measurement.
    /// </summary>
    /// <param name="k">The one-dimensional Kalman filter.</param>
    /// <param name="measurement">The measurement value.</param>
    /// <returns>The updated state of the one-dimensional Kalman filter.</returns>
    public static double Update(OneDimensionalKalman k, double measurement)
    {
        // Prediction
        var prediction = k.State;
        var predictionError = k.EstimateError + k.ProcessNoise;

        // Update
        var kalmanGain = predictionError / (predictionError + k.MeasurementNoise);
        k.State = prediction + (kalmanGain * (measurement - prediction));
        k.EstimateError = (1 - kalmanGain) * predictionError;

        return k.State;
    }
}
