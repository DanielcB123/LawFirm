export type MatterListItem = {
  id: string
  matterNumber: string
  title: string
  practiceArea: string
  stage: string
  status: string
  responsibleAttorneyActorId: string | null
  responsibleAttorneyDisplayName: string | null
  paralegalActorId: string | null
  paralegalDisplayName: string | null
  openTaskCount: number
  createdAtUtc: string
}

export type MatterTask = {
  id: string
  matterId: string
  matterNumber: string | null
  matterTitle: string | null
  title: string
  description: string | null
  status: 'Open' | 'InProgress' | 'Waiting' | 'Completed'
  priority: 'Low' | 'Normal' | 'High' | 'Urgent'
  assigneeActorId: string | null
  assigneeDisplayName: string | null
  dueAtUtc: string | null
  completedAtUtc: string | null
  createdByActorId: string
  createdByDisplayName: string
  createdAtUtc: string
}

export type CreateTaskPayload = {
  matterId: string
  title: string
  description?: string
  priority?: MatterTask['priority']
  assigneeActorId?: string
  assigneeDisplayName?: string
  dueAtUtc?: string
}
