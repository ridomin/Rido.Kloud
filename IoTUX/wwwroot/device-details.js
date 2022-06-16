export default {
    data: () => ({
        device: {},
        command: {}
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
            const url = `/api/Command/`
            this.command.response = '.. loading ..'
            try {
                const resp = await (await fetch(url, { method: 'POST', body: '"2"', headers: { 'Content-Type': 'application/json' } })).json()
                this.command.response = JSON.stringify(resp, null, 2)
            } catch {
                this.command.response = "Offline"
            }
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        }
    }
}