/* subProcess.js */
function sleep(milliSecond) {
	var startTime = new Date().getTime();
	console.log(startTime);
	while (new Date().getTime() <= milliSecond + startTime) {}
	console.log(new Date().getTime());
}
process.on('message', function() {
	sleep(20000);
	process.send({});
});