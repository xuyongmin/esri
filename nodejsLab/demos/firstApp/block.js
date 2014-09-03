/*
 block.js
 author:xuyongmin
 date:20140902
 description:
*/

function blockcallback(msg,cb) {
	if (typeof cb === 'function') {

		/*
		 	 此处按照教程，还是以阻塞的模式运行，为什么？
		*/
		function sleep(milliSeconds) {
			var startTime = new Date().getTime();
			while (new Date().getTime() < startTime + milliSeconds);
		}
		sleep(10000);
		
		cb(msg);
	};
}

exports.blockcallback = blockcallback;