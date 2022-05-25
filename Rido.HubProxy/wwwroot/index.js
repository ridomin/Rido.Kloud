import { createApp } from 'https://unpkg.com/petite-vue?module'
createApp().mount('#app')

//let app = document.getElementById('app')
//const callCommand = async id => {
    
//    const pre = document.createElement('pre')
//    try {
//        const res = await fetch(`/pnp/${id}/commands/getRuntimeStats`, {
//            method: 'POST', body: 2, headers: { 'Content-Type': 'application/json' }
//        })
//        pre.innerText = id + JSON.stringify(await res.json(), null, 2)
//    } catch (e) {
//        pre.innerText = id + JSON.stringify(e)
//    }
//    app.appendChild(pre)
//}

//(async () => {
//    const devices = await (await fetch('devices/list')).json()
//    var html = ''
//    devices.forEach(async d => {
//        html = `<div><h3>${d.deviceId}</h3><span>${d.started}</span> | <span>${d.interval}</span> <a href="javascript:callCommand('${d.deviceId}')">getRuntimeStats</a></div>`
//        app.innerHTML += html
//    })
//})()