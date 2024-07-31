import {
	type CheckFileRequest,
	type CheckFileResponse,
	type DirectUploadRequest,
	DirectUploadRequestToFromData,
	type DirectUploadResponse,
	type FragmentUploadCreateRequest,
	type FragmentUploadRequest,
	FragmentUploadRequestToFromData,
	type TusAddUploadRequest,
	urls,
} from '@/contracts'
import type { PreviousUpload } from 'tus-js-client'
import { getRequest, getRequestR, postRequest, postRequestR } from './alova'

export const api = {
	// FileBase
	CheckFile: (request: CheckFileRequest) => postRequestR<CheckFileResponse>(urls.CheckFile, request),
	DirectUpload: (request: DirectUploadRequest) =>
		postRequestR<DirectUploadResponse>(urls.DirectUpload, DirectUploadRequestToFromData(request)),
	// Native
	FragmentUploadCreate: (request: FragmentUploadCreateRequest) => postRequest(urls.FragmentUploadCreate, request),
	FragmentUpload: (request: FragmentUploadRequest) =>
		postRequest(urls.FragmentUpload, FragmentUploadRequestToFromData(request)),
	// Tus
	// Tus: ()=>void 0,
	// TusUrlStorageApi
	FindAllUploads: () => getRequestR<PreviousUpload[]>(urls.FindAllUploads),
	FindUploadsByFileId: (fildId: string) => getRequestR<PreviousUpload>(urls.FindUploadsByFileId, { fildId }),
	RemoveUpload: (fildId: string) => getRequest(urls.RemoveUpload, { fildId }),
	AddUpload: (request: TusAddUploadRequest) => postRequest(urls.AddUpload, request),
}
