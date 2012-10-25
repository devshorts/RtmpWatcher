var currentTab;
var inited = true;

function changeTab(e){
	
    if(inited == true){
            currentTab = document.getElementById("ServiceBrowserTab");
            inited = false;
    }        

    if (window.event)
    	e = window.event;
    var srcElem = e.srcElement? e.srcElement : e.target;

    if (srcElem.className != "tab") {
            while(srcElem.parentNode && srcElem.className != "tab"){
	            srcElem = srcElem.parentNode;
            }
    }

    if(srcElem.className == "tab"){
            // Reset the last clicked tab
            currentTab.className = "tab";
            // Store a reference to the clicked tab
			currentTab = srcElem;
            currentTab.className = "selTab";

            switch(currentTab.id) {
                    case "ServiceBrowserTab" :
                            window.parent.frames["contentarea"].location.href = "ServiceBrowser.aspx";
                            break;
                    case "LogTab" :
                            window.parent.frames["contentarea"].location.href = "FluorineLog.aspx";
                            break;
                    case "HelpTab" :
                            window.parent.frames["contentarea"].location.href = "http://fluorine.thesilentgroup.com/fluorine/index.html";
                            break;
            }
    }
}
