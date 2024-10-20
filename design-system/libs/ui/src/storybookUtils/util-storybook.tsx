// Not used anywhere, consider remove after confirmation
export const SupportStatus = ({ __filename }: { __filename: string }) => (
  <>
    <h2 className="sbdocs sbdocs-h2 css-idch3x">
      Supported behavior (WIP, ignore)
    </h2>
    <p className="sbdocs sbdocs-p css-1p8ieni">
      You can use this component in the below situations. If you wish to use
      this outside of these, you must read our testing page (link) and add a
      test case that describes your use case, to be covered by our automated
      test suite.
    </p>

    <ul className="sbdocs sbdocs-ul css-122ju8q">
      <li className="sbdocs sbdocs-li css-1ta8r1d">List of tests</li>
      <li className="sbdocs sbdocs-li css-1ta8r1d">
        that tell us what we can do with it
      </li>
    </ul>

    <p className="sbdocs sbdocs-p css-1p8ieni">Further tested behavior:</p>

    <ul className="sbdocs sbdocs-ul css-122ju8q">
      <li className="sbdocs sbdocs-li css-1ta8r1d">In an accordion</li>
      <li className="sbdocs sbdocs-li css-1ta8r1d">More tests</li>
      <li className="sbdocs sbdocs-li css-1ta8r1d">
        That are more boring or we don't want to display to people
      </li>
    </ul>

    <p className="sbdocs sbdocs-p css-1p8ieni">
      <em>Meta: data computed for {__filename}</em>
    </p>
  </>
)

export const SourceLink = ({
  name,
  groupName,
}: {
  name: string
  groupName: string
}) => {
  const repoUrlBase =
    'https://github.com/WillowInc/TwinPlatform/blob/main/design-system/libs/ui/src/lib/'
  const repoTsxLink = repoUrlBase + groupName + '/' + name + '/'
  return (
    <p>
      <a href={repoTsxLink} target="_blank" rel="noreferrer">
        View source
      </a>
    </p>
  )
}
