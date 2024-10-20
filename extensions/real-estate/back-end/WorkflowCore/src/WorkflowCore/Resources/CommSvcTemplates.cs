namespace WorkflowCore
{
    public static class CommSvc
    {
        public static class Templates
        {
            public static class Email
            {
                public static class Tickets
                {
                    public static string Created    = "TicketCreated";
                    public static string Assigned   = "TicketAssigned";
                    public static string Reassigned = "TicketReassigned";
                    public static string Updated    = "TicketUpdated";
                }

                public static class Inspections
                {
                    public static string Summary    = "InspectionSummary";
                }
            }
        }
    }
}
