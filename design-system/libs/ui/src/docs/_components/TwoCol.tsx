type TwoColProps = {
  children: JSX.Element
}

const Row = (props: TwoColProps) => {
  return (
    <p className="sbdocs sbdocs-p">
      <div className="sbdocs sbdocs-div row">{props.children}</div>
    </p>
  )
}

const Col = (props: TwoColProps) => {
  return <div className="sbdocs sbdocs-div column">{props.children}</div>
}

export { Row, Col }
