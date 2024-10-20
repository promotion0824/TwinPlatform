import { useEffect } from 'react'
import _ from 'lodash'
import { styled } from 'twin.macro'

import TwinEditor from './TwinEditor'
import { useTwinEditor } from './TwinEditorContext'
import TwinWarranty from './TwinWarranty'
import useTwinAnalytics from '../useTwinAnalytics'
import isPlainObject from '@willow/common/utils/isPlainObject'

export default function SummaryTab() {
  const twinAnalytics = useTwinAnalytics()
  const { twin, modelInfo, conflictedFields, warranty } = useTwinEditor()

  useEffect(() => {
    if (twin?.id) {
      twinAnalytics.trackSummaryViewed({ twin })
    }
  }, [twin?.id])

  return (
    <>
      <TwinEditor
        initialTwin={twin}
        expandedModel={modelInfo?.expandedModel}
        conflictedFields={conflictedFields}
      />

      {isPlainObject(warranty) && (
        <>
          <HrContainer>
            <hr />
          </HrContainer>
          <div css={{ padding: '1rem' }}>
            <TwinWarranty warranty={warranty} />
          </div>
        </>
      )}
    </>
  )
}

const HrContainer = styled.div({
  padding: '0 1rem',
})
