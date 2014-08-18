/*1.7以后模块定义,AMD模式*/

define([
    "dojo/_base/declare",
    "dojo/_base/fx",
    "dojo/_base/lang",
    "dojo/dom-style",
    "dojo/mouse",
    "dojo/on",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dojo/text!customDijit/templates/IndexWidget.html"]
    ,function(declare, baseFx, lang, domStyle, mouse, on, _WidgetBase, _TemplatedMixin, template){
    return declare([_WidgetBase, _TemplatedMixin], {
		// Some default values for our author
        // These typically map to whatever you're passing to the constructor
        label: "No Name",
        // Using require.toUrl, we can get a path to our AuthorWidget's space
        // and we want to have a default avatar, just in case
        target: '_blank',
        url: "",
 
        // Our template - important!
        templateString: template,
 
        // A class to be applied to the root node in our template
        baseClass: "indexWidget",
 
        // A reference to our background animation
        mouseAnim: null,
 
        // Colors for our background animation
        baseBackgroundColor: "#fff",
        mouseBackgroundColor: "#def",
		postCreate: function(){
		    // Get a DOM node reference for the root of our widget
		    var domNode = this.domNode;
		 
		    // Run any parent postCreate processes - can be done at any point
		    this.inherited(arguments);
		 
		    // Set our DOM node's background color to white -
		    // smoothes out the mouseenter/leave event animations
		    domStyle.set(domNode, "backgroundColor", this.baseBackgroundColor);
		    // Set up our mouseenter/leave events
		    // Using dijit/Destroyable's "own" method ensures that event handlers are unregistered when the widget is destroyed
		    // Using dojo/mouse normalizes the non-standard mouseenter/leave events across browsers
		    // Passing a third parameter to lang.hitch allows us to specify not only the context,
		    // but also the first parameter passed to _changeBackground
		    this.own(
		        on(domNode, mouse.enter, lang.hitch(this, "_changeBackground", this.mouseBackgroundColor)),
		        on(domNode, mouse.leave, lang.hitch(this, "_changeBackground", this.baseBackgroundColor)),
		    	on(domNode, 'click', lang.hitch(this, "_linkTo",this.url))
		    );
		},
		_linkTo: function(url){
			if(url && url.length>0){
				window.open(url);
			}
		},
		_changeBackground: function(newColor) {
			// console.log(newColor);
		    // If we have an animation, stop it
		    if (this.mouseAnim) {
		        this.mouseAnim.stop();
		    }
		 
		    // Set up the new animation
		    this.mouseAnim = baseFx.animateProperty({
		        node: this.domNode,
		        properties: {
		            backgroundColor: newColor
		        },
		        onEnd: lang.hitch(this, function() {
		            // Clean up our mouseAnim property
		            this.mouseAnim = null;
		        })
		    }).play();
		}
    });
});