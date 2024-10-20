import { useState, useEffect } from 'react'
import { useMutation } from 'react-query'
import { post3dModule } from '../../../services/ThreeDimensionModule/ThreeDimensionModuleService'

function getProgressValue(loaded, total) {
  return (loaded / total) * 100
}

export default function useUpload3dModule(options) {
  const [progress, setProgress] = useState(0)
  const config = {
    onUploadProgress: ({ loaded, total }) => {
      const ongoingProgress = getProgressValue(loaded, total)
      setProgress(ongoingProgress)
    },
  }

  const mutation = useMutation(
    ({ siteId, formData }) => post3dModule(siteId, formData, config),
    options
  )

  useEffect(() => {
    if (mutation.status === 'idle') {
      setProgress(0)
    }
  }, [mutation.status])
  return { ...mutation, progress }
}
