export default {
    props: ['reportedNode', 'propName'],
    methods: {
        gv(object, string, defaultValue = '') {
            // https://stackoverflow.com/questions/70283134
            return _.get(object, string, defaultValue)
        },
        formatDate(d) {
            if (d === '0001-01-01T00:00:00Z') return ''
            return moment(d).fromNow()
        },
    },
    template: `
    <div class="prop">
        <span class="prop-name">{{propName}}</span>
        <span class="prop-value">{{gv(reportedNode, propName)}}</span>
        <div class="prop-md">
            <span>last updated {{formatDate(gv(reportedNode, '$metadata.'+ propName +'.$lastUpdated'))}}</span>
        </div>
    </div>
    `
}