const repoBaseUrl = 'https://raw.githubusercontent.com/iotmodels/iot-plugandplay-models/rido/pnp' // 'https://devicemodels.azure.com'
import * as creds from './creds.js'

const dtmiToPath = function (dtmi) {
    return `/${dtmi.toLowerCase().replace(/:/g, '/').replace(';', '-')}.json`
}

const isObject = obj => Object.prototype.toString.call(obj) === '[object Object]'

const resolveSchema = s => {
    if (!isObject(s) && s.startsWith('dtmi:')) {
        console.log('not supported schema', s)
        return null
    } else if (isObject(s) && s['@type'] === 'Enum') {
        return s.valueSchema
    } else {
        return s
    }
}

const client = mqtt.connect(`wss://${creds.hostname}:8884/mqtt`, { 
    clientId: creds.clientId, username: creds.username, password: creds.pwd })

export default {
    data: () => ({
        device: {},
        properties: [],
        commands: [],
        modelpath: ''
    }),
    created() {
        this.initModel()
        this.fetchData()
    },
    methods: {
        async initModel() {
            const qs =  new URLSearchParams(window.location.search)
            this.device = { 
                deviceId: qs.get('id'), 
                modelId: qs.get('model-id'), 
                properties: {
                    reported: {},
                    desired: {}
                }}
            this.modelpath = `${repoBaseUrl}${dtmiToPath(this.device.modelId)}`
            const model = await (await window.fetch(this.modelpath)).json()
            this.properties = model.contents.filter(c => c['@type'].includes('Property'))
            this.commands = model.contents.filter(c => c['@type'].includes('Command'))
        },
        async fetchData() {
          
            client.on('error', e => console.error(e))
            client.on('connect', () => {
                console.log('connected', client.connected)
                client.subscribe(`pnp/${this.device.deviceId}/#`)
                })
            client.on('message', (topic, message) => {
                const msg = JSON.parse(message)
                const ts = topic.split('/')
                // console.log(topic, msg)
                if (topic === `pnp/${this.device.deviceId}/birth`) {
                    this.device.connectionState = msg.status === 'online' ? 'Connected' : 'Disconnected'
                    this.device.lastActivityTime = msg.when
                }
                if (topic.startsWith(`pnp/${this.device.deviceId}/props`)) {
                    const propName = ts[3]
                    if (topic.endsWith('/set'))
                    {
                        this.device.properties.desired[propName] = msg
                    } else {
                        this.device.properties.reported[propName] = msg
                    }
                }
                if (topic === `pnp/${this.device.deviceId}/telemetry`) {
                    
                }
            })

            document.title = this.device.deviceId
        },
        async handlePropUpdate(name, val, schema) {
            const resSchema = resolveSchema(schema)
            console.log('upd', name, val, resSchema)
            this.device.properties.desired[name] = ''
            this.device.properties.reported[name] = ''
            const topic = `pnp/${this.device.deviceId}/props/${name}/set`
            let desiredValue = {}
            switch (resSchema) {
                case 'string':
                    desiredValue = val
                    break
                case 'integer':
                    desiredValue = parseInt(val)
                    break
                case 'boolean':
                    desiredValue = (val === 'true')
                    break
                case 'double':
                    desiredValue = parseFloat(val)
                    break
                default:
                    console.log('schema serializer not implemented', resSchema)
                    throw new Error('Schema serializer not implemented for' + Json.stringify(resSchema))
            }
            console.log(topic, JSON.stringify(desiredValue))
            client.publish(topic,JSON.stringify(desiredValue), {qos:1, retain: true})            
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