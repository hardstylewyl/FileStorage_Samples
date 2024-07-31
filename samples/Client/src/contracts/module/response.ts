export enum FileUploadStatus {
	notExist = 1,
	inProgress = 2,
	inSynchronizing = 4,
	completed = 8,
	syncFailed = 16,
}

export interface CheckFileResponse {
	Status: FileUploadStatus
	MissChunkList: number[] | null
	FileUrl: string | null
}

export interface DirectUploadResponse {
	FileUrl: string
}
