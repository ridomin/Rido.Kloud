export default {
    data: () => ({
        device: {},
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
                const resp = await (await fetch(url, { method: 'POST', body: `\"${this.command.request}\"` , headers: { 'Content-Type': 'application/json' } })).json()
                this.command.response = JSON.stringify(resp, null, 2)
            } catch {
                this.command.response = "Offline"
            }
        },
        async updateProp(name) {
            const url = `/api/Twin/${this.device.deviceId}`
            const input = document.getElementById('in-' + name)
            const desValue = input.value
            console.log(desValue)
            //try {
            //    const resp = await (await fetch(url, { method: 'POST', body: `\"${this.command.request}\"`, headers: { 'Content-Type': 'application/json' } })).json()
            //    this.command.response = JSON.stringify(resp, null, 2)
            //} catch {
            //    this.command.response = "Offline"
            //}
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        }
    }
}