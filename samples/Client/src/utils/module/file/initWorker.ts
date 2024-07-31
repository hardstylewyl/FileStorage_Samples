export const createMd5Worker = () => new Worker(new URL('./worker-md5.ts', import.meta.url), { type: 'module' })
