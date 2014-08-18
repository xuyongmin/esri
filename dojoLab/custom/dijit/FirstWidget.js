/*
 this is my first widget
 author:xuym
 date:20140701
*/

define([
	"dojo/_base/declare",
	"dijit/_WidgetBase",
	"dijit/_TemplatedMixin",
	"dijit/_WidgetsInTemplateMixin",
	"dojo/text!customDijit/templates/FirstWidget.html"
	],
	function(declare,_WidgetBase,_TemplatedMixin,_WidgetsInTemplateMixin,template){
		var FirstWidget = declare(
			[_WidgetBase,_TemplatedMixin,_WidgetsInTemplateMixin],
			{
				count: 0,
				templateString: template,
				increment: function() {
					this.count ++;
					console.log(this.count);
					this.counter.innerHTML = this.count;
				}
			}
		);

		return FirstWidget;
	}
);