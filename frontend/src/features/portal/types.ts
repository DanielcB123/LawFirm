export type ClientMatter = {
  id: string;
  matterNumber: string;
  title: string;
  practiceArea: string;
  status: string;
};

export type ClientDocument = {
  id: string;
  fileName: string;
  documentType: string;
  uploadedAtUtc: string;
  reviewStatus: string;
};

export type SubmitClientDocumentPayload = {
  fileName: string;
  documentType: string;
};
