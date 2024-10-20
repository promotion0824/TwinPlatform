import { useState } from "react";
import styled from "@emotion/styled";
import { Button, Modal, Textarea } from "@willowinc/ui";
import { useCommands } from "../../CommandsProvider";
import { ResolvedCommandAction } from "../../../../services/Clients";

export default function ConfirmationExecuteActionModal({
  opened,
  id,
}: {
  opened: boolean;
  id?: string;
}) {
  const { handleAction, closeModal } = useCommands();

  const commentState = useState<string>();

  return (
    <Modal centered opened={opened} onClose={closeModal} header="Are you sure?">
      <SectionContainer>
        <Textarea
          placeholder="Comment"
          maxLength={255}
          onChange={(e) => {
            commentState[1](e.target.value);
          }}
        />
      </SectionContainer>
      <ButtonContainer>
        <Button kind="secondary" onClick={closeModal}>
          Cancel
        </Button>
        <Button
          kind="primary"
          onClick={() => {
            handleAction(id!, ResolvedCommandAction.Execute, commentState[0]);
            closeModal();
          }}
        >
          Execute
        </Button>
      </ButtonContainer>
    </Modal>
  );
}

const ButtonContainer = styled("div")({
  display: "flex",
  flexDirection: "row",
  justifyContent: "flex-end",
  padding: "0px 16px 16px 16px",
  gap: 16,
});

const SectionContainer = styled("div")({ padding: 16 });
