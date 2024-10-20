/** Creates a file with the provided content and filename and triggers a download for it. */
export default function downloadTextFile(content: string, filename: string) {
  const element = document.createElement('a')
  element.href = `data:text/plain;base64,${window.btoa(content)}`
  element.download = filename
  element.click()
}
