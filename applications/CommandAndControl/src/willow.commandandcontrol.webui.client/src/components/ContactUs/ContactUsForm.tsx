import { Button, Icon, Modal, Select, TextInput, Textarea } from "@willowinc/ui";
import { useEffect, useRef } from "react";
import { Controller, useForm } from "react-hook-form";
import { styled } from "twin.macro";
import useUserInfo from "../../hooks/useUserInfo";
import { useSnackbar } from "../../providers/SnackbarProvider/SnackbarProvider";
import { IPostContactUsDto, PostContactUsDto } from "../../services/Clients";
import { isValidEmail } from "../../utils/validationRules";
import useCreateTicket from "./useCreateTicket";

const ContactUsForm: React.FC<ContactUsFormProps> = ({ isFormOpen, onClose, onSubmitForm }) => {
  const user = useUserInfo();

  useEffect(() => {
    if (user) {
      reset({
        requestersName: user.userName,
        requestersEmail: user.userEmail,
      });
    }
  }, [user?.userEmail]);

  const isUserEmailValid = isValidEmail(user.userEmail) &&
  !(user.userEmail || "").toLowerCase().includes("support@willowinc.com");

  const formDetails: IPostContactUsDto = {
    requestersName: isUserEmailValid
      ? user?.userName
      : "",
    requestersEmail: isUserEmailValid ? user.userEmail : "",
    comment: "",
  }

  const createTicketMutation = useCreateTicket();
  const { register, control, handleSubmit, reset } = useForm({
    defaultValues: formDetails,
  });
  const snackbar = useSnackbar();

  const onSubmit = (formData: IPostContactUsDto) => {

    const data = new PostContactUsDto({ ...formData, url: window.location.href }); // Adding browser url as part of the request

    // Passing insightsIds in request only if "Insight" category is selected
    createTicketMutation.mutate(data,
      {
        onError: () => {
          snackbar.show("An error has occurred", { variant: "error" });
        },
        onSuccess: () => {
          // Display snackbar messages depending on whether an insight or insights have been reported
          // or a support request has been submitted
          snackbar.show("Support request submitted", { variant: "success", closeButtonProps: { label: "Dismiss" } });
          reset();
          onClose?.(false);
          onSubmitForm?.();
        },
      }
    );
  };

  const onCloseModal = () => {
    reset();
    onClose?.(false);
  };

  const formRef = useRef<HTMLFormElement | null>(null);

  return (
    <StyledModal
      opened={!!isFormOpen}
      onClose={onCloseModal}
      header="Contact us"
      size="sm"
    >
      <Form onSubmit={handleSubmit(onSubmit)} noValidate ref={formRef}>
        <StyledFieldSet>
          <Controller
            name="requestersName"
            control={control}
            rules={{ required: "Name is required" }}
            render={({ fieldState: { error }, field }) => (
              <TextInput
                label="Name"
                required
                error={error?.message}
                {...field}
              />
            )}
          />
        </StyledFieldSet>
        <StyledFieldSet>
          <Controller
            name="requestersEmail"
            control={control}
            rules={{ required: true }}
            render={({ fieldState: { error }, field: { value } }) => (
              <TextInput
                {...register("requestersEmail", {
                  required: "Email is required",
                  validate: {
                    validEmail: (email: any) => {
                      // disallow Willow internal user from putting "support@willowinc.com" in the email field
                      // when submitting a contact us form on multi-tenant
                      // no need to translate, internal use only
                      // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/95416
                      if (
                        (email || "")
                          .toLowerCase()
                          .includes("support@willowinc.com")
                      ) {
                        return "Please use an email address other than \"support @willowinc.com\"";
                      }
                      return (
                        isValidEmail(email) || "Enter a valid email address"
                      );
                    },
                  },
                })}
                required
                label="Email Address"
                error={error?.message}
                value={value}
              />
            )}
          />
        </StyledFieldSet>
        <StyledFieldSet>
          <Controller
            name="comment"
            control={control}
            rules={{ required: "Comment is required" }}
            render={({ fieldState: { error }, field }) => (
              <Textarea
                label="How can we help you?"
                required
                error={error?.message}
                {...field}
              />
            )}
          />
        </StyledFieldSet>
        <StyledFieldSet>
          <Button
            loading={createTicketMutation.isPending}
            size="large"
            type="submit"
            tw="float-right"
          >
            {"Submit"}
          </Button>
        </StyledFieldSet>
      </Form>
    </StyledModal>
  )
}

export default ContactUsForm

const Form = styled.form(({ theme }) => ({
  padding: "0px 28px",

  "input:-webkit-autofill, input:-webkit-autofill:hover, input:-webkit-autofill:focus, input:-webkit-autofill:active":
  {
    filter: "none",
    // by setting the transition to 50000000s, we are effectively prevent the autofill
    // background color change from happening
    transition: "background-color 50000000s",
    "-webkit-text-fill-color": `${theme.color.neutral.fg.default} !important`,
  },
}))

const StyledModal = styled(Modal)(({ theme }) => ({
  "> div": {
    backgroundColor: "transparent",

    "> section": {
      position: "absolute",
      bottom: theme.spacing.s24,
      right: theme.spacing.s24,
      width: "380px",
      height: "383px",
      overflowY: "auto",
    },
  },
}))

const StyledFieldSet = styled.div(({ theme }) => ({
  marginTop: theme.spacing.s16,

  "> div": {
    gap: theme.spacing.s4,
    width: "100%",
  },
}))

interface ContactUsFormProps {
  siteId?: string
  isFormOpen?: boolean
  insightIds?: string[]
  onClose?: (nextBoolean: boolean) => void
  onSubmitForm?: () => void
  onClearInsightIds?: () => void
}

