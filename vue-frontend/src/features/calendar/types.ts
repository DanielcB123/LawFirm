export type CalendarEntry = {
  id: string
  title: string
  entryType: 'Task' | 'Event' | 'Deadline'
  deadlineType: 'OrdinaryTask' | 'InternalTarget' | 'CourtOrdered' | 'LegallySignificant'
  ownerActorId: string
  ownerDisplayName: string
  scheduledAtUtc: string
  timeZone: string
  matterReference: string | null
  sourceReference: string | null
  isCompleted: boolean
  completedAtUtc: string | null
  reminderOffsetsMinutes: number[]
  createdAtUtc: string
}

export type CreateCalendarEntryPayload = {
  title: string
  entryType: CalendarEntry['entryType']
  deadlineType: CalendarEntry['deadlineType']
  scheduledAtUtc: string
  timeZone: string
  matterReference?: string
  sourceReference?: string
  reminderOffsetsMinutes?: number[]
}
