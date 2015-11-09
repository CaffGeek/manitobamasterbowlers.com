window.onload = function () {
    $(document).ready(function() {
         Masters.LoadAnnouncements(
            function (result, context, method)
            {
                $(context).hide();
                
                //TODO: Allow Caching
	            var html = $.xslt({
                    xml: result.documentElement,
                    xmlCache: false, 
                    xslUrl: '../xslt/Announcements_Ticker.xslt?' + Math.random(),
                    xslCache: false
                });
                
                $(context).html(html).show().find("ul").newsticker();
            }, AJAXError, $('#announcements')
        );
    });
}