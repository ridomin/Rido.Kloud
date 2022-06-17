export default {
    data: () => ({
        device: {},
        propsUpdating: false,
        wpSyncs : [],
        command: {
            request: 2
        }
    }),

    created() {
        this.fetchData()
    },

    methods: {
        async fetchData() {
            this.propsUpdating = true
            var urlParams = new URLSearchParams(window.location.search)
            var id = urlParams.get('id')
            const url = `/api/Devices/${id}`
            this.device = await (await fetch(url)).json()
            this.syncDesiredProp('interval')
            this.syncDesiredProp('enabled')
            this.propsUpdating = false
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
        syncDesiredProp(name) {
            const desSet = this.gv(this.device, `properties.desired.${name}`)
            console.log(name, desSet)
            if (desSet === '') {
                this.wpSyncs[name] = 'beige'
                return 
            }
            const desV = this.gv(this.device, 'properties.desired.$version')
            const repV = this.gv(this.device, `properties.reported.${name}.av`)
            this.wpSyncs[name] = desV === repV ? 'lightgreen' : 'lightpink'
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
                    await this.fetchData()
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