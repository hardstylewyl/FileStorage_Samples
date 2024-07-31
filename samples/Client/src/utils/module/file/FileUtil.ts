/*File按指定大小进行切片*/
export function FileSlicing(file: File, chunkSize: number): Blob[] {
	if (!(file instanceof File)) throw new TypeError('Expected a File instance')
	const totalSize = file.size
	const chunks: Blob[] = []

	for (let offset = 0; offset < totalSize; offset += chunkSize) {
		const sliceEnd = Math.min(offset + chunkSize, totalSize)
		const chunk = file.slice(offset, sliceEnd)
		chunks.push(chunk)
	}

	return chunks
}

/*File转Bse64*/
export function FileToBase64(file: File): Promise<string> {
	return new Promise((resolve, reject) => {
		const reader = new FileReader()
		reader.readAsDataURL(file)
		reader.onload = () => {
			resolve(reader.result as string)
		}
		reader.onerror = (error) => {
			reject(error)
		}
	})
}

/*Base64转File*/
export function Base64ToFile(base64: string, fileName: string): File {
	const arr = base64.split(',')
	const mime = arr[0].match(/:(.*?);/)![1]
	const bstr = atob(arr[1])

	// 直接创建Uint8Array
	const u8arr = new Uint8Array(Array.from(bstr, char => char.charCodeAt(0)))

	return new File([u8arr], fileName, { type: mime })
}

/*Blob转File*/
export function BlobToFile(blob: Blob, fileName: string): File {
	return new File([blob], fileName, { type: blob.type })
}
