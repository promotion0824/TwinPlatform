using System;

namespace WillowRules.Filters;

/// <summary>
/// The state for a simple Kalman filter for a single value
/// </summary>
public class Kalman : IEquatable<Kalman>
{
	public double ErrMeasure { get; set; }
	public double ErrEstimate { get; set; }
	public double Q { get; set; }
	public double CurrentEstimate { get; set; }
	public double LastEstimate { get; set; }
	public double KalmanGain { get; set; }

	/// <summary>
	/// Initial Kalman filter
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

	/// <summary>
	/// Equals
	/// </summary>
	public bool Equals(Kalman? other)
	{
		return other is Kalman k && (this.CurrentEstimate, this.LastEstimate, this.ErrMeasure, this.ErrEstimate, this.Q, this.KalmanGain)
			.Equals((k.CurrentEstimate, k.LastEstimate, k.ErrMeasure, k.ErrEstimate, k.Q, k.KalmanGain));
	}
}
/// <summary>
/// Implementation for a Kalman filter
/// </summary>
/// <remarks>
/// See original implemenation on github https://github.com/IanMercer/pi-sniffer/blob/master/src/core/kalman.c
/// </remarks>
public static class KalmanFilter
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
		k.CurrentEstimate = k.LastEstimate + k.KalmanGain * (mea - k.LastEstimate);
		k.ErrEstimate = (1.0 - k.KalmanGain) * k.ErrEstimate + Math.Abs(k.LastEstimate - k.CurrentEstimate) * k.Q;
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
	/// State variable representing the continuous value of the binary variable
	/// </summary>
	public double State { get; set; }
	/// <summary>
	/// Error covariance
	/// </summary>
	public double Covariance { get; set; }
	/// <summary>
	/// Process noise covariance
	/// </summary>
	public double ProcessNoise { get; set; }
	/// <summary>
	/// Measurement noise covariance
	/// </summary>
	public double MeasurementNoise { get; set; }
	/// <summary>
	/// State transition parameter
	/// </summary>
	public double StateTransition { get; set; }
	/// <summary>
	/// Benchmark to validate result against
	/// </summary>
	public double BenchMark { get; set; }


	public BinaryKalmanFilter(double initialProbability, double initialCovariance, double processNoise = 0.01, double measurementNoise = 0.1, double stateTransition = 1.0)
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

	public bool Equals(BinaryKalmanFilter? other)
	{
		return other is BinaryKalmanFilter bK && (this.State, this.Covariance, this.ProcessNoise, this.MeasurementNoise, this.StateTransition)
			.Equals((bK.State, bK.Covariance, bK.ProcessNoise, bK.MeasurementNoise, bK.StateTransition));
	}
}

public static class BinaryKalmanFilterFunctions
{
	public static double Update(BinaryKalmanFilter bK, bool measurement)
	{
		// Map the boolean measurement to a continuous value
		var continuousMeasurement = measurement ? 1.0 : 0.0;

		// Predict the next state estimate and covariance
		var predictedState = bK.StateTransition * bK.State;
		var predictedCovariance = bK.StateTransition * bK.Covariance * bK.StateTransition + bK.ProcessNoise;

		// Calculate the Kalman gain
		var kalmanGain = predictedCovariance / (predictedCovariance + bK.MeasurementNoise);

		// Update the state estimate and covariance based on the measurement
		bK.State = predictedState + kalmanGain * (continuousMeasurement - predictedState);
		bK.Covariance = (1 - kalmanGain) * predictedCovariance;

		return bK.State; // Return the updated state estimate
	}
}

/// <summary>
/// One-dimensional Kalman filter
/// </summary>
public class OneDimensionalKalman : IEquatable<OneDimensionalKalman>
{
	/// <summary>
	/// The estimated state
	/// </summary>
	public double State { get; set; }

	/// <summary>
	/// Process noise covariance
	/// </summary>
	public double ProcessNoise { get; set; }

	/// <summary>
	/// Measurement noise covariance
	/// </summary>
	public double MeasurementNoise { get; set; }

	/// <summary>
	/// Estimate error covariance
	/// </summary>
	public double EstimateError { get; set; }

	public OneDimensionalKalman(double initialEstimate = 0, double initialError = 1, double processNoise = 0.1, double measurementNoise = 1)
	{
		State = initialEstimate;
		EstimateError = initialError;
		ProcessNoise = processNoise;
		MeasurementNoise = measurementNoise;
	}

	public bool Equals(OneDimensionalKalman? other)
	{
		return other is OneDimensionalKalman k && (this.State, this.ProcessNoise, this.MeasurementNoise, this.EstimateError)
			.Equals((k.State, k.ProcessNoise, k.MeasurementNoise, k.EstimateError));
	}
}

public static class OneDimensionalKalmanFunctions
{
	public static double Update(OneDimensionalKalman k, double measurement)
	{
		// Prediction
		double prediction = k.State;
		double predictionError = k.EstimateError + k.ProcessNoise;

		// Update
		double kalmanGain = predictionError / (predictionError + k.MeasurementNoise);
		k.State = prediction + kalmanGain * (measurement - prediction);
		k.EstimateError = (1 - kalmanGain) * predictionError;

		return k.State;
	}
}
