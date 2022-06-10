export default {
    data: () => ({
        device: {}
    }),

    created() {
        this.fetchData()
    },

    methods: {
        async fetchData() {
            const url = 'test-device.json' // `/devices/list`
            this.devices = await (await fetch(url)).json()
        },
        formatDate(d) {
            return moment(d).fromNow()
        }
    }
}