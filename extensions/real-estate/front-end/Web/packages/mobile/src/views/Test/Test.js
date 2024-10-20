import { useRef, useState } from 'react'
import { Button, Spacing } from '@willow/mobile-ui'

export default function Test() {
  const fileRef = useRef()
  const [file, setFile] = useState()
  const [messages, setMessages] = useState([])

  function addMessage(message) {
    setMessages((prevMessages) => [...prevMessages, message])
  }

  function showImage() {
    if (window.Android?.showImage1 != null) {
      const reader = new FileReader()
      reader.onload = () => {
        const dataUrl = reader.result

        addMessage('window.Android.showImage1(file.name, dataUrl)')
        window.Android.showImage1(file.name, dataUrl)
      }
      reader.readAsDataURL(file)
    }

    if (window.Android?.showImage2 != null) {
      addMessage('window.Android.showImage2(file.name, file)')
      window.Android.showImage2(file.name, file)
    }

    if (window.Android?.showImage3 != null) {
      const reader = new FileReader()
      reader.onload = () => {
        const arrayBuffer = reader.result
        addMessage('window.Android.showImage3(file.name, arrayBuffer)')
        window.Android.showImage3(file.name, arrayBuffer)
      }
      reader.readAsArrayBuffer(file)
    }

    if (window.Android?.showImage4 != null) {
      const reader = new FileReader()
      reader.onload = () => {
        const base64 = btoa(reader.result)
        addMessage('window.Android.showImage4(file.name, base64)')
        window.Android.showImage4(file.name, base64)
      }
      reader.readAsBinaryString(file)
    }

    if (window.Android?.showImage5 != null) {
      const objectUrl = URL.createObjectURL(file)
      addMessage('window.Android.showImage5(file.name, objectUrl)')
      window.Android.showImage5(file.name, objectUrl)
    }
  }

  return (
    <Spacing align="left" size="large">
      <div>
        <div>
          Add one/more of the following functions, and if it exists, clicking
          the button will call:
        </div>
        <ul>
          <li>window.Android.showImage1(file.name, dataUrl)</li>
          <li>window.Android.showImage2(file.name, file)</li>
          <li>window.Android.showImage3(file.name, arrayBuffer)</li>
          <li>window.Android.showImage4(file.name, base64)</li>
          <li>window.Android.showImage5(file.name, objectUrl)</li>
        </ul>
      </div>
      <input
        ref={fileRef}
        type="file"
        accept=".png,.jpg,.jpeg"
        onChange={(e) => {
          setFile(e.currentTarget.files[0])
        }}
      />
      <Button color="blue" disabled={file == null} onClick={() => showImage()}>
        window.Android.showImage1/2/3/4/5()
      </Button>
      <Spacing>
        {messages.map((message, i) => (
          // eslint-disable-next-line
          <div key={i}>{message}</div>
        ))}
      </Spacing>
    </Spacing>
  )
}
