'use strict';

const $ = (id) => document.getElementById(id);
const orders = new Map();
let connection = null;

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
    for (const id of ['joinBtn', 'leaveBtn', 'invokeBtn']) $(id).disabled = !connected;
    // REST update is not wired up yet (no PUT endpoint on the server)
    $('restBtn').disabled = true;
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

function upsertOrder(order) {
    orders.set(order.id, order);
    renderOrders(order.id);
}

//Connect/Disconnect
async function start() {
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
    connection.onreconnected(() => setStatus('Connected'));
    connection.onclose(() => {
        connection = null;
        setStatus('Disconnected');
    });

    setStatus('Connecting…');
    await connection.start();
    setStatus('Connected');
    log('Connected to /orderNotification');

    // se ho tempo aggiungo login, per ora carico tutto via API call
    const response = await fetch('/api/orders');
    if (!response.ok) throw new Error(`${response.status} ${await response.text()}`);
    orders.clear();
    for (const order of await response.json()) orders.set(order.id, order);
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
$('stopBtn').onclick = () => run(() => connection.stop());

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
    upsertOrder(order);
});

setStatus('Disconnected');
