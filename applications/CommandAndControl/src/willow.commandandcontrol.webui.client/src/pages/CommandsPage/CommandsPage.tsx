import Commands from "./components/Commands/Commands";
import { useParams } from "react-router-dom";
import CommandsProvider from "./CommandsProvider";
import CommandDetails from "./components/CommandDetails/CommandDetails";

export default function CommandsPage() {
  const { id } = useParams();

  return (
    <CommandsProvider selectedId={id}>
      {id ? <CommandDetails /> : <Commands />}
    </CommandsProvider>
  );
}
