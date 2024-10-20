import { FunctionComponent, ReactNode } from 'react'
import { render } from '../../jest/testUtils'

import { Select } from '../inputs/Select'
import { DateInput } from '../dates/DateInput'
import { DateTimeInput } from '../dates/DateTimeInput'
import { TimeInput } from '../dates/TimeInput'
import { Checkbox } from '../inputs/Checkbox'
import { CheckboxGroup } from '../inputs/CheckboxGroup'
import { Field } from '../inputs/Field'
import { InputGroup } from '../inputs/InputGroup'
import { MultiSelectTree } from '../inputs/MultiSelectTree'
import { Radio } from '../inputs/Radio'
import { RadioGroup } from '../inputs/RadioGroup'
import { SearchInput } from '../inputs/SearchInput'
import { SingleSelectTree } from '../inputs/SingleSelectTree'
import { TextInput } from '../inputs/TextInput'
import { Textarea } from '../inputs/Textarea'

const renderComponentWithProps =
  (
    Component: FunctionComponent<{
      error?: ReactNode
      description: ReactNode
    }>
  ) =>
  ({ error, description }: { error?: boolean | string; description: string }) =>
    render(<Component error={error} description={description} />)

const inputComponents: [string, ReturnType<typeof renderComponentWithProps>][] =
  [
    [
      'CheckboxGroup',
      renderComponentWithProps((props) => (
        <CheckboxGroup {...props}>
          <Checkbox label="Label" />
          <Checkbox label="Label" />
          <Checkbox label="Label" />
        </CheckboxGroup>
      )),
    ],
    ['DateInput', renderComponentWithProps(DateInput)],
    ['DateTimeInput', renderComponentWithProps(DateTimeInput)],
    [
      'Field',
      renderComponentWithProps((props) => (
        <Field {...props}>
          <input />
        </Field>
      )),
    ],
    [
      'InputGroup',
      renderComponentWithProps((props) => (
        <InputGroup {...props}>
          <div />
          <div />
        </InputGroup>
      )),
    ],
    [
      'MultiSelectTree',
      renderComponentWithProps((props) => (
        <MultiSelectTree
          data={[
            {
              id: 'asset-1',
              name: 'Ceiling',
            },
          ]}
          {...props}
        />
      )),
    ],
    [
      'RadioGroup',
      renderComponentWithProps((props) => (
        <RadioGroup {...props}>
          <Radio />
        </RadioGroup>
      )),
    ],
    ['SearchInput', renderComponentWithProps(SearchInput)],
    ['Select', renderComponentWithProps(Select)],
    [
      'SingleSelectTree',
      renderComponentWithProps((props) => (
        <SingleSelectTree
          data={[
            {
              id: 'asset-1',
              name: 'Ceiling',
            },
          ]}
          {...props}
        />
      )),
    ],
    ['Textarea', renderComponentWithProps(Textarea)],
    ['TextInput', renderComponentWithProps(TextInput)],
    ['TimeInput', renderComponentWithProps(TimeInput)],
  ]

test.each(inputComponents)(
  'should display description when description provided and error is true in %s',
  (_name, renderer) => {
    const description = 'This is a description'

    const { queryByText } = renderer({
      error: true,
      description,
    })

    expect(queryByText(description)).toBeInTheDocument()
  }
)

test.each(inputComponents)(
  'should display error when both description and error are provided in %s',
  (_name, renderer) => {
    const error = 'This field is required'
    const description = 'This is a description'
    const { queryByText } = renderer({
      error,
      description,
    })

    expect(queryByText(description)).not.toBeInTheDocument()
    expect(queryByText(error)).toBeInTheDocument()
  }
)
