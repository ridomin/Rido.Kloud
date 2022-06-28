import * as creds from './creds.js'

export default {
    data: () => ({
        hostname: '',
        config: '',
        devices: [],
        loading: true,
        error: ''
    }),
    created() {
        this.fetchData()
    },
    methods: {
        async fetchData() {
            this.hostname = creds.hostname
            const client = mqtt.connect(`wss://${creds.hostname}:8884/mqtt`, { 
                clientId: creds.clientId, username: creds.username, password: creds.pwd })

            client.on('error', e => console.error(e))
            client.on('connect', () => {
                console.log('connected', client.connected)
                client.subscribe('pnp/+/birth')
              })
            client.on('message', (topic, message) => {
                const msg = JSON.parse(message)
                // console.log(topic, msg)
                const deviceId = topic.split('/')[1]
                const dix = this.devices.findIndex (d => d.deviceId === deviceId)
                if (dix === -1) {
                    this.devices.push({deviceId, status: msg.status, modelId: msg['model-id'], when: msg.when})
                } else {
                    this.devices[dix] = {deviceId, status: msg.status, modelId: msg['model-id'], when: msg.when}
                }
                this.devices.sort((a,b) => new Date(a.when) > new Date(b.when) )
              })
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        }
    }
}