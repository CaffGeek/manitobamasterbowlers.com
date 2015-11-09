$(document).ready(function() {
    tinyMCE.init({mode : "textareas",
                  theme : "advanced",
                  theme_advanced_toolbar_location : "top",
                  theme_advanced_toolbar_align : "left",
                  theme_advanced_statusbar_location : "bottom",
                  theme_advanced_buttons1 : "bold,italic,underline,strikethrough,separator,bullist,numlist,undo,redo,link,unlink",
                  theme_advanced_buttons2 : "",
                  theme_advanced_buttons3 : "",
                  theme_advanced_resizing : true});
    
    $('.StartDate, .EndDate').datepicker();
    $('.StyledButton').styledbutton()

    LoadAnnouncements();
});

function Delete_Click(sender) {
    sender = $(sender);
    var xml = sender.parents('tr:first').data('data');
    var xmlString = xml.xml || (new XMLSerializer()).serializeToString(xml);
    
    Masters.DeleteAnnouncement('<Announcements>' + xmlString + '</Announcements>',
        function (result, context, method)
        {
            LoadAnnouncements();
        }, AJAXError, $('#status')
    );
}

function Edit_Click(sender) {
    sender = $(sender);
    var xml = sender.parents('tr:first').data('data');
    var valueNode = xml.selectSingleNode('Value');
    
    var announcement = innerXml(valueNode);
    var startDate = xml.getAttribute('StartDate');
    var endDate = xml.getAttribute('EndDate');
    var id = xml.getAttribute('Id');
    
    tinyMCE.get('Announcement').setContent(announcement);
    $('.EditAnnouncement .Id').text(id);
    $('.EditAnnouncement .StartDate').val(startDate);
    $('.EditAnnouncement .EndDate').val(endDate);    
}

function Clear_Click(sender) {
    sender = $(sender);
    
    tinyMCE.get('Announcement').setContent('');
    $('.EditAnnouncement .Id').text('');
    $('.EditAnnouncement .StartDate').val('');
    $('.EditAnnouncement .EndDate').val('');    
}

function Save_Click(sender) {
    sender = $(sender);
    var content = tinyMCE.get('Announcement').getContent();
    var startDate = $('.EditAnnouncement .StartDate').val();
    var endDate = $('.EditAnnouncement .EndDate').val();
    var id = $('.EditAnnouncement .Id').text();
    
    var filter = String.format('<Announcements><Announcement {1} {2} {3}><![CDATA[{0}]]></Announcement></Announcements>', 
        content, 
        id ? "Id='" + id + "'" : '',
        startDate ? "StartDate='" + startDate + "'" : '', 
        endDate ? "EndDate='" + endDate + "'" : ''
    );
    
    Masters.SaveAnnouncement(filter,
        function (result, context, method)
        {
            Clear_Click();            
            LoadAnnouncements();
        }, AJAXError, $('#status')
    );
}


function LoadAnnouncements() {
     Masters.LoadAnnouncements(
        function (result, context, method)
        {
			context.find("#status").text('Displaying Data...');
			LoadDataTable(result, context, method);
			context.find("#status").slideUp(1000);
        }, AJAXError, $('#dataTable')
    );
}

function LoadDataTable(data, context, method) {
	var start = new Date();
	
	// Remove old data and ensure data tbody exists
	var dataBody = $(context.children('tbody.data'));
	if (dataBody.length != 0) {
		// Temporarily hide it, since removal is slow in IE
		dataBody.removeClass('data').addClass('remove').hide();
		
		// Physically Remove it in 1 second
		setTimeout(function() {
			$('tbody.remove').remove();
		}, 1000);
	}
	dataBody = $('<tbody />').addClass('data');
	context.append(dataBody);
	
	// Load it with data
	$(data.documentElement.selectNodes("//Announcement")).each(
		function(i, item) {	
			var newRow = CreateRow(item, context);
			if (newRow) {
				newRow.data('data', item);
				dataBody.append(newRow);
				newRow.show();
			}
		}
	);

	// Scroll to the top
	context.parent().animate({ scrollTop: 0 }, 'fast');
	
	// Time taken to render DOM
	var stop = new Date();
	var renderTime = (stop - start) + 'ms';

	// Zebrify table
	ColorTable();
}

function CreateRow(item, dataTable, position) {
	// Transform and return jquery object for the row
	//TODO: Allow Caching
	html = $.xslt({
            xml: item,
            xmlCache: false, 
            xslUrl: '../xslt/Announcements_Edit.xslt?' + Math.random(), 
            xslCache: false
        });
        
	return $(html);
}

function ColorTable() {
	var dataBody = $('table#dataTable tbody.data');

	dataBody.children('tr:odd').addClass('alt');
	dataBody.children('tr:even').removeClass('alt');

	dataBody.children('tr:has(input.Modified)').removeClass('modified');
	dataBody.children('tr:has(input.Modified:checked)').addClass('modified');
	
    dataBody.find('.StyledButton').styledbutton();
}