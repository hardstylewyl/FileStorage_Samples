const baseURL = import.meta.env.VITE_API_BASE_URL
export const urls = {
	// FileBase
	CheckFile: '/CheckFile',
	DirectUpload: '/DirectUpload',
	// Native
	FragmentUploadCreate: '/Native/FragmentUploadCreate',
	FragmentUpload: '/Native/FragmentUpload',
	// Tus
	TusEndpoint: `${baseURL}/upload`,
	// TusUrlStorageApi
	FindAllUploads: '/TusStorage/FindAllUploads',
	FindUploadsByFileId: '/TusStorage/FindUploadsByFileId',
	RemoveUpload: '/TusStorage/RemoveUpload',
	AddUpload: '/TusStorage/AddUpload',
}
