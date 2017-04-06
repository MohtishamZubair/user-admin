
(function () {
    if (typeof enableDemo !== "undefined" && enableDemo){        
        loadDemoData();        
    }
}());

function loadDemoData() {

    var fontSize = $('#appFooter').css('font-size');
    var checkDataLoad = false;
    $('#appFooter')
    	.animate(
    		{				
    		    'font-size': 14 
    		},
    		(1), function() {
    		    checkDataLoad = $('#demo').length > 0;
    		}
            );
    $('#appFooter').promise().done(
    	function () {    	    
    	    if (checkDataLoad) {
                callDemoData();
    	    }
    	    else {
    	        loadDemoData();
            }
    	});
}

function callDemoData() {
    $(".container.body-content").nextAll().not('#demo').appendTo('#demo');
    $('#demo').css('display', 'none');
}