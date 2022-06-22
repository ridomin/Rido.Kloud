
export default {
    data: () => ({
        device: {},
        propsUpdating: false,
        wpSyncs : []
    }),

    created() {
        this.fetchData()
    },
    methods: {
        async fetchData() {
            var id = new URLSearchParams(window.location.search).get('id')
            const url = `/api/Devices/${id}`
            this.device = await (await fetch(url)).json()
        },
        async handlePropUpdate(name, val) {
            this.device.properties.desired[name] = ''
            this.device.properties.reported[name] = ''
            const url = `/api/Devices/${this.device.deviceId}`
            const desValue = {
                properties: {
                    desired: {}
                }
            }
            if (name === 'interval') desValue.properties.desired[name] = parseInt(val)
            if (name === 'enabled') desValue.properties.desired[name] = val === 'true'
            const payload = JSON.stringify(desValue)
            try {
                await (await fetch(url, { method: 'POST', body: payload, headers: { 'Content-Type': 'application/json' } }))
                setTimeout(async () => {
                    await this.fetchData()
                }, 2000)


            } catch (e) {
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