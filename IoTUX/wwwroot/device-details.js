import _ from 'https://unpkg.com/lodash-es'
export default {
    data: () => ({
        device: {},
        propsUpdating :false,
        command: {
            request: 2
        }
    }),

    created() {
        this.fetchData()
    },

    methods: {
        async fetchData() {
            var urlParams = new URLSearchParams(window.location.search)
            var id = urlParams.get('id')
            const url = `/api/Devices/${id}`
            this.device = await (await fetch(url)).json()
        },
        async invoke() {
            const url = `/api/Command/${this.device.deviceId}?cmdName=getRuntimeStats`
            this.command.response = '.. loading ..'
            console.log(this.command.request)
            try {
                const resp = await (await fetch(url, { method: 'POST', body: `\"${this.command.request}\"`, headers: { 'Content-Type': 'application/json' } })).json()
                this.command.response = JSON.stringify(resp, null, 2)
            } catch {
                this.command.response = "Offline"
            }
        },
        async updateProp(name) {
            this.propsUpdating = true
            const url = `/api/Devices/${this.device.deviceId}?propName=${name}`
            const input = document.getElementById('in-' + name)
            const desValue = input.value
            this.device.properties.desired.interval = desValue
            console.log(desValue)
            try {
                await (await fetch(url, { method: 'PUT', body: JSON.stringify(desValue), headers: { 'Content-Type': 'application/json' } }))

                setTimeout(async () => {
                    const url = `/api/Devices/${this.device.deviceId}`
                    this.device = await (await fetch(url)).json()
                    this.propsUpdating = false
                }, 2000)


            } catch (e){
                console.log(e)
            }
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        },
        gv(object, string, defaultValue = '') {
            // https://stackoverflow.com/questions/70283134
            return _.get(object, string, defaultValue)
        }
    }
}