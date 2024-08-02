import Message from 'vue-m-message'
import 'vue-m-message/dist/style.css'

function info(msg: string, duration: number = 2000) {
    Message.info(msg, {
        duration,
        zIndex: 2500
    })
}

function success(msg: string, duration: number = 2000) {
    Message.success(msg, {
        duration,
        zIndex: 2500
    })
}

function error(msg: string, duration: number = 2000) {
    Message.error(msg, {
        duration,
        zIndex: 2500
    })
}

function warning(msg: string, duration: number = 2000) {
    Message.warning(msg, {
        duration,
        zIndex: 2500
    })
}

export const messagePrompt = {
    info,
    success,
    error,
    warning
}



