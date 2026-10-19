import { getJson, postJson } from '../../lib/apiClient'
import type { CreateTaskPayload, MatterListItem, MatterTask } from './types'

export async function getMatterList(token: string): Promise<MatterListItem[]> {
  return getJson<MatterListItem[]>('/matters', token)
}

export async function getTaskList(token: string, status?: string): Promise<MatterTask[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  return getJson<MatterTask[]>(`/tasks${query}`, token)
}

export async function createTask(token: string, payload: CreateTaskPayload): Promise<MatterTask> {
  return postJson<MatterTask, CreateTaskPayload>('/tasks', payload, token)
}

export async function updateTaskStatus(token: string, taskId: string, status: MatterTask['status']): Promise<MatterTask> {
  return postJson<MatterTask, { status: MatterTask['status'] }>(`/tasks/${taskId}/status`, { status }, token)
}
