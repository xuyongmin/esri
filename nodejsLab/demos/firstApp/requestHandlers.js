/*
 requestHandlers.js
 author:xuyongmin
 date:20140902
 description:
*/
var exec = require("child_process").exec;
var block = require('./block');

function start(response) {
	console.log('Request handler "start" was called.');

	exec("dir", {
		timeout: 10000,
		maxBuffer: 20000 * 1024
	},
	function(error, stdout, stderr) {
		block.blockcallback(stdout, function(data) {
			response.writeHead(200, {
				"Content-Type": "text/plain"
			});
			response.write(data);
			response.end();
		})
	});
}

function upload(response) {
	console.log('Requst handler "upload" was called.');
	response.writeHead(200, {
		"Content-Type": "text/plain"
	});
	response.write('Requst handler "upload" was called.');
	response.end();
}

exports.start = start;
exports.upload = upload;