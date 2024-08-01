import type { App } from "vue"
import { createVuetify } from 'vuetify'
import { Ripple, ClickOutside, Touch } from 'vuetify/directives'

import 'vuetify/styles'

const vuetify = createVuetify({
    theme: {
        defaultTheme: 'light',
    }
})

export const setupVuetify = (app: App) => {
    app.use(vuetify)
    app.directive('ripple', Ripple)
    app.directive('click-outside', ClickOutside)
    app.directive('touch', Touch)
}