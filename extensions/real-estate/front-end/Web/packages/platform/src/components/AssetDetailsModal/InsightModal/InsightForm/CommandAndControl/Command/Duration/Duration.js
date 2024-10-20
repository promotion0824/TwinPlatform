import { FormControl, Flex, NumberInput } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import TextLabel from '../../TextLabel/TextLabel'
import FormattedDuration from './FormattedDuration'
import styles from './Duration.css'

export default function Duration({ ...rest }) {
  const { t } = useTranslation()
  return (
    <FormControl {...rest}>
      {(props) => (
        <Flex size="medium">
          <Flex horizontal fill="equal" size="medium">
            <Flex horizontal fill="header hidden">
              <NumberInput
                {...props}
                min={0}
                max={7}
                value={props.value?.days}
                onChange={(days) =>
                  props.onChange({
                    ...props.value,
                    days,
                    hours: days != null && days >= 7 ? 0 : props.value?.hours,
                    minutes:
                      days != null && days >= 7 ? 0 : props.value?.minutes,
                  })
                }
              />
              <TextLabel className={styles.textLabel}>
                {t('plainText.days')}
              </TextLabel>
            </Flex>
            <Flex horizontal fill="header hidden">
              <NumberInput
                min={0}
                max={props.value?.days >= 7 ? 0 : 23}
                value={props.value?.hours}
                readOnly={props.readOnly}
                onChange={(hours) =>
                  props.onChange({
                    ...props.value,
                    hours,
                  })
                }
              />
              <TextLabel className={styles.textLabel}>
                {t('plainText.hours')}
              </TextLabel>
            </Flex>
            <Flex horizontal fill="header hidden">
              <NumberInput
                min={0}
                max={props.value?.days >= 7 ? 0 : 59}
                value={props.value?.minutes}
                readOnly={props.readOnly}
                onChange={(minutes) =>
                  props.onChange({
                    ...props.value,
                    minutes,
                  })
                }
              />
              <TextLabel className={styles.textLabel}>
                {t('plainText.minutes')}
              </TextLabel>
            </Flex>
          </Flex>
          <FormattedDuration duration={props.value} />
        </Flex>
      )}
    </FormControl>
  )
}
