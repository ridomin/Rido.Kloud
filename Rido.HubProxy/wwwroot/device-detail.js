const params = new Proxy(new URLSearchParams(window.location.search), {
    get: (searchParams, prop) => searchParams.get(prop),
})

export default {
    data: () => ({
        device: {}
    }),

    created() {
        this.fetchData()
    },

    methods: {
        async fetchData() {

            const id = params.id
            this.device.id = id

            const baseUrl = `pnp/${id}/`
            this.device.started = await (await fetch(baseUrl + 'props/started')).json()
            this.device.interval = await (await fetch(baseUrl + 'props/interval')).json()
            this.device.enabled = await (await fetch(baseUrl + 'props/enabled')).json()
            console.log(this.device)
        },
        async getStats() {
            const url = `/pnp/${params.id}/commands/getRuntimeStats`
            try {
                const resp = await (await fetch(url, { method: 'POST', body: 2, headers: { 'Content-Type': 'application/json' } })).json()
                this.device.stats = JSON.stringify(resp, null, 2)
            } catch {
                this.device.stats = "Offline"
            }
        },
        async setInterval() {
            const url = `/pnp/${params.id}/props/interval`
            const body = JSON.stringify(this.device.interval)
            try {
                const resp = await (await fetch(url, { method: 'POST', body, headers: { 'Content-Type': 'application/json' } })).json()
                this.device.stats = JSON.stringify(resp, null, 2)
            } catch {
                console.log("err set interval")
            }
        },
        async setEnabled() {

        },
        formatDate(d) {
            return moment(d).fromNow()
        }
    }
}