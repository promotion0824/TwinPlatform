import duration from './duration'
import { useLanguage } from '../../providers/LanguageProvider/LanguageContext'

export default function useDuration() {
  const { language } = useLanguage()
  const durationFunc = (inputDuration, locale = language) =>
    duration(inputDuration, locale)
  return durationFunc
}
