import { useMutation } from "@tanstack/react-query";
import useApi from "../../hooks/useApi";
import { PostContactUsDto } from "../../services/Clients";

const useCreateTicket = (options?: any) => {
  const api = useApi();

  return useMutation<PostContactUsDto, Error, PostContactUsDto>({
    mutationFn: (customerFormDetails: PostContactUsDto) => api.contactUs(customerFormDetails),
    ...options
  });
};

export default useCreateTicket;
