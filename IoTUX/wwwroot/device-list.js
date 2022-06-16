export default {
    data: () => ({
        config: '',
        devices: [],
        error: ''
    }),
    created() {
        this.fetchData()
    },
    methods: {
        async fetchData() {
            this.config = await (await fetch('api/Config')).text()
            try {
                this.devices = await (await fetch(`/api/Devices`)).json()
            } catch (e) {
                console.log(e)
                this.error = 'Error loading devices'
            }
        },
        async remove(index, id) {
            const url = `api/Devices/${id}`
            const resp = await (await fetch(url, { method: 'DELETE', headers: { 'Content-type': 'application/json' } }))
            this.devices.splice(index, 1)
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        }
    }
}