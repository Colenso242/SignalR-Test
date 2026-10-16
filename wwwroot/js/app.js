'use strict';

const $ = (id) => document.getElementById(id);
const orders = new Map();
let connection = null;
let currentUserId = null;

function log(message) {
    const line = document.createElement('div');
    line.textContent = `${new Date().toLocaleTimeString()}  ${message}`;
    $('log').prepend(line);
}

function setStatus(text) {
    $('status').textContent = text;
    const connected = text === 'Connected';
    $('startBtn').disabled = connection !== null;
    $('stopBtn').disabled = connection === null;
    for (const id of ['joinBtn', 'leaveBtn', 'invokeBtn', 'restBtn']) $(id).disabled = !connected;
}

function renderOrders(highlightId) {
    const rows = [...orders.values()].sort((a, b) => a.id - b.id).map((o) => {
        const tr = document.createElement('tr');
        if (o.id === highlightId) tr.className = 'flash';
        for (const value of [o.id, o.product, o.userId, o.status, new Date(o.updatedAt).toLocaleTimeString()]) {
            const td = document.createElement('td');
            td.textContent = value;
            tr.append(td);
        }
        return tr;
    });
    $('orders').replaceChildren(...rows);
}

// la tabella mostra solo i miei ordini: quelli osservati via gruppo finiscono solo nel log
function upsertOrder(order) {
    if (order.userId !== currentUserId) return;
    orders.set(order.id, order);
    renderOrders(order.id);
}

async function fetchJson(url, options) {
    const response = await fetch(url, options);
    if (!response.ok) throw new Error(`${response.status} ${await response.text()}`);
    return response.status === 204 ? null : response.json();
}

async function loadMyOrders() {
    orders.clear();
    for (const order of await fetchJson('/api/orders/mine')) orders.set(order.id, order);
    renderOrders();
}

//Connect/Disconnect
async function start() {
    // il cookie di login viene inviato anche alla negotiate/websocket di SignalR
    const { userId } = await fetchJson('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userName: $('userName').value }),
    });
    currentUserId = userId;
    log(`Signed in as ${userId}`);

    connection = new signalR.HubConnectionBuilder()
        .withUrl('/orderNotification')
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on('OrderStatusUpdated', (order) => {
        upsertOrder(order);
        log(`OrderStatusUpdated: #${order.id} → ${order.status}`);
    });
    connection.on('SystemMessage', (message) => log(`SystemMessage: ${message}`));

    connection.onreconnecting(() => setStatus('Reconnecting…'));
    connection.onreconnected(() => {
        setStatus('Connected');
        // durante la disconnessione posso aver perso degli aggiornamenti
        run(loadMyOrders);
    });
    connection.onclose(() => {
        connection = null;
        setStatus('Disconnected');
    });

    setStatus('Connecting…');
    await connection.start();
    setStatus('Connected');
    log('Connected to /orderNotification');

    await loadMyOrders();
}

async function stop() {
    await connection.stop();
    await fetchJson('/api/auth/logout', { method: 'POST' });
    log(`Signed out ${currentUserId}`);
    currentUserId = null;
    orders.clear();
    renderOrders();
}

async function run(action) {
    try {
        await action();
    } catch (err) {
        log(`Error: ${err.message ?? err}`);
        if (connection?.state === signalR.HubConnectionState.Disconnected) {
            connection = null;
            setStatus('Disconnected');
        }
    }
}

$('startBtn').onclick = () => run(start);
$('stopBtn').onclick = () => run(stop);

//Watch order

$('joinBtn').onclick = () => run(async () => {
    const id = Number($('groupOrderId').value);
    await connection.invoke('JoinOrderGroup', id);
    log(`Joined group Order-${id}`);
});

$('leaveBtn').onclick = () => run(async () => {
    const id = Number($('groupOrderId').value);
    await connection.invoke('LeaveOrderGroup', id);
    log(`Left group Order-${id}`);
});

$('invokeBtn').onclick = () => run(async () => {
    //hub returns updated order,group members receive OrderStatusUpdated
    const order = await connection.invoke('SendOrderStatusUpdate', {
        orderId: Number($('updateOrderId').value),
        status: $('newStatus').value,
    });
    log(`Updated #${order.id} → ${order.status}`);
    upsertOrder(order);
});

$('restBtn').onclick = () => run(async () => {
    //PUT normale: la notifica SignalR arriva comunque da OrderService tramite IHubContext
    const order = await fetchJson(`/api/orders/${Number($('updateOrderId').value)}/status`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: $('newStatus').value }),
    });
    log(`Updated via REST #${order.id} → ${order.status}`);
    upsertOrder(order);
});

setStatus('Disconnected');
