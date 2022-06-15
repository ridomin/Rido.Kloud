export default {
    data: () => ({
        devices: []
    }),
    created() {
        this.fetchData()
    },
    methods: {
        async fetchData() {
            this.devices = await (await fetch(`/api/Devices`)).json()
        },
        async remove(index, id) {
            const url = `api/Devices/${id}`
            console.log(url)
            const resp = await (await fetch(url, { method: 'DELETE', headers: { 'Content-type': 'application/json' } }))
            this.devices.splice(index, 1)
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        }
    }
}