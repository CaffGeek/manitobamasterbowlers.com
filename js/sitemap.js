$(document).ready(function() {
    $('.StyledButton').styledbutton()

    LoadPages($('.ExistingPages'));
});

function Create_Click(sender) {
    sender = $(sender);
    
    Masters.CreatePage('<CreatePage><PageName>' + $('#PageName').val() + '</PageName></CreatePage>',
        function (result, context, method)
        {
            LoadPages(context);
        }, AJAXError, $('.ExistingPages')
    );
}

function LoadPages(target) {
    target.empty();
    Masters.GetPages('.*\.(aspx|html?|pdf|doc)$','^.*(_template\.|admin_.*\.).*$',
        function (result, context, method) {
            $(result).each(
                function (i, item) {
                    target.append(
                        $('<li />').append(
                            $('<a />').text(item).attr('href', item)
                        )
                    );
                }
            );
        },
        AJAXError,
        target
    );        
}