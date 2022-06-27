export default {
    props: ['deviceProps', 'propName'],
    emits: ['propUpdated'],
    methods: {
        gv(object, string, defaultValue = '') {
            // https://stackoverflow.com/questions/70283134
            return _.get(object, string, defaultValue)
        },
        getPropColorState(name) {
            const desSet = this.gv(this.deviceProps, 'desired.' + name)
            if (desSet === '') {
                return 'beige'
            } 
            const desV = this.gv(this.deviceProps, 'desired.$metadata.' + name + '.$lastUpdatedVersion')
            const repV = this.gv(this.deviceProps, 'reported.' + name + '.av')
            return repV >= desV ? 'lightgreen' : 'lightpink'
        },
        updateProp() {
            const input = document.getElementById('in-' + this.propName)
            this.$emit('propUpdated', this.propName, input.value)
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        }
    },
    template: `
        <div class="prop">
            <span class="prop-name">{{propName}}</span>
            <span class="prop-value">{{gv(deviceProps, 'reported.' + propName + '.value')}}</span>
            desired
            <input size="1" :value="gv(deviceProps, 'desired.' + propName)" type="text" :id="'in-' + propName" />
            <button @click="updateProp()">Update</button> 
            rv: {{gv(deviceProps,'reported.$version')}}
            dv: {{gv(deviceProps,'desired.$version')}}
            lu: {{gv(deviceProps,'desired.$metadata.' + propName + '.$lastUpdatedVersion')}}
            <div class="props-metadata" :style="{backgroundColor: getPropColorState(propName)}">
                <div class="prop-md">
                    <span>last updated:</span>
                    <span>{{formatDate(gv(deviceProps, 'reported.$metadata.' + this.propName +'.$lastUpdated'))}}</span>
                </div>
                <div class="prop-md">
                    <span>version:</span>
                    <span>{{gv(deviceProps, 'reported.' + propName + '.av')}}</span>
                </div>
                <div class="prop-md">
                    <span>status:</span>
                    <span>{{gv(deviceProps, 'reported.' + propName + '.ac')}}</span>
                </div>
                <div class="prop-md">
                    <span>descr:</span>
                    <span>{{gv(deviceProps, 'reported.' + propName + '.ad')}}</span>
                </div>
            </div>
        </div>
    `
}