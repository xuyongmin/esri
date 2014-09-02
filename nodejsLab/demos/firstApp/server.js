/*
 server.js
 author:xuyongmin
 date:20140902
 description:
 http server port:30000-33000
*/
var http = require("http");
var url = require('url');

function start(route, handle) {
	function onRequest(request, response) {
		var pathname = url.parse(request.url).pathname;
		console.log("Request for " + pathname + "  received.");
		route(handle, pathname, response);
	}

	http.createServer(onRequest).listen(30000);

	console.log("Server has started.");
}

exports.start = start;