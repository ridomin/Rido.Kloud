export default {
    data() {
        return {
            propsUpdating: false,
            wpSyncs : []
        }
    },
    props: ['reportedNode', 'desiredNode', 'propName', 'deviceId'],
    methods: {
        gv(object, string, defaultValue = '') {
            // https://stackoverflow.com/questions/70283134
            return _.get(object, string, defaultValue)
        },
        syncDesiredProp(name) {
            const desSet = this.gv(this.desiredNode, name)
            if (desSet === '') {
                this.wpSyncs[name] = 'beige'
                return
            }
            const desV = this.gv(this.desiredNode, '$metadata[name].$lastUpdatedVersion')
            const repV = this.gv(this.reportedNode, name + '.av')
            this.wpSyncs[name] = repV >= desV ? 'lightgreen' : 'lightpink'
        },
        async updateProp(name, tc) {
            this.propsUpdating = true
            const url = `/api/Devices/${this.deviceId}`
            const input = document.getElementById('in-' + name)
            const desValue = {
                properties: {
                    desired: {}
                }
            }
            desValue.properties.desired[name] = tc(input.value)
            //this.device.properties.desired.interval = desValue
            console.log(desValue)
            const payload = JSON.stringify(desValue)
            try {
                await (await fetch(url, { method: 'POST', body: payload, headers: { 'Content-Type': 'application/json' } }))
                setTimeout(async () => {
                   // await this.fetchData()
                }, 2000)


            } catch (e) {
                console.log(e)
            }
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        },
    },
    template: `
        <div class="prop">
            <span class="prop-name">{{propName}}</span>
            <span class="prop-value">{{gv(reportedNode, propName +'.value')}}</span>
            desired
            <input size="1" :value="gv(desiredNode, propName)" type="text" :id="'in-'+propName" />
            <button @click="updateProp(propName,parseInt)">Update</button>
            v: {{gv(desiredNode,'$version')}}
            lu: {{gv(desiredNode,'$metadata.' + propName + '.$lastUpdatedVersion')}}
            <div v-if="!this.propsUpdating" class="props-metadata" :style="{backgroundColor: this.wpSyncs[propName]}">
                <div class="prop-md">
                    <span>last updated:</span>
                    <span>{{formatDate(gv(reportedNode, '$metadata[propName].$lastUpdated'))}}</span>
                </div>
                <div class="prop-md">
                    <span>version:</span>
                    <span>{{gv(reportedNode, propName + '.av')}}</span>
                </div>
                <div class="prop-md">
                    <span>status:</span>
                    <span>{{gv(reportedNode, propName + '.ac')}}</span>
                </div>
                <div class="prop-md">
                    <span>descr:</span>
                    <span>{{gv(reportedNode, propName + '.ad')}}</span>
                </div>
            </div>
        </div>
    `
}