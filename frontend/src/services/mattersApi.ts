import { deleteRequest, getJson, postJson } from "./apiClient";
import type {
  CreateMatterDocumentRequest,
  CreateMatterTaskRequest,
  CreateMatterTimelineActionRequest,
  MatterDetail,
  MatterDocument,
  MatterListItem,
  MatterTask,
  MatterTimelineAction,
  ReviewMatterDocumentRequest,
  UpdateMatterTaskRequest,
} from "../types/matters";

export async function getMatterList(token: string): Promise<MatterListItem[]> {
  return getJson<MatterListItem[]>("/api/matters-v2", undefined, token);
}

export async function getMatterById(token: string, matterId: string): Promise<MatterDetail> {
  return getJson<MatterDetail>(`/api/matters-v2/${matterId}`, undefined, token);
}

export async function getMatterTasks(token: string, matterId: string): Promise<MatterTask[]> {
  return getJson<MatterTask[]>(`/api/matters-v2/${matterId}/tasks`, undefined, token);
}

export async function createMatterTask(
  token: string,
  matterId: string,
  payload: CreateMatterTaskRequest,
): Promise<MatterTask> {
  return postJson<MatterTask, CreateMatterTaskRequest>(`/api/matters-v2/${matterId}/tasks`, payload, token);
}

export async function updateMatterTask(
  token: string,
  matterId: string,
  taskId: string,
  payload: UpdateMatterTaskRequest,
): Promise<MatterTask> {
  return postJson<MatterTask, UpdateMatterTaskRequest>(`/api/matters-v2/${matterId}/tasks/${taskId}`, payload, token);
}

export async function deleteMatterTask(token: string, matterId: string, taskId: string): Promise<void> {
  return deleteRequest(`/api/matters-v2/${matterId}/tasks/${taskId}`, token);
}

export async function getMatterDocuments(token: string, matterId: string): Promise<MatterDocument[]> {
  return getJson<MatterDocument[]>(`/api/matters-v2/${matterId}/documents`, undefined, token);
}

export async function createMatterDocument(
  token: string,
  matterId: string,
  payload: CreateMatterDocumentRequest,
): Promise<MatterDocument> {
  return postJson<MatterDocument, CreateMatterDocumentRequest>(
    `/api/matters-v2/${matterId}/documents`,
    payload,
    token,
  );
}

export async function reviewMatterDocument(
  token: string,
  matterId: string,
  documentId: string,
  payload: ReviewMatterDocumentRequest,
): Promise<MatterDocument> {
  return postJson<MatterDocument, ReviewMatterDocumentRequest>(
    `/api/matters-v2/${matterId}/documents/${documentId}/review`,
    payload,
    token,
  );
}

export async function deleteMatterDocument(token: string, matterId: string, documentId: string): Promise<void> {
  return deleteRequest(`/api/matters-v2/${matterId}/documents/${documentId}`, token);
}

export async function getMatterTimelineActions(token: string, matterId: string): Promise<MatterTimelineAction[]> {
  return getJson<MatterTimelineAction[]>(`/api/matters-v2/${matterId}/timeline`, undefined, token);
}

export async function createMatterTimelineAction(
  token: string,
  matterId: string,
  payload: CreateMatterTimelineActionRequest,
): Promise<MatterTimelineAction> {
  return postJson<MatterTimelineAction, CreateMatterTimelineActionRequest>(
    `/api/matters-v2/${matterId}/timeline-actions`,
    payload,
    token,
  );
}
