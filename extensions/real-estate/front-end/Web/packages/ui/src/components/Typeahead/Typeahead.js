import { forwardRef } from 'react'
import { FormControl } from 'components/Form/Form'
import Typeahead from './Typeahead/Typeahead'

export { default as TypeaheadButton } from './Typeahead/TypeaheadButton'
export { default as TypeaheadContent } from './Typeahead/TypeaheadContent'

export default forwardRef(function InputComponent(
  { name, children, onSelect, zIndex, ...rest },
  forwardedRef
) {
  return (
    <FormControl {...rest} name={name} defaultValue="">
      {(props, form) => (
        <Typeahead
          {...props}
          ref={forwardedRef}
          onSelect={(item) => {
            form?.clearError(name)
            onSelect?.(item)
          }}
          zIndex={zIndex}
        >
          {children}
        </Typeahead>
      )}
    </FormControl>
  )
})
