import Progress from './Progress'

export default function TicketProgress({ className, isOpen, tasks }) {
  const totalTasksCount = tasks.length
  const completedTasksCount = tasks.reduce(
    (accumulator, task) => (task.isCompleted ? accumulator + 1 : accumulator),
    0
  )

  return (
    <span className={className}>
      {isOpen ? (
        `${totalTasksCount} ${totalTasksCount > 1 ? 'Tasks' : 'Task'}`
      ) : (
        <Progress min={completedTasksCount} max={totalTasksCount} />
      )}
    </span>
  )
}
