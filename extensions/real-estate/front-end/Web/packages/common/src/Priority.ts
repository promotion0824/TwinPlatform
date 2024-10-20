export type Priority =
  | {
      id: 1
      name: 'Critical'
      color: 'red'
    }
  | {
      id: 2
      name: 'High'
      color: 'orange'
    }
  | {
      id: 3
      name: 'Medium'
      color: 'yellow'
    }
  | {
      id: 4
      name: 'Low'
      color: 'blue'
    }

/**
 * possible priority values for Insights and Tickets
 */
export const priorities: Priority[] = [
  {
    id: 1,
    name: 'Critical',
    color: 'red',
  },
  {
    id: 2,
    name: 'High',
    color: 'orange',
  },
  {
    id: 3,
    name: 'Medium',
    color: 'yellow',
  },
  {
    id: 4,
    name: 'Low',
    color: 'blue',
  },
]

export type PriorityIds = 1 | 2 | 3 | 4
