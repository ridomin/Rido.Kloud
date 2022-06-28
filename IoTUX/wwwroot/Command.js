﻿export default {
    data() {
        return {
            request: 2,
            response:''
        }
    },
    props: ['commandName', 'deviceId'],
    methods: {
        
        async invoke() {
            const url = `/api/Command/${this.deviceId}?cmdName=${this.commandName}`
            this.response = '.. loading ..'
            console.log(this.request)
            try {
                const resp = await (await fetch(url, { method: 'POST', body: `\"${this.request}\"`, headers: { 'Content-Type': 'application/json' } })).json()
                this.response = JSON.stringify(resp, null, 2)
            } catch {
                this.response = "Offline"
            }
        }
    },
    template: `
        <div>{{commandName}}</div>
        <select v-model="request">
            <option value="0">minimal</option>
            <option value="1">complete</option>
            <option value="2" :selected="request === 2">full</option>
        </select>
        <button @click="invoke()">invoke</button>
        <pre>{{response}}</pre>
    `
}