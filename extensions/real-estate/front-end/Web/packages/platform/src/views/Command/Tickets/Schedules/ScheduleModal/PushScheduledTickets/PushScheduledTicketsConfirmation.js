import Assets from './Fields/Assets'
import ScheduleDetails from './Fields/ScheduleDetails'
import PushScheduledTicketSubmitButton from './PushScheduledTicketSubmitButton'

export default function PushScheduledTicketsConfirmation({ newAssets }) {
  return (
    <>
      <ScheduleDetails />
      <Assets newAssets={newAssets} />
      <PushScheduledTicketSubmitButton />
    </>
  )
}
