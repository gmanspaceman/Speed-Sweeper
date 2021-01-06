//Copyright (c) 2015 Gumpanat Keardkeawfa
//Licensed under the MIT license

//Websocket Jslib for UnityWebgl
//We will save this file as *.jslib for using in UNITY
var WebSocketJsLib = {
	Hello: function(){
		window.alert("Hello,world!");
	},
	InitWebSocket: function(url){
		var init_url = Pointer_stringify(url);
		window.wsclient = new WebSocket(init_url);
		window.wsclient.onopen = function(evt){ 
			console.log("[open]"+init_url);
			console.log("[Ready State]"+window.wsclient.readyState);
			console.log("[URL]"+window.wsclient.url);
			//window.wsclient.send("hello<EOM>");
		}; 
		window.wsclient.onclose = function(evt) {
			console.log("[close] "+evt.code+":"+evt.reason);
		}; 
		window.wsclient.onmessage = function(evt) {
			var received_msg = evt.data;
			if (received_msg == "hello") {
				window.wsclient.send("hello");
			} else {
				//console.log("[jslib recv] "+received_msg);
				SendMessage('Server Stuff', 'RecvString', received_msg);	
			}
		}; 
		window.wsclient.onerror = function(evt) {
			var error_msg = evt.data;
			console.log("[error] "+error_msg);
			SendMessage('Server Stuff', 'ErrorString', "close");
		};
	},
	State: function(){
	    var status = 0;
		//if ((typeof window.wsclient !== "undefined")&& (window.wsclient !== null))
		//	status = window.wsclient.readyState;
		return window.wsclient.readyState;
	},
	Send: function(msg){
		var message = Pointer_stringify(msg);
		if (typeof window.wsclient !== "undefined") {
			//console.log("[jslib send] "+message);
			window.wsclient.send(message);		
		} else {
			console.log("[send-failed] "+message);
		}
	},
	Close: function(){
		if ((typeof window.wsclient !== "undefined")&& (window.wsclient !== null))
		   window.wsclient.close();
	}
}
mergeInto(LibraryManager.library, WebSocketJsLib);