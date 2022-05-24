let app = document.getElementById('app')
const callCommand = async (id) => {
    
    const res = await (await fetch(`/pnp/${id}/commands/getRuntimeStats`,
        {
            method: 'POST', body: 2, headers: { 'Content-Type': 'application/json'  }
        })).json()
    //alert(JSON.stringify(res, null, 2))
    const pre = document.createElement('pre')
    pre.innerText = id + JSON.stringify(res, null, 2)
    app.appendChild(pre)
}

(async () => {
    let ids = await (await fetch('devices/list')).json()
    var html = ''
    ids.forEach(async id => {
        let started = await (await fetch(`/pnp/${id}/props/started`)).json()
        let interval = await (await fetch(`/pnp/${id}/props/interval`)).json()
        html = `<div><h3>${id}</h3><span>${started}</span> | <span>${interval}</span> <a href="javascript:callCommand('${id}')">getRuntimeStats</a></div>`
        app.innerHTML += html
    })
})()