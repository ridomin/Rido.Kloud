export default {
    data: () => ({
        devices: []
    }),

    created() {
        this.fetchData()
    },

    methods: {
        async fetchData() {
            const url = `/devices/list`
            this.devices = await (await fetch(url)).json()
        },
        async getStats(d) {
            const url = `/pnp/${d.deviceId}/commands/getRuntimeStats`
            try {
                const resp = await (await fetch(url, { method: 'POST', body: 2, headers: { 'Content-Type': 'application/json' } })).json()
                d.stats = JSON.stringify(resp, null, 2)
            } catch {
                d.stats = "Offline"
            }
        }
    }
}