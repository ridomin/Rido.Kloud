export default {
    data: () => ({
        device: {},
        commands: [{}]
    }),

    created() {
        this.fetchData()
    },

    methods: {
        async fetchData() {
            var urlParams = new URLSearchParams(window.location.search)
            var id = urlParams.get('id')
            const url = `/api/Devices/${id}`
            console.log(url)
            this.device = await (await fetch(url)).json()
            console.log(this.device)
        },
        async invoke() {
            const url = `/api/Devices/${d.deviceId}/commands/getRuntimeStats`
            try {
                const resp = await (await fetch(url, { method: 'POST', body: 2, headers: { 'Content-Type': 'application/json' } })).json()
                commands[0].response = JSON.stringify(resp, null, 2)
            } catch {
                commands[0].response = "Offline"
            }
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        }
    }
}