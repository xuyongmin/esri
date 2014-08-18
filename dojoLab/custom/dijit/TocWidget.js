/*
 TocWidget控件
 author:Xuym
 date:20140717
*/

define([
	"dojo/_base/declare",
	"dojo/on",
	"dijit/Tree",
	"dojo/store/Memory",
	"dijit/tree/ObjectStoreModel",
	"dijit/_WidgetBase",
	"dijit/_TemplatedMixin",
	"dojo/text!customDijit/templates/TocWidget.html"
	],
	function(declare, on, Tree, Memory, ObjectStoreModel, _WidgetBase, _TemplatedMixin, template){
		var TocWidget = declare(
			[_WidgetBase,_TemplatedMixin],
			{
				tocTree: null,
				templateString: template,
				postCreate: function(){
					// Get a DOM node reference for the root of our widget
					var domNode = this.domNode;
					// Run any parent postCreate processes - can be done at any point
		    		this.inherited(arguments);
		    		 // Create test store, adding the getChildren() method required by ObjectStoreModel
				    var myStore = new Memory({
				        // data: [
				        //     { id: 'world', name:'The earth', type:'planet', population: '6 billion'},
				        //     { id: 'AF', name:'Africa', type:'continent', population:'900 million', area: '30,221,532 sq km',
				        //             timezone: '-1 UTC to +4 UTC', parent: 'world'},
				        //         { id: 'EG', name:'Egypt', type:'country', parent: 'AF' },
				        //         { id: 'KE', name:'Kenya', type:'country', parent: 'AF' },
				        //             { id: 'Nairobi', name:'Nairobi', type:'city', parent: 'KE' },
				        //             { id: 'Mombasa', name:'Mombasa', type:'city', parent: 'KE' },
				        //         { id: 'SD', name:'Sudan', type:'country', parent: 'AF' },
				        //             { id: 'Khartoum', name:'Khartoum', type:'city', parent: 'SD' },
				        //     { id: 'AS', name:'Asia', type:'continent', parent: 'world' },
				        //         { id: 'CN', name:'China', type:'country', parent: 'AS' },
				        //         { id: 'IN', name:'India', type:'country', parent: 'AS' },
				        //         { id: 'RU', name:'Russia', type:'country', parent: 'AS' },
				        //         { id: 'MN', name:'Mongolia', type:'country', parent: 'AS' },
				        //     { id: 'OC', name:'Oceania', type:'continent', population:'21 million', parent: 'world'},
				        //     { id: 'EU', name:'Europe', type:'continent', parent: 'world' },
				        //         { id: 'DE', name:'Germany', type:'country', parent: 'EU' },
				        //         { id: 'FR', name:'France', type:'country', parent: 'EU' },
				        //         { id: 'ES', name:'Spain', type:'country', parent: 'EU' },
				        //         { id: 'IT', name:'Italy', type:'country', parent: 'EU' },
				        //     { id: 'NA', name:'North America', type:'continent', parent: 'world' },
				        //     { id: 'SA', name:'South America', type:'continent', parent: 'world' }
				        // ],
				        data: [

							{
								"id":"P6F2D955F5A84BA882CC7B645974376C",
								"label":"图层目录"
							},
							{
								"id":"P5F2D955F5A84BA882CC7B645974376C",
								"label":"基础图层",
								"parent":"P6F2D955F5A84BA882CC7B645974376C"
							},
							{
								"id":"P4F2D955F5A84BA882CC7B645974376C",
								"label":"项目图层",
								"parent":"P6F2D955F5A84BA882CC7B645974376C"
							},
							{
								"id":"B5F2D955F5A84BA882CC7B645974376C",
								"label":"影像",
								"url":"http://192.168.20.64/ArcGIS/rest/services/2009yx/MapServer",
								"type":"tiled",
								"layerTag":"st_nzydk",
								"parent":"P5F2D955F5A84BA882CC7B645974376C"
							},
							{
								"id":"B5F2D955F5A84BA882CC7B645974376C",
								"label":"农转用地块",
								"url":"http://192.168.20.64/ArcGIS/rest/services/nzy2013/MapServer",
								"type":"dynamic",
								"layerTag":"st_nzydk",
								"parent":"P4F2D955F5A84BA882CC7B645974376C"
							}
						],
				        getChildren: function(object){
				            return this.query({parent: object.id});
				        }
				    });

				    // Create the model
				    var myModel = new ObjectStoreModel({
				        store: myStore,
				        query: {id: 'P6F2D955F5A84BA882CC7B645974376C'},
				        labelAttr: "label"
				    });

					this.tocTree = new Tree({model: myModel});
					this.tocTree.onClick = function(item,node,evt){
						console.log(item);
						console.log(node);
						console.log(evt);
					}
					this.tocTree.placeAt(this.tocContainerNode);
					this.tocTree.startup();
				}
			}
		);

		return TocWidget;
	}
);