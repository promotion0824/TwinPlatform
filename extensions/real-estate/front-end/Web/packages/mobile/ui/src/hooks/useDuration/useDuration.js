import duration from './duration'

export default function useDuration() {
  const durationFunc = (inputDuration) => duration(inputDuration)
  return durationFunc
}
