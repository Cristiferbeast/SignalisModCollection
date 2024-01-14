// gun_server.js
require('./bundledDependencies');

var Gun = global.Gun;
var http = global.http;

var server = http.createServer(function (req, res) {
    if (Gun.serve(req, res)) return; // Gun will handle HTTP requests
    res.writeHead(200, { 'Content-Type': 'text/html' });
    res.end('<h1>Hello Gun.js Server!</h1>');
});

var gun = Gun({ web: server });

server.listen(8765);

// Function to store messages for user IDs
function SendMessage(userId, message) {
    // Store messages in Gun.js graph
    gun.get('messages').get(userId).set({ message: message });
}

// Function to read messages for a specific user ID
function ReadMessage(userId) {
    // Read messages from Gun.js graph
    var message = gun.get('messages').get(userId).get('message').val();

    // Return the message (or an empty string if not found)
    return message || null;
}