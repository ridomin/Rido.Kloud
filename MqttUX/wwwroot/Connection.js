let client
export default {
    data() {
        return {
            hostName :'',// 'f8826e3352314ca98102cfbde8aff20e.s2.eu.hivemq.cloud',
            port: '',//8884,
            useTls: '',//true,
            clientId: '',//'mqttUx_' + Date.now(),
            userName: '',//'demo1',
            password: '',//'MDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA1'
            connected: false,
            connectionError : ''
        }
    },
    emits: ['connected', 'newMessage'],
    //props: ['hostName', 'port', 'useTls', 'clientId', 'userName', 'password'],
    created() {
        let mqttCreds = JSON.parse(window.localStorage.getItem('mqttCreds'))
        if (mqttCreds) {

            this.hostName = mqttCreds.hostName
            this.port = mqttCreds.port
            this.useTls = mqttCreds.useTls
            this.clientId = mqttCreds.clientId
            this.userName = mqttCreds.userName
            this.password = mqttCreds.password
        }
    },
    methods: {
        change() {
            client.end()
            this.connected = false
            this.$emit('connected', false)
        },
        save() {
            let mqttCreds = {
                hostName: this.hostName, 
                port: this.port,
                useTls: this.useTls,
                clientId:this.clientId,
                userName:this.userName,
                password: this.password
            }
            window.localStorage.setItem('mqttCreds', JSON.stringify(mqttCreds))
            console.log(mqttCreds)
            client = mqtt.connect(`wss://${mqttCreds.hostName}:${mqttCreds.port}/mqtt`, { 
                clientId: mqttCreds.clientId, username: mqttCreds.userName, password: mqttCreds.password })
            client.on('error', e => {
                this.connectionError = e
                console.error(e)
            })
            client.on('connect', () => {
                this.connected = true
                this.$emit('connected', true)
                client.subscribe('pnp/+/birth')
            })
            client.on('message', (topic, message) => {
                this.$emit('newMessage', topic, message)
            })
                
        }
    },
    template: `
    <div v-if="connected">
        <h3>{{hostName}}</h3> <button @click="change">change</button>
    </div>
    <div v-if="!connected">
        <p>
            <label for="hostName">HostName</label>
            <input id="hostName" type="text" v-model="hostName" size="60">
        </p>
        <p>
            <label for="port">Port</label>
            <input id="port" type="text" v-model="port" size="6">
        </p>
        <p>
            <label>UseTls</label>
            yes <input type="radio" value="true" v-model="useTls">
            no <input type="radio" value="false" v-model="useTls">
        </p>
        <p>
            <label for="clientId">ClientId</label>
            <input id="clientId" type="text" v-model="clientId" size="60">
        </p>
        <p>
            <label for="userName">UserName</label>
            <input id="userName" type="text" v-model="userName" size="60">
        </p>
        <p>
            <label for="password">Password</label>
            <input id="password" type="password" v-model="password" size="60">
        </p>
        <p>
            <button @click="save">Connect</button>
        </p>

    </div>
    
    `
}