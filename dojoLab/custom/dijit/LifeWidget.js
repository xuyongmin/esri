/*LifeWidget.js*/


define([
	"dojo/_base/declare",
	"dijit/_WidgetBase",
	"dijit/_TemplatedMixin",
	"dijit/_WidgetsInTemplateMixin",
	"dojo/text!customDijit/templates/LifeWidget.html"
	],function(declare,_WidgetBase,_TemplatedMixin,_WidgetsInTemplateMixin,template) {
	var LifeWidget = declare([_WidgetBase,_TemplatedMixin,_WidgetsInTemplateMixin],{
		templateString: template,
		constructor: function() {
			this.inherited(arguments);
			console.log("constructor:");
			console.log(arguments);
		},
		postMixInProperties: function() { 
			this.inherited(arguments);
			console.log("postMixInProperties:");
			console.log(arguments);
		},
		buildRendering: function() {
			this.inherited(arguments);
			console.log("buildRendering:");
			console.log(arguments);
		},
		postCreate: function() {
			this.inherited(arguments);
			console.log("postCreate:");
			console.log(arguments);
		},
		startup: function() {
			this.inherited(arguments);
			console.log("startup:");
			console.log(arguments);
		},
		destroy: function() { 
			this.inherited(arguments);
			console.log("destroy:");
			console.log(arguments);
		}
	});

	return LifeWidget;
});