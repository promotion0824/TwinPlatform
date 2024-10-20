/** Converts a temperature from Fahrenheit to Celsius. */
export function convertToCelsius(temperature: number) {
  return (temperature - 32) / 1.8
}

/** Converts a temperature from Celsius to Fahrenheit. */
export function convertToFahrenheit(temperature: number) {
  return temperature * 1.8 + 32
}
