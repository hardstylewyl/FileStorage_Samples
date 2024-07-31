// 请求模型
import type { PreviousUpload } from 'tus-js-client'

export interface CheckFileRequest {
	isTus: boolean
	fileId: string
}

export interface DirectUploadRequest {
	fileId: string
	filename: string
	extension: string
	md5: string
	file: File
}

export interface FragmentUploadCreateRequest {
	fileId: string
	filename: string
	extension: string
	chunkCount: number
	chunkSize: number
}

export interface FragmentUploadRequest {
	fileId: string
	chunkSeq: number
	md5: string
	file: File
}

export type TusAddUploadRequest = {
	fileId: string
	value: PreviousUpload
}

export function DirectUploadRequestToFromData(u: DirectUploadRequest) {
	const formData = new FormData()
	formData.append('FileId', u.fileId)
	formData.append('Filename', u.filename)
	formData.append('Extension', u.extension)
	formData.append('Md5', u.md5)
	formData.append('File', u.file, u.file.name)
	return formData
}

export function FragmentUploadRequestToFromData(u: FragmentUploadRequest) {
	const formData = new FormData()
	formData.append('FileId', u.fileId)
	formData.append('ChunkSeq', u.chunkSeq.toString())
	formData.append('Md5', u.md5)
	formData.append('File', u.file, u.fileId)
	return formData
}
