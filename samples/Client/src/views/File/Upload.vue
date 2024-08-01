<script setup lang="ts">
import type { ProgressReport, UploadTask } from '@/types';
import { FileUploadStatus } from '@/contracts';
import { uploadService } from '@/services/upload';
import { TusUploadAsync } from '@/services/upload/tus-uploader';
import { cancelTokenUtil, type CancellationToken, type CancellationTokenSource } from '@/utils';
import { useFileDialog } from '@vueuse/core';
import { NativeUploadAsync } from '@/services/upload/native-uploader';


const { open, onChange } = useFileDialog({ accept: '*', multiple: false })
onChange(handleFileSelected)

const upload_type = ref<'tus' | 'native'>('tus')

let cts: CancellationTokenSource
let task = ref<UploadTask>(null!)

async function handleFileSelected(files: FileList | null) {
    if (!files) return

    cts = cancelTokenUtil.newSource()
    task.value = CreateUploadTask(files[0], 'hashing')

    try {

        //1.构建上下文
        const context = await uploadService
            .BuildContextAsync(files[0],
                cts.token,
                buildProgressReport(task, true),
                upload_type.value === 'native')

        const { status } = task.value.context = context


        if (status === 'error' ||
            status === FileUploadStatus.inSynchronizing ||
            status === FileUploadStatus.syncFailed) {
            return
        }

        if (status === FileUploadStatus.completed) {
            task.value.uploadStatus = 'success'
            task.value.uploadProgress = 100
            return
        }

        //上传前开启一个轮询的定时器，更新上传状态
        listenerTaskStatus(cts.token)

        //2.上传
        task.value.uploadStatus = 'uploading'
        if (upload_type.value === 'tus') {
            await TusUploadAsync(context,
                cts.token,
                buildProgressReport(task, false))
        } else {
            await NativeUploadAsync(context,
                cts.token,
                buildProgressReport(task, false))
        }



    } catch (e) {
        //抛出的异常为取消
        if (cancelTokenUtil.isCancel(e)) {
            task.value.uploadStatus = 'canceled'
        } else {
            task.value.uploadStatus = 'error'
            throw e
        }
    } finally {
        cts.token.dispose()
    }

}

function cancel() {
    cts?.cancel()
}

async function pause() {
    if (!task.value.context) return
    if (task.value.uploadStatus !== 'uploading') return
    if (task.value.uploadProgress >= 98) return
    console.log('暂停当前上传的任务')
    cts.cancel()
    const promise = cts.token.promise
    await promise
    task.value.uploadStatus = 'paused'
}

async function resume() {
    if (!task.value.context) return
    if (task.value.uploadStatus !== 'paused') return
    console.log('恢复已经暂停的任务')
    cts = cancelTokenUtil.newSource()
    try {
        //轮询更新任务状态，可以通过取消令牌随时取消轮询工作
        listenerTaskStatus(cts.token)
        //2.使用上传上下文进行上传，可取消
        if (upload_type.value === 'tus') {
            await TusUploadAsync(task.value.context,
                cts.token,
                buildProgressReport(task, false))
        } else {
            await NativeUploadAsync(task.value.context,
                cts.token,
                buildProgressReport(task, false))
        }
    } catch (e) {
        if (cancelTokenUtil.isCancel(e)) {
            task.value.uploadStatus = 'canceled'
        } else {
            task.value.uploadStatus = 'error'
            throw e
        }
    } finally {
        cts.token.dispose()
    }
}


//构建进度报告函数
function buildProgressReport(ref: Ref<UploadTask>, isHash: boolean): ProgressReport {
    const key = isHash ? 'hashProgress' : 'uploadProgress'
    return (loaded, total) => {
        ref.value[key] = loaded * 100 / total
    }
}

//构建一个未开始的上传任务
function CreateUploadTask(file: File, initStatus: UploadTask['uploadStatus'] = 'waiting'): UploadTask {
    const task: UploadTask = {
        context: undefined,
        extension: file.name.split('.').pop() ?? '',
        file: file,
        filename: file.name,
        hashProgress: 0,
        uploadProgress: 0,
        uploadStatus: initStatus
    }

    return task
}


function listenerTaskStatus(cancelToken: CancellationToken) {
    const context = task.value.context
    if (!context) return

    let timer: NodeJS.Timeout
    /*轮询间隔,根据文件大小来确定*/
    const fileSize = context.file.size
    const interval = 500 + (fileSize / (1024 * 1024 * 37.5)) * 300

    //取消轮询
    cancelToken?.onCanceled(() => {
        clearInterval(timer)
        task.value.uploadStatus = 'canceled'
    })

    const requestStatusAsync = async () => {
        const result = await uploadService
            .UploadCheckAsync(context, cancelToken)
            .catch(_ => {
                clearInterval(timer)
                task.value.uploadStatus = 'error'
            })
            .finally(() => {
                task.value.uploadStatus = 'uploading'
            })

        if (!result) return

        switch (result.Status) {
            case FileUploadStatus.inProgress: {
                task.value.uploadStatus = 'uploading'
                break
            }
            case FileUploadStatus.inSynchronizing: {
                task.value.uploadStatus = 'syncing'
                break
            }
            case FileUploadStatus.notExist: {
                //什么也不做
                break
            }
            case FileUploadStatus.syncFailed: {
                clearInterval(timer)
                task.value.uploadStatus = 'error'
                break
            }
            case FileUploadStatus.completed: {
                clearInterval(timer)
                task.value.uploadStatus = 'success'
                task.value.uploadProgress = 100
                context.fileUrl = result.FileUrl!
                break
            }
        }
    }

    //首次执行一次
    requestStatusAsync()

    //开始轮询更新状态
    timer = setInterval(requestStatusAsync, interval)

}




</script>
<template>
    <div style="padding: 1rem;">
        <h1>{{ upload_type }}上传测试</h1>
        <h3>上传类型</h3>
        <VSelect width="300px" v-model="upload_type" :items="['tus', 'native']" />
        <VBtnGroup >
            <VBtn color="primary" @click="open()">选择一个文件开始上传</VBtn>
            <VBtn color="red" @click="cancel()">取消</VBtn>
            <VBtn color="warning" @click="pause()">暂停</VBtn>
            <VBtn color="green" @click="resume()">恢复</VBtn>
        </VBtnGroup>
        <div><h3>任务状态 :{{ task ? task.uploadStatus : 'none' }}</h3></div>
        <div v-if="task" style="width: 600px;">
            hashProgress:{{ (task?.hashProgress).toFixed(2) + '%' }}
            <VProgressLinear color="primary" :model-value="task.hashProgress" />
            uploadProgress:{{ (task?.uploadProgress).toFixed(2) + '%' }}
            <VProgressLinear color="green" :model-value="task.uploadProgress" />
        </div>
        <div v-if="task">
            <h3>任务详情</h3>
            <VTextarea :rows="20" width="1000px" :model-value="JSON.stringify(task, null, 4)" />
        </div>

    </div>

</template>