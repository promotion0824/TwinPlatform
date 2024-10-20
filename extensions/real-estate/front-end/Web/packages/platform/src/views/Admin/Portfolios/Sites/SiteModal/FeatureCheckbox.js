import { useForm, Checkbox } from '@willow/ui'

export default function FeatureCheckbox({ feature, readOnly, children }) {
  const form = useForm()

  const isInversed = feature.endsWith('Disabled')

  return (
    <Checkbox
      value={
        isInversed ? !form.data.features[feature] : form.data.features[feature]
      }
      readOnly={readOnly}
      onChange={(value) => {
        form.setData((prevData) => ({
          ...prevData,
          features: {
            ...prevData.features,
            [feature]: isInversed ? !value : value,
          },
        }))
      }}
    >
      {children}
    </Checkbox>
  )
}
