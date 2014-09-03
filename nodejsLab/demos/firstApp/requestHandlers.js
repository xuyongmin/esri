/*
 requestHandlers.js
 author:xuyongmin
 date:20140902
 description:
*/
var querystring = require("querystring"),
	fs = require("fs"),
	formidable = require("formidable"),
	childProcess = require("child_process");

function hello(response) {
	var n = childProcess.fork(__dirname + "/subProcess.js");
	n.on('message', function() {
		response.writeHead(200, {
			"Content-Type": "text/plain"
		});
		response.write("say hello.");
		response.end();
	});
	n.send({});
};

function start(response) {
	console.log("Request handler 'start' was called.");

	var body = '<html>' +
		'<head>' +
		'<meta http-equiv="Content-Type" ' +
		'content="text/html; charset=UTF-8" />' +
		'</head>' +
		'<body>' +
		'<form action="/upload" enctype="multipart/form-data" ' +
		'method="post">' +
		'<input type="file" name="upload" multiple="multiple">' +
		'<input type="submit" value="Upload file" />' +
		'</form>' +
		'</body>' +
		'</html>';

	response.writeHead(200, {
		"Content-Type": "text/html"
	});
	response.write(body);
	response.end();
}

function upload(response, request) {
	console.log("Request handler 'upload' was called.");

	var form = new formidable.IncomingForm();
	console.log("about to parse");
	form.parse(request, function(error, fields, files) {
		console.log("parsing done");

		/* Possible error on Windows systems:
       tried to rename to an already existing file */
		fs.rename(files.upload.path, "c:/TEMP/test.png", function(err) {
			if (err) {
				fs.unlink("c:/TEMP/test.png");
				fs.rename(files.upload.path, "c:/TEMP/test.png");
			}
		});
		response.writeHead(200, {
			"Content-Type": "text/html"
		});
		response.write("received image:<br/>");
		response.write("<img src='/show' />");
		response.end();
	});
}

function show(response) {
	console.log("Request handler 'show' was called.");
	response.writeHead(200, {
		"Content-Type": "image/png"
	});
	fs.createReadStream("c:/TEMP/test.png").pipe(response);
}

exports.hello = hello;
exports.start = start;
exports.upload = upload;
exports.show = show;