//import * as creds from './creds.js'

// const client = mqtt.connect(`wss://${creds.hostname}:8884/mqtt`, { 
//     clientId: creds.clientId + Date.now(), username: creds.username, password: creds.pwd })
import mqtt from './mqttClient.js'
let client
const isBuffer = obj => {
    return obj != null && obj.constructor != null &&
        typeof obj.constructor.isBuffer === 'function' && obj.constructor.isBuffer(obj)
}

export default {
    data: () => ({
        hostName: '',
        config: '',
        devices: [],
        loading: true,
        error: '',
        connected: false
    }),
    created() {
        // if (client) {
        //     this.fetchData()
        // }
    },
    methods: {
        async fetchData() {
            console.log(client.options.href)
            client.on('error', e => console.error(e))
            client.on('connect', () => {
                this.hostName = client.options.href
                client.subscribe('pnp/+/birth')
                this.connected = true
            })
            client.subscribe('pnp/+/birth')
            client.on('message', (topic, message) => {
                const deviceId = topic.split('/')[1]
                const dix = this.devices.findIndex(d => d.deviceId === deviceId)
                if (isBuffer(message) && message.byteLength > 0) {
                    const msg = JSON.parse(message)
                    // console.log(topic, msg)
                    if (dix === -1) {
                        this.devices.push({ deviceId, status: msg.status, modelId: msg['model-id'], when: msg.when })
                    } else {
                        this.devices[dix] = { deviceId, status: msg.status, modelId: msg['model-id'], when: msg.when }
                    }
                    this.devices.sort((a, b) => new Date(a.when) < new Date(b.when) ? 1 : -1)
                }
            })
        },
        removeDevice(did) {
            const topic = `pnp/${did}/birth`
            client.publish(topic, '', { retain: true, qos: 1 })
            const dix = this.devices.findIndex(d => d.deviceId === did)
            this.devices.splice(dix, 1)
        },
        onConfigChanged() {
            console.log('config changed')
            client = mqtt.start()
            this.fetchData()
            
        },
        disconnect() {
            this.connected = false
            client.end()
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        }
    }
}