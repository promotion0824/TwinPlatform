/**
 * Exports text to the clipboard
 * @param text 
 */
const useCopyToClipboard = (text: string) => {
    const listener = (e: ClipboardEvent) => {
        e.clipboardData?.setData('text/plain', text);
        e.preventDefault();
        document.removeEventListener('copy', listener);
    };
    document.addEventListener('copy', listener);
    document.execCommand('copy');
};

export default useCopyToClipboard;