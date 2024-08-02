<script setup lang="ts">

import { useUploadQueueStore } from '@/store/uploadQueue'
import type { UploadTask } from '@/types'
import { useFileDialog } from '@vueuse/core'
import { reactive } from 'vue'

const uploadQueue = useUploadQueueStore()
const { open, onChange } = useFileDialog({ multiple: true, accept: '*' })
onChange(files => {
    if (!files) return
    for (let file of files) {
        uploadQueue.startUploadTask(file)
    }
})

const pauseTaskList = reactive<UploadTask[]>([])

function pauseTask(task: UploadTask) {
    if (task.uploadStatus !== 'uploading') return
    uploadQueue.pauseTask(task)
    pauseTaskList.push(task)
}

function inPauseCancelTask(task: UploadTask) {
    uploadQueue.cancelTask(task)
    pauseTaskList.splice(pauseTaskList.indexOf(task), 1)
}

function resumeTask(task: UploadTask) {
    if (task.uploadStatus !== 'paused') return
    uploadQueue.resumeTask(task)
    pauseTaskList.splice(pauseTaskList.indexOf(task), 1)
}

</script>

<template>
    <div style="padding: 1rem;">
        <h1>上传队列测试</h1>
        <VBtn color="green" @click="open()">选择文件开始上传</VBtn>

        <h3>等待队列 数量:{{ uploadQueue.waitingQueue.length }}</h3>
        <div>
            <div v-for="(task, index) in uploadQueue.waitingQueue" :key="index">
                {{ `${index}. ${task.filename}` }}&nbsp;
            </div>
        </div>

        <h3>暂停队列 数量:{{ pauseTaskList.length }}</h3>

        <div v-for="(task, index) in pauseTaskList" :key="index">
            <h3>{{ index }}. {{ task.filename }}任务状态 :{{ task.uploadStatus }}</h3>
            <div style="width: 600px;">
                hashProgress:{{ (task.hashProgress).toFixed(2) + '%' }}
                <VProgressLinear color="primary" :model-value="task.hashProgress" />
                uploadProgress:{{ (task.uploadProgress).toFixed(2) + '%' }}
                <VProgressLinear color="green" :model-value="task.uploadProgress" />
            </div>
            <div>
                <VBtnGroup>
                    <VBtn @click="inPauseCancelTask(task)" color="red">取消</VBtn>
                    <VBtn @click="resumeTask(task)" color="green">恢复</VBtn>
                </VBtnGroup>
            </div>
        </div>


        <h3>上传队列 数量:{{ uploadQueue.runningQueue.length }}</h3>
        <div v-for="(task, index) in uploadQueue.runningQueue" :key="index">
            <h3>{{ index }}. {{ task.filename }}任务状态 :{{ task.uploadStatus }}</h3>
            <div style="width: 600px;">
                hashProgress:{{ (task.hashProgress).toFixed(2) + '%' }}
                <VProgressLinear color="primary" :model-value="task.hashProgress" />
                uploadProgress:{{ (task.uploadProgress).toFixed(2) + '%' }}
                <VProgressLinear color="green" :model-value="task.uploadProgress" />
            </div>
            <div>
                <VBtnGroup>
                    <VBtn @click="uploadQueue.cancelTask(task)" color="red">取消</VBtn>
                    <VBtn @click="pauseTask(task)" color="warning">暂停</VBtn>
                </VBtnGroup>
            </div>
        </div>


    </div>
</template>
