/// <reference path="../../js/jquery-1.2.6.js" />

//Add CSS
var foo = (
	function() {
		var scripts = document.getElementsByTagName('script');
		var script = scripts[scripts.length - 1];
		var pathWithFile = script.getAttribute("src");
		var path = pathWithFile.indexOf('/') >= 0
			? pathWithFile.match(/^(.+)\//)[0]
			: '';
		var css = document.createElement('link');
		css.setAttribute('rel', 'stylesheet');
		css.setAttribute('type', 'text/css');
		css.setAttribute('href', path + 'jquery-ui.nygard.styledbutton.css');
		script.parentNode.insertBefore(css, script);
	}
)();

(function($) {
	$.fn.styledbutton = function() {
		return this.each(function() {
			var obj = $(this);
			var text = obj.html();

			obj.empty();
			obj.addClass('StyledButton');

			obj.append(
				$('<div />').addClass('L').append(
					$('<div />').addClass('R').append(
						$('<div />').addClass('C').append(
							$('<span />').addClass('ButtonText').html(text)
						)
					)
				)
			);
			
			obj.append(
				$('<div />').addClass('BL').append(
					$('<div />').addClass('BR').append(
						$('<div />').addClass('BC')
					)
				)
			);
		});
	};
})(jQuery);