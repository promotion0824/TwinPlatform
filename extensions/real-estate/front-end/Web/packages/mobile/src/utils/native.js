function androidFunctionExists(functionName) {
  return window.Android?.[functionName] != null
}

function iosFunctionExists(functionName) {
  return window.webkit?.messageHandlers?.[functionName]?.postMessage != null
}

function callFunction(functionName, ...args) {
  if (androidFunctionExists(functionName)) {
    return window.Android[functionName](...args)
  }

  if (iosFunctionExists(functionName)) {
    return window.webkit.messageHandlers[functionName].postMessage(args)
  }

  return undefined
}

export default {
  getDeviceId() {
    return new Promise((resolve) => {
      if (window.native == null) window.native = {}
      window.native.setDeviceId = (deviceId) => resolve(deviceId)

      if (window.Android?.getDeviceId != null) {
        window.Android.getDeviceId()
        return
      }

      if (window.webkit?.messageHandlers?.getDeviceId?.postMessage != null) {
        window.webkit.messageHandlers.getDeviceId.postMessage([])
        return
      }

      resolve(null)
    })
  },

  getNativeType() {
    if (window.Android != null) {
      return 'android'
    }

    if (window.webkit != null) {
      return 'ios'
    }

    return 'web'
  },

  scroll(scrollTop) {
    return callFunction('scroll', scrollTop)
  },

  showImage(image) {
    return callFunction('showImage', image)
  },

  showImageBase64(base64) {
    return callFunction('showImageBase64', base64)
  },

  showPdf(pdfUrl) {
    return callFunction('showPdf', pdfUrl)
  },

  functionExists(functionName) {
    return (
      androidFunctionExists(functionName) || iosFunctionExists(functionName)
    )
  },
}
