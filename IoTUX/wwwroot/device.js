﻿const repoBaseUrl = 'https://iotmodels.github.io/dmr/' // 'https://devicemodels.azure.com'
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
            const dtmi = new URLSearchParams(window.location.search).get('model-id')
            this.modelpath = `${repoBaseUrl}${dtmiToPath(dtmi)}`
            const model = await (await window.fetch(this.modelpath)).json()
            this.properties = model.contents.filter(c => c['@type'].includes('Property'))
            this.commands = model.contents.filter(c => c['@type'].includes('Command'))
        },
        async fetchData() {
            const id = new URLSearchParams(window.location.search).get('id')
            const url = `/api/Devices/${id}`
            this.device = await (await fetch(url)).json()
            document.title = this.device.deviceId
        },
        async handlePropUpdate(name, val, schema) {
            const resSchema = resolveSchema(schema)
            console.log('upd', name, val, resSchema)
            this.device.properties.desired[name] = ''
            this.device.properties.reported[name] = ''
            const url = `/api/Devices/${this.device.deviceId}`
            const desValue = {
                properties: {
                    desired: {}
                }
            }
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
            
            desValue.properties.desired[name] = desiredValue
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