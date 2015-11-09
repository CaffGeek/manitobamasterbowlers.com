$.fn.tinymce = function(options){
   return this.each(function(){
      tinyMCE.execCommand("mceAddControl", true, this.id);
   });
}

function initMCE(){
   tinyMCE.init({mode : "none",
      theme : "advanced",
      theme_advanced_toolbar_location : "top",
      theme_advanced_toolbar_align : "left",
      theme_advanced_statusbar_location : "bottom",
      theme_advanced_buttons1 : "bold,italic,underline,strikethrough,separator,bullist,numlist,undo,redo,link,unlink",
      theme_advanced_buttons2 : "",
      theme_advanced_buttons3 : "",
      theme_advanced_resizing : true});
}

initMCE();

$.editable.addInputType('mce', {
   element : function(settings, original) {
      var textarea = $('<textarea id="'+$(original).attr("id")+'_mce"/>');
      if (settings.rows) {
         textarea.attr('rows', settings.rows);
      } else {
         textarea.height(settings.height);
      }
      if (settings.cols) {
         textarea.attr('cols', settings.cols);
      } else {
         textarea.width(settings.width);
      }
      $(this).append(textarea);
         return(textarea);
      },
   plugin : function(settings, original) {
      tinyMCE.execCommand("mceAddControl", true, $(original).attr("id")+'_mce');
      $get(original.id + '_edit').style.display = 'none';
      },
   submit : function(settings, original) {
      tinyMCE.triggerSave();
      tinyMCE.execCommand("mceRemoveControl", true, $(original).attr("id")+'_mce');
      $get(original.id + '_edit').style.display = '';
      },
   reset : function(settings, original) {
      tinyMCE.execCommand("mceRemoveControl", true, $(original).attr("id")+'_mce');
      original.reset();
      $get(original.id + '_edit').style.display = '';
      }
});

$(document).ready(function() {
    $('#foo').tinymce();
    
    $(".edit_mce").editable(SaveContentBlock, {
        type : 'mce',
        event : 'edit',
        submit : 'Save',
        cancel : 'Cancel',
        indicator : "Saving...",
        placeholder : "Loading...",
        tooltip : '',
        width : '300px',
        height : '100px'
    });
    
    var contentBlocks = $('.contentBlock');
    var l = contentBlocks.length;
  
    for (var i=0; i<l; i++)
    {
       LoadContentBlock(contentBlocks[i]);
    }
});

function EditContentBlock(el) {
    el.edit();
}

function SaveContentBlock(value, settings){
    Masters.SaveContentBlock(this.id, value, 
        function (result, context, method)
        {
            $(context).html(result);
        }, AJAXError, this
    );
}

function LoadContentBlock(el){
    Masters.LoadContentBlock(el.id, 
        function (result, context, method)
        {
            $(context).html(result);
        }, AJAXError, el
    );
}

function AJAXError(result, context, method){
    $(context).html("Error loading content");
}