import { useRef } from 'react'
import { useForm, Button, Flex, Input, Label } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function ListCheck() {
  const checkForm = useForm()
  const { t } = useTranslation()

  const newValueRef = useRef()

  function handleKeyDown(e) {
    if (e.key === 'Enter') {
      e.preventDefault()

      newValueRef.current.focus()
    }
  }

  function handleNewValueKeyDown(e) {
    if (e.key === 'Enter' && e.currentTarget.value !== '') {
      e.preventDefault()

      newValueRef.current.blur()
    }
  }

  function handleBlur(nextValue, i) {
    if (nextValue === '') {
      checkForm.setData((prevData) => ({
        ...prevData,
        typeValue: prevData.typeValue.filter((prevValue, prevI) => prevI !== i),
      }))

      newValueRef.current.focus()
    } else {
      checkForm.setData((prevData) => ({
        ...prevData,
        typeValue: prevData.typeValue.map((prevValue, prevI) =>
          prevI === i ? nextValue : prevValue
        ),
      }))
    }
  }

  function handleNewValueBlur(nextValue) {
    if (nextValue !== '') {
      checkForm.setData((prevData) => ({
        ...prevData,
        typeValue: [...prevData.typeValue, nextValue],
      }))

      newValueRef.current.focus()
    }
  }

  function handleRemoveSelection(i) {
    checkForm.setData((prevData) => ({
      ...prevData,
      typeValue: prevData.typeValue.filter((prevValue, prevI) => prevI !== i),
    }))
  }

  return (
    <>
      <Flex size="small">
        <div>
          <Label label={t('labels.selections')} />
        </div>
        <Flex size="medium">
          {checkForm.data.typeValue.map((item, i) => (
            <Flex
              key={i} // eslint-disable-line
              horizontal
              fill="header hidden"
              size="medium"
            >
              <Input
                value={item}
                onKeyDown={handleKeyDown}
                onBlur={(e) => handleBlur(e.currentTarget.value, i)}
              />
              <Button icon="close" onClick={() => handleRemoveSelection(i)} />
            </Flex>
          ))}
          <Input
            ref={newValueRef}
            placeholder={t('placeholder.enterNewValue')}
            onKeyDown={handleNewValueKeyDown}
            onBlur={(e) => handleNewValueBlur(e.currentTarget.value)}
          />
        </Flex>
      </Flex>
    </>
  )
}
